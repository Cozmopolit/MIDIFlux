using System;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// View model for a unified MIDI mapping for display in the ProfileEditor
    /// </summary>
    public class MappingViewModel
    {
        /// <summary>
        /// Gets or sets the mapping type (NoteOn, NoteOff, ControlChange, etc.)
        /// </summary>
        public string MappingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trigger (MIDI note or control number)
        /// </summary>
        public string Trigger { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the channel information
        /// </summary>
        public string Channel { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string Device { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action type (KeyPressRelease, MouseClick, etc.)
        /// </summary>
        public string ActionType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action details
        /// </summary>
        public string ActionDetails { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets whether this mapping is enabled
        /// </summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>
        /// Gets or sets the source mapping configuration entry
        /// </summary>
        public MappingConfigEntry? SourceMapping { get; set; }

        /// <summary>
        /// Creates a MappingViewModel from a MappingConfigEntry
        /// </summary>
        /// <param name="mappingEntry">The mapping configuration entry</param>
        /// <returns>A new MappingViewModel</returns>
        public static MappingViewModel FromMappingEntry(MappingConfigEntry mappingEntry)
        {
            if (mappingEntry == null) throw new ArgumentNullException(nameof(mappingEntry));

            var viewModel = new MappingViewModel
            {
                MappingType = mappingEntry.InputType,
                Channel = mappingEntry.Channel?.ToString() ?? "Any",
                Device = "Any Device", // Will be set by the ProfileEditor based on the device context
                Description = mappingEntry.Description ?? string.Empty,
                IsEnabled = mappingEntry.IsEnabled,
                SourceMapping = mappingEntry
            };

            // Set trigger based on input type
            viewModel.Trigger = mappingEntry.InputType switch
            {
                "NoteOn" or "NoteOff" => $"Note {mappingEntry.Note}",
                "ControlChange" or "RelativeControlChange" => $"CC {mappingEntry.ControlNumber}",
                _ => "Unknown"
            };

            // Set action type and details based on the action configuration
            if (mappingEntry.Action != null)
            {
                viewModel.ActionType = GetActionTypeName(mappingEntry.Action);
                viewModel.ActionDetails = GetActionDetails(mappingEntry.Action);
            }
            else
            {
                viewModel.ActionType = "None";
                viewModel.ActionDetails = "No action configured";
            }

            return viewModel;
        }

        /// <summary>
        /// Gets a user-friendly name for an action configuration type using the registry
        /// </summary>
        /// <param name="actionConfig">The action configuration</param>
        /// <returns>A user-friendly action type name</returns>
        private static string GetActionTypeName(ActionConfig actionConfig)
        {
            return ActionRegistry.GetDisplayName(actionConfig);
        }

        /// <summary>
        /// Gets detailed information about an action configuration
        /// </summary>
        /// <param name="actionConfig">The action configuration</param>
        /// <returns>A string with action details</returns>
        private static string GetActionDetails(ActionConfig actionConfig)
        {
            return actionConfig switch
            {
                KeyPressReleaseConfig keyConfig => $"Key: {GetKeyName(keyConfig.VirtualKeyCode)}",
                KeyDownConfig keyDownConfig => $"Key: {GetKeyName(keyDownConfig.VirtualKeyCode)}" +
                    (keyDownConfig.AutoReleaseAfterMs.HasValue ? $" (Auto-release: {keyDownConfig.AutoReleaseAfterMs}ms)" : ""),
                KeyUpConfig keyUpConfig => $"Key: {GetKeyName(keyUpConfig.VirtualKeyCode)}",
                KeyToggleConfig keyToggleConfig => $"Key: {GetKeyName(keyToggleConfig.VirtualKeyCode)}",
                MouseClickConfig mouseConfig => $"Button: {mouseConfig.Button}",
                MouseScrollConfig scrollConfig => $"Direction: {scrollConfig.Direction}, Amount: {scrollConfig.Amount}",
                CommandExecutionConfig cmdConfig => $"Command: {cmdConfig.Command} ({cmdConfig.ShellType})",
                DelayConfig delayConfig => $"Duration: {delayConfig.Milliseconds}ms",
                GameControllerButtonConfig gameButtonConfig => $"Button: {gameButtonConfig.Button} (Controller {gameButtonConfig.ControllerIndex})",
                GameControllerAxisConfig gameAxisConfig => $"Axis: {gameAxisConfig.AxisName} = {gameAxisConfig.AxisValue} (Controller {gameAxisConfig.ControllerIndex})",
                SequenceConfig seqConfig => $"{seqConfig.SubActions.Count} actions, Error handling: {seqConfig.ErrorHandling}",
                ConditionalConfig condConfig => $"{condConfig.Conditions.Count} conditions",
                _ => "Unknown action type"
            };
        }

        /// <summary>
        /// Gets a user-friendly name for a virtual key code
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code</param>
        /// <returns>A user-friendly key name</returns>
        private static string GetKeyName(ushort virtualKeyCode)
        {
            // Convert to System.Windows.Forms.Keys enum for display
            try
            {
                var key = (System.Windows.Forms.Keys)virtualKeyCode;
                return key.ToString();
            }
            catch
            {
                return $"Key {virtualKeyCode}";
            }
        }

        /// <summary>
        /// Returns a string representation of this mapping
        /// </summary>
        /// <returns>A string describing this mapping</returns>
        public override string ToString()
        {
            return $"{MappingType} {Trigger} -> {ActionType}: {ActionDetails}";
        }
    }
}
