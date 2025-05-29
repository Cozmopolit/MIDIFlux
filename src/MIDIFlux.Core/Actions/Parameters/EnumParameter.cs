namespace MIDIFlux.Core.Actions.Parameters;

/// <summary>
/// Specialized parameter class for enum values with additional metadata.
/// Provides type-safe enum handling with display name support.
/// </summary>
public class EnumParameter : Parameter
{
    /// <summary>
    /// The name of the enum type for reflection-based operations
    /// </summary>
    public new string EnumTypeName { get; set; } = string.Empty;

    /// <summary>
    /// The currently selected option display name
    /// </summary>
    public string SelectedOption { get; set; } = string.Empty;

    /// <summary>
    /// The currently selected enum value
    /// </summary>
    public object? SelectedValue { get; set; }

    /// <summary>
    /// Initializes a new EnumParameter
    /// </summary>
    public EnumParameter() : base()
    {
        Type = ParameterType.Enum;
    }

    /// <summary>
    /// Initializes a new EnumParameter with enum definition
    /// </summary>
    /// <param name="displayName">Display name for UI</param>
    /// <param name="enumDefinition">The enum definition with options and values</param>
    /// <param name="initialValue">The initial selected value</param>
    public EnumParameter(string displayName, EnumDefinition enumDefinition, object? initialValue = null) : base()
    {
        Type = ParameterType.Enum;
        DisplayName = displayName;
        EnumDefinition = enumDefinition;

        if (initialValue != null)
        {
            SetSelectedValue(initialValue);
        }
        else if (enumDefinition.Values.Length > 0)
        {
            SetSelectedValue(enumDefinition.Values[0]);
        }
    }

    /// <summary>
    /// Sets the selected value and updates the corresponding option name
    /// </summary>
    /// <param name="value">The enum value to select</param>
    public void SetSelectedValue(object value)
    {
        SelectedValue = value;
        Value = value;

        if (EnumDefinition != null)
        {
            SelectedOption = EnumDefinition.GetDisplayName(value);
        }
        else
        {
            SelectedOption = value?.ToString() ?? string.Empty;
        }
    }

    /// <summary>
    /// Sets the selected option by display name and updates the corresponding value
    /// </summary>
    /// <param name="optionName">The display name to select</param>
    /// <returns>True if the option was found and set, false otherwise</returns>
    public bool SetSelectedOption(string optionName)
    {
        if (EnumDefinition == null)
        {
            return false;
        }

        var value = EnumDefinition.GetValue(optionName);
        if (value != null)
        {
            SetSelectedValue(value);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the selected value as the specified enum type
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <returns>The typed enum value</returns>
    public TEnum GetSelectedValue<TEnum>() where TEnum : Enum
    {
        if (SelectedValue == null)
        {
            throw new InvalidOperationException($"EnumParameter '{DisplayName}' has no selected value");
        }

        if (SelectedValue is TEnum enumValue)
        {
            return enumValue;
        }

        try
        {
            return (TEnum)Enum.ToObject(typeof(TEnum), SelectedValue);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"EnumParameter '{DisplayName}' value '{SelectedValue}' cannot be converted to enum type {typeof(TEnum).Name}", ex);
        }
    }
}
