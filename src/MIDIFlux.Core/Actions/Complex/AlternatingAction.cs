using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Stateful;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Unified action that alternates between two actions on repeated triggers.
/// Convenience wrapper around StateConditionalAction for common toggle use cases.
/// </summary>
public class AlternatingAction : IUnifiedAction
{
    private readonly StateConditionalAction _primaryConditional;
    private readonly StateConditionalAction _secondaryConditional;
    private readonly ActionStateManager _actionStateManager;
    private readonly string _stateKey;
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
    /// Initializes a new instance of AlternatingAction
    /// </summary>
    /// <param name="config">The alternating action configuration</param>
    /// <param name="actionStateManager">The state manager for tracking alternation state</param>
    /// <param name="actionFactory">The factory for creating sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    public AlternatingAction(AlternatingActionConfig config, ActionStateManager actionStateManager, UnifiedActionFactory actionFactory)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config));
        if (actionStateManager == null)
            throw new ArgumentNullException(nameof(actionStateManager));
        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory));

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid AlternatingActionConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();

        // Auto-generate state key if not provided (alphanumeric only)
        _stateKey = string.IsNullOrEmpty(config.StateKey) ?
            $"Alt{Guid.NewGuid().ToString("N")[..8]}" : config.StateKey;

        // Store references
        _actionStateManager = actionStateManager;

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

        // Build description
        var primaryDesc = config.PrimaryAction.Description ?? "Primary";
        var secondaryDesc = config.SecondaryAction.Description ?? "Secondary";
        Description = config.Description ?? $"Alternate: {primaryDesc} ↔ {secondaryDesc}";

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<AlternatingAction>();
    }

    /// <summary>
    /// Executes the alternating action synchronously.
    /// Checks current state and executes only the appropriate conditional to avoid race conditions.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing AlternatingAction: {Description}, MidiValue={MidiValue}", Description, midiValue);

            // Check current state and execute only the appropriate conditional
            // This prevents race conditions where both conditionals could execute
            var currentState = _actionStateManager.GetState(_stateKey);

            if (currentState == 0)
            {
                // Execute primary conditional (state 0 -> execute primary, set to 1)
                _primaryConditional.Execute(midiValue);
            }
            else if (currentState == 1)
            {
                // Execute secondary conditional (state 1 -> execute secondary, set to 0)
                _secondaryConditional.Execute(midiValue);
            }
            else
            {
                _logger.LogWarning("AlternatingAction state {StateKey} has unexpected value {CurrentState}, no action taken",
                    _stateKey, currentState);
            }

            _logger.LogTrace("Successfully executed AlternatingAction: {Description}", Description);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing AlternatingAction: {Description}", Description);
            ApplicationErrorHandler.ShowError(
                $"Error executing alternating action: {ex.Message}",
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
