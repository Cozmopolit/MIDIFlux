using MIDIFlux.Core.Actions.Configuration;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Describes an action type with metadata for GUI enumeration and factory creation.
/// Provides a single source of truth for action type information to eliminate
/// hardcoded lists and duplicate switch statements in the GUI layer.
/// </summary>
public class ActionDescriptor
{
    /// <summary>
    /// The action type enum value
    /// </summary>
    public ActionType ActionType { get; }

    /// <summary>
    /// Human-readable display name for the action type
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Category for filtering in specialized dialogs
    /// </summary>
    public ActionCategory Category { get; }

    /// <summary>
    /// Whether this action type is available for use (excludes future/deprecated types)
    /// </summary>
    public bool IsAvailable { get; }

    /// <summary>
    /// Factory method to create a default configuration for this action type
    /// </summary>
    public Func<ActionConfig> CreateDefaultConfig { get; }

    /// <summary>
    /// Initializes a new ActionDescriptor
    /// </summary>
    /// <param name="actionType">The action type enum value</param>
    /// <param name="displayName">Human-readable display name</param>
    /// <param name="category">Category for filtering</param>
    /// <param name="createDefaultConfig">Factory method for default configuration</param>
    /// <param name="isAvailable">Whether this action type is available for use</param>
    public ActionDescriptor(
        ActionType actionType,
        string displayName,
        ActionCategory category,
        Func<ActionConfig> createDefaultConfig,
        bool isAvailable = true)
    {
        ActionType = actionType;
        DisplayName = displayName;
        Category = category;
        CreateDefaultConfig = createDefaultConfig;
        IsAvailable = isAvailable;
    }

    /// <summary>
    /// Returns the display name for string representation
    /// </summary>
    public override string ToString() => DisplayName;
}

/// <summary>
/// Categories for action types to enable filtering in specialized dialogs
/// </summary>
public enum ActionCategory
{
    /// <summary>
    /// Keyboard input actions (key press, key down, key up, key toggle)
    /// </summary>
    Keyboard,

    /// <summary>
    /// Mouse input actions (click, scroll)
    /// </summary>
    Mouse,

    /// <summary>
    /// Game controller actions (button, axis)
    /// </summary>
    GameController,

    /// <summary>
    /// System actions (command execution, delay)
    /// </summary>
    System,

    /// <summary>
    /// MIDI output actions
    /// </summary>
    MidiOutput,

    /// <summary>
    /// Complex orchestration actions (sequence, conditional, alternating)
    /// </summary>
    Complex,

    /// <summary>
    /// Stateful actions (state conditional, set state)
    /// </summary>
    Stateful
}
