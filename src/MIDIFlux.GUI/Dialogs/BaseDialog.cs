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
        protected readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the BaseDialog class
        /// </summary>
        /// <param name="logger">The logger to use for this dialog</param>
        protected BaseDialog(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

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
            return ApplicationErrorHandler.ShowInformation(text, caption, _logger, this);
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
            return ApplicationErrorHandler.ShowError(text, caption, _logger, exception, this);
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

        #region Unified Dialog Patterns

        /// <summary>
        /// Unified OK button click handler that validates and closes the dialog
        /// </summary>
        /// <param name="validationMethod">The validation method to call before closing</param>
        /// <param name="operationDescription">Description of the operation for logging</param>
        protected void HandleOkButtonClick(Func<bool> validationMethod, string operationDescription)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (validationMethod())
                {
                    DialogResult = DialogResult.OK;
                }
            }, _logger, operationDescription, this);
        }

        /// <summary>
        /// Unified Cancel button click handler
        /// </summary>
        protected void HandleCancelButtonClick()
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Unified validation error display method
        /// </summary>
        /// <param name="validationErrors">List of validation errors</param>
        /// <param name="itemName">Name of the item being validated</param>
        /// <returns>True if no errors, false if errors were found and displayed</returns>
        protected bool DisplayValidationErrors(List<string> validationErrors, string itemName)
        {
            if (validationErrors.Count > 0)
            {
                var errorMessage = $"The {itemName} configuration has the following errors:\n\n" +
                                 string.Join("\n", validationErrors);
                ShowError(errorMessage, "Validation Error");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Unified validation method for configurations that have GetValidationErrors method
        /// </summary>
        /// <param name="configObject">Object with GetValidationErrors method</param>
        /// <param name="itemName">Name of the item being validated</param>
        /// <param name="additionalValidation">Optional additional validation logic</param>
        /// <returns>True if valid, false otherwise</returns>
        protected bool ValidateConfigurationObject(dynamic configObject, string itemName, Func<bool>? additionalValidation = null)
        {
            var errors = configObject.GetValidationErrors() as List<string> ?? new List<string>();
            if (!DisplayValidationErrors(errors, itemName))
                return false;

            return additionalValidation?.Invoke() ?? true;
        }

        #endregion
    }
}

