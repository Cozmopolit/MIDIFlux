using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Base class for keyboard actions
/// </summary>
public abstract class KeyboardAction : ActionBase
{
    /// <summary>
    /// The keyboard simulator to use
    /// </summary>
    protected readonly KeyboardSimulator KeyboardSimulator;

    /// <summary>
    /// The logger to use
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// The virtual key code to press/release
    /// </summary>
    protected readonly ushort VirtualKeyCode;

    /// <summary>
    /// Optional modifier keys to hold while pressing/releasing the main key
    /// </summary>
    protected readonly List<ushort> Modifiers;

    /// <summary>
    /// Creates a new instance of the KeyboardAction
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="virtualKeyCode">The virtual key code to press/release</param>
    /// <param name="modifiers">Optional modifier keys to hold</param>
    protected KeyboardAction(
        KeyboardSimulator keyboardSimulator,
        ILogger logger,
        ushort virtualKeyCode,
        List<ushort>? modifiers = null)
    {
        KeyboardSimulator = keyboardSimulator;
        Logger = logger;
        VirtualKeyCode = virtualKeyCode;
        Modifiers = modifiers ?? new List<ushort>();
    }

    /// <summary>
    /// Executes a key down action
    /// </summary>
    protected void ExecuteKeyDown()
    {
        try
        {
            // Press modifier keys
            foreach (var modifier in Modifiers)
            {
                if (!KeyboardSimulator.SendKeyDown(modifier))
                {
                    Logger.LogError("Failed to send key down for modifier {Modifier}", modifier);
                }
            }

            // Press the main key
            if (!KeyboardSimulator.SendKeyDown(VirtualKeyCode))
            {
                Logger.LogError("Failed to send key down for key {VirtualKeyCode}", VirtualKeyCode);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing key down action");
        }
    }

    /// <summary>
    /// Executes a key up action
    /// </summary>
    protected void ExecuteKeyUp()
    {
        try
        {
            // Release the main key
            if (!KeyboardSimulator.SendKeyUp(VirtualKeyCode))
            {
                Logger.LogError("Failed to send key up for key {VirtualKeyCode}", VirtualKeyCode);
            }

            // Release modifier keys in reverse order
            for (int i = Modifiers.Count - 1; i >= 0; i--)
            {
                if (!KeyboardSimulator.SendKeyUp(Modifiers[i]))
                {
                    Logger.LogError("Failed to send key up for modifier {Modifier}", Modifiers[i]);
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error executing key up action");
        }
    }
}
