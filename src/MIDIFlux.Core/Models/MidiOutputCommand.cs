using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a MIDI output command with all necessary data for sending MIDI messages.
/// Used by MidiOutputAction to define what MIDI messages to send.
/// </summary>
public class MidiOutputCommand
{
    /// <summary>
    /// The type of MIDI message to send
    /// </summary>
    public MidiMessageType MessageType { get; set; } = MidiMessageType.NoteOn;

    /// <summary>
    /// The MIDI channel (1-16)
    /// </summary>
    public int Channel { get; set; } = 1;

    /// <summary>
    /// First data byte (0-127) - meaning depends on MessageType
    /// - NoteOn/NoteOff: Note number
    /// - ControlChange: Controller number
    /// - ProgramChange: Program number
    /// - PitchBend: LSB
    /// - Aftertouch: Note number
    /// - ChannelPressure: Pressure value
    /// </summary>
    public int Data1 { get; set; } = 60;

    /// <summary>
    /// Second data byte (0-127) - meaning depends on MessageType
    /// - NoteOn/NoteOff: Velocity
    /// - ControlChange: Controller value
    /// - ProgramChange: Unused (should be 0)
    /// - PitchBend: MSB
    /// - Aftertouch: Pressure value
    /// - ChannelPressure: Unused (should be 0)
    /// </summary>
    public int Data2 { get; set; } = 127;

    /// <summary>
    /// SysEx data for System Exclusive messages (only used when MessageType is SysEx)
    /// </summary>
    public byte[]? SysExData { get; set; }

    /// <summary>
    /// Validates the MIDI command
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public bool IsValid()
    {
        var errors = GetValidationErrors();
        return errors.Count == 0;
    }

    /// <summary>
    /// Gets validation errors for this command
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        // Validate channel
        if (Channel < 1 || Channel > 16)
        {
            errors.Add($"Channel must be between 1 and 16, got {Channel}");
        }

        // Validate data bytes
        if (Data1 < 0 || Data1 > 127)
        {
            errors.Add($"Data1 must be between 0 and 127, got {Data1}");
        }

        if (Data2 < 0 || Data2 > 127)
        {
            errors.Add($"Data2 must be between 0 and 127, got {Data2}");
        }

        // Validate message type specific requirements
        switch (MessageType)
        {
            case MidiMessageType.SysEx:
                if (SysExData == null || SysExData.Length == 0)
                {
                    errors.Add("SysEx messages require SysExData to be provided");
                }
                break;

            case MidiMessageType.ProgramChange:
            case MidiMessageType.ChannelPressure:
                // These message types don't use Data2, but we don't enforce it to 0
                // to allow for flexibility in configuration
                break;
        }

        return errors;
    }

    /// <summary>
    /// Returns a string representation of this MIDI command
    /// </summary>
    /// <returns>String representation</returns>
    public override string ToString()
    {
        return MessageType switch
        {
            MidiMessageType.NoteOn => $"NoteOn Ch{Channel} Note{Data1} Vel{Data2}",
            MidiMessageType.NoteOff => $"NoteOff Ch{Channel} Note{Data1} Vel{Data2}",
            MidiMessageType.ControlChange => $"CC Ch{Channel} CC{Data1} Val{Data2}",
            MidiMessageType.ProgramChange => $"PC Ch{Channel} Prog{Data1}",
            MidiMessageType.PitchBend => $"PitchBend Ch{Channel} LSB{Data1} MSB{Data2}",
            MidiMessageType.Aftertouch => $"Aftertouch Ch{Channel} Note{Data1} Press{Data2}",
            MidiMessageType.ChannelPressure => $"ChannelPressure Ch{Channel} Press{Data1}",
            MidiMessageType.SysEx => $"SysEx {(SysExData?.Length ?? 0)} bytes",
            _ => $"{MessageType} Ch{Channel} {Data1} {Data2}"
        };
    }

    /// <summary>
    /// Creates a copy of this MIDI command
    /// </summary>
    /// <returns>A new MidiOutputCommand with the same values</returns>
    public MidiOutputCommand Clone()
    {
        return new MidiOutputCommand
        {
            MessageType = MessageType,
            Channel = Channel,
            Data1 = Data1,
            Data2 = Data2,
            SysExData = SysExData?.ToArray() // Create a copy of the array
        };
    }
}
