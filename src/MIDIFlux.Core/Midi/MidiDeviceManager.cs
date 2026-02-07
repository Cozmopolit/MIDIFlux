using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Hardware;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Manages MIDI event coordination and dispatching through hardware abstraction layer.
/// Simplified to pure event coordination - all hardware interaction delegated to IMidiHardwareAdapter.
/// </summary>
public class MidiDeviceManager : IDisposable
{
    private readonly IMidiHardwareAdapter _hardwareAdapter;
    private readonly ILogger _logger;
    private bool _isDisposed;
    private volatile ProfileManager? _profileManager;

    /// <summary>
    /// Event raised when a MIDI event is received from hardware adapter
    /// </summary>
    public event EventHandler<MidiEventArgs>? MidiEventReceived;

    /// <summary>
    /// Event raised when a MIDI device is connected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceConnected;

    /// <summary>
    /// Event raised when a MIDI device is disconnected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected;

    /// <summary>
    /// Creates a new instance of the MidiDeviceManager with hardware abstraction
    /// </summary>
    /// <param name="hardwareAdapter">The MIDI hardware adapter</param>
    /// <param name="logger">The logger to use</param>
    public MidiDeviceManager(IMidiHardwareAdapter hardwareAdapter, ILogger<MidiDeviceManager> logger)
    {
        _hardwareAdapter = hardwareAdapter ?? throw new ArgumentNullException(nameof(hardwareAdapter));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Subscribe to hardware adapter events
        _hardwareAdapter.MidiEventReceived += HardwareAdapter_MidiEventReceived;
        _hardwareAdapter.DeviceConnected += HardwareAdapter_DeviceConnected;
        _hardwareAdapter.DeviceDisconnected += HardwareAdapter_DeviceDisconnected;

        _logger.LogInformation("MidiDeviceManager initialized with hardware abstraction layer");
    }

    /// <summary>
    /// Sets the event dispatcher to use for MIDI events
    /// </summary>
    /// <param name="profileManager">The event dispatcher</param>
    public void SetProfileManager(ProfileManager profileManager)
    {
        _profileManager = profileManager ?? throw new ArgumentNullException(nameof(profileManager));
        _logger.LogInformation("Event dispatcher set");
    }

    /// <summary>
    /// Gets a list of available MIDI input devices
    /// </summary>
    /// <returns>A list of MIDI input devices</returns>
    public List<MidiDeviceInfo> GetAvailableDevices()
    {
        return _hardwareAdapter.GetInputDevices().ToList();
    }

    /// <summary>
    /// Gets a list of available MIDI output devices
    /// </summary>
    /// <returns>A list of MIDI output devices</returns>
    public List<MidiDeviceInfo> GetAvailableOutputDevices()
    {
        return _hardwareAdapter.GetOutputDevices().ToList();
    }

    /// <summary>
    /// Gets detailed information about a specific MIDI input device
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device information, or null if the device is not found</returns>
    public MidiDeviceInfo? GetDeviceInfo(string deviceId)
    {
        var inputDevices = _hardwareAdapter.GetInputDevices();
        return inputDevices.FirstOrDefault(d => d.DeviceId == deviceId);
    }

    /// <summary>
    /// Gets detailed information about a specific MIDI output device
    /// </summary>
    /// <param name="deviceId">The output device ID</param>
    /// <returns>The device information, or null if the device is not found</returns>
    public MidiDeviceInfo? GetOutputDeviceInfo(string deviceId)
    {
        var outputDevices = _hardwareAdapter.GetOutputDevices();
        return outputDevices.FirstOrDefault(d => d.DeviceId == deviceId);
    }

    /// <summary>
    /// Gets the list of currently active MIDI device IDs (both input and output)
    /// </summary>
    public IReadOnlyList<string> ActiveDeviceIds => _hardwareAdapter.GetActiveDeviceIds();

    /// <summary>
    /// Checks if a specific device is currently active (being listened to or used for output)
    /// </summary>
    /// <param name="deviceId">The device ID to check</param>
    /// <returns>True if the device is currently active, false otherwise</returns>
    public bool IsDeviceActive(string deviceId) => _hardwareAdapter.IsDeviceActive(deviceId);

    /// <summary>
    /// Refreshes the list of available MIDI devices
    /// </summary>
    public void RefreshDeviceList()
    {
        _hardwareAdapter.RefreshDeviceList();
        _logger.LogInformation("Device list refreshed");
    }

    /// <summary>
    /// Starts listening for MIDI events from the specified device
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device to listen to</param>
    /// <returns>True if the device was opened successfully, false otherwise</returns>
    public bool StartListening(string deviceId)
    {
        var result = _hardwareAdapter.StartInputDevice(deviceId);
        if (result)
        {
            _logger.LogInformation("Started listening to MIDI device ID: {DeviceId}", deviceId);
        }
        else
        {
            _logger.LogError("Failed to start MIDI device ID: {DeviceId}", deviceId);
        }
        return result;
    }

