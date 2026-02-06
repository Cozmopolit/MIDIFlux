using System;
using MIDIFlux.Core.Models;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// View model for a preview event
    /// </summary>
    public class PreviewEventViewModel
    {
        /// <summary>
        /// Gets or sets the timestamp of the event
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Gets or sets the event type
        /// </summary>
        public string EventType { get; set; }

        /// <summary>
        /// Gets or sets the trigger information
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// Gets or sets the action information
        /// </summary>
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName { get; set; }

        /// <summary>
        /// Creates a new instance of the PreviewEventViewModel class
        /// </summary>
        public PreviewEventViewModel()
        {
            Timestamp = DateTime.Now;
            EventType = string.Empty;
            Trigger = string.Empty;
            Action = string.Empty;
            DeviceName = string.Empty;
        }

        /// <summary>
        /// Creates a new instance of the PreviewEventViewModel class from a MIDI event
        /// </summary>
        /// <param name="midiEvent">The MIDI event</param>
        /// <param name="deviceName">The device name</param>
        /// <param name="action">The action description</param>
        public PreviewEventViewModel(MidiEvent midiEvent, string deviceName, string action)
        {
            Timestamp = DateTime.Now;
            DeviceName = deviceName;
            Action = action;

            switch (midiEvent.EventType)
            {
                case MidiEventType.NoteOn:
                    EventType = "Note On";
                    Trigger = $"Note {midiEvent.Note} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.NoteOff:
                    EventType = "Note Off";
                    Trigger = $"Note {midiEvent.Note} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.ControlChange:
                    EventType = "Control Change";
                    Trigger = $"CC {midiEvent.Controller} = {midiEvent.Value} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.ProgramChange:
                    EventType = "Program Change";
                    Trigger = $"Program {midiEvent.ProgramNumber} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.PitchBend:
                    EventType = "Pitch Bend";
                    Trigger = $"Value {midiEvent.PitchBendValue} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.ChannelPressure:
                    EventType = "Channel Pressure";
                    Trigger = $"Pressure {midiEvent.Pressure} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.PolyphonicKeyPressure:
                    EventType = "Polyphonic Pressure";
                    Trigger = $"Note {midiEvent.Note}, Pressure {midiEvent.Pressure} (Ch {midiEvent.Channel})";
                    break;
                case MidiEventType.SystemExclusive:
                    EventType = "SysEx";
                    Trigger = $"{midiEvent.SysExData?.Length ?? 0} bytes";
                    break;
                case MidiEventType.Error:
                    EventType = "Error";
                    Trigger = $"{midiEvent.ErrorType}";
                    break;
                default:
                    EventType = midiEvent.EventType.ToString();
                    Trigger = $"Ch {midiEvent.Channel}";
                    break;
            }
        }

        /// <summary>
        /// Creates a simple preview event with the specified information
        /// </summary>
        /// <param name="eventType">The event type</param>
        /// <param name="trigger">The trigger information</param>
        /// <param name="action">The action information</param>
        /// <param name="deviceName">The device name</param>
        /// <returns>A new preview event view model</returns>
        public static PreviewEventViewModel Create(string eventType, string trigger, string action, string deviceName)
        {
            return new PreviewEventViewModel
            {
                Timestamp = DateTime.Now,
                EventType = eventType,
                Trigger = trigger,
                Action = action,
                DeviceName = deviceName
            };
        }
    }
}
