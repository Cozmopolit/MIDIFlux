using System.ComponentModel.DataAnnotations;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for setting state values
/// </summary>
public class SetStateConfig : ActionConfig
{
    /// <summary>
    /// The state key to set (case-sensitive, alphanumeric only for user-defined states)
    /// </summary>
    [Required]
    public string StateKey { get; set; } = "";

    /// <summary>
    /// The state value to set
    /// </summary>
    public int StateValue { get; set; } = 0;

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (string.IsNullOrWhiteSpace(StateKey))
        {
            AddValidationError("StateKey cannot be null or empty");
        }
        else if (!IsValidStateKey(StateKey))
        {
            AddValidationError($"StateKey '{StateKey}' is invalid. User-defined state keys must contain only alphanumeric characters. Internal state keys (*Key...) are not allowed in configuration.");
        }

        return GetValidationErrors().Count == 0;
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
