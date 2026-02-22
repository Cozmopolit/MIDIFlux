using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Configuration;

/// <summary>
/// Represents the top-level configuration for a action profile.
/// Contains all mappings and metadata for a complete MIDI device configuration.
/// </summary>
public class MappingConfig
{
    /// <summary>
    /// The name of this profile
    /// </summary>
    public string ProfileName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description of this profile
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// List of MIDI device configurations
    /// </summary>
    public List<DeviceConfig> MidiDevices { get; set; } = new();

    /// <summary>
    /// Initial states for stateful actions (user-defined states only)
    /// </summary>
    public Dictionary<string, int>? InitialStates { get; set; }

    /// <summary>
    /// Validates the entire profile configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(ProfileName))
            return false;

        // Empty profiles (no MIDI devices) are valid
        return MidiDevices.All(device => device.IsValid());
    }

    /// <summary>
    /// Gets validation error messages for this profile
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(ProfileName))
            errors.Add("Profile name is required");

        // Empty profiles (no MIDI devices) are valid - no error needed

        foreach (var device in MidiDevices)
        {
            var deviceErrors = device.GetValidationErrors();
            errors.AddRange(deviceErrors.Select(e => $"Device '{device.DeviceName}': {e}"));
        }

        return errors;
    }
}

/// <summary>
/// Represents the configuration for a specific MIDI device in the action system.
/// </summary>
public class DeviceConfig
{
    /// <summary>
    /// The name of the MIDI device (or "*" for wildcard)
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// Optional description for this device configuration
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// The list of action mappings for this device
    /// </summary>
    public List<MappingConfigEntry> Mappings { get; set; } = new();

    /// <summary>
    /// Validates the device configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        // Device name can be empty (means "any device") or a specific name
        // Empty device names are automatically converted to "*" during processing
        return Mappings.All(mapping => mapping.IsValid());
    }

    /// <summary>
    /// Gets validation error messages for this device configuration
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Device name can be empty (means "any device"), so no validation needed

        for (int i = 0; i < Mappings.Count; i++)
        {
            var mapping = Mappings[i];
            var mappingErrors = mapping.GetValidationErrors();
            errors.AddRange(mappingErrors.Select(e => $"Mapping {i + 1}: {e}"));
        }

        return errors;
    }
}

/// <summary>
/// Represents a single mapping configuration entry in JSON format.
/// This is the serializable version that gets converted to ActionMapping.
/// </summary>
public class MappingConfigEntry
{
    /// <summary>
    /// Optional human-readable description of this mapping
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this mapping is currently enabled
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// MIDI input type (NoteOn, NoteOff, ControlChange, etc.)
    /// </summary>
    public string InputType { get; set; } = string.Empty;

    /// <summary>
    /// MIDI note number (for NoteOn/NoteOff events)
    /// </summary>
    public int? Note { get; set; }

    /// <summary>
    /// MIDI control number (for ControlChange events)
    /// </summary>
    public int? ControlNumber { get; set; }

    /// <summary>
    /// MIDI channel (1-16, null for any channel)
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// The action to execute (unified ActionBase system)
    /// </summary>
    public ActionBase Action { get; set; } = null!;

    /// <summary>
    /// The SysEx pattern to match (for SysEx input type only)
    /// Hex string representation of the SysEx bytes (e.g., "F0 43 12 00 F7")
    /// </summary>
    public string? SysExPattern { get; set; }

    /// <summary>
    /// Validates the mapping configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(InputType))
            return false;

        if (Action == null)
            return false;

        // Validate that the appropriate input number is provided based on input type
        if (InputType.Equals("NoteOn", StringComparison.OrdinalIgnoreCase) ||
            InputType.Equals("NoteOff", StringComparison.OrdinalIgnoreCase))
        {
            if (!Note.HasValue)
                return false;
        }
        else if (InputType.Equals("ControlChange", StringComparison.OrdinalIgnoreCase))
        {
            if (!ControlNumber.HasValue)
                return false;
        }
        else if (InputType.Equals("SysEx", StringComparison.OrdinalIgnoreCase))
        {
            if (string.IsNullOrWhiteSpace(SysExPattern))
                return false;
        }

