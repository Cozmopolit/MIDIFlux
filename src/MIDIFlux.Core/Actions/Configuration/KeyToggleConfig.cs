namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyToggle actions.
/// Represents toggling the state of a key (like CapsLock, NumLock, etc.).
/// </summary>
public class KeyToggleConfig : UnifiedActionConfig
{
    /// <summary>
    /// The virtual key code to toggle
    /// </summary>
    public ushort VirtualKeyCode { get; set; }

    /// <summary>
    /// Initializes a new instance of KeyToggleConfig
    /// </summary>
    public KeyToggleConfig()
    {
        Type = UnifiedActionType.KeyToggle;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return VirtualKeyCode > 0;
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (VirtualKeyCode == 0)
        {
            errors.Add("VirtualKeyCode must be greater than 0");
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
            
        return $"Toggle Key (VK: {VirtualKeyCode})";
    }
}
