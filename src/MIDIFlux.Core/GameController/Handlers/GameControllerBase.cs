using System;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.GameController.Handlers;

/// <summary>
/// Base class for game controller handlers
/// </summary>
public abstract class GameControllerBase : IDisposable
{
    protected readonly ILogger _logger;
    protected readonly GameControllerManager _controllerManager;
    protected readonly int _controllerIndex;
    protected IXbox360Controller? _controller => _controllerManager.GetController(_controllerIndex);

    /// <summary>
    /// Gets a value indicating whether the ViGEm client is available
    /// </summary>
    public bool IsViGEmAvailable => _controllerManager.IsViGEmAvailable;

    /// <summary>
    /// Gets the controller index
    /// </summary>
    public int ControllerIndex => _controllerIndex;

    /// <summary>
    /// Creates a new instance of the GameControllerBase
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="controllerIndex">The controller index (0-3)</param>
    protected GameControllerBase(ILogger logger, int controllerIndex = 0)
    {
        _logger = logger;
        _controllerIndex = Math.Clamp(controllerIndex, 0, 3);
        _controllerManager = GameControllerManager.GetInstance(logger);
    }

    // Static dictionary for button name mapping
    private static readonly Dictionary<string, Xbox360Button> _buttonMappings = new(StringComparer.OrdinalIgnoreCase)
    {
        { "a", Xbox360Button.A },
        { "b", Xbox360Button.B },
        { "x", Xbox360Button.X },
        { "y", Xbox360Button.Y },
        { "leftshoulder", Xbox360Button.LeftShoulder },
        { "rightshoulder", Xbox360Button.RightShoulder },
        { "back", Xbox360Button.Back },
        { "start", Xbox360Button.Start },
        { "leftthumb", Xbox360Button.LeftThumb },
        { "rightthumb", Xbox360Button.RightThumb },
        { "dpadup", Xbox360Button.Up },
        { "up", Xbox360Button.Up },
        { "dpaddown", Xbox360Button.Down },
        { "down", Xbox360Button.Down },
        { "dpadleft", Xbox360Button.Left },
        { "left", Xbox360Button.Left },
        { "dpadright", Xbox360Button.Right },
        { "right", Xbox360Button.Right },
        { "guide", Xbox360Button.Guide }
    };

    /// <summary>
    /// Maps a button name to an Xbox 360 button
    /// </summary>
    /// <param name="buttonName">The button name</param>
    /// <returns>The Xbox 360 button, or null if the name is invalid</returns>
    protected Xbox360Button? MapButtonName(string buttonName)
    {
        // Use dictionary lookup (case-insensitive)
        if (_buttonMappings.TryGetValue(buttonName, out var button))
        {
            return button;
        }

        // Return null if button name is not found
        return null;
    }

    // MapAxisName method has been moved to GameControllerAxisHandler as MapAxisNameToInfo

    /// <summary>
    /// Converts a MIDI value to an Xbox 360 axis value
    /// </summary>
    /// <remarks>
    /// This method performs the following conversions:
    /// 1. Clamps the input value to the specified range [minValue, maxValue]
    /// 2. Normalizes the value to a 0.0-1.0 range
    /// 3. Optionally inverts the normalized value (1.0 becomes 0.0, 0.0 becomes 1.0)
    /// 4. Maps the normalized value to the Xbox 360 axis range (-32768 to 32767)
    ///    - 0.0 maps to -32768 (full left/up)
    ///    - 0.5 maps to 0 (center position)
    ///    - 1.0 maps to 32767 (full right/down)
    /// </remarks>
    /// <param name="value">The MIDI value (typically 0-127, but can be any range)</param>
    /// <param name="minValue">The minimum value in the input range</param>
    /// <param name="maxValue">The maximum value in the input range</param>
    /// <param name="invert">Whether to invert the axis (e.g., to make up become down)</param>
    /// <returns>The Xbox 360 axis value in the range -32768 to 32767</returns>
    protected short ConvertToAxisValue(int value, int minValue, int maxValue, bool invert)
    {
        // Ensure value is within range
        value = Math.Clamp(value, minValue, maxValue);

        // Normalize to 0.0-1.0
        float normalized = (value - minValue) / (float)(maxValue - minValue);

        // Invert if needed
        if (invert)
        {
            normalized = 1.0f - normalized;
        }

        // Convert to Xbox 360 axis range (-32768 to 32767)
        // Multiply by 65535 to get the full range (0 to 65535), then subtract 32768 to center at 0
        return (short)(normalized * 65535 - 32768);
    }

    /// <summary>
    /// Converts a MIDI value to an Xbox 360 trigger value
    /// </summary>
    /// <remarks>
    /// This method performs the following conversions:
    /// 1. Clamps the input value to the specified range [minValue, maxValue]
    /// 2. Normalizes the value to a 0.0-1.0 range
    /// 3. Optionally inverts the normalized value (1.0 becomes 0.0, 0.0 becomes 1.0)
    /// 4. Maps the normalized value to the Xbox 360 trigger range (0 to 255)
    ///    - 0.0 maps to 0 (not pressed)
    ///    - 0.5 maps to 127 (half pressed)
    ///    - 1.0 maps to 255 (fully pressed)
    ///
    /// Note: Xbox 360 triggers are unidirectional (unlike axes which are bidirectional),
    /// so they only have a positive range from 0 (not pressed) to 255 (fully pressed).
    /// </remarks>
    /// <param name="value">The MIDI value (typically 0-127, but can be any range)</param>
    /// <param name="minValue">The minimum value in the input range</param>
    /// <param name="maxValue">The maximum value in the input range</param>
    /// <param name="invert">Whether to invert the trigger (e.g., to make high values become low)</param>
    /// <returns>The Xbox 360 trigger value in the range 0 to 255</returns>
    protected byte ConvertToTriggerValue(int value, int minValue, int maxValue, bool invert)
    {
        // Ensure value is within range
        value = Math.Clamp(value, minValue, maxValue);

        // Normalize to 0.0-1.0
        float normalized = (value - minValue) / (float)(maxValue - minValue);

        // Invert if needed
        if (invert)
        {
            normalized = 1.0f - normalized;
        }

        // Convert to Xbox 360 trigger range (0 to 255)
        return (byte)(normalized * 255);
    }

    /// <summary>
    /// Disposes the game controller
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the game controller
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        // No need to dispose the controller or client here
        // They are managed by the GameControllerManager singleton
    }
}
