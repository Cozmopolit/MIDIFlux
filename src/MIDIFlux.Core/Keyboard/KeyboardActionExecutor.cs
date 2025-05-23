using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Keyboard;

/// <summary>
/// Executes keyboard actions based on MIDI events
/// </summary>
public class KeyboardActionExecutor
{
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ILogger _logger;
    private readonly KeyStateManager _keyStateManager;

    /// <summary>
    /// Creates a new instance of the KeyboardActionExecutor
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyStateManager">The key state manager to use</param>
    public KeyboardActionExecutor(
        KeyboardSimulator keyboardSimulator,
        ILogger logger,
        KeyStateManager keyStateManager)
    {
        _keyboardSimulator = keyboardSimulator;
        _logger = logger;
        _keyStateManager = keyStateManager;
    }

    /// <summary>
    /// Executes a key down action
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key code to press</param>
    /// <param name="modifiers">Optional modifier keys to hold</param>
    public void ExecuteKeyDown(ushort virtualKeyCode, List<ushort> modifiers)
    {
        try
        {
            _logger.LogInformation("ExecuteKeyDown: VirtualKeyCode={VirtualKeyCode}, Modifiers={Modifiers}",
                virtualKeyCode, string.Join(",", modifiers));

            // Press modifier keys
            foreach (var modifier in modifiers)
            {
                _logger.LogDebug("Sending key down for modifier {Modifier}", modifier);
                bool result = _keyboardSimulator.SendKeyDown(modifier);
                if (!result)
                {
                    _logger.LogError("Failed to send key down for modifier {Modifier}", modifier);
                }
                else
                {
                    _logger.LogDebug("Successfully sent key down for modifier {Modifier}", modifier);
                }
            }

            // Press the main key
            _logger.LogDebug("Sending key down for key {VirtualKeyCode}", virtualKeyCode);
            bool keyResult = _keyboardSimulator.SendKeyDown(virtualKeyCode);
            if (!keyResult)
            {
                _logger.LogError("Failed to send key down for key {VirtualKeyCode}", virtualKeyCode);
            }
            else
            {
                _logger.LogInformation("Successfully sent key down for key {VirtualKeyCode}", virtualKeyCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing key down action for key {VirtualKeyCode}", virtualKeyCode);
        }
    }

    /// <summary>
    /// Executes a key up action
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key code to release</param>
    /// <param name="modifiers">Optional modifier keys to release</param>
    public void ExecuteKeyUp(ushort virtualKeyCode, List<ushort> modifiers)
    {
        try
        {
            // Release the main key
            if (!_keyboardSimulator.SendKeyUp(virtualKeyCode))
            {
                _logger.LogError("Failed to send key up for key {VirtualKeyCode}", virtualKeyCode);
            }

            // Release modifier keys in reverse order
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (!_keyboardSimulator.SendKeyUp(modifiers[i]))
                {
                    _logger.LogError("Failed to send key up for modifier {Modifier}", modifiers[i]);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing key up action");
        }
    }


}
