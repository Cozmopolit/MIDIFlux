using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for MIDI Output actions.
/// Represents sending one or more MIDI messages to a specified output device.
/// </summary>
public class MidiOutputConfig : UnifiedActionConfig
{
    /// <summary>
    /// The name of the MIDI output device to send messages to.
    /// Must be a specific device name - wildcards are not supported for output.
    /// </summary>
    public string OutputDeviceName { get; set; } = string.Empty;

    /// <summary>
    /// The list of MIDI commands to send in sequence.
    /// All commands are sent immediately without delays.
    /// Use SequenceAction with DelayAction for timing control.
    /// </summary>
    public List<MidiOutputCommand> Commands { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of MidiOutputConfig
    /// </summary>
    public MidiOutputConfig()
    {
        Type = UnifiedActionType.MidiOutput;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        // Device name must be specified (no wildcards)
        if (string.IsNullOrWhiteSpace(OutputDeviceName) || OutputDeviceName == "*")
            return false;

        // Must have at least one command
        if (Commands == null || Commands.Count == 0)
            return false;

        // All commands must be valid
        return Commands.All(cmd => cmd.IsValid());
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(OutputDeviceName))
        {
            errors.Add("Output device name must be specified");
        }
        else if (OutputDeviceName == "*")
        {
            errors.Add("Wildcard device names are not supported for MIDI output - specify a concrete device name");
        }

        if (Commands == null || Commands.Count == 0)
        {
            errors.Add("At least one MIDI command must be specified");
        }
        else
        {
            for (int i = 0; i < Commands.Count; i++)
            {
                var commandErrors = Commands[i].GetValidationErrors();
                foreach (var error in commandErrors)
                {
                    errors.Add($"Command {i + 1}: {error}");
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;

        var commandCount = Commands?.Count ?? 0;
        return $"MIDI Output to '{OutputDeviceName}' ({commandCount} command{(commandCount != 1 ? "s" : "")})";
    }
}

/// <summary>
/// Represents a single MIDI message to be sent as output.
/// Mirrors the structure of input MIDI events for consistency.
/// </summary>
public class MidiOutputCommand
{
    /// <summary>
    /// The type of MIDI message to send
    /// </summary>
    public MidiMessageType MessageType { get; set; }

    /// <summary>
    /// The MIDI channel (1-16) - must be specified, no wildcards
    /// </summary>
    public int Channel { get; set; } = 1;

    /// <summary>
    /// First data byte (Note number for Note On/Off, Controller number for CC, etc.)
    /// </summary>
    public int Data1 { get; set; } = 0;

    /// <summary>
    /// Second data byte (Velocity for Note On/Off, Value for CC, etc.)
    /// </summary>
    public int Data2 { get; set; } = 0;

    /// <summary>
    /// SysEx data for SystemExclusive messages.
    /// Should include F0 start and F7 end bytes.
    /// </summary>
    public byte[]? SysExData { get; set; }

    /// <summary>
    /// Optional description for this command
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Validates this MIDI command
    /// </summary>
    public bool IsValid()
    {
        // Channel must be 1-16
        if (Channel < 1 || Channel > 16)
            return false;

        // Validate based on message type
        switch (MessageType)
        {
            case MidiMessageType.NoteOn:
            case MidiMessageType.NoteOff:
                // Note number (Data1) must be 0-127
                // Velocity (Data2) must be 0-127
                return Data1 >= 0 && Data1 <= 127 && Data2 >= 0 && Data2 <= 127;

            case MidiMessageType.ControlChange:
                // Controller number (Data1) must be 0-127
                // Value (Data2) must be 0-127
                return Data1 >= 0 && Data1 <= 127 && Data2 >= 0 && Data2 <= 127;

            case MidiMessageType.ProgramChange:
                // Program number (Data1) must be 0-127
                // Data2 is not used for Program Change
                return Data1 >= 0 && Data1 <= 127;

            case MidiMessageType.PitchBend:
                // Pitch bend value is 14-bit (0-16383)
                // Data1 = LSB, Data2 = MSB
                return Data1 >= 0 && Data1 <= 127 && Data2 >= 0 && Data2 <= 127;

            case MidiMessageType.Aftertouch:
                // Note number (Data1) must be 0-127
                // Pressure (Data2) must be 0-127
                return Data1 >= 0 && Data1 <= 127 && Data2 >= 0 && Data2 <= 127;

            case MidiMessageType.ChannelPressure:
                // Pressure (Data1) must be 0-127
                // Data2 is not used for Channel Pressure
                return Data1 >= 0 && Data1 <= 127;

            case MidiMessageType.SysEx:
                // SysEx data must be provided and valid
                return SysExData != null && SysExData.Length >= 3 && 
                       SysExData[0] == 0xF0 && SysExData[SysExData.Length - 1] == 0xF7;

            default:
                return false;
        }
    }

    /// <summary>
    /// Gets validation error messages for this command
    /// </summary>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (Channel < 1 || Channel > 16)
        {
            errors.Add($"Channel must be between 1 and 16, got {Channel}");
        }

        switch (MessageType)
        {
            case MidiMessageType.NoteOn:
            case MidiMessageType.NoteOff:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Note number must be between 0 and 127, got {Data1}");
                if (Data2 < 0 || Data2 > 127)
                    errors.Add($"Velocity must be between 0 and 127, got {Data2}");
                break;

            case MidiMessageType.ControlChange:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Controller number must be between 0 and 127, got {Data1}");
                if (Data2 < 0 || Data2 > 127)
                    errors.Add($"Controller value must be between 0 and 127, got {Data2}");
                break;

            case MidiMessageType.ProgramChange:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Program number must be between 0 and 127, got {Data1}");
                break;

            case MidiMessageType.PitchBend:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Pitch bend LSB must be between 0 and 127, got {Data1}");
                if (Data2 < 0 || Data2 > 127)
                    errors.Add($"Pitch bend MSB must be between 0 and 127, got {Data2}");
                break;

            case MidiMessageType.Aftertouch:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Note number must be between 0 and 127, got {Data1}");
                if (Data2 < 0 || Data2 > 127)
                    errors.Add($"Pressure must be between 0 and 127, got {Data2}");
                break;

            case MidiMessageType.ChannelPressure:
                if (Data1 < 0 || Data1 > 127)
                    errors.Add($"Pressure must be between 0 and 127, got {Data1}");
                break;

            case MidiMessageType.SysEx:
                if (SysExData == null)
                {
                    errors.Add("SysEx data must be provided for SystemExclusive messages");
                }
                else
                {
                    if (SysExData.Length < 3)
                        errors.Add("SysEx data must be at least 3 bytes long");
                    else
                    {
                        if (SysExData[0] != 0xF0)
                            errors.Add("SysEx data must start with F0 byte");
                        if (SysExData[SysExData.Length - 1] != 0xF7)
                            errors.Add("SysEx data must end with F7 byte");
                    }
                }
                break;

            default:
                errors.Add($"Unknown message type: {MessageType}");
                break;
        }

        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;

        return MessageType switch
        {
            MidiMessageType.NoteOn => $"Note On Ch{Channel} Note{Data1} Vel{Data2}",
            MidiMessageType.NoteOff => $"Note Off Ch{Channel} Note{Data1} Vel{Data2}",
            MidiMessageType.ControlChange => $"CC Ch{Channel} CC{Data1} Val{Data2}",
            MidiMessageType.ProgramChange => $"PC Ch{Channel} Prog{Data1}",
            MidiMessageType.PitchBend => $"Pitch Bend Ch{Channel} Val{(Data2 << 7) | Data1}",
            MidiMessageType.Aftertouch => $"Aftertouch Ch{Channel} Note{Data1} Press{Data2}",
            MidiMessageType.ChannelPressure => $"Channel Pressure Ch{Channel} Press{Data1}",
            MidiMessageType.SysEx => $"SysEx ({SysExData?.Length ?? 0} bytes)",
            _ => $"Unknown MIDI Message"
        };
    }
}
