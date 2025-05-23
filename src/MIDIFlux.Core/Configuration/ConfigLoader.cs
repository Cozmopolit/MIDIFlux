using System.Text.Json;
using System.Text.Json.Serialization;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Configuration;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Config;

/// <summary>
/// Loads and parses configuration files
/// </summary>
public class ConfigLoader
{
    private readonly ILogger _logger;
    private readonly ConfigurationFileManager _fileManager;

    /// <summary>
    /// Creates a new instance of the ConfigLoader
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public ConfigLoader(ILogger<ConfigLoader> logger)
    {
        _logger = logger;
        _fileManager = new ConfigurationFileManager(logger);
    }

    /// <summary>
    /// Loads a configuration from a file
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration, or null if loading failed</returns>
    public Models.Configuration? LoadConfiguration(string filePath)
    {
        var config = _fileManager.LoadConfiguration(filePath);

        if (config != null)
        {
            _logger.LogInformation("Loaded configuration with {MappingCount} mappings", config.Mappings.Count);
        }

        return config;
    }

    /// <summary>
    /// Saves a configuration to a file
    /// </summary>
    /// <param name="config">The configuration to save</param>
    /// <param name="filePath">The path to save the configuration to</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SaveConfiguration(Models.Configuration config, string filePath)
    {
        return _fileManager.SaveConfiguration(config, filePath);
    }
}

/// <summary>
/// JSON converter for Configuration that includes validation
/// </summary>
public class ConfigurationJsonConverter : JsonConverter<Models.Configuration>
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the ConfigurationJsonConverter
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public ConfigurationJsonConverter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Reads and validates a Configuration from JSON
    /// </summary>
    public override Models.Configuration? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Create a new JsonSerializerOptions without this converter to avoid infinite recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();

        // Deserialize the configuration
        var config = JsonSerializer.Deserialize<Models.Configuration>(ref reader, newOptions);

        if (config == null)
        {
            _logger.LogError("Failed to deserialize configuration");
            return null;
        }

        // Validate the configuration
        if (!ValidateConfiguration(config, out string? errorMessage))
        {
            throw new JsonException($"Configuration validation failed: {errorMessage}");
        }

        return config;
    }

    /// <summary>
    /// Writes a Configuration to JSON
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Models.Configuration value, JsonSerializerOptions options)
    {
        // Create a new JsonSerializerOptions without this converter to avoid infinite recursion
        var newOptions = new JsonSerializerOptions(options);
        newOptions.Converters.Clear();

        // Serialize the configuration
        JsonSerializer.Serialize(writer, value, newOptions);
    }

    /// <summary>
    /// Validates a configuration for critical issues that could cause crashes
    /// </summary>
    /// <param name="config">The configuration to validate</param>
    /// <param name="errorMessage">The error message if validation fails</param>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    private bool ValidateConfiguration(Models.Configuration config, out string? errorMessage)
    {
        errorMessage = null;

        // Check for null collections that could cause crashes
        if (config.MidiDevices == null)
        {
            errorMessage = "Configuration must contain a MidiDevices collection";
            return false;
        }

        // Validate each device configuration
        foreach (var device in config.MidiDevices)
        {
            if (device == null)
            {
                errorMessage = "Configuration contains a null MIDI device";
                return false;
            }

            // Ensure collections are not null to prevent crashes
            if (device.Mappings == null)
            {
                errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has null Mappings collection";
                return false;
            }

            if (device.AbsoluteControlMappings == null)
            {
                errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has null AbsoluteControlMappings collection";
                return false;
            }

            if (device.RelativeControlMappings == null)
            {
                errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has null RelativeControlMappings collection";
                return false;
            }

            if (device.CCRangeMappings == null)
            {
                errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has null CCRangeMappings collection";
                return false;
            }

            // Validate key mappings for critical issues
            foreach (var mapping in device.Mappings)
            {
                if (mapping == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' contains a null key mapping";
                    return false;
                }

                // Validate MIDI note range
                if (mapping.MidiNote < 0 || mapping.MidiNote > 127)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid MIDI note {mapping.MidiNote}. MIDI notes must be between 0 and 127.";
                    return false;
                }

                // Validate modifiers collection
                if (mapping.Modifiers == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has a key mapping with null Modifiers collection";
                    return false;
                }
            }

            // Validate absolute control mappings
            foreach (var mapping in device.AbsoluteControlMappings)
            {
                if (mapping == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' contains a null absolute control mapping";
                    return false;
                }

                // Validate control number range
                if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid control number {mapping.ControlNumber}. Control numbers must be between 0 and 127.";
                    return false;
                }

                // Validate parameters collection
                if (mapping.Parameters == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has an absolute control mapping with null Parameters collection";
                    return false;
                }
            }

            // Validate relative control mappings
            foreach (var mapping in device.RelativeControlMappings)
            {
                if (mapping == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' contains a null relative control mapping";
                    return false;
                }

                // Validate control number range
                if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid control number {mapping.ControlNumber}. Control numbers must be between 0 and 127.";
                    return false;
                }

                // Validate parameters collection
                if (mapping.Parameters == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has a relative control mapping with null Parameters collection";
                    return false;
                }
            }

            // Validate CC range mappings
            foreach (var mapping in device.CCRangeMappings)
            {
                if (mapping == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' contains a null CC range mapping";
                    return false;
                }

                // Validate control number range
                if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid control number {mapping.ControlNumber} in CC range mapping. Control numbers must be between 0 and 127.";
                    return false;
                }

                // Validate ranges collection
                if (mapping.Ranges == null)
                {
                    errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has a CC range mapping with null Ranges collection";
                    return false;
                }

                // Validate each range
                foreach (var range in mapping.Ranges)
                {
                    if (range == null)
                    {
                        errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' contains a null CC value range";
                        return false;
                    }

                    // Validate range values
                    if (range.MinValue < 0 || range.MinValue > 127 || range.MaxValue < 0 || range.MaxValue > 127)
                    {
                        errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid CC range values ({range.MinValue}-{range.MaxValue}). Values must be between 0 and 127.";
                        return false;
                    }

                    if (range.MinValue > range.MaxValue)
                    {
                        errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has invalid CC range where MinValue ({range.MinValue}) is greater than MaxValue ({range.MaxValue}).";
                        return false;
                    }

                    // Validate action
                    if (range.Action == null)
                    {
                        errorMessage = $"Device '{device.InputProfile ?? "Unknown"}' has a CC range with null Action";
                        return false;
                    }
                }
            }
        }

        return true;
    }
}
