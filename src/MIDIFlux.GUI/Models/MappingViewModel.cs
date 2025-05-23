using System;
using MIDIFlux.Core.Models;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// View model for a MIDI mapping
    /// </summary>
    public class MappingViewModel
    {
        /// <summary>
        /// Gets or sets the mapping type (Note, Absolute Control, Relative Control, etc.)
        /// </summary>
        public string MappingType { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the trigger (MIDI note or control number)
        /// </summary>
        public string Trigger { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the action type (Key, Macro, etc.)
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
        /// Gets or sets the source mapping object
        /// </summary>
        public object? SourceMapping { get; set; }
    }
}

