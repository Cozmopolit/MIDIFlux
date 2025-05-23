using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Interfaces;

namespace MIDIFlux.Core.Handlers;

/// <summary>
/// Handles executing a macro (sequence of actions)
/// </summary>
public class MacroHandler : INoteHandler, IActionHandler
{
    private readonly ILogger _logger;
    private readonly IAction _macro;
    private readonly string _description;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => _description;

    /// <summary>
    /// Gets the type of this handler
    /// </summary>
    public string HandlerType => "Macro";

    /// <summary>
    /// Creates a new instance of the MacroHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="macro">The macro action to execute</param>
    /// <param name="description">Optional description of this handler</param>
    public MacroHandler(
        ILogger logger,
        IAction macro,
        string? description = null)
    {
        _logger = logger;
        _macro = macro;
        _description = description ?? $"Macro: {macro.Description}";
    }

    /// <summary>
    /// Handles a note on event by executing the macro
    /// </summary>
    /// <param name="velocity">The note velocity (0-127)</param>
    public void HandleNoteOn(int velocity)
    {
        _logger.LogDebug("Note On event received for macro: {Description}", _description);
        
        // Execute the macro asynchronously
        _ = _macro.ExecuteAsync();
    }

    /// <summary>
    /// Handles a note off event (does nothing for macros)
    /// </summary>
    public void HandleNoteOff()
    {
        // Do nothing for note off events
        _logger.LogDebug("Note Off event ignored for macro: {Description}", _description);
    }

    /// <summary>
    /// Executes the handler with the specified parameters
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>A task that completes when the handler is finished</returns>
    public Task ExecuteAsync(Dictionary<string, object> parameters)
    {
        _logger.LogDebug("Executing macro: {Description}", _description);
        
        return _macro.ExecuteAsync();
    }
}
