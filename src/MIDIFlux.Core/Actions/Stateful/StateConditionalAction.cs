using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// Action that executes different actions based on state value conditions.
/// Provides conditional logic based on state values rather than MIDI values.
/// </summary>
public class StateConditionalAction : ActionBase
{
    private const string StateKeyParam = "StateKey";
    private const string ComparisonTypeParam = "ComparisonType";
    private const string ComparisonValueParam = "ComparisonValue";
    private const string TrueActionParam = "TrueAction";
    private const string FalseActionParam = "FalseAction";



    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add StateKey parameter with string type
        Parameters[StateKeyParam] = new Parameter(
            ParameterType.String,
            "", // Default to empty string
            "State Key")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "required", true },
                { "pattern", @"^[a-zA-Z0-9]+$" } // Alphanumeric only for user-defined states
            }
        };

        // Add ComparisonType parameter with string type
        Parameters[ComparisonTypeParam] = new Parameter(
            ParameterType.String,
            "Equals", // Default to Equals
            "Comparison Type")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "Equals", "GreaterThan", "LessThan" } }
            }
        };

        // Add ComparisonValue parameter with integer type
        Parameters[ComparisonValueParam] = new Parameter(
            ParameterType.Integer,
            0, // Default to 0
            "Comparison Value")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "required", true }
            }
        };

        // Add TrueAction parameter with SubAction type
        Parameters[TrueActionParam] = new Parameter(
            ParameterType.SubAction,
            null, // Default to null (no action)
            "Action If True")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute when the condition is true" }
            }
        };

        // Add FalseAction parameter with SubAction type (optional)
        Parameters[FalseActionParam] = new Parameter(
            ParameterType.SubAction,
            null, // Default to null (no action)
            "Action If False")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute when the condition is false (optional)" }
            }
        };
    }

    /// <summary>
    /// Core execution logic for the state conditional action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var stateKey = GetParameterValue<string>(StateKeyParam);
        var comparisonType = GetParameterValue<string>(ComparisonTypeParam);
        var comparisonValue = GetParameterValue<int>(ComparisonValueParam);
        var trueAction = GetParameterValue<ActionBase?>(TrueActionParam);
        var falseAction = GetParameterValue<ActionBase?>(FalseActionParam);

        // Validate state key
        if (string.IsNullOrWhiteSpace(stateKey))
        {
            var errorMsg = "StateKey parameter cannot be empty";
            Logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        // Get ActionStateManager service - required for state operations
        var actionStateManager = GetService<ActionStateManager>();
        if (actionStateManager == null)
        {
            var errorMsg = "ActionStateManager service is not available";
            Logger.LogError(errorMsg);
            throw new InvalidOperationException(errorMsg);
        }

        // Get the current state value
        var currentStateValue = actionStateManager.GetState(stateKey);

        // Evaluate the condition
        bool conditionResult = comparisonType switch
        {
            "Equals" => currentStateValue == comparisonValue,
            "GreaterThan" => currentStateValue > comparisonValue,
            "LessThan" => currentStateValue < comparisonValue,
            _ => throw new InvalidOperationException($"Unknown comparison type: {comparisonType}")
        };

        Logger.LogTrace("StateConditionalAction: StateKey={StateKey}, CurrentValue={CurrentValue}, ComparisonType={ComparisonType}, ComparisonValue={ComparisonValue}, Result={Result}",
            stateKey, currentStateValue, comparisonType, comparisonValue, conditionResult);

        // Execute the appropriate action
        ActionBase? actionToExecute = conditionResult ? trueAction : falseAction;

        if (actionToExecute != null)
        {
            try
            {
                await actionToExecute.ExecuteAsync(midiValue);
                Logger.LogDebug("Successfully executed StateConditionalAction: StateKey={StateKey}, Condition={Condition}, ActionExecuted={ActionType}",
                    stateKey, $"{comparisonType} {comparisonValue}", conditionResult ? "TrueAction" : "FalseAction");
            }
            catch (Exception ex)
            {
                var actionType = conditionResult ? "TrueAction" : "FalseAction";
                var errorMsg = $"Failed to execute {actionType} in StateConditionalAction for state {stateKey}: {ex.Message}";
                Logger.LogError(ex, errorMsg);
                // Re-throw with context for caller to handle - UI error display handled by RunWithUiErrorHandling
                throw new InvalidOperationException(errorMsg, ex);
            }
        }
        else
        {
            var actionType = conditionResult ? "TrueAction" : "FalseAction";
            Logger.LogTrace("StateConditionalAction: No {ActionType} defined, skipping execution", actionType);
        }
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// StateConditionalAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public static InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
