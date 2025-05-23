using System.Threading.Tasks;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Base class for all actions
/// </summary>
public abstract class ActionBase : IAction
{
    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public abstract string ActionType { get; }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public abstract Task ExecuteAsync();
}
