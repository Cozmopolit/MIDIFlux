using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action that repeats sub-actions in a loop until a state condition signals stop.
/// Designed for use cases like "rapid-click while pedal is held down".
/// The loop checks the specified state key before each iteration and stops when
/// the state equals the StopValue (default: 0) or the state does not exist (-1).
/// A MaxRepetitions safety limit prevents runaway loops.
/// </summary>
[ActionDisplayName("Repeat While State")]
[ActionCategory(ActionCategory.FlowControl)]
public class RepeatAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string SubActionsParam = "SubActions";
    private const string IntervalMsParam = "IntervalMs";
    private const string StateKeyParam = "StateKey";
    private const string StopValueParam = "StopValue";
    private const string MaxRepetitionsParam = "MaxRepetitions";

    /// <summary>
    /// Initializes a new instance of RepeatAction with default parameters
    /// </summary>
    public RepeatAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Override JsonParameters setter to update description after JSON deserialization
    /// </summary>
    public new Dictionary<string, object?> JsonParameters
    {
        get => base.JsonParameters;
        set
        {
            base.JsonParameters = value;
            Description = GetDefaultDescription();
        }
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        Parameters[SubActionsParam] = new Parameter(
            ParameterType.SubActionList,
            new List<ActionBase>(),
            "Sub Actions")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Actions to execute on each repetition" }
            }
        };

        Parameters[IntervalMsParam] = new Parameter(
            ParameterType.Integer,
            50, // Default 50ms between repetitions
            "Interval (ms)")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 10 },
                { "max", 60000 },
                { "description", "Delay between repetitions in milliseconds" }
            }
        };

        Parameters[StateKeyParam] = new Parameter(
            ParameterType.String,
            "", // No default - user must specify
            "State Key")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "required", true },
                { "description", "State key to monitor. Loop continues while state != StopValue" }
            }
        };

        Parameters[StopValueParam] = new Parameter(
            ParameterType.Integer,
            0, // Default: stop when state is 0
            "Stop Value")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Loop stops when state equals this value (default: 0)" }
            }
        };

        Parameters[MaxRepetitionsParam] = new Parameter(
            ParameterType.Integer,
            1000, // Safety limit
            "Max Repetitions")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 1 },
                { "max", 100000 },
                { "description", "Safety limit for maximum loop iterations" }
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid();

        var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
        if (subActions.Count == 0)
        {
            AddValidationError("At least one sub-action must be specified for the repeat loop");
        }

        var stateKey = GetParameterValue<string>(StateKeyParam);
        if (string.IsNullOrWhiteSpace(stateKey))
        {
            AddValidationError("State Key must be specified");
        }

        var intervalMs = GetParameterValue<int?>(IntervalMsParam);
        if (!intervalMs.HasValue || intervalMs.Value < 10)
        {
            AddValidationError("Interval must be at least 10ms");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core async execution logic for the repeat action.
    /// Loops until the monitored state equals StopValue or does not exist (-1).
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the loop finishes</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
        var intervalMs = GetParameterValue<int>(IntervalMsParam);
        var stateKey = GetParameterValue<string>(StateKeyParam);
        var stopValue = GetParameterValue<int>(StopValueParam);
        var maxRepetitions = GetParameterValue<int>(MaxRepetitionsParam);

        // Get ActionStateManager service - required for state monitoring
        var actionStateManager = GetService<ActionStateManager>();
        if (actionStateManager == null)
        {
            var errorMsg = "ActionStateManager service is not available";
            Logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        Logger.LogDebug("RepeatAction starting: StateKey={StateKey}, StopValue={StopValue}, IntervalMs={IntervalMs}, MaxRepetitions={MaxRepetitions}",
            stateKey, stopValue, intervalMs, maxRepetitions);

        int iteration = 0;

        while (iteration < maxRepetitions)
        {
            // Check state BEFORE each iteration
            var currentState = actionStateManager.GetState(stateKey);

            // Stop if state equals StopValue OR state does not exist (-1)
            // The -1 check is critical for safety: profile switches call ClearAllStates(),
            // which removes the state key entirely. Without this check, the loop would
            // continue indefinitely after a profile switch.
            if (currentState == stopValue || currentState == -1)
            {
                Logger.LogDebug("RepeatAction stopping: StateKey={StateKey}, CurrentState={CurrentState}, StopValue={StopValue}, Iterations={Iterations}",
                    stateKey, currentState, stopValue, iteration);
                break;
            }

            iteration++;

            // Execute all sub-actions
            for (int i = 0; i < subActions.Count; i++)
            {
                try
                {
                    await subActions[i].ExecuteAsync(midiValue);
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error in RepeatAction sub-action {Step}/{Total} on iteration {Iteration}: {ErrorMessage}",
                        i + 1, subActions.Count, iteration, ex.Message);
                    // Continue with next sub-action - don't let one failure stop the loop
                }
            }

            // Wait for the specified interval before next iteration
            // This yields the thread back to the pool, allowing NoteOff events to be processed
            await Task.Delay(intervalMs);
        }

        if (iteration >= maxRepetitions)
        {
            Logger.LogWarning("RepeatAction hit MaxRepetitions limit ({MaxRepetitions}) for StateKey={StateKey}",
                maxRepetitions, stateKey);
        }

        Logger.LogDebug("RepeatAction completed: {Iterations} iterations for StateKey={StateKey}", iteration, stateKey);
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
            var intervalMs = GetParameterValue<int>(IntervalMsParam);
            var stateKey = GetParameterValue<string>(StateKeyParam);

            var actionDesc = subActions.Count > 0
                ? subActions[0].Description ?? "Action"
                : "Action";

            return $"Repeat {actionDesc} every {intervalMs}ms while {stateKey} is active";
        }
        catch
        {
            return "Repeat Action";
        }
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing RepeatAction: {Description}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// RepeatAction is only compatible with trigger signals (NoteOn starts the loop).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}