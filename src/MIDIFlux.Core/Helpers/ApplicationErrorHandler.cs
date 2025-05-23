using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Handles application errors including UI error display and critical exception recovery
    /// Provides unified error handling with logging, user notification, and application restart capabilities
    /// </summary>
    public static class ApplicationErrorHandler
    {
        /// <summary>
        /// Gets or sets a value indicating whether silent mode is enabled
        /// When silent mode is enabled, no message boxes are shown and all errors are only logged
        /// </summary>
        public static bool SilentMode { get; set; } = false;



        /// <summary>
        /// Displays an error message with logging
        /// </summary>
        /// <param name="message">The error message</param>
        /// <param name="title">The title of the error dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="exception">Optional exception that caused the error</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result</returns>
        public static DialogResult ShowError(string message, string title, ILogger logger, Exception? exception = null, IWin32Window? parent = null)
        {
            // Log the error
            if (exception != null)
            {
                logger.LogError(exception, message);
            }
            else
            {
                logger.LogError(message);
            }

            // If silent mode is enabled, don't show a message box
            if (SilentMode)
            {
                return DialogResult.OK;
            }

            try
            {
                // Display the error message
                return MessageBox.Show(
                    parent,
                    message,
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return OK
                logger.LogError(ex, "Failed to show error message box: {Message}", ex.Message);
                return DialogResult.OK;
            }
        }

        /// <summary>
        /// Displays a warning message with logging
        /// </summary>
        /// <param name="message">The warning message</param>
        /// <param name="title">The title of the warning dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result</returns>
        public static DialogResult ShowWarning(string message, string title, ILogger logger, IWin32Window? parent = null)
        {
            // Log the warning
            logger.LogWarning(message);

            // If silent mode is enabled, don't show a message box
            if (SilentMode)
            {
                return DialogResult.OK;
            }

            try
            {
                // Display the warning message
                return MessageBox.Show(
                    parent,
                    message,
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return OK
                logger.LogError(ex, "Failed to show warning message box: {Message}", ex.Message);
                return DialogResult.OK;
            }
        }

        /// <summary>
        /// Displays an information message with logging
        /// </summary>
        /// <param name="message">The information message</param>
        /// <param name="title">The title of the information dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result</returns>
        public static DialogResult ShowInformation(string message, string title, ILogger logger, IWin32Window? parent = null)
        {
            // Log the information
            logger.LogInformation(message);

            // If silent mode is enabled, don't show a message box
            if (SilentMode)
            {
                return DialogResult.OK;
            }

            try
            {
                // Display the information message
                return MessageBox.Show(
                    parent,
                    message,
                    title,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return OK
                logger.LogError(ex, "Failed to show information message box: {Message}", ex.Message);
                return DialogResult.OK;
            }
        }

        /// <summary>
        /// Displays a confirmation dialog with logging
        /// </summary>
        /// <param name="message">The confirmation message</param>
        /// <param name="title">The title of the confirmation dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="defaultResult">The default result to return in silent mode</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result</returns>
        public static DialogResult ShowConfirmation(string message, string title, ILogger logger, DialogResult defaultResult = DialogResult.Yes, IWin32Window? parent = null)
        {
            // Log the confirmation request
            logger.LogInformation("Confirmation requested: {Message}", message);

            // If silent mode is enabled, return the default result
            if (SilentMode)
            {
                logger.LogInformation("Silent mode enabled, returning default result: {Result}", defaultResult);
                return defaultResult;
            }

            try
            {
                // Display the confirmation dialog
                var result = MessageBox.Show(
                    parent,
                    message,
                    title,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                // Log the result
                logger.LogInformation("Confirmation result: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return the default result
                logger.LogError(ex, "Failed to show confirmation message box: {Message}", ex.Message);
                return defaultResult;
            }
        }

        /// <summary>
        /// Displays a validation error dialog with detailed information
        /// </summary>
        /// <param name="validationResult">The validation result</param>
        /// <param name="title">The title of the validation error dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result</returns>
        public static DialogResult ShowValidationError(ValidationResult validationResult, string title, ILogger logger, IWin32Window? parent = null)
        {
            // Build the validation error message
            var messageBuilder = new StringBuilder();

            if (validationResult.HasErrors)
            {
                messageBuilder.AppendLine("The following errors were found:");
                messageBuilder.AppendLine();

                foreach (var error in validationResult.Errors)
                {
                    messageBuilder.AppendLine($"• {error}");
                }

                messageBuilder.AppendLine();
            }

            if (validationResult.HasWarnings)
            {
                if (validationResult.HasErrors)
                {
                    messageBuilder.AppendLine("Additionally, the following warnings were found:");
                }
                else
                {
                    messageBuilder.AppendLine("The following warnings were found:");
                }

                messageBuilder.AppendLine();

                foreach (var warning in validationResult.Warnings)
                {
                    messageBuilder.AppendLine($"• {warning}");
                }
            }

            // Log the validation errors and warnings
            if (validationResult.HasErrors)
            {
                logger.LogError("Validation errors: {ErrorCount}", validationResult.Errors.Count);
                foreach (var error in validationResult.Errors)
                {
                    logger.LogError("Validation error: {Error}", error);
                }
            }

            if (validationResult.HasWarnings)
            {
                logger.LogWarning("Validation warnings: {WarningCount}", validationResult.Warnings.Count);
                foreach (var warning in validationResult.Warnings)
                {
                    logger.LogWarning("Validation warning: {Warning}", warning);
                }
            }

            // If silent mode is enabled, don't show a message box
            if (SilentMode)
            {
                return DialogResult.OK;
            }

            try
            {
                // Display the validation error dialog
                return MessageBox.Show(
                    parent,
                    messageBuilder.ToString(),
                    title,
                    MessageBoxButtons.OK,
                    validationResult.HasErrors ? MessageBoxIcon.Error : MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return OK
                logger.LogError(ex, "Failed to show validation error message box: {Message}", ex.Message);
                return DialogResult.OK;
            }
        }

        /// <summary>
        /// Displays an unsaved changes confirmation dialog with logging
        /// </summary>
        /// <param name="message">The confirmation message</param>
        /// <param name="title">The title of the confirmation dialog</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>The dialog result (Yes, No, or Cancel)</returns>
        public static DialogResult ShowUnsavedChangesConfirmation(string message, string title, ILogger logger, IWin32Window? parent = null)
        {
            // Log the unsaved changes confirmation request
            logger.LogInformation("Unsaved changes confirmation requested: {Message}", message);

            // If silent mode is enabled, return Yes (save changes)
            if (SilentMode)
            {
                logger.LogInformation("Silent mode enabled, returning default result: Yes");
                return DialogResult.Yes;
            }

            try
            {
                // Display the confirmation dialog
                var result = MessageBox.Show(
                    parent,
                    message,
                    title,
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                // Log the result
                logger.LogInformation("Unsaved changes confirmation result: {Result}", result);

                return result;
            }
            catch (Exception ex)
            {
                // If showing the message box fails, log the error and return Yes
                logger.LogError(ex, "Failed to show unsaved changes confirmation message box: {Message}", ex.Message);
                return DialogResult.Yes;
            }
        }

        /// <summary>
        /// Executes an action with unified UI error handling
        /// </summary>
        /// <param name="action">The action to execute</param>
        /// <param name="logger">The logger to use</param>
        /// <param name="operationName">The name of the operation being performed</param>
        /// <param name="parent">Optional parent control for the message box</param>
        /// <returns>True if the action executed successfully, false if an exception occurred</returns>
        public static bool RunWithUiErrorHandling(Action action, ILogger logger, string operationName, IWin32Window? parent = null)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception ex)
            {
                // Log with full details including call stack
                logger.LogError(ex, "Error while {OperationName}: {ErrorMessage}", operationName, ex.Message);

                // Show detailed user message
                ShowError(
                    $"An unexpected error occurred while {operationName}:\n\n{ex.Message}\n\nPlease check the logs for more details.",
                    "MIDIFlux - Error",
                    logger,
                    ex,
                    parent);
                return false;
            }
        }

        /// <summary>
        /// Handles a critical exception that would otherwise crash the application
        /// </summary>
        /// <param name="exception">The exception that occurred</param>
        /// <param name="source">The source of the exception (e.g., "AppDomain", "UI thread")</param>
        /// <param name="logger">The logger to use</param>
        public static void HandleCriticalException(Exception? exception, string source, ILogger logger)
        {
            // Get the error message
            string errorMessage = exception?.Message ?? "Unknown error";

            // Create the error message
            string fullMessage = $"A critical error occurred in MIDIFlux:\n\n{errorMessage}\n\nThe application will now close. Please check the logs for more details.";

            // Log the critical error
            logger.LogCritical(exception, "Unhandled exception in {Source}: {Message}", source, errorMessage);

            // Try to show the error message
            try
            {
                // Display the error message
                MessageBox.Show(
                    fullMessage,
                    "MIDIFlux - Critical Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            catch
            {
                // If showing the message box fails, there's not much we can do
            }
        }
    }
}
