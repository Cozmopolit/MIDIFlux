using MIDIFlux.Core.Handlers;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Keyboard;

/// <summary>
/// Factory for creating keyboard related handlers
/// </summary>
public class KeyboardHandlerFactory
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the KeyboardHandlerFactory
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public KeyboardHandlerFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers keyboard handlers with the handler factory
    /// </summary>
    /// <param name="handlerFactory">The handler factory to register with</param>
    public void RegisterHandlers(HandlerFactory handlerFactory)
    {
        // Register keyboard handlers
        handlerFactory.RegisterHandler<INoteHandler>("ToggleKey", CreateToggleKeyHandler);
    }

    /// <summary>
    /// Creates a toggle key handler
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public INoteHandler? CreateToggleKeyHandler(Dictionary<string, object> parameters)
    {
        // Get the KeyStateManager from the parameters
        if (!parameters.TryGetValue("keyStateManager", out var keyStateManagerObj) || keyStateManagerObj is not KeyStateManager keyStateManager)
        {
            _logger.LogError("KeyStateManager not provided for ToggleKey handler");
            return null;
        }

        // Extract parameters with simple helper methods
        ushort virtualKeyCode = GetUShortParameter(parameters, "virtualKeyCode", 0);
        List<ushort>? modifiers = GetListParameter<ushort>(parameters, "modifiers");

        return new ToggleKeyHandler(keyStateManager, _logger, virtualKeyCode, modifiers);
    }

    // Simple helper methods to reduce parameter extraction duplication
    private static ushort GetUShortParameter(Dictionary<string, object> parameters, string key, ushort defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is ushort ushortValue ? ushortValue : defaultValue;
    }

    private static List<T>? GetListParameter<T>(Dictionary<string, object> parameters, string key)
    {
        return parameters.TryGetValue(key, out var value) && value is List<T> listValue ? listValue : null;
    }
}
