using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Mouse;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for clicking a mouse button (Left, Right, Middle).
/// Implements sync-by-default execution for performance.
/// </summary>
public class MouseClickAction : IAction
{
    private readonly MouseButton _button;
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
    /// Gets the mouse button for this action
    /// </summary>
    public MouseButton Button => _button;

    /// <summary>
    /// Initializes a new instance of MouseClickAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public MouseClickAction(MouseClickConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "MouseClickConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid MouseClickConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Click {config.Button} Mouse Button";
        _button = config.Button;

        // Initialize mouse simulator and logger
        _logger = LoggingHelper.CreateLogger<MouseClickAction>();
        _mouseSimulator = new MouseSimulator(_logger);
    }

    /// <summary>
    /// Executes the mouse click action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing MouseClickAction: Button={Button}, MidiValue={MidiValue}",
                _button, midiValue);

            // Perform the mouse click
            if (!_mouseSimulator.SendMouseClick(_button))
            {
                var errorMsg = $"Failed to send mouse click for button {_button}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", _logger);
                return;
            }

            _logger.LogTrace("Successfully executed MouseClickAction for Button={Button}", _button);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing MouseClickAction for button {_button}";
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
