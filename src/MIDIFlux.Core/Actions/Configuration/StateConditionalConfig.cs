using System.ComponentModel.DataAnnotations;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// State comparison types for conditional actions
/// </summary>
public enum StateComparison
{
    Equals,        // state == value
    GreaterThan,   // state > value
    LessThan       // state < value
}

/// <summary>
/// Configuration for a single state condition entry
/// </summary>
public class StateConditionalEntry
{
    /// <summary>
    /// The state value to compare against
    /// </summary>
    public int StateValue { get; set; } = 0;

    /// <summary>
    /// The comparison type to use
    /// </summary>
    public StateComparison Comparison { get; set; } = StateComparison.Equals;

    /// <summary>
    /// The action to execute if the condition matches
    /// </summary>
    [Required]
    public ActionConfig Action { get; set; } = null!;

    /// <summary>
    /// The state value to set after executing the action (-1 = no change)
    /// </summary>
    public int SetStateAfter { get; set; } = -1;

    /// <summary>
    /// Human-readable description of this condition
    /// </summary>
    public string Description { get; set; } = "";
}

/// <summary>
/// Configuration for state conditional actions that execute different actions based on state values
/// </summary>
public class StateConditionalConfig : ActionConfig
{
    /// <summary>
    /// The state key to check (case-sensitive, alphanumeric only for user-defined states)
    /// </summary>
    [Required]
    public string StateKey { get; set; } = "";

    /// <summary>
    /// The condition to check (single condition model for simplicity)
    /// </summary>
    [Required]
    public StateConditionalEntry Condition { get; set; } = new();

    /// <summary>
    /// Validates the configuration
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(StateKey) &&
               IsValidStateKey(StateKey) &&
               Condition?.Action != null &&
               Condition.Action.IsValid();
    }

    /// <summary>
    /// Gets validation errors for this configuration
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(StateKey))
        {
            errors.Add("StateKey cannot be null or empty");
        }
        else if (!IsValidStateKey(StateKey))
        {
            errors.Add($"StateKey '{StateKey}' is invalid. User-defined state keys must contain only alphanumeric characters. Internal state keys (*Key...) are not allowed in configuration.");
        }

        if (Condition?.Action == null)
        {
            errors.Add("Condition.Action cannot be null");
        }
        else if (!Condition.Action.IsValid())
        {
            var actionErrors = Condition.Action.GetValidationErrors();
            errors.AddRange(actionErrors.Select(e => $"Condition.Action: {e}"));
        }

        return errors;
    }

    /// <summary>
    /// Validates state key format (user-defined states only in configuration)
    /// </summary>
    /// <param name="stateKey">The state key to validate</param>
    /// <returns>True if valid, false otherwise</returns>
    private static bool IsValidStateKey(string stateKey)
    {
        // User-defined state keys must be alphanumeric only
        // Internal state keys (*Key...) are not allowed in configuration
        return !string.IsNullOrWhiteSpace(stateKey) &&
               !stateKey.StartsWith("*") &&
               stateKey.All(char.IsLetterOrDigit);
    }
}
