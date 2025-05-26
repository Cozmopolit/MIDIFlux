using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Mouse;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for scrolling the mouse wheel in a specified direction.
/// Implements sync-by-default execution for performance.
/// </summary>
public class MouseScrollAction : ActionBase<MouseScrollConfig>
{
    private readonly ScrollDirection _direction;
    private readonly int _amount;
    private readonly MouseSimulator _mouseSimulator;

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
    public MouseScrollAction(MouseScrollConfig config) : base(config)
    {
        _direction = config.Direction;
        _amount = config.Amount;

        // Initialize mouse simulator
        var mouseLogger = LoggingHelper.CreateLogger<MouseSimulator>();
        _mouseSimulator = new MouseSimulator(mouseLogger);
    }

    /// <summary>
    /// Core execution logic for the mouse scroll action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Perform the mouse scroll
        if (!_mouseSimulator.SendMouseScroll(_direction, _amount))
        {
            var errorMsg = $"Failed to send mouse scroll for direction {_direction} with amount {_amount}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("Successfully executed MouseScrollAction for Direction={Direction}, Amount={Amount}",
            _direction, _amount);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var amountText = _amount == 1 ? "" : $" ({_amount} steps)";
        return $"Scroll {_direction}{amountText}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing MouseScrollAction for direction {_direction} with amount {_amount}";
    }
}
