using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Action that waits for a specified time
/// </summary>
public class DelayAction : ActionBase
{
    private readonly ILogger _logger;
    private readonly int _milliseconds;

    /// <summary>
    /// Creates a new instance of the DelayAction
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="milliseconds">The number of milliseconds to wait</param>
    public DelayAction(ILogger logger, int milliseconds)
    {
        _logger = logger;
        _milliseconds = Math.Max(0, milliseconds);
    }

    /// <summary>
    /// Gets a description of this action for UI and logging
    /// </summary>
    public override string Description => $"Wait for {_milliseconds} ms";

    /// <summary>
    /// Gets the type of this action
    /// </summary>
    public override string ActionType => nameof(Models.ActionType.Delay);

    /// <summary>
    /// Executes the action
    /// </summary>
    /// <returns>A task that completes when the action is finished</returns>
    public override async Task ExecuteAsync()
    {
        if (_milliseconds <= 0)
        {
            return;
        }

        _logger.LogDebug("Delaying for {Delay}ms", _milliseconds);
        await Task.Delay(_milliseconds);
    }
}
