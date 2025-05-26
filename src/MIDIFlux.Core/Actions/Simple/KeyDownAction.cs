using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for pressing a key down (and optionally auto-releasing it).
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyDownAction : ActionBase<KeyDownConfig>
{
    private readonly ushort _virtualKeyCode;
    private readonly int? _autoReleaseAfterMs;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ActionStateManager? _actionStateManager;
    private readonly string _stateKey;

    /// <summary>
    /// Gets the virtual key code for this action
    /// </summary>
    public ushort VirtualKeyCode => _virtualKeyCode;

    /// <summary>
    /// Gets the auto-release time in milliseconds, if configured
    /// </summary>
    public int? AutoReleaseAfterMs => _autoReleaseAfterMs;

    /// <summary>
    /// Initializes a new instance of KeyDownAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional action state manager for state tracking</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyDownAction(KeyDownConfig config, ActionStateManager? actionStateManager = null) : base(config)
    {
        _virtualKeyCode = config.VirtualKeyCode;
        _autoReleaseAfterMs = config.AutoReleaseAfterMs;
        _actionStateManager = actionStateManager;
        _stateKey = $"*Key{config.VirtualKeyCode}";

        // Initialize keyboard simulator
        _keyboardSimulator = new KeyboardSimulator(Logger);
    }

    /// <summary>
    /// Core execution logic for the key down action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Check current state to avoid duplicate key presses
        if (_actionStateManager != null)
        {
            var currentState = _actionStateManager.GetState(_stateKey);
            if (currentState == 1)
            {
                Logger.LogDebug("Key {VirtualKeyCode} is already pressed (state={State}), skipping key down", _virtualKeyCode, currentState);
                return ValueTask.CompletedTask;
            }
        }

        // Press the key down
        if (!_keyboardSimulator.SendKeyDown(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key down for virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Update state to pressed
        _actionStateManager?.SetState(_stateKey, 1);

        Logger.LogTrace("Successfully sent key down for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);

        // Handle auto-release if specified
        if (_autoReleaseAfterMs.HasValue && _autoReleaseAfterMs.Value > 0)
        {
            // Schedule auto-release on a background thread to avoid blocking
            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_autoReleaseAfterMs.Value);

                    if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
                    {
                        Logger.LogError("Failed to auto-release key for virtual key code {VirtualKeyCode}", _virtualKeyCode);
                    }
                    else
                    {
                        // Update state to released
                        _actionStateManager?.SetState(_stateKey, 0);
                        Logger.LogTrace("Successfully auto-released key for VirtualKeyCode={VirtualKeyCode} after {Delay}ms",
                            _virtualKeyCode, _autoReleaseAfterMs.Value);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError(ex, "Error during auto-release for virtual key code {VirtualKeyCode}", _virtualKeyCode);
                }
            });
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var autoReleaseText = _autoReleaseAfterMs.HasValue ? $" (auto-release after {_autoReleaseAfterMs}ms)" : "";
        return $"Press Key Down (VK: {_virtualKeyCode}){autoReleaseText}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing KeyDownAction for virtual key code {_virtualKeyCode}";
    }
}
