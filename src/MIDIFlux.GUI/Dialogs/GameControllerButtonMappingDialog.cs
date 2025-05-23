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
    /// Dialog for editing a game controller button mapping
    /// </summary>
    public partial class GameControllerButtonMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly GameControllerButtonMapping _mapping;
        private readonly MidiManager? _midiManager;
        private bool _isNewMapping;
        private bool _updatingUI = false;
        private bool _isListening = false;

        /// <summary>
        /// Gets the edited game controller button mapping
        /// </summary>
        public GameControllerButtonMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerButtonMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public GameControllerButtonMappingDialog(MidiManager? midiManager = null) : this(new GameControllerButtonMapping { MidiNote = 60, Button = "A" }, midiManager) // Default to note 60 (C4) and 'A' button
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerButtonMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The game controller button mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public GameControllerButtonMappingDialog(GameControllerButtonMapping mapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<GameControllerButtonMappingDialog>();
            _logger.LogDebug("Initializing GameControllerButtonMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping ?? new GameControllerButtonMapping();
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Button Mapping" : "Edit Button Mapping";

            // Set up event handlers
            midiNoteNumericUpDown.ValueChanged += MidiNoteNumericUpDown_ValueChanged;
            buttonComboBox.SelectedIndexChanged += ButtonComboBox_SelectedIndexChanged;
            controllerIndexNumericUpDown.ValueChanged += ControllerIndexNumericUpDown_ValueChanged;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            testButton.Click += TestButton_Click;
            listenButton.Click += ListenButton_Click;

            // Populate the button combo box
            PopulateButtonComboBox();

            // Load the mapping data
            LoadMappingData();
        }

        /// <summary>
        /// Populates the button combo box with valid button options
        /// </summary>
        private void PopulateButtonComboBox()
        {
            // Add valid button options
            buttonComboBox.Items.Clear();
            buttonComboBox.Items.AddRange(new string[]
            {
                "A",
                "B",
                "X",
                "Y",
                "LeftShoulder",
                "RightShoulder",
                "Back",
                "Start",
                "LeftThumb",
                "RightThumb",
                "DPadUp",
                "DPadDown",
                "DPadLeft",
                "DPadRight",
                "Guide"
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

                    // Set the MIDI note
                    midiNoteNumericUpDown.Value = _mapping.MidiNote;

                    // Set the button
                    buttonComboBox.SelectedItem = _mapping.Button;

                    // Set the controller index
                    controllerIndexNumericUpDown.Value = _mapping.ControllerIndex;

                    // Update the note name label
                    UpdateNoteNameLabel();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading game controller button mapping data", this);
        }

        /// <summary>
        /// Updates the note name label based on the current MIDI note value
        /// </summary>
        private void UpdateNoteNameLabel()
        {
            try
            {
                int note = (int)midiNoteNumericUpDown.Value;
                noteNameLabel.Text = GetNoteName(note);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note name label");
                noteNameLabel.Text = "Unknown";
            }
        }

        /// <summary>
        /// Gets the name of a MIDI note
        /// </summary>
        /// <param name="noteNumber">The MIDI note number</param>
        /// <returns>The note name (e.g. "C4")</returns>
        private string GetNoteName(int noteNumber)
        {
            string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
            int octave = noteNumber / 12 - 1;
            int noteIndex = noteNumber % 12;
            return noteNames[noteIndex] + octave;
        }

        /// <summary>
        /// Saves the mapping data from the UI
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool SaveMappingData()
        {
            try
            {
                // Get the MIDI note
                _mapping.MidiNote = (int)midiNoteNumericUpDown.Value;

                // Get the button
                _mapping.Button = buttonComboBox.SelectedItem?.ToString() ?? string.Empty;

                // Get the controller index
                _mapping.ControllerIndex = (int)controllerIndexNumericUpDown.Value;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game controller button mapping data");
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
            // Validate the button
            if (string.IsNullOrEmpty(_mapping.Button))
            {
                ApplicationErrorHandler.ShowError("Please select a button.", "Validation Error", _logger, null, this);
                return false;
            }

            return true;
        }

        #region MIDI Listening

        /// <summary>
        /// Starts listening for MIDI note events
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

                _logger.LogInformation("Started listening for MIDI note events");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MIDI listening");
                ApplicationErrorHandler.ShowError("An error occurred while starting MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Stops listening for MIDI note events
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

                _logger.LogInformation("Stopped listening for MIDI note events");
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
                // Only process Note On events
                if (e.Event.EventType != MidiEventType.NoteOn)
                    return;

                // Update the MIDI note field on the UI thread
                RunOnUI(() =>
                {
                    try
                    {
                        // Set the MIDI note value
                        if (e.Event.Note.HasValue)
                        {
                            midiNoteNumericUpDown.Value = e.Event.Note.Value;
                        }

                        // Stop listening after receiving the first note for better UX
                        StopMidiListening();

                        _logger.LogInformation("Auto-populated MIDI note {Note} from MIDI input", e.Event.Note);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error updating MIDI note from received event");
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
        /// Handles the ValueChanged event of the MidiNoteNumericUpDown
        /// </summary>
        private void MidiNoteNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;

            // Update the note name label
            UpdateNoteNameLabel();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ButtonComboBox
        /// </summary>
        private void ButtonComboBox_SelectedIndexChanged(object? sender, EventArgs e)
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
            }, _logger, "testing game controller button mapping", this);
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
            }, _logger, "saving game controller button mapping", this);
        }

        #endregion
    }
}
