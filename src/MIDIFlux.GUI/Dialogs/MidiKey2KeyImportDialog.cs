using System;
using System.IO;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for importing MIDIKey2Key configuration files
    /// </summary>
    public partial class MidiKey2KeyImportDialog : BaseDialog
    {
        /// <summary>
        /// Gets the selected INI file path
        /// </summary>
        public string SelectedFilePath { get; private set; } = string.Empty;

        /// <summary>
        /// Gets the profile name for the imported configuration
        /// </summary>
        public string ProfileName { get; private set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the MidiKey2KeyImportDialog class
        /// </summary>
        public MidiKey2KeyImportDialog() : base(LoggingHelper.CreateLogger<MidiKey2KeyImportDialog>())
        {
            InitializeComponent();

            // Set dialog properties
            Text = "Import MIDIKey2Key Configuration";

            // Initialize UI state
            UpdateUI();
        }

        /// <summary>
        /// Updates the UI state based on current selections
        /// </summary>
        private void UpdateUI()
        {
            // Enable import button only if file is selected and profile name is provided
            importButton.Enabled = !string.IsNullOrWhiteSpace(filePathTextBox.Text) &&
                                  !string.IsNullOrWhiteSpace(profileNameTextBox.Text);
        }

        /// <summary>
        /// Validates the current input
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateInput()
        {
            // Check if file is selected
            if (string.IsNullOrWhiteSpace(filePathTextBox.Text))
            {
                ShowError("Please select a MIDIKey2Key INI file to import.");
                return false;
            }

            // Check if file exists
            if (!File.Exists(filePathTextBox.Text))
            {
                ShowError("The selected file does not exist.");
                return false;
            }

            // Check if profile name is provided
            if (string.IsNullOrWhiteSpace(profileNameTextBox.Text))
            {
                ShowError("Please enter a name for the imported profile.");
                profileNameTextBox.Focus();
                return false;
            }

            // Validate file extension
            var extension = Path.GetExtension(filePathTextBox.Text).ToLowerInvariant();
            if (extension != ".ini")
            {
                ShowError("Please select a valid MIDIKey2Key INI file (.ini extension).");
                return false;
            }

            // Store the results
            SelectedFilePath = filePathTextBox.Text.Trim();
            ProfileName = profileNameTextBox.Text.Trim();

            return true;
        }

        #region Event Handlers

        /// <summary>
        /// Handles the Click event of the Browse button
        /// </summary>
        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using var openFileDialog = new OpenFileDialog
            {
                Title = "Select MIDIKey2Key Configuration File",
                Filter = "MIDIKey2Key Files (*.ini)|*.ini|All Files (*.*)|*.*",
                FilterIndex = 1,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                filePathTextBox.Text = openFileDialog.FileName;

                // Auto-generate profile name from filename if not already set
                if (string.IsNullOrWhiteSpace(profileNameTextBox.Text))
                {
                    var fileName = Path.GetFileNameWithoutExtension(openFileDialog.FileName);
                    profileNameTextBox.Text = $"Imported_{fileName}";
                }

                UpdateUI();
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the file path text box
        /// </summary>
        private void FilePathTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the TextChanged event of the profile name text box
        /// </summary>
        private void ProfileNameTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateUI();
        }

        /// <summary>
        /// Handles the Click event of the Import button
        /// </summary>
        private void ImportButton_Click(object? sender, EventArgs e)
        {
            HandleOkButtonClick(ValidateInput, "importing MIDIKey2Key configuration");
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        private void CancelButton_Click(object? sender, EventArgs e)
        {
            HandleCancelButtonClick();
        }

        #endregion

        /// <summary>
        /// Called when the dialog is loaded
        /// </summary>
        protected override void OnDialogLoaded()
        {
            base.OnDialogLoaded();

            // Set focus to the browse button
            browseButton.Focus();
        }
    }
}
