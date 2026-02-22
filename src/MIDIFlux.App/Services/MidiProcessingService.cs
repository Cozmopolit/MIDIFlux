using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using MIDIFlux.Core.State;

namespace MIDIFlux.App.Services;

/// <summary>
/// Background service for MIDI processing
/// </summary>
public class MidiProcessingService : BackgroundService
{
    private readonly ILogger<MidiProcessingService> _logger;
    private readonly MidiDeviceManager _MidiDeviceManager;
    private readonly ProfileManager _ProfileManager;
    private readonly ActionStateManager _actionStateManager;
    private readonly ConfigurationManager _configManager;
    private readonly DeviceConnectionHandler _connectionHandler;

    /// <summary>
    /// Event raised when the MIDI processing status changes
    /// </summary>
    public event EventHandler<bool>? StatusChanged;

    /// <summary>
    /// Event raised when the active configuration changes
    /// </summary>
    public event EventHandler<string>? ConfigurationChanged;

    /// <summary>
    /// Gets whether the MIDI processing is running
    /// </summary>
    public bool IsRunning => _connectionHandler.IsRunning;

    /// <summary>
    /// Gets the currently active configuration path
    /// </summary>
    public string ActiveConfigurationPath => _configManager.ActiveConfigurationPath;

    /// <summary>
    /// Gets the list of available configurations
    /// </summary>
    public IEnumerable<string> AvailableConfigurations => _configManager.AvailableConfigurations;

