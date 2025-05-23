using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Keyboard;

/// <summary>
/// Manages the state of toggled keys
/// </summary>
public class KeyStateManager
{
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ILogger _logger;
    private readonly ConcurrentDictionary<ushort, bool> _keyStates = new();

    /// <summary>
    /// Creates a new instance of the KeyStateManager
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    public KeyStateManager(KeyboardSimulator keyboardSimulator, ILogger<KeyStateManager> logger)
    {
        _keyboardSimulator = keyboardSimulator;
        _logger = logger;
    }

    /// <summary>
    /// Toggles the state of a key
    /// </summary>
    /// <param name="keyCode">The virtual key code to toggle</param>
    /// <param name="modifiers">Optional modifier keys</param>
    /// <returns>The new state of the key (true = pressed, false = released)</returns>
    public bool ToggleKey(ushort keyCode, List<ushort> modifiers)
    {
        // Get the current state or default to released (false)
        bool currentState = _keyStates.GetValueOrDefault(keyCode, false);

        // Toggle the state
        bool newState = !currentState;
        _keyStates[keyCode] = newState;

        // Execute the appropriate key action
        if (newState)
        {
            // Press the key
            ExecuteKeyDown(keyCode, modifiers);
            _logger.LogInformation("Toggled key {KeyCode} to pressed state", keyCode);
        }
        else
        {
            // Release the key
            ExecuteKeyUp(keyCode, modifiers);
            _logger.LogInformation("Toggled key {KeyCode} to released state", keyCode);
        }

        return newState;
    }

    /// <summary>
    /// Releases all toggled keys
    /// </summary>
    public void ReleaseAllKeys()
    {
        _logger.LogInformation("Releasing all toggled keys");

        foreach (var keyEntry in _keyStates.Where(k => k.Value))
        {
            ExecuteKeyUp(keyEntry.Key, new List<ushort>());
            _logger.LogDebug("Released toggled key {KeyCode}", keyEntry.Key);
        }

        // Clear the state dictionary
        _keyStates.Clear();
    }

    /// <summary>
    /// Executes a key down action
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key code to press</param>
    /// <param name="modifiers">Optional modifier keys to hold</param>
    private void ExecuteKeyDown(ushort virtualKeyCode, List<ushort> modifiers)
    {
        try
        {
            // Press modifier keys
            foreach (var modifier in modifiers)
            {
                if (!_keyboardSimulator.SendKeyDown(modifier))
                {
                    _logger.LogError("Failed to send key down for modifier {Modifier}", modifier);
                }
            }

            // Press the main key
            if (!_keyboardSimulator.SendKeyDown(virtualKeyCode))
            {
                _logger.LogError("Failed to send key down for key {VirtualKeyCode}", virtualKeyCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing key down action");
        }
    }

    /// <summary>
    /// Executes a key up action
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key code to release</param>
    /// <param name="modifiers">Optional modifier keys to release</param>
    private void ExecuteKeyUp(ushort virtualKeyCode, List<ushort> modifiers)
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
