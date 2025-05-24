namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for ConditionalAction actions.
/// Represents executing different actions based on MIDI value ranges (fader-to-buttons).
/// </summary>
public class ConditionalConfig : UnifiedActionConfig
{
    /// <summary>
    /// The list of value conditions and their associated actions
    /// </summary>
    public List<ValueConditionConfig> Conditions { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of ConditionalConfig
    /// </summary>
    public ConditionalConfig()
    {
        Type = UnifiedActionType.ConditionalAction;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return Conditions.Count > 0 && 
               Conditions.All(condition => condition.IsValid()) &&
               !HasOverlappingRanges();
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (Conditions.Count == 0)
        {
            errors.Add("ConditionalConfig must contain at least one condition");
        }
        
        // Validate each condition
        for (int i = 0; i < Conditions.Count; i++)
        {
            var conditionErrors = Conditions[i].GetValidationErrors();
            foreach (var error in conditionErrors)
            {
                errors.Add($"Condition {i + 1}: {error}");
            }
        }
        
        // Check for overlapping ranges
        if (HasOverlappingRanges())
        {
            errors.Add("Conditions have overlapping value ranges");
        }
        
        return errors;
    }

    /// <summary>
    /// Checks if any conditions have overlapping value ranges
    /// </summary>
    /// <returns>True if there are overlapping ranges, false otherwise</returns>
    private bool HasOverlappingRanges()
    {
        for (int i = 0; i < Conditions.Count; i++)
        {
            for (int j = i + 1; j < Conditions.Count; j++)
            {
                var condition1 = Conditions[i];
                var condition2 = Conditions[j];
                
                // Check if ranges overlap
                if (condition1.MinValue <= condition2.MaxValue && condition2.MinValue <= condition1.MaxValue)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Finds the condition that matches the given MIDI value
    /// </summary>
    /// <param name="midiValue">The MIDI value to match</param>
    /// <returns>The matching condition, or null if no match found</returns>
    public ValueConditionConfig? FindMatchingCondition(int midiValue)
    {
        return Conditions.FirstOrDefault(condition => condition.IsInRange(midiValue));
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;
            
        return $"Conditional ({Conditions.Count} conditions)";
    }
}
