namespace MIDIFlux.Core.Models;

/// <summary>
/// Information about a MIDI device (input or output).
/// This is a pure data model with no dependencies on specific MIDI implementations.
/// Device ID format varies by adapter: NAudio uses "0", "1", "2"...; Windows MIDI Services uses endpoint strings.
/// </summary>
public class MidiDeviceInfo
{
    /// <summary>
    /// The device ID (format depends on the MIDI adapter implementation).
    /// NAudio: "0", "1", "2"... (string representation of integer indices)
    /// Windows MIDI Services: Endpoint device ID strings (e.g., "\\?\SWD#MMDEVAPI#...")
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

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
    /// Whether this device is currently being actively used (listening or sending)
    /// </summary>
    public bool IsActive { get; set; } = false;

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

        var status = "";
        if (!IsConnected)
            status = " (Disconnected)";
        else if (IsActive)
            status = " (Active)";

        return $"[{DeviceId}] {Name}{capabilities}{status}";
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
