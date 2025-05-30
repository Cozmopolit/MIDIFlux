using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Provides pattern matching functionality for MIDI System Exclusive (SysEx) messages.
/// Uses exact byte-for-byte comparison for reliable pattern matching.
/// </summary>
public class SysExPatternMatcher
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of SysExPatternMatcher
    /// </summary>
    public SysExPatternMatcher()
    {
        _logger = LoggingHelper.CreateLogger<SysExPatternMatcher>();
    }

    /// <summary>
    /// Checks if a received SysEx message matches the configured pattern
    /// </summary>
    /// <param name="receivedSysEx">The SysEx message received from the MIDI device</param>
    /// <param name="configuredPattern">The SysEx pattern configured for matching</param>
    /// <returns>True if the patterns match exactly, false otherwise</returns>
    public bool Matches(byte[] receivedSysEx, byte[] configuredPattern)
    {
        try
        {
            // Null checks
            if (receivedSysEx == null || configuredPattern == null)
            {
                _logger.LogDebug("SysEx pattern match failed: null input (received={ReceivedNull}, pattern={PatternNull})",
                    receivedSysEx == null, configuredPattern == null);
                return false;
            }

            // Length check - must be exact match
            if (receivedSysEx.Length != configuredPattern.Length)
            {
                _logger.LogDebug("SysEx pattern match failed: length mismatch (received={ReceivedLength}, pattern={PatternLength})",
                    receivedSysEx.Length, configuredPattern.Length);
                return false;
            }

            // Byte-for-byte comparison
            bool matches = receivedSysEx.SequenceEqual(configuredPattern);

            if (matches)
            {
                _logger.LogDebug("SysEx pattern matched: {Length} bytes", receivedSysEx.Length);
            }
            else
            {
                _logger.LogDebug("SysEx pattern match failed: content mismatch");
            }

            return matches;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SysEx pattern matching: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Validates that a SysEx pattern has the correct structure
    /// </summary>
    /// <param name="sysExPattern">The SysEx pattern to validate</param>
    /// <returns>True if the pattern is valid, false otherwise</returns>
    public bool IsValidSysExPattern(byte[] sysExPattern)
    {
        try
        {
            if (sysExPattern == null)
            {
                _logger.LogDebug("SysEx pattern validation failed: null pattern");
                return false;
            }

            // Minimum length check (F0 + at least 1 data byte + F7)
            if (sysExPattern.Length < 3)
            {
                _logger.LogDebug("SysEx pattern validation failed: too short ({Length} bytes, minimum 3)", sysExPattern.Length);
                return false;
            }

            // Must start with F0 (SysEx start)
            if (sysExPattern[0] != 0xF0)
            {
                _logger.LogDebug("SysEx pattern validation failed: does not start with F0 (starts with {FirstByte:X2})", sysExPattern[0]);
                return false;
            }

            // Must end with F7 (SysEx end)
            if (sysExPattern[^1] != 0xF7)
            {
                _logger.LogDebug("SysEx pattern validation failed: does not end with F7 (ends with {LastByte:X2})", sysExPattern[^1]);
                return false;
            }

            // Check that data bytes (between F0 and F7) are valid (0x00-0x7F)
            for (int i = 1; i < sysExPattern.Length - 1; i++)
            {
                if (sysExPattern[i] > 0x7F)
                {
                    _logger.LogDebug("SysEx pattern validation failed: invalid data byte {Byte:X2} at position {Position}",
                        sysExPattern[i], i);
                    return false;
                }
            }

            _logger.LogDebug("SysEx pattern validation successful: {Length} bytes", sysExPattern.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during SysEx pattern validation: {Message}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Formats a SysEx pattern as a hex string for display purposes
    /// </summary>
    /// <param name="sysExPattern">The SysEx pattern to format</param>
    /// <returns>A hex string representation of the pattern</returns>
    public string FormatSysExPattern(byte[] sysExPattern)
    {
        try
        {
            if (sysExPattern == null || sysExPattern.Length == 0)
                return "Empty";

            // Use HexByteConverter for consistent hex formatting
            return HexByteConverter.FormatByteArray(sysExPattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting SysEx pattern: {Message}", ex.Message);
            return "Error";
        }
    }
}
