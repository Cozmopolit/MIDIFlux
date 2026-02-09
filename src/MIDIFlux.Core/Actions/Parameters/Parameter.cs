
namespace MIDIFlux.Core.Actions.Parameters;

/// <summary>
/// Represents a parameter in the unified action parameter system.
/// Stores the parameter value, type information, and validation hints for UI generation.
/// </summary>
public class Parameter
{
    /// <summary>
    /// The type of this parameter, determining validation and UI control type
    /// </summary>
    public ParameterType Type { get; set; }

    /// <summary>
    /// The current value of this parameter (stored as object for flexibility)
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Human-readable display name for UI generation
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Optional validation hints for UI controls (e.g., min/max for integers, maxLength for strings)
    /// </summary>
    public Dictionary<string, object>? ValidationHints { get; set; }

    /// <summary>
    /// For Enum parameters: the enum type name for reflection-based enum handling
    /// </summary>
    public string? EnumTypeName { get; set; }

    /// <summary>
    /// For Enum parameters: the available options and their corresponding values
    /// </summary>
    public EnumDefinition? EnumDefinition { get; set; }

    /// <summary>
    /// Initializes a new Parameter instance
    /// </summary>
    public Parameter()
    {
    }

    /// <summary>
    /// Initializes a new Parameter with basic information
    /// </summary>
    /// <param name="type">The parameter type</param>
    /// <param name="value">The initial value</param>
    /// <param name="displayName">The display name for UI</param>
    public Parameter(ParameterType type, object? value, string displayName)
    {
        Type = type;
        Value = value;
        DisplayName = displayName;
    }

    /// <summary>
    /// Gets the parameter value as the specified type with validation
    /// </summary>
    /// <typeparam name="T">The expected type</typeparam>
    /// <returns>The typed value</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value cannot be converted to the specified type</exception>
    public T GetValue<T>()
    {
        if (Value == null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException($"Parameter '{DisplayName}' has null value but type {typeof(T).Name} is not nullable");
            }
            return default(T)!;
        }

        if (Value is T directValue)
        {
            return directValue;
        }

        // Special handling for Enum parameters
        if (Type == ParameterType.Enum && typeof(T).IsEnum)
        {
            if (Value is int intValue)
            {
                return (T)Enum.ToObject(typeof(T), intValue);
            }
            if (Value is string stringValue)
            {
                return (T)Enum.Parse(typeof(T), stringValue);
            }
        }

        try
        {
            return (T)Convert.ChangeType(Value, typeof(T));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Parameter '{DisplayName}' value '{Value}' cannot be converted to type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// Sets the parameter value
    /// </summary>
    /// <typeparam name="T">The value type</typeparam>
    /// <param name="value">The new value</param>
    public void SetValue<T>(T value)
    {
        Value = value;
    }
}
