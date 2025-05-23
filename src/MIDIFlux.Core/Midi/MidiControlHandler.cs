using MIDIFlux.Core.Handlers;
using MIDIFlux.Core.Interfaces;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Represents a handler for a MIDI control
/// </summary>
public class MidiControlHandler
{
    /// <summary>
    /// The control number or note number
    /// </summary>
    public int ControlNumber { get; set; }

    /// <summary>
    /// The type of handler
    /// </summary>
    public MidiHandlerType HandlerType { get; set; }

    /// <summary>
    /// The absolute value handler (if applicable)
    /// </summary>
    public IAbsoluteValueHandler? AbsoluteHandler { get; set; }

    /// <summary>
    /// The relative value handler (if applicable)
    /// </summary>
    public IRelativeValueHandler? RelativeHandler { get; set; }

    /// <summary>
    /// The note handler (if applicable)
    /// </summary>
    public INoteHandler? NoteHandler { get; set; }

    /// <summary>
    /// Gets a description of the handler
    /// </summary>
    public string Description
    {
        get
        {
            return HandlerType switch
            {
                MidiHandlerType.Absolute => AbsoluteHandler?.Description ?? "Unknown",
                MidiHandlerType.Relative => RelativeHandler?.Description ?? "Unknown",
                MidiHandlerType.Note => NoteHandler?.Description ?? "Unknown",
                _ => "Unknown"
            };
        }
    }
}

/// <summary>
/// The type of MIDI handler
/// </summary>
public enum MidiHandlerType
{
    Absolute,
    Relative,
    Note
}
