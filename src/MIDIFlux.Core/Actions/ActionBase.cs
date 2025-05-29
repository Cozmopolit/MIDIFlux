using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Unified base class for all actions implementing the parameter system.
/// Consolidates ActionConfig classes into Action classes with unified parameter handling.
/// Supports both runtime (with services) and GUI (without services) contexts.
///
/// Input Type Compatibility:
/// Each derived action class must implement a static GetCompatibleInputCategories() method
/// that returns an array of InputTypeCategory values indicating which MIDI input types
/// are logically compatible with this action. This is used by the GUI to filter available
/// actions based on the selected input type, preventing illogical combinations.
///
/// Example implementation:
/// public static InputTypeCategory[] GetCompatibleInputCategories()
/// {
///     return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue };
/// }
/// </summary>
[JsonConverter(typeof(ActionJsonConverter))]
public abstract class ActionBase : IAction
{
    private readonly List<string> _validationErrors = new();

    /// <summary>
    /// Static service provider for dependency injection in runtime context
    /// </summary>
    public static IServiceProvider? ServiceProvider { get; set; }

    /// <summary>
    /// Protected parameters dictionary for derived classes to populate
    /// </summary>
    [JsonIgnore]
    protected readonly Dictionary<string, Parameter> Parameters = new();

    /// <summary>
    /// JSON-serializable parameters dictionary that handles both simple values and SubActionList
    /// </summary>
    [JsonPropertyName("Parameters")]
    [JsonConverter(typeof(ParametersJsonConverter))]
    public Dictionary<string, object?> JsonParameters
    {
        get
        {
            var result = new Dictionary<string, object?>();
            foreach (var kvp in Parameters)
            {
                if (kvp.Value.Type == ParameterType.SubAction && kvp.Value.Value is ActionBase singleAction)
                {
                    // Keep SubAction as ActionBase type - ParametersJsonConverter will handle polymorphic serialization
                    result[kvp.Key] = singleAction;
                }
                else if (kvp.Value.Type == ParameterType.SubActionList && kvp.Value.Value is List<ActionBase> actionList)
                {
                    // Keep SubActionList as ActionBase[] - ParametersJsonConverter will handle polymorphic serialization
                    result[kvp.Key] = actionList.ToArray();
                }
                else if (kvp.Value.Type == ParameterType.ValueConditionList && kvp.Value.Value is List<Parameters.ValueCondition> conditionList)
                {
                    // Serialize ValueConditionList as array of condition objects
                    result[kvp.Key] = conditionList.ToArray();
                }
                else
                {
                    // Keep original value - ParametersJsonConverter will handle enum string conversion
                    result[kvp.Key] = kvp.Value.Value;
                }
            }
            return result;
        }
        set
        {
            // During deserialization, populate the Parameters dictionary with values
            foreach (var kvp in value)
            {
                if (Parameters.TryGetValue(kvp.Key, out var parameter))
                {
                    if (parameter.Type == ParameterType.SubAction && kvp.Value is JsonElement singleActionElement)
                    {
                        // Handle SubAction deserialization
                        var actionJson = singleActionElement.GetRawText();
                        var action = JsonSerializer.Deserialize<ActionBase>(actionJson);
                        if (action != null)
                        {
                            parameter.SetValue(action);
                        }
                    }
                    else if (parameter.Type == ParameterType.SubActionList && kvp.Value is JsonElement jsonElement)
                    {
                        // Handle SubActionList deserialization
                        var actionList = new List<ActionBase>();
                        if (jsonElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var actionElement in jsonElement.EnumerateArray())
                            {
                                var actionJson = actionElement.GetRawText();
                                var action = JsonSerializer.Deserialize<ActionBase>(actionJson);
                                if (action != null)
                                {
                                    actionList.Add(action);
                                }
                            }
                        }
                        parameter.SetValue(actionList);
                    }
                    else if (parameter.Type == ParameterType.ValueConditionList && kvp.Value is JsonElement conditionJsonElement)
                    {
                        // Handle ValueConditionList deserialization
                        var conditionList = new List<Parameters.ValueCondition>();
                        if (conditionJsonElement.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var conditionElement in conditionJsonElement.EnumerateArray())
                            {
                                var conditionJson = conditionElement.GetRawText();
                                var condition = JsonSerializer.Deserialize<Parameters.ValueCondition>(conditionJson);
                                if (condition != null)
                                {
                                    conditionList.Add(condition);
                                }
                            }
                        }
                        parameter.SetValue(conditionList);
                    }
                    else
                    {
                        // Handle proper type conversion for JsonElement values
                        if (kvp.Value is JsonElement paramJsonElement)
                        {
                            var convertedValue = ConvertJsonElementToParameterType(paramJsonElement, parameter.Type);
                            parameter.SetValue(convertedValue);
                        }
                        else
                        {
                            parameter.SetValue(kvp.Value);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Logger for this action instance
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    [JsonIgnore]
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Indicates whether this action is running in configuration mode (GUI) vs runtime mode
    /// </summary>
    [JsonIgnore]
    public bool IsConfigurationMode => ServiceProvider == null;

    /// <summary>
    /// Initializes the base action with parameter setup
    /// </summary>
    protected ActionBase()
    {
        Id = Guid.NewGuid().ToString();
        Logger = LoggingHelper.CreateLoggerForType(GetType());

        // Initialize parameters FIRST - derived classes will populate this in their constructors
        InitializeParameters();

        // THEN set description after parameters are available
        Description = GetDefaultDescription();
    }

    /// <summary>
    /// Abstract method for derived classes to initialize their parameters
    /// </summary>
    protected abstract void InitializeParameters();

    /// <summary>
    /// Gets a list of parameter information for UI generation
    /// </summary>
    /// <returns>List of parameter metadata</returns>
    public List<ParameterInfo> GetParameterList()
    {
        return Parameters.Select(kvp => new ParameterInfo(kvp.Key, kvp.Value)).ToList();
    }

    /// <summary>
    /// Gets a parameter value with type safety and validation
    /// </summary>
    /// <typeparam name="T">The expected parameter type</typeparam>
    /// <param name="parameterName">The parameter name</param>
    /// <returns>The typed parameter value</returns>
    /// <exception cref="ArgumentException">Thrown when parameter doesn't exist</exception>
    /// <exception cref="InvalidOperationException">Thrown when parameter cannot be converted to specified type</exception>
    public T GetParameterValue<T>(string parameterName)
    {
        if (!Parameters.TryGetValue(parameterName, out var parameter))
        {
            throw new ArgumentException($"Parameter '{parameterName}' not found in action {GetType().Name}", nameof(parameterName));
        }

        try
        {
            return parameter.GetValue<T>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get parameter '{parameterName}' as type {typeof(T).Name} in action {GetType().Name}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Sets a parameter value with type safety
    /// </summary>
    /// <typeparam name="T">The parameter type</typeparam>
    /// <param name="parameterName">The parameter name</param>
    /// <param name="value">The new value</param>
    /// <exception cref="ArgumentException">Thrown when parameter doesn't exist</exception>
    public void SetParameterValue<T>(string parameterName, T value)
    {
        if (!Parameters.TryGetValue(parameterName, out var parameter))
        {
            throw new ArgumentException($"Parameter '{parameterName}' not found in action {GetType().Name}", nameof(parameterName));
        }

        parameter.SetValue(value);
    }

    /// <summary>
    /// Gets a service from the service provider (optional - returns null in GUI mode)
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance, or null if not available</returns>
    protected T? GetService<T>() where T : class
    {
        if (ServiceProvider == null)
        {
            return null; // Services not available in GUI mode
        }

        return ServiceProvider.GetService<T>();
    }

    /// <summary>
    /// Gets a required service from the service provider
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance</returns>
    /// <exception cref="InvalidOperationException">Thrown when service is not available</exception>
    protected T GetRequiredService<T>() where T : class
    {
        if (ServiceProvider == null)
        {
            throw new InvalidOperationException($"Service {typeof(T).Name} is not available in configuration mode");
        }

        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Validates the action configuration and parameters with recursive validation for SubActionList
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public virtual bool IsValid()
    {
        _validationErrors.Clear();

        // Validate SubAction and SubActionList parameters recursively
        foreach (var parameter in Parameters.Values)
        {
            if (parameter.Type == ParameterType.SubAction && parameter.Value is ActionBase singleAction)
            {
                if (!singleAction.IsValid())
                {
                    var subErrors = singleAction.GetValidationErrors();
                    foreach (var error in subErrors)
                    {
                        AddValidationError($"Sub-action ({singleAction.GetType().Name}): {error}");
                    }
                }
            }
            else if (parameter.Type == ParameterType.SubActionList && parameter.Value is List<ActionBase> actionList)
            {
                for (int i = 0; i < actionList.Count; i++)
                {
                    var subAction = actionList[i];
                    if (!subAction.IsValid())
                    {
                        var subErrors = subAction.GetValidationErrors();
                        foreach (var error in subErrors)
                        {
                            AddValidationError($"Sub-action {i + 1} ({subAction.GetType().Name}): {error}");
                        }
                    }
                }
            }
            else if (parameter.Type == ParameterType.ValueConditionList && parameter.Value is List<Parameters.ValueCondition> conditionList)
            {
                for (int i = 0; i < conditionList.Count; i++)
                {
                    var condition = conditionList[i];
                    if (!condition.IsValid())
                    {
                        var conditionErrors = condition.GetValidationErrors();
                        foreach (var error in conditionErrors)
                        {
                            AddValidationError($"Condition {i + 1}: {error}");
                        }
                    }
                }
            }
        }

        // Derived classes can override this to add specific validation
        return _validationErrors.Count == 0;
    }

    /// <summary>
    /// Gets the validation errors from the last IsValid() call
    /// </summary>
    /// <returns>List of validation error messages</returns>
    public List<string> GetValidationErrors()
    {
        return new List<string>(_validationErrors);
    }

    /// <summary>
    /// Adds a validation error to the internal error list
    /// </summary>
    /// <param name="error">The validation error message</param>
    protected void AddValidationError(string error)
    {
        _validationErrors.Add(error);
    }

    /// <summary>
    /// Executes the action asynchronously with centralized error handling and logging
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        try
        {
            // Call the derived class implementation
            await ExecuteAsyncCore(midiValue);
        }
        catch (Exception ex)
        {
            var errorMsg = GetErrorMessage();
            Logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", Logger, ex);
        }
    }

    /// <summary>
    /// Core async execution logic that derived classes must implement
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected abstract ValueTask ExecuteAsyncCore(int? midiValue);

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected virtual string GetDefaultDescription()
    {
        return $"{GetType().Name} Action";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected virtual string GetErrorMessage()
    {
        return $"Error executing {GetType().Name}";
    }

    /// <summary>
    /// Converts a JsonElement to the appropriate type based on the parameter type
    /// </summary>
    /// <param name="jsonElement">The JsonElement to convert</param>
    /// <param name="parameterType">The target parameter type</param>
    /// <returns>The converted value</returns>
    private static object? ConvertJsonElementToParameterType(JsonElement jsonElement, ParameterType parameterType)
    {
        return parameterType switch
        {
            ParameterType.Integer => jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.String when int.TryParse(jsonElement.GetString(), out var intValue) => intValue,
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to integer")
            },
            ParameterType.String => jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => jsonElement.GetRawText()
            },
            ParameterType.Boolean => jsonElement.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(jsonElement.GetString(), out var boolValue) => boolValue,
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to boolean")
            },
            ParameterType.Enum => jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.String => jsonElement.GetString(), // Return string value for enum name parsing
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to enum")
            },
            ParameterType.ByteArray => jsonElement.ValueKind switch
            {
                JsonValueKind.Array => jsonElement.EnumerateArray().Select(e => e.GetByte()).ToArray(),
                JsonValueKind.String => Convert.FromBase64String(jsonElement.GetString() ?? ""),
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to byte array")
            },
            _ => throw new JsonException($"Unsupported parameter type: {parameterType}")
        };
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
