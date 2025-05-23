namespace MIDIFlux.Core.Models;

/// <summary>
/// Types of actions that can be executed as part of a macro
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Press and release a key
    /// </summary>
    KeyPressRelease,

    /// <summary>
    /// Press a key down but don't release it
    /// </summary>
    KeyDown,

    /// <summary>
    /// Release a key that was previously pressed
    /// </summary>
    KeyUp,

    /// <summary>
    /// Toggle a key state (like CapsLock)
    /// </summary>
    KeyToggle,

    /// <summary>
    /// Execute a command (PowerShell or Command Prompt)
    /// </summary>
    CommandExecution,

    /// <summary>
    /// Wait for a specified time
    /// </summary>
    Delay,

    /// <summary>
    /// Move the mouse cursor to a specific position
    /// </summary>
    MouseMove,

    /// <summary>
    /// Press a mouse button
    /// </summary>
    MouseDown,

    /// <summary>
    /// Release a mouse button
    /// </summary>
    MouseUp,

    /// <summary>
    /// Click a mouse button (press and release)
    /// </summary>
    MouseClick,

    /// <summary>
    /// Scroll the mouse wheel
    /// </summary>
    MouseScroll,

    /// <summary>
    /// Execute a sequence of actions
    /// </summary>
    Macro
}
