namespace MIDIFlux.Core.Models;

/// <summary>
/// Base class for MIDI control mappings
/// </summary>
public abstract class ControlMapping
{
    /// <summary>
    /// The MIDI control number
    /// </summary>
    public int ControlNumber { get; set; }

    /// <summary>
    /// The MIDI channel (0-15, null for all channels)
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// The type of handler to use
    /// </summary>
    public string HandlerType { get; set; } = string.Empty;
}

/// <summary>
/// Mapping for absolute value controls (0-127)
/// </summary>
public class AbsoluteControlMapping : ControlMapping
{
    /// <summary>
    /// The minimum value to consider (default: 0)
    /// </summary>
    public int MinValue { get; set; } = 0;

    /// <summary>
    /// The maximum value to consider (default: 127)
    /// </summary>
    public int MaxValue { get; set; } = 127;

    /// <summary>
    /// Whether to invert the value (default: false)
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// Additional parameters for the handler
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Optional description of this mapping
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Mapping for relative value controls (increments/decrements)
/// </summary>
public class RelativeControlMapping : ControlMapping
{
    /// <summary>
    /// The sensitivity multiplier (default: 1)
    /// </summary>
    public int Sensitivity { get; set; } = 1;

    /// <summary>
    /// Whether to invert the direction (default: false)
    /// </summary>
    public bool Invert { get; set; } = false;

    /// <summary>
    /// The encoding method for relative values
    /// </summary>
    public RelativeValueEncoding Encoding { get; set; } = RelativeValueEncoding.SignMagnitude;

    /// <summary>
    /// Additional parameters for the handler
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Optional description of this mapping
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// Encoding methods for relative values
/// </summary>
public enum RelativeValueEncoding
{
    /// <summary>
    /// Values 1-63 are positive, 65-127 are negative (common in many controllers)
    /// </summary>
    SignMagnitude,

    /// <summary>
    /// Values 1-64 are positive, 127-65 are negative (two's complement)
    /// </summary>
    TwosComplement,

    /// <summary>
    /// 64 is zero, above is positive, below is negative (binary offset)
    /// </summary>
    BinaryOffset
}
