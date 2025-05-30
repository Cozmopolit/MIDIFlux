using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Services;

namespace MIDIFlux.GUI.Controls.Common
{
    /// <summary>
    /// Base class for all user controls in the MIDIFlux GUI
    /// </summary>
    public abstract class BaseUserControl : UserControl
    {
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BaseUserControl class
        /// </summary>
        /// <param name="logger">The logger to use for this control</param>
        protected BaseUserControl(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Set up event handlers
            Load += BaseUserControl_Load;
        }

        /// <summary>
        /// Handles the Load event of the BaseUserControl
        /// </summary>
        private void BaseUserControl_Load(object? sender, EventArgs e)
        {
            OnControlLoaded();
        }

        /// <summary>
        /// Called when the control is loaded
        /// </summary>
        protected virtual void OnControlLoaded()
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
            return MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowInformation(text, caption, _logger, this);
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
            return MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(text, caption, _logger, exception, this);
        }

        /// <summary>
        /// Shows a warning message box with the specified text
        /// </summary>
        /// <param name="text">The text to display</param>
        /// <param name="caption">The caption to display</param>
        /// <returns>The result of the message box</returns>
        protected DialogResult ShowWarning(string text, string caption = "Warning")
        {
            return ApplicationErrorHandler.ShowWarning(text, caption, _logger, this);
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
            var result = ApplicationErrorHandler.ShowConfirmation(
                text,
                caption,
                _logger,
                defaultResult ? DialogResult.Yes : DialogResult.No,
                this);

            return result == DialogResult.Yes;
        }

        /// <summary>
        /// Shows validation errors and warnings
        /// </summary>
        /// <param name="validationResult">The validation result</param>
        /// <param name="caption">The caption to display</param>
        /// <returns>The dialog result</returns>
        protected DialogResult ShowValidationResult(ValidationResult validationResult, string caption = "Validation")
        {
            return ApplicationErrorHandler.ShowValidationError(validationResult, caption, _logger, this);
        }
    }
}

