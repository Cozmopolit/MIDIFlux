using System.ComponentModel.DataAnnotations;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for RelativeCC actions that handle relative MIDI controllers.
/// Maps relative CC values (scratch wheels, encoders) to increase/decrease actions.
/// </summary>
public class RelativeCCConfig : ActionConfig
{
    /// <summary>
    /// The action to execute for positive/increase values
    /// </summary>
    [Required]
    public ActionConfig IncreaseAction { get; set; } = null!;

    /// <summary>
    /// The action to execute for negative/decrease values
    /// </summary>
    [Required]
    public ActionConfig DecreaseAction { get; set; } = null!;



    /// <summary>
    /// Initializes a new instance of RelativeCCConfig
    /// </summary>
    public RelativeCCConfig()
    {
        Type = ActionType.RelativeCCAction;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (IncreaseAction == null)
        {
            AddValidationError("IncreaseAction is required");
        }
        else if (!IncreaseAction.IsValid())
        {
            AddValidationError($"IncreaseAction is invalid: {string.Join(", ", IncreaseAction.GetValidationErrors())}");
        }

        if (DecreaseAction == null)
        {
            AddValidationError("DecreaseAction is required");
        }
        else if (!DecreaseAction.IsValid())
        {
            AddValidationError($"DecreaseAction is invalid: {string.Join(", ", DecreaseAction.GetValidationErrors())}");
        }



        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;

        var increaseDesc = IncreaseAction?.ToString() ?? "Unknown";
        var decreaseDesc = DecreaseAction?.ToString() ?? "Unknown";
        return $"Relative CC: +({increaseDesc}) / -({decreaseDesc})";
    }
}
