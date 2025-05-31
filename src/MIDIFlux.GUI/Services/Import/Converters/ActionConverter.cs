using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Services.Import.Models;
using MIDIFlux.GUI.Services.Import.Parsers;

namespace MIDIFlux.GUI.Services.Import.Converters
{
    /// <summary>
    /// Converts MIDIKey2Key actions to MIDIFlux action configurations
    /// </summary>
    public class ActionConverter
    {
        private readonly KeyboardStringParser _keyboardParser;
        private readonly SysExConverter _sysExConverter;
        private readonly MidiDataParser _midiDataParser;

        /// <summary>
        /// Initializes a new instance of the ActionConverter class
        /// </summary>
        public ActionConverter()
        {
            _keyboardParser = new KeyboardStringParser();
            _sysExConverter = new SysExConverter();
            _midiDataParser = new MidiDataParser();
        }

        /// <summary>
        /// Converts a MIDIKey2Key action to a MIDIFlux action configuration
        /// </summary>
        /// <param name="action">The MIDIKey2Key action to convert</param>
        /// <returns>MIDIFlux action configuration, or null if not convertible</returns>
        public ActionBase? ConvertAction(MidiKey2KeyAction action)
        {
            try
            {
                // Handle different action types based on priority

                // 1. Program execution has highest priority
                if (!string.IsNullOrWhiteSpace(action.Start))
                {
                    return ConvertToCommandExecution(action);
                }

                // 2. Keyboard actions
                if (!string.IsNullOrWhiteSpace(action.Keyboard) || !string.IsNullOrWhiteSpace(action.KeyboardB))
                {
                    return ConvertToKeyboardAction(action);
                }

                // 3. MIDI output actions
                if (action.SendMidi && !string.IsNullOrWhiteSpace(action.SendMidiCommands))
                {
                    return ConvertToMidiOutput(action);
                }

                // 4. Startup actions are not convertible to regular actions
                if (action.Data == "STARTUP")
                {
                    return null;
                }

                return null; // No convertible action found
            }
            catch (Exception)
            {
                return null; // Failed to convert
            }
        }

        /// <summary>
        /// Converts to a command execution action
        /// </summary>
        /// <param name="action">The MIDIKey2Key action</param>
        /// <returns>CommandExecutionConfig action</returns>
        private ActionBase ConvertToCommandExecution(MidiKey2KeyAction action)
        {
            // Create a CommandExecutionAction using the ActionTypeRegistry
            var commandAction = ActionTypeRegistry.Instance.CreateActionInstance("CommandExecutionAction");
            if (commandAction == null)
            {
                throw new InvalidOperationException("Failed to create CommandExecutionAction instance");
            }

            // Set the description
            commandAction.Description = !string.IsNullOrWhiteSpace(action.Comment) ? action.Comment : action.Name;

            // Build the command string
            var command = action.Start;
            if (!string.IsNullOrWhiteSpace(action.Arguments))
            {
                command = $"{action.Start} {action.Arguments}";
            }

            // Set parameters
            commandAction.SetParameterValue("Command", command);
            commandAction.SetParameterValue("ShellType", "CMD"); // Default to CMD for compatibility

            // Set working directory if specified
            if (!string.IsNullOrWhiteSpace(action.WorkingDirectory))
            {
                commandAction.SetParameterValue("WorkingDirectory", action.WorkingDirectory);
            }

            return commandAction;
        }

        /// <summary>
        /// Converts to a keyboard action
        /// </summary>
        /// <param name="action">The MIDIKey2Key action</param>
        /// <returns>Keyboard action configuration</returns>
        private ActionBase? ConvertToKeyboardAction(MidiKey2KeyAction action)
        {
            var keyboardActions = new List<ActionBase>();

            // Convert primary keyboard action
            if (!string.IsNullOrWhiteSpace(action.Keyboard))
            {
                var keyboardAction = ConvertSingleKeyboardString(action.Keyboard, action);
                if (keyboardAction != null)
                {
                    keyboardActions.Add(keyboardAction);
                }
            }

            // Convert secondary keyboard action
            if (!string.IsNullOrWhiteSpace(action.KeyboardB))
            {
                var keyboardBAction = ConvertSingleKeyboardString(action.KeyboardB, action);
                if (keyboardBAction != null)
                {
                    keyboardActions.Add(keyboardBAction);
                }
            }

            // Add delay if specified
            if (action.KeyboardDelay > 0)
            {
                var delayAction = ActionTypeRegistry.Instance.CreateActionInstance("DelayAction");
                if (delayAction != null)
                {
                    delayAction.SetParameterValue("Milliseconds", action.KeyboardDelay);
                    delayAction.Description = $"Delay {action.KeyboardDelay}ms";
                    keyboardActions.Add(delayAction);
                }
            }

            // If we have multiple actions, wrap in a sequence
            if (keyboardActions.Count > 1)
            {
                var sequenceAction = ActionTypeRegistry.Instance.CreateActionInstance("SequenceAction");
                if (sequenceAction != null)
                {
                    sequenceAction.SetParameterValue("SubActions", keyboardActions);
                    sequenceAction.Description = !string.IsNullOrWhiteSpace(action.Comment) ? action.Comment : action.Name;
                    sequenceAction.SetParameterValue("ErrorHandling", "ContinueOnError");
                    return sequenceAction;
                }
            }
            else if (keyboardActions.Count == 1)
            {
                return keyboardActions[0];
            }

            return null; // No valid keyboard actions
        }

