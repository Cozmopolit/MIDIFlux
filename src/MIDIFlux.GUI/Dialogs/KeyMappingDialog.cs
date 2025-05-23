using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing a key mapping
    /// </summary>
    public partial class KeyMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly KeyMapping _keyMapping;
        private bool _isNewMapping;
        private bool _updatingUI = false;

        // MIDI listening fields
        private MidiManager? _midiManager;
        private bool _isListening = false;

        /// <summary>
        /// Gets the edited key mapping
        /// </summary>
        public KeyMapping KeyMapping => _keyMapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public KeyMappingDialog(MidiManager? midiManager = null) : this(new KeyMapping { MidiNote = 60, VirtualKeyCode = 77 }, midiManager) // Default to note 60 (C4) and 'M' key
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="keyMapping">The key mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public KeyMappingDialog(KeyMapping keyMapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<KeyMappingDialog>();
            _logger.LogDebug("Initializing KeyMappingDialog");

            // Store the key mapping and MIDI manager
            _keyMapping = keyMapping;
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Key Mapping" : "Edit Key Mapping";

            // Populate the action type dropdown
            actionTypeComboBox.Items.Add(KeyActionType.PressAndRelease);
            actionTypeComboBox.Items.Add(KeyActionType.KeyDown);
            actionTypeComboBox.Items.Add(KeyActionType.KeyUp);
            actionTypeComboBox.Items.Add(KeyActionType.Toggle);
            actionTypeComboBox.Items.Add(KeyActionType.CommandExecution);

            // Populate the virtual key dropdown
            PopulateVirtualKeyDropdown();

            // Populate the shell type dropdown
            shellTypeComboBox.Items.Add(CommandShellType.PowerShell);
            shellTypeComboBox.Items.Add(CommandShellType.CommandPrompt);

            // Set up event handlers
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;
            ignoreNoteOffCheckBox.CheckedChanged += IgnoreNoteOffCheckBox_CheckedChanged;
            autoReleaseCheckBox.CheckedChanged += AutoReleaseCheckBox_CheckedChanged;
            testButton.Click += TestButton_Click;

            // Load the key mapping data
            LoadKeyMapping();
        }

        /// <summary>
        /// Handles the FormClosing event to clean up MIDI listening
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                // Stop MIDI listening if active
                if (_isListening)
                {
                    StopMidiListening();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up MIDI listening on form close");
            }

            base.OnFormClosing(e);
        }

        /// <summary>
        /// Populates the virtual key dropdown with all available keys
        /// </summary>
        private void PopulateVirtualKeyDropdown()
        {
            // Clear the dropdown
            virtualKeyComboBox.Items.Clear();

            // Add all keys
            foreach (Keys key in Enum.GetValues(typeof(Keys)))
            {
                // Skip modifiers and special keys
                if (key == Keys.Control || key == Keys.Shift || key == Keys.Alt || key == Keys.None)
                    continue;

                virtualKeyComboBox.Items.Add(new KeyItem(key));
            }

            // Sort the items
            virtualKeyComboBox.Sorted = true;
        }

        /// <summary>
        /// Loads the key mapping data into the UI
        /// </summary>
        private void LoadKeyMapping()
        {
            _updatingUI = true;
            try
            {
                // Set the MIDI note
                midiNoteNumericUpDown.Value = _keyMapping.MidiNote;
                UpdateNoteNameDisplay();

                // Set the action type
                actionTypeComboBox.SelectedItem = _keyMapping.ActionType;

                // Set the virtual key
                foreach (KeyItem item in virtualKeyComboBox.Items)
                {
                    if ((ushort)item.Key == _keyMapping.VirtualKeyCode)
                    {
                        virtualKeyComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Set the modifiers
                shiftCheckBox.Checked = _keyMapping.Modifiers.Contains(16); // Keys.Shift
                ctrlCheckBox.Checked = _keyMapping.Modifiers.Contains(17); // Keys.Control
                altCheckBox.Checked = _keyMapping.Modifiers.Contains(18); // Keys.Alt
                winCheckBox.Checked = _keyMapping.Modifiers.Contains(91) || _keyMapping.Modifiers.Contains(92); // Keys.LWin or Keys.RWin

                // Set the command execution options
                commandTextBox.Text = _keyMapping.Command ?? string.Empty;
                shellTypeComboBox.SelectedItem = _keyMapping.ShellType;
                runHiddenCheckBox.Checked = _keyMapping.RunHidden;
                waitForExitCheckBox.Checked = _keyMapping.WaitForExit;

                // Set the note-on only options
                ignoreNoteOffCheckBox.Checked = _keyMapping.IgnoreNoteOff;
                autoReleaseCheckBox.Checked = _keyMapping.AutoReleaseAfterMs.HasValue;
                autoReleaseNumericUpDown.Value = _keyMapping.AutoReleaseAfterMs ?? 1000;

                // Set the description
                descriptionTextBox.Text = _keyMapping.Description ?? string.Empty;

                // Update the UI based on the action type
                UpdateUIForActionType();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading key mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while loading the key mapping data.", "Error", _logger, ex, this);
            }
            finally
            {
                _updatingUI = false;
            }
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
                // Get the selected action type
                if (actionTypeComboBox.SelectedItem == null)
                    return;

                var actionType = (KeyActionType)actionTypeComboBox.SelectedItem;

                // Show/hide panels based on the action type
                keySelectionPanel.Visible = actionType != KeyActionType.CommandExecution;
                commandExecutionPanel.Visible = actionType == KeyActionType.CommandExecution;

                // Update the note-on only options
                UpdateNoteOnOnlyOptions();
            }, _logger, "updating UI for action type", this);
        }

        /// <summary>
        /// Updates the note-on only options based on the checkbox states
        /// </summary>
        private void UpdateNoteOnOnlyOptions()
        {
            if (_updatingUI)
                return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Enable/disable the auto-release options
                autoReleaseCheckBox.Enabled = ignoreNoteOffCheckBox.Checked;
                autoReleaseNumericUpDown.Enabled = ignoreNoteOffCheckBox.Checked && autoReleaseCheckBox.Checked;
            }, _logger, "updating note-on only options", this);
        }

        /// <summary>
        /// Updates the note name display based on the MIDI note number
        /// </summary>
        private void UpdateNoteNameDisplay()
        {
            try
            {
                // Get the MIDI note number
                int midiNote = (int)midiNoteNumericUpDown.Value;

                // Calculate the note name
                string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
                int octave = midiNote / 12 - 1;
                int noteIndex = midiNote % 12;
                string noteName = noteNames[noteIndex] + octave;

                // Update the note name display
                noteNameLabel.Text = noteName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating note name display");
                // Don't show an error message for this minor issue
            }
        }

        /// <summary>
        /// Saves the UI data to the key mapping
        /// </summary>
        private bool SaveKeyMapping()
        {
            try
            {
                // Get the MIDI note
                _keyMapping.MidiNote = (int)midiNoteNumericUpDown.Value;

                // Get the action type
                if (actionTypeComboBox.SelectedItem != null)
                {
                    _keyMapping.ActionType = (KeyActionType)actionTypeComboBox.SelectedItem;
                }

                // Get the virtual key
                if (virtualKeyComboBox.SelectedItem is KeyItem keyItem)
                {
                    _keyMapping.VirtualKeyCode = (ushort)keyItem.Key;
                }

                // Get the modifiers
                _keyMapping.Modifiers.Clear();
                if (shiftCheckBox.Checked)
                    _keyMapping.Modifiers.Add(16); // Keys.Shift
                if (ctrlCheckBox.Checked)
                    _keyMapping.Modifiers.Add(17); // Keys.Control
                if (altCheckBox.Checked)
                    _keyMapping.Modifiers.Add(18); // Keys.Alt
                if (winCheckBox.Checked)
                    _keyMapping.Modifiers.Add(91); // Keys.LWin

                // Get the command execution options
                _keyMapping.Command = commandTextBox.Text.Trim();
                if (shellTypeComboBox.SelectedItem is CommandShellType shellType)
                {
                    _keyMapping.ShellType = shellType;
                }
                _keyMapping.RunHidden = runHiddenCheckBox.Checked;
                _keyMapping.WaitForExit = waitForExitCheckBox.Checked;

                // Get the note-on only options
                _keyMapping.IgnoreNoteOff = ignoreNoteOffCheckBox.Checked;
                _keyMapping.AutoReleaseAfterMs = autoReleaseCheckBox.Checked ? (int?)autoReleaseNumericUpDown.Value : null;

                // Get the description
                _keyMapping.Description = descriptionTextBox.Text.Trim();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving key mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the key mapping data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Validates the key mapping data
        /// </summary>
        private bool ValidateKeyMapping()
        {
            try
            {
                // Check if a virtual key is selected for keyboard actions
                if (_keyMapping.ActionType != KeyActionType.CommandExecution && virtualKeyComboBox.SelectedItem == null)
                {
                    ApplicationErrorHandler.ShowError("Please select a virtual key.", "Validation Error", _logger, null, this);
                    virtualKeyComboBox.Focus();
                    return false;
                }

                // Check if a command is entered for command execution
                if (_keyMapping.ActionType == KeyActionType.CommandExecution && string.IsNullOrWhiteSpace(_keyMapping.Command))
                {
                    ApplicationErrorHandler.ShowError("Please enter a command.", "Validation Error", _logger, null, this);
                    commandTextBox.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating key mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while validating the key mapping data.", "Error", _logger, ex, this);
                return false;
            }
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
        /// Updates the listen button state based on the listening status
        /// </summary>
        private void UpdateListenButtonState()
        {
            try
            {
                if (_isListening)
                {
                    listenButton.Text = "Stop Listening";
                    listenButton.BackColor = Color.LightCoral;
                }
                else
                {
                    listenButton.Text = "Listen";
                    listenButton.BackColor = SystemColors.Control;
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

                        // Update the note name display
                        UpdateNoteNameDisplay();

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
                _logger.LogError(ex, "Error handling MIDI event");
            }
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ActionTypeComboBox
        /// </summary>
        private void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIForActionType();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the IgnoreNoteOffCheckBox
        /// </summary>
        private void IgnoreNoteOffCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateNoteOnOnlyOptions();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the AutoReleaseCheckBox
        /// </summary>
        private void AutoReleaseCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateNoteOnOnlyOptions();
        }

        /// <summary>
        /// Handles the Click event of the TestButton
        /// </summary>
        private void TestButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the key mapping data
                if (!SaveKeyMapping())
                    return;

                MessageBox.Show("Test functionality not implemented yet.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, _logger, "testing key mapping", this);
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the key mapping data
                if (!SaveKeyMapping())
                    return;

                // Validate the key mapping data
                if (!ValidateKeyMapping())
                    return;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving key mapping", this);
        }

        /// <summary>
        /// Handles the ValueChanged event of the MidiNoteNumericUpDown
        /// </summary>
        private void midiNoteNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            UpdateNoteNameDisplay();
        }

        /// <summary>
        /// Handles the Click event of the ListenButton
        /// </summary>
        private void listenButton_Click(object? sender, EventArgs e)
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

        #endregion

        /// <summary>
        /// Represents an item in the virtual key dropdown
        /// </summary>
        private class KeyItem
        {
            /// <summary>
            /// Gets the key
            /// </summary>
            public Keys Key { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="KeyItem"/> class
            /// </summary>
            /// <param name="key">The key</param>
            public KeyItem(Keys key)
            {
                Key = key;
            }

            /// <summary>
            /// Returns a string representation of the key
            /// </summary>
            public override string ToString()
            {
                return $"{Key} ({(int)Key})";
            }
        }
    }
}

