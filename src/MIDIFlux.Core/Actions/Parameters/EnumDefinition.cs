namespace MIDIFlux.Core.Actions.Parameters;

/// <summary>
/// Defines the available options and values for an enum parameter.
/// Each action class will contain its own enum definitions to maintain encapsulation.
/// </summary>
public class EnumDefinition
{
    /// <summary>
    /// Array of display names for the enum options (shown in UI).
    /// Immutable after construction to preserve the Options/Values length invariant.
    /// </summary>
    public string[] Options { get; init; } = Array.Empty<string>();

    /// <summary>
    /// Array of corresponding values for the enum options (stored in parameter).
    /// Immutable after construction to preserve the Options/Values length invariant.
    /// </summary>
    public object[] Values { get; init; } = Array.Empty<object>();

    /// <summary>
    /// Initializes an empty EnumDefinition
    /// </summary>
    public EnumDefinition()
    {
    }

    /// <summary>
    /// Initializes an EnumDefinition with options and values
    /// </summary>
    /// <param name="options">Display names for UI</param>
    /// <param name="values">Corresponding values</param>
    /// <exception cref="ArgumentException">Thrown when options and values arrays have different lengths</exception>
    public EnumDefinition(string[] options, object[] values)
    {
        if (options.Length != values.Length)
        {
            throw new ArgumentException("Options and Values arrays must have the same length");
        }

        Options = options;
        Values = values;
    }

    /// <summary>
    /// Creates an EnumDefinition from a .NET enum type using reflection
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>EnumDefinition with enum names and values</returns>
    public static EnumDefinition FromEnum<TEnum>() where TEnum : Enum
    {
        var enumType = typeof(TEnum);
        var names = Enum.GetNames(enumType);
        var values = Enum.GetValues(enumType).Cast<object>().ToArray();

        return new EnumDefinition(names, values);
    }

    /// <summary>
    /// Creates an EnumDefinition with custom display names for enum values
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <param name="displayNames">Custom display names (must match enum count)</param>
    /// <returns>EnumDefinition with custom names and enum values</returns>
    public static EnumDefinition FromEnumWithDisplayNames<TEnum>(string[] displayNames) where TEnum : Enum
    {
        var enumType = typeof(TEnum);
        var values = Enum.GetValues(enumType).Cast<object>().ToArray();

        if (displayNames.Length != values.Length)
        {
            throw new ArgumentException($"Display names count ({displayNames.Length}) must match enum value count ({values.Length})");
        }

        return new EnumDefinition(displayNames, values);
    }

    /// <summary>
    /// Gets the display name for a given value
    /// </summary>
    /// <param name="value">The value to find</param>
    /// <returns>The display name, or the value's string representation if not found</returns>
    public string GetDisplayName(object value)
    {
        for (int i = 0; i < Values.Length; i++)
        {
            // Direct equality check works when both are the same type
            if (Values[i].Equals(value))
            {
                return Options[i];
            }

            // Handle boxed enum vs integer comparison:
            // After JSON deserialization, enum values may arrive as integers (e.g., 0 instead of MyEnum.Value).
            // Boxed enum.Equals(int) returns false even when the underlying values match.
            if (Values[i] is Enum && value is IConvertible &&
                Convert.ToInt32(Values[i]) == Convert.ToInt32(value))
            {
                return Options[i];
            }
        }

        return value?.ToString() ?? "Unknown";
    }

    /// <summary>
    /// Gets the value for a given display name
    /// </summary>
    /// <param name="displayName">The display name to find</param>
    /// <returns>The corresponding value, or null if not found</returns>
    public object? GetValue(string displayName)
    {
        for (int i = 0; i < Options.Length; i++)
        {
            if (Options[i].Equals(displayName, StringComparison.OrdinalIgnoreCase))
            {
                return Values[i];
            }
        }

        return null;
    }
}
