namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyDown actions.
/// Represents pressing a key down and optionally auto-releasing it after a delay.
/// </summary>
public class KeyDownConfig : ActionConfig
{
    /// <summary>
    /// The virtual key code to press down
    /// </summary>
    public ushort VirtualKeyCode { get; set; }

    /// <summary>
    /// Optional time in milliseconds after which to automatically release the key.
    /// If null, the key remains pressed until explicitly released.
    /// </summary>
    public int? AutoReleaseAfterMs { get; set; }

    /// <summary>
    /// Initializes a new instance of KeyDownConfig
    /// </summary>
    public KeyDownConfig()
    {
        Type = ActionType.KeyDown;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return VirtualKeyCode > 0 && (AutoReleaseAfterMs == null || AutoReleaseAfterMs > 0);
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
        
        if (AutoReleaseAfterMs.HasValue && AutoReleaseAfterMs <= 0)
        {
            errors.Add("AutoReleaseAfterMs must be greater than 0 when specified");
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
            
        var autoRelease = AutoReleaseAfterMs.HasValue ? $" (auto-release after {AutoReleaseAfterMs}ms)" : "";
        return $"Press Key Down (VK: {VirtualKeyCode}){autoRelease}";
    }
}
