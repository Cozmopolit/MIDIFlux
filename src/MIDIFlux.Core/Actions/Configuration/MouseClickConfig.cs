using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for MouseClick actions.
/// Represents clicking a mouse button (Left, Right, Middle).
/// </summary>
public class MouseClickConfig : UnifiedActionConfig
{
    /// <summary>
    /// The mouse button to click
    /// </summary>
    public MouseButton Button { get; set; } = MouseButton.Left;

    /// <summary>
    /// Initializes a new instance of MouseClickConfig
    /// </summary>
    public MouseClickConfig()
    {
        Type = UnifiedActionType.MouseClick;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return Enum.IsDefined(typeof(MouseButton), Button);
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (!Enum.IsDefined(typeof(MouseButton), Button))
        {
            errors.Add($"Invalid mouse button: {Button}");
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
            
        return $"Click {Button} Mouse Button";
    }
}
