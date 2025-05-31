using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Tests.Utilities;

/// <summary>
/// Builder class for creating MIDI events for testing
/// </summary>
public class MidiEventBuilder
{
    private MidiEventType _eventType = MidiEventType.NoteOn;
    private int _channel = 1;
    private int? _note;
    private int? _velocity;
    private int? _controller;
    private int? _value;
    private byte[]? _sysExData;

    /// <summary>
    /// Creates a new MIDI event builder
    /// </summary>
    public static MidiEventBuilder Create() => new();

    /// <summary>
    /// Sets the event type
    /// </summary>
    public MidiEventBuilder WithEventType(MidiEventType eventType)
    {
        _eventType = eventType;
        return this;
    }

    /// <summary>
    /// Sets the MIDI channel (1-16)
    /// </summary>
    public MidiEventBuilder WithChannel(int channel)
    {
        if (channel < 1 || channel > 16)
            throw new ArgumentOutOfRangeException(nameof(channel), "Channel must be between 1 and 16");

        _channel = channel;
        return this;
    }

    /// <summary>
    /// Sets the note number (0-127)
    /// </summary>
    public MidiEventBuilder WithNote(int note)
    {
        if (note < 0 || note > 127)
            throw new ArgumentOutOfRangeException(nameof(note), "Note must be between 0 and 127");

        _note = note;
        return this;
    }

    /// <summary>
    /// Sets the velocity (0-127)
    /// </summary>
    public MidiEventBuilder WithVelocity(int velocity)
    {
        if (velocity < 0 || velocity > 127)
            throw new ArgumentOutOfRangeException(nameof(velocity), "Velocity must be between 0 and 127");

        _velocity = velocity;
        return this;
    }

    /// <summary>
    /// Sets the controller number (0-127)
    /// </summary>
    public MidiEventBuilder WithController(int controller)
    {
        if (controller < 0 || controller > 127)
            throw new ArgumentOutOfRangeException(nameof(controller), "Controller must be between 0 and 127");

        _controller = controller;
        return this;
    }

    /// <summary>
    /// Sets the control value (0-127)
    /// </summary>
    public MidiEventBuilder WithValue(int value)
    {
        if (value < 0 || value > 127)
            throw new ArgumentOutOfRangeException(nameof(value), "Value must be between 0 and 127");

        _value = value;
        return this;
    }

    /// <summary>
    /// Sets SysEx data
    /// </summary>
    public MidiEventBuilder WithSysExData(byte[] data)
    {
        _sysExData = data;
        return this;
    }

    /// <summary>
    /// Builds the MIDI event
    /// </summary>
    public MidiEvent Build()
    {
        return new MidiEvent
        {
            EventType = _eventType,
            Channel = _channel,
            Note = _note,
            Velocity = _velocity,
            Controller = _controller,
            Value = _value,
            SysExData = _sysExData
        };
    }

    // Convenience methods for common event types

    /// <summary>
    /// Creates a Note On event
    /// </summary>
    public static MidiEvent NoteOn(int channel, int note, int velocity = 127)
    {
        return Create()
            .WithEventType(MidiEventType.NoteOn)
            .WithChannel(channel)
            .WithNote(note)
            .WithVelocity(velocity)
            .Build();
    }

    /// <summary>
    /// Creates a Note Off event
    /// </summary>
    public static MidiEvent NoteOff(int channel, int note, int velocity = 0)
    {
        return Create()
            .WithEventType(MidiEventType.NoteOff)
            .WithChannel(channel)
            .WithNote(note)
            .WithVelocity(velocity)
            .Build();
    }

    /// <summary>
    /// Creates a Control Change event
    /// </summary>
    public static MidiEvent ControlChange(int channel, int controller, int value)
    {
        return Create()
            .WithEventType(MidiEventType.ControlChange)
            .WithChannel(channel)
            .WithController(controller)
            .WithValue(value)
            .Build();
    }

    // Note: ProgramChange, PitchBend, and SysEx are not currently supported in MidiEventType
    // These methods are commented out until the enum is extended

    /*
    /// <summary>
    /// Creates a Program Change event
    /// </summary>
    public static MidiEvent ProgramChange(int channel, int program)
    {
        return Create()
            .WithEventType(MidiEventType.ProgramChange)
            .WithChannel(channel)
            .WithValue(program)
            .Build();
    }

    /// <summary>
    /// Creates a Pitch Bend event
    /// </summary>
    public static MidiEvent PitchBend(int channel, int value)
    {
        return Create()
            .WithEventType(MidiEventType.PitchBend)
            .WithChannel(channel)
            .WithValue(value)
            .Build();
    }

    /// <summary>
    /// Creates a SysEx event
    /// </summary>
    public static MidiEvent SysEx(byte[] data)
    {
        return Create()
            .WithEventType(MidiEventType.SysEx)
            .WithSysExData(data)
            .Build();
    }
    */
}
