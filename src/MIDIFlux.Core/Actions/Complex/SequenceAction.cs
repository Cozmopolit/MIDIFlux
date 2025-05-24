using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Complex;

/// <summary>
/// Unified action for executing a sequence of actions (macros).
/// Implements true async behavior for complex orchestration.
/// </summary>
public class SequenceAction : IUnifiedAction
{
    private readonly SequenceErrorHandling _errorHandling;
    private readonly List<IUnifiedAction> _subActions;
    private readonly IUnifiedActionFactory _actionFactory;
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
    /// Gets the error handling strategy for this sequence
    /// </summary>
    public SequenceErrorHandling ErrorHandling => _errorHandling;

    /// <summary>
    /// Gets the child actions in this sequence
    /// </summary>
    /// <returns>List of child actions</returns>
    public List<IUnifiedAction> GetChildActions() => new List<IUnifiedAction>(_subActions);

    /// <summary>
    /// Initializes a new instance of SequenceAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionFactory">The factory to create sub-actions</param>
    /// <exception cref="ArgumentNullException">Thrown when config or actionFactory is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public SequenceAction(SequenceConfig config, IUnifiedActionFactory actionFactory)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "SequenceConfig cannot be null");

        if (actionFactory == null)
            throw new ArgumentNullException(nameof(actionFactory), "IUnifiedActionFactory cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid SequenceConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Sequence ({config.SubActions.Count} actions)";
        _errorHandling = config.ErrorHandling;
        _actionFactory = actionFactory;

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<SequenceAction>();

        // Create sub-actions from configuration
        _subActions = new List<IUnifiedAction>();
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
    /// Executes the sequence action synchronously (not recommended for sequences).
    /// This will execute all sub-actions synchronously without delays.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing SequenceAction (sync): {Description}, SubActions={Count}, ErrorHandling={ErrorHandling}, MidiValue={MidiValue}",
                Description, _subActions.Count, _errorHandling, midiValue);

            // For sequences, we execute all sub-actions synchronously
            // This means DelayActions won't actually delay
            var exceptions = new List<Exception>();

            for (int i = 0; i < _subActions.Count; i++)
            {
                try
                {
                    var subAction = _subActions[i];
                    _logger.LogDebug("Executing sequence step {Step}/{Total}: {ActionDescription}",
                        i + 1, _subActions.Count, subAction.Description);

                    subAction.Execute(midiValue);

                    _logger.LogTrace("Successfully completed sequence step {Step}/{Total}: {ActionDescription}",
                        i + 1, _subActions.Count, subAction.Description);
                }
                catch (Exception ex)
                {
                    var actionDescription = _subActions[i].Description ?? $"Sub-action {i + 1}";
                    var wrappedException = new InvalidOperationException($"Error in sequence step {i + 1} ({actionDescription}): {ex.Message}", ex);
                    exceptions.Add(wrappedException);

                    _logger.LogError(ex, "Error in sequence step {Step}/{Total} ({ActionDescription}): {ErrorMessage}",
                        i + 1, _subActions.Count, actionDescription, ex.Message);

                    if (_errorHandling == SequenceErrorHandling.StopOnError)
                    {
                        _logger.LogWarning("Stopping sequence execution due to error in step {Step} (StopOnError mode)", i + 1);
                        break;
                    }
                    else
                    {
                        _logger.LogDebug("Continuing sequence execution despite error in step {Step} (ContinueOnError mode)", i + 1);
                    }
                }
            }

            // Handle accumulated exceptions
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    _logger.LogError("SequenceAction failed with single error");
                    ApplicationErrorHandler.ShowError($"Sequence action failed: {exceptions[0].Message}", "MIDIFlux - Error", _logger, exceptions[0]);
                    throw exceptions[0];
                }
                else
                {
                    var aggregateEx = new AggregateException($"Multiple errors occurred in sequence execution", exceptions);
                    _logger.LogError("SequenceAction failed with {ErrorCount} errors", exceptions.Count);
                    ApplicationErrorHandler.ShowError($"Sequence action failed with {exceptions.Count} errors. Check logs for details.", "MIDIFlux - Error", _logger, aggregateEx);
                    throw aggregateEx;
                }
            }

            _logger.LogTrace("Successfully completed SequenceAction (sync): {Description}", Description);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException || ex is AggregateException))
        {
            // Handle unexpected exceptions
            var errorMsg = $"Unexpected error executing SequenceAction: {Description}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
            throw;
        }
    }

    /// <summary>
    /// Executes the sequence action asynchronously with proper delay handling.
    /// This is the recommended way to execute sequences.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when all sub-actions are finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing SequenceAction (async): {Description}, SubActions={Count}, ErrorHandling={ErrorHandling}, MidiValue={MidiValue}",
                Description, _subActions.Count, _errorHandling, midiValue);

            // Execute sub-actions sequentially with proper async/await
            // This allows DelayActions to work properly with actual delays
            var exceptions = new List<Exception>();

            for (int i = 0; i < _subActions.Count; i++)
            {
                try
                {
                    var subAction = _subActions[i];
                    _logger.LogDebug("Executing sequence step {Step}/{Total} (async): {ActionDescription}",
                        i + 1, _subActions.Count, subAction.Description);

                    // Use ExecuteAsync for proper async behavior (especially important for DelayAction)
                    await subAction.ExecuteAsync(midiValue);

                    _logger.LogTrace("Successfully completed sequence step {Step}/{Total} (async): {ActionDescription}",
                        i + 1, _subActions.Count, subAction.Description);
                }
                catch (Exception ex)
                {
                    var actionDescription = _subActions[i].Description ?? $"Sub-action {i + 1}";
                    var wrappedException = new InvalidOperationException($"Error in sequence step {i + 1} ({actionDescription}): {ex.Message}", ex);
                    exceptions.Add(wrappedException);

                    _logger.LogError(ex, "Error in sequence step {Step}/{Total} ({ActionDescription}): {ErrorMessage}",
                        i + 1, _subActions.Count, actionDescription, ex.Message);

                    if (_errorHandling == SequenceErrorHandling.StopOnError)
                    {
                        _logger.LogWarning("Stopping sequence execution due to error in step {Step} (StopOnError mode)", i + 1);
                        break;
                    }
                    else
                    {
                        _logger.LogDebug("Continuing sequence execution despite error in step {Step} (ContinueOnError mode)", i + 1);
                    }
                }
            }

            // Handle accumulated exceptions
            if (exceptions.Count > 0)
            {
                if (exceptions.Count == 1)
                {
                    _logger.LogError("SequenceAction (async) failed with single error");
                    ApplicationErrorHandler.ShowError($"Sequence action failed: {exceptions[0].Message}", "MIDIFlux - Error", _logger, exceptions[0]);
                    throw exceptions[0];
                }
                else
                {
                    var aggregateEx = new AggregateException($"Multiple errors occurred in sequence execution", exceptions);
                    _logger.LogError("SequenceAction (async) failed with {ErrorCount} errors", exceptions.Count);
                    ApplicationErrorHandler.ShowError($"Sequence action failed with {exceptions.Count} errors. Check logs for details.", "MIDIFlux - Error", _logger, aggregateEx);
                    throw aggregateEx;
                }
            }

            _logger.LogTrace("Successfully completed SequenceAction (async): {Description}", Description);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException || ex is AggregateException))
        {
            // Handle unexpected exceptions
            var errorMsg = $"Unexpected error executing SequenceAction (async): {Description}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
            throw;
        }
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
