using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents a mapping between a MIDI note and a command to execute
/// </summary>
public class CommandMapping
{
    /// <summary>
    /// The MIDI note number to map
    /// </summary>
    public int MidiNote { get; set; }

    /// <summary>
    /// The command to execute
    /// </summary>
    public string Command { get; set; } = string.Empty;

    /// <summary>
    /// The type of shell to use for command execution
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
}

/// <summary>
/// Types of command shells
/// </summary>
public enum CommandShellType
{
    /// <summary>
    /// Windows Command Prompt (cmd.exe)
    /// </summary>
    [JsonPropertyName("cmd")]
    CommandPrompt,

    /// <summary>
    /// Windows PowerShell (powershell.exe)
    /// </summary>
    [JsonPropertyName("powershell")]
    PowerShell
}
