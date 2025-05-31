using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Provides pattern matching functionality for MIDI System Exclusive (SysEx) messages.
/// Supports both exact byte-for-byte comparison and wildcard pattern matching.
/// Wildcard bytes (0xFF) match any received byte value.
/// </summary>
public class SysExPatternMatcher
{
    private readonly ILogger _logger;

    /// <summary>
    /// The byte value used to represent wildcards in SysEx patterns.
    /// 0xFF is used because it's not a valid SysEx data byte (data bytes must be 0x00-0x7F).
    /// </summary>
    public const byte WildcardByte = 0xFF;

    /// <summary>
    /// Initializes a new instance of SysExPatternMatcher
    /// </summary>
    public SysExPatternMatcher()
    {
        _logger = LoggingHelper.CreateLogger<SysExPatternMatcher>();
    }

    /// <summary>
    /// Checks if a received SysEx message matches the configured pattern.
    /// Supports wildcard matching where 0xFF bytes in the pattern match any received byte.
    /// </summary>
    /// <param name="receivedSysEx">The SysEx message received from the MIDI device</param>
    /// <param name="configuredPattern">The SysEx pattern configured for matching (may contain wildcards)</param>
    /// <returns>True if the patterns match (including wildcard matches), false otherwise</returns>
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

            // Check if pattern contains wildcards
            bool hasWildcards = configuredPattern.Contains(WildcardByte);

            bool matches;
            if (hasWildcards)
            {
                // Wildcard matching - compare byte by byte, treating 0xFF as wildcard
                matches = MatchesWithWildcards(receivedSysEx, configuredPattern);

                if (matches)
                {
                    _logger.LogDebug("SysEx pattern matched with wildcards: {Length} bytes", receivedSysEx.Length);
                }
                else
                {
                    _logger.LogDebug("SysEx pattern match failed: wildcard pattern mismatch");
                }
            }
            else
            {
                // Exact matching - use fast SequenceEqual
                matches = receivedSysEx.SequenceEqual(configuredPattern);

                if (matches)
                {
                    _logger.LogDebug("SysEx pattern matched exactly: {Length} bytes", receivedSysEx.Length);
                }
                else
                {
                    _logger.LogDebug("SysEx pattern match failed: exact content mismatch");
                }
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
    /// Validates that a SysEx pattern has the correct structure.
    /// Allows wildcard bytes (0xFF) in data positions.
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

            // Must start with F0 (SysEx start) - no wildcards allowed
            if (sysExPattern[0] != 0xF0)
            {
                _logger.LogDebug("SysEx pattern validation failed: does not start with F0 (starts with {FirstByte:X2})", sysExPattern[0]);
                return false;
            }

            // Must end with F7 (SysEx end) - no wildcards allowed
            if (sysExPattern[^1] != 0xF7)
            {
                _logger.LogDebug("SysEx pattern validation failed: does not end with F7 (ends with {LastByte:X2})", sysExPattern[^1]);
                return false;
            }

            // Check that data bytes (between F0 and F7) are valid (0x00-0x7F or wildcard 0xFF)
            for (int i = 1; i < sysExPattern.Length - 1; i++)
            {
                byte dataByte = sysExPattern[i];
                if (dataByte > 0x7F && dataByte != WildcardByte)
                {
                    _logger.LogDebug("SysEx pattern validation failed: invalid data byte {Byte:X2} at position {Position} (must be 0x00-0x7F or wildcard 0xFF)",
                        dataByte, i);
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
    /// Formats a SysEx pattern as a hex string for display purposes.
    /// Wildcard bytes (0xFF) are displayed as "XX" for clarity.
    /// </summary>
    /// <param name="sysExPattern">The SysEx pattern to format</param>
    /// <returns>A hex string representation of the pattern with wildcards shown as "XX"</returns>
    public string FormatSysExPattern(byte[] sysExPattern)
    {
        try
        {
            if (sysExPattern == null || sysExPattern.Length == 0)
                return "Empty";

            // Format with wildcard support
            var hexStrings = sysExPattern.Select(b => b == WildcardByte ? "XX" : b.ToString("X2"));
            return string.Join(" ", hexStrings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error formatting SysEx pattern: {Message}", ex.Message);
            return "Error";
        }
    }

    /// <summary>
    /// Performs wildcard-aware pattern matching between received SysEx data and a pattern.
    /// Wildcard bytes (0xFF) in the pattern match any received byte.
    /// </summary>
    /// <param name="receivedSysEx">The received SysEx message</param>
    /// <param name="configuredPattern">The pattern to match against (may contain wildcards)</param>
    /// <returns>True if the pattern matches, false otherwise</returns>
    private bool MatchesWithWildcards(byte[] receivedSysEx, byte[] configuredPattern)
    {
        // Length should already be validated by caller, but double-check for safety
        if (receivedSysEx.Length != configuredPattern.Length)
            return false;

        // Compare byte by byte, treating 0xFF as wildcard
        for (int i = 0; i < receivedSysEx.Length; i++)
        {
            byte patternByte = configuredPattern[i];
            byte receivedByte = receivedSysEx[i];

            // If pattern byte is wildcard, it matches any received byte
            if (patternByte == WildcardByte)
                continue;

            // Otherwise, must be exact match
            if (patternByte != receivedByte)
                return false;
        }

        return true;
    }
}
