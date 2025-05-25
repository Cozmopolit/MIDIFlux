using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// action for executing actions based on MIDI value conditions (fader-to-buttons).
/// Implements true async behavior for complex orchestration.
/// </summary>
public class ConditionalAction : IAction
{
    private readonly List<ValueConditionConfig> _conditions;
    private readonly List<IAction> _conditionActions;
    private readonly IActionFactory _actionFactory;
    private readonly ILogger<ConditionalAction> _logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the child actions from all conditions
    /// </summary>
    /// <returns>List of child actions</returns>
    public List<IAction> GetChildActions() => new List<IAction>(_conditionActions);

    /// <summary>
    /// Initializes a new instance of ConditionalAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionFactory">The factory to create condition actions</param>
    /// <exception cref="ArgumentNullException">Thrown when config or actionFactory is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public ConditionalAction(ConditionalConfig config, IActionFactory actionFactory)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "ConditionalConfig cannot be null");

        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory), "IActionFactory cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid ConditionalConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Conditional ({config.Conditions.Count} conditions)";
        _conditions = config.Conditions;
        _actionFactory = actionFactory;

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<ConditionalAction>();

        // Create actions for each condition
        _conditionActions = new List<IAction>();
        for (int i = 0; i < config.Conditions.Count; i++)
        {
            try
            {
                var conditionAction = _actionFactory.CreateAction(config.Conditions[i].Action);
                _conditionActions.Add(conditionAction);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to create action for condition {i + 1}: {ex.Message}", nameof(config), ex);
            }
        }
    }

    /// <summary>
    /// Executes the conditional action synchronously.
    /// Finds the first matching condition and executes its action.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) to check against conditions</param>
    public void Execute(int? midiValue = null)
    {
        if (!midiValue.HasValue)
        {
            _logger.LogDebug("ConditionalAction.Execute called without MIDI value, skipping execution: {Description}", Description);
            return;
        }

        _logger.LogDebug("Executing ConditionalAction (sync): {Description}, Conditions={Count}, MidiValue={MidiValue}",
            Description, _conditions.Count, midiValue);

        // Find first matching condition and execute its action
        for (int i = 0; i < _conditions.Count; i++)
        {
            if (_conditions[i].IsInRange(midiValue.Value))
            {
                try
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1} (range {_conditions[i].MinValue}-{_conditions[i].MaxValue})";
                    _logger.LogDebug("Executing matching condition {ConditionIndex}/{Total}: {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);

                    _conditionActions[i].Execute(midiValue);

                    _logger.LogTrace("Successfully executed condition {ConditionIndex}/{Total}: {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);
                    return; // First match wins
                }
                catch (Exception ex)
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1}";
                    _logger.LogError(ex, "Error executing condition {ConditionIndex}/{Total} ({ConditionDescription}): {ErrorMessage}",
                        i + 1, _conditions.Count, conditionDescription, ex.Message);
                    throw new InvalidOperationException($"Error executing condition '{conditionDescription}': {ex.Message}", ex);
                }
            }
        }

        // No matching condition found - this is not an error, just no action to take
        _logger.LogDebug("No matching condition found for MIDI value {MidiValue} in ConditionalAction: {Description}",
            midiValue, Description);
    }

    /// <summary>
    /// Executes the conditional action asynchronously.
    /// Finds the first matching condition and executes its action.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) to check against conditions</param>
    /// <returns>A ValueTask that completes when the matching action is finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        if (!midiValue.HasValue)
        {
            _logger.LogDebug("ConditionalAction.ExecuteAsync called without MIDI value, skipping execution: {Description}", Description);
            return;
        }

        _logger.LogDebug("Executing ConditionalAction (async): {Description}, Conditions={Count}, MidiValue={MidiValue}",
            Description, _conditions.Count, midiValue);

        // Find first matching condition and execute its action asynchronously
        for (int i = 0; i < _conditions.Count; i++)
        {
            if (_conditions[i].IsInRange(midiValue.Value))
            {
                try
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1} (range {_conditions[i].MinValue}-{_conditions[i].MaxValue})";
                    _logger.LogDebug("Executing matching condition {ConditionIndex}/{Total} (async): {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);

                    // Use ExecuteAsync for proper async behavior (especially important for DelayAction and SequenceAction)
                    await _conditionActions[i].ExecuteAsync(midiValue);

                    _logger.LogTrace("Successfully executed condition {ConditionIndex}/{Total} (async): {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);
                    return; // First match wins
                }
                catch (Exception ex)
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1}";
                    _logger.LogError(ex, "Error executing condition {ConditionIndex}/{Total} ({ConditionDescription}): {ErrorMessage}",
                        i + 1, _conditions.Count, conditionDescription, ex.Message);
                    throw new InvalidOperationException($"Error executing condition '{conditionDescription}': {ex.Message}", ex);
                }
            }
        }

        // No matching condition found - this is not an error, just no action to take
        _logger.LogDebug("No matching condition found for MIDI value {MidiValue} in ConditionalAction: {Description}",
            midiValue, Description);
    }

    /// <summary>
    /// Gets the list of value conditions for this conditional action
    /// </summary>
    /// <returns>A read-only list of value condition configurations</returns>
    public IReadOnlyList<ValueConditionConfig> GetConditions()
    {
        return _conditions.AsReadOnly();
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
