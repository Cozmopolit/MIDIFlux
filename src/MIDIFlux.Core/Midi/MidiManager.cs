using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using NAudio.Midi;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Manages MIDI input devices and events
/// </summary>
public class MidiManager : IDisposable
{
    private readonly Dictionary<int, MidiIn> _midiInputs = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly ILogger _logger;
    private bool _isDisposed;
    private bool _autoReconnect = true;
    private EventDispatcher? _eventDispatcher;
    private readonly MidiEventConverter _eventConverter;
    private readonly MidiDeviceMonitor _deviceMonitor;

    /// <summary>
    /// Event raised when a MIDI device is connected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceConnected
    {
        add => _deviceMonitor.DeviceConnected += value;
        remove => _deviceMonitor.DeviceConnected -= value;
    }

    /// <summary>
    /// Event raised when a MIDI event is received
    /// </summary>
    public event EventHandler<MidiEventArgs>? MidiEventReceived;

    /// <summary>
    /// Event raised when a MIDI device is disconnected
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected
    {
        add => _deviceMonitor.DeviceDisconnected += value;
        remove => _deviceMonitor.DeviceDisconnected -= value;
    }

    /// <summary>
    /// Gets or sets whether to automatically reconnect to the MIDI device if it is disconnected
    /// </summary>
    public bool AutoReconnect
    {
        get => _autoReconnect;
        set => _autoReconnect = value;
    }

    /// <summary>
    /// Gets the list of currently active MIDI device IDs
    /// </summary>
    public IReadOnlyList<int> ActiveDeviceIds => _midiInputs.Keys.ToList();

    /// <summary>
    /// Creates a new instance of the MidiManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public MidiManager(ILogger<MidiManager> logger)
    {
        _logger = logger;
        _eventConverter = new MidiEventConverter(logger);
        _deviceMonitor = new MidiDeviceMonitor(logger, this);

        // Perform initial device scan
        RefreshDeviceList();
    }

    /// <summary>
    /// Sets the event dispatcher to use for MIDI events
    /// </summary>
    /// <param name="eventDispatcher">The event dispatcher</param>
    public void SetEventDispatcher(EventDispatcher eventDispatcher)
    {
        _eventDispatcher = eventDispatcher;
        _logger.LogInformation("Event dispatcher set");
    }

    /// <summary>
    /// Refreshes the list of available MIDI devices
    /// </summary>
    public void RefreshDeviceList()
    {
        _deviceMonitor.RefreshDeviceList();
    }

    /// <summary>
    /// Gets a list of available MIDI input devices
    /// </summary>
    /// <returns>A list of MIDI input devices</returns>
    public List<MidiDeviceInfo> GetAvailableDevices()
    {
        return _deviceMonitor.GetAvailableDevices();
    }

    /// <summary>
    /// Gets detailed information about a specific MIDI device
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device information, or null if the device is not found</returns>
    public MidiDeviceInfo? GetDeviceInfo(int deviceId)
    {
        return _deviceMonitor.GetDeviceInfo(deviceId);
    }

