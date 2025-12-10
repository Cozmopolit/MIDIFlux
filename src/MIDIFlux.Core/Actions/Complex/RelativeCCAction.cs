using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action that handles relative MIDI controllers (scratch wheels, endless encoders) using the parameter system.
/// Decodes relative CC values and executes increase/decrease actions multiple times based on magnitude.
/// </summary>
[ActionDisplayName("Relative CC (Encoder)")]
[ActionCategory(ActionCategory.FlowControl)]
public class RelativeCCAction : ActionBase
{
    // Parameter names
    private const string IncreaseActionParam = "IncreaseAction";
    private const string DecreaseActionParam = "DecreaseAction";

    /// <summary>
    /// Gets the increase action (internal convenience method)
    /// </summary>
    private ActionBase GetIncreaseAction() => GetParameterValue<ActionBase>(IncreaseActionParam);

    /// <summary>
    /// Gets the decrease action (internal convenience method)
    /// </summary>
    private ActionBase GetDecreaseAction() => GetParameterValue<ActionBase>(DecreaseActionParam);

    /// <summary>
    /// Initializes a new instance of RelativeCCAction with default parameters
    /// </summary>
    public RelativeCCAction()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add IncreaseAction parameter (for positive relative values)
        Parameters[IncreaseActionParam] = new Parameter(
            ParameterType.SubAction,
            null, // No default - user must specify action
            "Increase Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute for positive relative controller values (clockwise/up)" }
            }
        };

        // Add DecreaseAction parameter (for negative relative values)
        Parameters[DecreaseActionParam] = new Parameter(
            ParameterType.SubAction,
            null, // No default - user must specify action
            "Decrease Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute for negative relative controller values (counter-clockwise/down)" }
            }
        };
    }

    /// <summary>
    /// Core execution logic for the relative CC action.
    /// Decodes the MIDI value and executes the appropriate action multiple times.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) from the relative controller</param>
    /// <returns>A ValueTask that completes when all actions are finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        Logger.LogDebug("RelativeCCAction.ExecuteAsyncCore called with MIDI value: {MidiValue}", midiValue);

        if (!midiValue.HasValue)
        {
            Logger.LogWarning("RelativeCCAction received null MIDI value, ignoring");
            return;
        }

        // Decode the relative value using SignMagnitude encoding
        var (direction, magnitude) = DecodeSignMagnitude(midiValue.Value);

        Logger.LogDebug("RelativeCCAction decoded MIDI value {MidiValue} -> direction: {Direction}, magnitude: {Magnitude}",
            midiValue.Value, direction, magnitude);

        if (magnitude == 0)
        {
            Logger.LogDebug("RelativeCCAction received zero magnitude, ignoring");
            return;
        }

        // Select the appropriate action
        var actionToExecute = direction > 0 ? GetIncreaseAction() : GetDecreaseAction();
        var directionText = direction > 0 ? "increase" : "decrease";

        Logger.LogDebug("RelativeCCAction executing {Direction} action {Magnitude} times (MIDI value: {MidiValue}), Action: {ActionType}",
            directionText, magnitude, midiValue.Value, actionToExecute?.GetType().Name ?? "null");

        if (actionToExecute == null)
        {
            Logger.LogError("RelativeCCAction: {Direction} action is null!", directionText);
            return;
        }

        // Execute the action multiple times based on magnitude
        var exceptions = new List<Exception>();
        for (int i = 0; i < magnitude; i++)
        {
            try
            {
                Logger.LogDebug("RelativeCCAction executing {Direction} action iteration {Iteration}/{Magnitude}",
                    directionText, i + 1, magnitude);
                await actionToExecute.ExecuteAsync(midiValue);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing {Direction} action (iteration {Iteration}/{Magnitude}): {ErrorMessage}",
                    directionText, i + 1, magnitude, ex.Message);

                // Collect exceptions but continue with remaining iterations
                exceptions.Add(new InvalidOperationException($"Error executing {directionText} action (iteration {i + 1}/{magnitude}): {ex.Message}", ex));
            }
        }

        // If any exceptions occurred, throw them for caller to handle
        // UI error display is now handled by callers using RunWithUiErrorHandling
        if (exceptions.Count > 0)
        {
            if (exceptions.Count == 1)
            {
                throw exceptions[0];
            }
            else
            {
                throw new AggregateException($"Multiple errors occurred in RelativeCCAction execution", exceptions);
            }
        }

        Logger.LogDebug("RelativeCCAction completed {Direction} execution with {Magnitude} iterations",
            directionText, magnitude);
    }



    /// <summary>
    /// Decodes relative controller encoding:
    /// - Values 1-63: Negative direction (counter-clockwise/down), magnitude = 64 - value
    /// - Value 64: No change
    /// - Values 65-127: Positive direction (clockwise/up), magnitude = value - 64
    /// </summary>
    private static (int direction, int magnitude) DecodeSignMagnitude(int midiValue)
    {
        if (midiValue == 64)
            return (0, 0); // No change

        if (midiValue >= 1 && midiValue <= 63)
            return (-1, 64 - midiValue); // Negative direction: 63->1, 62->2, etc.

        if (midiValue >= 65 && midiValue <= 127)
            return (1, midiValue - 64); // Positive direction: 65->1, 66->2, etc.

        // Value 0 - treat as no change
        return (0, 0);
    }



    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var increaseAction = GetParameterValue<ActionBase>(IncreaseActionParam);
            var decreaseAction = GetParameterValue<ActionBase>(DecreaseActionParam);

            var increaseDesc = increaseAction?.Description ?? "Unknown";
            var decreaseDesc = decreaseAction?.Description ?? "Unknown";
            return $"Relative CC: +({increaseDesc}) / -({decreaseDesc})";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Relative CC Action";
        }
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return "Error executing RelativeCCAction";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// RelativeCCAction is ONLY compatible with relative value signals (endless encoders, scratch wheels).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.RelativeValue };
    }
}
