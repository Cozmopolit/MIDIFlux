using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for pressing and releasing a key.
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyPressReleaseAction : ActionBase<KeyPressReleaseConfig>
{
    private readonly ushort _virtualKeyCode;
    private readonly KeyboardSimulator _keyboardSimulator;

    /// <summary>
    /// Gets the virtual key code for this action
    /// </summary>
    public ushort VirtualKeyCode => _virtualKeyCode;

    /// <summary>
    /// Initializes a new instance of KeyPressReleaseAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional action state manager (not used by this action)</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyPressReleaseAction(KeyPressReleaseConfig config, ActionStateManager? actionStateManager = null) : base(config)
    {
        _virtualKeyCode = config.VirtualKeyCode;

        // Initialize keyboard simulator
        _keyboardSimulator = new KeyboardSimulator(Logger);
    }

    /// <summary>
    /// Core execution logic for the key press and release action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Press the key down
        if (!_keyboardSimulator.SendKeyDown(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key down for virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Release the key
        if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
        {
            var errorMsg = $"Failed to send key up for virtual key code {_virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("Successfully executed KeyPressReleaseAction for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Press/Release Key (VK: {_virtualKeyCode})";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing KeyPressReleaseAction for virtual key code {_virtualKeyCode}";
    }
}