    /// <summary>
    /// Starts listening for MIDI events from the specified device
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device to listen to</param>
    /// <returns>True if the device was opened successfully, false otherwise</returns>
    public bool StartListening(int deviceId)
    {
        try
        {
            // Check if already listening to this device
            if (_midiInputs.ContainsKey(deviceId))
            {
                _logger.LogWarning("Already listening to MIDI device ID: {DeviceId}", deviceId);
                return true;
            }

            if (deviceId < 0 || deviceId >= MidiIn.NumberOfDevices)
            {
                _logger.LogError("Invalid MIDI device ID: {DeviceId}", deviceId);
                return false;
            }

            // Get or create device info
            var deviceInfo = GetDeviceInfo(deviceId);
            if (deviceInfo == null)
            {
                _logger.LogError("Failed to get device info for device ID: {DeviceId}", deviceId);
                return false;
            }

            var midiIn = new MidiIn(deviceId);
            midiIn.MessageReceived += MidiIn_MessageReceived;
            midiIn.ErrorReceived += MidiIn_ErrorReceived;
            midiIn.Start();

            // Store the MidiIn instance
            _midiInputs[deviceId] = midiIn;

            // Update device info
            deviceInfo.IsConnected = true;
            deviceInfo.LastSeen = DateTime.Now;

            _logger.LogInformation("Started listening to MIDI device: {Device}", deviceInfo);

            // Start the device monitor if not already running
            if (_midiInputs.Count == 1)
            {
                _deviceMonitor.Start();
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting MIDI device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Stops listening for MIDI events from all devices
    /// </summary>
    public void StopListening()
    {
        foreach (var deviceId in _midiInputs.Keys.ToList())
        {
            StopListening(deviceId);
        }
    }

    /// <summary>
    /// Stops listening for MIDI events from the specified device
    /// </summary>
    /// <param name="deviceId">The ID of the MIDI device to stop listening to</param>
    public void StopListening(int deviceId)
    {
        if (!_midiInputs.TryGetValue(deviceId, out var midiIn))
        {
            return;
        }

        try
        {
            midiIn.Stop();
            midiIn.MessageReceived -= MidiIn_MessageReceived;
            midiIn.ErrorReceived -= MidiIn_ErrorReceived;
            midiIn.Dispose();

            _midiInputs.Remove(deviceId);

            // Get device info for logging
            var deviceInfo = GetDeviceInfo(deviceId);
            _logger.LogInformation("Stopped listening to MIDI device: {Device}",
                deviceInfo != null ? deviceInfo.ToString() : $"ID: {deviceId}");

            // Stop the device monitor if no devices are being monitored
            if (_midiInputs.Count == 0)
            {
                _deviceMonitor.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping MIDI device {DeviceId}", deviceId);
        }
    }

    /// <summary>
    /// Handles a device disconnection
    /// </summary>
    /// <param name="deviceId">The ID of the disconnected device</param>
    public void HandleDeviceDisconnection(int deviceId)
    {
        try
        {
            if (!_midiInputs.TryGetValue(deviceId, out var midiIn))
            {
                return;
            }

            _logger.LogInformation("Cleaning up disconnected MIDI device {DeviceId}", deviceId);

            try
            {
                // Try to stop and clean up the device, but don't throw if it fails
                midiIn.Stop();
                midiIn.MessageReceived -= MidiIn_MessageReceived;
                midiIn.ErrorReceived -= MidiIn_ErrorReceived;
                midiIn.Dispose();
            }
            catch (Exception ex)
            {
                // Just log the error but continue with cleanup
                _logger.LogError(ex, "Error stopping disconnected MIDI device {DeviceId}", deviceId);
            }

            // Remove from active inputs
            _midiInputs.Remove(deviceId);

            // Notify the event dispatcher about the disconnection
            if (_eventDispatcher != null)
            {
                // Create a special disconnection event
                var disconnectEvent = _eventConverter.CreateDeviceDisconnectionEvent();
                var eventArgs = new MidiEventArgs(deviceId, disconnectEvent);
                _eventDispatcher.HandleDeviceDisconnection(deviceId, eventArgs);
            }

            // Stop the device monitor if no devices are being monitored
            if (_midiInputs.Count == 0)
            {
                _deviceMonitor.Stop();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device disconnection for device {DeviceId}", deviceId);
        }
    }

    /// <summary>
    /// Handles a device reconnection
    /// </summary>
    /// <param name="deviceId">The ID of the reconnected device</param>
    public void HandleDeviceReconnection(int deviceId)
    {
        // If auto-reconnect is enabled and this was one of the active devices, reconnect to it
        if (_autoReconnect && !_midiInputs.ContainsKey(deviceId))
        {
            var deviceInfo = GetDeviceInfo(deviceId);
            if (deviceInfo != null)
            {
                _logger.LogInformation("Auto-reconnecting to device: {Device}", deviceInfo);
                StartListening(deviceId);
            }
        }
    }

    private void MidiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            // Create a structured log context for this MIDI event
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                ["Command"] = e.MidiEvent.CommandCode,
                ["Channel"] = e.MidiEvent.Channel,
                ["Type"] = e.MidiEvent.GetType().Name,
                ["RawMessage"] = e.RawMessage
            }))
            {
                // Log the raw MIDI event with structured data
                if (e.MidiEvent is NoteEvent rawNoteEvent)
                {
                    bool isNoteOn = e.MidiEvent is NoteOnEvent noteOn && noteOn.Velocity > 0;
                    int velocity = e.MidiEvent is NoteOnEvent on ? on.Velocity : 0;

                    _logger.LogInformation("MIDI Note Event: {NoteNumber}, Velocity={Velocity}, Channel={Channel}, CommandCode={CommandCode}, IsNoteOn={IsNoteOn}",
                        rawNoteEvent.NoteNumber,
                        velocity,
                        e.MidiEvent.Channel,
                        e.MidiEvent.CommandCode,
                        isNoteOn);
                }
                else if (e.MidiEvent is ControlChangeEvent ccEvent)
                {
                    _logger.LogInformation("MIDI Control Change: Controller={Controller}, Value={Value}, Channel={Channel}",
                        ccEvent.Controller,
                        ccEvent.ControllerValue,
                        ccEvent.Channel);
                }
                else
                {
                    _logger.LogInformation("MIDI Event received: Type={Type}, Channel={Channel}, CommandCode={CommandCode}, RawMessage={RawMessage}",
                        e.MidiEvent.GetType().Name,
                        e.MidiEvent.Channel,
                        e.MidiEvent.CommandCode,
                        e.RawMessage);
                }

                // Find the device ID from the sender
                int deviceId = -1;
                foreach (var kvp in _midiInputs)
                {
                    if (kvp.Value == sender)
                    {
                        deviceId = kvp.Key;
                        break;
                    }
                }

                if (deviceId == -1)
                {
                    _logger.LogWarning("Received MIDI message from unknown device");
                    return;
                }

                // Create and populate the MIDI event
                var midiEvent = _eventConverter.CreateMidiEventFromNAudio(e);

                // Create event args
                var eventArgs = new MidiEventArgs(deviceId, midiEvent);

                // Directly dispatch the event to the handler
                _logger.LogDebug("MIDI Event from device {DeviceId}: {MidiEvent}", deviceId, midiEvent);

                // Raise the MidiEventReceived event
                MidiEventReceived?.Invoke(this, eventArgs);

                // Forward to the event dispatcher if set
                if (_eventDispatcher != null)
                {
                    _eventDispatcher.HandleMidiEvent(eventArgs);
                }
                else
                {
                    _logger.LogWarning("No event dispatcher set, MIDI event ignored");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI message");
        }
    }

    private void MidiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            _logger.LogError("MIDI error received: {RawMessage}", e.RawMessage);

            // Find the device ID from the sender
            int deviceId = -1;
            foreach (var kvp in _midiInputs)
            {
                if (kvp.Value == sender)
                {
                    deviceId = kvp.Key;
                    break;
                }
            }

            if (deviceId == -1)
            {
                _logger.LogWarning("Received MIDI error from unknown device");
                return;
            }

            // Create an error event
            var midiEvent = _eventConverter.CreateMidiErrorEvent(e.RawMessage);

            // Create event args
            var eventArgs = new MidiEventArgs(deviceId, midiEvent);

            // Raise the MidiEventReceived event
            MidiEventReceived?.Invoke(this, eventArgs);

            // Forward to the event dispatcher if set
            if (_eventDispatcher != null)
            {
                _eventDispatcher.HandleMidiEvent(eventArgs);
            }
            else
            {
                _logger.LogWarning("No event dispatcher set, MIDI error event ignored");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI error message");
        }
    }

    /// <summary>
    /// Disposes the MidiManager
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the MidiManager
    /// </summary>
    /// <param name="disposing">Whether to dispose managed resources</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                // Cancel any pending tasks
                _cancellationTokenSource.Cancel();

                // Stop listening for MIDI events
                StopListening();

                // Dispose of the device monitor
                _deviceMonitor.Dispose();

                // Dispose of managed resources
                _cancellationTokenSource.Dispose();
            }

            _isDisposed = true;
        }
    }
}
