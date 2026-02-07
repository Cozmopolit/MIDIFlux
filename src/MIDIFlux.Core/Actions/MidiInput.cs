namespace MIDIFlux.Core.Actions;

/// <summary>
/// Represents MIDI input specification for action mappings.
/// Defines WHEN an action should be triggered based on MIDI events.
/// </summary>
public class MidiInput
{
    /// <summary>
    /// The type of MIDI input (NoteOn, NoteOff, ControlChangeAbsolute, ControlChangeRelative, etc.)
    /// </summary>
    public MidiInputType InputType { get; set; }

    /// <summary>
    /// The MIDI input number (note number for notes, CC number for controls, etc.)
    /// </summary>
    public int InputNumber { get; set; }

    /// <summary>
    /// The MIDI channel (1-16, null for all channels)
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// The MIDI device name. Use null for wildcard (matches all devices).
    /// In lookup keys, null is represented as "*".
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// The SysEx pattern to match (for SysEx input type only)
    /// Contains the complete SysEx message including F0 start and F7 end bytes
    /// </summary>
    public byte[]? SysExPattern { get; set; }

    /// <summary>
    /// Generates a lookup key for O(1) performance in mapping registries.
    /// Format: "DeviceName|Channel|InputNumber|InputType"
    /// Uses "*" for wildcards to enable efficient dictionary lookups.
    /// For ControlChange, both Absolute and Relative types use the same lookup key
    /// since the distinction is only relevant for GUI configuration, not runtime lookup.
    /// </summary>
    /// <returns>A string key optimized for dictionary lookup performance</returns>
    public string GetLookupKey()
    {
        // For ControlChange, ignore the absolute/relative distinction in lookup
        // Both types should map to the same controller at runtime
        var lookupInputType = InputType == MidiInputType.ControlChangeRelative
            ? MidiInputType.ControlChangeAbsolute
            : InputType;

        return $"{DeviceName ?? "*"}|{Channel?.ToString() ?? "*"}|{InputNumber}|{lookupInputType}";
    }

    /// <summary>
    /// Creates a copy of this MIDI input specification
    /// </summary>
    /// <returns>A new instance with the same values</returns>
    public MidiInput Clone()
    {
        return new MidiInput
        {
            InputType = InputType,
            InputNumber = InputNumber,
            Channel = Channel,
            DeviceName = DeviceName,
            SysExPattern = SysExPattern?.ToArray()
        };
    }

    /// <summary>
    /// Returns a human-readable string representation of this MIDI input
    /// </summary>
    public override string ToString()
    {
        var device = string.IsNullOrEmpty(DeviceName) || DeviceName == "*" ? "Any Device" : DeviceName;
        var channel = Channel?.ToString() ?? "Any Channel";

        // For SysEx, show pattern info instead of InputNumber
        if (InputType == MidiInputType.SysEx && SysExPattern != null && SysExPattern.Length > 0)
        {
            return $"{device} - Ch:{channel} - SysEx ({SysExPattern.Length} bytes)";
        }

        return $"{device} - Ch:{channel} - {InputType}:{InputNumber}";
    }
}
