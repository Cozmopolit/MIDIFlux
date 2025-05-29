namespace MIDIFlux.Core.Actions;

/// <summary>
/// Static helper class providing common validation methods for action parameters.
/// Centralizes validation logic to ensure consistency across all action types.
/// </summary>
public static class ActionHelper
{
    /// <summary>
    /// Validates that an integer value is within the specified range (inclusive)
    /// </summary>
    /// <param name="value">The value to validate</param>
    /// <param name="min">The minimum allowed value (inclusive)</param>
    /// <param name="max">The maximum allowed value (inclusive)</param>
    /// <returns>True if the value is within range, false otherwise</returns>
    public static bool IsIntegerInRange(int value, int min, int max)
    {
        return value >= min && value <= max;
    }

    /// <summary>
    /// Validates that a string is not null or empty
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <returns>True if the string is not null or empty, false otherwise</returns>
    public static bool IsStringNotEmpty(string? value)
    {
        return !string.IsNullOrEmpty(value);
    }

    /// <summary>
    /// Validates that a string length is within the specified range
    /// </summary>
    /// <param name="value">The string to validate</param>
    /// <param name="minLength">The minimum allowed length (inclusive)</param>
    /// <param name="maxLength">The maximum allowed length (inclusive)</param>
    /// <returns>True if the string length is within range, false otherwise</returns>
    public static bool IsStringLengthInRange(string? value, int minLength, int maxLength)
    {
        if (value == null)
        {
            return minLength <= 0;
        }

        return value.Length >= minLength && value.Length <= maxLength;
    }

    /// <summary>
    /// Validates that a virtual key code is valid (greater than 0)
    /// </summary>
    /// <param name="virtualKeyCode">The virtual key code to validate</param>
    /// <returns>True if the virtual key code is valid, false otherwise</returns>
    public static bool IsValidVirtualKeyCode(ushort virtualKeyCode)
    {
        return virtualKeyCode > 0;
    }

    /// <summary>
    /// Validates that a delay value is positive
    /// </summary>
    /// <param name="milliseconds">The delay in milliseconds to validate</param>
    /// <returns>True if the delay is positive, false otherwise</returns>
    public static bool IsValidDelay(int milliseconds)
    {
        return milliseconds > 0;
    }

    /// <summary>
    /// Validates that an optional delay value is either null or positive
    /// </summary>
    /// <param name="milliseconds">The optional delay in milliseconds to validate</param>
    /// <returns>True if the delay is null or positive, false otherwise</returns>
    public static bool IsValidOptionalDelay(int? milliseconds)
    {
        return milliseconds == null || milliseconds > 0;
    }

    /// <summary>
    /// Validates that an enum value is defined in the specified enum type
    /// </summary>
    /// <typeparam name="TEnum">The enum type</typeparam>
    /// <param name="value">The value to validate</param>
    /// <returns>True if the value is defined in the enum, false otherwise</returns>
    public static bool IsValidEnumValue<TEnum>(TEnum value) where TEnum : Enum
    {
        return Enum.IsDefined(typeof(TEnum), value);
    }

    /// <summary>
    /// Validates that a MIDI value is in the valid range (0-127)
    /// </summary>
    /// <param name="midiValue">The MIDI value to validate</param>
    /// <returns>True if the MIDI value is valid, false otherwise</returns>
    public static bool IsValidMidiValue(int midiValue)
    {
        return IsIntegerInRange(midiValue, 0, 127);
    }

    /// <summary>
    /// Validates that a MIDI channel is in the valid range (1-16)
    /// </summary>
    /// <param name="channel">The MIDI channel to validate</param>
    /// <returns>True if the MIDI channel is valid, false otherwise</returns>
    public static bool IsValidMidiChannel(int channel)
    {
        return IsIntegerInRange(channel, 1, 16);
    }

    /// <summary>
    /// Validates that a collection is not null and not empty
    /// </summary>
    /// <typeparam name="T">The collection element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <returns>True if the collection is not null and not empty, false otherwise</returns>
    public static bool IsCollectionNotEmpty<T>(ICollection<T>? collection)
    {
        return collection != null && collection.Count > 0;
    }

    /// <summary>
    /// Validates that a collection count is within the specified range
    /// </summary>
    /// <typeparam name="T">The collection element type</typeparam>
    /// <param name="collection">The collection to validate</param>
    /// <param name="minCount">The minimum allowed count (inclusive)</param>
    /// <param name="maxCount">The maximum allowed count (inclusive)</param>
    /// <returns>True if the collection count is within range, false otherwise</returns>
    public static bool IsCollectionCountInRange<T>(ICollection<T>? collection, int minCount, int maxCount)
    {
        if (collection == null)
        {
            return minCount <= 0;
        }

        return IsIntegerInRange(collection.Count, minCount, maxCount);
    }
}
