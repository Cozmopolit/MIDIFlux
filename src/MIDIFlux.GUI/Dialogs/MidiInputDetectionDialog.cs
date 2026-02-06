using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for detecting and monitoring MIDI input
    /// </summary>
    public partial class MidiInputDetectionDialog : BaseDialog
    {
        private readonly MidiDeviceManager _MidiDeviceManager;
        private readonly List<MidiEventArgs> _recentEvents = new();
        private readonly int _maxEvents = 100;
        private string? _selectedDeviceId = null;
        private bool _isListening = false;
        private bool _listenToAllDevices = false;

        /// <summary>
        /// Gets the selected MIDI event
        /// </summary>
        public MidiEvent? SelectedMidiEvent { get; private set; }

        /// <summary>
        /// Gets the selected device ID (null means no specific device or all devices)
        /// </summary>
        public string? SelectedDeviceId => _selectedDeviceId;

        /// <summary>
        /// Gets the selected device name
        /// </summary>
        public string SelectedDeviceName => deviceComboBox.SelectedItem?.ToString() ?? string.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="MidiInputDetectionDialog"/> class
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="MidiDeviceManager">The MIDI manager</param>
        public MidiInputDetectionDialog(ILogger logger, MidiDeviceManager MidiDeviceManager) : base(logger)
        {
            InitializeComponent();
            _MidiDeviceManager = MidiDeviceManager;

            // Set up the dialog
            Text = "MIDI Input Detection";
            MinimumSize = new Size(600, 400);
            Size = new Size(700, 500);

            // Initialize the UI synchronization context
            MIDIFlux.GUI.Helpers.UISynchronizationHelper.Initialize(System.Threading.SynchronizationContext.Current);
            _logger.LogDebug("UI synchronization context initialized in MidiInputDetectionDialog");

            // Set up the event handlers
            Load += MidiInputDetectionDialog_Load;
            FormClosing += MidiInputDetectionDialog_FormClosing;
        }

        /// <summary>
        /// Handles the Load event of the dialog
        /// </summary>
        private void MidiInputDetectionDialog_Load(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Populate the device combo box
                PopulateDeviceComboBox();

                // Set up the event handler for MIDI events
                _MidiDeviceManager.MidiEventReceived += MidiDeviceManager_MidiEventReceived;

                // Update the UI
                UpdateUI();
            }, _logger, "loading MIDI Input Detection dialog", this);
        }

        /// <summary>
        /// Handles the FormClosing event of the dialog
        /// </summary>
        private void MidiInputDetectionDialog_FormClosing(object? sender, FormClosingEventArgs e)
        {
            try
            {
                // Stop listening for MIDI events
                StopListening();

                // Unsubscribe from MIDI events
                _MidiDeviceManager.MidiEventReceived -= MidiDeviceManager_MidiEventReceived;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing MIDI Input Detection dialog");
                // Don't show an error message here as the dialog is closing
            }
        }

        /// <summary>
        /// Populates the device combo box with available MIDI devices
        /// </summary>
        private void PopulateDeviceComboBox()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Use centralized helper for device combo box population
                Helpers.MidiDeviceComboBoxHelper.PopulateDetectionDeviceComboBox(
                    deviceComboBox,
                    _MidiDeviceManager,
                    _logger);
            }, _logger, "populating device combo box", this);
        }

        /// <summary>
        /// Updates the UI based on the current state
        /// </summary>
        private void UpdateUI()
        {
            try
            {
                // Update the listen button text
                listenButton.Text = _isListening ? "Stop Listening" : "Start Listening";

                // Enable/disable the copy button based on whether an event is selected
                copyButton.Enabled = _isListening && eventsListView.SelectedItems.Count > 0;

                // Update the status label
                statusLabel.Text = _isListening
                    ? $"Listening to {deviceComboBox.SelectedItem}"
                    : "Not listening";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating UI");
                // Don't show an error message for this minor issue
            }
        }

        /// <summary>
        /// Starts listening for MIDI events from the selected device
        /// </summary>
        private void StartListening()
        {
            try
            {
                // Get the selected device
                if (deviceComboBox.SelectedItem is MidiDeviceInfo deviceInfo)
                {
                    _selectedDeviceId = deviceInfo.DeviceId;

                    // Clear the events list view
                    ClearEventsListView();

                    // Start listening to the device
                    bool success = _MidiDeviceManager.StartListening(_selectedDeviceId);
                    if (success)
                    {
                        _isListening = true;
                        _logger.LogInformation("Started listening to MIDI device: {Device}", deviceInfo);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to start listening to MIDI device: {Device}", deviceInfo);
                        ApplicationErrorHandler.ShowError($"Failed to start listening to MIDI device: {deviceInfo}", "Error", _logger, null, this);
                    }
                }
                else
                {
                    _logger.LogWarning("No valid MIDI device selected");
                    ApplicationErrorHandler.ShowError("No valid MIDI device selected.", "Error", _logger, null, this);
                }

                // Update the UI
                UpdateUI();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MIDI listening");
                ApplicationErrorHandler.ShowError("An error occurred while starting MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Stops listening for MIDI events
        /// </summary>
        private void StopListening()
        {
            try
            {
                if (_selectedDeviceId != null && _isListening)
                {
                    // We don't actually stop listening to the device here, as other parts of the application
                    // might still be using it. We just stop processing events in this dialog.
                    _isListening = false;
                    _logger.LogInformation("Stopped listening to MIDI device ID: {DeviceId}", _selectedDeviceId);
                }

                // Update the UI
                UpdateUI();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MIDI listening");
                ApplicationErrorHandler.ShowError("An error occurred while stopping MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles MIDI events from the MIDI manager
        /// </summary>
        private void MidiDeviceManager_MidiEventReceived(object? sender, MidiEventArgs e)
        {
            try
            {
                // Only process events if we're listening
                if (!_isListening)
                {
                    return;
                }

                // If we're not listening to all devices, check if the event is from the selected device
                if (!_listenToAllDevices && e.DeviceId != _selectedDeviceId)
                {
                    return;
                }

                // Add the event to the list
                AddEventToList(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MIDI event");
                // Don't show an error message for this as it could flood the UI
            }
        }



        /// <summary>
        /// Adds a MIDI event to the list
        /// </summary>
        /// <param name="e">The MIDI event arguments</param>
        private void AddEventToList(MidiEventArgs e)
        {
            try
            {
                // Add the event to the recent events list
                _recentEvents.Add(e);

                // Trim the list if it's too long - remove oldest events (at the beginning)
                while (_recentEvents.Count > _maxEvents)
                {
                    _recentEvents.RemoveAt(0);
                }

                // Add the event to the list view on the UI thread
                try
                {
                    RunOnUI(() =>
                    {
                        try
                        {
                            var midiEvent = e.Event;

                            // Create the list view item
                            var item = new ListViewItem(new string[]
                            {
                                midiEvent.Timestamp.ToString("HH:mm:ss.fff"),
                                midiEvent.EventType.ToString(),
                                GetEventDetails(midiEvent),
                                $"Channel {midiEvent.Channel}",  // Channel is already 1-based from MidiEventConverter
                                BitConverter.ToString(midiEvent.RawData)
                            });

                            // Store the event in the tag
                            item.Tag = e;

                            // Add the item to the list view
                            eventsListView.Items.Insert(0, item);

                            // Trim the list if it's too long
                            while (eventsListView.Items.Count > _maxEvents)
                            {
                                eventsListView.Items.RemoveAt(eventsListView.Items.Count - 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error adding event to list view");
                        }
                    });
                }
                catch (InvalidOperationException ex) when (ex.Message.Contains("UI synchronization context has not been initialized"))
                {
                    // If the UI synchronization context is not initialized, log the error and continue
                    _logger.LogWarning(ex, "UI synchronization context not initialized in AddEventToList");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding event to list");
            }
        }

        /// <summary>
        /// Gets the details of a MIDI event as a string
        /// </summary>
        /// <param name="midiEvent">The MIDI event</param>
        /// <returns>The event details as a string</returns>
        private string GetEventDetails(MidiEvent midiEvent)
        {
            return midiEvent.EventType switch
            {
                MidiEventType.NoteOn => $"Note {midiEvent.Note} Velocity {midiEvent.Velocity}",
                MidiEventType.NoteOff => $"Note {midiEvent.Note} Off",
                MidiEventType.ControlChange => $"CC {midiEvent.Controller} Value {midiEvent.Value}" + (midiEvent.IsRelative ? " (Relative)" : ""),
                MidiEventType.Error => $"Error: {midiEvent.ErrorType}",
                _ => "Other MIDI Event"
            };
        }

        /// <summary>
        /// Clears the events list view and resets the recent events list
        /// </summary>
        private void ClearEventsListView()
        {
            try
            {
                // Clear the recent events list
                _recentEvents.Clear();

                // Clear the list view on the UI thread
                RunOnUI(() =>
                {
                    try
                    {
                        eventsListView.Items.Clear();
                        _logger.LogDebug("Events list view cleared");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error clearing events list view");
                    }
                });
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("UI synchronization context has not been initialized"))
            {
                // If the UI synchronization context is not initialized, log the error and continue
                _logger.LogWarning(ex, "UI synchronization context not initialized in ClearEventsListView");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing events list");
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
                    StopListening();
                }
                else
                {
                    StartListening();
                }
            }, _logger, "toggling MIDI listening", this);
        }

        /// <summary>
        /// Handles the Click event of the RefreshButton
        /// </summary>
        private void RefreshButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Refresh the device list
                _MidiDeviceManager.RefreshDeviceList();
                PopulateDeviceComboBox();
            }, _logger, "refreshing device list", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DeviceComboBox
        /// </summary>
        private void DeviceComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // If we're listening, stop listening to the current device
                if (_isListening)
                {
                    StopListening();
                }

                // Clear the events list view
                ClearEventsListView();

                // Update the UI
                UpdateUI();
            }, _logger, "changing selected device", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the EventsListView
        /// </summary>
        private void EventsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                // Enable/disable the copy button based on whether an event is selected
                copyButton.Enabled = _isListening && eventsListView.SelectedItems.Count > 0;

                // Update the selected MIDI event
                if (eventsListView.SelectedItems.Count > 0 && eventsListView.SelectedItems[0].Tag is MidiEventArgs eventArgs)
                {
                    SelectedMidiEvent = eventArgs.Event;
                }
                else
                {
                    SelectedMidiEvent = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event selection");
                // Don't show an error message for this minor issue
            }
        }

        /// <summary>
        /// Handles the Click event of the CopyButton
        /// </summary>
        private void CopyButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Check if an event is selected
                if (eventsListView.SelectedItems.Count > 0 && eventsListView.SelectedItems[0].Tag is MidiEventArgs eventArgs)
                {
                    // Set the selected MIDI event
                    SelectedMidiEvent = eventArgs.Event;
                    _selectedDeviceId = eventArgs.DeviceId;

                    // Close the dialog with OK result
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    ApplicationErrorHandler.ShowError("No MIDI event selected.", "Error", _logger, null, this);
                }
            }, _logger, "copying MIDI event", this);
        }

        /// <summary>
        /// Handles the Click event of the CloseButton
        /// </summary>
        private void CloseButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Close the dialog with Cancel result
                DialogResult = DialogResult.Cancel;
                Close();
            }, _logger, "closing dialog", this);
        }

        /// <summary>
        /// Handles the CheckedChanged event of the ListenAllCheckBox
        /// </summary>
        private void ListenAllCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            try
            {
                // Update the listen to all devices flag
                _listenToAllDevices = listenAllCheckBox.Checked;

                // Update the UI
                deviceComboBox.Enabled = !_listenToAllDevices;

                // Update the status label
                if (_isListening)
                {
                    statusLabel.Text = _listenToAllDevices
                        ? "Listening to all MIDI devices"
                        : $"Listening to {deviceComboBox.SelectedItem}";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing listen all devices setting");
                ApplicationErrorHandler.ShowError("An error occurred while changing the listen all devices setting.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearButton
        /// </summary>
        private void ClearButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Clear the events list
                _recentEvents.Clear();
                eventsListView.Items.Clear();

            }, _logger, "clearing events", this);
        }
    }
}
