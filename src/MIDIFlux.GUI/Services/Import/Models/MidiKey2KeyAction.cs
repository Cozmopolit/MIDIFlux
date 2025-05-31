using System;
using System.Collections.Generic;

namespace MIDIFlux.GUI.Services.Import.Models
{
    /// <summary>
    /// Represents a single action from a MIDIKey2Key configuration
    /// </summary>
    public class MidiKey2KeyAction
    {
        /// <summary>
        /// Gets or sets the action name
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Gets or sets the action comment/description
        /// </summary>
        public string Comment { get; set; } = "";

        /// <summary>
        /// Gets or sets the MIDI data that triggers this action (hex string)
        /// </summary>
        public string Data { get; set; } = "";

        /// <summary>
        /// Gets or sets the primary keyboard action
        /// </summary>
        public string Keyboard { get; set; } = "";

        /// <summary>
        /// Gets or sets the secondary keyboard action
        /// </summary>
        public string KeyboardB { get; set; } = "";

        /// <summary>
        /// Gets or sets the keyboard delay in milliseconds
        /// </summary>
        public int KeyboardDelay { get; set; }

        /// <summary>
        /// Gets or sets whether this action should wait for note off (hold mode)
        /// </summary>
        public bool Hold { get; set; }

        /// <summary>
        /// Gets or sets whether to send MIDI output
        /// </summary>
        public bool SendMidi { get; set; }

        /// <summary>
        /// Gets or sets the MIDI commands to send (hex string)
        /// </summary>
        public string SendMidiCommands { get; set; } = "";

        /// <summary>
        /// Gets or sets the controller action flag
        /// </summary>
        public bool ControllerAction { get; set; }

        /// <summary>
        /// Gets or sets the program to start
        /// </summary>
        public string Start { get; set; } = "";

        /// <summary>
        /// Gets or sets the working directory for program execution
        /// </summary>
        public string WorkingDirectory { get; set; } = "";

        /// <summary>
        /// Gets or sets the command line arguments for program execution
        /// </summary>
        public string Arguments { get; set; } = "";

        /// <summary>
        /// Gets or sets the line number in the source INI file
        /// </summary>
        public int SourceLineNumber { get; set; }

        /// <summary>
        /// Gets or sets the section name in the INI file (e.g., "Action0", "Action1")
        /// </summary>
        public string SectionName { get; set; } = "";

        /// <summary>
        /// Gets the type of this action based on its properties
        /// </summary>
        public MidiKey2KeyActionType ActionType
        {
            get
            {
                if (Data == "STARTUP")
                    return MidiKey2KeyActionType.Startup;

                if (!string.IsNullOrWhiteSpace(Start))
                    return MidiKey2KeyActionType.ProgramExecution;

                if (SendMidi && !string.IsNullOrWhiteSpace(SendMidiCommands))
                    return MidiKey2KeyActionType.MidiOutput;

                if (!string.IsNullOrWhiteSpace(Keyboard) || !string.IsNullOrWhiteSpace(KeyboardB))
                    return MidiKey2KeyActionType.Keyboard;

                if (!string.IsNullOrWhiteSpace(Data))
                    return MidiKey2KeyActionType.MidiInput;

                return MidiKey2KeyActionType.Unknown;
            }
        }

        /// <summary>
        /// Gets whether this action contains SysEx data
        /// </summary>
        public bool IsSysEx
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Data))
                    return false;

                var cleanData = Data.Replace(" ", "").ToUpperInvariant();
                return cleanData.StartsWith("F0") && cleanData.EndsWith("F7");
            }
        }

        /// <summary>
        /// Gets whether this action contains wildcard patterns
        /// </summary>
        public bool HasWildcards
        {
            get
            {
                return !string.IsNullOrWhiteSpace(Data) &&
                       (Data.Contains("xx", StringComparison.OrdinalIgnoreCase) ||
                        Data.Contains("XX"));
            }
        }

        /// <summary>
        /// Gets validation errors for this action
        /// </summary>
        /// <returns>List of validation error messages</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(Name))
            {
                errors.Add("Action name is required");
            }

            if (ActionType == MidiKey2KeyActionType.Unknown)
            {
                errors.Add("Action has no recognizable type (no data, keyboard, or program execution)");
            }

            if (ActionType == MidiKey2KeyActionType.MidiInput && string.IsNullOrWhiteSpace(Data))
            {
                errors.Add("MIDI input action requires data field");
            }

            if (ActionType == MidiKey2KeyActionType.ProgramExecution && string.IsNullOrWhiteSpace(Start))
            {
                errors.Add("Program execution action requires start field");
            }

            if (KeyboardDelay < 0)
            {
                errors.Add("Keyboard delay cannot be negative");
            }

            return errors;
        }
    }

    /// <summary>
    /// Types of MIDIKey2Key actions
    /// </summary>
    public enum MidiKey2KeyActionType
    {
        /// <summary>
        /// Unknown or unrecognized action type
        /// </summary>
        Unknown,

        /// <summary>
        /// Startup action
        /// </summary>
        Startup,

        /// <summary>
        /// MIDI input trigger
        /// </summary>
        MidiInput,

        /// <summary>
        /// Keyboard action
        /// </summary>
        Keyboard,

        /// <summary>
        /// MIDI output action
        /// </summary>
        MidiOutput,

        /// <summary>
        /// Program execution
        /// </summary>
        ProgramExecution
    }
}
