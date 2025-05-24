namespace MIDIFlux.Core.Actions;

/// <summary>
/// Defines the types of MIDI input that can trigger unified actions.
/// Used for mapping MIDI events to actions with optimized lookup performance.
/// </summary>
public enum UnifiedActionMidiInputType
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
    /// MIDI Control Change event (knobs, faders, buttons)
    /// </summary>
    ControlChange,
    
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
    ChannelPressure

    // Note: NoteOnOff input type removed as it complicates lookup logic
    // by requiring checks for multiple conditions during MIDI event processing
}
