using System.ComponentModel.DataAnnotations;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for alternating actions that toggle between two actions on repeated triggers.
/// Convenience wrapper around the stateful action system for common toggle use cases.
/// </summary>
public class AlternatingActionConfig : ActionConfig
{
    /// <summary>
    /// The first action to execute (when state is 0 or starting state)
    /// </summary>
    [Required]
    public ActionConfig PrimaryAction { get; set; } = null!;

    /// <summary>
    /// The second action to execute (when state is 1 or alternate state)
    /// </summary>
    [Required]
    public ActionConfig SecondaryAction { get; set; } = null!;

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
        Type = ActionType.AlternatingAction;
    }

    /// <summary>
    /// Validates the alternating action configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (PrimaryAction == null)
        {
            AddValidationError("PrimaryAction is required");
        }
        else if (!PrimaryAction.IsValid())
        {
            AddValidationError("PrimaryAction configuration is invalid");
            foreach (var error in PrimaryAction.GetValidationErrors())
            {
                AddValidationError($"PrimaryAction: {error}");
            }
        }

        if (SecondaryAction == null)
        {
            AddValidationError("SecondaryAction is required");
        }
        else if (!SecondaryAction.IsValid())
        {
            AddValidationError("SecondaryAction configuration is invalid");
            foreach (var error in SecondaryAction.GetValidationErrors())
            {
                AddValidationError($"SecondaryAction: {error}");
            }
        }

        if (!string.IsNullOrEmpty(StateKey))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(StateKey, @"^[a-zA-Z0-9]+$"))
            {
                AddValidationError("StateKey must contain only alphanumeric characters");
            }
        }

        return GetValidationErrors().Count == 0;
    }
}
