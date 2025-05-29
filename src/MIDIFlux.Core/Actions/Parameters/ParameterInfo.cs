namespace MIDIFlux.Core.Actions.Parameters;

/// <summary>
/// Provides read-only metadata about a parameter for UI generation and introspection.
/// Exposes parameter information without allowing direct modification of the parameter.
/// </summary>
public class ParameterInfo
{
    /// <summary>
    /// The parameter name/key
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The type of this parameter
    /// </summary>
    public ParameterType Type { get; }

    /// <summary>
    /// Human-readable display name for UI generation
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Current value of the parameter (read-only)
    /// </summary>
    public object? Value { get; }

    /// <summary>
    /// Validation hints for UI controls (read-only)
    /// </summary>
    public IReadOnlyDictionary<string, object>? ValidationHints { get; }

    /// <summary>
    /// For Enum parameters: the enum definition (read-only)
    /// </summary>
    public EnumDefinition? EnumDefinition { get; }

    /// <summary>
    /// For Enum parameters: the enum type name
    /// </summary>
    public string? EnumTypeName { get; }

    /// <summary>
    /// Initializes a new ParameterInfo from a Parameter
    /// </summary>
    /// <param name="name">The parameter name/key</param>
    /// <param name="parameter">The source parameter</param>
    public ParameterInfo(string name, Parameter parameter)
    {
        Name = name;
        Type = parameter.Type;
        DisplayName = parameter.DisplayName;
        Value = parameter.Value;
        ValidationHints = parameter.ValidationHints?.AsReadOnly();
        EnumDefinition = parameter.EnumDefinition;
        EnumTypeName = parameter.EnumTypeName;
    }

    /// <summary>
    /// Gets the parameter value as the specified type
    /// </summary>
    /// <typeparam name="T">The expected type</typeparam>
    /// <returns>The typed value</returns>
    public T GetValue<T>()
    {
        if (Value == null)
        {
            if (typeof(T).IsValueType && Nullable.GetUnderlyingType(typeof(T)) == null)
            {
                throw new InvalidOperationException($"Parameter '{Name}' has null value but type {typeof(T).Name} is not nullable");
            }
            return default(T)!;
        }

        if (Value is T directValue)
        {
            return directValue;
        }

        // Special handling for Enum parameters (same logic as Parameter.GetValue<T>)
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
            throw new InvalidOperationException($"Parameter '{Name}' value '{Value}' cannot be converted to type {typeof(T).Name}", ex);
        }
    }

    /// <summary>
    /// For enum parameters, gets the value as the specified enum type
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>The typed enum value</returns>
    public TEnum GetEnumValue<TEnum>() where TEnum : Enum
    {
        if (Type != ParameterType.Enum)
        {
            throw new InvalidOperationException($"Parameter '{Name}' is not an enum parameter");
        }

        return GetValue<TEnum>();
    }

    /// <summary>
    /// Gets validation hint value by key
    /// </summary>
    /// <param name="key">The hint key</param>
    /// <returns>The hint value, or null if not found</returns>
    public object? GetValidationHint(string key)
    {
        return ValidationHints?.GetValueOrDefault(key);
    }

    /// <summary>
    /// Gets validation hint value as specified type
    /// </summary>
    /// <typeparam name="T">The expected type</typeparam>
    /// <param name="key">The hint key</param>
    /// <returns>The typed hint value, or default if not found</returns>
    public T? GetValidationHint<T>(string key)
    {
        var value = GetValidationHint(key);
        if (value == null)
        {
            return default(T);
        }

        if (value is T directValue)
        {
            return directValue;
        }

        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return default(T);
        }
    }
}
