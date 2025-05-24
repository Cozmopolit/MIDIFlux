using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for populating MIDI device combo boxes with consistent behavior
    /// </summary>
    public static class MidiDeviceComboBoxHelper
    {
        /// <summary>
        /// Populates a combo box with available MIDI devices, including an "Any Device" option
        /// </summary>
        /// <param name="comboBox">The combo box to populate</param>
        /// <param name="midiManager">The MIDI manager to get devices from</param>
        /// <param name="logger">Logger for error handling</param>
        /// <param name="includeAnyDevice">Whether to include "Any Device" option</param>
        /// <param name="selectedDeviceName">Device name to select, or null for first item</param>
        public static void PopulateDeviceComboBox(
            ComboBox comboBox, 
            MidiManager? midiManager, 
            ILogger logger,
            bool includeAnyDevice = true,
            string? selectedDeviceName = null)
        {
            try
            {
                // Clear existing items
                comboBox.Items.Clear();

                // Add "Any Device" option if requested
                if (includeAnyDevice)
                {
                    comboBox.Items.Add("Any Device");
                }

                // Get available devices from MIDI manager
                if (midiManager != null)
                {
                    var devices = midiManager.GetAvailableDevices();
                    foreach (var device in devices)
                    {
                        // Avoid duplicates
                        if (!comboBox.Items.Contains(device.Name))
                        {
                            comboBox.Items.Add(device.Name);
                        }
                    }

                    logger.LogDebug("Populated device combo box with {DeviceCount} devices", devices.Count);
                }
                else
                {
                    logger.LogWarning("MidiManager is null, cannot populate device combo box");
                }

                // Select the specified device or default
                SelectDevice(comboBox, selectedDeviceName, includeAnyDevice);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error populating device combo box");
                
                // Ensure combo box has at least one item
                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add(includeAnyDevice ? "Any Device" : "No devices available");
                    comboBox.SelectedIndex = 0;
                    comboBox.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Populates a combo box with available MIDI devices for detection dialog
        /// </summary>
        /// <param name="comboBox">The combo box to populate</param>
        /// <param name="midiManager">The MIDI manager to get devices from</param>
        /// <param name="logger">Logger for error handling</param>
        public static void PopulateDetectionDeviceComboBox(
            ComboBox comboBox, 
            MidiManager midiManager, 
            ILogger logger)
        {
            try
            {
                // Clear existing items
                comboBox.Items.Clear();

                // Get available devices
                var devices = midiManager.GetAvailableDevices();
                
                // Add each device as MidiDeviceInfo object
                foreach (var device in devices)
                {
                    comboBox.Items.Add(device);
                }

                // Select first device if available
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                    comboBox.Enabled = true;
                }
                else
                {
                    // No devices available
                    comboBox.Items.Add("No MIDI devices found");
                    comboBox.SelectedIndex = 0;
                    comboBox.Enabled = false;
                }

                logger.LogDebug("Populated detection device combo box with {DeviceCount} devices", devices.Count);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error populating detection device combo box");
                
                // Ensure combo box has at least one item
                if (comboBox.Items.Count == 0)
                {
                    comboBox.Items.Add("Error loading devices");
                    comboBox.SelectedIndex = 0;
                    comboBox.Enabled = false;
                }
            }
        }

        /// <summary>
        /// Selects a device in the combo box by name
        /// </summary>
        /// <param name="comboBox">The combo box</param>
        /// <param name="deviceName">The device name to select</param>
        /// <param name="includeAnyDevice">Whether "Any Device" option is available</param>
        private static void SelectDevice(ComboBox comboBox, string? deviceName, bool includeAnyDevice)
        {
            if (string.IsNullOrEmpty(deviceName))
            {
                // Select first item (usually "Any Device" or first actual device)
                if (comboBox.Items.Count > 0)
                {
                    comboBox.SelectedIndex = 0;
                }
                return;
            }

            // Try to find exact match
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                var item = comboBox.Items[i]?.ToString();
                if (item == deviceName)
                {
                    comboBox.SelectedIndex = i;
                    return;
                }
            }

            // If not found and device name is not "Any Device", add it
            if (deviceName != "Any Device")
            {
                comboBox.Items.Add(deviceName);
                comboBox.SelectedItem = deviceName;
            }
            else if (includeAnyDevice)
            {
                // Select "Any Device" if available
                comboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Gets the selected device name from a combo box, handling "Any Device" option
        /// </summary>
        /// <param name="comboBox">The combo box</param>
        /// <returns>Device name or null for "Any Device"</returns>
        public static string? GetSelectedDeviceName(ComboBox comboBox)
        {
            var selectedItem = comboBox.SelectedItem?.ToString();
            return selectedItem == "Any Device" ? null : selectedItem;
        }

        /// <summary>
        /// Refreshes the device list in a combo box while preserving selection
        /// </summary>
        /// <param name="comboBox">The combo box to refresh</param>
        /// <param name="midiManager">The MIDI manager</param>
        /// <param name="logger">Logger for error handling</param>
        /// <param name="includeAnyDevice">Whether to include "Any Device" option</param>
        public static void RefreshDeviceComboBox(
            ComboBox comboBox, 
            MidiManager? midiManager, 
            ILogger logger,
            bool includeAnyDevice = true)
        {
            // Remember current selection
            var currentSelection = comboBox.SelectedItem?.ToString();
            
            // Refresh the list
            PopulateDeviceComboBox(comboBox, midiManager, logger, includeAnyDevice, currentSelection);
        }
    }
}
