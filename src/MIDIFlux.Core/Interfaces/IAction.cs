using System.Threading.Tasks;

namespace MIDIFlux.Core.Interfaces;

/// <summary>
/// Represents an action that can be executed as part of a macro
/// </summary>
public interface IAction
{
    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    Task ExecuteAsync();

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    string ActionType { get; }
}
