using System;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Event arguments for MIDI events that include the device ID
/// </summary>
public class MidiEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the MIDI device that generated the event.
    /// Format depends on the MIDI adapter implementation:
    /// NAudio: "0", "1", "2"... (string representation of integer indices)
    /// Windows MIDI Services: Endpoint device ID strings
    /// </summary>
    public string DeviceId { get; }

    /// <summary>
    /// The MIDI event data
    /// </summary>
    public MidiEvent Event { get; }

    /// <summary>
    /// Creates a new instance of MidiEventArgs
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device</param>
    /// <param name="midiEvent">The MIDI event data</param>
    public MidiEventArgs(string deviceId, MidiEvent midiEvent)
    {
        DeviceId = deviceId ?? throw new ArgumentNullException(nameof(deviceId));
        Event = midiEvent ?? throw new ArgumentNullException(nameof(midiEvent));
    }
}
