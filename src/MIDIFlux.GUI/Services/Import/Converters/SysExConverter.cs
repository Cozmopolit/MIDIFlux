using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Services.Import.Parsers;

namespace MIDIFlux.GUI.Services.Import.Converters
{
    /// <summary>
    /// Converts MIDIKey2Key SysEx patterns and MIDI commands to MIDIFlux format
    /// </summary>
    public class SysExConverter
    {
        private readonly MidiDataParser _midiDataParser;

        /// <summary>
        /// Initializes a new instance of the SysExConverter class
        /// </summary>
        public SysExConverter()
        {
            _midiDataParser = new MidiDataParser();
        }

        /// <summary>
        /// Converts MIDIKey2Key MIDI command strings to MIDIFlux MidiOutputCommand list
        /// </summary>
        /// <param name="midiCommandString">MIDIKey2Key MIDI command string (e.g., "903C7F,904C7F")</param>
        /// <returns>List of MidiOutputCommand objects, or null if conversion fails</returns>
        public List<MidiOutputCommand>? ConvertMidiCommands(string midiCommandString)
        {
            if (string.IsNullOrWhiteSpace(midiCommandString))
            {
                return null;
            }

            try
            {
                var commands = new List<MidiOutputCommand>();

                // Split by comma to handle multiple commands
                var commandStrings = midiCommandString.Split(',', StringSplitOptions.RemoveEmptyEntries);

                foreach (var commandStr in commandStrings)
                {
                    var command = ConvertSingleMidiCommand(commandStr.Trim());
                    if (command != null)
                    {
                        commands.Add(command);
                    }
                }

                return commands.Count > 0 ? commands : null;
            }
            catch (Exception)
            {
                return null; // Failed to convert
            }
        }

        /// <summary>
        /// Converts a single MIDIKey2Key MIDI command to MIDIFlux MidiOutputCommand
        /// </summary>
        /// <param name="commandString">Single MIDI command string (e.g., "903C7F")</param>
        /// <returns>MidiOutputCommand object, or null if conversion fails</returns>
        private MidiOutputCommand? ConvertSingleMidiCommand(string commandString)
        {
            try
            {
                var midiData = _midiDataParser.ParseHexString(commandString);
                if (!midiData.IsValid)
                {
                    return null;
                }

                // Handle SysEx messages
                if (midiData.IsSysEx)
                {
                    return ConvertSysExCommand(midiData);
                }

                // Handle regular MIDI messages
                return ConvertRegularMidiCommand(midiData);
            }
            catch (Exception)
            {
                return null; // Failed to convert
            }
        }

        /// <summary>
        /// Converts SysEx MIDI data to MidiOutputCommand
        /// </summary>
        /// <param name="midiData">Parsed SysEx MIDI data</param>
        /// <returns>MidiOutputCommand for SysEx</returns>
        private MidiOutputCommand ConvertSysExCommand(MidiDataInfo midiData)
        {
            // Parse the hex string to byte array
            var sysExBytes = ParseHexStringToBytes(midiData.RawHexData);

            return new MidiOutputCommand
            {
                MessageType = Core.Models.MidiMessageType.SysEx,
                Channel = 1, // Not used for SysEx but required by structure
                Data1 = 0,   // Not used for SysEx
                Data2 = 0,   // Not used for SysEx
                SysExData = sysExBytes
            };
        }

        /// <summary>
        /// Converts regular MIDI message data to MidiOutputCommand
        /// </summary>
        /// <param name="midiData">Parsed MIDI data</param>
        /// <returns>MidiOutputCommand for regular MIDI message</returns>
        private MidiOutputCommand? ConvertRegularMidiCommand(MidiDataInfo midiData)
        {
            // Handle wildcards by converting to SysEx pattern
            if (midiData.HasWildcards)
            {
                // For wildcards, we'll create a SysEx command with the pattern
                var sysExPattern = ConvertWildcardToSysExBytes(midiData.RawHexData);
                return new MidiOutputCommand
                {
                    MessageType = Core.Models.MidiMessageType.SysEx,
                    Channel = 1,
                    Data1 = 0,
                    Data2 = 0,
                    SysExData = sysExPattern
                };
            }

            // Convert based on MIDI message type
            var command = new MidiOutputCommand
            {
                Channel = midiData.Channel,
                Data1 = midiData.Data1 ?? 0,
                Data2 = midiData.Data2 ?? 0
            };

            // Map the message type from parser enum to Core enum
            command.MessageType = midiData.MessageType switch
            {
                Parsers.MidiMessageType.NoteOff => Core.Models.MidiMessageType.NoteOff,
                Parsers.MidiMessageType.NoteOn => Core.Models.MidiMessageType.NoteOn,
                Parsers.MidiMessageType.PolyphonicKeyPressure => Core.Models.MidiMessageType.Aftertouch,
                Parsers.MidiMessageType.ControlChange => Core.Models.MidiMessageType.ControlChange,
                Parsers.MidiMessageType.ProgramChange => Core.Models.MidiMessageType.ProgramChange,
                Parsers.MidiMessageType.ChannelPressure => Core.Models.MidiMessageType.ChannelPressure,
                Parsers.MidiMessageType.PitchBend => Core.Models.MidiMessageType.PitchBend,
                _ => Core.Models.MidiMessageType.NoteOn // Default fallback
            };

            return command;
        }

