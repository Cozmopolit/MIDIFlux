using System;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Interfaces;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.GameController.Handlers;

/// <summary>
/// Handles mapping MIDI notes to game controller buttons
/// </summary>
public class GameControllerButtonHandler : GameControllerBase, INoteHandler
{
    private readonly Xbox360Button? _button;
    private readonly string _buttonName;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => $"Game Controller {ControllerIndex} Button: {_buttonName}";

    /// <summary>
    /// Creates a new instance of the GameControllerButtonHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="buttonName">The name of the button to emulate</param>
    /// <param name="controllerIndex">The controller index (0-3)</param>
    public GameControllerButtonHandler(ILogger logger, string buttonName, int controllerIndex = 0) : base(logger, controllerIndex)
    {
        _buttonName = buttonName;
        _button = MapButtonName(buttonName);

        if (_button == null)
        {
            _logger.LogWarning("Invalid button name: {ButtonName}. Button will not work.", buttonName);
        }
    }

    /// <summary>
    /// Handles a note on event
    /// </summary>
    /// <param name="velocity">The note velocity (0-127)</param>
    public void HandleNoteOn(int velocity)
    {
        if (!IsViGEmAvailable || _button == null || _controller == null)
        {
            _logger.LogDebug("Cannot press button {ButtonName}: ViGEm not available or button invalid", _buttonName);
            return;
        }

        try
        {
            // Log detailed information about the button
            _logger.LogDebug("Attempting to press button {ButtonName} (enum value: {ButtonValue})",
                _buttonName, (int)_button.Value);

            // Use the mapped button directly
            _controller.SetButtonState(_button.Value, true);
            _logger.LogDebug("Pressed game controller button: {ButtonName}", _buttonName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to press button {ButtonName}: {Message}", _buttonName, ex.Message);
        }
    }

    /// <summary>
    /// Handles a note off event
    /// </summary>
    public void HandleNoteOff()
    {
        if (!IsViGEmAvailable || _button == null || _controller == null)
        {
            _logger.LogDebug("Cannot release button {ButtonName}: ViGEm not available or button invalid", _buttonName);
            return;
        }

        try
        {
            // Log detailed information about the button
            _logger.LogDebug("Attempting to release button {ButtonName} (enum value: {ButtonValue})",
                _buttonName, (int)_button.Value);

            // Use the mapped button directly
            _controller.SetButtonState(_button.Value, false);
            _logger.LogDebug("Released game controller button: {ButtonName}", _buttonName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to release button {ButtonName}: {Message}", _buttonName, ex.Message);
        }
    }
}
