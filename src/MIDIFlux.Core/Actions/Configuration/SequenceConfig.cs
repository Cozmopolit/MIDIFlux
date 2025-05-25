namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for SequenceAction (macro) actions.
/// Represents executing a sequence of actions in order.
/// </summary>
public class SequenceConfig : ActionConfig
{
    /// <summary>
    /// How to handle errors during sequence execution
    /// </summary>
    public SequenceErrorHandling ErrorHandling { get; set; } = SequenceErrorHandling.ContinueOnError;

    /// <summary>
    /// The list of sub-actions to execute in sequence.
    /// Each action includes a description to clearly denote which key/action is being performed.
    /// </summary>
    public List<ActionConfig> SubActions { get; set; } = new();

    /// <summary>
    /// Initializes a new instance of SequenceConfig
    /// </summary>
    public SequenceConfig()
    {
        Type = ActionType.SequenceAction;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        return SubActions.Count > 0 && 
               Enum.IsDefined(typeof(SequenceErrorHandling), ErrorHandling) &&
               SubActions.All(action => action.IsValid());
    }

    /// <summary>
    /// Gets validation error messages for this configuration
    /// </summary>
    public override List<string> GetValidationErrors()
    {
        var errors = new List<string>();
        
        if (SubActions.Count == 0)
        {
            errors.Add("SequenceConfig must contain at least one sub-action");
        }
        
        if (!Enum.IsDefined(typeof(SequenceErrorHandling), ErrorHandling))
        {
            errors.Add($"Invalid error handling mode: {ErrorHandling}");
        }
        
        // Validate each sub-action
        for (int i = 0; i < SubActions.Count; i++)
        {
            var subActionErrors = SubActions[i].GetValidationErrors();
            foreach (var error in subActionErrors)
            {
                errors.Add($"Sub-action {i + 1}: {error}");
            }
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
            
        return $"Sequence ({SubActions.Count} actions)";
    }
}
