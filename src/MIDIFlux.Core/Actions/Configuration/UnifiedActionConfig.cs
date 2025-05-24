using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Represents the top-level configuration for a unified action profile.
/// Contains all mappings and metadata for a complete MIDI device configuration.
/// </summary>
public class UnifiedMappingConfig
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
    public List<UnifiedDeviceConfig> MidiDevices { get; set; } = new();

    /// <summary>
    /// Validates the entire profile configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(ProfileName))
            return false;

        if (MidiDevices.Count == 0)
            return false;

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

        if (MidiDevices.Count == 0)
            errors.Add("At least one MIDI device configuration is required");

        foreach (var device in MidiDevices)
        {
            var deviceErrors = device.GetValidationErrors();
            errors.AddRange(deviceErrors.Select(e => $"Device '{device.DeviceName}': {e}"));
        }

        return errors;
    }
}

/// <summary>
/// Represents the configuration for a specific MIDI device in the unified action system.
/// </summary>
public class UnifiedDeviceConfig
{
    /// <summary>
    /// A unique identifier or name for this input profile
    /// </summary>
    public string InputProfile { get; set; } = string.Empty;

    /// <summary>
    /// The name of the MIDI device (or "*" for wildcard)
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// The MIDI channels to listen to (null or empty means all channels)
    /// </summary>
    public List<int>? MidiChannels { get; set; }

    /// <summary>
    /// The list of unified action mappings for this device
    /// </summary>
    public List<UnifiedMappingConfigEntry> Mappings { get; set; } = new();

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
/// This is the serializable version that gets converted to UnifiedActionMapping.
/// </summary>
public class UnifiedMappingConfigEntry
{
    /// <summary>
    /// Optional unique identifier for this mapping
    /// </summary>
    public string? Id { get; set; }

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
    /// The action configuration to execute
    /// </summary>
    public UnifiedActionConfig Action { get; set; } = null!;

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
/// Base class for all unified action configurations.
/// Provides type-safe action configuration with compile-time validation.
/// Eliminates Dictionary&lt;string, object&gt; parameter bags for better performance and type safety.
/// </summary>
public abstract class UnifiedActionConfig
{
    /// <summary>
    /// The type of action this configuration represents
    /// </summary>
    public UnifiedActionType Type { get; set; }

    /// <summary>
    /// Optional human-readable description of this action
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Validates the configuration parameters.
    /// Derived classes should override this to implement action-specific validation.
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public virtual bool IsValid()
    {
        return true;
    }

    /// <summary>
    /// Gets validation error messages for this configuration.
    /// Derived classes should override this to provide specific error details.
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public virtual List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (!IsValid())
        {
            errors.Add($"Invalid configuration for action type {Type}");
        }

        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation of this configuration
    /// </summary>
    public override string ToString()
    {
        return string.IsNullOrEmpty(Description) ? $"{Type} Action" : Description;
    }
}
