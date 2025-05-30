using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using System.Collections.Concurrent;

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
}
