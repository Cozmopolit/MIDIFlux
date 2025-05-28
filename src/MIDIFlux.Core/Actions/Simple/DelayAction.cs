using MIDIFlux.Core.Actions.Configuration;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for waiting/delaying for a specified time.
/// Implements true async behavior with Task.Delay.
/// </summary>
public class DelayAction : ActionBase<DelayConfig>
{
    private readonly int _milliseconds;

    /// <summary>
    /// Gets the delay duration in milliseconds
    /// </summary>
    public int Milliseconds => _milliseconds;

    /// <summary>
    /// Initializes a new instance of DelayAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public DelayAction(DelayConfig config) : base(config)
    {
        _milliseconds = config.Milliseconds;
    }

    /// <summary>
    /// Core async execution logic for the delay action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes after the specified delay</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (_milliseconds <= 0)
        {
            Logger.LogDebug("DelayAction: Skipping delay because milliseconds is {Milliseconds}", _milliseconds);
            return;
        }

        await Task.Delay(_milliseconds);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Wait for {_milliseconds} ms";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing DelayAction for {_milliseconds}ms";
    }
}
