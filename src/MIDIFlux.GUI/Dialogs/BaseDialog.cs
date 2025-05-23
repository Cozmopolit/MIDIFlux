using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Base class for all dialogs in the MIDIFlux GUI
    /// </summary>
    public abstract class BaseDialog : Form
    {
        /// <summary>
        /// Gets a logger for the current dialog
        /// </summary>
        /// <returns>A logger for the current dialog</returns>
        protected ILogger GetLogger()
        {
            return LoggingHelper.CreateLogger(GetType());
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseDialog"/> class
        /// </summary>
        protected BaseDialog()
        {
            // Set common dialog properties
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            // Set up event handlers
            Load += BaseDialog_Load;
        }

        /// <summary>
        /// Handles the Load event of the BaseDialog
        /// </summary>
        private void BaseDialog_Load(object? sender, EventArgs e)
        {
            OnDialogLoaded();
        }

        /// <summary>
        /// Called when the dialog is loaded
        /// </summary>
        protected virtual void OnDialogLoaded()
        {
            // Base implementation does nothing
        }

        /// <summary>
        /// Runs an action on the UI thread
        /// </summary>
        /// <param name="action">The action to run</param>
        protected void RunOnUI(Action action)
        {
            UISynchronizationHelper.RunOnUI(action);
        }

        /// <summary>
        /// Shows a message box with the specified text
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="caption">The caption to display</param>
        /// <returns>The result of the message box</returns>
        protected DialogResult ShowMessage(string text, string caption = "MIDIFlux")
        {
            var logger = GetLogger();
            return ApplicationErrorHandler.ShowInformation(text, caption, logger, this);
        }

        /// <summary>
        /// Shows an error message box with the specified text
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="caption">The caption to display</param>
        /// <param name="exception">Optional exception that caused the error</param>
        /// <returns>The result of the message box</returns>
        protected DialogResult ShowError(string text, string caption = "Error", Exception? exception = null)
        {
            var logger = GetLogger();
            return ApplicationErrorHandler.ShowError(text, caption, logger, exception, this);
        }

        /// <summary>
        /// Shows a warning message box with the specified text
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="caption">The caption to display</param>
        /// <returns>The result of the message box</returns>
        protected DialogResult ShowWarning(string text, string caption = "Warning")
        {
            var logger = GetLogger();
            return ApplicationErrorHandler.ShowWarning(text, caption, logger, this);
        }

        /// <summary>
        /// Shows a confirmation message box with the specified text
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="caption">The caption to display</param>
        /// <param name="defaultResult">The default result to return in silent mode</param>
        /// <returns>True if the user clicked Yes, false otherwise</returns>
        protected bool ShowConfirmation(string text, string caption = "Confirmation", bool defaultResult = true)
        {
            var logger = GetLogger();
            var result = ApplicationErrorHandler.ShowConfirmation(
                text,
                caption,
                logger,
                defaultResult ? DialogResult.Yes : DialogResult.No,
                this);

            return result == DialogResult.Yes;
        }
    }
}

