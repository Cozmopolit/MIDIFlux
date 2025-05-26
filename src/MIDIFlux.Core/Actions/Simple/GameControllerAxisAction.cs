using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for setting a game controller axis value.
/// Implements sync-by-default execution for performance.
/// Uses existing ViGEm integration in GameController directory.
/// </summary>
public class GameControllerAxisAction : ActionBase<GameControllerAxisConfig>
{
    private readonly string _axisName;
    private readonly float _axisValue;
    private readonly int _controllerIndex;
    private readonly bool _useMidiValue;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly bool _invert;
    private readonly GameControllerManager _controllerManager;
    private readonly AxisInfo? _axisInfo;

    /// <summary>
    /// Gets the axis name for this action
    /// </summary>
    public string AxisName => _axisName;

    /// <summary>
    /// Gets the axis value for this action
    /// </summary>
    public float AxisValue => _axisValue;

    /// <summary>
    /// Gets the controller index for this action
    /// </summary>
    public int ControllerIndex => _controllerIndex;

    /// <summary>
    /// Initializes a new instance of GameControllerAxisAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public GameControllerAxisAction(GameControllerAxisConfig config) : base(config)
    {
        _axisName = config.AxisName;
        _axisValue = config.AxisValue;
        _controllerIndex = config.ControllerIndex;
        _useMidiValue = config.UseMidiValue;
        _minValue = config.MinValue;
        _maxValue = config.MaxValue;
        _invert = config.Invert;

        // Initialize game controller manager
        _controllerManager = GameControllerManager.GetInstance(Logger);
        _axisInfo = MapAxisNameToInfo(config.AxisName);

        if (_axisInfo == null)
        {
            Logger.LogWarning("Invalid axis name: {AxisName}. Axis will not work.", config.AxisName);
        }
    }

    /// <summary>
    /// Maps an axis name to axis information
    /// </summary>
    /// <param name="axisName">The axis name</param>
    /// <returns>The axis information, or null if the name is invalid</returns>
    private AxisInfo? MapAxisNameToInfo(string axisName)
    {
        return axisName.ToLowerInvariant() switch
        {
            "leftx" or "leftthumbx" => new AxisInfo(Xbox360Axis.LeftThumbX),
            "lefty" or "leftthumby" => new AxisInfo(Xbox360Axis.LeftThumbY),
            "rightx" or "rightthumbx" => new AxisInfo(Xbox360Axis.RightThumbX),
            "righty" or "rightthumby" => new AxisInfo(Xbox360Axis.RightThumbY),
            "lefttrigger" => new AxisInfo(Xbox360Slider.LeftTrigger),
            "righttrigger" => new AxisInfo(Xbox360Slider.RightTrigger),
            _ => null
        };
    }

    /// <summary>
    /// Converts a MIDI value to a trigger value (0-255)
    /// </summary>
    private static byte ConvertToTriggerValue(int midiValue, int minValue, int maxValue, bool invert)
    {
        // Clamp the MIDI value to the specified range
        int clampedValue = Math.Clamp(midiValue, minValue, maxValue);

        // Map to 0-255 range
        float normalizedValue = (float)(clampedValue - minValue) / (maxValue - minValue);

        // Apply inversion if requested
        if (invert)
        {
            normalizedValue = 1.0f - normalizedValue;
        }

        // Convert to byte range (0-255)
        return (byte)Math.Round(normalizedValue * 255.0f);
    }

    /// <summary>
    /// Converts a MIDI value to an axis value (-32768 to 32767)
    /// </summary>
    private static short ConvertToAxisValue(int midiValue, int minValue, int maxValue, bool invert)
    {
        // Clamp the MIDI value to the specified range
        int clampedValue = Math.Clamp(midiValue, minValue, maxValue);

        // Map to -1.0 to 1.0 range
        float normalizedValue = (float)(clampedValue - minValue) / (maxValue - minValue);
        normalizedValue = (normalizedValue * 2.0f) - 1.0f; // Convert 0-1 to -1 to 1

        // Apply inversion if requested
        if (invert)
        {
            normalizedValue = -normalizedValue;
        }

        // Convert to short range (-32768 to 32767)
        return (short)Math.Round(normalizedValue * 32767.0f);
    }

    /// <summary>
    /// Core execution logic for the game controller axis action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Check if ViGEm is available
        if (!_controllerManager.IsViGEmAvailable)
        {
            var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Get the controller instance
        var controller = _controllerManager.GetController(_controllerIndex);
        if (controller == null)
        {
            var errorMsg = $"Failed to get controller instance for index {_controllerIndex}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Determine the value to use
        int valueToUse;
        if (_useMidiValue && midiValue.HasValue)
        {
            // Use the MIDI value with range mapping
            valueToUse = midiValue.Value;
        }
        else
        {
            // Convert the fixed axis value to MIDI range for consistency with existing handler
            valueToUse = (int)Math.Round(_axisValue * 127.0f);
        }

        // Check if axis mapping is valid
        if (_axisInfo == null)
        {
            var errorMsg = $"Invalid axis name: {_axisName}. Axis will not work.";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Apply the axis value directly
        if (_axisInfo.IsSlider)
        {
            // Handle trigger
            byte triggerValue = ConvertToTriggerValue(valueToUse, _minValue, _maxValue, _invert);
            controller.SetSliderValue(_axisInfo.Slider, triggerValue);
            Logger.LogDebug("Set game controller trigger {AxisName} to {Value}", _axisName, triggerValue);
        }
        else
        {
            // Handle axis
            short axisValue = ConvertToAxisValue(valueToUse, _minValue, _maxValue, _invert);
            controller.SetAxisValue(_axisInfo.Axis, axisValue);
            Logger.LogDebug("Set game controller axis {AxisName} to {Value}", _axisName, axisValue);
        }

        Logger.LogTrace("Successfully executed GameControllerAxisAction for Axis={AxisName}, ControllerIndex={ControllerIndex}, Value={Value}",
            _axisName, _controllerIndex, valueToUse);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Controller {_controllerIndex + 1} {_axisName} = {(_useMidiValue ? "MIDI Value" : _axisValue.ToString("F2"))}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing GameControllerAxisAction for axis {_axisName} on controller {_controllerIndex}";
    }

    /// <summary>
    /// Information about an axis or slider
    /// </summary>
    private class AxisInfo
    {
        /// <summary>
        /// The Xbox 360 axis (for thumbsticks)
        /// </summary>
        public Xbox360Axis Axis { get; }

        /// <summary>
        /// The Xbox 360 slider (for triggers)
        /// </summary>
        public Xbox360Slider Slider { get; }

        /// <summary>
        /// Whether this is a slider (trigger) or an axis (thumbstick)
        /// </summary>
        public bool IsSlider { get; }

        /// <summary>
        /// Creates a new instance for an axis (thumbstick)
        /// </summary>
        /// <param name="axis">The Xbox 360 axis</param>
        public AxisInfo(Xbox360Axis axis)
        {
            Axis = axis;
            Slider = Xbox360Slider.LeftTrigger; // Default value, won't be used
            IsSlider = false;
        }

        /// <summary>
        /// Creates a new instance for a slider (trigger)
        /// </summary>
        /// <param name="slider">The Xbox 360 slider</param>
        public AxisInfo(Xbox360Slider slider)
        {
            Axis = Xbox360Axis.LeftThumbX; // Default value, won't be used
            Slider = slider;
            IsSlider = true;
        }
    }
}
