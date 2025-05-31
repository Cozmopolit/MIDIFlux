using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Reflection;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Attribute to specify the display name for an action type in the GUI.
/// This eliminates the need for hardcoded display name mappings.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ActionDisplayNameAttribute : Attribute
{
    /// <summary>
    /// The display name for this action type
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Initializes a new instance of the ActionDisplayNameAttribute
    /// </summary>
    /// <param name="displayName">The display name for the action type</param>
    public ActionDisplayNameAttribute(string displayName)
    {
        DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
    }
}

/// <summary>
/// Base class for all actions implementing the parameter system.
/// Consolidates ActionConfig classes into Action classes with parameter handling.
/// Supports both runtime (with services) and GUI (without services) contexts.
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
                // Return the actual parameter values (converted during deserialization)
                result[kvp.Key] = kvp.Value.Value;
            }
            return result;
        }
        set
        {
            // During deserialization, populate the Parameters dictionary with converted values
            PopulateParametersFromJson(value);
        }
    }

    /// <summary>
    /// Logger for this action instance
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; set; }



    /// <summary>
    /// Initializes the base action with parameter setup
    /// </summary>
    protected ActionBase()
    {
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
    /// Gets the input type categories that are compatible with this action.
    /// Used by the GUI to filter available actions based on the selected input type.
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public abstract InputTypeCategory[] GetCompatibleInputCategories();

    /// <summary>
    /// Gets a list of parameter information for UI generation
    /// </summary>
    /// <returns>List of parameter metadata</returns>
    public List<Parameters.ParameterInfo> GetParameterList()
    {
        return Parameters.Select(kvp => new Parameters.ParameterInfo(kvp.Key, kvp.Value)).ToList();
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
    /// Gets a service from the service provider (optional - returns null in configuration context)
    /// </summary>
    /// <typeparam name="T">The service type</typeparam>
    /// <returns>The service instance, or null if not available</returns>
    protected T? GetService<T>() where T : class
    {
        if (ServiceProvider == null)
        {
            return null; // Services not available in configuration context
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
            throw new InvalidOperationException($"Service {typeof(T).Name} is not available in configuration context");
        }

        return ServiceProvider.GetRequiredService<T>();
    }

    /// <summary>
    /// Determines if the current action is being executed in GUI context by checking the calling assembly
    /// </summary>
    /// <returns>True if called from GUI context, false if called from runtime context</returns>
    protected bool IsRunningInGuiContext()
    {
        // Check the call stack for GUI assemblies
        var stackTrace = new System.Diagnostics.StackTrace();
        var frames = stackTrace.GetFrames();

        if (frames != null)
        {
            // Look for any frame that comes from MIDIFlux.GUI assembly
            foreach (var frame in frames)
            {
                var method = frame.GetMethod();
                var assembly = method?.DeclaringType?.Assembly;
                var assemblyName = assembly?.GetName().Name;

                if (assemblyName == "MIDIFlux.GUI")
                {
                    Logger.LogDebug("IsRunningInGuiContext: Found GUI assembly in call stack");
                    return true;
                }
            }
        }

        Logger.LogDebug("IsRunningInGuiContext: No GUI assembly found in call stack");
        return false;
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
    /// Executes the action asynchronously
    /// Error handling is delegated to callers using RunWithUiErrorHandling
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    public async ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Call the derived class implementation directly
        // Error handling is now delegated to callers using ApplicationErrorHandler.RunWithUiErrorHandling
        await ExecuteAsyncCore(midiValue);
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
    /// Populates the Parameters dictionary from JSON values using the converter logic
    /// </summary>
    /// <param name="jsonValues">The JSON parameter values</param>
    private void PopulateParametersFromJson(Dictionary<string, object?> jsonValues)
    {
        foreach (var kvp in jsonValues)
        {
            if (Parameters.TryGetValue(kvp.Key, out var parameter))
            {
                // Handle proper type conversion for JsonElement values using the converter
                if (kvp.Value is JsonElement jsonElement)
                {
                    var convertedValue = ParametersJsonConverter.ConvertJsonElementToParameterType(jsonElement, parameter.Type);
                    parameter.SetValue(convertedValue);
                }
                else
                {
                    // Handle specific type conversions for deserialized values
                    var convertedValue = ConvertDeserializedValue(kvp.Value, parameter.Type);
                    parameter.SetValue(convertedValue);
                }
            }
        }
    }

    /// <summary>
    /// Converts already-deserialized JSON values to the correct parameter types
    /// </summary>
    /// <param name="value">The deserialized value</param>
    /// <param name="parameterType">The target parameter type</param>
    /// <returns>The converted value</returns>
    private object? ConvertDeserializedValue(object? value, ParameterType parameterType)
    {
        if (value == null)
            return null;

        return parameterType switch
        {
            // Handle SubAction parameter type (single ActionBase)
            ParameterType.SubAction when value is ActionBase singleAction => singleAction,

            // Handle SubActionList parameter type (array to list conversion)
            ParameterType.SubActionList when value is ActionBase[] actionArray => actionArray.ToList(),

            // Handle ValueConditionList parameter type (array to list conversion)
            ParameterType.ValueConditionList when value is Parameters.ValueCondition[] conditionArray => conditionArray.ToList(),

            // Handle Enum parameter type (string values need to be converted back to enum)
            ParameterType.Enum when value is string stringValue => ConvertStringToEnum(stringValue),

            // Handle other basic types as-is
            _ => value
        };
    }

    /// <summary>
    /// Converts a string value back to the appropriate enum type
    /// </summary>
    /// <param name="stringValue">The string representation of the enum</param>
    /// <returns>The enum value</returns>
    private object? ConvertStringToEnum(string stringValue)
    {
        // Try to convert common enum types used in actions
        if (Enum.TryParse<Configuration.SequenceErrorHandling>(stringValue, out var sequenceErrorHandling))
            return sequenceErrorHandling;

        // Add other enum types as needed
        // For now, return the string value if we can't convert it
        return stringValue;
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
