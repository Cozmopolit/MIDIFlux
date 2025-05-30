using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Interfaces;

namespace MIDIFlux.GUI.Controls.Common
{
    /// <summary>
    /// Base class for all tab user controls in the MIDIFlux GUI
    /// </summary>
    public abstract class BaseTabUserControl : BaseUserControl, ITabControl
    {
        private string _tabTitle = string.Empty;
        private bool _hasUnsavedChanges = false;

        /// <summary>
        /// Initializes a new instance of the BaseTabUserControl class
        /// </summary>
        /// <param name="logger">The logger to use for this control</param>
        protected BaseTabUserControl(ILogger logger) : base(logger)
        {
        }

        /// <summary>
        /// Gets or sets the title of the tab
        /// </summary>
        public string TabTitle
        {
            get => _tabTitle;
            protected set
            {
                if (_tabTitle != value)
                {
                    _tabTitle = value;
                    TabTitleChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the tab has unsaved changes
        /// </summary>
        public bool HasUnsavedChanges
        {
            get => _hasUnsavedChanges;
            protected set
            {
                if (_hasUnsavedChanges != value)
                {
                    _hasUnsavedChanges = value;
                    UnsavedChangesChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Event raised when the tab's title changes
        /// </summary>
        public event EventHandler? TabTitleChanged;

        /// <summary>
        /// Event raised when the tab's unsaved changes status changes
        /// </summary>
        public event EventHandler? UnsavedChangesChanged;

        /// <summary>
        /// Activates the tab
        /// </summary>
        public virtual void Activate()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Deactivates the tab
        /// </summary>
        public virtual void Deactivate()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Saves any unsaved changes
        /// </summary>
        /// <returns>True if the save was successful, false otherwise</returns>
        public abstract bool Save();

        /// <summary>
        /// Attempts to close the tab
        /// </summary>
        /// <returns>True if the tab can be closed, false otherwise</returns>
        public virtual bool TryClose()
        {
            if (HasUnsavedChanges)
            {
                var message = $"Do you want to save changes to '{TabTitle}'?";

                // Use ApplicationErrorHandler to show the confirmation dialog and log the action
                var result = MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowUnsavedChangesConfirmation(
                    message,
                    "Unsaved Changes",
                    _logger,
                    this);

                switch (result)
                {
                    case DialogResult.Yes:
                        return Save();
                    case DialogResult.No:
                        return true;
                    case DialogResult.Cancel:
                        return false;
                    default:
                        return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Marks the control as having unsaved changes
        /// </summary>
        protected virtual void MarkDirty()
        {
            HasUnsavedChanges = true;
            OnDirtyStateChanged();
        }

        /// <summary>
        /// Marks the control as having no unsaved changes
        /// </summary>
        protected virtual void MarkClean()
        {
            HasUnsavedChanges = false;
            OnDirtyStateChanged();
        }

        /// <summary>
        /// Called when the dirty state changes. Override in derived classes to perform additional actions.
        /// </summary>
        protected virtual void OnDirtyStateChanged()
        {
            // Base implementation does nothing - derived classes can override
        }
    }
}

