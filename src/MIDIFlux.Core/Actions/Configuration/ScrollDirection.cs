namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Defines the direction for mouse scroll actions.
/// Used by MouseScrollConfig to specify scroll wheel direction.
/// </summary>
public enum ScrollDirection
{
    /// <summary>
    /// Scroll up (away from user)
    /// </summary>
    Up,

    /// <summary>
    /// Scroll down (toward user)
    /// </summary>
    Down,

    /// <summary>
    /// Scroll left (horizontal scrolling)
    /// </summary>
    Left,

    /// <summary>
    /// Scroll right (horizontal scrolling)
    /// </summary>
    Right
}
