using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Mouse;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Unified action for scrolling the mouse wheel in a specified direction.
/// Implements sync-by-default execution for performance.
/// </summary>
public class MouseScrollAction : IUnifiedAction
{
    private readonly ScrollDirection _direction;
    private readonly int _amount;
    private readonly MouseSimulator _mouseSimulator;
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
    /// Gets the scroll direction for this action
    /// </summary>
    public ScrollDirection Direction => _direction;

    /// <summary>
    /// Gets the scroll amount for this action
    /// </summary>
    public int Amount => _amount;

    /// <summary>
    /// Initializes a new instance of MouseScrollAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public MouseScrollAction(MouseScrollConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "MouseScrollConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid MouseScrollConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        var amountText = config.Amount == 1 ? "" : $" ({config.Amount} steps)";
        Description = config.Description ?? $"Scroll {config.Direction}{amountText}";
        _direction = config.Direction;
        _amount = config.Amount;

        // Initialize mouse simulator and logger
        _logger = LoggingHelper.CreateLogger<MouseScrollAction>();
        _mouseSimulator = new MouseSimulator(_logger);
    }

    /// <summary>
    /// Executes the mouse scroll action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing MouseScrollAction: Direction={Direction}, Amount={Amount}, MidiValue={MidiValue}",
                _direction, _amount, midiValue);

            // Perform the mouse scroll
            if (!_mouseSimulator.SendMouseScroll(_direction, _amount))
            {
                var errorMsg = $"Failed to send mouse scroll for direction {_direction} with amount {_amount}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", _logger);
                return;
            }

            _logger.LogTrace("Successfully executed MouseScrollAction for Direction={Direction}, Amount={Amount}",
                _direction, _amount);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing MouseScrollAction for direction {_direction} with amount {_amount}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Async adapter for the synchronous Execute method.
    /// Uses ValueTask for zero allocation when the operation is synchronous.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A completed ValueTask</returns>
    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
