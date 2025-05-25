using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for MIDI input validation with consistent error messages
    /// </summary>
    public static class MidiValidationHelper
    {
        /// <summary>
        /// Validates a MIDI input number (0-127 range)
        /// </summary>
        /// <param name="inputNumber">The input number to validate</param>
        /// <param name="inputType">The type of input (for error messages)</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMidiInputNumber(
            int inputNumber,
            string inputType,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            if (inputNumber < 0 || inputNumber > 127)
            {
                var errorMessage = $"MIDI {inputType} number must be between 0 and 127. Current value: {inputNumber}";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a MIDI channel (1-16 range, or null for any channel)
        /// </summary>
        /// <param name="channel">The channel to validate</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMidiChannel(
            int? channel,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            if (channel.HasValue && (channel.Value < 1 || channel.Value > 16))
            {
                var errorMessage = $"MIDI channel must be between 1 and 16, or null for any channel. Current value: {channel}";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a MIDI velocity value (0-127 range)
        /// </summary>
        /// <param name="velocity">The velocity to validate</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMidiVelocity(
            int velocity,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            if (velocity < 0 || velocity > 127)
            {
                var errorMessage = $"MIDI velocity must be between 0 and 127. Current value: {velocity}";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a MIDI control change value (0-127 range)
        /// </summary>
        /// <param name="value">The CC value to validate</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMidiControlValue(
            int value,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            if (value < 0 || value > 127)
            {
                var errorMessage = $"MIDI control change value must be between 0 and 127. Current value: {value}";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a complete MIDI input configuration
        /// </summary>
        /// <param name="inputNumber">The input number (note/CC)</param>
        /// <param name="channel">The MIDI channel</param>
        /// <param name="inputType">The input type for error messages</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if all validation passes, false otherwise</returns>
        public static bool ValidateMidiInput(
            int inputNumber,
            int? channel,
            string inputType,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            // Validate input number
            if (!ValidateMidiInputNumber(inputNumber, inputType, logger, parent))
                return false;

            // Validate channel
            if (!ValidateMidiChannel(channel, logger, parent))
                return false;

            return true;
        }

        /// <summary>
        /// Validates a range of MIDI values (for conditional actions)
        /// </summary>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateMidiValueRange(
            int minValue,
            int maxValue,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            // Validate individual values
            if (!ValidateMidiControlValue(minValue, logger, parent))
                return false;

            if (!ValidateMidiControlValue(maxValue, logger, parent))
                return false;

            // Validate range logic
            if (minValue > maxValue)
            {
                var errorMessage = $"Minimum value ({minValue}) cannot be greater than maximum value ({maxValue})";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets validation errors for a MIDI input configuration without showing dialogs
        /// </summary>
        /// <param name="inputNumber">The input number</param>
        /// <param name="channel">The MIDI channel</param>
        /// <param name="inputType">The input type</param>
        /// <returns>List of validation error messages</returns>
        public static List<string> GetMidiInputValidationErrors(
            int inputNumber,
            int? channel,
            string inputType)
        {
            var errors = new List<string>();

            // Check input number
            if (inputNumber < 0 || inputNumber > 127)
            {
                errors.Add($"MIDI {inputType} number must be between 0 and 127 (current: {inputNumber})");
            }

            // Check channel
            if (channel.HasValue && (channel.Value < 1 || channel.Value > 16))
            {
                errors.Add($"MIDI channel must be between 1 and 16 (current: {channel})");
            }

            return errors;
        }

        /// <summary>
        /// Checks if a MIDI input number is valid without showing error dialogs
        /// </summary>
        /// <param name="inputNumber">The input number to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsMidiInputNumberValid(int inputNumber)
        {
            return inputNumber >= 0 && inputNumber <= 127;
        }

        /// <summary>
        /// Checks if a MIDI channel is valid without showing error dialogs
        /// </summary>
        /// <param name="channel">The channel to check</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool IsMidiChannelValid(int? channel)
        {
            return !channel.HasValue || (channel.Value >= 1 && channel.Value <= 16);
        }

        /// <summary>
        /// Validates a generic numeric range (min <= max)
        /// </summary>
        /// <param name="minValue">Minimum value</param>
        /// <param name="maxValue">Maximum value</param>
        /// <param name="fieldName">Name of the field being validated</param>
        /// <param name="logger">Logger for error reporting</param>
        /// <param name="parent">Parent window for error dialogs</param>
        /// <returns>True if valid, false otherwise</returns>
        public static bool ValidateNumericRange(
            int minValue,
            int maxValue,
            string fieldName,
            ILogger logger,
            System.Windows.Forms.IWin32Window? parent = null)
        {
            if (minValue > maxValue)
            {
                var errorMessage = $"{fieldName}: Minimum value ({minValue}) cannot be greater than maximum value ({maxValue})";
                ApplicationErrorHandler.ShowError(errorMessage, "Validation Error", logger, null, parent);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Constants for MIDI validation
        /// </summary>
        public static class Constants
        {
            public const int MinMidiValue = 0;
            public const int MaxMidiValue = 127;
            public const int MinMidiChannel = 1;
            public const int MaxMidiChannel = 16;
        }
    }
}
