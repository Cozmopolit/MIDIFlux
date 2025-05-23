using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Commands;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Handlers;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Handlers.Factory;

/// <summary>
/// Factory for creating MIDI control handlers
/// </summary>
public class HandlerFactory
{
    private readonly ILogger _logger;
    private readonly Dictionary<(string name, Type type), Func<Dictionary<string, object>, IMidiControlHandler?>> _handlerFactories = new();
    private readonly GameControllerHandlerFactory _gameControllerFactory;
    private readonly KeyboardHandlerFactory _keyboardFactory;
    private readonly CommandHandlerFactory _commandFactory;

    /// <summary>
    /// Creates a new instance of the HandlerFactory
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyboardSimulator">The keyboard simulator to use (optional)</param>
    /// <param name="keyStateManager">The key state manager to use (optional)</param>
    public HandlerFactory(
        ILogger<HandlerFactory> logger,
        KeyboardSimulator? keyboardSimulator = null,
        KeyStateManager? keyStateManager = null)
    {
        _logger = logger;

        // Create the action factory if keyboard simulator and key state manager are provided
        ActionFactory? actionFactory = null;
        if (keyboardSimulator != null && keyStateManager != null)
        {
            actionFactory = new ActionFactory(
                LoggingHelper.CreateLogger<ActionFactory>(),
                keyboardSimulator,
                keyStateManager);
        }

        // Create specialized factories
        _gameControllerFactory = new GameControllerHandlerFactory(logger);
        _keyboardFactory = new KeyboardHandlerFactory(logger);
        _commandFactory = new CommandHandlerFactory(logger, actionFactory);

        // Register built-in handlers
        RegisterBuiltInHandlers();
    }

    /// <summary>
    /// Registers the built-in handler types
    /// </summary>
    private void RegisterBuiltInHandlers()
    {
        // Register system handlers
        RegisterHandler<IAbsoluteValueHandler>("SystemVolume", CreateSystemVolumeHandler);
        RegisterHandler<IAbsoluteValueHandler>("CCRange", CreateCCRangeHandler);
        RegisterHandler<IRelativeValueHandler>("ScrollWheel", CreateScrollWheelHandler);

        // Register handlers from specialized factories
        _gameControllerFactory.RegisterHandlers(this);
        _keyboardFactory.RegisterHandlers(this);
        _commandFactory.RegisterHandlers(this);

        int absoluteCount = _handlerFactories.Count(x => x.Key.type == typeof(IAbsoluteValueHandler));
        int relativeCount = _handlerFactories.Count(x => x.Key.type == typeof(IRelativeValueHandler));
        int noteCount = _handlerFactories.Count(x => x.Key.type == typeof(INoteHandler));

        _logger.LogInformation("Registered built-in handlers: {AbsoluteCount} absolute, {RelativeCount} relative, {NoteCount} note",
            absoluteCount, relativeCount, noteCount);
    }

    /// <summary>
    /// Registers a handler factory function
    /// </summary>
    /// <typeparam name="T">The type of handler interface</typeparam>
    /// <param name="name">The name of the handler</param>
    /// <param name="factory">The factory function</param>
    public void RegisterHandler<T>(string name, Func<Dictionary<string, object>, T?> factory) where T : IMidiControlHandler
    {
        _handlerFactories[(name, typeof(T))] = parameters => factory(parameters);
    }

    /// <summary>
    /// Creates a handler of the specified type
    /// </summary>
    /// <typeparam name="T">The type of handler interface</typeparam>
    /// <param name="handlerType">The name of the handler to create</param>
    /// <param name="parameters">Parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public T? CreateHandler<T>(string handlerType, Dictionary<string, object> parameters) where T : IMidiControlHandler
    {
        if (!_handlerFactories.TryGetValue((handlerType, typeof(T)), out var factory))
        {
            _logger.LogError("Unknown handler type: {HandlerType} for {HandlerInterface}", handlerType, typeof(T).Name);
            return default;
        }

        try
        {
            return (T?)factory(parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating handler of type {HandlerType}", handlerType);
            return default;
        }
    }

    /// <summary>
    /// Creates an absolute value handler
    /// </summary>
    /// <param name="handlerType">The type of handler to create</param>
    /// <param name="parameters">Parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public IAbsoluteValueHandler? CreateAbsoluteHandler(string handlerType, Dictionary<string, object> parameters)
    {
        return CreateHandler<IAbsoluteValueHandler>(handlerType, parameters);
    }

    /// <summary>
    /// Creates a relative value handler
    /// </summary>
    /// <param name="handlerType">The type of handler to create</param>
    /// <param name="parameters">Parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public IRelativeValueHandler? CreateRelativeHandler(string handlerType, Dictionary<string, object> parameters)
    {
        return CreateHandler<IRelativeValueHandler>(handlerType, parameters);
    }

    /// <summary>
    /// Creates a note handler
    /// </summary>
    /// <param name="handlerType">The type of handler to create</param>
    /// <param name="parameters">Parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public INoteHandler? CreateNoteHandler(string handlerType, Dictionary<string, object> parameters)
    {
        return CreateHandler<INoteHandler>(handlerType, parameters);
    }

    // Factory methods for specific handlers

    private IAbsoluteValueHandler? CreateSystemVolumeHandler(Dictionary<string, object> parameters)
    {
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogError("SystemVolume handler is only supported on Windows");
            return null;
        }

        return new SystemVolumeHandler(_logger);
    }

    private IRelativeValueHandler? CreateScrollWheelHandler(Dictionary<string, object> parameters)
    {
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogError("ScrollWheel handler is only supported on Windows");
            return null;
        }

        // Extract sensitivity parameter
        int sensitivity = parameters.TryGetValue("sensitivity", out var sensitivityObj) && sensitivityObj is int sens ? sens : 1;
        return new ScrollWheelHandler(_logger, sensitivity);
    }

    private IAbsoluteValueHandler? CreateCCRangeHandler(Dictionary<string, object> parameters)
    {
        // Extract ranges parameter
        if (!parameters.TryGetValue("ranges", out var rangesObj) || rangesObj is not List<CCValueRange> ranges || ranges.Count == 0)
        {
            _logger.LogError("No valid ranges provided for CCRange handler");
            return null;
        }

        // Extract description if present
        string? description = parameters.TryGetValue("description", out var descriptionObj) && descriptionObj is string desc ? desc : null;

        return new CCRangeHandler(_logger, ranges, description);
    }
}
