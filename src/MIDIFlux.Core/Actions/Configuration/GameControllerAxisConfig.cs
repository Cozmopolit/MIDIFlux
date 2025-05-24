namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for GameControllerAxis actions.
/// Represents setting a game controller axis value.
/// </summary>
public class GameControllerAxisConfig : UnifiedActionConfig
{
    /// <summary>
    /// The axis value (-1.0 to 1.0) - used when UseMidiValue is false
    /// </summary>
    public float AxisValue { get; set; } = 0.0f;

    /// <summary>
    /// The controller index (0-3)
    /// </summary>
    public int ControllerIndex { get; set; } = 0;

    /// <summary>
    /// The axis name (LeftX, LeftY, RightX, RightY, LeftTrigger, RightTrigger)
    /// </summary>
    public string AxisName { get; set; } = "";

    /// <summary>
    /// Whether to use the incoming MIDI value instead of the fixed AxisValue
    /// </summary>
    public bool UseMidiValue { get; set; } = false;

    /// <summary>
    /// Minimum MIDI value for range mapping (default: 0)
    /// </summary>
    public int MinValue { get; set; } = 0;

    /// <summary>
    /// Maximum MIDI value for range mapping (default: 127)
    /// </summary>
    public int MaxValue { get; set; } = 127;

    /// <summary>
    /// Whether to invert the axis value
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Initializes a new instance of GameControllerAxisConfig
    /// </summary>
    public GameControllerAxisConfig()
    {
        Type = UnifiedActionType.GameControllerAxis;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(AxisName) &&
               ControllerIndex >= 0 &&
               ControllerIndex <= 3 &&
               AxisValue >= -1.0f &&
               AxisValue <= 1.0f &&
               MinValue >= 0 &&
               MaxValue <= 127 &&
               MinValue <= MaxValue;
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(AxisName))
        {
            errors.Add("AxisName cannot be empty or whitespace");
        }

        if (ControllerIndex < 0 || ControllerIndex > 3)
        {
            errors.Add("ControllerIndex must be between 0 and 3");
        }

        if (AxisValue < -1.0f || AxisValue > 1.0f)
        {
            errors.Add("AxisValue must be between -1.0 and 1.0");
        }

        if (MinValue < 0)
        {
            errors.Add("MinValue must be >= 0");
        }

        if (MaxValue > 127)
        {
            errors.Add("MaxValue must be <= 127");
        }

        if (MinValue > MaxValue)
        {
            errors.Add("MinValue must be <= MaxValue");
        }

        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;

        var valueDescription = UseMidiValue ? "MIDI Value" : AxisValue.ToString("F2");
        var invertDescription = Invert ? " (inverted)" : "";
        return $"Controller {ControllerIndex + 1} {AxisName} = {valueDescription}{invertDescription}";
    }
}
