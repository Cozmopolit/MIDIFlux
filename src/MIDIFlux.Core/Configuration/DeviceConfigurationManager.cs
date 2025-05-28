using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Config;

/// <summary>
/// Manages device configurations and mappings using the action system.
/// Completely replaces the old fragmented mapping system with a single unified approach.
/// </summary>
public class DeviceConfigurationManager
{
    private readonly ILogger<DeviceConfigurationManager> _logger;
    private readonly IServiceProvider? _serviceProvider;
    private readonly ActionMappingRegistry _actionRegistry;
    private readonly ActionConfigurationLoader _configurationLoader;
    private MappingConfig? _configuration;

    /// <summary>
    /// Creates a new instance of the DeviceConfigurationManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="actionFactory">The action factory to use</param>
    /// <param name="configurationService">The configuration service for file operations</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public DeviceConfigurationManager(
        ILogger<DeviceConfigurationManager> logger,
        IActionFactory actionFactory,
        ConfigurationService configurationService,
        IServiceProvider? serviceProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider;

        // Create the action registry with proper logger type
        var registryLogger = LoggingHelper.CreateLogger<ActionMappingRegistry>();
        _actionRegistry = new ActionMappingRegistry(registryLogger);

        // Create the configuration loader using the unified configuration service
        _configurationLoader = new ActionConfigurationLoader(logger, actionFactory, configurationService);

        _logger.LogDebug("DeviceConfigurationManager initialized with action system");
    }

    /// <summary>
    /// Sets the unified configuration to use for mapping MIDI events to actions.
    /// Completely replaces the old configuration system with the action system.
    /// </summary>
    /// <param name="configuration">The unified configuration to use</param>
    public void SetConfiguration(MappingConfig configuration)
    {
        try
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

            _logger.LogInformation("Loading unified configuration '{ProfileName}' with {DeviceCount} MIDI devices",
                configuration.ProfileName, configuration.MidiDevices.Count);

            // Load mappings into the registry with atomic update
            var success = _configurationLoader.LoadMappingsIntoRegistry(_actionRegistry, configuration);

            if (success)
            {
                var stats = _actionRegistry.GetStatistics();
                _logger.LogInformation("Successfully loaded unified configuration: {Statistics}", stats);
            }
            else
            {
                var errorMessage = "Failed to load unified configuration into registry";
                _logger.LogError(errorMessage);
                ApplicationErrorHandler.ShowError(errorMessage, "MIDIFlux - Configuration Loading Error", _logger);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting unified configuration: {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowError(
                $"Failed to load configuration: {ex.Message}",
                "MIDIFlux - Configuration Loading Error",
                _logger,
                ex);
            throw;
        }
    }

    /// <summary>
    /// Gets the action registry for MIDI event processing.
    /// Replaces the old device handlers system with action lookup.
    /// </summary>
    /// <returns>The action mapping registry</returns>
    public ActionMappingRegistry GetActionRegistry()
    {
        return _actionRegistry;
    }

    /// <summary>
    /// Finds actions for the given MIDI input using the action system.
    /// Replaces all old handler lookup methods with a single unified approach.
    /// </summary>
    /// <param name="input">The MIDI input to find actions for</param>
    /// <returns>List of matching actions, empty if no matches found</returns>
    public List<IAction> FindActions(ActionMidiInput input)
    {
        try
        {
            return _actionRegistry.FindActions(input);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding actions for MIDI input: {Input}", input);
            ApplicationErrorHandler.ShowError(
                $"Error finding actions for MIDI input: {ex.Message}",
                "MIDIFlux - Action Lookup Error",
                _logger,
                ex);
            return new List<IAction>();
        }
    }

    /// <summary>
    /// Finds all unified device configurations that match a given device ID.
    /// Replaces the old MidiDeviceConfiguration lookup with unified device configs.
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>A list of matching unified device configurations, or an empty list if none found</returns>
    public List<DeviceConfig> FindDeviceConfigsForId(int deviceId)
    {
        if (_configuration == null)
        {
            _logger.LogDebug("No configuration loaded, returning empty device config list");
            return new List<DeviceConfig>();
        }

        try
        {
            // Get the device name from the device ID
            var deviceName = GetDeviceNameFromId(deviceId);
            if (string.IsNullOrEmpty(deviceName))
            {
                _logger.LogWarning("Could not find device name for device ID {DeviceId}", deviceId);
                return new List<DeviceConfig>();
            }

            // Find all configurations that match this device name
            var matchingConfigs = _configuration.MidiDevices
                .Where(config => deviceName.Equals(config.DeviceName, StringComparison.OrdinalIgnoreCase) ||
                                 deviceName.Contains(config.DeviceName, StringComparison.OrdinalIgnoreCase) ||
                                 config.DeviceName == "*") // Include wildcard devices
                .ToList();

            _logger.LogDebug("Found {Count} device configurations for device '{DeviceName}' (ID: {DeviceId})",
                matchingConfigs.Count, deviceName, deviceId);

            return matchingConfigs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding device configurations for device ID {DeviceId}", deviceId);
            ApplicationErrorHandler.ShowError(
                $"Error finding device configurations: {ex.Message}",
                "MIDIFlux - Device Configuration Lookup Error",
                _logger,
                ex);
            return new List<DeviceConfig>();
        }
    }

    /// <summary>
    /// Gets the current unified configuration.
    /// Provides access to the loaded configuration for external components.
    /// </summary>
    /// <returns>The current unified configuration, or null if none loaded</returns>
    public MappingConfig? GetConfiguration()
    {
        return _configuration;
    }

    /// <summary>
    /// Gets registry statistics for monitoring and debugging.
    /// Provides insight into the current state of the action registry.
    /// </summary>
    /// <returns>Registry statistics</returns>
    public RegistryStatistics GetRegistryStatistics()
    {
        return _actionRegistry.GetStatistics();
    }

    /// <summary>
    /// Gets the device name from a device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device name, or an empty string if not found</returns>
    private string GetDeviceNameFromId(int deviceId)
    {
        try
        {
            // Try to get the device info from the MidiManager
            var midiManager = _serviceProvider?.GetService(typeof(MidiManager)) as MidiManager;
            if (midiManager != null)
            {
                var deviceInfo = midiManager.GetDeviceInfo(deviceId);
                if (deviceInfo != null)
                {
                    return deviceInfo.Name;
                }
            }

            _logger.LogDebug("Could not resolve device name for device ID {DeviceId}", deviceId);
            return string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting device name for device ID {DeviceId}: {ErrorMessage}", deviceId, ex.Message);
            return string.Empty;
        }
    }
}
