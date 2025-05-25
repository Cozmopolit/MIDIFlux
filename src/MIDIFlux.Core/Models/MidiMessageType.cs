namespace MIDIFlux.Core.Models;

/// <summary>
/// Defines the types of MIDI messages that can be sent as output.
/// Mirrors the input message types for consistency.
/// </summary>
public enum MidiMessageType
{
    /// <summary>
    /// MIDI Note On message (key press on keyboard/pad)
    /// </summary>
    NoteOn,

    /// <summary>
    /// MIDI Note Off message (key release on keyboard/pad)
    /// </summary>
    NoteOff,

    /// <summary>
    /// MIDI Control Change message (knobs, faders, buttons)
    /// </summary>
    ControlChange,

    /// <summary>
    /// MIDI Program Change message (preset selection)
    /// </summary>
    ProgramChange,

    /// <summary>
    /// MIDI Pitch Bend message (pitch wheel)
    /// </summary>
    PitchBend,

    /// <summary>
    /// MIDI Aftertouch message (pressure sensitive keys)
    /// </summary>
    Aftertouch,

    /// <summary>
    /// MIDI Channel Pressure message (channel-wide pressure)
    /// </summary>
    ChannelPressure,

    /// <summary>
    /// MIDI System Exclusive message (device-specific messages)
    /// </summary>
    SysEx
}
