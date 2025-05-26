using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// action for executing actions based on MIDI value conditions (fader-to-buttons).
/// Implements true async behavior for complex orchestration.
/// </summary>
public class ConditionalAction : AsyncActionBase<ConditionalConfig>
{
    private readonly List<ValueConditionConfig> _conditions;
    private readonly List<IAction> _conditionActions;
    private readonly IActionFactory _actionFactory;

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
    public ConditionalAction(ConditionalConfig config, IActionFactory actionFactory) : base(config)
    {
        _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory), "IActionFactory cannot be null");

        _conditions = config.Conditions;

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
    /// Core async execution logic for the conditional action.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) to check against conditions</param>
    /// <returns>A ValueTask that completes when the matching action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (!midiValue.HasValue)
        {
            Logger.LogDebug("ConditionalAction called without MIDI value, skipping execution: {Description}", Description);
            return;
        }

        // Find first matching condition and execute its action
        for (int i = 0; i < _conditions.Count; i++)
        {
            if (_conditions[i].IsInRange(midiValue.Value))
            {
                try
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1} (range {_conditions[i].MinValue}-{_conditions[i].MaxValue})";
                    Logger.LogDebug("Executing matching condition {ConditionIndex}/{Total}: {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);

                    // Use ExecuteAsync for proper async behavior (especially important for DelayAction and SequenceAction)
                    await _conditionActions[i].ExecuteAsync(midiValue);

                    Logger.LogTrace("Successfully executed condition {ConditionIndex}/{Total}: {ConditionDescription}",
                        i + 1, _conditions.Count, conditionDescription);
                    return; // First match wins
                }
                catch (Exception ex)
                {
                    var conditionDescription = _conditions[i].Description ?? $"Condition {i + 1}";
                    Logger.LogError(ex, "Error executing condition {ConditionIndex}/{Total} ({ConditionDescription}): {ErrorMessage}",
                        i + 1, _conditions.Count, conditionDescription, ex.Message);
                    throw new InvalidOperationException($"Error executing condition '{conditionDescription}': {ex.Message}", ex);
                }
            }
        }

        // No matching condition found - this is not an error, just no action to take
        Logger.LogDebug("No matching condition found for MIDI value {MidiValue} in ConditionalAction: {Description}",
            midiValue, Description);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Conditional ({Config.Conditions.Count} conditions)";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing ConditionalAction: {Description}";
    }

    /// <summary>
    /// Gets the list of value conditions for this conditional action
    /// </summary>
    /// <returns>A read-only list of value condition configurations</returns>
    public IReadOnlyList<ValueConditionConfig> GetConditions()
    {
        return _conditions.AsReadOnly();
    }
}
