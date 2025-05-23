using System;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Event arguments for MIDI events that include the device ID
/// </summary>
public class MidiEventArgs : EventArgs
{
    /// <summary>
    /// The ID of the MIDI device that generated the event
    /// </summary>
    public int DeviceId { get; }
    
    /// <summary>
    /// The MIDI event data
    /// </summary>
    public MidiEvent Event { get; }
    
    /// <summary>
    /// Creates a new instance of MidiEventArgs
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device</param>
    /// <param name="midiEvent">The MIDI event data</param>
    public MidiEventArgs(int deviceId, MidiEvent midiEvent)
    {
        DeviceId = deviceId;
        Event = midiEvent;
    }
}
