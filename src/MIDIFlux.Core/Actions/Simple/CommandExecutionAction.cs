using System.Diagnostics;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Unified action for executing shell commands.
/// Supports both sync and async execution with configurable behavior.
/// </summary>
public class CommandExecutionAction : IUnifiedAction
{
    private readonly string _command;
    private readonly CommandShellType _shellType;
    private readonly bool _runHidden;
    private readonly bool _waitForExit;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

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
    public CommandExecutionAction(CommandExecutionConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "CommandExecutionConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid CommandExecutionConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Execute {config.ShellType} Command: {config.Command}";
        _command = config.Command;
        _shellType = config.ShellType;
        _runHidden = config.RunHidden;
        _waitForExit = config.WaitForExit;

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<CommandExecutionAction>();
    }

    /// <summary>
    /// Executes the command execution action synchronously.
    /// For commands that need to wait for completion, use ExecuteAsync() instead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing CommandExecutionAction: Command={Command}, ShellType={ShellType}, RunHidden={RunHidden}, WaitForExit={WaitForExit}, MidiValue={MidiValue}",
                _command, _shellType, _runHidden, _waitForExit, midiValue);

            // Execute the command using the existing pattern
            ExecuteCommandInternal(_waitForExit);

            _logger.LogTrace("Successfully executed CommandExecutionAction for Command={Command}", _command);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing CommandExecutionAction for command: {_command}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Executes the command execution action asynchronously.
    /// Provides proper async behavior for commands that need to wait for completion.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the command execution is finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing CommandExecutionAction (async): Command={Command}, ShellType={ShellType}, RunHidden={RunHidden}, WaitForExit={WaitForExit}, MidiValue={MidiValue}",
                _command, _shellType, _runHidden, _waitForExit, midiValue);

            // Execute the command using the existing pattern with async support
            await ExecuteCommandInternalAsync();

            _logger.LogTrace("Successfully executed CommandExecutionAction (async) for Command={Command}", _command);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing CommandExecutionAction (async) for command: {_command}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }

    /// <summary>
    /// Internal method to execute the command synchronously
    /// </summary>
    /// <param name="waitForExit">Whether to wait for the process to complete</param>
    private void ExecuteCommandInternal(bool waitForExit)
    {
        _logger.LogInformation("Executing command: {Command} using {ShellType}",
            _command, _shellType == CommandShellType.PowerShell ? "PowerShell" : "CMD");

        var startInfo = CreateProcessStartInfo();

        // Start the process
        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (waitForExit)
        {
            process.WaitForExit();
            _logger.LogDebug("Command completed with exit code: {ExitCode}", process.ExitCode);
        }
        else
        {
            _logger.LogDebug("Command started (not waiting for completion)");
        }
    }

    /// <summary>
    /// Internal method to execute the command asynchronously
    /// </summary>
    private async Task ExecuteCommandInternalAsync()
    {
        _logger.LogInformation("Executing command (async): {Command} using {ShellType}",
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
                _logger.LogDebug("Command output: {Output}", output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                _logger.LogError("Command error: {Error}", error);
            }

            _logger.LogDebug("Command completed with exit code: {ExitCode}", process.ExitCode);
        }
        else
        {
            _logger.LogDebug("Command started (not waiting for completion)");
        }
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
