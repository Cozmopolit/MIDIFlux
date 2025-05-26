using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Stateful;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// action that alternates between two actions on repeated triggers.
/// Convenience wrapper around StateConditionalAction for common toggle use cases.
/// </summary>
public class AlternatingAction : ActionBase<AlternatingActionConfig>
{
    private readonly StateConditionalAction _primaryConditional;
    private readonly StateConditionalAction _secondaryConditional;
    private readonly ActionStateManager _actionStateManager;
    private readonly string _stateKey;

    /// <summary>
    /// Initializes a new instance of AlternatingAction
    /// </summary>
    /// <param name="config">The alternating action configuration</param>
    /// <param name="actionStateManager">The state manager for tracking alternation state</param>
    /// <param name="actionFactory">The factory for creating sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    public AlternatingAction(AlternatingActionConfig config, ActionStateManager actionStateManager, ActionFactory actionFactory) : base(config)
    {
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager));
        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory));

        // Auto-generate state key if not provided (alphanumeric only)
        _stateKey = string.IsNullOrEmpty(config.StateKey) ?
            $"Alt{Guid.NewGuid().ToString("N")[..8]}" : config.StateKey;

        // Initialize the state to the starting value (0 for primary, 1 for secondary)
        var initialStateValue = config.StartWithPrimary ? 0 : 1;
        _actionStateManager.SetState(_stateKey, initialStateValue);

        // Create primary conditional (executes when state is 0, sets to 1)
        _primaryConditional = new StateConditionalAction(new StateConditionalConfig
        {
            StateKey = _stateKey,
            Condition = new StateConditionalEntry
            {
                StateValue = 0,
                Comparison = StateComparison.Equals,
                Action = config.PrimaryAction,
                SetStateAfter = 1
            }
        }, _actionStateManager, actionFactory);

        // Create secondary conditional (executes when state is 1, sets to 0)
        _secondaryConditional = new StateConditionalAction(new StateConditionalConfig
        {
            StateKey = _stateKey,
            Condition = new StateConditionalEntry
            {
                StateValue = 1,
                Comparison = StateComparison.Equals,
                Action = config.SecondaryAction,
                SetStateAfter = 0
            }
        }, _actionStateManager, actionFactory);
    }

    /// <summary>
    /// Core execution logic for the alternating action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Check current state and execute only the appropriate conditional
        // This prevents race conditions where both conditionals could execute
        var currentState = _actionStateManager.GetState(_stateKey);

        if (currentState == 0)
        {
            // Execute primary conditional (state 0 -> execute primary, set to 1)
            await _primaryConditional.ExecuteAsync(midiValue);
        }
        else if (currentState == 1)
        {
            // Execute secondary conditional (state 1 -> execute secondary, set to 0)
            await _secondaryConditional.ExecuteAsync(midiValue);
        }
        else
        {
            Logger.LogWarning("AlternatingAction state {StateKey} has unexpected value {CurrentState}, no action taken",
                _stateKey, currentState);
        }

        Logger.LogTrace("Successfully executed AlternatingAction: {Description}", Description);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var primaryDesc = Config.PrimaryAction.Description ?? "Primary";
        var secondaryDesc = Config.SecondaryAction.Description ?? "Secondary";
        return $"Alternate: {primaryDesc} ↔ {secondaryDesc}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing AlternatingAction: {Description}";
    }
}
