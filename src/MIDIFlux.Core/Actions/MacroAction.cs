using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that executes a sequence of actions
/// </summary>
public class MacroAction : ActionBase
{
    private readonly ILogger _logger;
    private readonly List<IAction> _actions;
    private readonly string _description;

    /// <summary>
    /// Creates a new instance of the MacroAction
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="actions">The sequence of actions to execute</param>
    /// <param name="description">Optional description of this macro</param>
    public MacroAction(
        ILogger logger,
        List<IAction> actions,
        string? description = null)
    {
        _logger = logger;
        _actions = actions;
        _description = description ?? $"Macro with {actions.Count} actions";
    }

    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public override string Description => _description;

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.Macro);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override async Task ExecuteAsync()
    {
        _logger.LogDebug("Executing macro with {Count} actions", _actions.Count);

        foreach (var action in _actions)
        {
            _logger.LogDebug("Executing action: {ActionType} - {Description}", 
                action.ActionType, action.Description);
            
            await action.ExecuteAsync();
        }
    }
}
