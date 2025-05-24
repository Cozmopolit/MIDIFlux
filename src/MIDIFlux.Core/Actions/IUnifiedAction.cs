using System.Threading.Tasks;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Unified interface for all actions in the MIDIFlux system.
/// Provides sync-by-default execution for performance with async adapter support.
/// </summary>
public interface IUnifiedAction
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
    /// Executes the action synchronously (hot path - no Task overhead).
    /// Most actions are synchronous and should implement this method directly.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    void Execute(int? midiValue = null);

    /// <summary>
    /// Executes the action asynchronously.
    /// Default implementation wraps the synchronous Execute method.
    /// Complex actions can override this for true async behavior.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
    }
}
