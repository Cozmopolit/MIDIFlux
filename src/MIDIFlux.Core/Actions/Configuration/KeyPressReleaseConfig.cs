namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyPressRelease actions.
/// Represents a simple key press and release operation.
/// </summary>
public class KeyPressReleaseConfig : ActionConfig
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
        Type = ActionType.KeyPressRelease;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (VirtualKeyCode == 0)
        {
            AddValidationError("VirtualKeyCode must be greater than 0");
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

        return $"Press/Release Key (VK: {VirtualKeyCode})";
    }
}
