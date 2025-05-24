using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions.Configuration;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Factory for creating unified actions from strongly-typed configuration.
/// Implements type-safe action creation with comprehensive error handling and logging.
/// </summary>
public class UnifiedActionFactory : IUnifiedActionFactory
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the UnifiedActionFactory
    /// </summary>
    /// <param name="logger">The logger to use for error handling and diagnostics</param>
    public UnifiedActionFactory(ILogger<UnifiedActionFactory> logger)
    {
        _logger = logger;
        _logger.LogDebug("UnifiedActionFactory initialized");
    }

    /// <summary>
    /// Creates a unified action from strongly-typed configuration.
    /// Uses pattern matching on config types for type-safe creation with no runtime parameter parsing.
    /// </summary>
    /// <param name="config">The strongly-typed configuration for the action</param>
    /// <returns>The created action instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="NotSupportedException">Thrown when the config type is not supported</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    public IUnifiedAction CreateAction(UnifiedActionConfig config)
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
            IUnifiedAction action = config switch
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

    // Simple action creation methods
    private IUnifiedAction CreateKeyPressReleaseAction(KeyPressReleaseConfig config)
    {
        _logger.LogTrace("Creating KeyPressReleaseAction with VirtualKeyCode: {VirtualKeyCode}", config.VirtualKeyCode);
        return new Simple.KeyPressReleaseAction(config);
    }

    private IUnifiedAction CreateKeyDownAction(KeyDownConfig config)
    {
        _logger.LogTrace("Creating KeyDownAction with VirtualKeyCode: {VirtualKeyCode}, AutoRelease: {AutoRelease}",
            config.VirtualKeyCode, config.AutoReleaseAfterMs);
        return new Simple.KeyDownAction(config);
    }

    private IUnifiedAction CreateKeyUpAction(KeyUpConfig config)
    {
        _logger.LogTrace("Creating KeyUpAction with VirtualKeyCode: {VirtualKeyCode}", config.VirtualKeyCode);
        return new Simple.KeyUpAction(config);
    }

    private IUnifiedAction CreateKeyToggleAction(KeyToggleConfig config)
    {
        _logger.LogTrace("Creating KeyToggleAction with VirtualKeyCode: {VirtualKeyCode}", config.VirtualKeyCode);
        return new Simple.KeyToggleAction(config);
    }

    private IUnifiedAction CreateMouseClickAction(MouseClickConfig config)
    {
        _logger.LogTrace("Creating MouseClickAction with Button: {Button}", config.Button);
        return new Simple.MouseClickAction(config);
    }

    private IUnifiedAction CreateMouseScrollAction(MouseScrollConfig config)
    {
        _logger.LogTrace("Creating MouseScrollAction with Direction: {Direction}, Amount: {Amount}",
            config.Direction, config.Amount);
        return new Simple.MouseScrollAction(config);
    }

    private IUnifiedAction CreateCommandExecutionAction(CommandExecutionConfig config)
    {
        _logger.LogTrace("Creating CommandExecutionAction with Command: {Command}, Shell: {Shell}",
            config.Command, config.ShellType);
        return new Simple.CommandExecutionAction(config);
    }

    private IUnifiedAction CreateDelayAction(DelayConfig config)
    {
        _logger.LogTrace("Creating DelayAction with Milliseconds: {Milliseconds}", config.Milliseconds);
        return new Simple.DelayAction(config);
    }

    private IUnifiedAction CreateGameControllerButtonAction(GameControllerButtonConfig config)
    {
        _logger.LogTrace("Creating GameControllerButtonAction with Button: {Button}, Controller: {Controller}",
            config.Button, config.ControllerIndex);
        return new Simple.GameControllerButtonAction(config);
    }

    private IUnifiedAction CreateGameControllerAxisAction(GameControllerAxisConfig config)
    {
        _logger.LogTrace("Creating GameControllerAxisAction with Axis: {Axis}, Value: {Value}, Controller: {Controller}",
            config.AxisName, config.AxisValue, config.ControllerIndex);
        return new Simple.GameControllerAxisAction(config);
    }

    // Complex action creation methods
    private IUnifiedAction CreateSequenceAction(SequenceConfig config)
    {
        _logger.LogTrace("Creating SequenceAction with {Count} sub-actions, ErrorHandling: {ErrorHandling}",
            config.SubActions.Count, config.ErrorHandling);
        return new Complex.SequenceAction(config, this);
    }

    private IUnifiedAction CreateConditionalAction(ConditionalConfig config)
    {
        _logger.LogTrace("Creating ConditionalAction with {Count} conditions", config.Conditions.Count);
        return new Complex.ConditionalAction(config, this);
    }
}
