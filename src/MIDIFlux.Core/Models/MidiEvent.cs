using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a MIDI event with all relevant information
/// </summary>
public class MidiEvent
{
    /// <summary>
    /// The type of MIDI event
    /// </summary>
    public MidiEventType EventType { get; set; }

    /// <summary>
    /// The MIDI channel (1-16, user-facing numbering)
    /// </summary>
    public int Channel { get; set; }

    /// <summary>
    /// The note number (for Note On/Off events)
    /// </summary>
    public int? Note { get; set; }

    /// <summary>
    /// The velocity (for Note On/Off events)
    /// </summary>
    public int? Velocity { get; set; }

    /// <summary>
    /// The controller number (for Control Change events)
    /// </summary>
    public int? Controller { get; set; }

    /// <summary>
    /// The controller value (for Control Change events)
    /// </summary>
    public int? Value { get; set; }

    /// <summary>
    /// The program number (for ProgramChange events, 0-127)
    /// </summary>
    public int? ProgramNumber { get; set; }

    /// <summary>
    /// The pitch bend value (for PitchBend events, 0-16383, center is 8192)
    /// </summary>
    public int? PitchBendValue { get; set; }

    /// <summary>
    /// The pressure value (for ChannelPressure and PolyphonicKeyPressure events, 0-127)
    /// </summary>
    public int? Pressure { get; set; }

    /// <summary>
    /// Indicates if this Control Change message represents a relative value (for jog wheels, etc.)
    /// </summary>
    public bool IsRelative { get; set; }

    /// <summary>
    /// The encoding method for relative values (for jog wheels, etc.)
    /// </summary>
    public RelativeValueEncoding? RelativeEncoding { get; set; }

    /// <summary>
    /// The specific error type for Error events
    /// </summary>
    public MidiErrorType? ErrorType { get; set; }

    /// <summary>
    /// The raw MIDI message data
    /// </summary>
    public byte[] RawData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// The SysEx data (for SystemExclusive events only)
    /// Contains the complete SysEx message including F0 start and F7 end bytes
    /// </summary>
    public byte[]? SysExData { get; set; }

    /// <summary>
    /// The timestamp when the event was received
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Returns a string representation of the MIDI event
    /// </summary>
    public override string ToString()
    {
        return EventType switch
        {
            MidiEventType.NoteOn => $"Note On: Channel={Channel}, Note={Note}, Velocity={Velocity}",
            MidiEventType.NoteOff => $"Note Off: Channel={Channel}, Note={Note}, Velocity={Velocity}",
            MidiEventType.ControlChange => $"Control Change: Channel={Channel}, Controller={Controller}, Value={Value}" + (IsRelative ? " (Relative)" : ""),
            MidiEventType.ProgramChange => $"Program Change: Channel={Channel}, Program={ProgramNumber}",
            MidiEventType.PitchBend => $"Pitch Bend: Channel={Channel}, Value={PitchBendValue}",
            MidiEventType.ChannelPressure => $"Channel Pressure: Channel={Channel}, Pressure={Pressure}",
            MidiEventType.PolyphonicKeyPressure => $"Polyphonic Key Pressure: Channel={Channel}, Note={Note}, Pressure={Pressure}",
            MidiEventType.SystemExclusive => $"SysEx: {(SysExData?.Length ?? 0)} bytes" + (SysExData?.Length > 0 ? $" [{HexByteConverter.FormatByteArray(SysExData.Take(8).ToArray())}...]" : ""),
            MidiEventType.Error => $"Error: {ErrorType}",
            MidiEventType.Other => $"Other MIDI Event: Channel={Channel}",
            _ => $"Unknown MIDI Event: Type={EventType}, Channel={Channel}"
        };
    }
}

/// <summary>
/// Types of MIDI events we're interested in
/// </summary>
public enum MidiEventType
{
    /// <summary>
    /// Note On event (key press)
    /// </summary>
    NoteOn,

    /// <summary>
    /// Note Off event (key release)
    /// </summary>
    NoteOff,

    /// <summary>
    /// Control Change event (fader, knob, etc.)
    /// </summary>
    ControlChange,

    /// <summary>
    /// Program Change event (preset selection)
    /// </summary>
    ProgramChange,

    /// <summary>
    /// Pitch Bend event (pitch wheel)
    /// </summary>
    PitchBend,

    /// <summary>
    /// Channel Pressure event (channel-wide aftertouch)
    /// </summary>
    ChannelPressure,

    /// <summary>
    /// Polyphonic Key Pressure event (per-note aftertouch)
    /// </summary>
    PolyphonicKeyPressure,

    /// <summary>
    /// System Exclusive event (device-specific messages)
    /// </summary>
    SystemExclusive,

    /// <summary>
    /// Error event from the MIDI device
    /// </summary>
    Error,

    /// <summary>
    /// Other MIDI event types
    /// </summary>
    Other
}

/// <summary>
/// Types of MIDI errors
/// </summary>
public enum MidiErrorType
{
    /// <summary>
    /// Generic error
    /// </summary>
    Generic,

    /// <summary>
    /// Device disconnected
    /// </summary>
    DeviceDisconnected,

    /// <summary>
    /// Device error
    /// </summary>
    DeviceError,

    /// <summary>
    /// Communication error
    /// </summary>
    CommunicationError,

    /// <summary>
    /// Error processing MIDI event
    /// </summary>
    ProcessingError
}
