using System;
using System.Collections.Generic;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for pressing and holding a game controller button down.
/// Implements sync-by-default execution for performance.
/// Uses existing ViGEm integration in GameController directory.
/// </summary>
[ActionDisplayName("Game Controller Button Down")]
[ActionCategory(ActionCategory.GameController)]
public class GameControllerButtonDownAction : ActionBase
{
    private const string ButtonParam = "Button";
    private const string ControllerIndexParam = "ControllerIndex";

    /// <summary>
    /// Initializes a new instance of GameControllerButtonDownAction with default parameters
    /// </summary>
    public GameControllerButtonDownAction() : base()
    {
        // No hardware initialization during construction - only during execution
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Button parameter with string type for Xbox controller buttons
        Parameters[ButtonParam] = new Parameter(
            ParameterType.String,
            "A", // Default to A button
            "Button")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "A", "B", "X", "Y", "LeftShoulder", "RightShoulder", "Back", "Start", "LeftThumb", "RightThumb", "Up", "Down", "Left", "Right", "Guide" } }
            }
        };

        // Add ControllerIndex parameter with integer type
        Parameters[ControllerIndexParam] = new Parameter(
            ParameterType.Integer,
            0, // Default to controller 0
            "Controller Index")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 3 }
            }
        };
    }

    /// <summary>
    /// Validates this action's parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        var isValid = base.IsValid();

        // Validate button name
        var button = GetParameterValue<string>(ButtonParam);
        if (string.IsNullOrEmpty(button))
        {
            AddValidationError("Button name cannot be empty");
            isValid = false;
        }
        else if (MapButtonName(button) == null)
        {
            AddValidationError($"Invalid button name: {button}");
            isValid = false;
        }

        // Validate controller index
        var controllerIndex = GetParameterValue<int>(ControllerIndexParam);
        if (!ActionHelper.IsIntegerInRange(controllerIndex, 0, 3))
        {
            AddValidationError($"Controller index must be between 0 and 3, got: {controllerIndex}");
            isValid = false;
        }

        return isValid;
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
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var button = GetParameterValue<string>(ButtonParam) ?? "";
        return $"Press Game Controller Button Down ({button})";
    }

    /// <summary>
    /// Core execution logic for the game controller button down action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue = null)
    {
        var button = GetParameterValue<string>(ButtonParam);
        var controllerIndex = GetParameterValue<int>(ControllerIndexParam);

        // Get GameControllerManager at execution time (not during construction)
        var controllerManager = GameControllerManager.GetInstance(Logger);

        // Check if ViGEm is available
        if (!controllerManager.IsViGEmAvailable)
        {
            var errorMsg = "ViGEm Bus Driver not available - game controller features are disabled";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Get the controller instance
        var controller = controllerManager.GetController(controllerIndex);
        if (controller == null)
        {
            var errorMsg = $"Failed to get controller instance for index {controllerIndex}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Map button name to Xbox360Button
        var mappedButton = MapButtonName(button);
        if (mappedButton == null)
        {
            var errorMsg = $"Invalid button name: {button}. Button will not work.";
            Logger.LogWarning(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Game Controller Warning", Logger);
            return ValueTask.CompletedTask;
        }

        // Get ActionStateManager service if available for state tracking
        var actionStateManager = GetService<ActionStateManager>();
        var stateKey = $"*GameControllerButton{controllerIndex}_{button}"; // Internal state key for this button

        // Check current state to avoid duplicate button presses
        if (actionStateManager != null)
        {
            var currentState = actionStateManager.GetState(stateKey);
            if (currentState == 1)
            {
                Logger.LogDebug("Game controller button {ButtonName} on controller {ControllerIndex} is already pressed (state={State}), skipping button down", 
                    button, controllerIndex, currentState);
                return ValueTask.CompletedTask;
            }
        }

        // Press the button down and hold
        Logger.LogDebug("Attempting to press button {ButtonName} down (enum value: {ButtonValue})",
            button, (int)mappedButton.Value);

        controller.SetButtonState(mappedButton, true);
        Logger.LogDebug("Pressed game controller button down: {ButtonName}", button);

        // Update state if state manager is available
        if (actionStateManager != null)
        {
            actionStateManager.SetState(stateKey, 1); // 1 = pressed
        }

        Logger.LogTrace("Successfully executed GameControllerButtonDownAction for Button={Button}, ControllerIndex={ControllerIndex}",
            button, controllerIndex);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// GameControllerButtonDownAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
