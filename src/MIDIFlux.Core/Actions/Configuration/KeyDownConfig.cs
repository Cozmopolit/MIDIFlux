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
        base.IsValid(); // Clear previous errors

        if (VirtualKeyCode == 0)
        {
            AddValidationError("VirtualKeyCode must be greater than 0");
        }

        if (AutoReleaseAfterMs.HasValue && AutoReleaseAfterMs <= 0)
        {
            AddValidationError("AutoReleaseAfterMs must be greater than 0 when specified");
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

        var autoRelease = AutoReleaseAfterMs.HasValue ? $" (auto-release after {AutoReleaseAfterMs}ms)" : "";
        return $"Press Key Down (VK: {VirtualKeyCode}){autoRelease}";
    }
}
