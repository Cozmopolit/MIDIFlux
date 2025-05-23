using System;
using System.Diagnostics;
using System.IO;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Commands;

/// <summary>
/// Handles executing commands when a MIDI note is pressed
/// </summary>
public class CommandExecutionHandler : INoteHandler
{
    private readonly ILogger _logger;
    private readonly string _command;
    private readonly CommandShellType _shellType;
    private readonly bool _runHidden;
    private readonly bool _waitForExit;
    private readonly string _description;

    /// <summary>
    /// Creates a new instance of CommandExecutionHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="command">The command to execute</param>
    /// <param name="shellType">The type of shell to use</param>
    /// <param name="runHidden">Whether to run the command hidden</param>
    /// <param name="waitForExit">Whether to wait for the command to complete</param>
    /// <param name="description">Optional description of this handler</param>
    public CommandExecutionHandler(
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
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// Handles a note on event by executing the command
    /// </summary>
    /// <param name="velocity">The note velocity (0-127)</param>
    public void HandleNoteOn(int velocity)
    {
        _logger.LogDebug("Note On event received for command execution: {Command}", _command);
        ExecuteCommand();
    }

    /// <summary>
    /// Handles a note off event (does nothing for command execution)
    /// </summary>
    public void HandleNoteOff()
    {
        // Do nothing for note off events in command execution mode
        _logger.LogDebug("Note Off event ignored for command execution");
    }

    /// <summary>
    /// Executes the command using the specified shell
    /// </summary>
    private void ExecuteCommand()
    {
        try
        {
            _logger.LogInformation("Executing command: {Command} using {ShellType}",
                _command, _shellType == CommandShellType.PowerShell ? "PowerShell" : "CMD");

            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = _runHidden
            };

            // Configure the process based on shell type
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
            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogInformation("Command output: {Output}", e.Data);
                }
            };
            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    _logger.LogError("Command error: {Error}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (_waitForExit)
            {
                process.WaitForExit();
                _logger.LogInformation("Command completed with exit code: {ExitCode}", process.ExitCode);
            }
            else
            {
                _logger.LogInformation("Command started (not waiting for completion)");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Message}", ex.Message);
        }
    }
}
