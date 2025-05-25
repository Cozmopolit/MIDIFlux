using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Configuration;

/// <summary>
/// Loads action configurations from JSON files with strongly-typed deserialization.
/// Provides type-safe configuration loading, validation, and conversion to runtime objects.
/// Uses ConfigurationFileManager for all file operations to eliminate duplication.
/// </summary>
public class ActionConfigurationLoader
{
    private readonly ILogger _logger;
    private readonly IActionFactory _actionFactory;
    private readonly ConfigurationFileManager _fileManager;

    /// <summary>
    /// Initializes a new instance of the ActionConfigurationLoader
    /// </summary>
    /// <param name="logger">The logger to use for logging</param>
    /// <param name="actionFactory">The factory for creating actions from configurations</param>
    /// <param name="fileManager">The configuration file manager for file operations</param>
    public ActionConfigurationLoader(ILogger logger, IActionFactory actionFactory, ConfigurationFileManager fileManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));
        _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    }

    /// <summary>
    /// Loads a action configuration from a JSON file.
    /// Performs strongly-typed deserialization and validation.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration, or null if loading failed</returns>
    public MappingConfig? LoadConfiguration(string filePath)
    {
        try
        {
            _logger.LogDebug("Loading action configuration from {FilePath}", filePath);

            // Use ConfigurationFileManager for file operations
            var config = _fileManager.ReadActionConfig(filePath, "action configuration");

            if (config == null)
            {
                return null;
            }

            // Validate the configuration structure
            if (!config.IsValid())
            {
                var errors = config.GetValidationErrors();
                var errorMessage = string.Join("; ", errors);
                _logger.LogError("Configuration validation failed for {FilePath}: {Errors}", filePath, errorMessage);
                return null;
            }

            _logger.LogInformation("Successfully loaded action configuration from {FilePath} with {DeviceCount} devices",
                filePath, config.MidiDevices.Count);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading configuration from {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Converts a unified mapping configuration to runtime mappings for the registry.
    /// Pre-compiles actions and validates parameters at creation time.
    /// </summary>
    /// <param name="config">The configuration to convert</param>
    /// <returns>A list of runtime mappings ready for registry loading</returns>
    public List<ActionMapping> ConvertToMappings(MappingConfig config)
    {
        var mappings = new List<ActionMapping>();
        var errors = new List<string>();

        try
        {
            _logger.LogDebug("Converting configuration to runtime mappings for profile '{ProfileName}'", config.ProfileName);

            foreach (var deviceConfig in config.MidiDevices)
            {
                foreach (var mappingConfig in deviceConfig.Mappings)
                {
                    try
                    {
                        // Convert the mapping configuration to a runtime mapping
                        var mapping = ConvertMappingConfigToMapping(mappingConfig, deviceConfig);
                        if (mapping != null)
                        {
                            mappings.Add(mapping);
                            _logger.LogTrace("Converted mapping: {Description}", mapping.Description ?? "No description");
                        }
                    }
                    catch (Exception ex)
                    {
                        var errorMsg = $"Failed to convert mapping '{mappingConfig.Description ?? mappingConfig.Id ?? "Unknown"}': {ex.Message}";
                        errors.Add(errorMsg);
                        _logger.LogWarning(ex, errorMsg);
                    }
                }
            }

            if (errors.Count > 0)
            {
                var errorMessage = string.Join("\n", errors);
                _logger.LogWarning("Some mappings failed to convert:\n{Errors}", errorMessage);
            }

            _logger.LogInformation("Successfully converted {MappingCount} mappings from configuration", mappings.Count);
            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert configuration to mappings: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Saves a action configuration to a JSON file.
    /// Performs strongly-typed serialization with validation.
    /// </summary>
    /// <param name="config">The configuration to save</param>
    /// <param name="filePath">The path to save the configuration file</param>
    /// <returns>True if saving succeeded, false otherwise</returns>
    public bool SaveConfiguration(MappingConfig config, string filePath)
    {
        try
        {
            _logger.LogDebug("Saving action configuration to {FilePath}", filePath);

            // Validate the configuration before saving
            if (!config.IsValid())
            {
                var errors = config.GetValidationErrors();
                var errorMessage = string.Join("; ", errors);
                _logger.LogError("Configuration validation failed before saving to {FilePath}: {Errors}", filePath, errorMessage);
                return false;
            }

            // Use ConfigurationFileManager for file operations
            var success = _fileManager.WriteActionConfig(config, filePath, "action configuration");

            if (success)
            {
                _logger.LogInformation("Successfully saved action configuration to {FilePath} with {DeviceCount} devices",
                    filePath, config.MidiDevices.Count);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving configuration to {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Converts runtime mappings back to a configuration structure.
    /// Enables round-trip conversion from registry to configuration.
    /// </summary>
    /// <param name="mappings">The runtime mappings to convert</param>
    /// <param name="profileName">The name for the profile</param>
    /// <param name="description">Optional description for the profile</param>
    /// <returns>The configuration structure</returns>
    public MappingConfig ConvertFromMappings(IEnumerable<ActionMapping> mappings, string profileName, string? description = null)
    {
        try
        {
            _logger.LogDebug("Converting runtime mappings to configuration for profile '{ProfileName}'", profileName);

            var config = new MappingConfig
            {
                ProfileName = profileName,
                Description = description
            };

            // Group mappings by device name
            var deviceGroups = mappings.GroupBy(m => m.Input.DeviceName ?? "*");

            foreach (var deviceGroup in deviceGroups)
            {
                var deviceConfig = new DeviceConfig
                {
                    DeviceName = deviceGroup.Key,
                    InputProfile = $"{profileName}-{deviceGroup.Key}",
                    Mappings = new List<MappingConfigEntry>()
                };

                foreach (var mapping in deviceGroup)
                {
                    try
                    {
                        var mappingConfig = ConvertMappingToConfig(mapping);
                        deviceConfig.Mappings.Add(mappingConfig);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to convert mapping '{Description}' to configuration: {ErrorMessage}",
                            mapping.Description ?? "No description", ex.Message);
                    }
                }

                if (deviceConfig.Mappings.Count > 0)
                {
                    config.MidiDevices.Add(deviceConfig);
                }
            }

            _logger.LogInformation("Successfully converted {MappingCount} mappings to configuration for profile '{ProfileName}'",
                mappings.Count(), profileName);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert mappings to configuration: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Loads mappings into the registry with atomic update.
    /// Integrates with the existing registry pattern for thread-safe updates.
    /// </summary>
    /// <param name="registry">The registry to load mappings into</param>
    /// <param name="config">The configuration to load</param>
    /// <returns>True if loading succeeded, false otherwise</returns>
    public bool LoadMappingsIntoRegistry(ActionMappingRegistry registry, MappingConfig config)
    {
        try
        {
            _logger.LogDebug("Loading mappings into registry for profile '{ProfileName}'", config.ProfileName);

            // Convert configuration to runtime mappings
            var mappings = ConvertToMappings(config);

            // Use atomic registry update
            registry.LoadMappings(mappings);

            _logger.LogInformation("Successfully loaded {MappingCount} mappings into registry", mappings.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load mappings into registry: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Converts a single mapping configuration entry to a runtime mapping.
    /// Pre-compiles the action and validates all parameters.
    /// </summary>
    /// <param name="mappingConfig">The mapping configuration to convert</param>
    /// <param name="deviceConfig">The device configuration context</param>
    /// <returns>The runtime mapping, or null if conversion failed</returns>
    private ActionMapping? ConvertMappingConfigToMapping(MappingConfigEntry mappingConfig, DeviceConfig deviceConfig)
    {
        try
        {
            // Create the action from configuration (pre-compile and validate)
            var action = _actionFactory.CreateAction(mappingConfig.Action);

            // Convert input configuration to runtime input
            var input = ConvertToMidiInput(mappingConfig, deviceConfig);

            // Create the runtime mapping
            var mapping = new ActionMapping
            {
                Input = input,
                Action = action,
                Description = mappingConfig.Description,
                IsEnabled = mappingConfig.IsEnabled
            };

            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert mapping configuration: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converts mapping configuration to ActionMidiInput.
    /// Handles input type parsing and validation.
    /// </summary>
    /// <param name="mappingConfig">The mapping configuration</param>
    /// <param name="deviceConfig">The device configuration context</param>
    /// <returns>The converted MIDI input specification</returns>
    private ActionMidiInput ConvertToMidiInput(MappingConfigEntry mappingConfig, DeviceConfig deviceConfig)
    {
        // Parse input type
        if (!Enum.TryParse<ActionMidiInputType>(mappingConfig.InputType, true, out var inputType))
        {
            throw new ArgumentException($"Invalid input type: {mappingConfig.InputType}");
        }

        // Determine input number based on type (SysEx uses 0 as it doesn't have a meaningful input number)
        int inputNumber = inputType switch
        {
            ActionMidiInputType.NoteOn or ActionMidiInputType.NoteOff =>
                mappingConfig.Note ?? throw new ArgumentException("Note number is required for NoteOn/NoteOff"),
            ActionMidiInputType.ControlChange =>
                mappingConfig.ControlNumber ?? throw new ArgumentException("Controller number is required for ControlChange"),
            ActionMidiInputType.SysEx => 0, // SysEx doesn't use input number, pattern matching is used instead
            _ => throw new ArgumentException($"Unsupported input type: {inputType}")
        };

        // Parse SysEx pattern if needed
        byte[]? sysExPattern = null;
        if (inputType == ActionMidiInputType.SysEx)
        {
            if (string.IsNullOrWhiteSpace(mappingConfig.SysExPattern))
            {
                throw new ArgumentException("SysEx pattern is required for SysEx input type");
            }
            sysExPattern = ParseSysExPattern(mappingConfig.SysExPattern);
        }

        return new ActionMidiInput
        {
            InputType = inputType,
            InputNumber = inputNumber,
            Channel = mappingConfig.Channel,
            DeviceName = deviceConfig.DeviceName,
            SysExPattern = sysExPattern
        };
    }

    /// <summary>
    /// Converts a runtime mapping back to a configuration entry.
    /// Enables round-trip conversion for saving configurations.
    /// </summary>
    /// <param name="mapping">The runtime mapping to convert</param>
    /// <returns>The configuration entry</returns>
    private MappingConfigEntry ConvertMappingToConfig(ActionMapping mapping)
    {
        try
        {
            // Convert the action back to configuration
            var actionConfig = ConvertActionToConfig(mapping.Action);

            // Create the mapping configuration entry
            var mappingConfig = new MappingConfigEntry
            {
                Id = mapping.Action.Id,
                Description = mapping.Description,
                IsEnabled = mapping.IsEnabled,
                InputType = mapping.Input.InputType.ToString(),

                Channel = mapping.Input.Channel,
                Action = actionConfig
            };

            // Set the appropriate input number field based on input type
            switch (mapping.Input.InputType)
            {
                case ActionMidiInputType.NoteOn:
                case ActionMidiInputType.NoteOff:
                    mappingConfig.Note = mapping.Input.InputNumber;
                    break;
                case ActionMidiInputType.ControlChange:
                    mappingConfig.ControlNumber = mapping.Input.InputNumber;
                    break;
                case ActionMidiInputType.SysEx:
                    mappingConfig.SysExPattern = FormatSysExPattern(mapping.Input.SysExPattern);
                    break;
                default:
                    throw new ArgumentException($"Unsupported input type for conversion: {mapping.Input.InputType}");
            }

            return mappingConfig;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert mapping to configuration: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converts a runtime action back to its configuration representation.
    /// Uses reflection to create the appropriate configuration type.
    /// </summary>
    /// <param name="action">The runtime action to convert</param>
    /// <returns>The action configuration</returns>
    private ActionConfig ConvertActionToConfig(IAction action)
    {
        try
        {
            // Map action types to their configuration types
            var actionType = action.GetType();
            var configType = actionType.Name switch
            {
                "KeyPressReleaseAction" => typeof(KeyPressReleaseConfig),
                "KeyDownAction" => typeof(KeyDownConfig),
                "KeyUpAction" => typeof(KeyUpConfig),
                "KeyToggleAction" => typeof(KeyToggleConfig),
                "MouseClickAction" => typeof(MouseClickConfig),
                "MouseScrollAction" => typeof(MouseScrollConfig),
                "CommandExecutionAction" => typeof(CommandExecutionConfig),
                "DelayAction" => typeof(DelayConfig),
                "GameControllerButtonAction" => typeof(GameControllerButtonConfig),
                "GameControllerAxisAction" => typeof(GameControllerAxisConfig),
                "SequenceAction" => typeof(SequenceConfig),
                "ConditionalAction" => typeof(ConditionalConfig),
                "MidiOutputAction" => typeof(MidiOutputConfig),
                _ => throw new ArgumentException($"Unknown action type for conversion: {actionType.Name}")
            };

            // Create an instance of the configuration type
            var config = Activator.CreateInstance(configType) as ActionConfig;
            if (config == null)
            {
                throw new InvalidOperationException($"Failed to create configuration instance for type {configType.Name}");
            }

            // Copy properties from action to configuration
            // This is a simplified approach - in a real implementation, each action type
            // would need specific conversion logic
            CopyActionPropertiesToConfig(action, config);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert action to configuration: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Copies properties from a runtime action to its configuration.
    /// This is a simplified implementation that copies common properties.
    /// </summary>
    /// <param name="action">The source action</param>
    /// <param name="config">The target configuration</param>
    private void CopyActionPropertiesToConfig(IAction action, ActionConfig config)
    {
        // Copy common properties
        if (config.GetType().GetProperty("Description") != null)
        {
            config.GetType().GetProperty("Description")?.SetValue(config, action.Description);
        }

        // For now, we'll implement a basic property copying mechanism
        // In a full implementation, each action type would have specific conversion logic
        var actionProperties = action.GetType().GetProperties();
        var configProperties = config.GetType().GetProperties();

        foreach (var actionProp in actionProperties)
        {
            var configProp = configProperties.FirstOrDefault(p => p.Name == actionProp.Name && p.CanWrite);
            if (configProp != null && actionProp.CanRead)
            {
                try
                {
                    var value = actionProp.GetValue(action);
                    configProp.SetValue(config, value);
                }
                catch (Exception ex)
                {
                    _logger.LogTrace(ex, "Failed to copy property {PropertyName} from action to config", actionProp.Name);
                    // Continue with other properties
                }
            }
        }
    }

    /// <summary>
    /// Parses a hex string SysEx pattern into a byte array
    /// </summary>
    /// <param name="hexPattern">Hex string pattern (e.g., "F0 43 12 00 F7" or "F0431200F7")</param>
    /// <returns>Byte array representation of the pattern</returns>
    private byte[] ParseSysExPattern(string hexPattern)
    {
        try
        {
            // Remove spaces and normalize
            var cleanPattern = hexPattern.Replace(" ", "").Replace("-", "").ToUpperInvariant();

            // Validate length (must be even number of hex characters)
            if (cleanPattern.Length % 2 != 0)
            {
                throw new ArgumentException($"Invalid SysEx pattern length: {hexPattern}");
            }

            // Convert hex string to byte array
            var bytes = new byte[cleanPattern.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var hexByte = cleanPattern.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(hexByte, 16);
            }

            // Validate SysEx structure using the pattern matcher
            var patternMatcher = new Midi.SysExPatternMatcher();
            if (!patternMatcher.IsValidSysExPattern(bytes))
            {
                throw new ArgumentException($"Invalid SysEx pattern structure: {hexPattern}");
            }

            return bytes;
        }
        catch (Exception ex) when (!(ex is ArgumentException))
        {
            _logger.LogError(ex, "Error parsing SysEx pattern: {Pattern}", hexPattern);
            throw new ArgumentException($"Failed to parse SysEx pattern '{hexPattern}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Formats a byte array SysEx pattern as a hex string
    /// </summary>
    /// <param name="sysExPattern">Byte array pattern</param>
    /// <returns>Hex string representation (e.g., "F0 43 12 00 F7")</returns>
    private string FormatSysExPattern(byte[]? sysExPattern)
    {
        if (sysExPattern == null || sysExPattern.Length == 0)
            return "";

        return string.Join(" ", sysExPattern.Select(b => b.ToString("X2")));
    }
}

/// <summary>
/// JSON converter for polymorphic ActionConfig deserialization.
/// Handles type-safe conversion from JSON to strongly-typed action configurations.
/// </summary>
public class ActionConfigJsonConverter : JsonConverter<ActionConfig>
{
    /// <summary>
    /// Reads ActionConfig from JSON with polymorphic type resolution
    /// </summary>
    public override ActionConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the JSON object into a JsonDocument for inspection
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // Get the $type property to determine the concrete type
        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type property in action configuration");
        }

        var typeName = typeProperty.GetString();
        if (string.IsNullOrEmpty(typeName))
        {
            throw new JsonException("Empty $type property in action configuration");
        }

        // Map type names to concrete types
        var configType = typeName switch
        {
            "KeyPressReleaseConfig" => typeof(KeyPressReleaseConfig),
            "KeyDownConfig" => typeof(KeyDownConfig),
            "KeyUpConfig" => typeof(KeyUpConfig),
            "KeyToggleConfig" => typeof(KeyToggleConfig),
            "MouseClickConfig" => typeof(MouseClickConfig),
            "MouseScrollConfig" => typeof(MouseScrollConfig),
            "CommandExecutionConfig" => typeof(CommandExecutionConfig),
            "DelayConfig" => typeof(DelayConfig),
            "GameControllerButtonConfig" => typeof(GameControllerButtonConfig),
            "GameControllerAxisConfig" => typeof(GameControllerAxisConfig),
            "SequenceConfig" => typeof(SequenceConfig),
            "ConditionalConfig" => typeof(ConditionalConfig),
            "MidiOutputConfig" => typeof(MidiOutputConfig),
            _ => throw new JsonException($"Unknown action configuration type: {typeName}")
        };

        // Deserialize to the concrete type
        var rawText = root.GetRawText();
        var result = JsonSerializer.Deserialize(rawText, configType, options) as ActionConfig;

        if (result == null)
        {
            throw new JsonException($"Failed to deserialize action configuration of type {typeName}");
        }

        return result;
    }

    /// <summary>
    /// Writes ActionConfig to JSON with type information
    /// </summary>
    public override void Write(Utf8JsonWriter writer, ActionConfig value, JsonSerializerOptions options)
    {
        // Get the concrete type name
        var typeName = value.GetType().Name;

        writer.WriteStartObject();

        // Write the $type property first
        writer.WriteString("$type", typeName);

        // Serialize the rest of the object
        var json = JsonSerializer.Serialize(value, value.GetType(), options);
        using var doc = JsonDocument.Parse(json);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Name != "$type") // Skip $type as we already wrote it
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }


}
