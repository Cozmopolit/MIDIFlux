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
    /// Dialog for editing a relative control mapping
    /// </summary>
    public partial class RelativeControlMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly RelativeControlMapping _mapping;
        private readonly MidiManager? _midiManager;
        private bool _isNewMapping;
        private bool _updatingUI = false;
        private bool _isListening = false;

        /// <summary>
        /// Gets the edited relative control mapping
        /// </summary>
        public RelativeControlMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeControlMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public RelativeControlMappingDialog(MidiManager? midiManager = null) : this(new RelativeControlMapping { ControlNumber = 16, HandlerType = "ScrollWheel", Sensitivity = 1 }, midiManager) // Default to CC 16 and ScrollWheel handler
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeControlMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The relative control mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public RelativeControlMappingDialog(RelativeControlMapping mapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<RelativeControlMappingDialog>();
            _logger.LogDebug("Initializing RelativeControlMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping;
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Relative Control Mapping" : "Edit Relative Control Mapping";

            // Set up event handlers
            handlerTypeComboBox.SelectedIndexChanged += HandlerTypeComboBox_SelectedIndexChanged;
            selectChannelButton.Click += SelectChannelButton_Click;
            encodingComboBox.SelectedIndexChanged += EncodingComboBox_SelectedIndexChanged;
            testButton.Click += TestButton_Click;
            listenButton.Click += ListenButton_Click;

            // Load the mapping data
            LoadMappingData();
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

                    // Set the channel display
                    UpdateChannelDisplay();

                    // Set the handler type
                    handlerTypeComboBox.Items.Clear();
                    handlerTypeComboBox.Items.Add("ScrollWheel");
                    handlerTypeComboBox.Items.Add("GameControllerAxis");

                    // Select the current handler type or default to the first one
                    if (!string.IsNullOrEmpty(_mapping.HandlerType) && handlerTypeComboBox.Items.Contains(_mapping.HandlerType))
                    {
                        handlerTypeComboBox.SelectedItem = _mapping.HandlerType;
                    }
                    else
                    {
                        handlerTypeComboBox.SelectedIndex = 0;
                        _mapping.HandlerType = handlerTypeComboBox.SelectedItem?.ToString() ?? "ScrollWheel";
                    }

                    // Set the sensitivity
                    sensitivityNumericUpDown.Value = _mapping.Sensitivity;

                    // Set the invert option
                    invertCheckBox.Checked = _mapping.Invert;

                    // Set the encoding
                    encodingComboBox.SelectedIndex = (int)_mapping.Encoding;

                    // Set the description
                    descriptionTextBox.Text = _mapping.Description ?? string.Empty;

                    // Update the UI based on the handler type
                    UpdateUIForHandlerType();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading relative control mapping data", this);
        }

        /// <summary>
        /// Updates the UI based on the selected handler type
        /// </summary>
        private void UpdateUIForHandlerType()
        {
            if (_updatingUI)
                return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Get the selected handler type
                    string handlerType = handlerTypeComboBox.SelectedItem?.ToString() ?? "ScrollWheel";

                    // Update the parameters panel based on the handler type
                    // For now, we don't have any specific parameters for the built-in handlers
                    parametersPanel.Visible = false;

                    // Update the description of the handler
                    string description = handlerType switch
                    {
                        "ScrollWheel" => "Controls the mouse scroll wheel",
                        "GameControllerAxis" => "Maps to a game controller axis",
                        _ => "Unknown handler type"
                    };

                    handlerDescriptionLabel.Text = description;
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "updating UI for handler type", this);
        }

        /// <summary>
        /// Updates the channel display
        /// </summary>
        private void UpdateChannelDisplay()
        {
            if (_mapping.Channel.HasValue)
            {
                channelTextBox.Text = $"Channel {_mapping.Channel.Value}";
            }
            else
            {
                channelTextBox.Text = "All Channels";
            }
        }

        /// <summary>
        /// Updates the encoding description
        /// </summary>
        private void UpdateEncodingDescription()
        {
            if (_updatingUI)
                return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Get the selected encoding
                var encoding = (RelativeValueEncoding)encodingComboBox.SelectedIndex;

                // Update the description
                string description = encoding switch
                {
                    RelativeValueEncoding.SignMagnitude => "Values 1-63 are positive, 65-127 are negative",
                    RelativeValueEncoding.TwosComplement => "Values 1-64 are positive, 127-65 are negative",
                    RelativeValueEncoding.BinaryOffset => "64 is zero, above is positive, below is negative",
                    _ => "Unknown encoding"
                };

                encodingDescriptionLabel.Text = description;
            }, _logger, "updating encoding description", this);
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

                // Get the handler type
                _mapping.HandlerType = handlerTypeComboBox.SelectedItem?.ToString() ?? "ScrollWheel";

                // Get the sensitivity
                _mapping.Sensitivity = (int)sensitivityNumericUpDown.Value;

                // Get the invert option
                _mapping.Invert = invertCheckBox.Checked;

                // Get the encoding
                _mapping.Encoding = (RelativeValueEncoding)encodingComboBox.SelectedIndex;

                // Get the description
                _mapping.Description = descriptionTextBox.Text.Trim();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving relative control mapping data");
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
            try
            {
                // Check if a handler type is selected
                if (handlerTypeComboBox.SelectedItem == null)
                {
                    ApplicationErrorHandler.ShowError("Please select a handler type.", "Validation Error", _logger, null, this);
                    handlerTypeComboBox.Focus();
                    return false;
                }

                // Check if sensitivity is greater than 0
                if (sensitivityNumericUpDown.Value <= 0)
                {
                    ApplicationErrorHandler.ShowError("Sensitivity must be greater than 0.", "Validation Error", _logger, null, this);
                    sensitivityNumericUpDown.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating relative control mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while validating the mapping data.", "Error", _logger, ex, this);
                return false;
            }
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
        /// Handles the SelectedIndexChanged event of the HandlerTypeComboBox
        /// </summary>
        private void HandlerTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIForHandlerType();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the EncodingComboBox
        /// </summary>
        private void EncodingComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateEncodingDescription();
        }

        /// <summary>
        /// Handles the Click event of the SelectChannelButton
        /// </summary>
        private void SelectChannelButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a list of selected channels
                List<int>? selectedChannels = null;
                if (_mapping.Channel.HasValue)
                {
                    selectedChannels = new List<int> { _mapping.Channel.Value };
                }

                // Open the channel picker dialog
                using (var channelPicker = new ChannelPickerDialog(selectedChannels))
                {
                    if (channelPicker.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the mapping with the selected channels
                        if (channelPicker.SelectedChannels != null && channelPicker.SelectedChannels.Count == 1)
                        {
                            _mapping.Channel = channelPicker.SelectedChannels[0];
                        }
                        else
                        {
                            _mapping.Channel = null;
                        }

                        // Update the channel display
                        UpdateChannelDisplay();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error selecting MIDI channel");
                ApplicationErrorHandler.ShowError("An error occurred while selecting the MIDI channel.", "Error", _logger, ex, this);
            }
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

                MessageBox.Show(this, "Test functionality not yet implemented.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, _logger, "testing relative control mapping", this);
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
            }, _logger, "saving relative control mapping", this);
        }

        #endregion
    }
}
