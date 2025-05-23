using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Factory for creating actions from configuration
/// </summary>
public class ActionFactory
{
    private readonly ILogger _logger;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly KeyStateManager _keyStateManager;

    /// <summary>
    /// Creates a new instance of the ActionFactory
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="keyStateManager">The key state manager to use</param>
    public ActionFactory(
        ILogger<ActionFactory> logger,
        KeyboardSimulator keyboardSimulator,
        KeyStateManager keyStateManager)
    {
        _logger = logger;
        _keyboardSimulator = keyboardSimulator;
        _keyStateManager = keyStateManager;
    }

    /// <summary>
    /// Creates an action from the specified parameters
    /// </summary>
    /// <param name="actionType">The type of action to create</param>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action, or null if the action could not be created</returns>
    public IAction? CreateAction(ActionType actionType, Dictionary<string, object> parameters)
    {
        try
        {
            return actionType switch
            {
                ActionType.KeyPressRelease => CreateKeyPressReleaseAction(parameters),
                ActionType.KeyDown => CreateKeyDownAction(parameters),
                ActionType.KeyUp => CreateKeyUpAction(parameters),
                ActionType.KeyToggle => CreateKeyToggleAction(parameters),
                ActionType.CommandExecution => CreateCommandExecutionAction(parameters),
                ActionType.Delay => CreateDelayAction(parameters),
                ActionType.Macro => CreateMacroAction(parameters),
                _ => null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating action of type {ActionType}", actionType);
            return null;
        }
    }

    /// <summary>
    /// Creates a key press and release action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateKeyPressReleaseAction(Dictionary<string, object> parameters)
    {
        ushort virtualKeyCode = GetVirtualKeyCode(parameters);
        List<ushort> modifiers = GetModifiers(parameters);

        return new KeyPressReleaseAction(_keyboardSimulator, _logger, virtualKeyCode, modifiers);
    }

    /// <summary>
    /// Creates a key down action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateKeyDownAction(Dictionary<string, object> parameters)
    {
        ushort virtualKeyCode = GetVirtualKeyCode(parameters);
        List<ushort> modifiers = GetModifiers(parameters);

        return new KeyDownAction(_keyboardSimulator, _logger, virtualKeyCode, modifiers);
    }

    /// <summary>
    /// Creates a key up action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateKeyUpAction(Dictionary<string, object> parameters)
    {
        ushort virtualKeyCode = GetVirtualKeyCode(parameters);
        List<ushort> modifiers = GetModifiers(parameters);

        return new KeyUpAction(_keyboardSimulator, _logger, virtualKeyCode, modifiers);
    }

    /// <summary>
    /// Creates a key toggle action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateKeyToggleAction(Dictionary<string, object> parameters)
    {
        ushort virtualKeyCode = GetVirtualKeyCode(parameters);
        List<ushort> modifiers = GetModifiers(parameters);

        return new KeyToggleAction(_keyboardSimulator, _logger, _keyStateManager, virtualKeyCode, modifiers);
    }

    /// <summary>
    /// Creates a command execution action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateCommandExecutionAction(Dictionary<string, object> parameters)
    {
        string command = parameters.TryGetValue("command", out var commandObj) && commandObj is string cmd ? cmd : string.Empty;
        CommandShellType shellType = parameters.TryGetValue("shellType", out var shellTypeObj) && shellTypeObj is CommandShellType type ? type : CommandShellType.PowerShell;
        bool runHidden = parameters.TryGetValue("runHidden", out var runHiddenObj) && runHiddenObj is bool hidden && hidden;
        bool waitForExit = parameters.TryGetValue("waitForExit", out var waitForExitObj) && waitForExitObj is bool wait && wait;
        string? description = parameters.TryGetValue("description", out var descriptionObj) && descriptionObj is string desc ? desc : null;

        return new CommandExecutionAction(_logger, command, shellType, runHidden, waitForExit, description);
    }

    /// <summary>
    /// Creates a delay action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateDelayAction(Dictionary<string, object> parameters)
    {
        int milliseconds = parameters.TryGetValue("milliseconds", out var millisecondsObj) && millisecondsObj is int ms ? ms : 0;

        return new DelayAction(_logger, milliseconds);
    }

    /// <summary>
    /// Creates a macro action
    /// </summary>
    /// <param name="parameters">The parameters for the action</param>
    /// <returns>The created action</returns>
    private IAction CreateMacroAction(Dictionary<string, object> parameters)
    {
        List<IAction> actions = new();

        if (parameters.TryGetValue("actions", out var actionsObj) && actionsObj is List<Dictionary<string, object>> actionsList)
        {
            foreach (var actionParams in actionsList)
            {
                if (actionParams.TryGetValue("type", out var typeObj) && typeObj is string typeStr &&
                    Enum.TryParse<ActionType>(typeStr, out var type))
                {
                    var action = CreateAction(type, actionParams);
                    if (action != null)
                    {
                        actions.Add(action);
                    }
                }
            }
        }

        string? description = parameters.TryGetValue("description", out var descriptionObj) && descriptionObj is string desc ? desc : null;

        return new MacroAction(_logger, actions, description);
    }

    /// <summary>
    /// Gets the virtual key code from the parameters
    /// </summary>
    /// <param name="parameters">The parameters</param>
    /// <returns>The virtual key code</returns>
    private static ushort GetVirtualKeyCode(Dictionary<string, object> parameters)
    {
        return parameters.TryGetValue("virtualKeyCode", out var keyCodeObj) && keyCodeObj is ushort keyCode ? keyCode : (ushort)0;
    }

    /// <summary>
    /// Gets the modifiers from the parameters
    /// </summary>
    /// <param name="parameters">The parameters</param>
    /// <returns>The modifiers</returns>
    private static List<ushort> GetModifiers(Dictionary<string, object> parameters)
    {
        if (parameters.TryGetValue("modifiers", out var modifiersObj) && modifiersObj is List<ushort> modifiers)
        {
            return modifiers;
        }

        return new List<ushort>();
    }
}
