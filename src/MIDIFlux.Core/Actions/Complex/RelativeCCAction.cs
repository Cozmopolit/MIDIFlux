using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action that handles relative MIDI controllers (scratch wheels, endless encoders).
/// Decodes relative CC values and executes increase/decrease actions multiple times based on magnitude.
/// </summary>
public class RelativeCCAction : ActionBase<RelativeCCConfig>
{
    private readonly IAction _increaseAction;
    private readonly IAction _decreaseAction;
    private readonly IActionFactory _actionFactory;

    /// <summary>
    /// Gets the increase action
    /// </summary>
    public IAction IncreaseAction => _increaseAction;

    /// <summary>
    /// Gets the decrease action
    /// </summary>
    public IAction DecreaseAction => _decreaseAction;



    /// <summary>
    /// Initializes a new instance of RelativeCCAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionFactory">Factory for creating sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when config or actionFactory is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public RelativeCCAction(RelativeCCConfig config, IActionFactory actionFactory) : base(config)
    {
        _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory));

        // Create the sub-actions
        _increaseAction = _actionFactory.CreateAction(config.IncreaseAction);
        _decreaseAction = _actionFactory.CreateAction(config.DecreaseAction);
    }

    /// <summary>
    /// Core execution logic for the relative CC action.
    /// Decodes the MIDI value and executes the appropriate action multiple times.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) from the relative controller</param>
    /// <returns>A ValueTask that completes when all actions are finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (!midiValue.HasValue)
        {
            Logger.LogWarning("RelativeCCAction received null MIDI value, ignoring");
            return;
        }

        // Decode the relative value using SignMagnitude encoding
        var (direction, magnitude) = DecodeSignMagnitude(midiValue.Value);

        if (magnitude == 0)
        {
            Logger.LogTrace("RelativeCCAction received zero magnitude, ignoring");
            return;
        }

        // Select the appropriate action
        var actionToExecute = direction > 0 ? _increaseAction : _decreaseAction;
        var directionText = direction > 0 ? "increase" : "decrease";

        Logger.LogTrace("RelativeCCAction executing {Direction} action {Magnitude} times (MIDI value: {MidiValue})",
            directionText, magnitude, midiValue.Value);

        // Execute the action multiple times based on magnitude
        for (int i = 0; i < magnitude; i++)
        {
            try
            {
                await actionToExecute.ExecuteAsync(midiValue);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error executing {Direction} action (iteration {Iteration}/{Magnitude}): {ErrorMessage}",
                    directionText, i + 1, magnitude, ex.Message);

                // Continue with remaining iterations even if one fails
                ApplicationErrorHandler.ShowError(
                    $"Error executing {directionText} action (iteration {i + 1}/{magnitude}): {ex.Message}",
                    "MIDIFlux - RelativeCC Action Error",
                    Logger,
                    ex);
            }
        }
    }



    /// <summary>
    /// Decodes sign-magnitude encoding: values 1-63 are positive, 65-127 are negative, 64 is zero
    /// </summary>
    private static (int direction, int magnitude) DecodeSignMagnitude(int midiValue)
    {
        if (midiValue == 64)
            return (0, 0); // No change

        if (midiValue >= 1 && midiValue <= 63)
            return (1, midiValue); // Positive direction

        if (midiValue >= 65 && midiValue <= 127)
            return (-1, midiValue - 64); // Negative direction

        // Value 0 - treat as no change
        return (0, 0);
    }



    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var increaseDesc = _increaseAction?.Description ?? "Unknown";
        var decreaseDesc = _decreaseAction?.Description ?? "Unknown";
        return $"Relative CC: +({increaseDesc}) / -({decreaseDesc})";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return "Error executing RelativeCCAction";
    }

    /// <summary>
    /// Gets the child actions for this complex action
    /// </summary>
    /// <returns>List of child actions</returns>
    public List<IAction> GetChildActions()
    {
        return new List<IAction> { _increaseAction, _decreaseAction };
    }
}
