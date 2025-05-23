namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a mapping between a MIDI note and keyboard actions
/// </summary>
public class KeyMapping
{
    /// <summary>
    /// The MIDI note number to map
    /// </summary>
    public int MidiNote { get; set; }

    /// <summary>
    /// The virtual key code to press/release (for simple mappings)
    /// </summary>
    public ushort VirtualKeyCode { get; set; }

    /// <summary>
    /// Optional modifier keys to hold while pressing/releasing the main key (for simple mappings)
    /// </summary>
    public List<ushort> Modifiers { get; set; } = new List<ushort>();

    /// <summary>
    /// The type of action to perform (default is PressAndRelease)
    /// </summary>
    public KeyActionType ActionType { get; set; } = KeyActionType.PressAndRelease;



    /// <summary>
    /// The command to execute (for CommandExecution action type)
    /// </summary>
    public string? Command { get; set; }

    /// <summary>
    /// The type of shell to use for command execution (for CommandExecution action type)
    /// </summary>
    public CommandShellType ShellType { get; set; } = CommandShellType.PowerShell;

    /// <summary>
    /// Whether to run the command hidden (without showing a console window)
    /// </summary>
    public bool RunHidden { get; set; } = false;

    /// <summary>
    /// Whether to wait for the command to complete before continuing
    /// </summary>
    public bool WaitForExit { get; set; } = false;

    /// <summary>
    /// Optional description of this mapping
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether to ignore Note-Off events for this mapping
    /// </summary>
    public bool IgnoreNoteOff { get; set; } = false;

    /// <summary>
    /// Optional time in milliseconds after which to automatically release the key when IgnoreNoteOff is true
    /// </summary>
    public int? AutoReleaseAfterMs { get; set; }
}



/// <summary>
/// Types of key actions
/// </summary>
public enum KeyActionType
{
    /// <summary>
    /// Press and release the key (default)
    /// </summary>
    PressAndRelease,

    /// <summary>
    /// Press the key down but don't release it
    /// </summary>
    KeyDown,

    /// <summary>
    /// Release a key that was previously pressed
    /// </summary>
    KeyUp,

    /// <summary>
    /// Toggle the key state (like CapsLock)
    /// </summary>
    Toggle,

    /// <summary>
    /// Execute a command (PowerShell or Command Prompt)
    /// </summary>
    CommandExecution
}
