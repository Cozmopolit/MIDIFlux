using MIDIFlux.Core.Extensions;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MIDIFlux.Core.Handlers;

/// <summary>
/// Handles mapping CC value ranges to different actions
/// </summary>
public class CCRangeHandler : IAbsoluteValueHandler
{
    private readonly ILogger _logger;
    private readonly List<CCValueRange> _ranges;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly string _description;

    // Track the last active range to avoid repeated actions
    private int? _lastActiveRangeIndex;
    private int? _lastValue;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// Creates a new instance of the CCRangeHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="ranges">The list of CC value ranges and their associated actions</param>
    /// <param name="description">Optional description of this handler</param>
    public CCRangeHandler(
        ILogger logger,
        List<CCValueRange> ranges,
        string? description = null)
    {
        _logger = logger;
        _ranges = ranges;
        _keyboardSimulator = new KeyboardSimulator();
        _description = description ?? "CC Range Mapping";
        _lastActiveRangeIndex = null;
        _lastValue = null;
    }

    /// <summary>
    /// Handles an absolute value from a MIDI control
    /// </summary>
    /// <param name="value">The value (0-127)</param>
    public void HandleValue(int value)
    {
        // Store the value for debugging
        _lastValue = value;

        // Find the range that contains this value
        int? rangeIndex = FindRangeIndex(value);

        // If no range found or same as last active range, do nothing
        if (rangeIndex == null || rangeIndex == _lastActiveRangeIndex)
        {
            return;
        }

        // We have a new active range
        _lastActiveRangeIndex = rangeIndex;
        var range = _ranges[(int)rangeIndex];

        _logger.LogInformation("CC value {Value} is in range {Min}-{Max}, executing action of type {ActionType}",
            value, range.MinValue, range.MaxValue, range.Action.Type);

        // Execute the action for this range
        ExecuteAction(range.Action);
    }

    /// <summary>
    /// Finds the index of the range that contains the given value
    /// </summary>
    /// <param name="value">The CC value to check</param>
    /// <returns>The index of the matching range, or null if no range matches</returns>
    private int? FindRangeIndex(int value)
    {
        for (int i = 0; i < _ranges.Count; i++)
        {
            var range = _ranges[i];
            if (value >= range.MinValue && value <= range.MaxValue)
            {
                return i;
            }
        }

        return null;
    }

    /// <summary>
    /// Executes the action for a CC value range
    /// </summary>
    /// <param name="action">The action to execute</param>
    private void ExecuteAction(CCRangeAction action)
    {
        try
        {
            switch (action.Type)
            {
                case CCRangeActionType.KeyPress:
                    ExecuteKeyPressAction(action);
                    break;

                case CCRangeActionType.CommandExecution:
                    ExecuteCommandAction(action);
                    break;



                default:
                    _logger.LogWarning("Unknown action type: {ActionType}", action.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing action for CC value {Value}", _lastValue);
        }
    }

    /// <summary>
    /// Executes a key press action
    /// </summary>
    /// <param name="action">The action to execute</param>
    private void ExecuteKeyPressAction(CCRangeAction action)
    {
        // Check if we have a virtual key code or a key string
        if (action.VirtualKeyCode.HasValue)
        {
            var modifiers = action.Modifiers ?? new List<ushort>();
            _logger.LogDebug("Pressing key with virtual key code {KeyCode}", action.VirtualKeyCode.Value);
            _keyboardSimulator.PressAndReleaseKey(action.VirtualKeyCode.Value, modifiers);
        }
        else if (!string.IsNullOrEmpty(action.Key))
        {
            _logger.LogDebug("Pressing key {Key}", action.Key);
            _keyboardSimulator.PressAndReleaseKey(action.Key[0]);
        }
        else
        {
            _logger.LogWarning("Key press action has no key specified");
        }
    }

    /// <summary>
    /// Executes a command action
    /// </summary>
    /// <param name="action">The action to execute</param>
    private void ExecuteCommandAction(CCRangeAction action)
    {
        if (string.IsNullOrEmpty(action.Command))
        {
            _logger.LogWarning("Command execution action has no command specified");
            return;
        }

        _logger.LogDebug("Executing command: {Command}", action.Command);

        try
        {
            // Determine which shell to use
            string shellPath = action.ShellType == CommandShellType.PowerShell
                ? "powershell.exe"
                : "cmd.exe";

            string arguments = action.ShellType == CommandShellType.PowerShell
                ? $"-Command \"{action.Command}\""
                : $"/c {action.Command}";

            var startInfo = new ProcessStartInfo
            {
                FileName = shellPath,
                Arguments = arguments,
                CreateNoWindow = action.RunHidden,
                UseShellExecute = false
            };

            var process = Process.Start(startInfo);

            if (action.WaitForExit && process != null)
            {
                process.WaitForExit();
                _logger.LogDebug("Command completed with exit code {ExitCode}", process.ExitCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing command: {Command}", action.Command);
        }
    }


}
