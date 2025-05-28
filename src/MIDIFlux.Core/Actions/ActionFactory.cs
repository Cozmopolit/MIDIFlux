using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Factory for creating actions from strongly-typed configuration.
/// Context-aware: services are required in runtime context, optional in GUI context.
/// </summary>
public class ActionFactory : IActionFactory
{
    private readonly ILogger _logger;
    private readonly IServiceProvider? _serviceProvider;
    private readonly bool _isGuiContext;

    /// <summary>
    /// Initializes a new instance of the ActionFactory for runtime usage.
    /// Services are required and will throw if not available.
    /// </summary>
    /// <param name="logger">The logger to use for error handling and diagnostics</param>
    /// <param name="serviceProvider">Service provider for dependency injection (required for runtime)</param>
    public ActionFactory(ILogger<ActionFactory> logger, IServiceProvider serviceProvider)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _isGuiContext = false;
    }

    /// <summary>
    /// Creates an ActionFactory for GUI/configuration editing usage.
    /// Services are intentionally not available.
    /// </summary>
    /// <param name="logger">The logger to use for error handling and diagnostics</param>
    /// <returns>ActionFactory configured for GUI context</returns>
    public static ActionFactory CreateForGui(ILogger<ActionFactory> logger)
    {
        return new ActionFactory(logger, null, isGuiContext: true);
    }

    /// <summary>
    /// Private constructor for GUI context creation
    /// </summary>
    private ActionFactory(ILogger<ActionFactory> logger, IServiceProvider? serviceProvider, bool isGuiContext)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider;
        _isGuiContext = isGuiContext;
    }

    /// <summary>
    /// Creates an action from strongly-typed configuration.
    /// </summary>
    /// <param name="config">The strongly-typed configuration for the action</param>
    /// <returns>The created action instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="NotSupportedException">Thrown when the config type is not supported</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    public IAction CreateAction(ActionConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        if (!config.IsValid())
        {
            var validationErrors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid configuration for {config.Type}: {string.Join(", ", validationErrors)}", nameof(config));
        }

        return config switch
        {
            // Simple keyboard actions - optional services (state tracking)
            KeyPressReleaseConfig keyConfig => new Simple.KeyPressReleaseAction(keyConfig, GetOptionalService<ActionStateManager>()),
            KeyDownConfig keyDownConfig => new Simple.KeyDownAction(keyDownConfig, GetOptionalService<ActionStateManager>()),
            KeyUpConfig keyUpConfig => new Simple.KeyUpAction(keyUpConfig, GetOptionalService<ActionStateManager>()),
            KeyToggleConfig keyToggleConfig => new Simple.KeyToggleAction(keyToggleConfig, GetOptionalService<ActionStateManager>()),

            // Mouse actions - no services needed
            MouseClickConfig mouseClickConfig => new Simple.MouseClickAction(mouseClickConfig),
            MouseScrollConfig mouseScrollConfig => new Simple.MouseScrollAction(mouseScrollConfig),

            // System actions - no services needed
            CommandExecutionConfig cmdConfig => new Simple.CommandExecutionAction(cmdConfig),
            DelayConfig delayConfig => new Simple.DelayAction(delayConfig),

            // Game controller actions - no services needed
            GameControllerButtonConfig gameButtonConfig => new Simple.GameControllerButtonAction(gameButtonConfig),
            GameControllerAxisConfig gameAxisConfig => new Simple.GameControllerAxisAction(gameAxisConfig),

            // Complex actions - pass ActionFactory, handle service requirements internally
            SequenceConfig seqConfig => new Complex.SequenceAction(seqConfig, this),
            ConditionalConfig condConfig => new Complex.ConditionalAction(condConfig, this),
            AlternatingActionConfig altConfig => new Complex.AlternatingAction(altConfig, GetRequiredService<ActionStateManager>(), this),
            RelativeCCConfig relativeCCConfig => new Complex.RelativeCCAction(relativeCCConfig, this),

            // Stateful actions - require services
            StateConditionalConfig stateCondConfig => new Stateful.StateConditionalAction(stateCondConfig, GetRequiredService<ActionStateManager>(), this),
            SetStateConfig setStateConfig => new Stateful.SetStateAction(setStateConfig, GetRequiredService<ActionStateManager>()),

            // MIDI Output actions - require services
            MidiOutputConfig midiOutputConfig => new Simple.MidiOutputAction(midiOutputConfig, GetRequiredService<MidiManager>()),

            // Unsupported types
            _ => throw new NotSupportedException($"Action config type {config.GetType().Name} is not supported")
        };
    }

    /// <summary>
    /// Gets an optional service - null in GUI context, required in runtime context.
    /// </summary>
    /// <typeparam name="T">The service type to resolve</typeparam>
    /// <returns>The resolved service instance or null in GUI context</returns>
    private T? GetOptionalService<T>() where T : class
    {
        if (_isGuiContext)
        {
            return null; // Services intentionally not available in GUI
        }

        // In runtime context, services should be available
        return _serviceProvider!.GetRequiredService<T>();
    }

    /// <summary>
    /// Gets a required service from the service provider.
    /// Throws clear exception if service is not available.
    /// </summary>
    /// <typeparam name="T">The service type to resolve</typeparam>
    /// <returns>The resolved service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not available</exception>
    private T GetRequiredService<T>() where T : notnull
    {
        if (_serviceProvider == null)
        {
            throw new InvalidOperationException($"Cannot create actions requiring {typeof(T).Name} - no service provider configured. This ActionFactory instance is for configuration editing only.");
        }
        return _serviceProvider.GetRequiredService<T>();
    }
}
