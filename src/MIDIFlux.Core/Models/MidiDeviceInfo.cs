using NAudio.Midi;

namespace MIDIFlux.Core.Models;

/// <summary>
/// Information about a MIDI device (input or output)
/// </summary>
public class MidiDeviceInfo
{
    /// <summary>
    /// The device ID
    /// </summary>
    public int DeviceId { get; set; }

    /// <summary>
    /// The device name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The device manufacturer
    /// </summary>
    public string Manufacturer { get; set; } = string.Empty;

    /// <summary>
    /// The device driver version
    /// </summary>
    public string DriverVersion { get; set; } = string.Empty;

    /// <summary>
    /// Whether the device is currently connected
    /// </summary>
    public bool IsConnected { get; set; }

    /// <summary>
    /// The last time the device was seen
    /// </summary>
    public DateTime LastSeen { get; set; } = DateTime.Now;

    /// <summary>
    /// Whether this device supports input (receiving MIDI messages)
    /// </summary>
    public bool SupportsInput { get; set; } = true;

    /// <summary>
    /// Whether this device supports output (sending MIDI messages)
    /// </summary>
    public bool SupportsOutput { get; set; } = false;

    /// <summary>
    /// Creates a new instance of MidiDeviceInfo from NAudio MidiInCapabilities
    /// </summary>
    public static MidiDeviceInfo FromCapabilities(int deviceId, MidiInCapabilities capabilities)
    {
        return new MidiDeviceInfo
        {
            DeviceId = deviceId,
            Name = capabilities.ProductName,
            Manufacturer = capabilities.Manufacturer.ToString(),
            DriverVersion = "N/A",
            IsConnected = true,
            LastSeen = DateTime.Now,
            SupportsInput = true,
            SupportsOutput = false
        };
    }

    /// <summary>
    /// Creates a new instance of MidiDeviceInfo from NAudio MidiOutCapabilities
    /// </summary>
    public static MidiDeviceInfo FromOutputCapabilities(int deviceId, MidiOutCapabilities capabilities)
    {
        return new MidiDeviceInfo
        {
            DeviceId = deviceId,
            Name = capabilities.ProductName,
            Manufacturer = capabilities.Manufacturer.ToString(),
            DriverVersion = "N/A",
            IsConnected = true,
            LastSeen = DateTime.Now,
            SupportsInput = false,
            SupportsOutput = true
        };
    }

    /// <summary>
    /// Returns a string representation of the MIDI device
    /// </summary>
    public override string ToString()
    {
        var capabilities = "";
        if (SupportsInput && SupportsOutput)
            capabilities = " (I/O)";
        else if (SupportsInput)
            capabilities = " (Input)";
        else if (SupportsOutput)
            capabilities = " (Output)";

        return $"[{DeviceId}] {Name}{capabilities}" + (IsConnected ? "" : " (Disconnected)");
    }

    /// <summary>
    /// Returns a detailed string representation of the MIDI device
    /// </summary>
    public string ToDetailedString()
    {
        var capabilities = "";
        if (SupportsInput && SupportsOutput)
            capabilities = "Input/Output";
        else if (SupportsInput)
            capabilities = "Input Only";
        else if (SupportsOutput)
            capabilities = "Output Only";
        else
            capabilities = "Unknown";

        return $"[{DeviceId}] {Name}\n" +
               $"  Manufacturer: {Manufacturer}\n" +
               $"  Driver Version: {DriverVersion}\n" +
               $"  Capabilities: {capabilities}\n" +
               $"  Status: {(IsConnected ? "Connected" : "Disconnected")}\n" +
               $"  Last Seen: {LastSeen:yyyy-MM-dd HH:mm:ss}";
    }
}
