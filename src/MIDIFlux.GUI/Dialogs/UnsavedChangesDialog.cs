using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Dialogs;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for prompting the user about unsaved changes
    /// </summary>
    public partial class UnsavedChangesDialog : BaseDialog
    {
        /// <summary>
        /// Gets the result of the dialog
        /// </summary>
        public UnsavedChangesResult Result { get; private set; } = UnsavedChangesResult.Cancel;

        /// <summary>
        /// Initializes a new instance of the <see cref="UnsavedChangesDialog"/> class
        /// </summary>
        /// <param name="itemName">The name of the item with unsaved changes</param>
        /// <param name="logger">The logger to use for this dialog</param>
        public UnsavedChangesDialog(string itemName, ILogger<UnsavedChangesDialog> logger) : base(logger)
        {
            InitializeComponent();

            // Set the dialog text
            lblMessage.Text = $"Do you want to save changes to '{itemName}'?";
        }

        /// <summary>
        /// Handles the Click event of the Save button
        /// </summary>
        private void btnSave_Click(object sender, EventArgs e)
        {
            Result = UnsavedChangesResult.Save;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Don't Save button
        /// </summary>
        private void btnDontSave_Click(object sender, EventArgs e)
        {
            Result = UnsavedChangesResult.DontSave;
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            Result = UnsavedChangesResult.Cancel;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Shows the dialog and returns the result
        /// </summary>
        /// <param name="itemName">The name of the item with unsaved changes</param>
        /// <returns>The result of the dialog</returns>
        public static UnsavedChangesResult Show(string itemName)
        {
            var logger = LoggingHelper.CreateLogger<UnsavedChangesDialog>();
            using var dialog = new UnsavedChangesDialog(itemName, logger);
            dialog.ShowDialog();
            return dialog.Result;
        }
    }

    /// <summary>
    /// Represents the result of an unsaved changes dialog
    /// </summary>
    public enum UnsavedChangesResult
    {
        /// <summary>
        /// Save the changes
        /// </summary>
        Save,

        /// <summary>
        /// Don't save the changes
        /// </summary>
        DontSave,

        /// <summary>
        /// Cancel the operation
        /// </summary>
        Cancel
    }
}

