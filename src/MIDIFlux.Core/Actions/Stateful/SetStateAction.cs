using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Stateful;

/// <summary>
/// Unified action that sets a state value.
/// Simple action for state management without complex logic.
/// </summary>
public class SetStateAction : IUnifiedAction
{
    private readonly string _stateKey;
    private readonly int _stateValue;
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
    /// Initializes a new instance of SetStateAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">The action state manager for state operations</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public SetStateAction(SetStateConfig config, ActionStateManager actionStateManager)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "SetStateConfig cannot be null");
        if (actionStateManager == null)
            throw new ArgumentNullException(nameof(actionStateManager), "ActionStateManager cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid SetStateConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        _stateKey = config.StateKey;
        _stateValue = config.StateValue;
        _actionStateManager = actionStateManager;

        Description = config.Description ?? $"Set {_stateKey} = {_stateValue}";

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<SetStateAction>();
    }

    /// <summary>
    /// Executes the set state action synchronously.
    /// Sets the specified state to the configured value.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing SetStateAction: StateKey={StateKey}, StateValue={StateValue}, MidiValue={MidiValue}",
                _stateKey, _stateValue, midiValue);

            // Set the state value
            _actionStateManager.SetState(_stateKey, _stateValue);

            _logger.LogTrace("Successfully set state {StateKey} to {StateValue}", _stateKey, _stateValue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SetStateAction for StateKey={StateKey}, StateValue={StateValue}",
                _stateKey, _stateValue);
            ApplicationErrorHandler.ShowError(
                $"Error setting state: {ex.Message}",
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
}
