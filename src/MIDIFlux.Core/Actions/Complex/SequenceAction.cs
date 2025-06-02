using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action for executing a sequence of actions (macros).
/// Implements true async behavior for complex orchestration using the unified parameter system.
/// </summary>
[ActionDisplayName("Sequence (Macro)")]
public class SequenceAction : ActionBase
{
    // Parameter names
    private const string SubActionsParam = "SubActions";
    private const string ErrorHandlingParam = "ErrorHandling";



    /// <summary>
    /// Gets the child actions in this sequence (internal convenience method)
    /// </summary>
    /// <returns>List of child actions</returns>
    private List<ActionBase> GetChildActions() => GetParameterValue<List<ActionBase>>(SubActionsParam);

    /// <summary>
    /// Initializes a new instance of SequenceAction with default parameters
    /// </summary>
    public SequenceAction()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Sets a parameter value and updates the description when sub-actions change
    /// </summary>
    public new void SetParameterValue<T>(string parameterName, T value)
    {
        base.SetParameterValue(parameterName, value);

        // Update description when sub-actions are modified
        if (parameterName == SubActionsParam)
        {
            UpdateDescription();
        }
    }

    /// <summary>
    /// Updates the description based on current parameter values
    /// </summary>
    private void UpdateDescription()
    {
        Description = GetDefaultDescription();
    }

    /// <summary>
    /// Override JsonParameters setter to update description after JSON deserialization
    /// </summary>
    public new Dictionary<string, object?> JsonParameters
    {
        get => base.JsonParameters;
        set
        {
            base.JsonParameters = value;
            // Update description after parameters are loaded from JSON
            UpdateDescription();
        }
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add SubActions parameter with SubActionList type
        Parameters[SubActionsParam] = new Parameter(
            ParameterType.SubActionList,
            new List<ActionBase>(), // Default to empty list
            "Sub Actions")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "List of actions to execute in sequence" }
            }
        };

        // Add ErrorHandling parameter with enum type
        Parameters[ErrorHandlingParam] = new Parameter(
            ParameterType.Enum,
            null, // No default - user must explicitly choose error handling behavior
            "Error Handling")
        {
            EnumTypeName = nameof(SequenceErrorHandling),
            EnumDefinition = new EnumDefinition
            {
                Options = new[] { "Continue on Error", "Stop on Error" },
                Values = new object[] { SequenceErrorHandling.ContinueOnError, SequenceErrorHandling.StopOnError }
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        // Clear previous validation errors and validate base parameters
        base.IsValid();

        // Validate that ErrorHandling parameter is set (not null)
        var errorHandlingParam = Parameters[ErrorHandlingParam];
        if (errorHandlingParam.Value == null)
        {
            AddValidationError("Error Handling must be specified - choose either 'Continue on Error' or 'Stop on Error'");
        }

        // Validate that SubActions list is not empty
        var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
        if (subActions.Count == 0)
        {
            AddValidationError("At least one sub-action must be specified for the sequence");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core async execution logic for the sequence action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when all sub-actions are finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
        var errorHandling = GetParameterValue<SequenceErrorHandling>(ErrorHandlingParam);

        // Execute sub-actions sequentially with proper async/await
        // This allows DelayActions to work properly with actual delays
        var exceptions = new List<Exception>();

        for (int i = 0; i < subActions.Count; i++)
        {
            try
            {
                var subAction = subActions[i];

                // Use ExecuteAsync for proper async behavior (especially important for DelayAction)
                await subAction.ExecuteAsync(midiValue);
            }
            catch (Exception ex)
            {
                var actionDescription = subActions[i].Description ?? $"Sub-action {i + 1}";
                var wrappedException = new InvalidOperationException($"Error in sequence step {i + 1} ({actionDescription}): {ex.Message}", ex);
                exceptions.Add(wrappedException);

                Logger.LogError(ex, "Error in sequence step {Step}/{Total} ({ActionDescription}): {ErrorMessage}",
                    i + 1, subActions.Count, actionDescription, ex.Message);

                if (errorHandling == SequenceErrorHandling.StopOnError)
                {
                    Logger.LogWarning("Stopping sequence execution due to error in step {Step} (StopOnError mode)", i + 1);
                    break;
                }
                else
                {
                    Logger.LogDebug("Continuing sequence execution despite error in step {Step} (ContinueOnError mode)", i + 1);
                }
            }
        }

        // Handle accumulated exceptions - throw them for caller to handle
        // UI error display is now handled by callers using RunWithUiErrorHandling
        if (exceptions.Count > 0)
        {
            if (exceptions.Count == 1)
            {
                Logger.LogError("SequenceAction failed with single error");
                throw exceptions[0];
            }
            else
            {
                var aggregateEx = new AggregateException($"Multiple errors occurred in sequence execution", exceptions);
                Logger.LogError("SequenceAction failed with {ErrorCount} errors", exceptions.Count);
                throw aggregateEx;
            }
        }

        Logger.LogTrace("Successfully completed SequenceAction: {Description}", Description);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var subActions = GetParameterValue<List<ActionBase>>(SubActionsParam);
            return $"Sequence ({subActions.Count} actions)";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Sequence Action";
        }
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing SequenceAction: {Description}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// SequenceAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
