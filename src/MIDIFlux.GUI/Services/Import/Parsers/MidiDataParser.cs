using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace MIDIFlux.GUI.Services.Import.Parsers
{
    /// <summary>
    /// Parser for MIDIKey2Key MIDI data hex strings
    /// </summary>
    public class MidiDataParser
    {
        /// <summary>
        /// Parses a MIDIKey2Key hex string into MIDI data components
        /// </summary>
        /// <param name="hexString">The hex string to parse (e.g., "903C7F", "90xx7F")</param>
        /// <returns>Parsed MIDI data information</returns>
        public MidiDataInfo ParseHexString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
            {
                throw new ArgumentException("Hex string cannot be null or empty", nameof(hexString));
            }

            // Clean the hex string (remove spaces, convert to uppercase)
            var cleanHex = hexString.Replace(" ", "").ToUpperInvariant();

            // Check for special cases
            if (cleanHex == "STARTUP")
            {
                return new MidiDataInfo
                {
                    IsStartupAction = true,
                    IsValid = true
                };
            }

            // Check if it's a SysEx message
            if (cleanHex.StartsWith("F0") && cleanHex.EndsWith("F7"))
            {
                return ParseSysExData(cleanHex);
            }

            // Parse regular MIDI message (should be 6 characters for 3-byte message)
            if (cleanHex.Length < 2)
            {
                return new MidiDataInfo
                {
                    IsValid = false,
                    ErrorMessage = "Hex string too short"
                };
            }

            return ParseRegularMidiData(cleanHex);
        }

        /// <summary>
        /// Parses a SysEx hex string
        /// </summary>
        /// <param name="cleanHex">Clean hex string starting with F0 and ending with F7</param>
        /// <returns>Parsed SysEx data information</returns>
        private MidiDataInfo ParseSysExData(string cleanHex)
        {
            var result = new MidiDataInfo
            {
                IsSysEx = true,
                IsValid = true,
                RawHexData = cleanHex
            };

            // Check for wildcards
            result.HasWildcards = cleanHex.Contains("XX");

            // Convert to space-separated format for MIDIFlux
            result.SysExPattern = ConvertToSpaceSeparatedHex(cleanHex);

            return result;
        }

        /// <summary>
        /// Parses regular MIDI data (Note On/Off, Control Change, etc.)
        /// </summary>
        /// <param name="cleanHex">Clean hex string</param>
        /// <returns>Parsed MIDI data information</returns>
        private MidiDataInfo ParseRegularMidiData(string cleanHex)
        {
            var result = new MidiDataInfo
            {
                RawHexData = cleanHex,
                HasWildcards = cleanHex.Contains("XX")
            };

            try
            {
                // Parse status byte (first byte)
                var statusHex = cleanHex.Substring(0, 2);
                if (statusHex == "XX")
                {
                    result.HasWildcards = true;
                    result.StatusByte = null;
                }
                else
                {
                    result.StatusByte = Convert.ToByte(statusHex, 16);
                    result.MessageType = GetMidiMessageType(result.StatusByte.Value);
                    result.Channel = (result.StatusByte.Value & 0x0F) + 1; // Convert to 1-based
                }

                // Parse data bytes if present
                if (cleanHex.Length >= 4)
                {
                    var data1Hex = cleanHex.Substring(2, 2);
                    if (data1Hex == "XX")
                    {
                        result.HasWildcards = true;
                        result.Data1 = null;
                    }
                    else
                    {
                        result.Data1 = Convert.ToByte(data1Hex, 16);
                    }
                }

                if (cleanHex.Length >= 6)
                {
                    var data2Hex = cleanHex.Substring(4, 2);
                    if (data2Hex == "XX")
                    {
                        result.HasWildcards = true;
                        result.Data2 = null;
                    }
                    else
                    {
                        result.Data2 = Convert.ToByte(data2Hex, 16);
                    }
                }

                result.IsValid = true;
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Failed to parse hex data: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Determines the MIDI message type from the status byte
        /// </summary>
        /// <param name="statusByte">The MIDI status byte</param>
        /// <returns>The MIDI message type</returns>
        private MidiMessageType GetMidiMessageType(byte statusByte)
        {
            var messageType = statusByte & 0xF0;
            return messageType switch
            {
                0x80 => MidiMessageType.NoteOff,
                0x90 => MidiMessageType.NoteOn,
                0xA0 => MidiMessageType.PolyphonicKeyPressure,
                0xB0 => MidiMessageType.ControlChange,
                0xC0 => MidiMessageType.ProgramChange,
                0xD0 => MidiMessageType.ChannelPressure,
                0xE0 => MidiMessageType.PitchBend,
                0xF0 => MidiMessageType.SystemExclusive,
                _ => MidiMessageType.Unknown
            };
        }

        /// <summary>
        /// Converts a hex string to space-separated format
        /// </summary>
        /// <param name="hexString">Input hex string</param>
        /// <returns>Space-separated hex string</returns>
        private string ConvertToSpaceSeparatedHex(string hexString)
        {
            var result = new List<string>();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                if (i + 1 < hexString.Length)
                {
                    result.Add(hexString.Substring(i, 2));
                }
            }
            return string.Join(" ", result);
        }

        /// <summary>
        /// Validates a hex string format
        /// </summary>
        /// <param name="hexString">The hex string to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidHexString(string hexString)
        {
            if (string.IsNullOrWhiteSpace(hexString))
                return false;

            var cleanHex = hexString.Replace(" ", "").ToUpperInvariant();
            
            // Allow special cases
            if (cleanHex == "STARTUP")
                return true;

            // Check if it contains only valid hex characters and wildcards
            var hexPattern = @"^[0-9A-FX]+$";
            if (!Regex.IsMatch(cleanHex, hexPattern))
                return false;

            // Must have even number of characters (complete bytes)
            return cleanHex.Length % 2 == 0;
        }
    }

    /// <summary>
    /// Information about parsed MIDI data
    /// </summary>
    public class MidiDataInfo
    {
        /// <summary>
        /// Gets or sets whether the parsing was successful
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if parsing failed
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Gets or sets whether this is a startup action
        /// </summary>
        public bool IsStartupAction { get; set; }

        /// <summary>
        /// Gets or sets whether this is a SysEx message
        /// </summary>
        public bool IsSysEx { get; set; }

        /// <summary>
        /// Gets or sets whether the data contains wildcards
        /// </summary>
        public bool HasWildcards { get; set; }

        /// <summary>
        /// Gets or sets the raw hex data
        /// </summary>
        public string RawHexData { get; set; } = "";

        /// <summary>
        /// Gets or sets the MIDI message type
        /// </summary>
        public MidiMessageType MessageType { get; set; }

        /// <summary>
        /// Gets or sets the MIDI channel (1-based)
        /// </summary>
        public int Channel { get; set; }

        /// <summary>
        /// Gets or sets the status byte (null if wildcard)
        /// </summary>
        public byte? StatusByte { get; set; }

        /// <summary>
        /// Gets or sets the first data byte (null if wildcard or not present)
        /// </summary>
        public byte? Data1 { get; set; }

        /// <summary>
        /// Gets or sets the second data byte (null if wildcard or not present)
        /// </summary>
        public byte? Data2 { get; set; }

        /// <summary>
        /// Gets or sets the SysEx pattern in MIDIFlux format (space-separated)
        /// </summary>
        public string SysExPattern { get; set; } = "";
    }

    /// <summary>
    /// MIDI message types
    /// </summary>
    public enum MidiMessageType
    {
        Unknown,
        NoteOff,
        NoteOn,
        PolyphonicKeyPressure,
        ControlChange,
        ProgramChange,
        ChannelPressure,
        PitchBend,
        SystemExclusive
    }
}
