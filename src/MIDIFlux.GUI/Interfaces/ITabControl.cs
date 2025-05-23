using System;

namespace MIDIFlux.GUI.Interfaces
{
    /// <summary>
    /// Interface for tab controls in the MIDIFlux GUI
    /// </summary>
    public interface ITabControl
    {
        /// <summary>
        /// Gets the title of the tab
        /// </summary>
        string TabTitle { get; }

        /// <summary>
        /// Gets a value indicating whether the tab has unsaved changes
        /// </summary>
        bool HasUnsavedChanges { get; }

        /// <summary>
        /// Event raised when the tab's title changes
        /// </summary>
        event EventHandler? TabTitleChanged;

        /// <summary>
        /// Event raised when the tab's unsaved changes status changes
        /// </summary>
        event EventHandler? UnsavedChangesChanged;

        /// <summary>
        /// Activates the tab
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates the tab
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Saves any unsaved changes
        /// </summary>
        /// <returns>True if the save was successful, false otherwise</returns>
        bool Save();

        /// <summary>
        /// Attempts to close the tab
        /// </summary>
        /// <returns>True if the tab can be closed, false otherwise</returns>
        bool TryClose();
    }
}

