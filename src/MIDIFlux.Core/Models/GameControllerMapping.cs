using System;
using System.Collections.Generic;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a mapping between a MIDI note and a game controller button
/// </summary>
public class GameControllerButtonMapping
{
    /// <summary>
    /// The MIDI note number to map
    /// </summary>
    public int MidiNote { get; set; }

    /// <summary>
    /// The controller button to emulate
    /// Valid values: A, B, X, Y, LeftShoulder, RightShoulder, Back, Start,
    /// LeftThumb, RightThumb, DPadUp, DPadDown, DPadLeft, DPadRight, Guide
    /// </summary>
    public string Button { get; set; } = string.Empty;

    /// <summary>
    /// The controller index (0-3, default: 0)
    /// </summary>
    public int ControllerIndex { get; set; } = 0;
}

/// <summary>
/// Represents a mapping between a MIDI control and a game controller axis
/// </summary>
public class GameControllerAxisMapping
{
    /// <summary>
    /// The MIDI control number to map
    /// </summary>
    public int ControlNumber { get; set; }

    /// <summary>
    /// The controller axis to emulate
    /// Valid values: LeftThumbX, LeftThumbY, RightThumbX, RightThumbY, LeftTrigger, RightTrigger
    /// </summary>
    public string Axis { get; set; } = string.Empty;

    /// <summary>
    /// The minimum MIDI value (default: 0)
    /// </summary>
    public int MinValue { get; set; } = 0;

    /// <summary>
    /// The maximum MIDI value (default: 127)
    /// </summary>
    public int MaxValue { get; set; } = 127;

    /// <summary>
    /// Whether to invert the axis (default: false)
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// The controller index (0-3, default: 0)
    /// </summary>
    public int ControllerIndex { get; set; } = 0;
}

/// <summary>
/// Collection of game controller mappings
/// </summary>
public class GameControllerMappings
{
    /// <summary>
    /// List of button mappings
    /// </summary>
    public List<GameControllerButtonMapping> Buttons { get; set; } = new List<GameControllerButtonMapping>();

    /// <summary>
    /// List of axis mappings
    /// </summary>
    public List<GameControllerAxisMapping> Axes { get; set; } = new List<GameControllerAxisMapping>();

    /// <summary>
    /// The default controller index for all mappings (0-3, default: 0)
    /// </summary>
    public int DefaultControllerIndex { get; set; } = 0;
}
