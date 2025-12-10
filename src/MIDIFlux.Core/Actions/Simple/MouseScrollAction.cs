using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Mouse;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for scrolling the mouse wheel in a specified direction.
/// Consolidates MouseScrollConfig into the action class using the parameter system.
/// </summary>
[ActionDisplayName("Mouse Scroll")]
[ActionCategory(ActionCategory.Mouse)]
public class MouseScrollAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string DirectionParam = "Direction";
    private const string AmountParam = "Amount";



    /// <summary>
    /// Initializes a new instance of MouseScrollAction with default parameters
    /// </summary>
    public MouseScrollAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of MouseScrollAction with specified direction and amount
    /// </summary>
    /// <param name="direction">The scroll direction</param>
    /// <param name="amount">The scroll amount (default: 1)</param>
    public MouseScrollAction(ScrollDirection direction, int amount = 1) : base()
    {
        var directionString = direction switch
        {
            ScrollDirection.Up => "Up",
            ScrollDirection.Down => "Down",
            ScrollDirection.Left => "Left",
            ScrollDirection.Right => "Right",
            _ => "Up" // Default fallback
        };
        SetParameterValue(DirectionParam, directionString);
        SetParameterValue(AmountParam, amount);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add Direction parameter with string type
        Parameters[DirectionParam] = new Parameter(
            ParameterType.String,
            "", // No default - user must specify direction
            "Scroll Direction")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "allowedValues", new[] { "Up", "Down", "Left", "Right" } }
            }
        };

        // Add Amount parameter with integer type
        Parameters[AmountParam] = new Parameter(
            ParameterType.Integer,
            null, // No default - user must specify amount
            "Scroll Amount")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 1 },
                { "max", 10 }
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

        var direction = GetParameterValue<string>(DirectionParam);
        var allowedDirections = new[] { "Up", "Down", "Left", "Right" };
        if (string.IsNullOrWhiteSpace(direction) || !allowedDirections.Contains(direction))
        {
            AddValidationError($"Direction must be specified and one of: {string.Join(", ", allowedDirections)}");
        }

        var amount = GetParameterValue<int?>(AmountParam);
        if (!amount.HasValue || amount.Value < 1 || amount.Value > 10)
        {
            AddValidationError("Amount must be specified and between 1 and 10");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the mouse scroll action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var directionString = GetParameterValue<string>(DirectionParam);
        var amount = GetParameterValue<int?>(AmountParam) ?? throw new InvalidOperationException("Amount not specified");

        // Convert string to ScrollDirection enum for MouseSimulator
        var direction = directionString switch
        {
            "Up" => ScrollDirection.Up,
            "Down" => ScrollDirection.Down,
            "Left" => ScrollDirection.Left,
            "Right" => ScrollDirection.Right,
            _ => throw new InvalidOperationException($"Unknown scroll direction: {directionString}")
        };

        // Create mouse simulator - no service dependency needed for this simple action
        var mouseLogger = LoggingHelper.CreateLogger<MouseSimulator>();
        var mouseSimulator = new MouseSimulator(mouseLogger);

        // Perform the mouse scroll
        if (!mouseSimulator.SendMouseScroll(direction, amount))
        {
            var errorMsg = $"Failed to send mouse scroll for direction {directionString} with amount {amount}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Mouse Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("Successfully executed MouseScrollAction for Direction={Direction}, Amount={Amount}",
            directionString, amount);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var direction = GetParameterValue<string>(DirectionParam);
            var amount = GetParameterValue<int?>(AmountParam);
            var amountText = amount.HasValue && amount.Value != 1 ? $" ({amount.Value} steps)" : "";
            return $"Scroll {direction ?? "?"}{amountText}";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Scroll Mouse";
        }
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var direction = GetParameterValue<string>(DirectionParam);
        var amount = GetParameterValue<int?>(AmountParam);
        return $"Error executing MouseScrollAction for direction {direction ?? "?"} with amount {amount?.ToString() ?? "?"}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// MouseScrollAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
