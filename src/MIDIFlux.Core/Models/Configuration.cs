using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Represents the configuration for a specific MIDI device
/// </summary>
public class MidiDeviceConfiguration
{
    /// <summary>
    /// A unique identifier or name for this input profile
    /// </summary>
    public string InputProfile { get; set; } = string.Empty;

    /// <summary>
    /// The name of the MIDI device
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// The MIDI channels to listen to (null or empty means all channels)
    /// </summary>
    public List<int>? MidiChannels { get; set; }

    /// <summary>
    /// The list of key mappings for this device
    /// </summary>
    public List<KeyMapping> Mappings { get; set; } = new List<KeyMapping>();

    /// <summary>
    /// Mappings for absolute value controls (faders, knobs, etc.) for this device
    /// </summary>
    public List<AbsoluteControlMapping> AbsoluteControlMappings { get; set; } = new List<AbsoluteControlMapping>();

    /// <summary>
    /// Mappings for relative value controls (jog wheels, etc.) for this device
    /// </summary>
    public List<RelativeControlMapping> RelativeControlMappings { get; set; } = new List<RelativeControlMapping>();

    /// <summary>
    /// Mappings for game controller integration (requires ViGEm) for this device
    /// </summary>
    public GameControllerMappings? GameControllerMappings { get; set; }

    /// <summary>
    /// Mappings for CC value ranges to different actions
    /// </summary>
    public List<CCRangeMapping> CCRangeMappings { get; set; } = new List<CCRangeMapping>();

    /// <summary>
    /// Macro mappings for this device
    /// </summary>
    public List<MacroMapping> MacroMappings { get; set; } = new List<MacroMapping>();
}

/// <summary>
/// Represents the configuration for MIDIFlux
/// </summary>
public class Configuration
{
    /// <summary>
    /// List of MIDI device configurations
    /// </summary>
    public List<MidiDeviceConfiguration> MidiDevices { get; set; } = new List<MidiDeviceConfiguration>();

    // For backward compatibility
    [JsonIgnore]
    public string? MidiDeviceName
    {
        get => MidiDevices.FirstOrDefault()?.DeviceName;
        set
        {
            if (value != null && MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { DeviceName = value });
            }
            else if (value != null && MidiDevices.Count > 0)
            {
                MidiDevices[0].DeviceName = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<int>? MidiChannels
    {
        get => MidiDevices.FirstOrDefault()?.MidiChannels;
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { MidiChannels = value });
            }
            else
            {
                MidiDevices[0].MidiChannels = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<KeyMapping> Mappings
    {
        get => MidiDevices.FirstOrDefault()?.Mappings ?? new List<KeyMapping>();
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { Mappings = value });
            }
            else
            {
                MidiDevices[0].Mappings = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<AbsoluteControlMapping> AbsoluteControlMappings
    {
        get => MidiDevices.FirstOrDefault()?.AbsoluteControlMappings ?? new List<AbsoluteControlMapping>();
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { AbsoluteControlMappings = value });
            }
            else
            {
                MidiDevices[0].AbsoluteControlMappings = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<RelativeControlMapping> RelativeControlMappings
    {
        get => MidiDevices.FirstOrDefault()?.RelativeControlMappings ?? new List<RelativeControlMapping>();
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { RelativeControlMappings = value });
            }
            else
            {
                MidiDevices[0].RelativeControlMappings = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public GameControllerMappings? GameControllerMappings
    {
        get => MidiDevices.FirstOrDefault()?.GameControllerMappings;
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { GameControllerMappings = value });
            }
            else
            {
                MidiDevices[0].GameControllerMappings = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<CCRangeMapping> CCRangeMappings
    {
        get => MidiDevices.FirstOrDefault()?.CCRangeMappings ?? new List<CCRangeMapping>();
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { CCRangeMappings = value });
            }
            else
            {
                MidiDevices[0].CCRangeMappings = value;
            }
        }
    }

    // For backward compatibility
    [JsonIgnore]
    public List<MacroMapping> MacroMappings
    {
        get => MidiDevices.FirstOrDefault()?.MacroMappings ?? new List<MacroMapping>();
        set
        {
            if (MidiDevices.Count == 0)
            {
                MidiDevices.Add(new MidiDeviceConfiguration { MacroMappings = value });
            }
            else
            {
                MidiDevices[0].MacroMappings = value;
            }
        }
    }
}
