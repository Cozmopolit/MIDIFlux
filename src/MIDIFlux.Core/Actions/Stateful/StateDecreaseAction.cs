using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// Action that decreases a state value by a specified amount.
/// Provides building blocks for complex stateful behaviors.
/// </summary>
public class StateDecreaseAction : ActionBase
{
    private const string StateKeyParam = "StateKey";
    private const string ValueParam = "Value";

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

        // Add Value parameter with integer type
        Parameters[ValueParam] = new Parameter(
            ParameterType.Integer,
            1, // Default to 1
            "Decrease Amount")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "required", true },
                { "min", 1 }
            }
        };
    }

    /// <summary>
    /// Core execution logic for the state decrease action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var stateKey = GetParameterValue<string>(StateKeyParam);
        var decreaseAmount = GetParameterValue<int>(ValueParam);

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

        // Decrease the state value
        try
        {
            var currentValue = actionStateManager.GetState(stateKey);
            var newValue = (currentValue == -1 ? 0 : currentValue) - decreaseAmount;
            actionStateManager.SetState(stateKey, newValue);

            Logger.LogTrace("Decreased state {StateKey} by {DecreaseAmount} (from {OldValue} to {NewValue})",
                stateKey, decreaseAmount, currentValue, newValue);
            Logger.LogDebug("Successfully executed StateDecreaseAction: StateKey={StateKey}, DecreaseAmount={DecreaseAmount}",
                stateKey, decreaseAmount);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Failed to decrease state {stateKey} by {decreaseAmount}: {ex.Message}";
            Logger.LogError(ex, errorMsg);
            throw new InvalidOperationException(errorMsg, ex);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// StateDecreaseAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public static InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