        return Action.IsValid();
    }

    /// <summary>
    /// Gets validation error messages for this mapping configuration
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(InputType))
            errors.Add("Input type is required");

        if (Action == null)
        {
            errors.Add("Action configuration is required");
            return errors; // Can't validate action if it's null
        }

        // Validate input number based on type
        if (InputType.Equals("NoteOn", StringComparison.OrdinalIgnoreCase) ||
            InputType.Equals("NoteOff", StringComparison.OrdinalIgnoreCase))
        {
            if (!Note.HasValue)
                errors.Add("Note number is required for NoteOn/NoteOff input types");
        }
        else if (InputType.Equals("ControlChange", StringComparison.OrdinalIgnoreCase))
        {
            if (!ControlNumber.HasValue)
                errors.Add("Controller number is required for ControlChange input type");
        }

        // Validate action configuration
        var actionErrors = Action.GetValidationErrors();
        errors.AddRange(actionErrors.Select(e => $"Action: {e}"));

        return errors;
    }
}

/// <summary>
/// Loads action configurations from JSON files with strongly-typed deserialization.
/// Provides type-safe configuration loading, validation, and conversion to runtime objects.
/// Uses ConfigurationFileManager for all file operations to eliminate duplication.
/// </summary>
public class ActionConfigurationLoader
{
    private readonly ILogger _logger;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ActionConfigurationLoader
    /// </summary>
    /// <param name="logger">The logger to use for logging</param>
    /// <param name="configurationService">The configuration service for file operations</param>
    public ActionConfigurationLoader(ILogger logger, ConfigurationService configurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Loads a action configuration from a JSON file.
    /// Performs strongly-typed deserialization and validation.
    /// </summary>
    /// <param name="filePath">The path to the configuration file</param>
    /// <returns>The loaded configuration</returns>
    /// <exception cref="InvalidOperationException">Thrown when loading or validation fails</exception>
    public MappingConfig? LoadConfiguration(string filePath)
    {
        try
        {
            _logger.LogDebug("Loading action configuration from {FilePath}", filePath);

            // Use ConfigurationService for file operations
            var config = _configurationService.LoadProfileConfiguration(filePath);

            if (config == null)
            {
                throw new InvalidOperationException($"Failed to load configuration from '{filePath}': Configuration service returned null");
            }

            // Validate the configuration structure
            if (!config.IsValid())
            {
                var errors = config.GetValidationErrors();
                var errorMessage = string.Join("; ", errors);
                _logger.LogError("Configuration validation failed for {FilePath}: {Errors}", filePath, errorMessage);
                throw new InvalidOperationException($"Configuration validation failed for '{filePath}': {errorMessage}");
            }

            _logger.LogInformation("Successfully loaded action configuration from {FilePath} with {DeviceCount} devices",
                filePath, config.MidiDevices.Count);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading configuration from {FilePath}: {ErrorMessage}", filePath, ex.Message);
            throw;
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
                        var errorMsg = $"Failed to convert mapping '{mappingConfig.Description ?? "Unknown"}': {ex.Message}";
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

            // Initialize actions that need runtime data (e.g., PlaySoundAction audio pre-loading)
            InitializeActionsForRuntime(mappings);

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

            // Use ConfigurationService for file operations
            var success = _configurationService.SaveProfileConfiguration(config, filePath);

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
    /// Initializes actions that require runtime data preparation (e.g., audio pre-loading)
    /// </summary>
    /// <param name="mappings">The mappings containing actions to initialize</param>
    private void InitializeActionsForRuntime(List<ActionMapping> mappings)
    {
        _logger.LogDebug("Initializing actions for runtime execution");

        foreach (var mapping in mappings)
        {
            if (mapping.Action is ActionBase actionBase)
            {
                InitializeActionRecursively(actionBase);
            }
        }

        _logger.LogDebug("Completed action initialization for runtime");
    }

    /// <summary>
    /// Recursively initializes an action and any sub-actions
    /// </summary>
    /// <param name="action">The action to initialize</param>
    private void InitializeActionRecursively(ActionBase action)
    {
        try
        {
            // Initialize PlaySoundActions by calling EnsureAudioDataLoaded
            if (action is Actions.Simple.PlaySoundAction playSoundAction)
            {
                // Use reflection to call the private EnsureAudioDataLoaded method
                var method = typeof(Actions.Simple.PlaySoundAction).GetMethod("EnsureAudioDataLoaded",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                method?.Invoke(playSoundAction, null);

                _logger.LogDebug("Initialized PlaySoundAction: {Description}", action.Description);
            }

            // Initialize any sub-actions recursively
            foreach (var parameter in action.GetParameterList())
            {
                if (parameter.Type == ParameterType.SubActionList && parameter.Value is List<ActionBase> subActions)
                {
                    foreach (var subAction in subActions)
                    {
                        InitializeActionRecursively(subAction);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize action {ActionType}: {ErrorMessage}",
                action.GetType().Name, ex.Message);
            throw;
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
            // Action is already an ActionBase instance from JSON deserialization
            var action = mappingConfig.Action;

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
    /// Converts mapping configuration to MidiInput.
    /// Handles input type parsing and validation.
    /// </summary>
    /// <param name="mappingConfig">The mapping configuration</param>
    /// <param name="deviceConfig">The device configuration context</param>
    /// <returns>The converted MIDI input specification</returns>
    private MidiInput ConvertToMidiInput(MappingConfigEntry mappingConfig, DeviceConfig deviceConfig)
    {
        // Parse input type with backward compatibility for old "ControlChange" value
        if (!Enum.TryParse<MidiInputType>(mappingConfig.InputType, true, out var inputType))
        {
            // Handle backward compatibility for old "ControlChange" enum value
            if (string.Equals(mappingConfig.InputType, "ControlChange", StringComparison.OrdinalIgnoreCase))
            {
                // Default to ControlChangeAbsolute for backward compatibility
                // Most existing profiles use faders/knobs which are absolute controllers
                inputType = MidiInputType.ControlChangeAbsolute;
                _logger.LogInformation("Converting legacy 'ControlChange' input type to 'ControlChangeAbsolute' for mapping: {Description}",
                    mappingConfig.Description);
            }
            else
            {
                throw new ArgumentException($"Invalid input type: {mappingConfig.InputType}");
            }
        }

        // Determine input number based on type (SysEx uses 0 as it doesn't have a meaningful input number)
        int inputNumber = inputType switch
        {
            MidiInputType.NoteOn or MidiInputType.NoteOff =>
                mappingConfig.Note ?? throw new ArgumentException("Note number is required for NoteOn/NoteOff"),
            MidiInputType.ControlChangeAbsolute or MidiInputType.ControlChangeRelative =>
                mappingConfig.ControlNumber ?? throw new ArgumentException("Controller number is required for ControlChange"),
            MidiInputType.SysEx => 0, // SysEx doesn't use input number, pattern matching is used instead
            _ => throw new ArgumentException($"Unsupported input type: {inputType}")
        };

        // Parse SysEx pattern if needed
        byte[]? sysExPattern = null;
        if (inputType == MidiInputType.SysEx)
        {
            if (string.IsNullOrWhiteSpace(mappingConfig.SysExPattern))
            {
                throw new ArgumentException("SysEx pattern is required for SysEx input type");
            }
            sysExPattern = ParseSysExPattern(mappingConfig.SysExPattern);
        }

        return new MidiInput
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
            // Action should be an ActionBase instance in the new system
            if (mapping.Action is not ActionBase actionBase)
            {
                throw new InvalidOperationException($"Action {mapping.Action.GetType().Name} is not an ActionBase instance");
            }

            // Create the mapping configuration entry
            var mappingConfig = new MappingConfigEntry
            {
                Description = mapping.Description,
                IsEnabled = mapping.IsEnabled,
                InputType = mapping.Input.InputType.ToString(),

                Channel = mapping.Input.Channel,
                Action = actionBase
            };

            // Set the appropriate input number field based on input type
            switch (mapping.Input.InputType)
            {
                case MidiInputType.NoteOn:
                case MidiInputType.NoteOff:
                    mappingConfig.Note = mapping.Input.InputNumber;
                    break;
                case MidiInputType.ControlChangeAbsolute:
                case MidiInputType.ControlChangeRelative:
                    mappingConfig.ControlNumber = mapping.Input.InputNumber;
                    break;
                case MidiInputType.SysEx:
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
    /// Parses a hex string SysEx pattern into a byte array
    /// </summary>
    /// <param name="hexPattern">Hex string pattern (e.g., "F0 43 12 00 F7" or "F0431200F7")</param>
    /// <returns>Byte array representation of the pattern</returns>
    private byte[] ParseSysExPattern(string hexPattern)
    {
        try
        {
            // Use HexByteConverter for consistent hex parsing
            var bytes = HexByteConverter.ParseHexString(hexPattern);

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

        // Use HexByteConverter for consistent hex formatting
        return HexByteConverter.FormatByteArray(sysExPattern);
    }

    /// <summary>
    /// Converts current registry mappings back to a MappingConfig structure.
    /// Enables extraction of runtime configuration for saving or inspection.
    /// </summary>
    /// <param name="registry">The registry to extract mappings from</param>
    /// <param name="profileName">The name for the extracted profile</param>
    /// <param name="description">Optional description for the extracted profile</param>
    /// <returns>MappingConfig representing current registry state</returns>
    public MappingConfig ConvertFromRegistry(ActionMappingRegistry registry, string profileName, string? description = null)
    {
        if (registry == null)
            throw new ArgumentNullException(nameof(registry));

        if (string.IsNullOrWhiteSpace(profileName))
            throw new ArgumentException("Profile name cannot be null or empty", nameof(profileName));

        try
        {
            _logger.LogDebug("Converting registry mappings to configuration for profile '{ProfileName}'", profileName);

            // Get all mappings from registry
            var allMappings = registry.GetAllMappings();

            // Use existing ConvertFromMappings method
            var config = ConvertFromMappings(allMappings, profileName, description);

            _logger.LogInformation("Successfully converted {MappingCount} registry mappings to configuration '{ProfileName}'",
                allMappings.Count(), profileName);

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert registry to configuration: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Converts a single MappingConfigEntry to an ActionMapping for registry operations.
    /// Enables individual mapping operations without full configuration conversion.
    /// </summary>
    /// <param name="mappingConfig">The mapping configuration to convert</param>
    /// <param name="deviceName">The device name for the mapping</param>
    /// <returns>ActionMapping ready for registry operations, or null if conversion failed</returns>
    public ActionMapping? ConvertSingleMapping(MappingConfigEntry mappingConfig, string deviceName)
    {
        if (mappingConfig == null)
        {
            _logger.LogWarning("Cannot convert null mapping configuration");
            return null;
        }

        if (string.IsNullOrWhiteSpace(deviceName))
        {
            _logger.LogWarning("Cannot convert mapping with null or empty device name");
            return null;
        }

        try
        {
            _logger.LogDebug("Converting single mapping configuration for device '{DeviceName}': {Description}",
                deviceName, mappingConfig.Description ?? "No description");

            // Create a temporary device config for the conversion
            var tempDeviceConfig = new DeviceConfig
            {
                DeviceName = deviceName,
                Mappings = new List<MappingConfigEntry> { mappingConfig }
            };

            // Use existing conversion logic
            var mapping = ConvertMappingConfigToMapping(mappingConfig, tempDeviceConfig);

            if (mapping != null)
            {
                _logger.LogDebug("Successfully converted single mapping: {Description}",
                    mapping.Description ?? mapping.Action.Description);
            }
            else
            {
                _logger.LogWarning("Failed to convert single mapping configuration");
            }

            return mapping;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert single mapping configuration: {ErrorMessage}", ex.Message);
            return null;
        }
    }
}


