using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that executes a command
/// </summary>
public class CommandExecutionAction : ActionBase
{
    private readonly ILogger _logger;
    private readonly string _command;
    private readonly CommandShellType _shellType;
    private readonly bool _runHidden;
    private readonly bool _waitForExit;
    private readonly string _description;

    /// <summary>
    /// Creates a new instance of the CommandExecutionAction
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="command">The command to execute</param>
    /// <param name="shellType">The type of shell to use</param>
    /// <param name="runHidden">Whether to run the command hidden</param>
    /// <param name="waitForExit">Whether to wait for the command to complete</param>
    /// <param name="description">Optional description of this action</param>
    public CommandExecutionAction(
        ILogger logger,
        string command,
        CommandShellType shellType = CommandShellType.PowerShell,
        bool runHidden = false,
        bool waitForExit = false,
        string? description = null)
    {
        _logger = logger;
        _command = command;
        _shellType = shellType;
        _runHidden = runHidden;
        _waitForExit = waitForExit;
        _description = description ?? $"Execute {(_shellType == CommandShellType.PowerShell ? "PowerShell" : "CMD")} command";
    }

    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public override string Description => _description;

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.CommandExecution);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override async Task ExecuteAsync()
    {
        _logger.LogInformation("Executing command: {Command} using {ShellType}", 
            _command, _shellType == CommandShellType.PowerShell ? "PowerShell" : "CMD");

        try
        {
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = _runHidden
            };

            // Configure the process based on the shell type
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

            // Start the process
            using var process = new Process { StartInfo = startInfo };
            process.Start();

            // Wait for the process to complete if requested
            if (_waitForExit)
            {
                await process.WaitForExitAsync();
                string output = await process.StandardOutput.ReadToEndAsync();
                string error = await process.StandardError.ReadToEndAsync();

                if (!string.IsNullOrEmpty(output))
                {
                    _logger.LogDebug("Command output: {Output}", output);
                }

                if (!string.IsNullOrEmpty(error))
                {
                    _logger.LogError("Command error: {Error}", error);
                }

                _logger.LogDebug("Command completed with exit code: {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Command}", _command);
        }
    }
}
