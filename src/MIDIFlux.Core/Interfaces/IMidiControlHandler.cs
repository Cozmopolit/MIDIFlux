using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Interfaces;

/// <summary>
/// Base interface for all MIDI control handlers
/// </summary>
public interface IMidiControlHandler
{
    /// <summary>
    /// Gets the description of this handler for UI and logging
    /// </summary>
    string Description { get; }
}

/// <summary>
/// Handler for absolute value controls (0-127)
/// </summary>
public interface IAbsoluteValueHandler : IMidiControlHandler
{
    /// <summary>
    /// Handles an absolute value from a MIDI control
    /// </summary>
    /// <param name="value">The value (0-127)</param>
    void HandleValue(int value);
}

/// <summary>
/// Handler for relative value controls (increments/decrements)
/// </summary>
public interface IRelativeValueHandler : IMidiControlHandler
{
    /// <summary>
    /// Handles a relative change from a MIDI control
    /// </summary>
    /// <param name="increment">The relative change (positive or negative)</param>
    void HandleIncrement(int increment);
}

/// <summary>
/// Interface for handlers that respond to MIDI note events
/// </summary>
public interface INoteHandler : IMidiControlHandler
{
    /// <summary>
    /// Handles a note on event
    /// </summary>
    /// <param name="velocity">The note velocity (0-127)</param>
    void HandleNoteOn(int velocity);

    /// <summary>
    /// Handles a note off event
    /// </summary>
    void HandleNoteOff();
}
