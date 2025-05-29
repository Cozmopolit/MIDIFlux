namespace MIDIFlux.Core.Actions.Parameters;

/// <summary>
/// Defines the types of parameters supported by the unified action parameter system.
/// Each type corresponds to a specific UI control and validation approach.
/// </summary>
public enum ParameterType
{
    /// <summary>
    /// Integer numeric parameter with optional min/max validation
    /// </summary>
    Integer,

    /// <summary>
    /// String text parameter with optional length validation
    /// </summary>
    String,

    /// <summary>
    /// Boolean checkbox parameter
    /// </summary>
    Boolean,

    /// <summary>
    /// Enumeration dropdown parameter with predefined options
    /// </summary>
    Enum,

    /// <summary>
    /// Byte array parameter for binary data (e.g., SysEx data)
    /// </summary>
    ByteArray,

    /// <summary>
    /// Single sub-action for simple nested action structures
    /// </summary>
    SubAction,

    /// <summary>
    /// List of sub-actions for complex nested action structures
    /// </summary>
    SubActionList,

    /// <summary>
    /// List of value conditions for ConditionalAction (MinValue, MaxValue, Action, Description)
    /// </summary>
    ValueConditionList
}
