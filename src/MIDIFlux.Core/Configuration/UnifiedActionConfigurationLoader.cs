using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Configuration;

/// <summary>
/// Loads unified action configurations from JSON files with strongly-typed deserialization.
/// Provides type-safe configuration loading, validation, and conversion to runtime objects.
/// Integrates with existing DeviceConfigurationManager patterns and error handling.
/// </summary>
public class UnifiedActionConfigurationLoader
{
    private readonly ILogger _logger;
    private readonly IUnifiedActionFactory _actionFactory;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the UnifiedActionConfigurationLoader
    /// </summary>
    /// <param name="logger">The logger to use for logging</param>
    /// <param name="actionFactory">The factory for creating actions from configurations</param>
    public UnifiedActionConfigurationLoader(ILogger logger, IUnifiedActionFactory actionFactory)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));

        // Configure JSON serialization options for strongly-typed deserialization
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = true,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
                new UnifiedActionConfigJsonConverter()
            }
        };
    }

    /// <summary>
    /// Loads a unified action configuration from a JSON file.
    /// Performs strongly-typed deserialization and validation.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration, or null if loading failed</returns>
    public UnifiedMappingConfig? LoadConfiguration(string filePath)
    {
        try
        {
            _logger.LogDebug("Loading unified action configuration from {FilePath}", filePath);

            if (!File.Exists(filePath))
            {
                _logger.LogError("Configuration file not found: {FilePath}", filePath);
                return null;
            }

            // Read and deserialize the JSON file
            var json = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize<UnifiedMappingConfig>(json, _jsonOptions);

            if (config == null)
            {
                _logger.LogError("Failed to deserialize configuration from {FilePath}", filePath);
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

            _logger.LogInformation("Successfully loaded unified action configuration from {FilePath} with {DeviceCount} devices",
                filePath, config.MidiDevices.Count);

            return config;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error loading configuration from {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return null;
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
    public List<UnifiedActionMapping> ConvertToMappings(UnifiedMappingConfig config)
    {
        var mappings = new List<UnifiedActionMapping>();
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
    /// Saves a unified action configuration to a JSON file.
    /// Performs strongly-typed serialization with validation.
    /// </summary>
    /// <param name="config">The configuration to save</param>
    /// <param name="filePath">The path to save the configuration file</param>
    /// <returns>True if saving succeeded, false otherwise</returns>
    public bool SaveConfiguration(UnifiedMappingConfig config, string filePath)
    {
        try
        {
            _logger.LogDebug("Saving unified action configuration to {FilePath}", filePath);

            // Validate the configuration before saving
            if (!config.IsValid())
            {
                var errors = config.GetValidationErrors();
                var errorMessage = string.Join("; ", errors);
                _logger.LogError("Configuration validation failed before saving to {FilePath}: {Errors}", filePath, errorMessage);
                return false;
            }

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }

            // Serialize and write the JSON file
            var json = JsonSerializer.Serialize(config, _jsonOptions);
            File.WriteAllText(filePath, json);

            _logger.LogInformation("Successfully saved unified action configuration to {FilePath} with {DeviceCount} devices",
                filePath, config.MidiDevices.Count);

            return true;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON serialization error saving configuration to {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return false;
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
    public UnifiedMappingConfig ConvertFromMappings(IEnumerable<UnifiedActionMapping> mappings, string profileName, string? description = null)
    {
        try
        {
            _logger.LogDebug("Converting runtime mappings to configuration for profile '{ProfileName}'", profileName);

            var config = new UnifiedMappingConfig
            {
                ProfileName = profileName,
                Description = description
            };

            // Group mappings by device name
            var deviceGroups = mappings.GroupBy(m => m.Input.DeviceName ?? "*");

            foreach (var deviceGroup in deviceGroups)
            {
                var deviceConfig = new UnifiedDeviceConfig
                {
                    DeviceName = deviceGroup.Key,
                    InputProfile = $"{profileName}-{deviceGroup.Key}",
                    Mappings = new List<UnifiedMappingConfigEntry>()
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
    public bool LoadMappingsIntoRegistry(UnifiedActionMappingRegistry registry, UnifiedMappingConfig config)
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
    private UnifiedActionMapping? ConvertMappingConfigToMapping(UnifiedMappingConfigEntry mappingConfig, UnifiedDeviceConfig deviceConfig)
    {
        try
        {
            // Create the action from configuration (pre-compile and validate)
            var action = _actionFactory.CreateAction(mappingConfig.Action);

            // Convert input configuration to runtime input
            var input = ConvertToMidiInput(mappingConfig, deviceConfig);

            // Create the runtime mapping
            var mapping = new UnifiedActionMapping
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
    /// Converts mapping configuration to UnifiedActionMidiInput.
    /// Handles input type parsing and validation.
    /// </summary>
    /// <param name="mappingConfig">The mapping configuration</param>
    /// <param name="deviceConfig">The device configuration context</param>
    /// <returns>The converted MIDI input specification</returns>
    private UnifiedActionMidiInput ConvertToMidiInput(UnifiedMappingConfigEntry mappingConfig, UnifiedDeviceConfig deviceConfig)
    {
        // Parse input type
        if (!Enum.TryParse<UnifiedActionMidiInputType>(mappingConfig.InputType, true, out var inputType))
        {
            throw new ArgumentException($"Invalid input type: {mappingConfig.InputType}");
        }

        // Determine input number based on type
        int inputNumber = inputType switch
        {
            UnifiedActionMidiInputType.NoteOn or UnifiedActionMidiInputType.NoteOff =>
                mappingConfig.Note ?? throw new ArgumentException("Note number is required for NoteOn/NoteOff"),
            UnifiedActionMidiInputType.ControlChange =>
                mappingConfig.ControlNumber ?? throw new ArgumentException("Controller number is required for ControlChange"),
            _ => throw new ArgumentException($"Unsupported input type: {inputType}")
        };

        return new UnifiedActionMidiInput
        {
            InputType = inputType,
            InputNumber = inputNumber,
            Channel = mappingConfig.Channel,
            DeviceName = deviceConfig.DeviceName
        };
    }

    /// <summary>
    /// Converts a runtime mapping back to a configuration entry.
    /// Enables round-trip conversion for saving configurations.
    /// </summary>
    /// <param name="mapping">The runtime mapping to convert</param>
    /// <returns>The configuration entry</returns>
    private UnifiedMappingConfigEntry ConvertMappingToConfig(UnifiedActionMapping mapping)
    {
        try
        {
            // Convert the action back to configuration
            var actionConfig = ConvertActionToConfig(mapping.Action);

            // Create the mapping configuration entry
            var mappingConfig = new UnifiedMappingConfigEntry
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
                case UnifiedActionMidiInputType.NoteOn:
                case UnifiedActionMidiInputType.NoteOff:
                    mappingConfig.Note = mapping.Input.InputNumber;
                    break;
                case UnifiedActionMidiInputType.ControlChange:
                    mappingConfig.ControlNumber = mapping.Input.InputNumber;
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
    private UnifiedActionConfig ConvertActionToConfig(IUnifiedAction action)
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
                _ => throw new ArgumentException($"Unknown action type for conversion: {actionType.Name}")
            };

            // Create an instance of the configuration type
            var config = Activator.CreateInstance(configType) as UnifiedActionConfig;
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
    private void CopyActionPropertiesToConfig(IUnifiedAction action, UnifiedActionConfig config)
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
}

/// <summary>
/// JSON converter for polymorphic UnifiedActionConfig deserialization.
/// Handles type-safe conversion from JSON to strongly-typed action configurations.
/// </summary>
public class UnifiedActionConfigJsonConverter : JsonConverter<UnifiedActionConfig>
{
    /// <summary>
    /// Reads UnifiedActionConfig from JSON with polymorphic type resolution
    /// </summary>
    public override UnifiedActionConfig? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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
            _ => throw new JsonException($"Unknown action configuration type: {typeName}")
        };

        // Deserialize to the concrete type
        var rawText = root.GetRawText();
        var result = JsonSerializer.Deserialize(rawText, configType, options) as UnifiedActionConfig;

        if (result == null)
        {
            throw new JsonException($"Failed to deserialize action configuration of type {typeName}");
        }

        return result;
    }

    /// <summary>
    /// Writes UnifiedActionConfig to JSON with type information
    /// </summary>
    public override void Write(Utf8JsonWriter writer, UnifiedActionConfig value, JsonSerializerOptions options)
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
