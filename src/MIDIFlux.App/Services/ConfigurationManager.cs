using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using System.Collections.Concurrent;

namespace MIDIFlux.App.Services;

/// <summary>
/// Manages configuration loading and saving
/// </summary>
public class ConfigurationManager
{
    private readonly ILogger _logger;
    private readonly ConfigLoader _configLoader;
    private readonly ConfigurationFileManager _fileManager;
    private readonly ConcurrentDictionary<string, Configuration> _configurations = new();
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
    /// Creates a new instance of the ConfigurationManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="configLoader">The configuration loader</param>
    public ConfigurationManager(ILogger logger, ConfigLoader configLoader)
    {
        _logger = logger;
        _configLoader = configLoader;
        _fileManager = new ConfigurationFileManager(logger);
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
            if (!_fileManager.ValidateFileExists(configPath, "configuration file"))
            {
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

            // Load the configuration
            var config = _configLoader.LoadConfiguration(configPath);
            if (config == null)
            {
                _logger.LogError("Failed to load configuration from {ConfigPath}", configPath);
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
    /// Gets the active configuration
    /// </summary>
    /// <returns>The active configuration, or null if none is loaded</returns>
    public Configuration? GetActiveConfiguration()
    {
        if (string.IsNullOrEmpty(_activeConfigurationPath))
        {
            return null;
        }

        if (_configurations.TryGetValue(_activeConfigurationPath, out var config))
        {
            return config;
        }

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

            var currentConfig = _fileManager.ReadJsonFile<LastUsedConfig>(currentConfigPath, "last used configuration file");

            if (currentConfig == null || string.IsNullOrEmpty(currentConfig.ConfigPath))
            {
                _logger.LogDebug("No valid last used configuration data found");
                return null;
            }

            // Verify the configuration file exists
            if (!_fileManager.ValidateFileExists(currentConfig.ConfigPath, "last used configuration file"))
            {
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

            if (_fileManager.WriteJsonFile(currentConfig, currentConfigPath, "current configuration path"))
            {
                _logger.LogDebug("Saved current configuration path to {CurrentConfigPath}", currentConfigPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving current configuration path: {ErrorMessage}", ex.Message);
        }
    }
}
