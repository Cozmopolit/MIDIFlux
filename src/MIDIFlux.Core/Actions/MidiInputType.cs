namespace MIDIFlux.Core.Actions;

/// <summary>
/// Defines the types of MIDI input that can trigger actions.
/// Used for mapping MIDI events to actions with optimized lookup performance.
/// Supports both absolute and relative control change types for proper action compatibility.
/// </summary>
public enum MidiInputType
{
    /// <summary>
    /// MIDI Note On event (key press on keyboard/pad)
    /// </summary>
    NoteOn,

    /// <summary>
    /// MIDI Note Off event (key release on keyboard/pad)
    /// </summary>
    NoteOff,

    /// <summary>
    /// MIDI Control Change event for absolute controllers (faders, absolute knobs)
    /// Provides continuous values representing absolute positions/levels (0-127)
    /// </summary>
    ControlChangeAbsolute,

    /// <summary>
    /// MIDI Control Change event for relative controllers (endless encoders, scratch wheels, jog wheels)
    /// Provides relative movement/change information (direction + magnitude)
    /// </summary>
    ControlChangeRelative,

    /// <summary>
    /// MIDI Program Change event (preset selection)
    /// </summary>
    ProgramChange,

    /// <summary>
    /// MIDI Pitch Bend event (pitch wheel)
    /// </summary>
    PitchBend,

    /// <summary>
    /// MIDI Aftertouch event (pressure sensitive keys)
    /// </summary>
    Aftertouch,

    /// <summary>
    /// MIDI Channel Pressure event (channel-wide pressure)
    /// </summary>
    ChannelPressure,

    /// <summary>
    /// MIDI System Exclusive event (device-specific messages)
    /// </summary>
    SysEx

    // Note: NoteOnOff input type removed as it complicates lookup logic
    // by requiring checks for multiple conditions during MIDI event processing
    // Note: Original ControlChange enum value removed - clean break for v2.0
}
