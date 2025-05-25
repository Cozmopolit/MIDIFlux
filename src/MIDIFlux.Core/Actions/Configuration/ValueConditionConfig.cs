namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Represents a single value condition for conditional actions.
/// Defines a MIDI value range and the action to execute when the value falls within that range.
/// </summary>
public class ValueConditionConfig
{
    /// <summary>
    /// The minimum MIDI value for this condition (inclusive)
    /// </summary>
    public int MinValue { get; set; }

    /// <summary>
    /// The maximum MIDI value for this condition (inclusive)
    /// </summary>
    public int MaxValue { get; set; }

    /// <summary>
    /// The action to execute when the MIDI value falls within this range
    /// </summary>
    public ActionConfig Action { get; set; } = new KeyPressReleaseConfig();

    /// <summary>
    /// Optional description of this condition
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Validates the condition configuration
    /// </summary>
    /// <returns>True if the configuration is valid, false otherwise</returns>
    public bool IsValid()
    {
        return MinValue >= 0 && 
               MaxValue <= 127 && 
               MinValue <= MaxValue && 
               Action.IsValid();
    }

    /// <summary>
    /// Gets validation error messages for this condition
    /// </summary>
    /// <returns>A list of validation error messages, empty if valid</returns>
    public List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (MinValue < 0)
        {
            errors.Add("MinValue must be >= 0");
        }
        
        if (MaxValue > 127)
        {
            errors.Add("MaxValue must be <= 127");
        }
        
        if (MinValue > MaxValue)
        {
            errors.Add("MinValue must be <= MaxValue");
        }
        
        var actionErrors = Action.GetValidationErrors();
        foreach (var error in actionErrors)
        {
            errors.Add($"Action: {error}");
        }
        
        return errors;
    }

    /// <summary>
    /// Checks if a MIDI value falls within this condition's range
    /// </summary>
    /// <param name="midiValue">The MIDI value to check</param>
    /// <returns>True if the value is within range, false otherwise</returns>
    public bool IsInRange(int midiValue)
    {
        return midiValue >= MinValue && midiValue <= MaxValue;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;
            
        var range = MinValue == MaxValue ? $"= {MinValue}" : $"{MinValue}-{MaxValue}";
        return $"Value {range} → {Action}";
    }
}
