using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// action that executes different actions based on state values.
/// Implements single condition model for simplicity and predictability.
/// </summary>
public class StateConditionalAction : ActionBase<StateConditionalConfig>
{
    private readonly string _stateKey;
    private readonly StateConditionalEntry _condition;
    private readonly IAction _subAction;
    private readonly ActionStateManager _actionStateManager;

    /// <summary>
    /// Initializes a new instance of StateConditionalAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">The action state manager for state operations</param>
    /// <param name="actionFactory">The action factory for creating sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public StateConditionalAction(
        StateConditionalConfig config,
        ActionStateManager actionStateManager,
        IActionFactory actionFactory) : base(config)
    {
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager), "ActionStateManager cannot be null");
        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory), "IActionFactory cannot be null");

        _stateKey = config.StateKey;
        _condition = config.Condition;

        // Create the sub-action
        _subAction = actionFactory.CreateAction(_condition.Action);
    }

    /// <summary>
    /// Core execution logic for the state conditional action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
            // Get current state value
            var currentState = _actionStateManager.GetState(_stateKey);
            Logger.LogTrace("Current state for {StateKey}: {CurrentState}", _stateKey, currentState);

            // Check if condition matches
            var matches = DoesConditionMatch(currentState, _condition);
            Logger.LogTrace("Condition check for {StateKey}: {CurrentState} {Comparison} {TargetValue} = {Matches}",
                _stateKey, currentState, _condition.Comparison, _condition.StateValue, matches);

            if (matches)
            {
                Logger.LogDebug("Condition matched, executing sub-action: {SubActionDescription}", _subAction.Description);

                // Execute the sub-action
                try
                {
                    await _subAction.ExecuteAsync(midiValue);
                    Logger.LogTrace("Sub-action executed successfully");
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error executing sub-action in StateConditionalAction");
                    ApplicationErrorHandler.ShowError(
                        $"Error executing conditional action: {ex.Message}",
                        "MIDIFlux - Error",
                        Logger,
                        ex);
                    // Continue to state update even if sub-action failed (established pattern)
                }

                // Update state if specified
                if (_condition.SetStateAfter != -1)
                {
                    _actionStateManager.SetState(_stateKey, _condition.SetStateAfter);
                    Logger.LogTrace("Updated state {StateKey} to {NewState}", _stateKey, _condition.SetStateAfter);
                }
            }
            else
            {
                Logger.LogDebug("Condition did not match, no action taken");
            }
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var comparisonText = _condition.Comparison switch
        {
            StateComparison.Equals => "==",
            StateComparison.GreaterThan => ">",
            StateComparison.LessThan => "<",
            _ => "?"
        };

        var setStateText = _condition.SetStateAfter != -1 ? $", then set to {_condition.SetStateAfter}" : "";
        return $"If {_stateKey} {comparisonText} {_condition.StateValue}: {_subAction.Description}{setStateText}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing StateConditionalAction for StateKey={_stateKey}";
    }

    /// <summary>
    /// Checks if the current state matches the condition
    /// </summary>
    /// <param name="currentState">The current state value</param>
    /// <param name="condition">The condition to check</param>
    /// <returns>True if condition matches, false otherwise</returns>
    private static bool DoesConditionMatch(int currentState, StateConditionalEntry condition)
    {
        return condition.Comparison switch
        {
            StateComparison.Equals => currentState == condition.StateValue,
            StateComparison.GreaterThan => currentState > condition.StateValue,
            StateComparison.LessThan => currentState < condition.StateValue,
            _ => false
        };
    }
}
