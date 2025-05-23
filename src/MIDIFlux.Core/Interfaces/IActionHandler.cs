using System.Collections.Generic;
using System.Threading.Tasks;

namespace MIDIFlux.Core.Interfaces;

/// <summary>
/// Base interface for all action handlers
/// </summary>
public interface IActionHandler
{
    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Gets the type of this handler
    /// </summary>
    string HandlerType { get; }

    /// <summary>
    /// Executes the handler with the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>A task that completes when the handler is finished</returns>
    Task ExecuteAsync(Dictionary<string, object> parameters);
}
