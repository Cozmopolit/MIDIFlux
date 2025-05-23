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
    /// Dialog for editing a macro mapping
    /// </summary>
    public partial class MacroMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly MacroMapping _mapping;
        private readonly MidiManager? _midiManager;
        private bool _isNewMapping;
        private bool _updatingUI = false;
        private bool _isListening = false;
        private List<MacroActionDefinition> _actions = new List<MacroActionDefinition>();
        private MacroActionDefinition? _selectedAction = null;

        /// <summary>
        /// Gets the edited macro mapping
        /// </summary>
        public MacroMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public MacroMappingDialog(MidiManager? midiManager = null) : this(new MacroMapping { MidiNote = 60 }, midiManager) // Default to note 60 (C4)
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The macro mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public MacroMappingDialog(MacroMapping mapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<MacroMappingDialog>();
            _logger.LogDebug("Initializing MacroMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping;
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Macro Mapping" : "Edit Macro Mapping";

            // Set up event handlers
            midiNoteNumericUpDown.ValueChanged += MidiNoteNumericUpDown_ValueChanged;
            ignoreNoteOffCheckBox.CheckedChanged += IgnoreNoteOffCheckBox_CheckedChanged;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            addActionButton.Click += AddActionButton_Click;
            editActionButton.Click += EditActionButton_Click;
            deleteActionButton.Click += DeleteActionButton_Click;
            moveUpButton.Click += MoveUpButton_Click;
            moveDownButton.Click += MoveDownButton_Click;
            actionsListView.SelectedIndexChanged += ActionsListView_SelectedIndexChanged;
            actionsListView.DoubleClick += ActionsListView_DoubleClick;
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

                    // Set the MIDI note
                    midiNoteNumericUpDown.Value = _mapping.MidiNote;

                    // Set the ignore note off flag
                    ignoreNoteOffCheckBox.Checked = _mapping.IgnoreNoteOff;

                    // Set the description
                    descriptionTextBox.Text = _mapping.Description ?? string.Empty;

                    // Load the actions
                    _actions = new List<MacroActionDefinition>(_mapping.Actions);
                    UpdateActionsListView();

                    // Update the note name label
                    UpdateNoteNameLabel();

                    // Update the UI state
                    UpdateUIState();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading macro mapping data", this);
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
        /// Updates the actions list view
        /// </summary>
        private void UpdateActionsListView()
        {
            actionsListView.Items.Clear();

            for (int i = 0; i < _actions.Count; i++)
            {
                var action = _actions[i];
                var item = new ListViewItem(new string[]
                {
                    (i + 1).ToString(),
                    action.Type.ToString(),
                    action.DisplayDescription,
                    action.DelayAfter > 0 ? $"{action.DelayAfter} ms" : string.Empty
                });
                item.Tag = action;
                actionsListView.Items.Add(item);
            }

            // Update the UI state
            UpdateUIState();
        }

        /// <summary>
        /// Updates the UI state based on the current selection
        /// </summary>
        private void UpdateUIState()
        {
            bool hasSelection = actionsListView.SelectedItems.Count > 0;
            editActionButton.Enabled = hasSelection;
            deleteActionButton.Enabled = hasSelection;

            // Enable/disable move buttons based on selection and position
            if (hasSelection)
            {
                int selectedIndex = actionsListView.SelectedIndices[0];
                moveUpButton.Enabled = selectedIndex > 0;
                moveDownButton.Enabled = selectedIndex < actionsListView.Items.Count - 1;
            }
            else
            {
                moveUpButton.Enabled = false;
                moveDownButton.Enabled = false;
            }
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

                // Get the ignore note off flag
                _mapping.IgnoreNoteOff = ignoreNoteOffCheckBox.Checked;

                // Get the description
                _mapping.Description = descriptionTextBox.Text.Trim();
                if (string.IsNullOrEmpty(_mapping.Description))
                    _mapping.Description = null;

                // Set the actions
                _mapping.Actions = new List<MacroActionDefinition>(_actions);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving macro mapping data");
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
            // Validate that there is at least one action
            if (_actions.Count == 0)
            {
                ApplicationErrorHandler.ShowError("Please add at least one action to the macro.", "Validation Error", _logger, null, this);
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
        /// Handles the CheckedChanged event of the IgnoreNoteOffCheckBox
        /// </summary>
        private void IgnoreNoteOffCheckBox_CheckedChanged(object? sender, EventArgs e)
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
        /// Handles the Click event of the AddActionButton
        /// </summary>
        private void AddActionButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a new action
                var action = new MacroActionDefinition
                {
                    Type = ActionType.KeyPressRelease,
                    VirtualKeyCode = 65, // Default to 'A' key
                    Description = "Press and release A key"
                };

                // Edit the action
                using (var dialog = new MacroActionDialog(action))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Add the action to the list
                        _actions.Add(action);

                        // Update the list view
                        UpdateActionsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding action");
                ApplicationErrorHandler.ShowError("An error occurred while adding an action.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditActionButton
        /// </summary>
        private void EditActionButton_Click(object? sender, EventArgs e)
        {
            EditSelectedAction();
        }

        /// <summary>
        /// Handles the Click event of the DeleteActionButton
        /// </summary>
        private void DeleteActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected action
                var selectedItem = actionsListView.SelectedItems[0];
                var action = selectedItem.Tag as MacroActionDefinition;

                if (action == null)
                {
                    return;
                }

                // Confirm deletion
                var result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to delete the action '{action.DisplayDescription}'?",
                    "Delete Action",
                    _logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the action
                _actions.Remove(action);

                // Update the list view
                UpdateActionsListView();
            }, _logger, "deleting action", this);
        }

        /// <summary>
        /// Handles the Click event of the MoveUpButton
        /// </summary>
        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                int selectedIndex = actionsListView.SelectedIndices[0];
                if (selectedIndex <= 0)
                {
                    return;
                }

                // Swap the actions
                var action = _actions[selectedIndex];
                _actions.RemoveAt(selectedIndex);
                _actions.Insert(selectedIndex - 1, action);

                // Update the list view
                UpdateActionsListView();

                // Reselect the moved item
                actionsListView.Items[selectedIndex - 1].Selected = true;
            }, _logger, "moving action up", this);
        }

        /// <summary>
        /// Handles the Click event of the MoveDownButton
        /// </summary>
        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                int selectedIndex = actionsListView.SelectedIndices[0];
                if (selectedIndex >= actionsListView.Items.Count - 1)
                {
                    return;
                }

                // Swap the actions
                var action = _actions[selectedIndex];
                _actions.RemoveAt(selectedIndex);
                _actions.Insert(selectedIndex + 1, action);

                // Update the list view
                UpdateActionsListView();

                // Reselect the moved item
                actionsListView.Items[selectedIndex + 1].Selected = true;
            }, _logger, "moving action down", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ActionsListView
        /// </summary>
        private void ActionsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (actionsListView.SelectedItems.Count > 0)
            {
                _selectedAction = actionsListView.SelectedItems[0].Tag as MacroActionDefinition;
            }
            else
            {
                _selectedAction = null;
            }

            UpdateUIState();
        }

        /// <summary>
        /// Handles the DoubleClick event of the ActionsListView
        /// </summary>
        private void ActionsListView_DoubleClick(object? sender, EventArgs e)
        {
            EditSelectedAction();
        }

        /// <summary>
        /// Edits the selected action
        /// </summary>
        private void EditSelectedAction()
        {
            try
            {
                if (actionsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected action
                var selectedItem = actionsListView.SelectedItems[0];
                var action = selectedItem.Tag as MacroActionDefinition;

                if (action == null)
                {
                    return;
                }

                // Edit the action
                using (var dialog = new MacroActionDialog(action))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the list view
                        UpdateActionsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing action");
                ApplicationErrorHandler.ShowError("An error occurred while editing the action.", "Error", _logger, ex, this);
            }
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
                MessageBox.Show(this, "Macro test functionality will be implemented in a future version.", "Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }, _logger, "testing macro mapping", this);
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
            }, _logger, "saving macro mapping", this);
        }

        #endregion
    }
}