        /// <summary>
        /// Converts a hex string to byte array
        /// </summary>
        /// <param name="hexString">Hex string (e.g., "F0431200F7")</param>
        /// <returns>Byte array</returns>
        private byte[] ParseHexStringToBytes(string hexString)
        {
            // Remove any spaces and ensure even length
            var cleanHex = hexString.Replace(" ", "").Replace("-", "");
            if (cleanHex.Length % 2 != 0)
            {
                cleanHex = "0" + cleanHex; // Pad with leading zero
            }

            var bytes = new byte[cleanHex.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                var hexByte = cleanHex.Substring(i * 2, 2);
                bytes[i] = byte.Parse(hexByte, NumberStyles.HexNumber);
            }

            return bytes;
        }

        /// <summary>
        /// Converts wildcard MIDI data to SysEx byte pattern
        /// </summary>
        /// <param name="hexData">Hex data with wildcards (e.g., "90xx7F")</param>
        /// <returns>SysEx byte array with wildcard markers</returns>
        private byte[] ConvertWildcardToSysExBytes(string hexData)
        {
            // For wildcards, we need to wrap in SysEx format and convert xx to 0xFF
            var cleanHex = hexData.Replace(" ", "").Replace("-", "");

            // Replace wildcard patterns
            cleanHex = cleanHex.Replace("xx", "FF").Replace("XX", "FF");

            // Wrap in SysEx format (F0 ... F7)
            var sysExHex = "F0" + cleanHex + "F7";

            return ParseHexStringToBytes(sysExHex);
        }

        /// <summary>
        /// Converts MIDIKey2Key SysEx pattern to MIDIFlux format
        /// </summary>
        /// <param name="mk2kSysExPattern">MIDIKey2Key SysEx pattern (e.g., "F041xx4212xxxxF7")</param>
        /// <returns>MIDIFlux SysEx pattern (e.g., "F0 41 XX 42 12 XX XX F7")</returns>
        public string ConvertSysExPattern(string mk2kSysExPattern)
        {
            if (string.IsNullOrWhiteSpace(mk2kSysExPattern))
            {
                return "";
            }

            try
            {
                // Clean the input
                var cleanHex = mk2kSysExPattern.Replace(" ", "").Replace("-", "").ToUpper();

                // Replace lowercase wildcards with uppercase
                cleanHex = cleanHex.Replace("xx", "XX");

                // Add spaces between bytes
                var spacedHex = "";
                for (int i = 0; i < cleanHex.Length; i += 2)
                {
                    if (i > 0) spacedHex += " ";
                    if (i + 1 < cleanHex.Length)
                    {
                        spacedHex += cleanHex.Substring(i, 2);
                    }
                    else
                    {
                        spacedHex += "0" + cleanHex.Substring(i, 1); // Pad single character
                    }
                }

                return spacedHex;
            }
            catch (Exception)
            {
                return ""; // Failed to convert
            }
        }

        /// <summary>
        /// Validates that a SysEx pattern is properly formatted
        /// </summary>
        /// <param name="sysExPattern">SysEx pattern to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidSysExPattern(string sysExPattern)
        {
            if (string.IsNullOrWhiteSpace(sysExPattern))
            {
                return false;
            }

            var cleanHex = sysExPattern.Replace(" ", "").Replace("-", "").ToUpper();

            // Must start with F0 and end with F7
            if (!cleanHex.StartsWith("F0") || !cleanHex.EndsWith("F7"))
            {
                return false;
            }

            // Must have even length (complete bytes)
            if (cleanHex.Length % 2 != 0)
            {
                return false;
            }

            // All characters must be valid hex or wildcards
            foreach (char c in cleanHex)
            {
                if (!char.IsDigit(c) && c != 'A' && c != 'B' && c != 'C' &&
                    c != 'D' && c != 'E' && c != 'F' && c != 'X')
                {
                    return false;
                }
            }

            return true;
        }
    }
}
