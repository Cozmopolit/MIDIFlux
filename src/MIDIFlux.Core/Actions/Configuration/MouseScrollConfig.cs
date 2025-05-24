namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for MouseScroll actions.
/// Represents scrolling the mouse wheel in a specified direction.
/// </summary>
public class MouseScrollConfig : UnifiedActionConfig
{
    /// <summary>
    /// The direction to scroll
    /// </summary>
    public ScrollDirection Direction { get; set; } = ScrollDirection.Up;

    /// <summary>
    /// The number of scroll steps to perform (default: 1)
    /// </summary>
    public int Amount { get; set; } = 1;

    /// <summary>
    /// Initializes a new instance of MouseScrollConfig
    /// </summary>
    public MouseScrollConfig()
    {
        Type = UnifiedActionType.MouseScroll;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return Enum.IsDefined(typeof(ScrollDirection), Direction) && Amount > 0;
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (!Enum.IsDefined(typeof(ScrollDirection), Direction))
        {
            errors.Add($"Invalid scroll direction: {Direction}");
        }
        
        if (Amount <= 0)
        {
            errors.Add("Amount must be greater than 0");
        }
        
        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;
            
        var amountText = Amount == 1 ? "" : $" ({Amount} steps)";
        return $"Scroll {Direction}{amountText}";
    }
}
