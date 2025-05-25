using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.State;

namespace MIDIFlux.App.Services;

/// <summary>
/// Background service for MIDI processing
/// </summary>
public class MidiProcessingService : BackgroundService
{
    private readonly ILogger<MidiProcessingService> _logger;
    private readonly MidiManager _midiManager;
    private readonly EventDispatcher _eventDispatcher;
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
    /// Creates a new instance of the MidiProcessingService with unified action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="midiManager">The MIDI manager</param>
    /// <param name="eventDispatcher">The event dispatcher</param>
    /// <param name="actionFactory">The unified action factory</param>
    /// <param name="actionStateManager">The action state manager</param>
    public MidiProcessingService(
        ILogger<MidiProcessingService> logger,
        MidiManager midiManager,
        EventDispatcher eventDispatcher,
        IUnifiedActionFactory actionFactory,
        ActionStateManager actionStateManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _midiManager = midiManager ?? throw new ArgumentNullException(nameof(midiManager));
        _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager));

        // Create the configuration file manager
        var fileManager = new ConfigurationFileManager(logger);

        // Create the unified action configuration loader
        var configLoader = new UnifiedActionConfigurationLoader(logger, actionFactory, fileManager);

        // Create the configuration manager with unified system
        _configManager = new ConfigurationManager(logger, configLoader);
        _configManager.ConfigurationChanged += ConfigManager_ConfigurationChanged;

        // Create the device connection handler
        _connectionHandler = new DeviceConnectionHandler(logger, midiManager, _configManager);

        // Connect the MidiManager directly to the EventDispatcher
        _midiManager.SetEventDispatcher(_eventDispatcher);

        _logger.LogDebug("MidiProcessingService initialized with unified action system");
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
            _eventDispatcher.SetConfiguration(config);

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
        var devices = _midiManager.GetAvailableDevices();

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

                // Use the helper to find the device
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
        else
        {
            _logger.LogWarning("No device configurations found in unified configuration");
        }

        // If no devices were found, use the first available
        if (_connectionHandler.SelectedDeviceIds.Count == 0 && devices.Count > 0)
        {
            _connectionHandler.AddSelectedDeviceId(devices[0].DeviceId);
            _logger.LogInformation("No configured devices found or matched, using first available: {Device}", devices[0]);
        }

        if (_connectionHandler.SelectedDeviceIds.Count == 0)
        {
            _logger.LogError("No MIDI devices selected");
            return false;
        }

        // Start listening for MIDI events on all selected devices
        bool anySuccess = false;
        foreach (var deviceId in _connectionHandler.SelectedDeviceIds)
        {
            if (_midiManager.StartListening(deviceId))
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

        _midiManager.StopListening();
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

    /// <summary>
    /// Stops the background service
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    public override Task StopAsync(CancellationToken cancellationToken)
    {
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

        return base.StopAsync(cancellationToken);
    }
}
