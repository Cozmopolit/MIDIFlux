using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// View model for a unified MIDI device configuration
    /// </summary>
    public class DeviceViewModel
    {
        private readonly DeviceConfig _deviceConfig;

        /// <summary>
        /// Gets the underlying device configuration
        /// </summary>
        public DeviceConfig DeviceConfig => _deviceConfig;

        /// <summary>
        /// Gets or sets the input profile name
        /// </summary>
        public string InputProfile
        {
            get => _deviceConfig.InputProfile;
            set => _deviceConfig.InputProfile = value;
        }

        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName
        {
            get => _deviceConfig.DeviceName ?? string.Empty;
            set => _deviceConfig.DeviceName = value;
        }

        /// <summary>
        /// Gets the number of mappings in this device
        /// </summary>
        public int MappingCount => _deviceConfig.Mappings.Count;

        /// <summary>
        /// Gets the mappings for this device
        /// </summary>
        public List<MappingConfigEntry> Mappings => _deviceConfig.Mappings;

        /// <summary>
        /// Creates a new instance of the DeviceViewModel class
        /// </summary>
        /// <param name="deviceConfig">The device configuration to wrap</param>
        public DeviceViewModel(DeviceConfig deviceConfig)
        {
            _deviceConfig = deviceConfig ?? throw new ArgumentNullException(nameof(deviceConfig));
        }

        /// <summary>
        /// Adds a new mapping to this device
        /// </summary>
        /// <param name="mapping">The mapping to add</param>
        public void AddMapping(MappingConfigEntry mapping)
        {
            if (mapping == null) throw new ArgumentNullException(nameof(mapping));
            _deviceConfig.Mappings.Add(mapping);
        }

        /// <summary>
        /// Removes a mapping from this device
        /// </summary>
        /// <param name="mapping">The mapping to remove</param>
        /// <returns>True if the mapping was removed, false if it wasn't found</returns>
        public bool RemoveMapping(MappingConfigEntry mapping)
        {
            if (mapping == null) return false;
            return _deviceConfig.Mappings.Remove(mapping);
        }

        /// <summary>
        /// Gets a summary of the action types used in this device
        /// </summary>
        /// <returns>A string describing the action types</returns>
        public string GetActionTypesSummary()
        {
            if (_deviceConfig.Mappings.Count == 0)
                return "No mappings";

            var actionTypes = _deviceConfig.Mappings
                .Select(m => m.Action?.GetType().Name ?? "Unknown")
                .GroupBy(t => t)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            return string.Join(", ", actionTypes);
        }

        /// <summary>
        /// Gets a summary of the input types used in this device
        /// </summary>
        /// <returns>A string describing the input types</returns>
        public string GetInputTypesSummary()
        {
            if (_deviceConfig.Mappings.Count == 0)
                return "No mappings";

            var inputTypes = _deviceConfig.Mappings
                .GroupBy(m => m.InputType)
                .Select(g => $"{g.Key}: {g.Count()}")
                .ToList();

            return string.Join(", ", inputTypes);
        }

        /// <summary>
        /// Creates a copy of this device view model
        /// </summary>
        /// <returns>A new DeviceViewModel with copied data</returns>
        public DeviceViewModel Clone()
        {
            var clonedConfig = new DeviceConfig
            {
                DeviceName = _deviceConfig.DeviceName,
                InputProfile = _deviceConfig.InputProfile + " (Copy)",
                Mappings = new List<MappingConfigEntry>()
            };

            // Deep copy the mappings
            foreach (var mapping in _deviceConfig.Mappings)
            {
                var clonedMapping = new MappingConfigEntry
                {
                    Id = Guid.NewGuid().ToString(),
                    Description = mapping.Description,
                    InputType = mapping.InputType,
                    Note = mapping.Note,
                    ControlNumber = mapping.ControlNumber,
                    Channel = mapping.Channel,
                    IsEnabled = mapping.IsEnabled,
                    Action = mapping.Action // Note: This is a shallow copy of the action config
                };

                clonedConfig.Mappings.Add(clonedMapping);
            }

            return new DeviceViewModel(clonedConfig);
        }

        /// <summary>
        /// Returns a string representation of this device
        /// </summary>
        /// <returns>A string describing this device</returns>
        public override string ToString()
        {
            return $"{InputProfile} ({DeviceName}) - {MappingCount} mappings";
        }
    }
}
