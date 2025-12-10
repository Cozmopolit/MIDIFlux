using System.Diagnostics;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for executing shell commands.
/// Consolidates CommandExecutionConfig into the action class using the parameter system.
/// Supports async execution with configurable behavior for optimal performance.
/// </summary>
[ActionDisplayName("Execute Command")]
[ActionCategory(ActionCategory.Utility)]
public class CommandExecutionAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string CommandParam = "Command";
    private const string ShellTypeParam = "ShellType";
    private const string RunHiddenParam = "RunHidden";
    private const string WaitForExitParam = "WaitForExit";



    /// <summary>
    /// Initializes a new instance of CommandExecutionAction with default parameters
    /// </summary>
    public CommandExecutionAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of CommandExecutionAction with specified parameters
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="shellType">The shell type (default: PowerShell)</param>
    /// <param name="runHidden">Whether to run hidden (default: true)</param>
    /// <param name="waitForExit">Whether to wait for exit (default: true)</param>
    public CommandExecutionAction(string command, CommandShellType shellType = CommandShellType.PowerShell,
        bool runHidden = true, bool waitForExit = true) : base()
    {
        SetParameterValue(CommandParam, command);
        var shellTypeString = shellType switch
        {
            CommandShellType.PowerShell => "PowerShell",
            CommandShellType.CommandPrompt => "CommandPrompt",
            _ => "PowerShell" // Default fallback
        };
        SetParameterValue(ShellTypeParam, shellTypeString);
        SetParameterValue(RunHiddenParam, runHidden);
        SetParameterValue(WaitForExitParam, waitForExit);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Command parameter with string type
        Parameters[CommandParam] = new Parameter(
            ParameterType.String,
            "", // No default - user must specify command
            "Command")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "maxLength", 1000 }
            }
        };

        // Add ShellType parameter with string type
        Parameters[ShellTypeParam] = new Parameter(
            ParameterType.String,
            "", // No default - user must specify shell type
            "Shell Type")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "PowerShell", "CommandPrompt" } }
            }
        };

        // Add RunHidden parameter with boolean type
        Parameters[RunHiddenParam] = new Parameter(
            ParameterType.Boolean,
            true, // Default to hidden
            "Run Hidden")
        {
            ValidationHints = new Dictionary<string, object>()
        };

        // Add WaitForExit parameter with boolean type
        Parameters[WaitForExitParam] = new Parameter(
            ParameterType.Boolean,
            true, // Default to wait for exit
            "Wait For Exit")
        {
            ValidationHints = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var command = GetParameterValue<string>(CommandParam);
        if (string.IsNullOrWhiteSpace(command))
        {
            AddValidationError("Command cannot be empty");
        }
        else if (command.Length > 1000)
        {
            AddValidationError("Command must not exceed 1000 characters");
        }

        var shellType = GetParameterValue<string>(ShellTypeParam);
        var allowedShellTypes = new[] { "PowerShell", "CommandPrompt" };
        if (string.IsNullOrWhiteSpace(shellType) || !allowedShellTypes.Contains(shellType))
        {
            AddValidationError($"ShellType must be specified and one of: {string.Join(", ", allowedShellTypes)}");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core async execution logic for the command execution action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the command execution is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var command = GetParameterValue<string>(CommandParam);
        var shellTypeString = GetParameterValue<string>(ShellTypeParam);
        var runHidden = GetParameterValue<bool>(RunHiddenParam);
        var waitForExit = GetParameterValue<bool>(WaitForExitParam);

        Logger.LogDebug("Executing command: {Command} using {ShellType}",
            command, shellTypeString);

        var startInfo = CreateProcessStartInfo(command, shellTypeString, runHidden);

        // Start the process
        using var process = new Process { StartInfo = startInfo };
        process.Start();

        if (waitForExit)
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

        Logger.LogTrace("Successfully executed CommandExecutionAction for Command={Command}", command);
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            return $"Execute {GetParameterValue<string>(ShellTypeParam)} Command: {GetParameterValue<string>(CommandParam)}";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Execute Command";
        }
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing CommandExecutionAction for command: {GetParameterValue<string>(CommandParam)}";
    }

    /// <summary>
    /// Creates the ProcessStartInfo for command execution
    /// </summary>
    /// <param name="command">The command to execute</param>
    /// <param name="shellType">The shell type</param>
    /// <param name="runHidden">Whether to run hidden</param>
    /// <returns>Configured ProcessStartInfo</returns>
    private static ProcessStartInfo CreateProcessStartInfo(string command, string shellType, bool runHidden)
    {
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = runHidden
        };

        // Configure the process based on shell type (following existing patterns)
        if (shellType == "PowerShell")
        {
            startInfo.FileName = "powershell.exe";
            startInfo.Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{command}\"";
        }
        else // CommandPrompt
        {
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = $"/c {command}";
        }

        return startInfo;
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// CommandExecutionAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
