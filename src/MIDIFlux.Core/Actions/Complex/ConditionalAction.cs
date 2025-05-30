using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action for executing actions based on MIDI value conditions (fader-to-buttons).
/// Implements true async behavior for complex orchestration using the parameter system.
/// </summary>
[ActionDisplayName("Conditional (CC Range)")]
public class ConditionalAction : ActionBase
{
    // Parameter names
    private const string ConditionsParam = "Conditions";

    /// <summary>
    /// Gets the value conditions for this conditional action (internal convenience method)
    /// </summary>
    private List<ValueCondition> GetConditions() => GetParameterValue<List<ValueCondition>>(ConditionsParam);

    /// <summary>
    /// Initializes a new instance of ConditionalAction with default parameters
    /// </summary>
    public ConditionalAction()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Conditions parameter with ValueConditionList type
        Parameters[ConditionsParam] = new Parameter(
            ParameterType.ValueConditionList,
            new List<ValueCondition>(), // Default to empty list
            "Conditions")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "List of value conditions with ranges and associated actions" }
            }
        };
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

        var conditions = GetConditions();

        // Find first matching condition and execute its action
        for (int i = 0; i < conditions.Count; i++)
        {
            if (conditions[i].IsInRange(midiValue.Value))
            {
                try
                {
                    // Use ExecuteAsync for proper async behavior (especially important for DelayAction and SequenceAction)
                    await conditions[i].Action.ExecuteAsync(midiValue);
                    return; // First match wins
                }
                catch (Exception ex)
                {
                    var conditionDescription = conditions[i].Description ?? $"Condition {i + 1}";
                    Logger.LogError(ex, "Error executing condition {ConditionIndex}/{Total} ({ConditionDescription}): {ErrorMessage}",
                        i + 1, conditions.Count, conditionDescription, ex.Message);
                    // Re-throw with context for caller to handle - UI error display handled by RunWithUiErrorHandling
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
        var conditions = GetParameterValue<List<ValueCondition>>(ConditionsParam);
        return $"Conditional ({conditions.Count} conditions)";
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
    /// Gets the input type categories that are compatible with this action.
    /// ConditionalAction supports both trigger signals (for threshold-based logic) and absolute value signals (for range-based logic).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue };
    }
}
