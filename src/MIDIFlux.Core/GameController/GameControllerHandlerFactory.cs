using MIDIFlux.Core.GameController.Handlers;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.GameController;

/// <summary>
/// Factory for creating game controller related handlers
/// </summary>
public class GameControllerHandlerFactory
{
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a new instance of the GameControllerHandlerFactory
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public GameControllerHandlerFactory(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Registers game controller handlers with the handler factory
    /// </summary>
    /// <param name="handlerFactory">The handler factory to register with</param>
    public void RegisterHandlers(HandlerFactory handlerFactory)
    {
        // Register game controller handlers
        handlerFactory.RegisterHandler<IAbsoluteValueHandler>("GameControllerAxis", parameters => (IAbsoluteValueHandler)CreateGameControllerAxisHandler(parameters)!);
        handlerFactory.RegisterHandler<IRelativeValueHandler>("GameControllerAxis", parameters => (IRelativeValueHandler)CreateGameControllerAxisHandler(parameters)!);
        handlerFactory.RegisterHandler<INoteHandler>("GameControllerButton", CreateGameControllerButtonHandler);
    }

    /// <summary>
    /// Creates a game controller axis handler
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public IMidiControlHandler? CreateGameControllerAxisHandler(Dictionary<string, object> parameters)
    {
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogError("GameControllerAxis handler is only supported on Windows");
            return null;
        }

        // Extract parameters with simple helper method
        string axisName = GetStringParameter(parameters, "axis", "LeftThumbX");
        int minValue = GetIntParameter(parameters, "minValue", 0);
        int maxValue = GetIntParameter(parameters, "maxValue", 127);
        bool invert = GetBoolParameter(parameters, "invert", false);
        int sensitivity = GetIntParameter(parameters, "sensitivity", 1);
        int controllerIndex = GetIntParameter(parameters, "controllerIndex", 0);

        return new GameControllerAxisHandler(_logger, axisName, minValue, maxValue, invert, sensitivity, controllerIndex);
    }

    /// <summary>
    /// Creates a game controller button handler
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public INoteHandler? CreateGameControllerButtonHandler(Dictionary<string, object> parameters)
    {
        if (!OperatingSystem.IsWindows())
        {
            _logger.LogError("GameControllerButton handler is only supported on Windows");
            return null;
        }

        // Extract parameters with simple helper method
        string buttonName = GetStringParameter(parameters, "button", "A");
        int controllerIndex = GetIntParameter(parameters, "controllerIndex", 0);

        return new GameControllerButtonHandler(_logger, buttonName, controllerIndex);
    }

    // Simple helper methods to reduce parameter extraction duplication
    private static string GetStringParameter(Dictionary<string, object> parameters, string key, string defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is string stringValue ? stringValue : defaultValue;
    }

    private static int GetIntParameter(Dictionary<string, object> parameters, string key, int defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is int intValue ? intValue : defaultValue;
    }

    private static bool GetBoolParameter(Dictionary<string, object> parameters, string key, bool defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is bool boolValue ? boolValue : defaultValue;
    }
}
