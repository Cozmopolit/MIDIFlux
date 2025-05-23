using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Handlers;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Commands;

/// <summary>
/// Factory for creating command execution related handlers
/// </summary>
public class CommandHandlerFactory
{
    private readonly ILogger _logger;
    private readonly ActionFactory? _actionFactory;

    /// <summary>
    /// Creates a new instance of the CommandHandlerFactory
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="actionFactory">The action factory to use</param>
    public CommandHandlerFactory(ILogger logger, ActionFactory? actionFactory = null)
    {
        _logger = logger;
        _actionFactory = actionFactory;
    }

    /// <summary>
    /// Registers command handlers with the handler factory
    /// </summary>
    /// <param name="handlerFactory">The handler factory to register with</param>
    public void RegisterHandlers(HandlerFactory handlerFactory)
    {
        // Register command handlers
        handlerFactory.RegisterHandler<INoteHandler>("CommandExecution", CreateCommandExecutionHandler);
    }

    /// <summary>
    /// Creates a command execution handler
    /// </summary>
    /// <param name="parameters">The parameters for the handler</param>
    /// <returns>The created handler, or null if creation failed</returns>
    public INoteHandler? CreateCommandExecutionHandler(Dictionary<string, object> parameters)
    {
        // Extract required command parameter
        if (!parameters.TryGetValue("command", out var commandObj) || commandObj is not string command || string.IsNullOrWhiteSpace(command))
        {
            _logger.LogError("Command not provided for CommandExecution handler");
            return null;
        }

        // Extract optional parameters with simple helper methods
        var shellType = GetEnumParameter(parameters, "shellType", CommandShellType.PowerShell);
        bool runHidden = GetBoolParameter(parameters, "runHidden", false);
        bool waitForExit = GetBoolParameter(parameters, "waitForExit", false);
        string? description = GetStringParameter(parameters, "description", null);

        return new CommandExecutionHandler(_logger, command, shellType, runHidden, waitForExit, description);
    }

    // Simple helper methods to reduce parameter extraction duplication
    private static string? GetStringParameter(Dictionary<string, object> parameters, string key, string? defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is string stringValue && !string.IsNullOrWhiteSpace(stringValue) ? stringValue : defaultValue;
    }

    private static bool GetBoolParameter(Dictionary<string, object> parameters, string key, bool defaultValue)
    {
        return parameters.TryGetValue(key, out var value) && value is bool boolValue ? boolValue : defaultValue;
    }

    private static T GetEnumParameter<T>(Dictionary<string, object> parameters, string key, T defaultValue) where T : struct, Enum
    {
        if (parameters.TryGetValue(key, out var value))
        {
            if (value is T enumValue)
                return enumValue;
            if (value is string stringValue && Enum.TryParse<T>(stringValue, true, out var parsedValue))
                return parsedValue;
        }
        return defaultValue;
    }
}
