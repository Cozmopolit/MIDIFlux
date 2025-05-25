namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for Delay actions.
/// Represents waiting for a specified amount of time.
/// </summary>
public class DelayConfig : ActionConfig
{
    /// <summary>
    /// The number of milliseconds to wait
    /// </summary>
    public int Milliseconds { get; set; }

    /// <summary>
    /// Initializes a new instance of DelayConfig
    /// </summary>
    public DelayConfig()
    {
        Type = ActionType.Delay;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return Milliseconds > 0;
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (Milliseconds <= 0)
        {
            errors.Add("Milliseconds must be greater than 0");
        }
        
        return errors;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;
            
        return $"Delay {Milliseconds}ms";
    }
}
