using System;
using System.Collections.Generic;
using System.Linq;
using MIDIFlux.Core.Models;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// View model for a MIDI device configuration
    /// </summary>
    public class MidiDeviceViewModel
    {
        private readonly MidiDeviceConfiguration _deviceConfiguration;

        /// <summary>
        /// Gets the underlying device configuration
        /// </summary>
        public MidiDeviceConfiguration DeviceConfiguration => _deviceConfiguration;

        /// <summary>
        /// Gets or sets the input profile name
        /// </summary>
        public string InputProfile
        {
            get => _deviceConfiguration.InputProfile;
            set => _deviceConfiguration.InputProfile = value;
        }

        /// <summary>
        /// Gets or sets the device name
        /// </summary>
        public string DeviceName
        {
            get => _deviceConfiguration.DeviceName;
            set => _deviceConfiguration.DeviceName = value;
        }

        /// <summary>
        /// Gets or sets the MIDI channels
        /// </summary>
        public List<int>? MidiChannels
        {
            get => _deviceConfiguration.MidiChannels;
            set => _deviceConfiguration.MidiChannels = value;
        }

        /// <summary>
        /// Gets a display string for the MIDI channels
        /// </summary>
        public string ChannelsDisplay
        {
            get
            {
                if (_deviceConfiguration.MidiChannels == null || _deviceConfiguration.MidiChannels.Count == 0)
                {
                    return "All";
                }
                else
                {
                    return string.Join(", ", _deviceConfiguration.MidiChannels);
                }
            }
        }

        /// <summary>
        /// Gets the key mappings
        /// </summary>
        public List<KeyMapping> Mappings => _deviceConfiguration.Mappings;

        /// <summary>
        /// Gets the absolute control mappings
        /// </summary>
        public List<AbsoluteControlMapping> AbsoluteControlMappings => _deviceConfiguration.AbsoluteControlMappings;

        /// <summary>
        /// Gets the relative control mappings
        /// </summary>
        public List<RelativeControlMapping> RelativeControlMappings => _deviceConfiguration.RelativeControlMappings;

        /// <summary>
        /// Gets the CC range mappings
        /// </summary>
        public List<CCRangeMapping> CCRangeMappings => _deviceConfiguration.CCRangeMappings;

        /// <summary>
        /// Gets the game controller mappings
        /// </summary>
        public GameControllerMappings? GameControllerMappings => _deviceConfiguration.GameControllerMappings;

        /// <summary>
        /// Creates a new instance of the MidiDeviceViewModel class
        /// </summary>
        /// <param name="deviceConfiguration">The device configuration to wrap</param>
        public MidiDeviceViewModel(MidiDeviceConfiguration deviceConfiguration)
        {
            _deviceConfiguration = deviceConfiguration ?? throw new ArgumentNullException(nameof(deviceConfiguration));
        }
    }
}

