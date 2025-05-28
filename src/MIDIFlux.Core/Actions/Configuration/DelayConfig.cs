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
        base.IsValid(); // Clear previous errors

        if (Milliseconds <= 0)
        {
            AddValidationError("Milliseconds must be greater than 0");
        }

        return GetValidationErrors().Count == 0;
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
