using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using System.Collections.Concurrent;
using System.Reflection;

namespace MIDIFlux.App.Services;

/// <summary>
/// Manages configuration loading and saving.
/// Completely replaces the old Models.Configuration system with MappingConfig.
/// </summary>
public class ConfigurationManager
{
    private readonly ILogger _logger;
    private readonly ActionConfigurationLoader _configLoader;
    private readonly ConcurrentDictionary<string, MappingConfig> _configurations = new();
    private string _activeConfigurationPath = string.Empty;

    /// <summary>
    /// Event raised when the active configuration changes
    /// </summary>
    public event EventHandler<string>? ConfigurationChanged;

    /// <summary>
    /// Gets the currently active configuration path
    /// </summary>
    public string ActiveConfigurationPath => _activeConfigurationPath;

    /// <summary>
    /// Gets the list of available configurations
    /// </summary>
    public IEnumerable<string> AvailableConfigurations => _configurations.Keys;

    /// <summary>
    /// Creates a new instance of the ConfigurationManager with action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="configLoader">The action configuration loader</param>
    public ConfigurationManager(ILogger logger, ActionConfigurationLoader configLoader)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configLoader = configLoader ?? throw new ArgumentNullException(nameof(configLoader));

        _logger.LogDebug("ConfigurationManager initialized with action system");
    }

    /// <summary>
    /// Loads a configuration from a file
    /// </summary>
    /// <param name="configPath">The path to the configuration file</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool LoadConfiguration(string configPath)
    {
        try
        {
            if (!File.Exists(configPath))
            {
                _logger.LogError("Configuration file does not exist: {ConfigPath}", configPath);
                return false;
            }

            // Check if we already have this configuration loaded
            if (_configurations.TryGetValue(configPath, out var existingConfig))
            {
                _activeConfigurationPath = configPath;
                ConfigurationChanged?.Invoke(this, configPath);

                // Save the current configuration path
                SaveCurrentConfigurationPath(configPath);

                return true;
            }

            // Load the unified configuration
            MappingConfig? config = null;
            try
            {
                config = _configLoader.LoadConfiguration(configPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load unified configuration from {ConfigPath}: {Message}", configPath, ex.Message);

                // Show MessageBox error for user feedback
                MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                    $"Failed to load profile configuration:\n\n{ex.Message}",
                    "MIDIFlux - Profile Load Error",
                    _logger,
                    ex);
                return false;
            }

            if (config == null)
            {
                _logger.LogError("Failed to load unified configuration from {ConfigPath}: Configuration loader returned null", configPath);

                // Show MessageBox error for user feedback
                MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                    $"Failed to load profile configuration from:\n{configPath}\n\nThe profile file could not be parsed or is invalid.\n\nPlease check the logs for more details.",
                    "MIDIFlux - Profile Load Error",
                    _logger);
                return false;
            }

            // Initialize required hardware based on configuration
            InitializeRequiredHardware(config);

            // Store the configuration
            _configurations[configPath] = config;
            _activeConfigurationPath = configPath;
            ConfigurationChanged?.Invoke(this, configPath);

            // Save the current configuration path
            SaveCurrentConfigurationPath(configPath);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets the active unified configuration.
    /// Replaces the old Models.Configuration with MappingConfig.
    /// </summary>
    /// <returns>The active unified configuration, or null if none is loaded</returns>
    public MappingConfig? GetActiveConfiguration()
    {
        if (string.IsNullOrEmpty(_activeConfigurationPath))
        {
            _logger.LogDebug("No active configuration path set");
            return null;
        }

        if (_configurations.TryGetValue(_activeConfigurationPath, out var config))
        {
            return config;
        }

        _logger.LogWarning("Active configuration path '{ConfigPath}' not found in loaded configurations", _activeConfigurationPath);
        return null;
    }

    /// <summary>
    /// Gets the current runtime configuration by extracting mappings from the registry.
    /// Converts current registry state back to MappingConfig format.
    /// </summary>
    /// <returns>Current configuration as MappingConfig, or null if no configuration loaded</returns>
    public MappingConfig? GetCurrentRuntimeConfiguration()
    {
        try
        {
            // Get the active configuration to use as a base
            var activeConfig = GetActiveConfiguration();
            if (activeConfig == null)
            {
                _logger.LogWarning("No active configuration available for runtime extraction");
                return null;
            }

            // Return the active configuration as it represents the current runtime state
            // The configuration is kept synchronized with registry changes through
            // the Add/Remove methods that update both registry and configuration
            _logger.LogDebug("Returning synchronized runtime configuration for profile '{ProfileName}'",
                activeConfig.ProfileName);
            return activeConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current runtime configuration: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Adds a device to the current runtime configuration.
    /// Updates both the cached configuration and the active registry.
    /// </summary>
    /// <param name="device">The device configuration to add</param>
    /// <returns>True if successful, false if device already exists or operation failed</returns>
    public bool AddDevice(DeviceConfig device)
    {
        if (device == null)
        {
            _logger.LogWarning("Cannot add null device configuration");
            return false;
        }

        try
        {
            var activeConfig = GetActiveConfiguration();
            if (activeConfig == null)
            {
                _logger.LogWarning("No active configuration available for device addition");
                return false;
            }

            // Check if device already exists
            var existingDevice = activeConfig.MidiDevices.FirstOrDefault(d =>
                string.Equals(d.DeviceName, device.DeviceName, StringComparison.OrdinalIgnoreCase));

            if (existingDevice != null)
            {
                _logger.LogWarning("Device '{DeviceName}' already exists in configuration", device.DeviceName);
                return false;
            }

            // Add device to configuration
            activeConfig.MidiDevices.Add(device);

            _logger.LogInformation("Successfully added device '{DeviceName}' to runtime configuration", device.DeviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add device to runtime configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Removes a device from the current runtime configuration.
    /// Updates both the cached configuration and the active registry.
    /// </summary>
    /// <param name="deviceName">The device name to remove</param>
    /// <returns>True if device was found and removed, false otherwise</returns>
    public bool RemoveDevice(string deviceName)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            _logger.LogWarning("Cannot remove device with null or empty name");
            return false;
        }

        try
        {
            var activeConfig = GetActiveConfiguration();
            if (activeConfig == null)
            {
                _logger.LogWarning("No active configuration available for device removal");
                return false;
            }

            // Find and remove device
            var deviceToRemove = activeConfig.MidiDevices.FirstOrDefault(d =>
                string.Equals(d.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

            if (deviceToRemove == null)
            {
                _logger.LogWarning("Device '{DeviceName}' not found in configuration", deviceName);
                return false;
            }

            activeConfig.MidiDevices.Remove(deviceToRemove);

            _logger.LogInformation("Successfully removed device '{DeviceName}' from runtime configuration", deviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove device from runtime configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Adds a mapping to a specific device in the current runtime configuration.
    /// Updates both the cached configuration and the active registry.
    /// </summary>
    /// <param name="deviceName">The device name to add the mapping to</param>
    /// <param name="mappingConfig">The mapping configuration to add</param>
    /// <returns>True if successful, false if device not found or operation failed</returns>
    public bool AddMapping(string deviceName, MappingConfigEntry mappingConfig)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            _logger.LogWarning("Cannot add mapping to device with null or empty name");
            return false;
        }

        if (mappingConfig == null)
        {
            _logger.LogWarning("Cannot add null mapping configuration");
            return false;
        }

        try
        {
            var activeConfig = GetActiveConfiguration();
            if (activeConfig == null)
            {
                _logger.LogWarning("No active configuration available for mapping addition");
                return false;
            }

            // Find device
            var device = activeConfig.MidiDevices.FirstOrDefault(d =>
                string.Equals(d.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

            if (device == null)
            {
                _logger.LogWarning("Device '{DeviceName}' not found in configuration", deviceName);
                return false;
            }

            // Add mapping to device
            device.Mappings.Add(mappingConfig);

            _logger.LogInformation("Successfully added mapping '{Description}' to device '{DeviceName}'",
                mappingConfig.Description ?? "No description", deviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add mapping to runtime configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Removes a mapping from a specific device in the current runtime configuration.
    /// Updates both the cached configuration and the active registry.
    /// </summary>
    /// <param name="deviceName">The device name to remove the mapping from</param>
    /// <param name="mappingConfig">The mapping configuration to remove</param>
    /// <returns>True if mapping was found and removed, false otherwise</returns>
    public bool RemoveMapping(string deviceName, MappingConfigEntry mappingConfig)
    {
        if (string.IsNullOrWhiteSpace(deviceName))
        {
            _logger.LogWarning("Cannot remove mapping from device with null or empty name");
            return false;
        }

        if (mappingConfig == null)
        {
            _logger.LogWarning("Cannot remove null mapping configuration");
            return false;
        }

        try
        {
            var activeConfig = GetActiveConfiguration();
            if (activeConfig == null)
            {
                _logger.LogWarning("No active configuration available for mapping removal");
                return false;
            }

            // Find device
            var device = activeConfig.MidiDevices.FirstOrDefault(d =>
                string.Equals(d.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

            if (device == null)
            {
                _logger.LogWarning("Device '{DeviceName}' not found in configuration", deviceName);
                return false;
            }

            // Find and remove mapping (match by input configuration)
            var mappingToRemove = device.Mappings.FirstOrDefault(m => MappingConfigsMatch(m, mappingConfig));

            if (mappingToRemove == null)
            {
                _logger.LogWarning("Mapping not found in device '{DeviceName}'", deviceName);
                return false;
            }

            device.Mappings.Remove(mappingToRemove);

            _logger.LogInformation("Successfully removed mapping '{Description}' from device '{DeviceName}'",
                mappingToRemove.Description ?? "No description", deviceName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove mapping from runtime configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing mapping in a specific device by replacing it atomically.
    /// Identifies the existing mapping by input configuration, then replaces it with the new mapping at the same index.
    /// </summary>
    /// <param name="deviceName">The device name containing the mapping</param>
    /// <param name="oldMapping">The mapping to find (identified by input configuration)</param>
    /// <param name="newMapping">The new mapping to replace it with</param>
    /// <returns>True if mapping was found and updated, false otherwise</returns>
    public bool UpdateMapping(string deviceName, MappingConfigEntry oldMapping, MappingConfigEntry newMapping)
    {
        try
        {
            var config = GetActiveConfiguration();
            if (config == null)
            {
                _logger.LogWarning("Cannot update mapping: no active configuration");
                return false;
            }

            var device = config.MidiDevices?.FirstOrDefault(d =>
                string.Equals(d.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

            if (device == null)
            {
                _logger.LogWarning("Device '{DeviceName}' not found in active configuration", deviceName);
                return false;
            }

            // Find the existing mapping by input configuration
            var index = device.Mappings.FindIndex(m => MappingConfigsMatch(m, oldMapping));

            if (index < 0)
            {
                _logger.LogWarning("Mapping not found in device '{DeviceName}' for update", deviceName);
                return false;
            }

            // Atomic replace at the same index
            device.Mappings[index] = newMapping;

            _logger.LogInformation("Successfully updated mapping at index {Index} in device '{DeviceName}': '{OldDescription}' -> '{NewDescription}'",
                index, deviceName,
                oldMapping.Description ?? "No description",
                newMapping.Description ?? "No description");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update mapping in runtime configuration: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Checks if two mapping configurations match by their input configuration.
    /// Used for finding existing mappings when adding/removing.
    /// </summary>
    /// <param name="mapping1">First mapping to compare</param>
    /// <param name="mapping2">Second mapping to compare</param>
    /// <returns>True if mappings have the same input configuration</returns>
    private bool MappingConfigsMatch(MappingConfigEntry mapping1, MappingConfigEntry mapping2)
    {
        if (mapping1 == null || mapping2 == null)
            return false;

        return string.Equals(mapping1.InputType, mapping2.InputType, StringComparison.OrdinalIgnoreCase) &&
               mapping1.Channel == mapping2.Channel &&
               mapping1.Note == mapping2.Note &&
               mapping1.ControlNumber == mapping2.ControlNumber &&
               string.Equals(mapping1.SysExPattern, mapping2.SysExPattern, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Gets the path to the current.json file
    /// </summary>
    /// <returns>The path to the current.json file</returns>
    private string GetCurrentConfigFilePath()
    {
        return AppDataHelper.GetCurrentConfigFilePath();
    }

    /// <summary>
    /// Loads the path of the last used configuration from the special file
    /// </summary>
    /// <returns>The path to the last used configuration, or null if not found</returns>
    public string? LoadLastUsedConfigurationPath()
    {
        try
        {
            // Load from the regular current.json
            string currentConfigPath = GetCurrentConfigFilePath();

            LastUsedConfig? currentConfig = null;
            if (File.Exists(currentConfigPath))
            {
                try
                {
                    var json = File.ReadAllText(currentConfigPath);
                    currentConfig = System.Text.Json.JsonSerializer.Deserialize<LastUsedConfig>(json);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error reading last used configuration file: {Message}", ex.Message);
                }
            }

            if (currentConfig == null || string.IsNullOrEmpty(currentConfig.ConfigPath))
            {
                _logger.LogDebug("No valid last used configuration data found");
                return null;
            }

            // Verify the configuration file exists
            if (!File.Exists(currentConfig.ConfigPath))
            {
                _logger.LogWarning("Last used configuration file does not exist: {ConfigPath}", currentConfig.ConfigPath);
                return null;
            }

            _logger.LogInformation("Found last used configuration: {ConfigPath}", currentConfig.ConfigPath);
            return currentConfig.ConfigPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading last used configuration path: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Class to deserialize the last used configuration
    /// </summary>
    private class LastUsedConfig
    {
        public string? ConfigPath { get; set; }
    }

    /// <summary>
    /// Saves the path of the current configuration to a special file
    /// </summary>
    /// <param name="configPath">The path to the current configuration</param>
    private void SaveCurrentConfigurationPath(string configPath)
    {
        try
        {
            string currentConfigPath = GetCurrentConfigFilePath();

            // Create a simple object with the configuration path
            var currentConfig = new { ConfigPath = configPath };

            try
            {
                var json = System.Text.Json.JsonSerializer.Serialize(currentConfig, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(currentConfigPath, json);
                _logger.LogDebug("Saved current configuration path to {CurrentConfigPath}", currentConfigPath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing current configuration path file: {Message}", ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving current configuration path: {ErrorMessage}", ex.Message);
        }
    }

    /// <summary>
    /// Analyzes the configuration and initializes required hardware systems
    /// </summary>
    /// <param name="config">The loaded configuration to analyze</param>
    private void InitializeRequiredHardware(MappingConfig config)
    {
        try
        {
            _logger.LogDebug("Analyzing configuration for required hardware initialization");

            bool needsGameController = false;

            // Scan all actions in the configuration
            foreach (var device in config.MidiDevices)
            {
                foreach (var mapping in device.Mappings)
                {
                    if (mapping.Action != null)
                    {
                        needsGameController |= RequiresGameController(mapping.Action);
                    }
                }
            }

            // Initialize GameController if needed
            if (needsGameController)
            {
                _logger.LogInformation("Configuration requires GameController - initializing ViGEm");
                try
                {
                    var gameControllerManager = GameControllerManager.GetInstance(_logger);
                    if (gameControllerManager.IsViGEmAvailable)
                    {
                        _logger.LogInformation("ViGEm initialized successfully for GameController actions");
                    }
                    else
                    {
                        _logger.LogWarning("ViGEm Bus Driver not available - GameController actions will not work");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to initialize GameController: {Message}", ex.Message);
                    throw new InvalidOperationException($"Failed to initialize GameController system: {ex.Message}", ex);
                }
            }
            else
            {
                _logger.LogDebug("Configuration does not require GameController - skipping ViGEm initialization");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during hardware initialization: {Message}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Recursively checks if an action requires GameController functionality
    /// </summary>
    /// <param name="action">The action to check</param>
    /// <returns>True if GameController is required</returns>
    private bool RequiresGameController(object action)
    {
        if (action == null) return false;

        var actionTypeName = action.GetType().Name;

        // Check for GameController action types
        if (actionTypeName.StartsWith("GameController", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check for SequenceAction with GameController sub-actions
        if (actionTypeName == "SequenceAction")
        {
            // Use reflection to get SubActions property
            var subActionsProperty = action.GetType().GetProperty("SubActions");
            if (subActionsProperty?.GetValue(action) is IEnumerable<object> subActions)
            {
                foreach (var subAction in subActions)
                {
                    if (RequiresGameController(subAction))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }
}
