using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing a CC value range
    /// </summary>
    public partial class CCRangeEditDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly CCValueRange _range;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the edited CC value range
        /// </summary>
        public CCValueRange Range => _range;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCRangeEditDialog"/> class
        /// </summary>
        /// <param name="range">The CC value range to edit</param>
        public CCRangeEditDialog(CCValueRange range)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<CCRangeEditDialog>();
            _logger.LogDebug("Initializing CCRangeEditDialog");

            // Store the range
            _range = range ?? throw new ArgumentNullException(nameof(range));

            // Initialize components
            InitializeComponent();

            // Set up event handlers
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;
            keyTextBox.KeyDown += KeyTextBox_KeyDown;
            selectKeyButton.Click += SelectKeyButton_Click;

            // Load the range data
            LoadRangeData();
        }

        /// <summary>
        /// Loads the range data into the UI
        /// </summary>
        private void LoadRangeData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Set the min/max values
                    minValueNumericUpDown.Value = _range.MinValue;
                    maxValueNumericUpDown.Value = _range.MaxValue;

                    // Set the action type
                    actionTypeComboBox.SelectedIndex = (int)_range.Action.Type;

                    // Set the action details based on the type
                    switch (_range.Action.Type)
                    {
                        case CCRangeActionType.KeyPress:
                            keyTextBox.Text = _range.Action.Key ?? string.Empty;
                            break;
                        case CCRangeActionType.CommandExecution:
                            commandTextBox.Text = _range.Action.Command ?? string.Empty;
                            break;

                    }

                    // Update the UI based on the action type
                    UpdateUIForActionType();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading CC value range data", this);
        }

        /// <summary>
        /// Updates the UI based on the selected action type
        /// </summary>
        private void UpdateUIForActionType()
        {
            if (_updatingUI)
                return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Get the selected action type
                    var actionType = (CCRangeActionType)actionTypeComboBox.SelectedIndex;

                    // Show/hide the appropriate panels
                    keyPanel.Visible = actionType == CCRangeActionType.KeyPress;
                    commandPanel.Visible = actionType == CCRangeActionType.CommandExecution;

                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "updating UI for action type", this);
        }

        /// <summary>
        /// Saves the range data from the UI
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool SaveRangeData()
        {
            try
            {
                // Get the min/max values
                _range.MinValue = (int)minValueNumericUpDown.Value;
                _range.MaxValue = (int)maxValueNumericUpDown.Value;

                // Get the action type
                _range.Action.Type = (CCRangeActionType)actionTypeComboBox.SelectedIndex;

                // Get the action details based on the type
                switch (_range.Action.Type)
                {
                    case CCRangeActionType.KeyPress:
                        _range.Action.Key = keyTextBox.Text.Trim();
                        _range.Action.Command = null;
                        break;
                    case CCRangeActionType.CommandExecution:
                        _range.Action.Key = null;
                        _range.Action.Command = commandTextBox.Text.Trim();
                        break;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving CC value range data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the range data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Validates the range data
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateRange()
        {
            try
            {
                // Check if min value is less than or equal to max value
                if (minValueNumericUpDown.Value > maxValueNumericUpDown.Value)
                {
                    ApplicationErrorHandler.ShowError("Minimum value must be less than or equal to maximum value.", "Validation Error", _logger, null, this);
                    minValueNumericUpDown.Focus();
                    return false;
                }

                // Check if the action details are valid based on the type
                switch ((CCRangeActionType)actionTypeComboBox.SelectedIndex)
                {
                    case CCRangeActionType.KeyPress:
                        if (string.IsNullOrWhiteSpace(keyTextBox.Text))
                        {
                            ApplicationErrorHandler.ShowError("Please enter a key.", "Validation Error", _logger, null, this);
                            keyTextBox.Focus();
                            return false;
                        }
                        break;
                    case CCRangeActionType.CommandExecution:
                        if (string.IsNullOrWhiteSpace(commandTextBox.Text))
                        {
                            ApplicationErrorHandler.ShowError("Please enter a command.", "Validation Error", _logger, null, this);
                            commandTextBox.Focus();
                            return false;
                        }
                        break;

                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CC value range data");
                ApplicationErrorHandler.ShowError("An error occurred while validating the range data.", "Error", _logger, ex, this);
                return false;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ActionTypeComboBox
        /// </summary>
        private void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIForActionType();
        }

        /// <summary>
        /// Handles the KeyDown event of the KeyTextBox
        /// </summary>
        private void KeyTextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Capture the key
                keyTextBox.Text = e.KeyCode.ToString();

                // Prevent the key from being processed further
                e.SuppressKeyPress = true;
                e.Handled = true;
            }, _logger, "capturing key", this);
        }

        /// <summary>
        /// Handles the Click event of the SelectKeyButton
        /// </summary>
        private void SelectKeyButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Show a message to press a key
                ApplicationErrorHandler.ShowInformation("Press a key to select it.", "Select Key", _logger, this);

                // Focus the key text box
                keyTextBox.Focus();
            }, _logger, "selecting key", this);
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the range data
                if (!SaveRangeData())
                    return;

                // Validate the range data
                if (!ValidateRange())
                    return;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving CC value range", this);
        }

        #endregion
    }
}
