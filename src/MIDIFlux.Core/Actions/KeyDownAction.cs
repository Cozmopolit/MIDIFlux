using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that presses a key down but doesn't release it
/// </summary>
public class KeyDownAction : KeyboardAction
{
    /// <summary>
    /// Creates a new instance of the KeyDownAction
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="virtualKeyCode">The virtual key code to press</param>
    /// <param name="modifiers">Optional modifier keys to hold</param>
    public KeyDownAction(
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
    public override string Description => $"Press key {VirtualKeyCode}" +
        (Modifiers.Count > 0 ? $" with modifiers {string.Join(", ", Modifiers)}" : "");

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.KeyDown);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override Task ExecuteAsync()
    {
        Logger.LogDebug("Executing key down: {VirtualKeyCode}", VirtualKeyCode);

        ExecuteKeyDown();

        return Task.CompletedTask;
    }
}
