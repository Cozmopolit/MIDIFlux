using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MIDIFlux.GUI.Services.Import.Parsers
{
    /// <summary>
    /// Parser for MIDIKey2Key keyboard action strings
    /// </summary>
    public class KeyboardStringParser
    {
        // Mapping from MIDIKey2Key key names to Windows Virtual Key Codes
        private static readonly Dictionary<string, int> KeyNameToVirtualKeyCode = new(StringComparer.OrdinalIgnoreCase)
        {
            // Letters
            {"A", 0x41}, {"B", 0x42}, {"C", 0x43}, {"D", 0x44}, {"E", 0x45}, {"F", 0x46}, {"G", 0x47},
            {"H", 0x48}, {"I", 0x49}, {"J", 0x4A}, {"K", 0x4B}, {"L", 0x4C}, {"M", 0x4D}, {"N", 0x4E},
            {"O", 0x4F}, {"P", 0x50}, {"Q", 0x51}, {"R", 0x52}, {"S", 0x53}, {"T", 0x54}, {"U", 0x55},
            {"V", 0x56}, {"W", 0x57}, {"X", 0x58}, {"Y", 0x59}, {"Z", 0x5A},

            // Numbers
            {"0", 0x30}, {"1", 0x31}, {"2", 0x32}, {"3", 0x33}, {"4", 0x34},
            {"5", 0x35}, {"6", 0x36}, {"7", 0x37}, {"8", 0x38}, {"9", 0x39},

            // Function keys
            {"F1", 0x70}, {"F2", 0x71}, {"F3", 0x72}, {"F4", 0x73}, {"F5", 0x74}, {"F6", 0x75},
            {"F7", 0x76}, {"F8", 0x77}, {"F9", 0x78}, {"F10", 0x79}, {"F11", 0x7A}, {"F12", 0x7B},

            // Special keys
            {"SPACE", 0x20}, {"ENTER", 0x0D}, {"TAB", 0x09}, {"ESCAPE", 0x1B}, {"ESC", 0x1B},
            {"BACKSPACE", 0x08}, {"DELETE", 0x2E}, {"INSERT", 0x2D}, {"HOME", 0x24}, {"END", 0x23},
            {"PAGEUP", 0x21}, {"PAGEDOWN", 0x22}, {"UP", 0x26}, {"DOWN", 0x28}, {"LEFT", 0x25}, {"RIGHT", 0x27},

            // Modifier keys
            {"CTRL", 0x11}, {"ALT", 0x12}, {"SHIFT", 0x10}, {"WIN", 0x5B}, {"WINDOWS", 0x5B},
            {"LCTRL", 0xA2}, {"RCTRL", 0xA3}, {"LALT", 0xA4}, {"RALT", 0xA5},
            {"LSHIFT", 0xA0}, {"RSHIFT", 0xA1}, {"LWIN", 0x5B}, {"RWIN", 0x5C},

            // Numpad
            {"NUMPAD0", 0x60}, {"NUMPAD1", 0x61}, {"NUMPAD2", 0x62}, {"NUMPAD3", 0x63}, {"NUMPAD4", 0x64},
            {"NUMPAD5", 0x65}, {"NUMPAD6", 0x66}, {"NUMPAD7", 0x67}, {"NUMPAD8", 0x68}, {"NUMPAD9", 0x69},
            {"MULTIPLY", 0x6A}, {"ADD", 0x6B}, {"SUBTRACT", 0x6D}, {"DECIMAL", 0x6E}, {"DIVIDE", 0x6F},

            // Other common keys
            {"CAPSLOCK", 0x14}, {"NUMLOCK", 0x90}, {"SCROLLLOCK", 0x91}, {"PAUSE", 0x13}, {"BREAK", 0x13},
            {"PRINTSCREEN", 0x2C}, {"APPS", 0x5D}, {"MENU", 0x5D},

            // Punctuation and symbols
            {"SEMICOLON", 0xBA}, {"EQUALS", 0xBB}, {"COMMA", 0xBC}, {"MINUS", 0xBD}, {"PERIOD", 0xBE},
            {"SLASH", 0xBF}, {"GRAVE", 0xC0}, {"LBRACKET", 0xDB}, {"BACKSLASH", 0xDC}, {"RBRACKET", 0xDD},
            {"QUOTE", 0xDE}
        };

        /// <summary>
        /// Parses a MIDIKey2Key keyboard string into key sequence information
        /// </summary>
        /// <param name="keyboardString">The keyboard string to parse (e.g., "CTRL+C", "ALT+TAB")</param>
        /// <returns>Parsed keyboard sequence information</returns>
        public KeyboardSequenceInfo ParseKeyboardString(string keyboardString)
        {
            if (string.IsNullOrWhiteSpace(keyboardString))
            {
                return new KeyboardSequenceInfo
                {
                    IsValid = false,
                    ErrorMessage = "Keyboard string cannot be null or empty"
                };
            }

            try
            {
                var result = new KeyboardSequenceInfo
                {
                    OriginalString = keyboardString.Trim()
                };

                // Split by + to get individual keys
                var keyParts = result.OriginalString.Split('+')
                    .Select(k => k.Trim().ToUpperInvariant())
                    .Where(k => !string.IsNullOrEmpty(k))
                    .ToList();

                if (keyParts.Count == 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "No valid keys found in keyboard string";
                    return result;
                }

                // Parse each key part
                foreach (var keyPart in keyParts)
                {
                    if (!KeyNameToVirtualKeyCode.TryGetValue(keyPart, out var virtualKeyCode))
                    {
                        result.IsValid = false;
                        result.ErrorMessage = $"Unknown key name: {keyPart}";
                        return result;
                    }

                    var keyInfo = new KeyInfo
                    {
                        KeyName = keyPart,
                        VirtualKeyCode = virtualKeyCode,
                        IsModifier = IsModifierKey(virtualKeyCode)
                    };

                    result.Keys.Add(keyInfo);
                }

                // Separate modifiers from main keys
                result.ModifierKeys = result.Keys.Where(k => k.IsModifier).ToList();
                result.MainKeys = result.Keys.Where(k => !k.IsModifier).ToList();

                // Validate the combination
                if (result.MainKeys.Count == 0)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Keyboard combination must contain at least one non-modifier key";
                    return result;
                }

                if (result.MainKeys.Count > 1)
                {
                    result.IsValid = false;
                    result.ErrorMessage = "Keyboard combination can only contain one main key (multiple modifiers are allowed)";
                    return result;
                }

                result.IsValid = true;
                return result;
            }
            catch (Exception ex)
            {
                return new KeyboardSequenceInfo
                {
                    IsValid = false,
                    ErrorMessage = $"Failed to parse keyboard string: {ex.Message}",
                    OriginalString = keyboardString
                };
            }
        }

        /// <summary>
        /// Checks if a virtual key code represents a modifier key
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code to check</param>
        /// <returns>True if it's a modifier key, false otherwise</returns>
        private static bool IsModifierKey(int virtualKeyCode)
        {
            return virtualKeyCode switch
            {
                0x10 or 0x11 or 0x12 or // SHIFT, CTRL, ALT
                0x5B or 0x5C or         // LWIN, RWIN
                0xA0 or 0xA1 or         // LSHIFT, RSHIFT
                0xA2 or 0xA3 or         // LCTRL, RCTRL
                0xA4 or 0xA5            // LALT, RALT
                => true,
                _ => false
            };
        }

        /// <summary>
        /// Validates that a keyboard string can be parsed
        /// </summary>
        /// <param name="keyboardString">The keyboard string to validate</param>
        /// <returns>True if valid, false otherwise</returns>
        public bool IsValidKeyboardString(string keyboardString)
        {
            var result = ParseKeyboardString(keyboardString);
            return result.IsValid;
        }

        /// <summary>
        /// Gets all supported key names
        /// </summary>
        /// <returns>List of supported key names</returns>
        public static List<string> GetSupportedKeyNames()
        {
            return KeyNameToVirtualKeyCode.Keys.ToList();
        }
    }

    /// <summary>
    /// Information about a parsed keyboard sequence
    /// </summary>
    public class KeyboardSequenceInfo
    {
        /// <summary>
        /// Gets or sets whether the parsing was successful
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the error message if parsing failed
        /// </summary>
        public string ErrorMessage { get; set; } = "";

        /// <summary>
        /// Gets or sets the original keyboard string
        /// </summary>
        public string OriginalString { get; set; } = "";

        /// <summary>
        /// Gets or sets all keys in the sequence
        /// </summary>
        public List<KeyInfo> Keys { get; set; } = new();

        /// <summary>
        /// Gets or sets the modifier keys (CTRL, ALT, SHIFT, WIN)
        /// </summary>
        public List<KeyInfo> ModifierKeys { get; set; } = new();

        /// <summary>
        /// Gets or sets the main keys (non-modifiers)
        /// </summary>
        public List<KeyInfo> MainKeys { get; set; } = new();
    }

    /// <summary>
    /// Information about a single key
    /// </summary>
    public class KeyInfo
    {
        /// <summary>
        /// Gets or sets the key name (e.g., "CTRL", "A")
        /// </summary>
        public string KeyName { get; set; } = "";

        /// <summary>
        /// Gets or sets the Windows virtual key code
        /// </summary>
        public int VirtualKeyCode { get; set; }

        /// <summary>
        /// Gets or sets whether this is a modifier key
        /// </summary>
        public bool IsModifier { get; set; }
    }
}
