namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Defines how errors should be handled in sequence actions (macros).
/// Used by SequenceConfig to control error behavior during action execution.
/// </summary>
public enum SequenceErrorHandling
{
    /// <summary>
    /// Continue executing remaining actions even if one fails
    /// </summary>
    ContinueOnError,

    /// <summary>
    /// Stop executing the sequence if any action fails
    /// </summary>
    StopOnError
}
