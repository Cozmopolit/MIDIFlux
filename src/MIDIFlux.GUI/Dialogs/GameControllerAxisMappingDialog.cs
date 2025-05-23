using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing a game controller axis mapping
    /// </summary>
    public partial class GameControllerAxisMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly GameControllerAxisMapping _mapping;
        private readonly MidiManager? _midiManager;
        private bool _isNewMapping;
        private bool _updatingUI = false;
        private bool _isListening = false;

        /// <summary>
        /// Gets the edited game controller axis mapping
        /// </summary>
        public GameControllerAxisMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerAxisMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public GameControllerAxisMappingDialog(MidiManager? midiManager = null) : this(new GameControllerAxisMapping { ControlNumber = 7, Axis = "LeftThumbX" }, midiManager) // Default to CC 7 (Volume) and LeftThumbX axis
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerAxisMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The game controller axis mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public GameControllerAxisMappingDialog(GameControllerAxisMapping mapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<GameControllerAxisMappingDialog>();
            _logger.LogDebug("Initializing GameControllerAxisMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping ?? new GameControllerAxisMapping();
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Axis Mapping" : "Edit Axis Mapping";

            // Set up event handlers
            controlNumberNumericUpDown.ValueChanged += ControlNumberNumericUpDown_ValueChanged;
            axisComboBox.SelectedIndexChanged += AxisComboBox_SelectedIndexChanged;
            minValueNumericUpDown.ValueChanged += MinValueNumericUpDown_ValueChanged;
            maxValueNumericUpDown.ValueChanged += MaxValueNumericUpDown_ValueChanged;
            invertCheckBox.CheckedChanged += InvertCheckBox_CheckedChanged;
            controllerIndexNumericUpDown.ValueChanged += ControllerIndexNumericUpDown_ValueChanged;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            testButton.Click += TestButton_Click;
            listenButton.Click += ListenButton_Click;

            // Populate the axis combo box
            PopulateAxisComboBox();

            // Load the mapping data
            LoadMappingData();
        }

        /// <summary>
        /// Populates the axis combo box with valid axis options
        /// </summary>
        private void PopulateAxisComboBox()
        {
            // Add valid axis options
            axisComboBox.Items.Clear();
            axisComboBox.Items.AddRange(new string[]
            {
                "LeftThumbX",
                "LeftThumbY",
                "RightThumbX",
                "RightThumbY",
                "LeftTrigger",
                "RightTrigger"
            });
        }

        /// <summary>
        /// Loads the mapping data into the UI
        /// </summary>
        private void LoadMappingData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Set the control number
                    controlNumberNumericUpDown.Value = _mapping.ControlNumber;

                    // Set the axis
                    axisComboBox.SelectedItem = _mapping.Axis;

                    // Set the min/max values
                    minValueNumericUpDown.Value = _mapping.MinValue;
                    maxValueNumericUpDown.Value = _mapping.MaxValue;

                    // Set the invert flag
                    invertCheckBox.Checked = _mapping.Invert;

                    // Set the controller index
                    controllerIndexNumericUpDown.Value = _mapping.ControllerIndex;
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading game controller axis mapping data", this);
        }

        /// <summary>
        /// Saves the mapping data from the UI
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool SaveMappingData()
        {
            try
            {
                // Get the control number
                _mapping.ControlNumber = (int)controlNumberNumericUpDown.Value;

                // Get the axis
                _mapping.Axis = axisComboBox.SelectedItem?.ToString() ?? string.Empty;

                // Get the min/max values
                _mapping.MinValue = (int)minValueNumericUpDown.Value;
                _mapping.MaxValue = (int)maxValueNumericUpDown.Value;

                // Get the invert flag
                _mapping.Invert = invertCheckBox.Checked;

                // Get the controller index
                _mapping.ControllerIndex = (int)controllerIndexNumericUpDown.Value;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game controller axis mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the mapping data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Validates the mapping data
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateMapping()
        {
            // Validate the axis
            if (string.IsNullOrEmpty(_mapping.Axis))
            {
                ApplicationErrorHandler.ShowError("Please select an axis.", "Validation Error", _logger, null, this);
                return false;
            }

            // Validate the min/max values
            if (_mapping.MinValue >= _mapping.MaxValue)
            {
                ApplicationErrorHandler.ShowError("The minimum value must be less than the maximum value.", "Validation Error", _logger, null, this);
                return false;
            }

            return true;
        }

        #region MIDI Listening

        /// <summary>
        /// Starts listening for MIDI control change events
        /// </summary>
        private void StartMidiListening()
        {
            try
            {
                // Check if MIDI manager is available
                if (_midiManager == null)
                {
                    ApplicationErrorHandler.ShowError("MIDI manager not available. Please ensure the application is properly initialized.", "Error", _logger, null, this);
                    return;
                }

                // Subscribe to MIDI events
                _midiManager.MidiEventReceived += MidiManager_MidiEventReceived;
                _isListening = true;

                // Update UI
                UpdateListenButtonState();

                _logger.LogInformation("Started listening for MIDI control change events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MIDI listening");
                ApplicationErrorHandler.ShowError("An error occurred while starting MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Stops listening for MIDI control change events
        /// </summary>
        private void StopMidiListening()
        {
            try
            {
                if (_midiManager != null)
                {
                    _midiManager.MidiEventReceived -= MidiManager_MidiEventReceived;
                }

                _isListening = false;

                // Update UI
                UpdateListenButtonState();

                _logger.LogInformation("Stopped listening for MIDI control change events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MIDI listening");
                ApplicationErrorHandler.ShowError("An error occurred while stopping MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Updates the listen button state based on the current listening status
        /// </summary>
        private void UpdateListenButtonState()
        {
            try
            {
                if (_isListening)
                {
                    listenButton.Text = "Stop Listening";
                    listenButton.BackColor = System.Drawing.Color.LightCoral;
                }
                else
                {
                    listenButton.Text = "Listen";
                    listenButton.BackColor = System.Drawing.SystemColors.Control;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating listen button state");
            }
        }

        /// <summary>
        /// Handles MIDI events received from the MIDI manager
        /// </summary>
        private void MidiManager_MidiEventReceived(object? sender, MidiEventArgs e)
        {
            try
            {
                // Only process Control Change events
                if (e.Event.EventType != MidiEventType.ControlChange)
                    return;

                // Update the control number field on the UI thread
                RunOnUI(() =>
                {
                    try
                    {
                        // Set the control number value
                        if (e.Event.Controller.HasValue)
                        {
                            controlNumberNumericUpDown.Value = e.Event.Controller.Value;
                        }

                        // Stop listening after receiving the first control change for better UX
                        StopMidiListening();

                        _logger.LogInformation("Auto-populated control number {ControlNumber} from MIDI input", e.Event.Controller);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating control number from received event");
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing MIDI event");
            }
        }

        /// <summary>
        /// Cleanup when the dialog is closing
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Stop listening if we're currently listening
                if (_isListening)
                {
                    StopMidiListening();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during form closing cleanup");
            }

            base.OnFormClosing(e);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the ValueChanged event of the ControlNumberNumericUpDown
        /// </summary>
        private void ControlNumberNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the AxisComboBox
        /// </summary>
        private void AxisComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the ValueChanged event of the MinValueNumericUpDown
        /// </summary>
        private void MinValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;

            // Ensure min value is less than max value
            if (minValueNumericUpDown.Value >= maxValueNumericUpDown.Value)
            {
                maxValueNumericUpDown.Value = minValueNumericUpDown.Value + 1;
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the MaxValueNumericUpDown
        /// </summary>
        private void MaxValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;

            // Ensure max value is greater than min value
            if (maxValueNumericUpDown.Value <= minValueNumericUpDown.Value)
            {
                minValueNumericUpDown.Value = maxValueNumericUpDown.Value - 1;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the InvertCheckBox
        /// </summary>
        private void InvertCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the ValueChanged event of the ControllerIndexNumericUpDown
        /// </summary>
        private void ControllerIndexNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the TextChanged event of the DescriptionTextBox
        /// </summary>
        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the Click event of the TestButton
        /// </summary>
        private void TestButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the mapping data
                if (!SaveMappingData())
                    return;

                // Validate the mapping data
                if (!ValidateMapping())
                    return;

                // Test the mapping
                MessageBox.Show(this, "Test functionality not implemented yet.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, _logger, "testing game controller axis mapping", this);
        }

        /// <summary>
        /// Handles the Click event of the ListenButton
        /// </summary>
        private void ListenButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (_isListening)
                {
                    StopMidiListening();
                }
                else
                {
                    StartMidiListening();
                }
            }, _logger, "toggling MIDI listening", this);
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the mapping data
                if (!SaveMappingData())
                    return;

                // Validate the mapping data
                if (!ValidateMapping())
                    return;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving game controller axis mapping", this);
        }

        #endregion
    }
}
