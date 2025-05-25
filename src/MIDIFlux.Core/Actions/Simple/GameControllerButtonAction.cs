using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for pressing a game controller button.
/// Implements sync-by-default execution for performance.
/// Uses existing ViGEm integration in GameController directory.
/// </summary>
public class GameControllerButtonAction : IAction
{
    private readonly string _button;
    private readonly int _controllerIndex;
    private readonly GameControllerManager _controllerManager;
    private readonly ILogger _logger;
    private readonly Xbox360Button? _mappedButton;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the button name for this action
    /// </summary>
    public string Button => _button;

    /// <summary>
    /// Gets the controller index for this action
    /// </summary>
    public int ControllerIndex => _controllerIndex;

    /// <summary>
    /// Initializes a new instance of GameControllerButtonAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public GameControllerButtonAction(GameControllerButtonConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "GameControllerButtonConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid GameControllerButtonConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Controller {config.ControllerIndex + 1} Button {config.Button}";
        _button = config.Button;
        _controllerIndex = config.ControllerIndex;

        // Initialize logger and game controller manager
        _logger = LoggingHelper.CreateLogger<GameControllerButtonAction>();
        _controllerManager = GameControllerManager.GetInstance(_logger);
        _mappedButton = MapButtonName(config.Button);

        if (_mappedButton == null)
        {
            _logger.LogWarning("Invalid button name: {ButtonName}. Button will not work.", config.Button);
        }
    }

    /// <summary>
    /// Maps a button name to an Xbox 360 button
    /// </summary>
    /// <param name="buttonName">The button name</param>
    /// <returns>The Xbox 360 button, or null if the name is invalid</returns>
    private Xbox360Button? MapButtonName(string buttonName)
    {
        // Button mappings (case-insensitive)
        var buttonMappings = new Dictionary<string, Xbox360Button>(StringComparer.OrdinalIgnoreCase)
        {
            { "a", Xbox360Button.A },
            { "b", Xbox360Button.B },
            { "x", Xbox360Button.X },
            { "y", Xbox360Button.Y },
            { "leftshoulder", Xbox360Button.LeftShoulder },
            { "rightshoulder", Xbox360Button.RightShoulder },
            { "back", Xbox360Button.Back },
            { "start", Xbox360Button.Start },
            { "leftthumb", Xbox360Button.LeftThumb },
            { "rightthumb", Xbox360Button.RightThumb },
            { "up", Xbox360Button.Up },
            { "down", Xbox360Button.Down },
            { "left", Xbox360Button.Left },
            { "right", Xbox360Button.Right },
            { "guide", Xbox360Button.Guide }
        };

        // Use dictionary lookup (case-insensitive)
        if (buttonMappings.TryGetValue(buttonName, out var button))
        {
            return button;
        }

        // Return null if button name is not found
        return null;
    }

    /// <summary>
    /// Executes the game controller button action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing GameControllerButtonAction: Button={Button}, ControllerIndex={ControllerIndex}, MidiValue={MidiValue}",
                _button, _controllerIndex, midiValue);

            // Check if ViGEm is available
            if (!_controllerManager.IsViGEmAvailable)
            {
                var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
                _logger.LogWarning(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", _logger);
                return;
            }

            // Get the controller instance
            var controller = _controllerManager.GetController(_controllerIndex);
            if (controller == null)
            {
                var errorMsg = $"Failed to get controller instance for index {_controllerIndex}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", _logger);
                return;
            }

            // Check if button mapping is valid
            if (_mappedButton == null)
            {
                var errorMsg = $"Invalid button name: {_button}. Button will not work.";
                _logger.LogWarning(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", _logger);
                return;
            }

            // Press the button
            _logger.LogDebug("Attempting to press button {ButtonName} (enum value: {ButtonValue})",
                _button, (int)_mappedButton.Value);

            controller.SetButtonState(_mappedButton.Value, true);
            _logger.LogDebug("Pressed game controller button: {ButtonName}", _button);

            // Release the button immediately (complete button press action)
            controller.SetButtonState(_mappedButton.Value, false);
            _logger.LogDebug("Released game controller button: {ButtonName}", _button);

            _logger.LogTrace("Successfully executed GameControllerButtonAction for Button={Button}, ControllerIndex={ControllerIndex}",
                _button, _controllerIndex);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing GameControllerButtonAction for button {_button} on controller {_controllerIndex}";
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
