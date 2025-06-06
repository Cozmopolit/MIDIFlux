using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Keyboard;
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
        /// <param name="outputDeviceName">Output device name for MIDI output actions</param>
        /// <returns>MIDIFlux action configuration, or null if not convertible</returns>
        public ActionBase? ConvertAction(MidiKey2KeyAction action, string? outputDeviceName = null)
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
                    return ConvertToMidiOutput(action, outputDeviceName);
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

            // Handle Controller Action (alternating behavior) - KeyboardB is ONLY used when ControllerAction is enabled
            if (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB))
            {
                var keyboardBAction = ConvertSingleKeyboardString(action.KeyboardB, action);
                if (keyboardBAction != null && keyboardActions.Count > 0)
                {
                    // Create alternating action with primary and secondary keyboard actions
                    var alternatingAction = ActionTypeRegistry.Instance.CreateActionInstance("AlternatingAction");
                    if (alternatingAction != null)
                    {
                        alternatingAction.SetParameterValue("PrimaryAction", keyboardActions[0]);
                        alternatingAction.SetParameterValue("SecondaryAction", keyboardBAction);
                        alternatingAction.Description = $"Alternating: {action.Keyboard} / {action.KeyboardB}";
                        return alternatingAction;
                    }
                }
            }

            // Note: When ControllerAction is false, KeyboardB is completely ignored (as per MIDIKey2Key behavior)

            // Add delay if specified (only for non-alternating actions)
            if (action.KeyboardDelay > 0 && !action.ControllerAction)
            {
                var delayAction = ActionTypeRegistry.Instance.CreateActionInstance("DelayAction");
                if (delayAction != null)
                {
                    delayAction.SetParameterValue("DelayMs", action.KeyboardDelay);
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
            // Check if this is a sequence of multiple key combinations (separated by commas or semicolons)
            if (keyboardString.Contains(',') || keyboardString.Contains(';'))
            {
                return ConvertKeyboardSequence(keyboardString, action);
            }

            // Parse single key combination
            var keySequence = _keyboardParser.ParseKeyboardString(keyboardString);
            if (!keySequence.IsValid)
            {
                return null;
            }

            // For key combinations (including complex ones like Ctrl+Shift+S), use KeyPressReleaseAction
            if (keySequence.MainKeys.Count == 1)
            {
                var keyAction = CreateKeyPressReleaseAction(keySequence, keyboardString, action);

                // Apply keyboard delay if specified
                if (action.KeyboardDelay > 0 && keyAction != null)
                {
                    keyAction.SetParameterValue("AutoReleaseAfterMs", action.KeyboardDelay);
                }

                return keyAction;
            }

            // Multiple main keys not supported in a single combination
            return null;
        }

        /// <summary>
        /// Converts a keyboard sequence (multiple key combinations) to a SequenceAction
        /// </summary>
        /// <param name="keyboardString">The keyboard sequence string</param>
        /// <param name="action">The original MIDIKey2Key action for context</param>
        /// <returns>SequenceAction containing multiple key actions</returns>
        private ActionBase? ConvertKeyboardSequence(string keyboardString, MidiKey2KeyAction action)
        {
            // Split by comma or semicolon to get individual key combinations
            var separators = new char[] { ',', ';' };
            var keyParts = keyboardString.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                .Select(k => k.Trim())
                .Where(k => !string.IsNullOrEmpty(k))
                .ToList();

            if (keyParts.Count <= 1)
            {
                // Not actually a sequence, fall back to single key handling
                return ConvertSingleKeyboardString(keyboardString.Replace(",", "").Replace(";", ""), action);
            }

            var subActions = new List<ActionBase>();

            foreach (var keyPart in keyParts)
            {
                // Check if this part is a delay instruction (e.g., "DELAY:500" or "WAIT:1000")
                if (keyPart.StartsWith("DELAY:", StringComparison.OrdinalIgnoreCase) ||
                    keyPart.StartsWith("WAIT:", StringComparison.OrdinalIgnoreCase))
                {
                    var delayAction = CreateDelayAction(keyPart);
                    if (delayAction != null)
                    {
                        subActions.Add(delayAction);
                    }
                    continue;
                }

                // Parse as regular key combination
                var keySequence = _keyboardParser.ParseKeyboardString(keyPart);
                if (keySequence.IsValid && keySequence.MainKeys.Count == 1)
                {
                    var keyAction = CreateKeyPressReleaseAction(keySequence, keyPart, action);
                    if (keyAction != null)
                    {
                        subActions.Add(keyAction);
                    }
                }
            }

            if (subActions.Count == 0)
            {
                return null;
            }

            if (subActions.Count == 1)
            {
                return subActions[0];
            }

            // Create sequence action
            var sequenceAction = ActionTypeRegistry.Instance.CreateActionInstance("SequenceAction");
            if (sequenceAction != null)
            {
                sequenceAction.SetParameterValue("SubActions", subActions);
                sequenceAction.SetParameterValue("ErrorHandling", "ContinueOnError");
                sequenceAction.Description = $"Sequence: {keyboardString}";
            }

            return sequenceAction;
        }

        /// <summary>
        /// Creates a KeyPressReleaseAction from a parsed key sequence
        /// </summary>
        /// <param name="keySequence">Parsed key sequence</param>
        /// <param name="originalString">Original keyboard string for description</param>
        /// <param name="action">Original MIDIKey2Key action for context</param>
        /// <returns>KeyPressReleaseAction or null if creation failed</returns>
        private ActionBase? CreateKeyPressReleaseAction(KeyboardSequenceInfo keySequence, string originalString, MidiKey2KeyAction action)
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
            keyAction.Description = $"Press {originalString}";

            // Add modifier keys if present (supports multiple modifiers like Ctrl+Shift+S)
            if (keySequence.ModifierKeys.Count > 0)
            {
                var modifierKeys = keySequence.ModifierKeys.Select(m => (Keys)m.VirtualKeyCode).ToList();
                keyAction.SetParameterValue("ModifierKeys", modifierKeys);
            }

            return keyAction;
        }

        /// <summary>
        /// Creates a DelayAction from a delay instruction string
        /// </summary>
        /// <param name="delayString">Delay instruction (e.g., "DELAY:500")</param>
        /// <returns>DelayAction or null if parsing failed</returns>
        private ActionBase? CreateDelayAction(string delayString)
        {
            try
            {
                var parts = delayString.Split(':');
                if (parts.Length != 2)
                {
                    return null;
                }

                if (!int.TryParse(parts[1], out var delayMs) || delayMs < 0)
                {
                    return null;
                }

                var delayAction = ActionTypeRegistry.Instance.CreateActionInstance("DelayAction");
                if (delayAction != null)
                {
                    delayAction.SetParameterValue("DelayMs", delayMs);
                    delayAction.Description = $"Wait {delayMs}ms";
                }

                return delayAction;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts to a MIDI output action
        /// </summary>
        /// <param name="action">The MIDIKey2Key action</param>
        /// <param name="outputDeviceName">Output device name to use</param>
        /// <returns>MIDI output action configuration</returns>
        private ActionBase? ConvertToMidiOutput(MidiKey2KeyAction action, string? outputDeviceName)
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

                // Set parameters - use the provided output device name or fallback to default
                var deviceName = !string.IsNullOrWhiteSpace(outputDeviceName) ? outputDeviceName : "Default MIDI Output";
                midiOutputAction.SetParameterValue("OutputDeviceName", deviceName);
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
            var hasKeyboard = !string.IsNullOrWhiteSpace(action.Keyboard);
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

                // KeyboardB is only validated when ControllerAction is enabled
                if (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB))
                {
                    if (!_keyboardParser.IsValidKeyboardString(action.KeyboardB))
                    {
                        result.Warnings.Add($"Invalid secondary keyboard string: {action.KeyboardB}");
                    }
                }
                else if (!action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB))
                {
                    // Warn user that KeyboardB will be ignored when ControllerAction is disabled
                    result.Warnings.Add($"KeyboardB '{action.KeyboardB}' will be ignored because ControllerAction is disabled (matches MIDIKey2Key behavior)");
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
