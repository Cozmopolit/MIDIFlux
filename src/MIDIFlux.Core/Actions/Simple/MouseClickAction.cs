using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Mouse;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for clicking a mouse button (Left, Right, Middle).
/// Consolidates MouseClickConfig into the action class using the parameter system.
/// </summary>
[ActionDisplayName("Mouse Click")]
public class MouseClickAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string ButtonParam = "Button";



    /// <summary>
    /// Initializes a new instance of MouseClickAction with default parameters
    /// </summary>
    public MouseClickAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of MouseClickAction with specified mouse button
    /// </summary>
    /// <param name="button">The mouse button to click</param>
    public MouseClickAction(MouseButton button) : base()
    {
        var buttonString = button switch
        {
            MouseButton.Left => "Left",
            MouseButton.Right => "Right",
            MouseButton.Middle => "Middle",
            _ => "Left" // Default fallback
        };
        SetParameterValue(ButtonParam, buttonString);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Button parameter with string type
        Parameters[ButtonParam] = new Parameter(
            ParameterType.String,
            "Left", // Default to left mouse button
            "Mouse Button")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "Left", "Right", "Middle" } }
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var button = GetParameterValue<string>(ButtonParam);
        var allowedButtons = new[] { "Left", "Right", "Middle" };
        if (!allowedButtons.Contains(button))
        {
            AddValidationError($"Button must be one of: {string.Join(", ", allowedButtons)}");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the mouse click action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var buttonString = GetParameterValue<string>(ButtonParam);

        // Convert string to MouseButton enum for MouseSimulator
        var button = buttonString switch
        {
            "Left" => MouseButton.Left,
            "Right" => MouseButton.Right,
            "Middle" => MouseButton.Middle,
            _ => throw new InvalidOperationException($"Unknown mouse button: {buttonString}")
        };

        // Create mouse simulator - no service dependency needed for this simple action
        var mouseLogger = LoggingHelper.CreateLogger<MouseSimulator>();
        var mouseSimulator = new MouseSimulator(mouseLogger);

        // Perform the mouse click
        if (!mouseSimulator.SendMouseClick(button))
        {
            var errorMsg = $"Failed to send mouse click for button {buttonString}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("Successfully executed MouseClickAction for Button={Button}", buttonString);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Click {GetParameterValue<string>(ButtonParam)} Mouse Button";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing MouseClickAction for button {GetParameterValue<string>(ButtonParam)}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// MouseClickAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