    /// <summary>
    /// Starts a MIDI output device for sending messages
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI output device to start</param>
    /// <returns>True if the device was opened successfully, false otherwise</returns>
    public bool StartOutputDevice(string deviceId)
    {
        var result = _hardwareAdapter.StartOutputDevice(deviceId);
        if (result)
        {
            _logger.LogInformation("Started MIDI output device ID: {DeviceId}", deviceId);
        }
        else
        {
            _logger.LogError("Failed to start MIDI output device ID: {DeviceId}", deviceId);
        }
        return result;
    }

    /// <summary>
    /// Stops listening for MIDI events from the specified device
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device to stop listening to</param>
    public void StopListening(string deviceId)
    {
        var result = _hardwareAdapter.StopInputDevice(deviceId);
        if (result)
        {
            _logger.LogInformation("Stopped listening to MIDI device ID: {DeviceId}", deviceId);
        }
        else
        {
            _logger.LogWarning("Failed to stop MIDI device ID: {DeviceId} (may not have been started)", deviceId);
        }
    }

    /// <summary>
    /// Stops listening for MIDI events from all devices
    /// </summary>
    public void StopListening()
    {
        var activeDeviceIds = _hardwareAdapter.GetActiveDeviceIds();
        foreach (var deviceId in activeDeviceIds)
        {
            StopListening(deviceId);
        }
        _logger.LogInformation("Stopped listening to all MIDI devices");
    }

    /// <summary>
    /// Stops the specified MIDI output device
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI output device to stop</param>
    public void StopOutputDevice(string deviceId)
    {
        var result = _hardwareAdapter.StopOutputDevice(deviceId);
        if (result)
        {
            _logger.LogInformation("Stopped MIDI output device ID: {DeviceId}", deviceId);
        }
        else
        {
            _logger.LogWarning("Failed to stop MIDI output device ID: {DeviceId} (may not have been started)", deviceId);
        }
    }

    /// <summary>
    /// Sends a MIDI message to the specified output device
    /// </summary>
    /// <param name="deviceId">The output device ID</param>
    /// <param name="command">The MIDI output command to send</param>
    /// <returns>True if the message was sent successfully, false otherwise</returns>
    public bool SendMidiMessage(string deviceId, MidiOutputCommand command)
    {
        var result = _hardwareAdapter.SendMidiMessage(deviceId, command);
        if (result)
        {
            _logger.LogDebug("Sent MIDI message to device {DeviceId}: {Command}", deviceId, command);
        }
        else
        {
            _logger.LogError("Failed to send MIDI message to device {DeviceId}: {Command}", deviceId, command);
        }
        return result;
    }

    /// <summary>
    /// Handles MIDI events received from the hardware adapter
    /// </summary>
    private void HardwareAdapter_MidiEventReceived(object? sender, MidiEventArgs e)
    {
        try
        {
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("MIDI Event from device {DeviceId}: {MidiEvent}", e.DeviceId, e.Event);
            }

            // Raise the MidiEventReceived event
            MidiEventReceived?.Invoke(this, e);

            // Forward to the event dispatcher if set
            if (_profileManager != null)
            {
                _profileManager.HandleMidiEvent(e);
            }
            else
            {
                _logger.LogWarning("No event dispatcher set, MIDI event ignored");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI event from device {DeviceId}", e.DeviceId);
        }
    }

    /// <summary>
    /// Handles device connection events from the hardware adapter
    /// </summary>
    private void HardwareAdapter_DeviceConnected(object? sender, MidiDeviceInfo deviceInfo)
    {
        try
        {
            _logger.LogInformation("Device connected: {DeviceInfo}", deviceInfo);
            DeviceConnected?.Invoke(this, deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing device connection event for device {DeviceId}", deviceInfo.DeviceId);
        }
    }

    /// <summary>
    /// Handles device disconnection events from the hardware adapter
    /// </summary>
    private void HardwareAdapter_DeviceDisconnected(object? sender, MidiDeviceInfo deviceInfo)
    {
        try
        {
            _logger.LogInformation("Device disconnected: {DeviceInfo}", deviceInfo);
            DeviceDisconnected?.Invoke(this, deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing device disconnection event for device {DeviceId}", deviceInfo.DeviceId);
        }
    }



    /// <summary>
    /// Disposes the MidiDeviceManager
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the MidiDeviceManager
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Unsubscribe from hardware adapter events
                _hardwareAdapter.MidiEventReceived -= HardwareAdapter_MidiEventReceived;
                _hardwareAdapter.DeviceConnected -= HardwareAdapter_DeviceConnected;
                _hardwareAdapter.DeviceDisconnected -= HardwareAdapter_DeviceDisconnected;

                // Dispose of the hardware adapter
                _hardwareAdapter.Dispose();
            }

            _isDisposed = true;
        }
    }
}
