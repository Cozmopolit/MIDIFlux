using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for CommandExecution actions.
/// Represents executing a shell command (PowerShell or Command Prompt).
/// </summary>
public class CommandExecutionConfig : ActionConfig
{
    /// <summary>
    /// The command to execute
    /// </summary>
    public string Command { get; set; } = "";

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
    /// Initializes a new instance of CommandExecutionConfig
    /// </summary>
    public CommandExecutionConfig()
    {
        Type = ActionType.CommandExecution;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (string.IsNullOrWhiteSpace(Command))
        {
            AddValidationError("Command cannot be empty or whitespace");
        }

        if (!Enum.IsDefined(typeof(CommandShellType), ShellType))
        {
            AddValidationError($"Invalid shell type: {ShellType}");
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

        var shellName = ShellType == CommandShellType.PowerShell ? "PowerShell" : "CMD";
        var commandPreview = Command.Length > 30 ? Command.Substring(0, 30) + "..." : Command;
        return $"Execute {shellName}: {commandPreview}";
    }
}
