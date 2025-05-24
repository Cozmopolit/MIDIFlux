using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Actions.Configuration;

namespace MIDIFlux.Core.Configuration;

/// <summary>
/// Unified manager for configuration file operations including loading, saving, and validation
/// Consolidates common file I/O operations and standardizes error handling across the application
/// </summary>
public class ConfigurationFileManager
{
    private readonly ILogger _logger;

    /// <summary>
    /// Standard JSON serialization options for reading configuration files
    /// </summary>
    public static JsonSerializerOptions ReadOptions => new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { }
    };

    /// <summary>
    /// Standard JSON serialization options for writing configuration files
    /// </summary>
    public static JsonSerializerOptions WriteOptions => new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// Creates a new instance of the ConfigurationFileManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public ConfigurationFileManager(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates that a file exists and logs appropriate messages
    /// </summary>
    /// <param name="filePath">The path to the file to validate</param>
    /// <param name="fileDescription">Description of the file for logging purposes</param>
    /// <returns>True if the file exists, false otherwise</returns>
    public bool ValidateFileExists(string filePath, string fileDescription = "file")
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            _logger.LogError("File path is null or empty for {FileDescription}", fileDescription);
            return false;
        }

        if (!File.Exists(filePath))
        {
            _logger.LogError("{FileDescription} not found: {FilePath}", fileDescription, filePath);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Ensures the directory for a file path exists
    /// </summary>
    /// <param name="filePath">The file path whose directory should be ensured</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool EnsureDirectoryExists(string filePath)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating directory for file {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Reads and deserializes a JSON file to the specified type
    /// </summary>
    /// <typeparam name="T">The type to deserialize to</typeparam>
    /// <param name="filePath">The path to the JSON file</param>
    /// <param name="fileDescription">Description of the file for logging purposes</param>
    /// <param name="customOptions">Optional custom JSON serialization options</param>
    /// <returns>The deserialized object, or null if unsuccessful</returns>
    public T? ReadJsonFile<T>(string filePath, string fileDescription = "JSON file", JsonSerializerOptions? customOptions = null) where T : class
    {
        try
        {
            if (!ValidateFileExists(filePath, fileDescription))
            {
                return null;
            }

            var json = File.ReadAllText(filePath);
            var options = customOptions ?? ReadOptions;

            var result = JsonSerializer.Deserialize<T>(json, options);

            if (result == null)
            {
                _logger.LogError("Failed to parse {FileDescription}: {FilePath}", fileDescription, filePath);
                return null;
            }

            _logger.LogDebug("Successfully loaded {FileDescription}: {FilePath}", fileDescription, filePath);
            return result;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON error loading {FileDescription} from {FilePath}: {ErrorMessage}", fileDescription, filePath, ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading {FileDescription} from {FilePath}: {ErrorMessage}", fileDescription, filePath, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Serializes and writes an object to a JSON file
    /// </summary>
    /// <typeparam name="T">The type of object to serialize</typeparam>
    /// <param name="obj">The object to serialize</param>
    /// <param name="filePath">The path to write the JSON file to</param>
    /// <param name="fileDescription">Description of the file for logging purposes</param>
    /// <param name="customOptions">Optional custom JSON serialization options</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool WriteJsonFile<T>(T obj, string filePath, string fileDescription = "JSON file", JsonSerializerOptions? customOptions = null)
    {
        try
        {
            if (!EnsureDirectoryExists(filePath))
            {
                return false;
            }

            var options = customOptions ?? WriteOptions;
            var json = JsonSerializer.Serialize(obj, options);

            File.WriteAllText(filePath, json);

            _logger.LogDebug("Successfully saved {FileDescription}: {FilePath}", fileDescription, filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving {FileDescription} to {FilePath}: {ErrorMessage}", fileDescription, filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets all files matching a pattern in a directory
    /// </summary>
    /// <param name="directoryPath">The directory to search</param>
    /// <param name="searchPattern">The search pattern (e.g., "*.json")</param>
    /// <param name="searchOption">The search option (default: AllDirectories)</param>
    /// <param name="ensureDirectoryExists">Whether to ensure the directory exists before searching</param>
    /// <returns>An array of file paths</returns>
    public string[] GetFiles(string directoryPath, string searchPattern, SearchOption searchOption = SearchOption.AllDirectories, bool ensureDirectoryExists = true)
    {
        try
        {
            if (ensureDirectoryExists)
            {
                AppDataHelper.EnsureDirectoriesExist();
            }

            if (!Directory.Exists(directoryPath))
            {
                _logger.LogWarning("Directory does not exist: {DirectoryPath}", directoryPath);
                return Array.Empty<string>();
            }

            var files = Directory.GetFiles(directoryPath, searchPattern, searchOption);
            _logger.LogDebug("Found {FileCount} files matching pattern '{SearchPattern}' in {DirectoryPath}", files.Length, searchPattern, directoryPath);

            return files;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting files from {DirectoryPath} with pattern {SearchPattern}: {ErrorMessage}", directoryPath, searchPattern, ex.Message);
            return Array.Empty<string>();
        }
    }

    /// <summary>
    /// Creates a default configuration file if it doesn't exist
    /// </summary>
    /// <param name="filePath">The path where the default configuration should be created</param>
    /// <returns>True if successful or file already exists, false otherwise</returns>
    public bool EnsureDefaultConfigurationExists(string filePath)
    {
        if (File.Exists(filePath))
        {
            return true;
        }

        try
        {
            // Create a default unified configuration with a simple mapping
            var config = new Actions.Configuration.UnifiedMappingConfig
            {
                ProfileName = "Default Profile",
                Description = "Default MIDIFlux configuration with basic key mapping",
                MidiDevices = new List<Actions.Configuration.UnifiedDeviceConfig>
                {
                    new Actions.Configuration.UnifiedDeviceConfig
                    {
                        InputProfile = "Default",
                        DeviceName = "MIDI Controller",
                        Mappings = new List<Actions.Configuration.UnifiedMappingConfigEntry>
                        {
                            new Actions.Configuration.UnifiedMappingConfigEntry
                            {
                                Id = "default-mapping",
                                Description = "YouTube mute toggle (M key)",
                                InputType = "NoteOn",
                                Note = 60,
                                Channel = 1,
                                IsEnabled = true,
                                Action = new Actions.Configuration.KeyPressReleaseConfig
                                {
                                    VirtualKeyCode = 77, // 'M' key
                                    Description = "Press M key"
                                }
                            }
                        }
                    }
                }
            };

            return WriteJsonFile(config, filePath, "default configuration");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default configuration at {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return false;
        }
    }

    // Legacy LoadConfiguration method removed - use UnifiedActionConfigurationLoader instead

    // Legacy SaveConfiguration method removed - use UnifiedActionConfigurationLoader instead
}
