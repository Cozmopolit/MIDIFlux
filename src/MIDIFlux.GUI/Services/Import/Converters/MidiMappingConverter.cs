using System;
using System.Collections.Generic;
using MIDIFlux.Core.Configuration;
using MIDIFlux.GUI.Services.Import.Models;
using MIDIFlux.GUI.Services.Import.Parsers;

namespace MIDIFlux.GUI.Services.Import.Converters
{
    /// <summary>
    /// Converts MIDIKey2Key MIDI data to MIDIFlux MIDI input configurations
    /// </summary>
    public class MidiMappingConverter
    {
        private readonly MidiDataParser _midiDataParser;

        /// <summary>
        /// Initializes a new instance of the MidiMappingConverter class
        /// </summary>
        public MidiMappingConverter()
        {
            _midiDataParser = new MidiDataParser();
        }

        /// <summary>
        /// Parses MIDI data from a MIDIKey2Key hex string
        /// </summary>
        /// <param name="hexString">The hex string to parse</param>
        /// <returns>Parsed MIDI data information</returns>
        public MidiDataInfo? ParseMidiData(string hexString)
        {
            try
            {
                return _midiDataParser.ParseHexString(hexString);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a MIDIKey2Key action to a MIDIFlux mapping configuration entry
        /// </summary>
        /// <param name="action">The MIDIKey2Key action to convert</param>
        /// <returns>MIDIFlux mapping configuration entry, or null if not convertible</returns>
        public MappingConfigEntry? ConvertToMidiInput(MidiKey2KeyAction action)
        {
            if (string.IsNullOrWhiteSpace(action.Data) || action.Data == "STARTUP")
            {
                return null; // Not a MIDI input action
            }

            try
            {
                var midiData = _midiDataParser.ParseHexString(action.Data);
                if (!midiData.IsValid)
                {
                    return null;
                }

                // Handle SysEx messages
                if (midiData.IsSysEx)
                {
                    return new MappingConfigEntry
                    {
                        InputType = "SysEx",
                        SysExPattern = midiData.SysExPattern,
                        Description = !string.IsNullOrWhiteSpace(action.Comment) ? action.Comment : action.Name,
                        IsEnabled = true
                    };
                }

                // Handle regular MIDI messages
                return ConvertRegularMidiMessage(midiData, action);
            }
            catch (Exception)
            {
                return null; // Failed to convert
            }
        }

        /// <summary>
        /// Converts regular MIDI message data to MIDIFlux mapping configuration entry
        /// </summary>
        /// <param name="midiData">Parsed MIDI data</param>
        /// <param name="action">Original MIDIKey2Key action</param>
        /// <returns>MIDIFlux mapping configuration entry</returns>
        private MappingConfigEntry ConvertRegularMidiMessage(MidiDataInfo midiData, MidiKey2KeyAction action)
        {
            var config = new MappingConfigEntry
            {
                Description = !string.IsNullOrWhiteSpace(action.Comment) ? action.Comment : action.Name,
                IsEnabled = true
            };

            // Handle wildcards by converting to SysEx pattern
            if (midiData.HasWildcards)
            {
                config.InputType = "SysEx";
                config.SysExPattern = ConvertWildcardToSysExPattern(midiData);
                return config;
            }

            // Convert based on message type
            switch (midiData.MessageType)
            {
                case MidiMessageType.NoteOn:
                    config.InputType = "NoteOn";
                    config.Channel = midiData.Channel;
                    config.Note = midiData.Data1 ?? 0;
                    // Note: Velocity is not stored in MappingConfigEntry, it's part of the trigger
                    break;

                case MidiMessageType.NoteOff:
                    config.InputType = "NoteOff";
                    config.Channel = midiData.Channel;
                    config.Note = midiData.Data1 ?? 0;
                    break;

                case MidiMessageType.ControlChange:
                    config.InputType = "ControlChange";
                    config.Channel = midiData.Channel;
                    config.ControlNumber = midiData.Data1 ?? 0;
                    // Note: Value is not stored in MappingConfigEntry, it's part of the trigger
                    break;

                case MidiMessageType.ProgramChange:
                    config.InputType = "ProgramChange";
                    config.Channel = midiData.Channel;
                    // Note: Program number is not stored in MappingConfigEntry for ProgramChange
                    break;

                case MidiMessageType.PitchBend:
                    config.InputType = "PitchBend";
                    config.Channel = midiData.Channel;
                    // Note: Pitch bend value is not stored in MappingConfigEntry
                    break;

                case MidiMessageType.ChannelPressure:
                    config.InputType = "ChannelPressure";
                    config.Channel = midiData.Channel;
                    // Note: Pressure value is not stored in MappingConfigEntry
                    break;

                case MidiMessageType.PolyphonicKeyPressure:
                    config.InputType = "PolyphonicKeyPressure";
                    config.Channel = midiData.Channel;
                    config.Note = midiData.Data1 ?? 0;
                    // Note: Pressure value is not stored in MappingConfigEntry
                    break;

                default:
                    // Fallback to SysEx pattern for unknown types
                    config.InputType = "SysEx";
                    config.SysExPattern = $"F0 {midiData.RawHexData.Insert(2, " ").Insert(5, " ")} F7";
                    break;
            }

            return config;
        }

        /// <summary>
        /// Converts wildcard MIDI data to SysEx pattern format
        /// </summary>
        /// <param name="midiData">MIDI data with wildcards</param>
        /// <returns>SysEx pattern string</returns>
        private string ConvertWildcardToSysExPattern(MidiDataInfo midiData)
        {
            var hexData = midiData.RawHexData;
            var spacedHex = "";

            // Add spaces between bytes
            for (int i = 0; i < hexData.Length; i += 2)
            {
                if (i > 0) spacedHex += " ";
                spacedHex += hexData.Substring(i, 2);
            }

            // Wrap in SysEx format (F0 ... F7)
            return $"F0 {spacedHex} F7";
        }

        /// <summary>
        /// Gets the expected number of data bytes for a MIDI message type
        /// </summary>
        /// <param name="messageType">The MIDI message type</param>
        /// <returns>Number of expected data bytes</returns>
        private int GetExpectedDataBytes(MidiMessageType messageType)
        {
            return messageType switch
            {
                MidiMessageType.NoteOn => 2,
                MidiMessageType.NoteOff => 2,
                MidiMessageType.ControlChange => 2,
                MidiMessageType.ProgramChange => 1,
                MidiMessageType.ChannelPressure => 1,
                MidiMessageType.PitchBend => 2,
                MidiMessageType.PolyphonicKeyPressure => 2,
                _ => 0
            };
        }

        /// <summary>
        /// Validates that a MIDIKey2Key action can be converted to MIDI input
        /// </summary>
        /// <param name="action">The action to validate</param>
        /// <returns>Validation result with any errors</returns>
        public ConversionValidationResult ValidateMidiInputConversion(MidiKey2KeyAction action)
        {
            var result = new ConversionValidationResult { IsValid = true };

            if (string.IsNullOrWhiteSpace(action.Data))
            {
                result.IsValid = false;
                result.ErrorMessage = "Action has no MIDI data";
                return result;
            }

            if (action.Data == "STARTUP")
            {
                result.IsValid = false;
                result.ErrorMessage = "Startup actions cannot be converted to MIDI input";
                return result;
            }

            if (!_midiDataParser.IsValidHexString(action.Data))
            {
                result.IsValid = false;
                result.ErrorMessage = "Invalid hex string format";
                return result;
            }

            try
            {
                var midiData = _midiDataParser.ParseHexString(action.Data);
                if (!midiData.IsValid)
                {
                    result.IsValid = false;
                    result.ErrorMessage = midiData.ErrorMessage;
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.ErrorMessage = $"Failed to parse MIDI data: {ex.Message}";
            }

            return result;
        }
    }

    /// <summary>
    /// Result of conversion validation
    /// </summary>
    public class ConversionValidationResult
    {
        /// <summary>
        /// Gets or sets whether the conversion is valid
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if validation failed
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Gets or sets any warnings about the conversion
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
