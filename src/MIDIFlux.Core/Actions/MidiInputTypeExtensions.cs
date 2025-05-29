namespace MIDIFlux.Core.Actions;

/// <summary>
/// Extension methods and helper utilities for MidiInputType.
/// Provides mapping between technical MIDI types and logical input categories.
/// </summary>
public static class MidiInputTypeExtensions
{
    /// <summary>
    /// Maps a MidiInputType to its corresponding InputTypeCategory.
    /// This mapping determines which actions are compatible with each input type.
    /// </summary>
    /// <param name="inputType">The MIDI input type to categorize</param>
    /// <returns>The logical category for action compatibility filtering</returns>
    public static InputTypeCategory GetCategory(this MidiInputType inputType)
    {
        return inputType switch
        {
            // Trigger Signals (Binary/Event-based)
            MidiInputType.NoteOn => InputTypeCategory.Trigger,
            MidiInputType.NoteOff => InputTypeCategory.Trigger,
            MidiInputType.SysEx => InputTypeCategory.Trigger,
            MidiInputType.ProgramChange => InputTypeCategory.Trigger,

            // Absolute Value Signals (Range-based)
            MidiInputType.ControlChangeAbsolute => InputTypeCategory.AbsoluteValue,
            MidiInputType.PitchBend => InputTypeCategory.AbsoluteValue,
            MidiInputType.Aftertouch => InputTypeCategory.AbsoluteValue,
            MidiInputType.ChannelPressure => InputTypeCategory.AbsoluteValue,

            // Relative Value Signals (Delta-based)
            MidiInputType.ControlChangeRelative => InputTypeCategory.RelativeValue,

            _ => throw new ArgumentOutOfRangeException(nameof(inputType), inputType, "Unknown MIDI input type")
        };
    }

    /// <summary>
    /// Gets all MidiInputType values that belong to a specific category.
    /// Useful for reverse lookups and validation.
    /// </summary>
    /// <param name="category">The category to get input types for</param>
    /// <returns>Array of MIDI input types in the specified category</returns>
    public static MidiInputType[] GetInputTypesInCategory(InputTypeCategory category)
    {
        return category switch
        {
            InputTypeCategory.Trigger => new[]
            {
                MidiInputType.NoteOn,
                MidiInputType.NoteOff,
                MidiInputType.SysEx,
                MidiInputType.ProgramChange
            },
            InputTypeCategory.AbsoluteValue => new[]
            {
                MidiInputType.ControlChangeAbsolute,
                MidiInputType.PitchBend,
                MidiInputType.Aftertouch,
                MidiInputType.ChannelPressure
            },
            InputTypeCategory.RelativeValue => new[]
            {
                MidiInputType.ControlChangeRelative
            },
            _ => throw new ArgumentOutOfRangeException(nameof(category), category, "Unknown input type category")
        };
    }

    /// <summary>
    /// Gets a user-friendly display name for a MidiInputType.
    /// Used in GUI dropdowns and user-facing messages.
    /// </summary>
    /// <param name="inputType">The MIDI input type</param>
    /// <returns>Human-readable display name</returns>
    public static string GetDisplayName(this MidiInputType inputType)
    {
        return inputType switch
        {
            MidiInputType.NoteOn => "Note On",
            MidiInputType.NoteOff => "Note Off",
            MidiInputType.ControlChangeAbsolute => "Control Change (Absolute)",
            MidiInputType.ControlChangeRelative => "Control Change (Relative)",
            MidiInputType.ProgramChange => "Program Change",
            MidiInputType.PitchBend => "Pitch Bend",
            MidiInputType.Aftertouch => "Aftertouch",
            MidiInputType.ChannelPressure => "Channel Pressure",
            MidiInputType.SysEx => "SysEx",
            _ => inputType.ToString()
        };
    }

    /// <summary>
    /// Gets a tooltip description explaining the characteristics of a MidiInputType.
    /// Used in GUI to help users understand the difference between input types.
    /// </summary>
    /// <param name="inputType">The MIDI input type</param>
    /// <returns>Descriptive tooltip text</returns>
    public static string GetTooltip(this MidiInputType inputType)
    {
        return inputType switch
        {
            MidiInputType.NoteOn => "Key press events from keyboards, drum pads, or buttons",
            MidiInputType.NoteOff => "Key release events from keyboards, drum pads, or buttons",
            MidiInputType.ControlChangeAbsolute => "Absolute position controllers like faders, absolute knobs (0-127 range)",
            MidiInputType.ControlChangeRelative => "Relative movement controllers like endless encoders, scratch wheels, jog wheels",
            MidiInputType.ProgramChange => "Preset selection messages (discrete values used as triggers)",
            MidiInputType.PitchBend => "Pitch wheel movements (14-bit values treated as 0-127 range)",
            MidiInputType.Aftertouch => "Pressure-sensitive key events (0-127 pressure values)",
            MidiInputType.ChannelPressure => "Channel-wide pressure events (0-127 pressure values)",
            MidiInputType.SysEx => "Device-specific system exclusive messages",
            _ => "Unknown MIDI input type"
        };
    }
}
