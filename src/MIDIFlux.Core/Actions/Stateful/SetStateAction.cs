using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// action that sets a state value.
/// Simple action for state management without complex logic.
/// </summary>
public class SetStateAction : ActionBase<SetStateConfig>
{
    private readonly string _stateKey;
    private readonly int _stateValue;
    private readonly ActionStateManager _actionStateManager;

    /// <summary>
    /// Initializes a new instance of SetStateAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">The action state manager for state operations</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public SetStateAction(SetStateConfig config, ActionStateManager actionStateManager) : base(config)
    {
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager), "ActionStateManager cannot be null");

        _stateKey = config.StateKey;
        _stateValue = config.StateValue;
    }

    /// <summary>
    /// Core execution logic for the set state action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Set the state value
        _actionStateManager.SetState(_stateKey, _stateValue);

        Logger.LogTrace("Successfully set state {StateKey} to {StateValue}", _stateKey, _stateValue);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Set {_stateKey} = {_stateValue}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing SetStateAction for StateKey={_stateKey}, StateValue={_stateValue}";
    }
}
