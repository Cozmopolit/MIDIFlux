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
/// Defines categories for grouping actions in the GUI.
/// </summary>
public enum ActionCategory
{
    /// <summary>Keyboard-related actions (key press, hold, release, etc.)</summary>
    Keyboard,
    /// <summary>Mouse-related actions (click, move, scroll)</summary>
    Mouse,
    /// <summary>Virtual game controller actions (buttons, axes)</summary>
    GameController,
    /// <summary>MIDI output actions (send notes, CC, SysEx)</summary>
    MidiOutput,
    /// <summary>Flow control actions that orchestrate other actions (sequence, conditional)</summary>
    FlowControl,
    /// <summary>State management actions (set, increase, decrease, conditional)</summary>
    State,
    /// <summary>Utility actions (delay, play sound, log, execute command)</summary>
    Utility
}

/// <summary>
/// Attribute to specify the category for an action type in the GUI.
/// Used for grouping actions in the action type selector.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ActionCategoryAttribute : Attribute
{
    /// <summary>
    /// The category for this action type
    /// </summary>
    public ActionCategory Category { get; }

    /// <summary>
    /// Initializes a new instance of the ActionCategoryAttribute
    /// </summary>
    /// <param name="category">The category for the action type</param>
    public ActionCategoryAttribute(ActionCategory category)
    {
        Category = category;
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
    private bool _isInitialized;

    /// <summary>
    /// Static service provider for dependency injection in runtime context.
    /// When set, indicates runtime context (MIDI processing).
    /// When null, indicates GUI/configuration context.
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
            EnsureInitialized();
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
            EnsureInitialized();
            // During deserialization, populate the Parameters dictionary with converted values
            PopulateParametersFromJson(value);
        }
    }

    /// <summary>
    /// Logger for this action instance
    /// </summary>
    protected readonly ILogger Logger;

    /// <summary>
    /// Backing field for Description property
    /// </summary>
    private string _description = string.Empty;

    /// <summary>
    /// Gets or sets a human-readable description of this action.
    /// Getter triggers lazy initialization to ensure description is populated.
    /// </summary>
    public string Description
    {
        get
        {
            EnsureInitialized();
            return _description;
        }
        set => _description = value;
    }



    /// <summary>
    /// Initializes the base action. Does NOT call virtual methods - use EnsureInitialized() for lazy init.
    /// </summary>
    protected ActionBase()
    {
        Logger = LoggingHelper.CreateLoggerForType(GetType());
        // NOTE: Do NOT call InitializeParameters() or GetDefaultDescription() here!
        // Calling virtual/abstract methods in constructor is unsafe - derived class fields are not yet initialized.
        // Instead, we use lazy initialization via EnsureInitialized().
    }

    /// <summary>
    /// Ensures the action is fully initialized (parameters and description).
    /// Called lazily on first access to parameters or execution.
    /// Safe to call multiple times - only initializes once.
    /// </summary>
    private void EnsureInitialized()
    {
        if (_isInitialized) return;

        // Initialize parameters - derived classes populate the Parameters dictionary
        InitializeParameters();

        // Mark as initialized BEFORE calling GetDefaultDescription() to prevent infinite recursion.
        // GetDefaultDescription() may call GetParameterValue() which calls EnsureInitialized().
        // By setting the flag first, the recursive call returns immediately.
        _isInitialized = true;

        // Set description after parameters are available (if not already set by JSON deserialization)
        // Use _description directly to avoid recursive call to EnsureInitialized() via Description getter
        if (string.IsNullOrEmpty(_description))
        {
            _description = GetDefaultDescription();
        }
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
        EnsureInitialized();
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
        EnsureInitialized();

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
        EnsureInitialized();

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
    /// Determines if the current action is being executed in GUI/configuration context.
    /// GUI context = ServiceProvider is null (no runtime services available).
    /// Runtime context = ServiceProvider is set (MIDI processing with full services).
    /// </summary>
    /// <returns>True if in GUI/configuration context, false if in runtime context</returns>
    protected bool IsRunningInGuiContext()
    {
        // Simple and fast: ServiceProvider is only set during runtime MIDI processing.
        // In GUI/configuration context, ServiceProvider is null.
        return ServiceProvider == null;
    }

    /// <summary>
    /// Validates the action configuration and parameters with recursive validation for SubActionList
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public virtual bool IsValid()
    {
        EnsureInitialized();
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
        EnsureInitialized();

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
                    // For enum parameters, the JSON converter returns raw strings (e.g., "A") that need
                    // to be resolved to actual enum values (e.g., Keys.A) using the parameter's EnumDefinition
                    if (parameter.Type == ParameterType.Enum && convertedValue is string stringValue)
                    {
                        convertedValue = ConvertStringToEnum(stringValue, parameter.EnumDefinition);
                    }
                    parameter.SetValue(convertedValue);
                }
                else
                {
                    // Handle specific type conversions for deserialized values
                    var convertedValue = ConvertDeserializedValue(kvp.Value, parameter);
                    parameter.SetValue(convertedValue);
                }
            }
        }
    }

    /// <summary>
    /// Converts already-deserialized JSON values to the correct parameter types
    /// </summary>
    /// <param name="value">The deserialized value</param>
    /// <param name="parameter">The target parameter with type and enum definition</param>
    /// <returns>The converted value</returns>
    private object? ConvertDeserializedValue(object? value, Parameter parameter)
    {
        if (value == null)
            return null;

        return parameter.Type switch
        {
            // Handle SubAction parameter type (single ActionBase)
            ParameterType.SubAction when value is ActionBase singleAction => singleAction,

            // Handle SubActionList parameter type (array to list conversion)
            ParameterType.SubActionList when value is ActionBase[] actionArray => actionArray.ToList(),

            // Handle ValueConditionList parameter type (array to list conversion)
            ParameterType.ValueConditionList when value is Parameters.ValueCondition[] conditionArray => conditionArray.ToList(),

            // Handle Enum parameter type (string values need to be converted using EnumDefinition)
            ParameterType.Enum when value is string stringValue => ConvertStringToEnum(stringValue, parameter.EnumDefinition),

            // Handle other basic types as-is
            _ => value
        };
    }

    /// <summary>
    /// Converts a string value back to the appropriate enum type using the parameter's EnumDefinition
    /// </summary>
    /// <param name="stringValue">The string representation of the enum</param>
    /// <param name="enumDefinition">The enum definition containing valid options and values</param>
    /// <returns>The enum value, or the original string if conversion fails</returns>
    private object? ConvertStringToEnum(string stringValue, EnumDefinition? enumDefinition)
    {
        // If we have an EnumDefinition, use it to look up the value
        if (enumDefinition != null)
        {
            var enumValue = enumDefinition.GetValue(stringValue);
            if (enumValue != null)
            {
                return enumValue;
            }
        }

        // Fallback: return the string value (will likely cause issues downstream, but at least we don't crash)
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
