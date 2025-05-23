using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Controls
{
    /// <summary>
    /// Reusable control for displaying MIDI events in a ListView
    /// </summary>
    public partial class MidiEventDisplayControl : UserControl
    {
        private readonly ILogger _logger;
        private ListView _eventsListView = null!;
        private Label _statusLabel = null!;
        private MidiEventArgs? _selectedEvent;

        /// <summary>
        /// Event raised when a MIDI event is selected
        /// </summary>
        public event EventHandler<MidiEventArgs>? EventSelected;

        /// <summary>
        /// Gets the currently selected MIDI event
        /// </summary>
        public MidiEventArgs? SelectedEvent => _selectedEvent;

        /// <summary>
        /// Gets or sets whether to show the status label
        /// </summary>
        public bool ShowStatusLabel
        {
            get => _statusLabel.Visible;
            set => _statusLabel.Visible = value;
        }

        /// <summary>
        /// Gets or sets the status text
        /// </summary>
        public string StatusText
        {
            get => _statusLabel.Text;
            set => _statusLabel.Text = value;
        }

        /// <summary>
        /// Creates a new MIDI event display control
        /// </summary>
        public MidiEventDisplayControl()
        {
            _logger = MIDIFlux.Core.Helpers.LoggingHelper.CreateLogger<MidiEventDisplayControl>();
            InitializeComponent();
        }

        /// <summary>
        /// Initializes the control components
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();

            // Create status label
            _statusLabel = new Label
            {
                Text = "Ready",
                Dock = DockStyle.Bottom,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(5, 0, 0, 0),
                BackColor = SystemColors.Control,
                BorderStyle = BorderStyle.FixedSingle
            };

            // Create ListView
            _eventsListView = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };

            // Add columns
            _eventsListView.Columns.Add("Time", 100);
            _eventsListView.Columns.Add("Type", 100);
            _eventsListView.Columns.Add("Details", 200);
            _eventsListView.Columns.Add("Channel", 70);
            _eventsListView.Columns.Add("Raw Data", 150);

            // Wire up events
            _eventsListView.SelectedIndexChanged += EventsListView_SelectedIndexChanged;
            _eventsListView.DoubleClick += EventsListView_DoubleClick;

            // Add controls
            Controls.Add(_eventsListView);
            Controls.Add(_statusLabel);

            ResumeLayout(false);
        }

        /// <summary>
        /// Adds a MIDI event to the display
        /// </summary>
        /// <param name="eventArgs">The MIDI event to add</param>
        public void AddEvent(MidiEventArgs eventArgs)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<MidiEventArgs>(AddEvent), eventArgs);
                    return;
                }

                var midiEvent = eventArgs.Event;

                // Create the list view item
                var item = new ListViewItem(new string[]
                {
                    midiEvent.Timestamp.ToString("HH:mm:ss.fff"),
                    midiEvent.EventType.ToString(),
                    GetEventDetails(midiEvent),
                    $"Channel {midiEvent.Channel + 1}",
                    BitConverter.ToString(midiEvent.RawData)
                });

                // Store the event in the tag
                item.Tag = eventArgs;

                // Add the item to the list view (at the top)
                _eventsListView.Items.Insert(0, item);

                // Trim the list if it's too long (keep max 100 items)
                while (_eventsListView.Items.Count > 100)
                {
                    _eventsListView.Items.RemoveAt(_eventsListView.Items.Count - 1);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding MIDI event to display");
            }
        }

        /// <summary>
        /// Clears all events from the display
        /// </summary>
        public void ClearEvents()
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action(ClearEvents));
                    return;
                }

                _eventsListView.Items.Clear();
                _selectedEvent = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing MIDI events from display");
            }
        }

        /// <summary>
        /// Gets a formatted string with event details
        /// </summary>
        private string GetEventDetails(MidiEvent midiEvent)
        {
            return midiEvent.EventType switch
            {
                MidiEventType.NoteOn => $"Note {midiEvent.Note}, Velocity {midiEvent.Velocity}",
                MidiEventType.NoteOff => $"Note {midiEvent.Note}, Velocity {midiEvent.Velocity}",
                MidiEventType.ControlChange => $"CC {midiEvent.Controller}, Value {midiEvent.Value}" +
                                             (midiEvent.IsRelative ? " (Relative)" : ""),
                MidiEventType.Error => $"Error: {midiEvent.ErrorType}",
                MidiEventType.Other => "Other MIDI Event",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Handles selection changes in the events list
        /// </summary>
        private void EventsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            try
            {
                if (_eventsListView.SelectedItems.Count > 0 &&
                    _eventsListView.SelectedItems[0].Tag is MidiEventArgs eventArgs)
                {
                    _selectedEvent = eventArgs;
                    EventSelected?.Invoke(this, eventArgs);
                }
                else
                {
                    _selectedEvent = null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event selection");
            }
        }

        /// <summary>
        /// Handles double-click on events list
        /// </summary>
        private void EventsListView_DoubleClick(object? sender, EventArgs e)
        {
            try
            {
                if (_selectedEvent != null)
                {
                    EventSelected?.Invoke(this, _selectedEvent);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling event double-click");
            }
        }

        /// <summary>
        /// Updates the status text with flood control information
        /// </summary>
        /// <param name="droppedEvents">Number of events dropped due to flood control</param>
        public void UpdateFloodControlStatus(int droppedEvents)
        {
            try
            {
                if (InvokeRequired)
                {
                    Invoke(new Action<int>(UpdateFloodControlStatus), droppedEvents);
                    return;
                }

                StatusText = $"Flood control: {droppedEvents} events dropped in the last second";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating flood control status");
            }
        }
    }
}
