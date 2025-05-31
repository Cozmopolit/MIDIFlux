using System.Collections.Generic;

namespace MIDIFlux.GUI.Services.Import.Models
{
    /// <summary>
    /// Represents a complete MIDIKey2Key configuration
    /// </summary>
    public class MidiKey2KeyConfig
    {
        /// <summary>
        /// Gets or sets the list of actions in the configuration
        /// </summary>
        public List<MidiKey2KeyAction> Actions { get; set; } = new();

        /// <summary>
        /// Gets or sets the device settings
        /// </summary>
        public Dictionary<string, string> DeviceSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the global settings
        /// </summary>
        public Dictionary<string, string> GlobalSettings { get; set; } = new();

        /// <summary>
        /// Gets or sets the source file path
        /// </summary>
        public string SourceFilePath { get; set; } = "";

        /// <summary>
        /// Gets validation errors for this configuration
        /// </summary>
        /// <returns>List of validation error messages</returns>
        public List<string> GetValidationErrors()
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(SourceFilePath))
            {
                errors.Add("Source file path is required");
            }

            if (Actions.Count == 0)
            {
                errors.Add("Configuration must contain at least one action");
            }

            // Validate each action
            for (int i = 0; i < Actions.Count; i++)
            {
                var actionErrors = Actions[i].GetValidationErrors();
                foreach (var error in actionErrors)
                {
                    errors.Add($"Action {i}: {error}");
                }
            }

            return errors;
        }
    }
}
