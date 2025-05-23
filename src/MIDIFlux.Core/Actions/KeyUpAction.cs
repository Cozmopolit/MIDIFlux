using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that releases a key that was previously pressed
/// </summary>
public class KeyUpAction : KeyboardAction
{
    /// <summary>
    /// Creates a new instance of the KeyUpAction
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="virtualKeyCode">The virtual key code to release</param>
    /// <param name="modifiers">Optional modifier keys to release</param>
    public KeyUpAction(
        KeyboardSimulator keyboardSimulator,
        ILogger logger,
        ushort virtualKeyCode,
        List<ushort>? modifiers = null)
        : base(keyboardSimulator, logger, virtualKeyCode, modifiers)
    {
    }

    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public override string Description => $"Release key {VirtualKeyCode}" +
        (Modifiers.Count > 0 ? $" with modifiers {string.Join(", ", Modifiers)}" : "");

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.KeyUp);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override Task ExecuteAsync()
    {
        Logger.LogDebug("Executing key up: {VirtualKeyCode}", VirtualKeyCode);

        ExecuteKeyUp();

        return Task.CompletedTask;
    }
}
