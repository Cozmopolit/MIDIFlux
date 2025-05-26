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
public class GameControllerButtonAction : ActionBase<GameControllerButtonConfig>
{
    private readonly string _button;
    private readonly int _controllerIndex;
    private readonly GameControllerManager _controllerManager;
    private readonly Xbox360Button? _mappedButton;

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
    public GameControllerButtonAction(GameControllerButtonConfig config) : base(config)
    {
        _button = config.Button;
        _controllerIndex = config.ControllerIndex;

        // Initialize game controller manager
        _controllerManager = GameControllerManager.GetInstance(Logger);
        _mappedButton = MapButtonName(config.Button);

        if (_mappedButton == null)
        {
            Logger.LogWarning("Invalid button name: {ButtonName}. Button will not work.", config.Button);
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
    /// Core execution logic for the game controller button action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Check if ViGEm is available
        if (!_controllerManager.IsViGEmAvailable)
        {
            var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Get the controller instance
        var controller = _controllerManager.GetController(_controllerIndex);
        if (controller == null)
        {
            var errorMsg = $"Failed to get controller instance for index {_controllerIndex}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Check if button mapping is valid
        if (_mappedButton == null)
        {
            var errorMsg = $"Invalid button name: {_button}. Button will not work.";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Press the button
        Logger.LogDebug("Attempting to press button {ButtonName} (enum value: {ButtonValue})",
            _button, (int)_mappedButton.Value);

        controller.SetButtonState(_mappedButton.Value, true);
        Logger.LogDebug("Pressed game controller button: {ButtonName}", _button);

        // Release the button immediately (complete button press action)
        controller.SetButtonState(_mappedButton.Value, false);
        Logger.LogDebug("Released game controller button: {ButtonName}", _button);

        Logger.LogTrace("Successfully executed GameControllerButtonAction for Button={Button}, ControllerIndex={ControllerIndex}",
            _button, _controllerIndex);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Controller {_controllerIndex + 1} Button {_button}";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing GameControllerButtonAction for button {_button} on controller {_controllerIndex}";
    }
}
