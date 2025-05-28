namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for KeyUp actions.
/// Represents releasing a key that was previously pressed down.
/// </summary>
public class KeyUpConfig : ActionConfig
{
    /// <summary>
    /// The virtual key code to release
    /// </summary>
    public ushort VirtualKeyCode { get; set; }

    /// <summary>
    /// Initializes a new instance of KeyUpConfig
    /// </summary>
    public KeyUpConfig()
    {
        Type = ActionType.KeyUp;
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

        return $"Release Key (VK: {VirtualKeyCode})";
    }
}
