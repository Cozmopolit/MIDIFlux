using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using System.Collections.Generic;

namespace MIDIFlux.Core.Extensions
{
    /// <summary>
    /// Extension methods for the KeyboardSimulator class
    /// </summary>
    public static class KeyboardSimulatorExtensions
    {
        /// <summary>
        /// Presses and releases a key with optional modifier keys
        /// </summary>
        /// <param name="keyboardSimulator">The keyboard simulator</param>
        /// <param name="keyCode">The virtual key code to press and release</param>
        /// <param name="modifiers">Optional modifier keys to hold while pressing the main key</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool PressAndReleaseKey(this KeyboardSimulator keyboardSimulator, ushort keyCode, List<ushort>? modifiers = null)
        {
            bool success = true;
            modifiers ??= new List<ushort>();

            // Press modifier keys
            foreach (var modifier in modifiers)
            {
                if (!keyboardSimulator.SendKeyDown(modifier))
                {
                    success = false;
                }
            }

            // Press and release the main key
            if (!keyboardSimulator.SendKeyDown(keyCode))
            {
                success = false;
            }

            if (!keyboardSimulator.SendKeyUp(keyCode))
            {
                success = false;
            }

            // Release modifier keys in reverse order
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (!keyboardSimulator.SendKeyUp(modifiers[i]))
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Presses and releases a character key
        /// </summary>
        /// <param name="keyboardSimulator">The keyboard simulator</param>
        /// <param name="key">The character to press and release</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool PressAndReleaseKey(this KeyboardSimulator keyboardSimulator, char key)
        {
            // Convert the character to a virtual key code
            ushort keyCode = VirtualKeyCodeFromChar(key);
            return keyboardSimulator.PressAndReleaseKey(keyCode);
        }

        /// <summary>
        /// Presses a key with optional modifier keys
        /// </summary>
        /// <param name="keyboardSimulator">The keyboard simulator</param>
        /// <param name="keyCode">The virtual key code to press</param>
        /// <param name="modifiers">Optional modifier keys to hold while pressing the main key</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool PressKey(this KeyboardSimulator keyboardSimulator, ushort keyCode, List<ushort>? modifiers = null)
        {
            bool success = true;
            modifiers ??= new List<ushort>();

            // Press modifier keys
            foreach (var modifier in modifiers)
            {
                if (!keyboardSimulator.SendKeyDown(modifier))
                {
                    success = false;
                }
            }

            // Press the main key
            if (!keyboardSimulator.SendKeyDown(keyCode))
            {
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Releases a key and optional modifier keys
        /// </summary>
        /// <param name="keyboardSimulator">The keyboard simulator</param>
        /// <param name="keyCode">The virtual key code to release</param>
        /// <param name="modifiers">Optional modifier keys to release after the main key</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool ReleaseKey(this KeyboardSimulator keyboardSimulator, ushort keyCode, List<ushort>? modifiers = null)
        {
            bool success = true;
            modifiers ??= new List<ushort>();

            // Release the main key
            if (!keyboardSimulator.SendKeyUp(keyCode))
            {
                success = false;
            }

            // Release modifier keys in reverse order
            for (int i = modifiers.Count - 1; i >= 0; i--)
            {
                if (!keyboardSimulator.SendKeyUp(modifiers[i]))
                {
                    success = false;
                }
            }

            return success;
        }

        /// <summary>
        /// Converts a character to a virtual key code
        /// </summary>
        /// <param name="c">The character to convert</param>
        /// <returns>The virtual key code</returns>
        private static ushort VirtualKeyCodeFromChar(char c)
        {
            // Handle numbers
            if (c >= '0' && c <= '9')
            {
                return (ushort)(c - '0' + 0x30);
            }

            // Handle uppercase letters
            if (c >= 'A' && c <= 'Z')
            {
                return (ushort)(c - 'A' + 0x41);
            }

            // Handle lowercase letters (convert to uppercase)
            if (c >= 'a' && c <= 'z')
            {
                return (ushort)(c - 'a' + 0x41);
            }

            // Handle special characters
            switch (c)
            {
                case ' ': return 0x20; // Space
                case '!': return 0x31; // 1
                case '@': return 0x32; // 2
                case '#': return 0x33; // 3
                case '$': return 0x34; // 4
                case '%': return 0x35; // 5
                case '^': return 0x36; // 6
                case '&': return 0x37; // 7
                case '*': return 0x38; // 8
                case '(': return 0x39; // 9
                case ')': return 0x30; // 0
                case '-': return 0xBD; // Minus
                case '_': return 0xBD; // Minus
                case '=': return 0xBB; // Equals
                case '+': return 0xBB; // Equals
                case '[': return 0xDB; // Left bracket
                case '{': return 0xDB; // Left bracket
                case ']': return 0xDD; // Right bracket
                case '}': return 0xDD; // Right bracket
                case '\\': return 0xDC; // Backslash
                case '|': return 0xDC; // Backslash
                case ';': return 0xBA; // Semicolon
                case ':': return 0xBA; // Semicolon
                case '\'': return 0xDE; // Quote
                case '"': return 0xDE; // Quote
                case ',': return 0xBC; // Comma
                case '<': return 0xBC; // Comma
                case '.': return 0xBE; // Period
                case '>': return 0xBE; // Period
                case '/': return 0xBF; // Slash
                case '?': return 0xBF; // Slash
                default: return 0x20; // Default to space
            }
        }
    }
}
