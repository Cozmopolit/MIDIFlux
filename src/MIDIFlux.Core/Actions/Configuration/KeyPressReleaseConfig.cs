namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyPressRelease actions.
/// Represents a simple key press and release operation.
/// </summary>
public class KeyPressReleaseConfig : UnifiedActionConfig
{
    /// <summary>
    /// The virtual key code to press and release
    /// </summary>
    public ushort VirtualKeyCode { get; set; }

    /// <summary>
    /// Initializes a new instance of KeyPressReleaseConfig
    /// </summary>
    public KeyPressReleaseConfig()
    {
        Type = UnifiedActionType.KeyPressRelease;
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
            
        return $"Press/Release Key (VK: {VirtualKeyCode})";
    }
}
