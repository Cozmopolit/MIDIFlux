using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Utilities;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for generating CC value ranges from a key sequence
    /// </summary>
    public partial class CCRangeGeneratorDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private List<CCValueRange> _generatedRanges = new List<CCValueRange>();

        /// <summary>
        /// Gets the generated CC value ranges
        /// </summary>
        public List<CCValueRange> GeneratedRanges => _generatedRanges;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCRangeGeneratorDialog"/> class
        /// </summary>
        public CCRangeGeneratorDialog()
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<CCRangeGeneratorDialog>();
            _logger.LogDebug("Initializing CCRangeGeneratorDialog");

            // Initialize components
            InitializeComponent();

            // Set up event handlers
            generateButton.Click += GenerateButton_Click;
            keySequenceTextBox.TextChanged += KeySequenceTextBox_TextChanged;

            // Set default values
            minValueNumericUpDown.Value = 0;
            maxValueNumericUpDown.Value = 127;
            keySequenceTextBox.Text = "1234567890";

            // Update the UI state
            UpdateUIState();
        }

        /// <summary>
        /// Updates the UI state based on the current input
        /// </summary>
        private void UpdateUIState()
        {
            // Enable/disable the generate button based on whether there's a key sequence
            generateButton.Enabled = !string.IsNullOrWhiteSpace(keySequenceTextBox.Text);

            // Update the preview
            UpdatePreview();
        }

        /// <summary>
        /// Updates the preview of the generated ranges
        /// </summary>
        private void UpdatePreview()
        {
            try
            {
                // Clear the preview
                previewListView.Items.Clear();

                // If there's no key sequence, return
                if (string.IsNullOrWhiteSpace(keySequenceTextBox.Text))
                {
                    return;
                }

                // Get the key sequence
                string keySequence = keySequenceTextBox.Text.Trim();

                // Get the min/max values
                int minValue = (int)minValueNumericUpDown.Value;
                int maxValue = (int)maxValueNumericUpDown.Value;

                // Generate the ranges
                var ranges = CCRangeMappingGenerator.GenerateEvenlyDistributedRanges(keySequence, minValue, maxValue);

                // Add the ranges to the preview
                foreach (var range in ranges)
                {
                    var item = new ListViewItem(new string[]
                    {
                        $"{range.MinValue}-{range.MaxValue}",
                        "Key Press",
                        range.Action.Key ?? string.Empty
                    });
                    previewListView.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preview");
                ApplicationErrorHandler.ShowError("An error occurred while updating the preview.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Generates the ranges from the key sequence
        /// </summary>
        private void GenerateRanges()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Clear the generated ranges
                _generatedRanges.Clear();

                // Get the key sequence
                string keySequence = keySequenceTextBox.Text.Trim();

                // Get the min/max values
                int minValue = (int)minValueNumericUpDown.Value;
                int maxValue = (int)maxValueNumericUpDown.Value;

                // Generate the ranges
                _generatedRanges = CCRangeMappingGenerator.GenerateEvenlyDistributedRanges(keySequence, minValue, maxValue);

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "generating ranges", this);
        }

        #region Event Handlers

        /// <summary>
        /// Handles the TextChanged event of the KeySequenceTextBox
        /// </summary>
        private void KeySequenceTextBox_TextChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Handles the Click event of the GenerateButton
        /// </summary>
        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            GenerateRanges();
        }

        /// <summary>
        /// Handles the Click event of the CancelButton
        /// </summary>
        private void cancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion
    }
}
