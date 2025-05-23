using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using NAudio.Midi;
using Timer = System.Threading.Timer;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Monitors MIDI devices for connections and disconnections
/// </summary>
public class MidiDeviceMonitor : IDisposable
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, MidiDeviceInfo> _deviceInfoCache = new();
    private readonly Timer _deviceScanTimer;
    private bool _isDisposed;
    private readonly MidiManager _midiManager;

    /// <summary>
    /// Event raised when a MIDI device is connected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceConnected;

    /// <summary>
    /// Event raised when a MIDI device is disconnected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected;

    /// <summary>
    /// Creates a new instance of the MidiDeviceMonitor
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="midiManager">The MIDI manager</param>
    public MidiDeviceMonitor(ILogger logger, MidiManager midiManager)
    {
        _logger = logger;
        _midiManager = midiManager;

        // Initialize the device scan timer (check every 5 seconds)
        _deviceScanTimer = new Timer(ScanForDevices, null, Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Starts the device monitor
    /// </summary>
    public void Start()
    {
        _deviceScanTimer.Change(TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
    }

    /// <summary>
    /// Stops the device monitor
    /// </summary>
    public void Stop()
    {
        _deviceScanTimer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    /// <summary>
    /// Scans for MIDI devices and updates the device cache
    /// </summary>
    /// <param name="state">The timer state (not used)</param>
    private void ScanForDevices(object? state)
    {
        try
        {
            _logger.LogDebug("Scanning for MIDI devices...");

            // Get the current list of devices
            var currentDevices = new Dictionary<int, MidiDeviceInfo>();
            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                var capabilities = MidiIn.DeviceInfo(i);
                var deviceInfo = MidiDeviceInfo.FromCapabilities(i, capabilities);
                currentDevices[i] = deviceInfo;
            }

            // Check for disconnected devices
            foreach (var cachedDevice in _deviceInfoCache.Values.ToList())
            {
                if (!currentDevices.ContainsKey(cachedDevice.DeviceId))
                {
                    // Device was disconnected
                    cachedDevice.IsConnected = false;
                    _logger.LogWarning("MIDI device disconnected: {Device}", cachedDevice);

                    // Notify the MIDI manager about the disconnection
                    _midiManager.HandleDeviceDisconnection(cachedDevice.DeviceId);

                    // Notify subscribers about the disconnection
                    DeviceDisconnected?.Invoke(this, cachedDevice);
                }
            }

            // Check for new or reconnected devices
            foreach (var device in currentDevices.Values)
            {
                if (!_deviceInfoCache.TryGetValue(device.DeviceId, out var cachedDevice))
                {
                    // New device
                    _deviceInfoCache[device.DeviceId] = device;
                    _logger.LogInformation("New MIDI device detected: {Device}", device);
                    DeviceConnected?.Invoke(this, device);
                }
                else if (!cachedDevice.IsConnected)
                {
                    // Reconnected device
                    cachedDevice.IsConnected = true;
                    cachedDevice.LastSeen = DateTime.Now;
                    _logger.LogInformation("MIDI device reconnected: {Device}", cachedDevice);
                    DeviceConnected?.Invoke(this, device);

                    // Notify the MIDI manager about the reconnection
                    _midiManager.HandleDeviceReconnection(cachedDevice.DeviceId);
                }
                else
                {
                    // Update last seen time
                    cachedDevice.LastSeen = DateTime.Now;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning for MIDI devices");
        }
    }

    /// <summary>
    /// Refreshes the list of available MIDI devices
    /// </summary>
    public void RefreshDeviceList()
    {
        ScanForDevices(null);
    }

    /// <summary>
    /// Gets a list of available MIDI input devices
    /// </summary>
    /// <returns>A list of MIDI input devices</returns>
    public List<MidiDeviceInfo> GetAvailableDevices()
    {
        // Refresh the device list to ensure it's up to date
        RefreshDeviceList();

        // Return a copy of the cached device list
        return _deviceInfoCache.Values.ToList();
    }

    /// <summary>
    /// Gets detailed information about a specific MIDI device
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device information, or null if the device is not found</returns>
    public MidiDeviceInfo? GetDeviceInfo(int deviceId)
    {
        if (_deviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
        {
            return deviceInfo;
        }

        // Try to get the device info directly
        if (deviceId >= 0 && deviceId < MidiIn.NumberOfDevices)
        {
            var capabilities = MidiIn.DeviceInfo(deviceId);
            var info = MidiDeviceInfo.FromCapabilities(deviceId, capabilities);
            _deviceInfoCache[deviceId] = info;
            return info;
        }

        return null;
    }

    /// <summary>
    /// Disposes the MidiDeviceMonitor
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the MidiDeviceMonitor
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Dispose of the device scan timer
                _deviceScanTimer.Dispose();

                // Clear the device cache
                _deviceInfoCache.Clear();
            }

            _isDisposed = true;
        }
    }
}
