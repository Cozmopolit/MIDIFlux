using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for waiting/delaying for a specified time.
/// Overrides ExecuteAsync for true async behavior with Task.Delay.
/// </summary>
public class DelayAction : IAction
{
    private readonly int _milliseconds;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

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
    public DelayAction(DelayConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "DelayConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid DelayConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Wait for {config.Milliseconds} ms";
        _milliseconds = config.Milliseconds;

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<DelayAction>();
    }

    /// <summary>
    /// Executes the delay action synchronously (no-op for delays).
    /// For delay actions, the async version should be used instead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        // For delay actions, we don't block the synchronous path
        // The actual delay should be handled by ExecuteAsync()
        // This allows the caller to choose between sync (no delay) and async (with delay)
    }

    /// <summary>
    /// Executes the delay action asynchronously with actual delay.
    /// This is the proper implementation for DelayAction that uses await Task.Delay().
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes after the specified delay</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        try
        {
            if (_milliseconds <= 0)
            {
                _logger.LogDebug("DelayAction: Skipping delay because milliseconds is {Milliseconds}", _milliseconds);
                return;
            }

            _logger.LogDebug("Executing DelayAction: Delaying for {Delay}ms, MidiValue={MidiValue}",
                _milliseconds, midiValue);

            await Task.Delay(_milliseconds);

            _logger.LogTrace("Successfully completed DelayAction for {Delay}ms", _milliseconds);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing DelayAction for {_milliseconds}ms";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
