using System;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Interfaces;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.GameController.Handlers;

/// <summary>
/// Handles mapping MIDI controls to game controller axes
/// </summary>
public class GameControllerAxisHandler : GameControllerBase, IAbsoluteValueHandler, IRelativeValueHandler
{
    private readonly string _axisName;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly bool _invert;

    // Type-safe approach for storing axis information
    private readonly AxisInfo? _axisInfo;
    private int _currentValue;
    private readonly int _sensitivity;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => $"Game Controller {ControllerIndex} Axis: {_axisName}";

    /// <summary>
    /// Creates a new instance of the GameControllerAxisHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="axisName">The name of the axis to emulate</param>
    /// <param name="minValue">The minimum MIDI value (default: 0)</param>
    /// <param name="maxValue">The maximum MIDI value (default: 127)</param>
    /// <param name="invert">Whether to invert the axis (default: false)</param>
    /// <param name="sensitivity">The sensitivity multiplier for relative controls (default: 1)</param>
    /// <param name="controllerIndex">The controller index (0-3)</param>
    public GameControllerAxisHandler(
        ILogger logger,
        string axisName,
        int minValue = 0,
        int maxValue = 127,
        bool invert = false,
        int sensitivity = 1,
        int controllerIndex = 0) : base(logger, controllerIndex)
    {
        _axisName = axisName;
        _minValue = minValue;
        _maxValue = maxValue;
        _invert = invert;
        _sensitivity = sensitivity;
        _axisInfo = MapAxisNameToInfo(axisName);
        _currentValue = (minValue + maxValue) / 2; // Start at middle position

        if (_axisInfo == null)
        {
            _logger.LogWarning("Invalid axis name: {AxisName}. Axis will not work.", axisName);
        }
    }

    /// <summary>
    /// Handles an absolute value from a MIDI control
    /// </summary>
    /// <param name="value">The value (0-127)</param>
    public void HandleValue(int value)
    {
        if (!IsViGEmAvailable || _axisInfo == null || _controller == null)
        {
            _logger.LogDebug("Cannot set axis {AxisName}: ViGEm not available or axis invalid", _axisName);
            return;
        }

        try
        {
            _currentValue = value;

            // Check if this is a slider (trigger) or an axis
            if (_axisInfo.IsSlider)
            {
                // Handle trigger
                byte triggerValue = ConvertToTriggerValue(value, _minValue, _maxValue, _invert);
                _controller.SetSliderValue(_axisInfo.Slider, triggerValue);
                _logger.LogDebug("Set game controller trigger {AxisName} to {Value}", _axisName, triggerValue);
            }
            else
            {
                // Handle axis
                short axisValue = ConvertToAxisValue(value, _minValue, _maxValue, _invert);
                _controller.SetAxisValue(_axisInfo.Axis, axisValue);
                _logger.LogDebug("Set game controller axis {AxisName} to {Value}", _axisName, axisValue);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to set axis {AxisName}: {Message}", _axisName, ex.Message);
        }
    }

    /// <summary>
    /// Handles a relative change from a MIDI control
    /// </summary>
    /// <param name="increment">The relative change (positive or negative)</param>
    public void HandleIncrement(int increment)
    {
        if (!IsViGEmAvailable || _axisInfo == null || _controller == null)
        {
            _logger.LogDebug("Cannot adjust axis {AxisName}: ViGEm not available or axis invalid", _axisName);
            return;
        }

        try
        {
            // Apply sensitivity
            increment *= _sensitivity;

            // Update current value
            _currentValue = Math.Clamp(_currentValue + increment, _minValue, _maxValue);

            // Apply the value
            HandleValue(_currentValue);
        }
        catch (Exception ex)
        {
            _logger.LogError("Failed to adjust axis {AxisName}: {Message}", _axisName, ex.Message);
        }
    }

    /// <summary>
    /// Maps an axis name to axis information
    /// </summary>
    /// <param name="axisName">The axis name</param>
    /// <returns>Axis information, or null if the name is invalid</returns>
    private static AxisInfo? MapAxisNameToInfo(string axisName)
    {
        return axisName.ToLowerInvariant() switch
        {
            // Thumb stick axes
            "leftthumbx" => new AxisInfo(Xbox360Axis.LeftThumbX),
            "leftthumby" => new AxisInfo(Xbox360Axis.LeftThumbY),
            "rightthumbx" => new AxisInfo(Xbox360Axis.RightThumbX),
            "rightthumby" => new AxisInfo(Xbox360Axis.RightThumbY),

            // Triggers (sliders)
            "lefttrigger" => new AxisInfo(Xbox360Slider.LeftTrigger),
            "righttrigger" => new AxisInfo(Xbox360Slider.RightTrigger),

            // Invalid axis name
            _ => null
        };
    }

    /// <summary>
    /// Represents information about an Xbox 360 controller axis or slider
    /// </summary>
    private class AxisInfo
    {
        /// <summary>
        /// Gets the Xbox 360 axis (for thumbsticks)
        /// </summary>
        public Xbox360Axis Axis { get; }

        /// <summary>
        /// Gets the Xbox 360 slider (for triggers)
        /// </summary>
        public Xbox360Slider Slider { get; }

        /// <summary>
        /// Gets a value indicating whether this is a slider (trigger) or an axis (thumbstick)
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
