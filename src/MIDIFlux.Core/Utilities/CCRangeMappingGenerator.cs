using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Utilities;

/// <summary>
/// Utility class for generating CC range mappings
/// </summary>
public static class CCRangeMappingGenerator
{
    /// <summary>
    /// Generates a CC range mapping from a key sequence string
    /// </summary>
    /// <param name="keySequence">The sequence of keys to map (e.g., "1234567890")</param>
    /// <param name="deviceName">The MIDI device name</param>
    /// <param name="controllerNumber">The CC controller number</param>
    /// <param name="channel">The MIDI channel (null for all channels)</param>
    /// <param name="minValue">The minimum CC value to consider (default: 0)</param>
    /// <param name="maxValue">The maximum CC value to consider (default: 127)</param>
    /// <returns>A configured CCRangeMapping object</returns>
    public static CCRangeMapping GenerateFromKeySequence(
        string keySequence,
        string deviceName,
        int controllerNumber,
        int? channel = null,
        int minValue = 0,
        int maxValue = 127)
    {
        if (string.IsNullOrEmpty(keySequence))
        {
            throw new ArgumentException("Key sequence cannot be empty", nameof(keySequence));
        }

        var mapping = new CCRangeMapping
        {
            ControlNumber = controllerNumber,
            Channel = channel,
            HandlerType = "CCRange",
            Description = $"Key sequence mapping for '{keySequence}'"
        };

        // Generate ranges for each key in the sequence
        mapping.Ranges = GenerateEvenlyDistributedRanges(keySequence, minValue, maxValue);

        return mapping;
    }

    /// <summary>
    /// Generates evenly distributed ranges for a key sequence
    /// </summary>
    /// <param name="keySequence">The sequence of keys to map</param>
    /// <param name="minValue">The minimum CC value</param>
    /// <param name="maxValue">The maximum CC value</param>
    /// <returns>A list of CC value ranges</returns>
    public static List<CCValueRange> GenerateEvenlyDistributedRanges(
        string keySequence,
        int minValue = 0,
        int maxValue = 127)
    {
        var ranges = new List<CCValueRange>();
        int keyCount = keySequence.Length;
        
        if (keyCount == 0)
        {
            return ranges;
        }

        // Calculate the size of each range
        double rangeSize = (maxValue - minValue + 1.0) / keyCount;

        // Create a range for each key
        for (int i = 0; i < keyCount; i++)
        {
            char key = keySequence[i];
            
            // Calculate the min and max values for this range
            int rangeMin = minValue + (int)Math.Floor(i * rangeSize);
            int rangeMax = (i == keyCount - 1) 
                ? maxValue  // Last range goes to the max value
                : minValue + (int)Math.Floor((i + 1) * rangeSize) - 1;
            
            // Create the range
            var range = new CCValueRange
            {
                MinValue = rangeMin,
                MaxValue = rangeMax,
                Action = new CCRangeAction
                {
                    Type = CCRangeActionType.KeyPress,
                    Key = key.ToString()
                }
            };
            
            ranges.Add(range);
        }

        return ranges;
    }

    /// <summary>
    /// Creates a CC range mapping with custom ranges
    /// </summary>
    /// <param name="deviceName">The MIDI device name</param>
    /// <param name="controllerNumber">The CC controller number</param>
    /// <param name="ranges">The list of CC value ranges</param>
    /// <param name="channel">The MIDI channel (null for all channels)</param>
    /// <param name="description">Optional description</param>
    /// <returns>A configured CCRangeMapping object</returns>
    public static CCRangeMapping CreateWithCustomRanges(
        string deviceName,
        int controllerNumber,
        List<CCValueRange> ranges,
        int? channel = null,
        string? description = null)
    {
        return new CCRangeMapping
        {
            ControlNumber = controllerNumber,
            Channel = channel,
            HandlerType = "CCRange",
            Ranges = ranges,
            Description = description ?? $"Custom CC range mapping for controller {controllerNumber}"
        };
    }
}
