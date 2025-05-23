using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that toggles a key state
/// </summary>
public class KeyToggleAction : KeyboardAction
{
    private readonly KeyStateManager _keyStateManager;

    /// <summary>
    /// Creates a new instance of the KeyToggleAction
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyStateManager">The key state manager to use</param>
    /// <param name="virtualKeyCode">The virtual key code to toggle</param>
    /// <param name="modifiers">Optional modifier keys</param>
    public KeyToggleAction(
        KeyboardSimulator keyboardSimulator,
        ILogger logger,
        KeyStateManager keyStateManager,
        ushort virtualKeyCode,
        List<ushort>? modifiers = null)
        : base(keyboardSimulator, logger, virtualKeyCode, modifiers)
    {
        _keyStateManager = keyStateManager;
    }

    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public override string Description => $"Toggle key {VirtualKeyCode}" +
        (Modifiers.Count > 0 ? $" with modifiers {string.Join(", ", Modifiers)}" : "");

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.KeyToggle);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override Task ExecuteAsync()
    {
        Logger.LogDebug("Executing key toggle: {VirtualKeyCode}", VirtualKeyCode);

        _keyStateManager.ToggleKey(VirtualKeyCode, Modifiers);

        return Task.CompletedTask;
    }
}
