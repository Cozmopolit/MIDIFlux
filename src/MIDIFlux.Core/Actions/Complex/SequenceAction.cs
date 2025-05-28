using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// action for executing a sequence of actions (macros).
/// Implements true async behavior for complex orchestration.
/// </summary>
public class SequenceAction : ActionBase<SequenceConfig>
{
    private readonly SequenceErrorHandling _errorHandling;
    private readonly List<IAction> _subActions;
    private readonly IActionFactory _actionFactory;

    /// <summary>
    /// Gets the error handling strategy for this sequence
    /// </summary>
    public SequenceErrorHandling ErrorHandling => _errorHandling;

    /// <summary>
    /// Gets the child actions in this sequence
    /// </summary>
    /// <returns>List of child actions</returns>
    public List<IAction> GetChildActions() => new List<IAction>(_subActions);

    /// <summary>
    /// Initializes a new instance of SequenceAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionFactory">The factory to create sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when config or actionFactory is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public SequenceAction(SequenceConfig config, IActionFactory actionFactory) : base(config)
    {
        _actionFactory = actionFactory ?? throw new ArgumentNullException(nameof(actionFactory), "IActionFactory cannot be null");

        _errorHandling = config.ErrorHandling;

        // Create sub-actions from configuration
        _subActions = new List<IAction>();
        for (int i = 0; i < config.SubActions.Count; i++)
        {
            try
            {
                var subAction = _actionFactory.CreateAction(config.SubActions[i]);
                _subActions.Add(subAction);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to create sub-action {i + 1}: {ex.Message}", nameof(config), ex);
            }
        }
    }

    /// <summary>
    /// Core async execution logic for the sequence action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when all sub-actions are finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        // Execute sub-actions sequentially with proper async/await
        // This allows DelayActions to work properly with actual delays
        var exceptions = new List<Exception>();

        for (int i = 0; i < _subActions.Count; i++)
        {
            try
            {
                var subAction = _subActions[i];
                Logger.LogDebug("Executing sequence step {Step}/{Total}: {ActionDescription}",
                    i + 1, _subActions.Count, subAction.Description);

                // Use ExecuteAsync for proper async behavior (especially important for DelayAction)
                await subAction.ExecuteAsync(midiValue);

                Logger.LogTrace("Successfully completed sequence step {Step}/{Total}: {ActionDescription}",
                    i + 1, _subActions.Count, subAction.Description);
            }
            catch (Exception ex)
            {
                var actionDescription = _subActions[i].Description ?? $"Sub-action {i + 1}";
                var wrappedException = new InvalidOperationException($"Error in sequence step {i + 1} ({actionDescription}): {ex.Message}", ex);
                exceptions.Add(wrappedException);

                Logger.LogError(ex, "Error in sequence step {Step}/{Total} ({ActionDescription}): {ErrorMessage}",
                    i + 1, _subActions.Count, actionDescription, ex.Message);

                if (_errorHandling == SequenceErrorHandling.StopOnError)
                {
                    Logger.LogWarning("Stopping sequence execution due to error in step {Step} (StopOnError mode)", i + 1);
                    break;
                }
                else
                {
                    Logger.LogDebug("Continuing sequence execution despite error in step {Step} (ContinueOnError mode)", i + 1);
                }
            }
        }

        // Handle accumulated exceptions
        if (exceptions.Count > 0)
        {
            if (exceptions.Count == 1)
            {
                Logger.LogError("SequenceAction failed with single error");
                ApplicationErrorHandler.ShowError($"Sequence action failed: {exceptions[0].Message}", "MIDIFlux - Error", Logger, exceptions[0]);
                throw exceptions[0];
            }
            else
            {
                var aggregateEx = new AggregateException($"Multiple errors occurred in sequence execution", exceptions);
                Logger.LogError("SequenceAction failed with {ErrorCount} errors", exceptions.Count);
                ApplicationErrorHandler.ShowError($"Sequence action failed with {exceptions.Count} errors. Check logs for details.", "MIDIFlux - Error", Logger, aggregateEx);
                throw aggregateEx;
            }
        }

        Logger.LogTrace("Successfully completed SequenceAction: {Description}", Description);
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"Sequence ({Config.SubActions.Count} actions)";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing SequenceAction: {Description}";
    }
}
