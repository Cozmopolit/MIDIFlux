namespace MIDIFlux.Core.Models;

/// <summary>
/// Types of MIDI controls
/// </summary>
public enum MidiControlType
{
    /// <summary>
    /// For buttons and pads (already handled by existing code)
    /// </summary>
    Button,

    /// <summary>
    /// For faders, knobs, etc. (0-127)
    /// </summary>
    AbsoluteValue,

    /// <summary>
    /// For jog wheels, endless encoders
    /// </summary>
    RelativeValue,

    /// <summary>
    /// For MIDI notes
    /// </summary>
    Note
}
