using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for toggling the state of a key (like CapsLock, NumLock, etc.).
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyToggleAction : ActionBase<KeyToggleConfig>
{
    private readonly ushort _virtualKeyCode;
    private readonly KeyboardSimulator _keyboardSimulator;

    /// <summary>
    /// Gets the virtual key code for this action
    /// </summary>
    public ushort VirtualKeyCode => _virtualKeyCode;

    /// <summary>
    /// Initializes a new instance of KeyToggleAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional state manager (not used by this action)</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyToggleAction(KeyToggleConfig config, ActionStateManager? actionStateManager = null) : base(config)
    {
        _virtualKeyCode = config.VirtualKeyCode;

        // Initialize keyboard simulator
        _keyboardSimulator = new KeyboardSimulator(Logger);
    }

    /// <summary>
    /// Core execution logic for the key toggle action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // For toggle keys, we need to press and release to toggle the state
        // This works for keys like CapsLock, NumLock, ScrollLock, etc.
        if (!_keyboardSimulator.SendKeyDown(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key down for toggle key virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key up for toggle key virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Toggle Key (VK: {_virtualKeyCode})";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing KeyToggleAction for virtual key code {_virtualKeyCode}";
    }
}
