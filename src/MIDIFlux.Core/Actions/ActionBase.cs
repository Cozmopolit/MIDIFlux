using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Unified base class for all actions that encapsulates common boilerplate.
/// Handles logger initialization, error handling, and async execution for both simple and complex actions.
/// Derived classes only need to implement ExecuteAsyncCore() with their business logic.
/// </summary>
/// <typeparam name="TConfig">The strongly-typed configuration class for this action</typeparam>
public abstract class ActionBase<TConfig> : IAction where TConfig : ActionConfig
{
    protected readonly ILogger Logger;
    protected readonly TConfig Config;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes the base action with configuration validation and logger setup
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config validation fails</exception>
    protected ActionBase(TConfig config)
    {
        Config = config ?? throw new ArgumentNullException(nameof(config));

        // Validate configuration
        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid {typeof(TConfig).Name}: {string.Join(", ", errors)}", nameof(config));
        }

        // Initialize common properties
        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? GetDefaultDescription();

        // Initialize logger using the concrete action type
        Logger = LoggingHelper.CreateLoggerForType(GetType());

        Logger.LogDebug("Created {ActionType}: {Description}", GetType().Name, Description);
    }

    /// <summary>
    /// Executes the action asynchronously with centralized error handling and logging.
    /// This is the unified execution path for all actions.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        try
        {
            Logger.LogDebug("Executing {ActionType}: {Description}, MidiValue={MidiValue}",
                GetType().Name, Description, midiValue);

            // Call the derived class implementation
            await ExecuteAsyncCore(midiValue);

            Logger.LogTrace("Successfully executed {ActionType}: {Description}", GetType().Name, Description);
        }
        catch (Exception ex)
        {
            var errorMsg = GetErrorMessage();
            Logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", Logger, ex);
        }
    }

    /// <summary>
    /// Core async execution logic that derived classes must implement.
    /// This method is called within the error handling and logging wrapper.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected abstract ValueTask ExecuteAsyncCore(int? midiValue);

    /// <summary>
    /// Gets the default description for this action type.
    /// Derived classes can override this to provide type-specific default descriptions.
    /// </summary>
    /// <returns>A default description string</returns>
    protected virtual string GetDefaultDescription()
    {
        return $"{GetType().Name} Action";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// Derived classes can override this to provide type-specific error messages.
    /// </summary>
    /// <returns>An error message string</returns>
    protected virtual string GetErrorMessage()
    {
        return $"Error executing {GetType().Name}";
    }



    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
