using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action that alternates between two actions on repeated triggers using the parameter system.
/// Implements simple toggle behavior without external state dependencies.
/// </summary>
[ActionDisplayName("Alternating Toggle")]
[ActionCategory(ActionCategory.FlowControl)]
public class AlternatingAction : ActionBase
{
    // Parameter names
    private const string PrimaryActionParam = "PrimaryAction";
    private const string SecondaryActionParam = "SecondaryAction";
    private const string StartWithPrimaryParam = "StartWithPrimary";

    // Internal state for alternation (instance-specific)
    private bool _isOnPrimary = true;
    private bool _isInitialized = false;

    /// <summary>
    /// Initializes a new instance of AlternatingAction with default parameters
    /// </summary>
    public AlternatingAction()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Sets a parameter value and updates the description when actions change
    /// </summary>
    public new void SetParameterValue<T>(string parameterName, T value)
    {
        base.SetParameterValue(parameterName, value);

        // Update description when primary or secondary actions are modified
        if (parameterName == PrimaryActionParam || parameterName == SecondaryActionParam)
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
        // Add PrimaryAction parameter
        Parameters[PrimaryActionParam] = new Parameter(
            ParameterType.SubActionList,
            new List<ActionBase>(), // No default - user must specify action
            "Primary Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute on first trigger (and odd-numbered triggers)" },
                { "maxItems", 1 } // AlternatingAction expects exactly one action
            }
        };

        // Add SecondaryAction parameter
        Parameters[SecondaryActionParam] = new Parameter(
            ParameterType.SubActionList,
            new List<ActionBase>(), // No default - user must specify action
            "Secondary Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute on second trigger (and even-numbered triggers)" },
                { "maxItems", 1 } // AlternatingAction expects exactly one action
            }
        };

        // Add StartWithPrimary parameter
        Parameters[StartWithPrimaryParam] = new Parameter(
            ParameterType.Boolean,
            true, // Default to starting with primary
            "Start with Primary")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Whether to start with the primary action (true) or secondary action (false)" }
            }
        };

        // Note: Starting state will be initialized on first execution to ensure
        // parameter values are properly set (especially during JSON deserialization)
    }

    /// <summary>
    /// Core execution logic for the alternating action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var primaryActions = GetParameterValue<List<ActionBase>>(PrimaryActionParam);
        var secondaryActions = GetParameterValue<List<ActionBase>>(SecondaryActionParam);

        // Initialize state on first execution to ensure parameters are properly set
        if (!_isInitialized)
        {
            _isOnPrimary = GetParameterValue<bool>(StartWithPrimaryParam);
            _isInitialized = true;
        }

        // Determine which action to execute based on current state
        List<ActionBase> actionsToExecute;
        string actionType;

        if (_isOnPrimary)
        {
            actionsToExecute = primaryActions;
            actionType = "Primary";
            _isOnPrimary = false; // Switch to secondary for next time
        }
        else
        {
            actionsToExecute = secondaryActions;
            actionType = "Secondary";
            _isOnPrimary = true; // Switch to primary for next time
        }

        Logger.LogDebug("Executing {ActionType} action in AlternatingAction: {Description}", actionType, Description);

        // Execute the selected action(s)
        foreach (var action in actionsToExecute)
        {
            try
            {
                await action.ExecuteAsync(midiValue);
                Logger.LogTrace("Successfully executed {ActionType} action: {ActionDescription}", actionType, action.Description);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing {ActionType} action in AlternatingAction: {ErrorMessage}", actionType, ex.Message);
                // Re-throw with context for caller to handle - UI error display handled by RunWithUiErrorHandling
                throw new InvalidOperationException($"Error executing {actionType} action in AlternatingAction: {ex.Message}", ex);
            }
        }

        Logger.LogTrace("Successfully executed AlternatingAction: {Description}", Description);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var primaryActions = GetParameterValue<List<ActionBase>>(PrimaryActionParam);
            var secondaryActions = GetParameterValue<List<ActionBase>>(SecondaryActionParam);

            var primaryDesc = primaryActions.FirstOrDefault()?.Description ?? "Primary";
            var secondaryDesc = secondaryActions.FirstOrDefault()?.Description ?? "Secondary";
            return $"Alternate: {primaryDesc} ↔ {secondaryDesc}";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Alternating Action";
        }
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing AlternatingAction: {Description}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// AlternatingAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
