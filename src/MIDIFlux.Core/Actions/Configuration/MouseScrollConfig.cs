namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for MouseScroll actions.
/// Represents scrolling the mouse wheel in a specified direction.
/// </summary>
public class MouseScrollConfig : ActionConfig
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
        Type = ActionType.MouseScroll;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (!Enum.IsDefined(typeof(ScrollDirection), Direction))
        {
            AddValidationError($"Invalid scroll direction: {Direction}");
        }

        if (Amount <= 0)
        {
            AddValidationError("Amount must be greater than 0");
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

        var amountText = Amount == 1 ? "" : $" ({Amount} steps)";
        return $"Scroll {Direction}{amountText}";
    }
}
