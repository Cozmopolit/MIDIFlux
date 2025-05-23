using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a mapping between a MIDI note and a sequence of actions (macro)
/// </summary>
public class MacroMapping
{
    /// <summary>
    /// The MIDI note number to map
    /// </summary>
    public int MidiNote { get; set; }

    /// <summary>
    /// The list of actions to execute in sequence
    /// </summary>
    public List<MacroActionDefinition> Actions { get; set; } = new List<MacroActionDefinition>();

    /// <summary>
    /// Optional description of this mapping
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether to ignore Note-Off events for this mapping
    /// </summary>
    public bool IgnoreNoteOff { get; set; } = true;

    /// <summary>
    /// Unique identifier for this macro mapping (used for anti-recursion protection)
    /// </summary>
    [JsonIgnore]
    public string Id => $"Macro_{MidiNote}";
}

/// <summary>
/// Represents an action definition to perform as part of a macro
/// </summary>
public class MacroActionDefinition
{
    /// <summary>
    /// The type of action to perform
    /// </summary>
    public ActionType Type { get; set; } = ActionType.KeyPressRelease;

    /// <summary>
    /// The virtual key code to press/release (for keyboard actions)
    /// </summary>
    public ushort? VirtualKeyCode { get; set; }

    /// <summary>
    /// Optional modifier keys to hold while pressing/releasing the main key (for keyboard actions)
    /// </summary>
    public List<ushort>? Modifiers { get; set; }

    /// <summary>
    /// The command to execute (for CommandExecution action type)
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// The type of shell to use for command execution (for CommandExecution action type)
    /// </summary>
    public CommandShellType? ShellType { get; set; }

    /// <summary>
    /// Whether to run the command hidden (for CommandExecution action type)
    /// </summary>
    public bool RunHidden { get; set; } = false;

    /// <summary>
    /// Whether to wait for the command to complete before continuing (for CommandExecution action type)
    /// </summary>
    public bool WaitForExit { get; set; } = false;

    /// <summary>
    /// The number of milliseconds to wait (for Delay action type)
    /// </summary>
    public int? Milliseconds { get; set; }

    /// <summary>
    /// The ID of the macro to execute (for Macro action type)
    /// </summary>
    public string? MacroId { get; set; }

    /// <summary>
    /// The mouse X position (for MouseMove action type)
    /// </summary>
    public int? MouseX { get; set; }

    /// <summary>
    /// The mouse Y position (for MouseMove action type)
    /// </summary>
    public int? MouseY { get; set; }

    /// <summary>
    /// The mouse button to press/release (for MouseDown/MouseUp action types)
    /// </summary>
    public MouseButton? MouseButton { get; set; }

    /// <summary>
    /// Optional description of this action
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional delay in milliseconds before the next action
    /// </summary>
    public int DelayAfter { get; set; } = 0;

    /// <summary>
    /// Gets a user-friendly description of this action
    /// </summary>
    [JsonIgnore]
    public string DisplayDescription
    {
        get
        {
            if (!string.IsNullOrEmpty(Description))
                return Description;

            switch (Type)
            {
                case ActionType.KeyPressRelease:
                    return $"Press and release key {VirtualKeyCode}";
                case ActionType.KeyDown:
                    return $"Press key {VirtualKeyCode}";
                case ActionType.KeyUp:
                    return $"Release key {VirtualKeyCode}";
                case ActionType.KeyToggle:
                    return $"Toggle key {VirtualKeyCode}";
                case ActionType.CommandExecution:
                    return $"Execute command: {Command}";
                case ActionType.Delay:
                    return $"Wait for {Milliseconds} ms";
                case ActionType.Macro:
                    return $"Execute macro: {MacroId}";
                case ActionType.MouseMove:
                    return $"Move mouse to ({MouseX}, {MouseY})";
                case ActionType.MouseDown:
                    return $"Press mouse button: {MouseButton}";
                case ActionType.MouseUp:
                    return $"Release mouse button: {MouseButton}";
                default:
                    return $"Unknown action type: {Type}";
            }
        }
    }
}

/// <summary>
/// Mouse buttons that can be used in mouse actions
/// </summary>
public enum MouseButton
{
    /// <summary>
    /// Left mouse button
    /// </summary>
    Left,

    /// <summary>
    /// Right mouse button
    /// </summary>
    Right,

    /// <summary>
    /// Middle mouse button
    /// </summary>
    Middle
}
