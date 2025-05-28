using System.Diagnostics;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for executing shell commands.
/// Supports async execution with configurable behavior for optimal performance.
/// </summary>
public class CommandExecutionAction : ActionBase<CommandExecutionConfig>
{
    private readonly string _command;
    private readonly CommandShellType _shellType;
    private readonly bool _runHidden;
    private readonly bool _waitForExit;

    /// <summary>
    /// Gets the command to execute
    /// </summary>
    public string Command => _command;

    /// <summary>
    /// Gets the shell type for command execution
    /// </summary>
    public CommandShellType ShellType => _shellType;

    /// <summary>
    /// Initializes a new instance of CommandExecutionAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public CommandExecutionAction(CommandExecutionConfig config) : base(config)
    {
        _command = config.Command;
        _shellType = config.ShellType;
        _runHidden = config.RunHidden;
        _waitForExit = config.WaitForExit;
    }

    /// <summary>
    /// Core async execution logic for the command execution action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the command execution is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        Logger.LogInformation("Executing command: {Command} using {ShellType}",
            _command, _shellType == CommandShellType.PowerShell ? "PowerShell" : "CMD");

        var startInfo = CreateProcessStartInfo();

        // Start the process
        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (_waitForExit)
        {
            await process.WaitForExitAsync();
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            if (!string.IsNullOrEmpty(output))
            {
                Logger.LogDebug("Command output: {Output}", output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                Logger.LogError("Command error: {Error}", error);
            }

            Logger.LogDebug("Command completed with exit code: {ExitCode}", process.ExitCode);
        }
        else
        {
            Logger.LogDebug("Command started (not waiting for completion)");
        }
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Execute {_shellType} Command: {_command}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing CommandExecutionAction for command: {_command}";
    }

    /// <summary>
    /// Creates the ProcessStartInfo for command execution
    /// </summary>
    /// <returns>Configured ProcessStartInfo</returns>
    private ProcessStartInfo CreateProcessStartInfo()
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = _runHidden
        };

        // Configure the process based on shell type (following existing patterns)
        if (_shellType == CommandShellType.PowerShell)
        {
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{_command}\"";
        }
        else
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c {_command}";
        }

        return startInfo;
    }
}
