namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyToggle actions.
/// Represents toggling the state of a key (like CapsLock, NumLock, etc.).
/// </summary>
public class KeyToggleConfig : ActionConfig
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
        Type = ActionType.KeyToggle;
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

        return $"Toggle Key (VK: {VirtualKeyCode})";
    }
}
