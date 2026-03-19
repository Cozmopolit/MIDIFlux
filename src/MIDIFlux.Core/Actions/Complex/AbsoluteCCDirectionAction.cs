using MIDIFlux.Core.Actions.Parameters;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Action that detects direction of change on absolute CC controllers and executes
/// different sub-actions for increase vs decrease. Tracks the previous CC value internally
/// to determine direction — no external state system required.
/// Use case: rotary knobs or faders mapped to keyboard shortcuts (e.g., scroll up/down).
/// </summary>
[ActionDisplayName("Absolute CC Direction")]
[ActionCategory(ActionCategory.FlowControl)]
public class AbsoluteCCDirectionAction : ActionBase
{
    // Parameter names
    private const string IncreaseActionParam = "IncreaseAction";
    private const string DecreaseActionParam = "DecreaseAction";

    // Internal state for direction tracking (instance-specific, like AlternatingAction._isOnPrimary)
    private int? _previousValue;

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        Parameters[IncreaseActionParam] = new Parameter(
            ParameterType.SubAction,
            null,
            "Increase Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute when the CC value increases (e.g., knob turned clockwise)" }
            }
        };

        Parameters[DecreaseActionParam] = new Parameter(
            ParameterType.SubAction,
            null,
            "Decrease Action")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "description", "Action to execute when the CC value decreases (e.g., knob turned counter-clockwise)" }
            }
        };
    }

    /// <summary>
    /// Core execution logic. Compares the incoming MIDI value against the previously
    /// stored value to determine direction, then executes the appropriate sub-action.
    /// The first received value is stored as baseline without triggering any action.
    /// </summary>
    /// <param name="midiValue">Absolute MIDI CC value (0-127)</param>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (!midiValue.HasValue)
        {
            Logger.LogWarning("AbsoluteCCDirectionAction received null MIDI value, ignoring");
            return;
        }

        var currentValue = midiValue.Value;

        // First message: store as baseline, no action
        if (!_previousValue.HasValue)
        {
            _previousValue = currentValue;
            Logger.LogDebug("AbsoluteCCDirectionAction: First value received ({Value}), stored as baseline", currentValue);
            return;
        }

        var previous = _previousValue.Value;
        _previousValue = currentValue;

        if (currentValue == previous)
        {
            Logger.LogTrace("AbsoluteCCDirectionAction: Value unchanged ({Value}), ignoring", currentValue);
            return;
        }

        var isIncrease = currentValue > previous;
        var actionToExecute = isIncrease
            ? GetParameterValue<ActionBase?>(IncreaseActionParam)
            : GetParameterValue<ActionBase?>(DecreaseActionParam);
        var directionText = isIncrease ? "increase" : "decrease";

        if (actionToExecute == null)
        {
            Logger.LogTrace("AbsoluteCCDirectionAction: No {Direction} action defined, skipping", directionText);
            return;
        }

        Logger.LogDebug("AbsoluteCCDirectionAction: Value {Direction}d ({Previous} -> {Current}), executing {ActionType}",
            directionText, previous, currentValue, actionToExecute.GetType().Name);

        try
        {
            await actionToExecute.ExecuteAsync(midiValue);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing {directionText} action in AbsoluteCCDirectionAction: {ex.Message}";
            Logger.LogError(ex, errorMsg);
            throw new InvalidOperationException(errorMsg, ex);
        }
    }

    /// <inheritdoc />
    protected override string GetDefaultDescription()
    {
        try
        {
            var increaseAction = GetParameterValue<ActionBase?>(IncreaseActionParam);
            var decreaseAction = GetParameterValue<ActionBase?>(DecreaseActionParam);
            var increaseDesc = increaseAction?.Description ?? "None";
            var decreaseDesc = decreaseAction?.Description ?? "None";
            return $"CC Direction: ↑({increaseDesc}) / ↓({decreaseDesc})";
        }
        catch
        {
            return "Absolute CC Direction Action";
        }
    }

    /// <inheritdoc />
    protected override string GetErrorMessage() => "Error executing AbsoluteCCDirectionAction";

    /// <summary>
    /// Compatible with absolute value signals (faders, absolute knobs).
    /// </summary>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.AbsoluteValue };
    }
}

