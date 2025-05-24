namespace MIDIFlux.Core.Actions;

/// <summary>
/// Represents MIDI input specification for unified action mappings.
/// Defines WHEN an action should be triggered based on MIDI events.
/// </summary>
public class UnifiedActionMidiInput
{
    /// <summary>
    /// The type of MIDI input (NoteOn, NoteOff, ControlChange, etc.)
    /// </summary>
    public UnifiedActionMidiInputType InputType { get; set; }

    /// <summary>
    /// The MIDI input number (note number for notes, CC number for controls, etc.)
    /// </summary>
    public int InputNumber { get; set; }

    /// <summary>
    /// The MIDI channel (1-16, null for all channels)
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// The MIDI device name (null for all devices, "*" for wildcard)
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// Generates a lookup key for O(1) performance in mapping registries.
    /// Format: "DeviceName|Channel|InputNumber|InputType"
    /// Uses "*" for wildcards to enable efficient dictionary lookups.
    /// </summary>
    /// <returns>A string key optimized for dictionary lookup performance</returns>
    public string GetLookupKey()
    {
        return $"{DeviceName ?? "*"}|{Channel?.ToString() ?? "*"}|{InputNumber}|{InputType}";
    }

    /// <summary>
    /// Creates a copy of this MIDI input specification
    /// </summary>
    /// <returns>A new instance with the same values</returns>
    public UnifiedActionMidiInput Clone()
    {
        return new UnifiedActionMidiInput
        {
            InputType = InputType,
            InputNumber = InputNumber,
            Channel = Channel,
            DeviceName = DeviceName
        };
    }

    /// <summary>
    /// Returns a human-readable string representation of this MIDI input
    /// </summary>
    public override string ToString()
    {
        var device = string.IsNullOrEmpty(DeviceName) ? "Any Device" : DeviceName;
        var channel = Channel?.ToString() ?? "Any Channel";
        return $"{device} - Ch:{channel} - {InputType}:{InputNumber}";
    }
}
