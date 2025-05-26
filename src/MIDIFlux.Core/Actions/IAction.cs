using System.Threading.Tasks;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Unified interface for all actions in the MIDIFlux system.
/// Provides async-first execution for consistent behavior and maintainable architecture.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action for UI and logging
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Executes the action asynchronously.
    /// All actions implement this unified execution model for consistent behavior.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    ValueTask ExecuteAsync(int? midiValue = null);
}
