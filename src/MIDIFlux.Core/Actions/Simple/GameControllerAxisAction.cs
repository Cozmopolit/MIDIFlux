using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Unified action for setting a game controller axis value.
/// Implements sync-by-default execution for performance.
/// Uses existing ViGEm integration in GameController directory.
/// </summary>
public class GameControllerAxisAction : IUnifiedAction
{
    private readonly string _axisName;
    private readonly float _axisValue;
    private readonly int _controllerIndex;
    private readonly bool _useMidiValue;
    private readonly int _minValue;
    private readonly int _maxValue;
    private readonly bool _invert;
    private readonly GameControllerManager _controllerManager;
    private readonly ILogger _logger;
    private readonly AxisInfo? _axisInfo;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

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
    public GameControllerAxisAction(GameControllerAxisConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "GameControllerAxisConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid GameControllerAxisConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Controller {config.ControllerIndex + 1} {config.AxisName} = {(config.UseMidiValue ? "MIDI Value" : config.AxisValue.ToString("F2"))}";
        _axisName = config.AxisName;
        _axisValue = config.AxisValue;
        _controllerIndex = config.ControllerIndex;
        _useMidiValue = config.UseMidiValue;
        _minValue = config.MinValue;
        _maxValue = config.MaxValue;
        _invert = config.Invert;

        // Initialize logger and game controller manager
        _logger = LoggingHelper.CreateLogger<GameControllerAxisAction>();
        _controllerManager = GameControllerManager.GetInstance(_logger);
        _axisInfo = MapAxisNameToInfo(config.AxisName);

        if (_axisInfo == null)
        {
            _logger.LogWarning("Invalid axis name: {AxisName}. Axis will not work.", config.AxisName);
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
    /// Executes the game controller axis action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing GameControllerAxisAction: Axis={AxisName}, ControllerIndex={ControllerIndex}, UseMidiValue={UseMidiValue}, MidiValue={MidiValue}",
                _axisName, _controllerIndex, _useMidiValue, midiValue);

            // Check if ViGEm is available
            if (!_controllerManager.IsViGEmAvailable)
            {
                var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
                _logger.LogWarning(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", _logger);
                return;
            }

            // Get the controller instance
            var controller = _controllerManager.GetController(_controllerIndex);
            if (controller == null)
            {
                var errorMsg = $"Failed to get controller instance for index {_controllerIndex}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", _logger);
                return;
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
                _logger.LogWarning(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", _logger);
                return;
            }

            // Apply the axis value directly
            if (_axisInfo.IsSlider)
            {
                // Handle trigger
                byte triggerValue = ConvertToTriggerValue(valueToUse, _minValue, _maxValue, _invert);
                controller.SetSliderValue(_axisInfo.Slider, triggerValue);
                _logger.LogDebug("Set game controller trigger {AxisName} to {Value}", _axisName, triggerValue);
            }
            else
            {
                // Handle axis
                short axisValue = ConvertToAxisValue(valueToUse, _minValue, _maxValue, _invert);
                controller.SetAxisValue(_axisInfo.Axis, axisValue);
                _logger.LogDebug("Set game controller axis {AxisName} to {Value}", _axisName, axisValue);
            }

            _logger.LogTrace("Successfully executed GameControllerAxisAction for Axis={AxisName}, ControllerIndex={ControllerIndex}, Value={Value}",
                _axisName, _controllerIndex, valueToUse);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing GameControllerAxisAction for axis {_axisName} on controller {_controllerIndex}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Async adapter for the synchronous Execute method.
    /// Uses ValueTask for zero allocation when the operation is synchronous.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A completed ValueTask</returns>
    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
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
