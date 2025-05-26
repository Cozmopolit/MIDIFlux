using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for releasing a key that was previously pressed down.
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyUpAction : ActionBase<KeyUpConfig>
{
    private readonly ushort _virtualKeyCode;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ActionStateManager? _actionStateManager;
    private readonly string _stateKey;

    /// <summary>
    /// Gets the virtual key code for this action
    /// </summary>
    public ushort VirtualKeyCode => _virtualKeyCode;

    /// <summary>
    /// Initializes a new instance of KeyUpAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional action state manager for state tracking</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyUpAction(KeyUpConfig config, ActionStateManager? actionStateManager = null) : base(config)
    {
        _virtualKeyCode = config.VirtualKeyCode;
        _actionStateManager = actionStateManager;
        _stateKey = $"*Key{config.VirtualKeyCode}";

        // Initialize keyboard simulator
        _keyboardSimulator = new KeyboardSimulator(Logger);
    }

    /// <summary>
    /// Core execution logic for the key release action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Check current state - only release if key is currently pressed
        if (_actionStateManager != null)
        {
            var currentState = _actionStateManager.GetState(_stateKey);
            if (currentState != 1)
            {
                Logger.LogDebug("Key {VirtualKeyCode} is not pressed (state={State}), skipping key up", _virtualKeyCode, currentState);
                return ValueTask.CompletedTask;
            }
        }

        // Release the key
        if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key up for virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Update state to released
        _actionStateManager?.SetState(_stateKey, 0);

        Logger.LogTrace("Successfully executed KeyUpAction for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Release Key (VK: {_virtualKeyCode})";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing KeyUpAction for virtual key code {_virtualKeyCode}";
    }
}
