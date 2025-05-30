using MIDIFlux.Core.Actions.Parameters;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for waiting/delaying for a specified time.
/// Consolidates DelayConfig into the action class using the parameter system.
/// Implements true async behavior with Task.Delay.
/// </summary>
[ActionDisplayName("Delay")]
public class DelayAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string MillisecondsParam = "Milliseconds";

    /// <summary>
    /// Gets the delay duration in milliseconds
    /// </summary>
    [JsonIgnore]
    public int Milliseconds => GetParameterValue<int>(MillisecondsParam);

    /// <summary>
    /// Initializes a new instance of DelayAction with default parameters
    /// </summary>
    public DelayAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of DelayAction with specified delay
    /// </summary>
    /// <param name="milliseconds">The delay duration in milliseconds</param>
    public DelayAction(int milliseconds) : base()
    {
        SetParameterValue(MillisecondsParam, milliseconds);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Milliseconds parameter with integer type
        Parameters[MillisecondsParam] = new Parameter(
            ParameterType.Integer,
            1000, // Default to 1 second delay
            "Delay Duration (ms)")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 60000 } // Max 60 seconds
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var milliseconds = GetParameterValue<int>(MillisecondsParam);
        if (milliseconds < 0)
        {
            AddValidationError("Milliseconds must be greater than or equal to 0");
        }
        else if (milliseconds > 60000)
        {
            AddValidationError("Milliseconds must not exceed 60000 (60 seconds)");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core async execution logic for the delay action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes after the specified delay</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var milliseconds = Milliseconds;

        if (milliseconds <= 0)
        {
            Logger.LogDebug("DelayAction: Skipping delay because milliseconds is {Milliseconds}", milliseconds);
            return;
        }

        Logger.LogTrace("DelayAction: Starting delay for {Milliseconds}ms", milliseconds);
        await Task.Delay(milliseconds);
        Logger.LogTrace("DelayAction: Completed delay for {Milliseconds}ms", milliseconds);
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Wait for {Milliseconds} ms";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing DelayAction for {Milliseconds}ms";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// DelayAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public static InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
