using System.ComponentModel.DataAnnotations;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for alternating actions that toggle between two actions on repeated triggers.
/// Convenience wrapper around the stateful action system for common toggle use cases.
/// </summary>
public class AlternatingActionConfig : UnifiedActionConfig
{
    /// <summary>
    /// The first action to execute (when state is 0 or starting state)
    /// </summary>
    [Required]
    public UnifiedActionConfig PrimaryAction { get; set; } = null!;

    /// <summary>
    /// The second action to execute (when state is 1 or alternate state)
    /// </summary>
    [Required]
    public UnifiedActionConfig SecondaryAction { get; set; } = null!;

    /// <summary>
    /// Whether to start with the primary action (true) or secondary action (false)
    /// </summary>
    public bool StartWithPrimary { get; set; } = true;

    /// <summary>
    /// Optional state key for this alternating action. If empty, will be auto-generated.
    /// Must be alphanumeric only to avoid JSON serialization issues.
    /// </summary>
    public string StateKey { get; set; } = "";

    /// <summary>
    /// Initializes a new instance of AlternatingActionConfig
    /// </summary>
    public AlternatingActionConfig()
    {
        Type = UnifiedActionType.AlternatingAction;
    }

    /// <summary>
    /// Validates the alternating action configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public override bool IsValid()
    {
        if (PrimaryAction == null || !PrimaryAction.IsValid())
            return false;

        if (SecondaryAction == null || !SecondaryAction.IsValid())
            return false;

        // Validate state key format if provided
        if (!string.IsNullOrEmpty(StateKey))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(StateKey, @"^[a-zA-Z0-9]+$"))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();

        if (PrimaryAction == null)
        {
            errors.Add("PrimaryAction is required");
        }
        else if (!PrimaryAction.IsValid())
        {
            errors.Add("PrimaryAction configuration is invalid");
            errors.AddRange(PrimaryAction.GetValidationErrors().Select(e => $"PrimaryAction: {e}"));
        }

        if (SecondaryAction == null)
        {
            errors.Add("SecondaryAction is required");
        }
        else if (!SecondaryAction.IsValid())
        {
            errors.Add("SecondaryAction configuration is invalid");
            errors.AddRange(SecondaryAction.GetValidationErrors().Select(e => $"SecondaryAction: {e}"));
        }

        if (!string.IsNullOrEmpty(StateKey))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(StateKey, @"^[a-zA-Z0-9]+$"))
            {
                errors.Add("StateKey must contain only alphanumeric characters");
            }
        }

        return errors;
    }
}
