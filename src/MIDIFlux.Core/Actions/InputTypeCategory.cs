namespace MIDIFlux.Core.Actions;

/// <summary>
/// Defines logical categories for MIDI input types to determine action compatibility.
/// Used by the GUI to filter available actions based on the selected input type.
/// </summary>
public enum InputTypeCategory
{
    /// <summary>
    /// Discrete events without meaningful continuous values.
    /// Characteristics:
    /// - Discrete events that happen at a specific moment
    /// - No meaningful continuous value (or value is ignored)
    /// - Typically used for "do something now" actions
    /// 
    /// MIDI Types: NoteOn, NoteOff, SysEx, ProgramChange
    /// </summary>
    Trigger,

    /// <summary>
    /// Continuous values representing absolute positions/levels (0-127).
    /// Characteristics:
    /// - Provide a continuous value in a range (typically 0-127 for MIDI)
    /// - Value represents an absolute position/level
    /// - Suitable for controlling parameters that have a specific target value
    /// 
    /// MIDI Types: ControlChangeAbsolute, PitchBend, Aftertouch, ChannelPressure
    /// </summary>
    AbsoluteValue,

    /// <summary>
    /// Relative movement/change information (direction + magnitude).
    /// Characteristics:
    /// - Provide relative movement/change information
    /// - Value indicates direction and magnitude of change
    /// - From endless encoders, scratch wheels, jog wheels
    /// 
    /// MIDI Types: ControlChangeRelative
    /// </summary>
    RelativeValue
}
