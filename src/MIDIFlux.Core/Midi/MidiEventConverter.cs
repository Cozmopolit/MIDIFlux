using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using NAudio.Midi;
using MidiEvent = MIDIFlux.Core.Models.MidiEvent;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Converts NAudio MIDI events to internal MIDIFlux event format
/// </summary>
public class MidiEventConverter
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the MidiEventConverter
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public MidiEventConverter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Creates a MidiEvent from a NAudio MIDI event
    /// </summary>
    /// <param name="e">The NAudio MIDI event</param>
    /// <returns>The converted MIDI event</returns>
    public MidiEvent CreateMidiEventFromNAudio(MidiInMessageEventArgs e)
    {
        try
        {
            var midiEvent = new MidiEvent
            {
                Timestamp = DateTime.Now,
                RawData = BitConverter.GetBytes(e.RawMessage),
                Channel = e.MidiEvent.Channel
            };

            // Process different types of MIDI events
            switch (e.MidiEvent.CommandCode)
            {
                case MidiCommandCode.NoteOn:
                    if (e.MidiEvent is NoteEvent noteEvent)
                    {
                        ProcessNoteOn(midiEvent, noteEvent);
                    }
                    break;

                case MidiCommandCode.NoteOff:
                    if (e.MidiEvent is NoteEvent noteOff)
                    {
                        ProcessNoteOff(midiEvent, noteOff);
                    }
                    break;

                case MidiCommandCode.ControlChange:
                    if (e.MidiEvent is ControlChangeEvent cc)
                    {
                        ProcessControlChange(midiEvent, cc);
                    }
                    break;

                default:
                    midiEvent.EventType = MidiEventType.Other;
                    break;
            }

            return midiEvent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting MIDI event: {Message}", ex.Message);

            // Return an error event if conversion fails
            return new MidiEvent
            {
                Timestamp = DateTime.Now,
                RawData = BitConverter.GetBytes(e.RawMessage),
                EventType = MidiEventType.Error,
                ErrorType = MidiErrorType.ProcessingError
            };
        }
    }

    /// <summary>
    /// Processes a Note On MIDI event
    /// </summary>
    /// <param name="midiEvent">The MIDI event to populate</param>
    /// <param name="noteEvent">The NAudio note event</param>
    private void ProcessNoteOn(MidiEvent midiEvent, NoteEvent noteEvent)
    {
        try
        {
            // Check if it's a Note On or Note Off event (Note On with velocity 0 is treated as Note Off)
            bool isNoteOn = noteEvent is NoteOnEvent noteOn && noteOn.Velocity > 0;

            if (isNoteOn)
            {
                midiEvent.EventType = MidiEventType.NoteOn;
                midiEvent.Note = noteEvent.NoteNumber;
                midiEvent.Velocity = noteEvent is NoteOnEvent on ? on.Velocity : 127;
            }
            else
            {
                midiEvent.EventType = MidiEventType.NoteOff;
                midiEvent.Note = noteEvent.NoteNumber;
                midiEvent.Velocity = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NoteOn event: {Message}", ex.Message);
            midiEvent.EventType = MidiEventType.Error;
            midiEvent.ErrorType = MidiErrorType.ProcessingError;
        }
    }

    /// <summary>
    /// Processes a Note Off MIDI event
    /// </summary>
    /// <param name="midiEvent">The MIDI event to populate</param>
    /// <param name="noteEvent">The NAudio note event</param>
    private void ProcessNoteOff(MidiEvent midiEvent, NoteEvent noteEvent)
    {
        try
        {
            midiEvent.EventType = MidiEventType.NoteOff;
            midiEvent.Note = noteEvent.NoteNumber;
            midiEvent.Velocity = 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing NoteOff event: {Message}", ex.Message);
            midiEvent.EventType = MidiEventType.Error;
            midiEvent.ErrorType = MidiErrorType.ProcessingError;
        }
    }

    /// <summary>
    /// Processes a Control Change MIDI event
    /// </summary>
    /// <param name="midiEvent">The MIDI event to populate</param>
    /// <param name="cc">The NAudio control change event</param>
    private void ProcessControlChange(MidiEvent midiEvent, ControlChangeEvent cc)
    {
        try
        {
            midiEvent.EventType = MidiEventType.ControlChange;
            midiEvent.Controller = (int)cc.Controller;
            midiEvent.Value = cc.ControllerValue;

            // Check if this is a relative control
            DetectRelativeControl(midiEvent, cc);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing ControlChange event: {Message}", ex.Message);
            midiEvent.EventType = MidiEventType.Error;
            midiEvent.ErrorType = MidiErrorType.ProcessingError;
        }
    }

    /// <summary>
    /// Detects if a control change event is from a relative control (like a jog wheel)
    /// </summary>
    /// <param name="midiEvent">The MIDI event to check</param>
    /// <param name="cc">The control change event</param>
    private void DetectRelativeControl(MidiEvent midiEvent, ControlChangeEvent cc)
    {
        // For the Traktor Kontrol S2 MK3, controller 30 on channel 4 is the jog wheel
        // This could be expanded to handle other relative controls in the future
        if (midiEvent.Channel == 4 && (int)cc.Controller == 30)
        {
            midiEvent.IsRelative = true;
            midiEvent.RelativeEncoding = RelativeValueEncoding.BinaryOffset;
            _logger.LogDebug("Detected relative control: Channel={Channel}, Controller={Controller}",
                midiEvent.Channel, cc.Controller);
        }
    }

    /// <summary>
    /// Creates a MIDI error event
    /// </summary>
    /// <param name="rawMessage">The raw MIDI message</param>
    /// <returns>The MIDI error event</returns>
    public MidiEvent CreateMidiErrorEvent(int rawMessage)
    {
        return new MidiEvent
        {
            Timestamp = DateTime.Now,
            RawData = BitConverter.GetBytes(rawMessage),
            EventType = MidiEventType.Error
        };
    }

    /// <summary>
    /// Creates a MIDI device disconnection event
    /// </summary>
    /// <returns>The MIDI device disconnection event</returns>
    public MidiEvent CreateDeviceDisconnectionEvent()
    {
        return new MidiEvent
        {
            Timestamp = DateTime.Now,
            EventType = MidiEventType.Error,
            ErrorType = MidiErrorType.DeviceDisconnected
        };
    }
}
