using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a mapping between CC value ranges and actions
/// </summary>
public class CCRangeMapping : ControlMapping
{
    /// <summary>
    /// The list of value ranges and their associated actions
    /// </summary>
    public List<CCValueRange> Ranges { get; set; } = new List<CCValueRange>();

    /// <summary>
    /// Optional description of this mapping
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Represents a range of CC values and the action to perform when the value is in that range
/// </summary>
public class CCValueRange
{
    /// <summary>
    /// The minimum CC value for this range (inclusive)
    /// </summary>
    public int MinValue { get; set; }

    /// <summary>
    /// The maximum CC value for this range (inclusive)
    /// </summary>
    public int MaxValue { get; set; }

    /// <summary>
    /// The action to perform when the CC value is in this range
    /// </summary>
    public CCRangeAction Action { get; set; } = new CCRangeAction();
}

/// <summary>
/// Represents an action to perform when a CC value is in a specific range
/// </summary>
public class CCRangeAction
{
    /// <summary>
    /// The type of action to perform
    /// </summary>
    public CCRangeActionType Type { get; set; } = CCRangeActionType.KeyPress;

    /// <summary>
    /// The key to press (for KeyPress action type)
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// The virtual key code to press (for KeyPress action type)
    /// </summary>
    public ushort? VirtualKeyCode { get; set; }

    /// <summary>
    /// Optional modifier keys to hold while pressing the main key (for KeyPress action type)
    /// </summary>
    public List<ushort>? Modifiers { get; set; }

    /// <summary>
    /// The command to execute (for CommandExecution action type)
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// The type of shell to use for command execution (for CommandExecution action type)
    /// </summary>
    public CommandShellType ShellType { get; set; } = CommandShellType.PowerShell;

    /// <summary>
    /// Whether to run the command hidden (for CommandExecution action type)
    /// </summary>
    public bool RunHidden { get; set; } = false;

    /// <summary>
    /// Whether to wait for the command to complete (for CommandExecution action type)
    /// </summary>
    public bool WaitForExit { get; set; } = false;


}

/// <summary>
/// Types of actions that can be performed in a CC range mapping
/// </summary>
public enum CCRangeActionType
{
    /// <summary>
    /// Press and release a key
    /// </summary>
    KeyPress,

    /// <summary>
    /// Execute a command
    /// </summary>
    CommandExecution
}
