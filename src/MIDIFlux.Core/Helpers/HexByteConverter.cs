using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Utility class for converting between hex strings and byte arrays
    /// Provides centralized, consistent hex parsing and formatting functionality
    /// </summary>
    public static class HexByteConverter
    {
        /// <summary>
        /// Parses a hex string into a byte array
        /// </summary>
        /// <param name="hexString">The hex string to parse (e.g., "F0 43 12 00 F7" or "F0431200F7")</param>
        /// <param name="options">Optional parsing options</param>
        /// <returns>Byte array representation of the hex string</returns>
        /// <exception cref="ArgumentException">Thrown when the hex string is invalid</exception>
        public static byte[] ParseHexString(string hexString, HexParseOptions? options = null)
        {
            if (string.IsNullOrEmpty(hexString))
                return Array.Empty<byte>();

            options ??= new HexParseOptions();

            try
            {
                // Clean and normalize the input
                var cleanHex = CleanHexString(hexString, options);

                // Validate length if required
                if (options.RequireEvenLength && cleanHex.Length % 2 != 0)
                {
                    throw new ArgumentException($"Invalid hex string length: {hexString} (must be even number of hex characters)");
                }

                // Handle odd length by padding with leading zero if allowed
                if (!options.RequireEvenLength && cleanHex.Length % 2 != 0)
                {
                    cleanHex = "0" + cleanHex;
                }

                // Convert to byte array
                var bytes = new byte[cleanHex.Length / 2];
                for (int i = 0; i < bytes.Length; i++)
                {
                    var hexByte = cleanHex.Substring(i * 2, 2);
                    bytes[i] = Convert.ToByte(hexByte, 16);
                }

                return bytes;
            }
            catch (Exception ex) when (!(ex is ArgumentException))
            {
                throw new ArgumentException($"Failed to parse hex string '{hexString}': {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tries to parse a hex string into a byte array without throwing exceptions
        /// </summary>
        /// <param name="hexString">The hex string to parse</param>
        /// <param name="bytes">The resulting byte array if parsing succeeds</param>
        /// <param name="options">Optional parsing options</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        public static bool TryParseHexString(string hexString, out byte[] bytes, HexParseOptions? options = null)
        {
            bytes = Array.Empty<byte>();

            try
            {
                bytes = ParseHexString(hexString, options);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats a byte array as a hex string
        /// </summary>
        /// <param name="bytes">The byte array to format</param>
        /// <param name="options">Optional formatting options</param>
        /// <returns>Hex string representation of the byte array</returns>
        public static string FormatByteArray(byte[] bytes, HexFormatOptions? options = null)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            options ??= new HexFormatOptions();

            var format = options.UpperCase ? "X2" : "x2";
            var hexStrings = bytes.Select(b => b.ToString(format));

            return options.UseSpaces ? string.Join(options.Separator, hexStrings) : string.Concat(hexStrings);
        }

        /// <summary>
        /// Validates whether a string is a valid hex string
        /// </summary>
        /// <param name="hexString">The string to validate</param>
        /// <param name="options">Optional parsing options</param>
        /// <returns>True if the string is valid hex, false otherwise</returns>
        public static bool IsValidHexString(string hexString, HexParseOptions? options = null)
        {
            return TryParseHexString(hexString, out _, options);
        }

        /// <summary>
        /// Cleans and normalizes a hex string according to the specified options
        /// </summary>
        /// <param name="hexString">The hex string to clean</param>
        /// <param name="options">Parsing options</param>
        /// <returns>Cleaned hex string</returns>
        private static string CleanHexString(string hexString, HexParseOptions options)
        {
            var result = hexString;

            // Remove allowed separators
            if (options.AllowSpaces)
            {
                result = result.Replace(" ", "").Replace("\t", "").Replace("\n", "").Replace("\r", "");
            }

            if (options.AllowDashes)
            {
                result = result.Replace("-", "");
            }

            // Handle case sensitivity
            if (!options.CaseSensitive)
            {
                result = result.ToUpperInvariant();
            }

            return result;
        }
    }

    /// <summary>
    /// Options for parsing hex strings
    /// </summary>
    public class HexParseOptions
    {
        /// <summary>
        /// Whether to allow spaces and whitespace characters as separators
        /// </summary>
        public bool AllowSpaces { get; set; } = true;

        /// <summary>
        /// Whether to allow dashes as separators
        /// </summary>
        public bool AllowDashes { get; set; } = true;

        /// <summary>
        /// Whether to require an even number of hex characters
        /// </summary>
        public bool RequireEvenLength { get; set; } = true;

        /// <summary>
        /// Whether hex parsing is case sensitive
        /// </summary>
        public bool CaseSensitive { get; set; } = false;
    }

    /// <summary>
    /// Options for formatting byte arrays as hex strings
    /// </summary>
    public class HexFormatOptions
    {
        /// <summary>
        /// Whether to include spaces between hex bytes
        /// </summary>
        public bool UseSpaces { get; set; } = true;

        /// <summary>
        /// Whether to use uppercase hex characters
        /// </summary>
        public bool UpperCase { get; set; } = true;

        /// <summary>
        /// The separator to use between hex bytes when UseSpaces is true
        /// </summary>
        public string Separator { get; set; } = " ";
    }
}
