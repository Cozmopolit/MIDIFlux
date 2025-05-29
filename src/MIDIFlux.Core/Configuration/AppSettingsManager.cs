using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Actions.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Configuration;

/// <summary>
/// Unified configuration service for both app settings and profile configurations.
/// Consolidates all configuration file operations into a single, simple service.
/// </summary>
public class ConfigurationService
{
    private readonly ILogger _logger;
    private readonly string _settingsPath;
    private readonly object _lock = new();

    // Centralized JSON serialization options
    private static readonly JsonSerializerOptions _standardReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    private static readonly JsonSerializerOptions _standardWriteOptions = new()
    {
        WriteIndented = true
    };

    private static readonly JsonSerializerOptions _actionOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = true,
        Converters = {
            new Actions.ActionJsonConverter(),
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Event raised when settings are changed
    /// </summary>
    public event EventHandler<AppSettingChangedEventArgs>? SettingChanged;

    /// <summary>
    /// Creates a new instance of the ConfigurationService
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public ConfigurationService(ILogger<ConfigurationService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _settingsPath = AppDataHelper.GetAppSettingsPath();
    }

    /// <summary>
    /// Updates a specific setting in appsettings.json
    /// </summary>
    /// <param name="settingPath">Dot-separated path to the setting (e.g., "Logging.LogLevel.Default")</param>
    /// <param name="value">New value for the setting</param>
    /// <returns>True if successful</returns>
    public bool UpdateSetting(string settingPath, object value)
    {
        lock (_lock)
        {
            try
            {
                var success = UpdateJsonProperty(_settingsPath, settingPath, value);
                if (success)
                {
                    SettingChanged?.Invoke(this, new AppSettingChangedEventArgs(settingPath, value));
                    _logger.LogInformation("Updated app setting {SettingPath} to {Value}", settingPath, value);
                }
                return success;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating app setting {SettingPath}: {Message}", settingPath, ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// Updates multiple settings atomically
    /// </summary>
    /// <param name="updates">Dictionary of setting paths and their new values</param>
    /// <returns>True if all updates were successful</returns>
    public bool UpdateSettings(Dictionary<string, object> updates)
    {
        lock (_lock)
        {
            try
            {
                bool allSuccessful = true;
                var appliedUpdates = new List<KeyValuePair<string, object>>();

                foreach (var update in updates)
                {
                    var success = UpdateJsonProperty(_settingsPath, update.Key, update.Value);
                    if (success)
                    {
                        appliedUpdates.Add(update);
                    }
                    else
                    {
                        allSuccessful = false;
                        _logger.LogError("Failed to update setting {SettingPath}", update.Key);
                        break;
                    }
                }

                if (allSuccessful)
                {
                    // Raise events for all successful changes
                    foreach (var update in appliedUpdates)
                    {
                        SettingChanged?.Invoke(this, new AppSettingChangedEventArgs(update.Key, update.Value));
                    }

                    _logger.LogInformation("Updated {Count} app settings successfully", updates.Count);
                }
                else
                {
                    _logger.LogError("Failed to update some app settings");
                }

                return allSuccessful;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating multiple app settings: {Message}", ex.Message);
                return false;
            }
        }
    }

    /// <summary>
    /// Gets a specific setting value from appsettings.json
    /// </summary>
    /// <typeparam name="T">Type of the setting value</typeparam>
    /// <param name="settingPath">Dot-separated path to the setting</param>
    /// <param name="defaultValue">Default value if setting is not found</param>
    /// <returns>The setting value or default</returns>
    public T GetSetting<T>(string settingPath, T defaultValue = default!)
    {
        lock (_lock)
        {
            try
            {
                if (!File.Exists(_settingsPath))
                {
                    _logger.LogWarning("App settings file does not exist: {SettingsPath}", _settingsPath);
                    return defaultValue;
                }

                var json = File.ReadAllText(_settingsPath);
                using var document = JsonDocument.Parse(json);

                var parts = settingPath.Split('.');
                JsonElement current = document.RootElement;

                foreach (var part in parts)
                {
                    if (!current.TryGetProperty(part, out current))
                    {
                        _logger.LogDebug("Setting path {SettingPath} not found, using default", settingPath);
                        return defaultValue;
                    }
                }

                // Convert JsonElement to the requested type
                if (typeof(T) == typeof(string))
                {
                    return (T)(object)(current.GetString() ?? defaultValue?.ToString() ?? "");
                }
                else if (typeof(T) == typeof(int))
                {
                    return (T)(object)current.GetInt32();
                }
                else if (typeof(T) == typeof(bool))
                {
                    return (T)(object)current.GetBoolean();
                }
                else if (typeof(T) == typeof(double))
                {
                    return (T)(object)current.GetDouble();
                }
                else if (typeof(T) == typeof(List<string>))
                {
                    var list = new List<string>();
                    if (current.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in current.EnumerateArray())
                        {
                            list.Add(item.GetString() ?? "");
                        }
                    }
                    return (T)(object)list;
                }
                else if (typeof(T) == typeof(Dictionary<string, string>))
                {
                    var dict = new Dictionary<string, string>();
                    if (current.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var property in current.EnumerateObject())
                        {
                            dict[property.Name] = property.Value.GetString() ?? "";
                        }
                    }
                    return (T)(object)dict;
                }
                else
                {
                    // For other types, try to deserialize
                    return JsonSerializer.Deserialize<T>(current.GetRawText()) ?? defaultValue;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting app setting {SettingPath}: {Message}", settingPath, ex.Message);
                return defaultValue;
            }
        }
    }

    /// <summary>
    /// Checks if the appsettings.json file exists and is valid
    /// </summary>
    /// <returns>True if the file exists and is valid JSON</returns>
    public bool IsValid()
    {
        try
        {
            if (!File.Exists(_settingsPath))
            {
                return false;
            }

            var json = File.ReadAllText(_settingsPath);
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "App settings file is invalid: {Message}", ex.Message);
            return false;
        }
    }



    /// <summary>
    /// Gets the path to the appsettings.json file
    /// </summary>
    public string SettingsPath => _settingsPath;

    // Profile Configuration Methods

    /// <summary>
    /// Loads a profile configuration from a JSON file
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration, or null if loading failed</returns>
    public MappingConfig? LoadProfileConfiguration(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("Profile configuration file does not exist: {FilePath}", filePath);
                throw new FileNotFoundException($"Profile configuration file not found: '{filePath}'");
            }

            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<MappingConfig>(json, _actionOptions);

            if (config?.IsValid() == true)
            {
                _logger.LogInformation("Successfully loaded profile configuration from {FilePath}", filePath);
                return config;
            }

            _logger.LogError("Invalid profile configuration in {FilePath}", filePath);
            throw new InvalidOperationException($"Profile configuration in '{filePath}' is invalid or failed validation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading profile configuration from {FilePath}: {Message}", filePath, ex.Message);
            throw new InvalidOperationException($"Failed to load profile configuration from '{filePath}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Saves a profile configuration to a JSON file
    /// </summary>
    /// <param name="config">The configuration to save</param>
    /// <param name="filePath">The path to save the file to</param>
    /// <returns>True if successful</returns>
    public bool SaveProfileConfiguration(MappingConfig config, string filePath)
    {
        try
        {
            if (!config.IsValid())
            {
                _logger.LogError("Cannot save invalid profile configuration to {FilePath}", filePath);
                return false;
            }

            var json = JsonSerializer.Serialize(config, _actionOptions);
            File.WriteAllText(filePath, json);

            _logger.LogInformation("Successfully saved profile configuration to {FilePath}", filePath);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving profile configuration to {FilePath}: {Message}", filePath, ex.Message);
            return false;
        }
    }

    // Private helper methods

    /// <summary>
    /// Updates a specific property in a JSON file
    /// </summary>
    private bool UpdateJsonProperty(string filePath, string propertyPath, object newValue)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                _logger.LogWarning("JSON file does not exist: {FilePath}", filePath);
                return false;
            }

            var json = File.ReadAllText(filePath);
            using var document = JsonDocument.Parse(json);

            using var memoryStream = new MemoryStream();
            using var jsonWriter = new Utf8JsonWriter(memoryStream, new JsonWriterOptions { Indented = true });

            var propertyParts = propertyPath.Split('.');
            WriteJsonWithPropertyUpdate(document.RootElement, jsonWriter, propertyParts, 0, newValue);

            // CRITICAL: Flush the writer before reading from the memory stream
            jsonWriter.Flush();

            var updatedJson = System.Text.Encoding.UTF8.GetString(memoryStream.ToArray());
            File.WriteAllText(filePath, updatedJson);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating JSON property {PropertyPath} in {FilePath}: {Message}", propertyPath, filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Recursively writes JSON with property updates
    /// </summary>
    private void WriteJsonWithPropertyUpdate(JsonElement element, Utf8JsonWriter writer, string[] propertyParts, int currentIndex, object newValue)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            writer.WriteStartObject();

            foreach (var property in element.EnumerateObject())
            {
                writer.WritePropertyName(property.Name);

                if (currentIndex < propertyParts.Length && property.Name == propertyParts[currentIndex])
                {
                    if (currentIndex == propertyParts.Length - 1)
                    {
                        // This is the property to update
                        WriteJsonValue(writer, newValue);
                    }
                    else
                    {
                        // Continue traversing
                        WriteJsonWithPropertyUpdate(property.Value, writer, propertyParts, currentIndex + 1, newValue);
                    }
                }
                else
                {
                    // Copy existing value
                    property.Value.WriteTo(writer);
                }
            }

            writer.WriteEndObject();
        }
        else
        {
            // Not an object, copy as-is
            element.WriteTo(writer);
        }
    }

    /// <summary>
    /// Writes a value to JSON writer based on its type
    /// </summary>
    private void WriteJsonValue(Utf8JsonWriter writer, object value)
    {
        switch (value)
        {
            case string s:
                writer.WriteStringValue(s);
                break;
            case int i:
                writer.WriteNumberValue(i);
                break;
            case double d:
                writer.WriteNumberValue(d);
                break;
            case bool b:
                writer.WriteBooleanValue(b);
                break;
            case null:
                writer.WriteNullValue();
                break;
            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}

/// <summary>
/// Event arguments for app setting changes
/// </summary>
public class AppSettingChangedEventArgs : EventArgs
{
    /// <summary>
    /// The path of the setting that changed
    /// </summary>
    public string SettingPath { get; }

    /// <summary>
    /// The new value of the setting
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Creates new app setting changed event arguments
    /// </summary>
    public AppSettingChangedEventArgs(string settingPath, object value)
    {
        SettingPath = settingPath;
        Value = value;
    }
}


