namespace MIDIFlux.Core.Models;

/// <summary>
/// Encoding methods for relative MIDI values (jog wheels, endless encoders, etc.)
/// </summary>
public enum RelativeValueEncoding
{
    /// <summary>
    /// Sign-magnitude encoding: values 1-63 are positive, 65-127 are negative
    /// </summary>
    SignMagnitude = 0,

    /// <summary>
    /// Two's complement encoding: values 1-64 are positive, 127-65 are negative
    /// </summary>
    TwosComplement = 1,

    /// <summary>
    /// Binary offset encoding: 64 is zero, above is positive, below is negative
    /// </summary>
    BinaryOffset = 2
}
