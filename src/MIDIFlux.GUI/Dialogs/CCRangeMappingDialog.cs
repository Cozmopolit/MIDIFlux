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
    /// Dialog for editing a CC range mapping
    /// </summary>
    public partial class CCRangeMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly CCRangeMapping _mapping;
        private readonly MidiManager? _midiManager;
        private bool _isNewMapping;
        private List<CCValueRange> _ranges = new List<CCValueRange>();

        // Track if we're currently updating the UI to prevent event recursion
        private bool _updatingUI = false;
        private bool _isListening = false;

        /// <summary>
        /// Gets the edited CC range mapping
        /// </summary>
        public CCRangeMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="CCRangeMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public CCRangeMappingDialog(MidiManager? midiManager = null) : this(new CCRangeMapping { ControlNumber = 7, HandlerType = "CCRange" }, midiManager) // Default to CC 7 (Volume)
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CCRangeMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The CC range mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public CCRangeMappingDialog(CCRangeMapping mapping, MidiManager? midiManager = null)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<CCRangeMappingDialog>();
            _logger.LogDebug("Initializing CCRangeMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping;
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add CC Range Mapping" : "Edit CC Range Mapping";

            // Set up event handlers
            selectChannelButton.Click += SelectChannelButton_Click;
            addRangeButton.Click += AddRangeButton_Click;
            editRangeButton.Click += EditRangeButton_Click;
            deleteRangeButton.Click += DeleteRangeButton_Click;
            rangesListView.SelectedIndexChanged += RangesListView_SelectedIndexChanged;
            rangesListView.DoubleClick += RangesListView_DoubleClick;
            generateButton.Click += GenerateButton_Click;
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

                    // Set the description
                    descriptionTextBox.Text = _mapping.Description ?? string.Empty;

                    // Load the ranges
                    _ranges = new List<CCValueRange>(_mapping.Ranges);
                    UpdateRangesListView();

                    // Update the UI state
                    UpdateUIState();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading CC range mapping data", this);
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
        /// Updates the ranges list view
        /// </summary>
        private void UpdateRangesListView()
        {
            rangesListView.Items.Clear();

            foreach (var range in _ranges)
            {
                var item = new ListViewItem(new string[]
                {
                    $"{range.MinValue}-{range.MaxValue}",
                    GetActionTypeDisplayName(range.Action.Type),
                    GetActionDetailsDisplayText(range.Action)
                });
                item.Tag = range;
                rangesListView.Items.Add(item);
            }

            // Update the UI state
            UpdateUIState();
        }

        /// <summary>
        /// Gets the display name for an action type
        /// </summary>
        private string GetActionTypeDisplayName(CCRangeActionType actionType)
        {
            return actionType switch
            {
                CCRangeActionType.KeyPress => "Key Press",
                CCRangeActionType.CommandExecution => "Command",

                _ => "Unknown"
            };
        }

        /// <summary>
        /// Gets the display text for an action's details
        /// </summary>
        private string GetActionDetailsDisplayText(CCRangeAction action)
        {
            return action.Type switch
            {
                CCRangeActionType.KeyPress => !string.IsNullOrEmpty(action.Key) ? action.Key : (action.VirtualKeyCode.HasValue ? $"VK: {action.VirtualKeyCode}" : "None"),
                CCRangeActionType.CommandExecution => !string.IsNullOrEmpty(action.Command) ? action.Command : "None",

                _ => "Unknown"
            };
        }

        /// <summary>
        /// Updates the UI state based on the current selection
        /// </summary>
        private void UpdateUIState()
        {
            // Skip if we're already updating the UI to prevent recursion
            if (_updatingUI)
                return;

            try
            {
                _updatingUI = true;

                bool hasSelection = rangesListView.SelectedItems.Count > 0;
                editRangeButton.Enabled = hasSelection;
                deleteRangeButton.Enabled = hasSelection;
            }
            finally
            {
                _updatingUI = false;
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
                // Get the control number
                _mapping.ControlNumber = (int)controlNumberNumericUpDown.Value;

                // Set the handler type (always CCRange for this dialog)
                _mapping.HandlerType = "CCRange";

                // Get the description
                _mapping.Description = descriptionTextBox.Text.Trim();

                // Set the ranges
                _mapping.Ranges = new List<CCValueRange>(_ranges);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving CC range mapping data");
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
                // Check if there are any ranges defined
                if (_ranges.Count == 0)
                {
                    ApplicationErrorHandler.ShowError("Please add at least one range.", "Validation Error", _logger, null, this);
                    addRangeButton.Focus();
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating CC range mapping data");
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
        /// Handles the Click event of the AddRangeButton
        /// </summary>
        private void AddRangeButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a new range
                var range = new CCValueRange
                {
                    MinValue = 0,
                    MaxValue = 127,
                    Action = new CCRangeAction
                    {
                        Type = CCRangeActionType.KeyPress,
                        Key = "A"
                    }
                };

                // Edit the range
                using (var dialog = new CCRangeEditDialog(range))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Add the range to the list
                        _ranges.Add(range);

                        // Update the list view
                        UpdateRangesListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding range");
                ApplicationErrorHandler.ShowError("An error occurred while adding a range.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditRangeButton
        /// </summary>
        private void EditRangeButton_Click(object? sender, EventArgs e)
        {
            EditSelectedRange();
        }

        /// <summary>
        /// Handles the Click event of the DeleteRangeButton
        /// </summary>
        private void DeleteRangeButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (rangesListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected range
                var selectedItem = rangesListView.SelectedItems[0];
                var range = selectedItem.Tag as CCValueRange;

                if (range == null)
                {
                    return;
                }

                // Confirm deletion
                var result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to delete the range {range.MinValue}-{range.MaxValue}?",
                    "Delete Range",
                    _logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the range
                _ranges.Remove(range);

                // Update the list view
                UpdateRangesListView();
            }, _logger, "deleting range", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the RangesListView
        /// </summary>
        private void RangesListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Handles the DoubleClick event of the RangesListView
        /// </summary>
        private void RangesListView_DoubleClick(object? sender, EventArgs e)
        {
            EditSelectedRange();
        }

        /// <summary>
        /// Edits the selected range
        /// </summary>
        private void EditSelectedRange()
        {
            try
            {
                if (rangesListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected range
                var selectedItem = rangesListView.SelectedItems[0];
                var range = selectedItem.Tag as CCValueRange;

                if (range == null)
                {
                    return;
                }

                // Edit the range
                using (var dialog = new CCRangeEditDialog(range))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the list view
                        UpdateRangesListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing range");
                ApplicationErrorHandler.ShowError("An error occurred while editing the range.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the GenerateButton
        /// </summary>
        private void GenerateButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Show the key sequence generator dialog
                using (var dialog = new CCRangeGeneratorDialog())
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get the generated ranges
                        var generatedRanges = dialog.GeneratedRanges;

                        // Add the ranges to the list
                        _ranges.AddRange(generatedRanges);

                        // Update the list view
                        UpdateRangesListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating ranges");
                ApplicationErrorHandler.ShowError("An error occurred while generating ranges.", "Error", _logger, ex, this);
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
            }, _logger, "testing CC range mapping", this);
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
            }, _logger, "saving CC range mapping", this);
        }

        #endregion
    }
}
