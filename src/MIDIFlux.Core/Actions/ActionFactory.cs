using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Factory for creating actions from strongly-typed configuration.
/// Implements type-safe action creation with comprehensive error handling and logging.
/// </summary>
public class ActionFactory : IActionFactory
{
    private readonly ILogger _logger;
    private readonly IServiceProvider? _serviceProvider;

    /// <summary>
    /// Initializes a new instance of the ActionFactory
    /// </summary>
    /// <param name="logger">The logger to use for error handling and diagnostics</param>
    /// <param name="serviceProvider">Optional service provider for dependency injection</param>
    public ActionFactory(ILogger<ActionFactory> logger, IServiceProvider? serviceProvider = null)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _logger.LogDebug("ActionFactory initialized with service provider: {HasServiceProvider}", serviceProvider != null);
    }

    /// <summary>
    /// Creates a action from strongly-typed configuration.
    /// Uses pattern matching on config types for type-safe creation with no runtime parameter parsing.
    /// </summary>
    /// <param name="config">The strongly-typed configuration for the action</param>
    /// <returns>The created action instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="NotSupportedException">Thrown when the config type is not supported</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    public IAction CreateAction(ActionConfig config)
    {
        if (config == null)
        {
            var ex = new ArgumentNullException(nameof(config), "Action configuration cannot be null");
            _logger.LogError(ex, "Attempted to create action with null configuration");
            throw ex;
        }

        _logger.LogTrace("Creating action of type {ActionType}: {Description}",
            config.Type, config.Description ?? "No description");

        try
        {
            // Validate configuration before creating action
            if (!config.IsValid())
            {
                var validationErrors = config.GetValidationErrors();
                var errorMessage = $"Invalid configuration for {config.Type}: {string.Join(", ", validationErrors)}";
                var ex = new ArgumentException(errorMessage, nameof(config));
                _logger.LogError(ex, "Action configuration validation failed for {ActionType}: {ValidationErrors}",
                    config.Type, string.Join(", ", validationErrors));
                throw ex;
            }

            // Type-safe creation using pattern matching - no runtime parameter parsing needed
            IAction action = config switch
            {
                // Simple keyboard actions
                KeyPressReleaseConfig keyConfig => CreateKeyPressReleaseAction(keyConfig),
                KeyDownConfig keyDownConfig => CreateKeyDownAction(keyDownConfig),
                KeyUpConfig keyUpConfig => CreateKeyUpAction(keyUpConfig),
                KeyToggleConfig keyToggleConfig => CreateKeyToggleAction(keyToggleConfig),

                // Mouse actions
                MouseClickConfig mouseClickConfig => CreateMouseClickAction(mouseClickConfig),
                MouseScrollConfig mouseScrollConfig => CreateMouseScrollAction(mouseScrollConfig),

                // System actions
                CommandExecutionConfig cmdConfig => CreateCommandExecutionAction(cmdConfig),
                DelayConfig delayConfig => CreateDelayAction(delayConfig),

                // Game controller actions
                GameControllerButtonConfig gameButtonConfig => CreateGameControllerButtonAction(gameButtonConfig),
                GameControllerAxisConfig gameAxisConfig => CreateGameControllerAxisAction(gameAxisConfig),

                // Complex actions (will be implemented in later phases)
                SequenceConfig seqConfig => CreateSequenceAction(seqConfig),
                ConditionalConfig condConfig => CreateConditionalAction(condConfig),
                AlternatingActionConfig altConfig => CreateAlternatingAction(altConfig),

                // Stateful actions
                StateConditionalConfig stateCondConfig => CreateStateConditionalAction(stateCondConfig),
                SetStateConfig setStateConfig => CreateSetStateAction(setStateConfig),

                // MIDI Output actions
                MidiOutputConfig midiOutputConfig => CreateMidiOutputAction(midiOutputConfig),

                // Unsupported types
                _ => throw new NotSupportedException($"Action config type {config.GetType().Name} is not supported")
            };

            _logger.LogDebug("Successfully created action {ActionId} of type {ActionType}: {Description}",
                action.Id, config.Type, action.Description);

            return action;
        }
        catch (Exception ex) when (!(ex is ArgumentNullException || ex is ArgumentException || ex is NotSupportedException))
        {
            // Log and re-throw unexpected exceptions with additional context
            _logger.LogError(ex, "Unexpected error creating action of type {ActionType}: {ErrorMessage}",
                config.Type, ex.Message);
            throw new InvalidOperationException($"Failed to create action of type {config.Type}: {ex.Message}", ex);
        }
    }

    // Simple action creation methods - unified pattern
    private IAction CreateKeyPressReleaseAction(KeyPressReleaseConfig config)
    {
        return CreateActionWithDependencies<Simple.KeyPressReleaseAction, KeyPressReleaseConfig>(
            config,
            () => GetActionStateManager(),
            $"VirtualKeyCode: {config.VirtualKeyCode}");
    }

    private IAction CreateKeyDownAction(KeyDownConfig config)
    {
        return CreateActionWithDependencies<Simple.KeyDownAction, KeyDownConfig>(
            config,
            () => GetActionStateManager(),
            $"VirtualKeyCode: {config.VirtualKeyCode}, AutoRelease: {config.AutoReleaseAfterMs}");
    }

    private IAction CreateKeyUpAction(KeyUpConfig config)
    {
        return CreateActionWithDependencies<Simple.KeyUpAction, KeyUpConfig>(
            config,
            () => GetActionStateManager(),
            $"VirtualKeyCode: {config.VirtualKeyCode}");
    }

    private IAction CreateKeyToggleAction(KeyToggleConfig config)
    {
        return CreateActionWithDependencies<Simple.KeyToggleAction, KeyToggleConfig>(
            config,
            () => GetActionStateManager(),
            $"VirtualKeyCode: {config.VirtualKeyCode}");
    }

    private IAction CreateMouseClickAction(MouseClickConfig config)
    {
        return CreateActionWithoutDependencies<Simple.MouseClickAction, MouseClickConfig>(
            config,
            $"Button: {config.Button}");
    }

    private IAction CreateMouseScrollAction(MouseScrollConfig config)
    {
        return CreateActionWithoutDependencies<Simple.MouseScrollAction, MouseScrollConfig>(
            config,
            $"Direction: {config.Direction}, Amount: {config.Amount}");
    }

    private IAction CreateCommandExecutionAction(CommandExecutionConfig config)
    {
        return CreateActionWithoutDependencies<Simple.CommandExecutionAction, CommandExecutionConfig>(
            config,
            $"Command: {config.Command}, Shell: {config.ShellType}");
    }

    private IAction CreateDelayAction(DelayConfig config)
    {
        return CreateActionWithoutDependencies<Simple.DelayAction, DelayConfig>(
            config,
            $"Milliseconds: {config.Milliseconds}");
    }

    private IAction CreateGameControllerButtonAction(GameControllerButtonConfig config)
    {
        return CreateActionWithoutDependencies<Simple.GameControllerButtonAction, GameControllerButtonConfig>(
            config,
            $"Button: {config.Button}, Controller: {config.ControllerIndex}");
    }

    private IAction CreateGameControllerAxisAction(GameControllerAxisConfig config)
    {
        return CreateActionWithoutDependencies<Simple.GameControllerAxisAction, GameControllerAxisConfig>(
            config,
            $"Axis: {config.AxisName}, Value: {config.AxisValue}, Controller: {config.ControllerIndex}");
    }

    // Complex action creation methods - unified pattern
    private IAction CreateSequenceAction(SequenceConfig config)
    {
        return CreateActionWithFactory<Complex.SequenceAction, SequenceConfig>(
            config,
            () => this,
            $"{config.SubActions.Count} sub-actions, ErrorHandling: {config.ErrorHandling}");
    }

    private IAction CreateConditionalAction(ConditionalConfig config)
    {
        return CreateActionWithFactory<Complex.ConditionalAction, ConditionalConfig>(
            config,
            () => this,
            $"{config.Conditions.Count} conditions");
    }

    private IAction CreateAlternatingAction(AlternatingActionConfig config)
    {
        return CreateActionWithMultipleDependencies<Complex.AlternatingAction, AlternatingActionConfig>(
            config,
            () => (GetActionStateManager(), this),
            $"StateKey: {config.StateKey}, StartWithPrimary: {config.StartWithPrimary}");
    }

    // Stateful action creation methods - unified pattern
    private IAction CreateStateConditionalAction(StateConditionalConfig config)
    {
        return CreateActionWithMultipleDependencies<Stateful.StateConditionalAction, StateConditionalConfig>(
            config,
            () => (GetActionStateManager(), this),
            $"StateKey: {config.StateKey}, Comparison: {config.Condition.Comparison}, Value: {config.Condition.StateValue}");
    }

    private IAction CreateSetStateAction(SetStateConfig config)
    {
        return CreateActionWithDependencies<Stateful.SetStateAction, SetStateConfig>(
            config,
            () => GetActionStateManager(),
            $"StateKey: {config.StateKey}, Value: {config.StateValue}");
    }

    // MIDI Output action creation methods - unified pattern
    private IAction CreateMidiOutputAction(MidiOutputConfig config)
    {
        return CreateActionWithDependencies<Simple.MidiOutputAction, MidiOutputConfig>(
            config,
            () => GetMidiManager(),
            $"Device: {config.OutputDeviceName}, Commands: {config.Commands?.Count ?? 0}");
    }

    // action creation helper methods - eliminates duplicate patterns

    /// <summary>
    /// Creates an action without dependencies using unified logging and error handling pattern
    /// </summary>
    /// <typeparam name="TAction">The action type to create</typeparam>
    /// <typeparam name="TConfig">The configuration type</typeparam>
    /// <param name="config">The configuration instance</param>
    /// <param name="logDetails">Details to include in the log message</param>
    /// <returns>The created action instance</returns>
    private IAction CreateActionWithoutDependencies<TAction, TConfig>(TConfig config, string logDetails)
        where TAction : class, IAction
        where TConfig : ActionConfig
    {
        var actionName = typeof(TAction).Name;
        _logger.LogTrace("Creating {ActionName} with {LogDetails}", actionName, logDetails);

        return (TAction?)Activator.CreateInstance(typeof(TAction), config)
            ?? throw new InvalidOperationException($"Failed to create instance of {actionName}");
    }

    /// <summary>
    /// Creates an action with a single dependency using unified logging and error handling pattern
    /// </summary>
    /// <typeparam name="TAction">The action type to create</typeparam>
    /// <typeparam name="TConfig">The configuration type</typeparam>
    /// <param name="config">The configuration instance</param>
    /// <param name="dependencyProvider">Function that provides the dependency</param>
    /// <param name="logDetails">Details to include in the log message</param>
    /// <returns>The created action instance</returns>
    private IAction CreateActionWithDependencies<TAction, TConfig>(TConfig config, Func<object> dependencyProvider, string logDetails)
        where TAction : class, IAction
        where TConfig : ActionConfig
    {
        var actionName = typeof(TAction).Name;
        _logger.LogTrace("Creating {ActionName} with {LogDetails}", actionName, logDetails);

        var dependency = dependencyProvider();
        return (TAction?)Activator.CreateInstance(typeof(TAction), config, dependency)
            ?? throw new InvalidOperationException($"Failed to create instance of {actionName}");
    }

    /// <summary>
    /// Creates an action with multiple dependencies using unified logging and error handling pattern
    /// </summary>
    /// <typeparam name="TAction">The action type to create</typeparam>
    /// <typeparam name="TConfig">The configuration type</typeparam>
    /// <param name="config">The configuration instance</param>
    /// <param name="dependencyProvider">Function that provides the dependencies as a tuple</param>
    /// <param name="logDetails">Details to include in the log message</param>
    /// <returns>The created action instance</returns>
    private IAction CreateActionWithMultipleDependencies<TAction, TConfig>(TConfig config, Func<(object, object)> dependencyProvider, string logDetails)
        where TAction : class, IAction
        where TConfig : ActionConfig
    {
        var actionName = typeof(TAction).Name;
        _logger.LogTrace("Creating {ActionName} with {LogDetails}", actionName, logDetails);

        var (dependency1, dependency2) = dependencyProvider();
        return (TAction?)Activator.CreateInstance(typeof(TAction), config, dependency1, dependency2)
            ?? throw new InvalidOperationException($"Failed to create instance of {actionName}");
    }

    /// <summary>
    /// Creates an action with factory dependency using unified logging and error handling pattern
    /// </summary>
    /// <typeparam name="TAction">The action type to create</typeparam>
    /// <typeparam name="TConfig">The configuration type</typeparam>
    /// <param name="config">The configuration instance</param>
    /// <param name="factoryProvider">Function that provides the factory</param>
    /// <param name="logDetails">Details to include in the log message</param>
    /// <returns>The created action instance</returns>
    private IAction CreateActionWithFactory<TAction, TConfig>(TConfig config, Func<ActionFactory> factoryProvider, string logDetails)
        where TAction : class, IAction
        where TConfig : ActionConfig
    {
        var actionName = typeof(TAction).Name;
        _logger.LogTrace("Creating {ActionName} with {LogDetails}", actionName, logDetails);

        var factory = factoryProvider();
        return (TAction?)Activator.CreateInstance(typeof(TAction), config, factory)
            ?? throw new InvalidOperationException($"Failed to create instance of {actionName}");
    }

    /// <summary>
    /// Gets the ActionStateManager from the service provider or creates a fallback instance
    /// </summary>
    /// <returns>ActionStateManager instance</returns>
    private ActionStateManager GetActionStateManager()
    {
        // Try to get from service provider first
        if (_serviceProvider != null)
        {
            var actionStateManager = _serviceProvider.GetService<ActionStateManager>();
            if (actionStateManager != null)
            {
                return actionStateManager;
            }
        }

        // Fallback: create a new instance (for testing or standalone usage)
        _logger.LogWarning("ActionStateManager not available from service provider, creating fallback instance");
        var keyboardSimulator = new Keyboard.KeyboardSimulator(Helpers.LoggingHelper.CreateLogger<Keyboard.KeyboardSimulator>());
        var logger = Helpers.LoggingHelper.CreateLogger<ActionStateManager>();
        return new ActionStateManager(keyboardSimulator, logger);
    }

    /// <summary>
    /// Gets the MidiManager from the service provider
    /// </summary>
    /// <returns>MidiManager instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when MidiManager is not available</exception>
    private MidiManager GetMidiManager()
    {
        // Try to get from service provider first
        if (_serviceProvider != null)
        {
            var midiManager = _serviceProvider.GetService<MidiManager>();
            if (midiManager != null)
            {
                return midiManager;
            }
        }

        // MidiManager is required for MIDI output actions - no fallback
        var errorMsg = "MidiManager not available from service provider. MIDI output actions require a properly configured MidiManager.";
        _logger.LogError(errorMsg);
        throw new InvalidOperationException(errorMsg);
    }
}
