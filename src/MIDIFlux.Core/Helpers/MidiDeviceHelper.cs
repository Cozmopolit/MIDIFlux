using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Helper class for MIDI device operations
    /// </summary>
    public static class MidiDeviceHelper
    {
        /// <summary>
        /// Finds a MIDI device by name, trying exact match first, then partial match
        /// </summary>
        /// <param name="devices">The list of available MIDI devices</param>
        /// <param name="deviceName">The device name to search for</param>
        /// <param name="logger">The logger to use</param>
        /// <returns>The matching device, or null if not found</returns>
        public static MidiDeviceInfo? FindDeviceByName(List<MidiDeviceInfo> devices, string deviceName, ILogger logger)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                logger.LogWarning("Device name is null or empty, cannot find device");
                return null;
            }

            logger.LogInformation("Looking for configured device: '{DeviceName}'", deviceName);

            // Wildcard "*" should be handled by the caller, not here
            if (deviceName == "*")
            {
                logger.LogWarning("Wildcard device name '*' should be handled by caller, not by FindDeviceByName");
                return null;
            }

            // Try exact match first
            var selectedDevice = devices.FirstOrDefault(d =>
                d.Name.Equals(deviceName, StringComparison.OrdinalIgnoreCase));

            if (selectedDevice != null)
            {
                logger.LogInformation("Using device specified in configuration: {Device}", selectedDevice);
                return selectedDevice;
            }

            // Try a partial match
            logger.LogInformation("Trying partial match for device name: '{DeviceName}'", deviceName);
            selectedDevice = devices.FirstOrDefault(d =>
                d.Name.Contains(deviceName, StringComparison.OrdinalIgnoreCase));

            if (selectedDevice != null)
            {
                logger.LogInformation("Using partially matched device: {Device}", selectedDevice);
                return selectedDevice;
            }

            logger.LogWarning("Configured device '{DeviceName}' not found", deviceName);
            return null;
        }
    }
}
