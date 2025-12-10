using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for setting a game controller axis value.
/// Implements sync-by-default execution for performance.
/// Uses existing ViGEm integration in GameController directory.
/// </summary>
public class GameControllerAxisAction : ActionBase
{
    private const string AxisNameParam = "Axis";
    private const string AxisValueParam = "AxisValue";
    private const string ControllerIndexParam = "ControllerIndex";
    private const string UseMidiValueParam = "UseMidiValue";
    private const string MinValueParam = "MinValue";
    private const string MaxValueParam = "MaxValue";
    private const string InvertParam = "Invert";

    /// <summary>
    /// Initializes a new instance of GameControllerAxisAction with default parameters
    /// </summary>
    public GameControllerAxisAction() : base()
    {
        // No hardware initialization during construction - only during execution
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add AxisName parameter with string type for Xbox controller axes
        Parameters[AxisNameParam] = new Parameter(
            ParameterType.String,
            "LeftThumbX", // Default to left thumbstick X
            "Axis Name")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "LeftThumbX", "LeftThumbY", "RightThumbX", "RightThumbY", "LeftTrigger", "RightTrigger" } }
            }
        };

        // Add AxisValue parameter - integer range matching ViGEm expectations
        Parameters[AxisValueParam] = new Parameter(
            ParameterType.Integer,
            0, // Default to center position
            "Axis Value")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", -32768 },
                { "max", 32767 },
                { "description", "Axis value: -32768 (full negative) to 32767 (full positive), 0 = center" }
            }
        };

        // Add ControllerIndex parameter with integer type
        Parameters[ControllerIndexParam] = new Parameter(
            ParameterType.Integer,
            0, // Default to controller 0
            "Controller Index")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 3 }
            }
        };

        // Add UseMidiValue parameter with boolean type
        Parameters[UseMidiValueParam] = new Parameter(
            ParameterType.Boolean,
            true, // Default to using MIDI value
            "Use MIDI Value");

        // Add MinValue parameter with integer type
        Parameters[MinValueParam] = new Parameter(
            ParameterType.Integer,
            0, // Default minimum
            "Min Value")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 127 }
            }
        };

        // Add MaxValue parameter with integer type
        Parameters[MaxValueParam] = new Parameter(
            ParameterType.Integer,
            127, // Default maximum
            "Max Value")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 127 }
            }
        };

        // Add Invert parameter with boolean type
        Parameters[InvertParam] = new Parameter(
            ParameterType.Boolean,
            false, // Default to not inverted
            "Invert");
    }

    /// <summary>
    /// Validates this action's parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        var isValid = base.IsValid();

        // Validate axis name
        var axisName = GetParameterValue<string>(AxisNameParam);
        if (string.IsNullOrEmpty(axisName))
        {
            AddValidationError("Axis name cannot be empty");
            isValid = false;
        }
        else if (MapAxisNameToInfo(axisName) == null)
        {
            AddValidationError($"Invalid axis name: {axisName}");
            isValid = false;
        }

        // Validate controller index
        var controllerIndex = GetParameterValue<int>(ControllerIndexParam);
        if (!ActionHelper.IsIntegerInRange(controllerIndex, 0, 3))
        {
            AddValidationError($"Controller index must be between 0 and 3, got: {controllerIndex}");
            isValid = false;
        }

        // Validate axis value range (-32768 to 32767, ViGEm range)
        var axisValue = GetParameterValue<int>(AxisValueParam);
        if (axisValue < -32768 || axisValue > 32767)
        {
            AddValidationError($"Axis value must be between -32768 and 32767, got: {axisValue}");
            isValid = false;
        }

        // Validate min/max value range
        var minValue = GetParameterValue<int>(MinValueParam);
        var maxValue = GetParameterValue<int>(MaxValueParam);
        if (minValue < 0 || minValue > 127)
        {
            AddValidationError($"Min value must be between 0 and 127, got: {minValue}");
            isValid = false;
        }
        if (maxValue < 0 || maxValue > 127)
        {
            AddValidationError($"Max value must be between 0 and 127, got: {maxValue}");
            isValid = false;
        }
        if (minValue >= maxValue)
        {
            AddValidationError($"Min value ({minValue}) must be less than max value ({maxValue})");
            isValid = false;
        }

        return isValid;
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

        // Simple integer conversion: midiValue * 2 (maps 0-127 to 0-254)
        int triggerValue = clampedValue * 2;

        // Apply inversion if requested
        if (invert)
        {
            triggerValue = 255 - triggerValue;
        }

        // Clamp to byte range and return
        return (byte)Math.Clamp(triggerValue, 0, 255);
    }

    /// <summary>
    /// Converts a MIDI value to an axis value (-32768 to 32767)
    /// </summary>
    private static short ConvertToAxisValue(int midiValue, int minValue, int maxValue, bool invert)
    {
        // Clamp the MIDI value to the specified range
        int clampedValue = Math.Clamp(midiValue, minValue, maxValue);

        // Simple integer conversion: (midiValue - 64) * 512 (maps 0-127 to -32768 to 32256)
        int axisValue = (clampedValue - 64) * 512;

        // Apply inversion if requested
        if (invert)
        {
            axisValue = -axisValue;
        }

        // Clamp to short range and return
        return (short)Math.Clamp(axisValue, -32768, 32767);
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var axisName = GetParameterValue<string>(AxisNameParam) ?? "";
        return $"Set Game Controller Axis ({axisName})";
    }

    /// <summary>
    /// Core execution logic for the game controller axis action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue = null)
    {
        var axisName = GetParameterValue<string>(AxisNameParam);
        var axisValue = GetParameterValue<int>(AxisValueParam);

        // Clamp to valid ViGEm range for axes
        axisValue = Math.Clamp(axisValue, -32768, 32767);
        var controllerIndex = GetParameterValue<int>(ControllerIndexParam);
        var useMidiValue = GetParameterValue<bool>(UseMidiValueParam);
        var minValue = GetParameterValue<int>(MinValueParam);
        var maxValue = GetParameterValue<int>(MaxValueParam);
        var invert = GetParameterValue<bool>(InvertParam);

        // Get GameControllerManager at execution time (not during construction)
        var controllerManager = GameControllerManager.GetInstance(Logger);

        // Check if ViGEm is available
        if (!controllerManager.IsViGEmAvailable)
        {
            var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Get the controller instance
        var controller = controllerManager.GetController(controllerIndex);
        if (controller == null)
        {
            var errorMsg = $"Failed to get controller instance for index {controllerIndex}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Map axis name to axis info
        var axisInfo = MapAxisNameToInfo(axisName);
        if (axisInfo == null)
        {
            var errorMsg = $"Invalid axis name: {axisName}. Axis will not work.";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Apply the value to ViGEm
        if (axisInfo.IsSlider)
        {
            // Handle trigger (0-255 range)
            byte triggerValue;
            if (useMidiValue && midiValue.HasValue)
            {
                // Convert MIDI (0-127) to trigger range (0-255)
                triggerValue = ConvertToTriggerValue(midiValue.Value, minValue, maxValue, invert);
            }
            else
            {
                // Use direct axis value (clamp to trigger range)
                int value = invert ? (255 - Math.Clamp(axisValue, 0, 255)) : Math.Clamp(axisValue, 0, 255);
                triggerValue = (byte)value;
            }

            controller.SetSliderValue(axisInfo.Slider, triggerValue);
            Logger.LogDebug("Set game controller trigger {AxisName} to {Value}", axisName, triggerValue);
        }
        else
        {
            // Handle axis (-32768 to 32767 range)
            short axisValueConverted;
            if (useMidiValue && midiValue.HasValue)
            {
                // Convert MIDI (0-127) to axis range (-32768 to 32767)
                axisValueConverted = ConvertToAxisValue(midiValue.Value, minValue, maxValue, invert);
            }
            else
            {
                // Use direct axis value (already in correct range)
                int value = invert ? -axisValue : axisValue;
                axisValueConverted = (short)Math.Clamp(value, -32768, 32767);
            }

            controller.SetAxisValue(axisInfo.Axis, axisValueConverted);
            Logger.LogDebug("Set game controller axis {AxisName} to {Value}", axisName, axisValueConverted);
        }

        Logger.LogTrace("Successfully executed GameControllerAxisAction for Axis={AxisName}, ControllerIndex={ControllerIndex}, AxisValue={AxisValue}",
            axisName, controllerIndex, axisValue);

        return ValueTask.CompletedTask;
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

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// GameControllerAxisAction supports both trigger signals (for fixed values) and absolute value signals (for MIDI pass-through).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue };
    }
}
