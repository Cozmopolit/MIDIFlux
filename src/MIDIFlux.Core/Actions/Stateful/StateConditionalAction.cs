using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// Unified action that executes different actions based on state values.
/// Implements single condition model for simplicity and predictability.
/// </summary>
public class StateConditionalAction : IUnifiedAction
{
    private readonly string _stateKey;
    private readonly StateConditionalEntry _condition;
    private readonly IUnifiedAction _subAction;
    private readonly ActionStateManager _actionStateManager;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

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
        IUnifiedActionFactory actionFactory)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "StateConditionalConfig cannot be null");
        if (actionStateManager == null)
            throw new ArgumentNullException(nameof(actionStateManager), "ActionStateManager cannot be null");
        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory), "IUnifiedActionFactory cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid StateConditionalConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        _stateKey = config.StateKey;
        _condition = config.Condition;
        _actionStateManager = actionStateManager;

        // Create the sub-action
        _subAction = actionFactory.CreateAction(_condition.Action);

        // Build description
        var comparisonText = _condition.Comparison switch
        {
            StateComparison.Equals => "==",
            StateComparison.GreaterThan => ">",
            StateComparison.LessThan => "<",
            _ => "?"
        };

        var setStateText = _condition.SetStateAfter != -1 ? $", then set to {_condition.SetStateAfter}" : "";
        Description = config.Description ?? 
                     $"If {_stateKey} {comparisonText} {_condition.StateValue}: {_subAction.Description}{setStateText}";

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<StateConditionalAction>();
    }

    /// <summary>
    /// Executes the conditional action synchronously.
    /// Checks state condition and executes sub-action if condition matches.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing StateConditionalAction: StateKey={StateKey}, MidiValue={MidiValue}",
                _stateKey, midiValue);

            // Get current state value
            var currentState = _actionStateManager.GetState(_stateKey);
            _logger.LogTrace("Current state for {StateKey}: {CurrentState}", _stateKey, currentState);

            // Check if condition matches
            var matches = DoesConditionMatch(currentState, _condition);
            _logger.LogTrace("Condition check for {StateKey}: {CurrentState} {Comparison} {TargetValue} = {Matches}",
                _stateKey, currentState, _condition.Comparison, _condition.StateValue, matches);

            if (matches)
            {
                _logger.LogDebug("Condition matched, executing sub-action: {SubActionDescription}", _subAction.Description);

                // Execute the sub-action
                try
                {
                    _subAction.Execute(midiValue);
                    _logger.LogTrace("Sub-action executed successfully");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing sub-action in StateConditionalAction");
                    ApplicationErrorHandler.ShowError(
                        $"Error executing conditional action: {ex.Message}",
                        "MIDIFlux - Error",
                        _logger,
                        ex);
                    // Continue to state update even if sub-action failed (established pattern)
                }

                // Update state if specified
                if (_condition.SetStateAfter != -1)
                {
                    _actionStateManager.SetState(_stateKey, _condition.SetStateAfter);
                    _logger.LogTrace("Updated state {StateKey} to {NewState}", _stateKey, _condition.SetStateAfter);
                }
            }
            else
            {
                _logger.LogDebug("Condition did not match, no action taken");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing StateConditionalAction for StateKey={StateKey}", _stateKey);
            ApplicationErrorHandler.ShowError(
                $"Error in conditional action: {ex.Message}",
                "MIDIFlux - Error",
                _logger,
                ex);
        }
    }

    /// <summary>
    /// Async wrapper for Execute method to satisfy IUnifiedAction interface
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>Completed ValueTask</returns>
    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
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
