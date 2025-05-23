using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for validating MIDIFlux configurations
    /// </summary>
    public static class ConfigurationValidator
    {
        /// <summary>
        /// Validates a configuration
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <param name="logger">The logger to use</param>
        /// <returns>A validation result containing any errors or warnings</returns>
        public static ValidationResult Validate(Configuration config, ILogger logger)
        {
            var result = new ValidationResult();

            if (config == null)
            {
                result.AddError("Configuration cannot be null");
                return result;
            }

            // Check if the configuration has any devices
            if (config.MidiDevices.Count == 0)
            {
                result.AddError("Configuration must have at least one MIDI device");
            }

            // Validate each device
            foreach (var device in config.MidiDevices)
            {
                ValidateDevice(device, result);
            }

            // Check for global issues across devices
            ValidateGlobalConfiguration(config, result);

            // Log validation results
            if (result.HasErrors)
            {
                logger.LogError("Configuration validation failed with {ErrorCount} errors and {WarningCount} warnings",
                    result.Errors.Count, result.Warnings.Count);

                foreach (var error in result.Errors)
                {
                    logger.LogError("Validation error: {Error}", error);
                }

                foreach (var warning in result.Warnings)
                {
                    logger.LogWarning("Validation warning: {Warning}", warning);
                }
            }
            else if (result.HasWarnings)
            {
                logger.LogWarning("Configuration validation succeeded with {WarningCount} warnings",
                    result.Warnings.Count);

                foreach (var warning in result.Warnings)
                {
                    logger.LogWarning("Validation warning: {Warning}", warning);
                }
            }
            else
            {
                logger.LogInformation("Configuration validation succeeded");
            }

            return result;
        }

        /// <summary>
        /// Validates global aspects of the configuration
        /// </summary>
        /// <param name="config">The configuration to validate</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateGlobalConfiguration(Configuration config, ValidationResult result)
        {
            // Check for duplicate device names
            var deviceNames = new HashSet<string>();
            foreach (var device in config.MidiDevices)
            {
                if (!string.IsNullOrWhiteSpace(device.DeviceName))
                {
                    if (!deviceNames.Add(device.DeviceName))
                    {
                        result.AddWarning($"Duplicate device name: '{device.DeviceName}'. This may cause issues with device identification.");
                    }
                }
            }

            // Check for duplicate input profiles
            var inputProfiles = new HashSet<string>();
            foreach (var device in config.MidiDevices)
            {
                if (!string.IsNullOrWhiteSpace(device.InputProfile))
                {
                    if (!inputProfiles.Add(device.InputProfile))
                    {
                        result.AddError($"Duplicate input profile: '{device.InputProfile}'. Each device must have a unique input profile.");
                    }
                }
            }

            // Check for potential key conflicts across devices
            var keyMappings = new Dictionary<ushort, List<string>>();
            foreach (var device in config.MidiDevices)
            {
                foreach (var mapping in device.Mappings)
                {
                    if (!keyMappings.TryGetValue(mapping.VirtualKeyCode, out var devices))
                    {
                        devices = new List<string>();
                        keyMappings[mapping.VirtualKeyCode] = devices;
                    }

                    devices.Add($"{device.DeviceName} (Note {mapping.MidiNote})");
                }
            }

            // Report devices that map to the same key
            foreach (var keyMapping in keyMappings)
            {
                if (keyMapping.Value.Count > 1)
                {
                    result.AddWarning($"Multiple mappings to key code {keyMapping.Key} from: {string.Join(", ", keyMapping.Value)}");
                }
            }
        }

        /// <summary>
        /// Validates a MIDI device configuration
        /// </summary>
        /// <param name="device">The device configuration to validate</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateDevice(MidiDeviceConfiguration device, ValidationResult result)
        {
            if (device == null)
            {
                result.AddError("Device configuration cannot be null");
                return;
            }

            // Check if the device has a name
            if (string.IsNullOrWhiteSpace(device.DeviceName))
            {
                result.AddError($"Device '{device.InputProfile}' must have a name");
            }

            // Check if the device has an input profile
            if (string.IsNullOrWhiteSpace(device.InputProfile))
            {
                result.AddError($"Device '{device.DeviceName}' must have an input profile");
            }

            // Check if the device has any mappings
            if (device.Mappings.Count == 0 &&
                device.AbsoluteControlMappings.Count == 0 &&
                device.RelativeControlMappings.Count == 0 &&
                device.GameControllerMappings == null)
            {
                result.AddWarning($"Device '{device.InputProfile}' has no mappings");
            }

            // Validate key mappings
            ValidateKeyMappings(device, result);

            // Validate absolute control mappings
            ValidateAbsoluteControlMappings(device, result);

            // Validate relative control mappings
            ValidateRelativeControlMappings(device, result);

            // Validate game controller mappings
            if (device.GameControllerMappings != null)
            {
                ValidateGameControllerMappings(device, result);
            }
        }

        /// <summary>
        /// Validates key mappings for a device
        /// </summary>
        /// <param name="device">The device configuration</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateKeyMappings(MidiDeviceConfiguration device, ValidationResult result)
        {
            // Check for duplicate MIDI notes in key mappings
            var midiNotes = new Dictionary<int, KeyMapping>();
            foreach (var mapping in device.Mappings)
            {
                // Validate the mapping itself
                if (mapping.MidiNote < 0 || mapping.MidiNote > 127)
                {
                    result.AddError($"Device '{device.InputProfile}' has invalid MIDI note {mapping.MidiNote}. MIDI notes must be between 0 and 127.");
                }

                if (mapping.VirtualKeyCode == 0)
                {
                    result.AddWarning($"Device '{device.InputProfile}' has mapping for MIDI note {mapping.MidiNote} with virtual key code 0, which may not work as expected.");
                }

                // Check for duplicate MIDI notes
                if (midiNotes.TryGetValue(mapping.MidiNote, out var existingMapping))
                {
                    result.AddError($"Device '{device.InputProfile}' has duplicate mapping for MIDI note {mapping.MidiNote}");
                }
                else
                {
                    midiNotes[mapping.MidiNote] = mapping;
                }
            }
        }

        /// <summary>
        /// Validates absolute control mappings for a device
        /// </summary>
        /// <param name="device">The device configuration</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateAbsoluteControlMappings(MidiDeviceConfiguration device, ValidationResult result)
        {
            // Check for duplicate control numbers in absolute control mappings
            var absoluteControls = new Dictionary<int, AbsoluteControlMapping>();
            foreach (var mapping in device.AbsoluteControlMappings)
            {
                // Validate the mapping itself
                if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                {
                    result.AddError($"Device '{device.InputProfile}' has invalid control number {mapping.ControlNumber} for absolute control. Control numbers must be between 0 and 127.");
                }

                // Check for duplicate control numbers
                if (absoluteControls.TryGetValue(mapping.ControlNumber, out var existingMapping))
                {
                    result.AddError($"Device '{device.InputProfile}' has duplicate mapping for absolute control {mapping.ControlNumber}");
                }
                else
                {
                    absoluteControls[mapping.ControlNumber] = mapping;
                }
            }
        }

        /// <summary>
        /// Validates relative control mappings for a device
        /// </summary>
        /// <param name="device">The device configuration</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateRelativeControlMappings(MidiDeviceConfiguration device, ValidationResult result)
        {
            // Check for duplicate control numbers in relative control mappings
            var relativeControls = new Dictionary<int, RelativeControlMapping>();
            foreach (var mapping in device.RelativeControlMappings)
            {
                // Validate the mapping itself
                if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                {
                    result.AddError($"Device '{device.InputProfile}' has invalid control number {mapping.ControlNumber} for relative control. Control numbers must be between 0 and 127.");
                }

                // Check for duplicate control numbers
                if (relativeControls.TryGetValue(mapping.ControlNumber, out var existingMapping))
                {
                    result.AddError($"Device '{device.InputProfile}' has duplicate mapping for relative control {mapping.ControlNumber}");
                }
                else
                {
                    relativeControls[mapping.ControlNumber] = mapping;
                }
            }
        }

        /// <summary>
        /// Validates game controller mappings for a device
        /// </summary>
        /// <param name="device">The device configuration</param>
        /// <param name="result">The validation result to update</param>
        private static void ValidateGameControllerMappings(MidiDeviceConfiguration device, ValidationResult result)
        {
            var gameControllerMappings = device.GameControllerMappings;
            if (gameControllerMappings == null)
            {
                return;
            }

            // Validate button mappings
            if (gameControllerMappings.Buttons != null)
            {
                foreach (var mapping in gameControllerMappings.Buttons)
                {
                    if (mapping.MidiNote < 0 || mapping.MidiNote > 127)
                    {
                        result.AddError($"Device '{device.InputProfile}' has invalid MIDI note {mapping.MidiNote} for game controller button. MIDI notes must be between 0 and 127.");
                    }
                }
            }

            // Validate axis mappings
            if (gameControllerMappings.Axes != null)
            {
                foreach (var mapping in gameControllerMappings.Axes)
                {
                    if (mapping.ControlNumber < 0 || mapping.ControlNumber > 127)
                    {
                        result.AddError($"Device '{device.InputProfile}' has invalid control number {mapping.ControlNumber} for game controller axis. Control numbers must be between 0 and 127.");
                    }
                }
            }
        }
    }

    // Note: We now use the ValidationResult class from MIDIFlux.GUI.Models
}