        /// <summary>
        /// Converts a single keyboard string to an action configuration
        /// </summary>
        /// <param name="keyboardString">The keyboard string to convert</param>
        /// <param name="action">The original MIDIKey2Key action for context</param>
        /// <returns>ActionBase instance</returns>
        private ActionBase? ConvertSingleKeyboardString(string keyboardString, MidiKey2KeyAction action)
        {
            var keySequence = _keyboardParser.ParseKeyboardString(keyboardString);
            if (!keySequence.IsValid)
            {
                return null;
            }

            // For simple key combinations, use KeyPressReleaseAction
            if (keySequence.MainKeys.Count == 1)
            {
                var mainKey = keySequence.MainKeys[0];
                var keyAction = ActionTypeRegistry.Instance.CreateActionInstance("KeyPressReleaseAction");
                if (keyAction == null)
                {
                    return null;
                }

                // Convert virtual key code to Keys enum
                var keysValue = (Keys)mainKey.VirtualKeyCode;
                keyAction.SetParameterValue("VirtualKeyCode", keysValue);
                keyAction.Description = $"Press {keyboardString}";

                // Add modifier keys if present
                if (keySequence.ModifierKeys.Count > 0)
                {
                    var modifierKeys = keySequence.ModifierKeys.Select(m => (Keys)m.VirtualKeyCode).ToList();
                    keyAction.SetParameterValue("ModifierKeys", modifierKeys);
                }

                return keyAction;
            }

            return null; // Complex sequences not supported yet
        }

        /// <summary>
        /// Converts to a MIDI output action
        /// </summary>
        /// <param name="action">The MIDIKey2Key action</param>
        /// <returns>MIDI output action configuration</returns>
        private ActionBase? ConvertToMidiOutput(MidiKey2KeyAction action)
        {
            try
            {
                var commands = _sysExConverter.ConvertMidiCommands(action.SendMidiCommands);
                if (commands == null || commands.Count == 0)
                {
                    return null;
                }

                // Create a MidiOutputAction using the ActionTypeRegistry
                var midiOutputAction = ActionTypeRegistry.Instance.CreateActionInstance("MidiOutputAction");
                if (midiOutputAction == null)
                {
                    return null;
                }

                // Set the description
                midiOutputAction.Description = !string.IsNullOrWhiteSpace(action.Comment) ? action.Comment : action.Name;

                // Set parameters - we'll use a default output device name that can be configured later
                midiOutputAction.SetParameterValue("OutputDeviceName", "Default MIDI Output");
                midiOutputAction.SetParameterValue("Commands", commands);

                return midiOutputAction;
            }
            catch (Exception)
            {
                return null; // Failed to convert
            }
        }

        /// <summary>
        /// Validates that a MIDIKey2Key action can be converted
        /// </summary>
        /// <param name="action">The action to validate</param>
        /// <returns>Validation result</returns>
        public ActionConversionValidationResult ValidateActionConversion(MidiKey2KeyAction action)
        {
            var result = new ActionConversionValidationResult { IsValid = true };

            // Check if action has any convertible content
            var hasProgram = !string.IsNullOrWhiteSpace(action.Start);
            var hasKeyboard = !string.IsNullOrWhiteSpace(action.Keyboard) || !string.IsNullOrWhiteSpace(action.KeyboardB);
            var hasMidiOutput = action.SendMidi && !string.IsNullOrWhiteSpace(action.SendMidiCommands);

            if (!hasProgram && !hasKeyboard && !hasMidiOutput)
            {
                result.IsValid = false;
                result.ErrorMessage = "Action has no convertible content (no program, keyboard, or MIDI output)";
                return result;
            }

            // Validate keyboard strings if present
            if (hasKeyboard)
            {
                if (!string.IsNullOrWhiteSpace(action.Keyboard) && !_keyboardParser.IsValidKeyboardString(action.Keyboard))
                {
                    result.Warnings.Add($"Invalid keyboard string: {action.Keyboard}");
                }

                if (!string.IsNullOrWhiteSpace(action.KeyboardB) && !_keyboardParser.IsValidKeyboardString(action.KeyboardB))
                {
                    result.Warnings.Add($"Invalid secondary keyboard string: {action.KeyboardB}");
                }
            }

            // Validate delay
            if (action.KeyboardDelay < 0)
            {
                result.Warnings.Add("Negative keyboard delay will be ignored");
            }

            return result;
        }
    }

    /// <summary>
    /// Result of action conversion validation
    /// </summary>
    public class ActionConversionValidationResult
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