    /// <summary>
    /// Creates a new instance of the MidiProcessingService with action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="MidiDeviceManager">The MIDI manager</param>
    /// <param name="ProfileManager">The event dispatcher</param>
    /// <param name="actionStateManager">The action state manager</param>
    /// <param name="configurationService">The configuration service</param>
    public MidiProcessingService(
        ILogger<MidiProcessingService> logger,
        MidiDeviceManager MidiDeviceManager,
        ProfileManager ProfileManager,
        ActionStateManager actionStateManager,
        ConfigurationService configurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _MidiDeviceManager = MidiDeviceManager ?? throw new ArgumentNullException(nameof(MidiDeviceManager));
        _ProfileManager = ProfileManager ?? throw new ArgumentNullException(nameof(ProfileManager));
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager));

        // Create the action configuration loader using the unified configuration service
        var configLoader = new ActionConfigurationLoader(logger, configurationService);

        // Create the configuration manager with unified system
        _configManager = new ConfigurationManager(logger, configLoader);
        _configManager.ConfigurationChanged += ConfigManager_ConfigurationChanged;

        // Create the device connection handler
        var connectionHandlerLogger = LoggingHelper.CreateLogger<DeviceConnectionHandler>();
        _connectionHandler = new DeviceConnectionHandler(connectionHandlerLogger, MidiDeviceManager, _configManager);

        // Connect the MidiDeviceManager directly to the ProfileManager
        _MidiDeviceManager.SetProfileManager(_ProfileManager);

        _logger.LogDebug("MidiProcessingService initialized with action system");
    }

    /// <summary>
    /// Handles configuration changes
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="configPath">The new configuration path</param>
    private void ConfigManager_ConfigurationChanged(object? sender, string configPath)
    {
        // Get the active configuration
        var config = _configManager.GetActiveConfiguration();
        if (config != null)
        {
            // Set the configuration in the event dispatcher
            _ProfileManager.SetConfiguration(config);

            // Forward the event to subscribers
            ConfigurationChanged?.Invoke(this, configPath);
        }
    }

    /// <summary>
    /// Loads a configuration from a file
    /// </summary>
    /// <param name="configPath">The path to the configuration file</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool LoadConfiguration(string configPath)
    {
        bool result = _configManager.LoadConfiguration(configPath);
        if (result)
        {
            // Forward the event to subscribers
            ConfigurationChanged?.Invoke(this, configPath);
        }
        return result;
    }

    /// <summary>
    /// Loads the path of the last used configuration from the special file
    /// </summary>
    /// <returns>The path to the last used configuration, or null if not found</returns>
    public string? LoadLastUsedConfigurationPath()
    {
        return _configManager.LoadLastUsedConfigurationPath();
    }

    /// <summary>
    /// Starts MIDI processing
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool Start()
    {
        if (_connectionHandler.IsRunning)
        {
            _logger.LogWarning("MIDI processing is already running");
            return true;
        }

        if (string.IsNullOrEmpty(_configManager.ActiveConfigurationPath))
        {
            _logger.LogError("No configuration loaded");
            return false;
        }

        // Get the list of available devices
        var devices = _MidiDeviceManager.GetAvailableDevices();

        // Log all available devices
        _logger.LogInformation("Available MIDI devices:");
        foreach (var device in devices)
        {
            _logger.LogInformation(" - {Device}", device);
        }

        if (devices.Count == 0)
        {
            _logger.LogError("No MIDI devices found");
            return false;
        }

        // Clear previously selected devices
        _connectionHandler.ClearSelectedDeviceIds();

        // Get the active configuration
        var config = _configManager.GetActiveConfiguration();
        if (config == null)
        {
            _logger.LogError("Failed to get active configuration");
            return false;
        }

        // Process unified device configurations
        if (config.MidiDevices.Count > 0)
        {
            _logger.LogInformation("Unified configuration has {Count} device configurations", config.MidiDevices.Count);

            // Process each device configuration
            foreach (var deviceConfig in config.MidiDevices)
            {
                if (string.IsNullOrEmpty(deviceConfig.DeviceName))
                {
                    _logger.LogWarning("Device configuration has no device name, skipping");
                    continue;
                }

                // Handle wildcard "*" - add ALL available devices
                if (deviceConfig.DeviceName == "*")
                {
                    _logger.LogInformation("Wildcard device configuration detected, adding ALL available devices");
                    foreach (var device in devices)
                    {
                        _connectionHandler.AddSelectedDeviceId(device.DeviceId);
                        _logger.LogDebug("Added device '{DeviceName}' (ID: {DeviceId}) to selected devices (wildcard)",
                            device.Name, device.DeviceId);
                    }
                }
                else
                {
                    // Use the helper to find the specific device
                    var selectedDevice = MidiDeviceHelper.FindDeviceByName(devices, deviceConfig.DeviceName, _logger);

                    if (selectedDevice != null)
                    {
                        _connectionHandler.AddSelectedDeviceId(selectedDevice.DeviceId);
                        _logger.LogDebug("Added device '{DeviceName}' (ID: {DeviceId}) to selected devices",
                            selectedDevice.Name, selectedDevice.DeviceId);
                    }
                    else
                    {
                        _logger.LogWarning("Could not find device '{DeviceName}' in available devices", deviceConfig.DeviceName);
                    }
                }
            }
        }
        else
        {
            _logger.LogError("No device configurations found in unified configuration");
            return false;
        }

        if (_connectionHandler.SelectedDeviceIds.Count == 0)
        {
            _logger.LogError("No MIDI devices matched the configuration");
            return false;
        }

        // Start listening for MIDI events on all selected devices
        bool anySuccess = false;
        foreach (var deviceId in _connectionHandler.SelectedDeviceIds)
        {
            if (_MidiDeviceManager.StartListening(deviceId))
            {
                anySuccess = true;
            }
            else
            {
                _logger.LogError("Failed to start listening on device ID {DeviceId}", deviceId);
            }
        }

        if (anySuccess)
        {
            _connectionHandler.SetRunningState(true);

            // Warm up the hot path to avoid first-event JIT compilation delay
            // Then enable latency measurement for clean statistics
            _ = Task.Run(() =>
            {
                try
                {
                    var warmupAction = new DelayAction();
                    warmupAction.SetParameterValue("Milliseconds", 1);
                    warmupAction.ExecuteAsync(127).AsTask().Wait();
                    _logger.LogDebug("Hot path warmup completed");

                    // Now enable latency measurement for real events
                    EnableLatencyMeasurement();
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Hot path warmup failed (not critical)");
                    // Enable measurement anyway, even if warmup failed
                    EnableLatencyMeasurement();
                }
            });

            _logger.LogInformation("MIDI processing started");
            StatusChanged?.Invoke(this, _connectionHandler.IsRunning);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Stops MIDI processing
    /// </summary>
    public void Stop()
    {
        if (!_connectionHandler.IsRunning)
        {
            _logger.LogWarning("MIDI processing is not running");
            return;
        }

        // Release all pressed keys
        _actionStateManager.ReleaseAllPressedKeys();

        _MidiDeviceManager.StopListening();
        _connectionHandler.SetRunningState(false);
        StatusChanged?.Invoke(this, _connectionHandler.IsRunning);
    }

    /// <summary>
    /// Executes the background service
    /// </summary>
    /// <param name="stoppingToken">The cancellation token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("MIDI processing service started");

        // Initialize the GameControllerManager singleton
        // This ensures only one controller is created regardless of how many handlers are instantiated
        try
        {
            var controllerManager = GameControllerManager.GetInstance(_logger);
            _logger.LogInformation("Game controller manager initialized");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize game controller manager");
        }

        // Ensure AppData directories exist
        AppDataHelper.EnsureDirectoriesExist(_logger);

        // We don't load a default configuration automatically anymore
        // This allows the user to explicitly specify a configuration with --config
        // or select one from the UI

        // No need for a background task to check for configuration changes
        // The GUI now communicates directly with the service

        // Wait for cancellation
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private bool _hasLoggedPerformanceStats = false;
    private bool _hasStopped = false;

    /// <summary>
    /// Stops the background service
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
        // Guard against being called multiple times (host shutdown + explicit stop)
        if (_hasStopped)
        {
            return base.StopAsync(cancellationToken);
        }
        _hasStopped = true;

        _logger.LogInformation("MIDI processing service stopping");

        // Stop MIDI processing (this will also release all toggled keys)
        Stop();

        // Dispose the game controller manager if it was created
        try
        {
            var controllerManager = GameControllerManager.GetInstance(_logger);
            controllerManager.Dispose();
            _logger.LogInformation("Game controller manager disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing game controller manager");
        }

        // Log performance statistics on shutdown (only once)
        if (!_hasLoggedPerformanceStats)
        {
            LogPerformanceStatistics();
            _hasLoggedPerformanceStats = true;
        }

        return base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Gets the MIDI manager instance
    /// </summary>
    /// <returns>The MIDI manager</returns>
    public MidiDeviceManager GetMidiDeviceManager()
    {
        return _MidiDeviceManager;
    }

    /// <summary>
    /// Gets processor statistics for performance monitoring
    /// </summary>
    /// <returns>Processor statistics, or null if not available</returns>
    public ProcessorStatistics? GetProcessorStatistics()
    {
        return _ProfileManager.GetProcessorStatistics();
    }

    /// <summary>
    /// Enables latency measurement for performance analysis
    /// </summary>
    public void EnableLatencyMeasurement()
    {
        _ProfileManager.EnableLatencyMeasurement();
    }

    /// <summary>
    /// Logs performance statistics to the logger
    /// </summary>
    public void LogPerformanceStatistics()
    {
        try
        {
            var stats = GetProcessorStatistics();
            if (stats == null)
            {
                _logger.LogInformation("No performance statistics available");
                return;
            }

            _logger.LogInformation("=== MIDIFlux Performance Statistics ===");

            // Registry statistics
            var registry = stats.RegistryStatistics;
            _logger.LogInformation("Registry: {EnabledMappings}/{TotalMappings} enabled mappings, {LookupKeys} lookup keys, {UniqueDevices} devices, {UniqueChannels} channels",
                registry.EnabledMappings, registry.TotalMappings, registry.LookupKeys, registry.UniqueDevices, registry.UniqueChannels);

            // Latency statistics
            var latency = stats.LatencyStatistics;
            if (latency.TotalMeasurements > 0)
            {
                _logger.LogInformation("Latency: {TotalMeasurements} measurements",
                    latency.TotalMeasurements);
                _logger.LogInformation("Latency: Avg={AvgMs:F3}ms, 95th={P95Ms:F3}ms, Max={MaxMs:F3}ms",
                    latency.AverageLatencyMs, latency.P95LatencyMs, latency.MaxLatencyMs);

                if (latency.HighLatencyCount > 0)
                {
                    var percentage = (latency.HighLatencyCount * 100.0) / latency.TotalMeasurements;
                    _logger.LogWarning("High latency events (>10ms): {HighLatencyCount} ({Percentage:F2}%)",
                        latency.HighLatencyCount, percentage);
                }
                else
                {
                    _logger.LogInformation("No high latency events detected (all events <10ms)");
                }
            }
            else
            {
                _logger.LogInformation("No latency measurements collected (measurement was not enabled)");
            }

            _logger.LogInformation("=== End Performance Statistics ===");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging performance statistics");
        }
    }

    /// <summary>
    /// Delegates to ConfigurationManager.AddDevice()
    /// </summary>
    public bool AddDevice(DeviceConfig device) => _configManager.AddDevice(device);

    /// <summary>
    /// Delegates to ConfigurationManager.RemoveDevice()
    /// </summary>
    public bool RemoveDevice(string deviceName) => _configManager.RemoveDevice(deviceName);

    /// <summary>
    /// Delegates to ConfigurationManager.AddMapping()
    /// </summary>
    public bool AddMapping(string deviceName, MappingConfigEntry mapping) => _configManager.AddMapping(deviceName, mapping);

    /// <summary>
    /// Delegates to ConfigurationManager.RemoveMapping()
    /// </summary>
    public bool RemoveMapping(string deviceName, MappingConfigEntry mapping) => _configManager.RemoveMapping(deviceName, mapping);

    /// <summary>
    /// Delegates to ConfigurationManager.GetCurrentRuntimeConfiguration()
    /// </summary>
    public MappingConfig? GetCurrentRuntimeConfiguration() => _configManager.GetCurrentRuntimeConfiguration();
}
