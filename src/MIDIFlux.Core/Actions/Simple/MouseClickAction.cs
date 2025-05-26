using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Mouse;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for clicking a mouse button (Left, Right, Middle).
/// Implements sync-by-default execution for performance.
/// </summary>
public class MouseClickAction : ActionBase<MouseClickConfig>
{
    private readonly MouseButton _button;
    private readonly MouseSimulator _mouseSimulator;

    /// <summary>
    /// Gets the mouse button for this action
    /// </summary>
    public MouseButton Button => _button;

    /// <summary>
    /// Initializes a new instance of MouseClickAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public MouseClickAction(MouseClickConfig config) : base(config)
    {
        _button = config.Button;

        // Initialize mouse simulator
        var mouseLogger = LoggingHelper.CreateLogger<MouseSimulator>();
        _mouseSimulator = new MouseSimulator(mouseLogger);
    }

    /// <summary>
    /// Core execution logic for the mouse click action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Perform the mouse click
        if (!_mouseSimulator.SendMouseClick(_button))
        {
            var errorMsg = $"Failed to send mouse click for button {_button}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Click {_button} Mouse Button";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing MouseClickAction for button {_button}";
    }
}
