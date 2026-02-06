using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Tests.Mocks;

/// <summary>
/// Mock implementation of IMidiHardwareAdapter for testing
/// Provides controllable MIDI event generation and device simulation
/// </summary>
public class MockMidiHardwareAdapter : IMidiHardwareAdapter
{
    private readonly ILogger<MockMidiHardwareAdapter> _logger;
    private readonly List<MidiDeviceInfo> _inputDevices = new();
    private readonly List<MidiDeviceInfo> _outputDevices = new();
    private readonly HashSet<string> _activeInputDevices = new();
    private readonly HashSet<string> _activeOutputDevices = new();
    private readonly List<MidiOutputCommand> _sentCommands = new();
    private bool _disposed;

    public event EventHandler<MidiEventArgs>? MidiEventReceived;
    public event EventHandler<MidiDeviceInfo>? DeviceConnected;
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected;

    /// <summary>
    /// Gets all MIDI commands that were sent through this adapter
    /// </summary>
    public IReadOnlyList<MidiOutputCommand> SentCommands => _sentCommands.AsReadOnly();

    /// <summary>
    /// Gets the currently active input device IDs
    /// </summary>
    public IReadOnlyList<string> ActiveInputDevices => _activeInputDevices.ToList().AsReadOnly();

    /// <summary>
    /// Gets the currently active output device IDs
    /// </summary>
    public IReadOnlyList<string> ActiveOutputDevices => _activeOutputDevices.ToList().AsReadOnly();

    public MockMidiHardwareAdapter(ILogger<MockMidiHardwareAdapter> logger)
    {
        _logger = logger;
        InitializeDefaultDevices();
    }

    /// <summary>
    /// Initializes default test devices
    /// </summary>
    private void InitializeDefaultDevices()
    {
        // Add some default test devices (using string device IDs like NAudioMidiAdapter)
        _inputDevices.Add(new MidiDeviceInfo
        {
            DeviceId = "0",
            Name = "Test MIDI Input 1",
            IsConnected = true,
            LastSeen = DateTime.Now
        });

        _inputDevices.Add(new MidiDeviceInfo
        {
            DeviceId = "1",
            Name = "Test MIDI Input 2",
            IsConnected = true,
            LastSeen = DateTime.Now
        });

        _outputDevices.Add(new MidiDeviceInfo
        {
            DeviceId = "0",
            Name = "Test MIDI Output 1",
            IsConnected = true,
            LastSeen = DateTime.Now
        });

        _outputDevices.Add(new MidiDeviceInfo
        {
            DeviceId = "1",
            Name = "Test MIDI Output 2",
            IsConnected = true,
            LastSeen = DateTime.Now
        });
    }

    public IEnumerable<MidiDeviceInfo> GetInputDevices()
    {
        return _inputDevices.ToList();
    }

    public IEnumerable<MidiDeviceInfo> GetOutputDevices()
    {
        return _outputDevices.ToList();
    }

    public bool StartInputDevice(string deviceId)
    {
        if (_disposed) return false;

        var device = _inputDevices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device == null)
        {
            _logger.LogWarning("Attempted to start non-existent input device {DeviceId}", deviceId);
            return false;
        }

        if (_activeInputDevices.Add(deviceId))
        {
            _logger.LogDebug("Started input device {DeviceId}: {DeviceName}", deviceId, device.Name);
            return true;
        }

        _logger.LogDebug("Input device {DeviceId} was already started", deviceId);
        return true;
    }

    public bool StartOutputDevice(string deviceId)
    {
        if (_disposed) return false;

        var device = _outputDevices.FirstOrDefault(d => d.DeviceId == deviceId);
        if (device == null)
        {
            _logger.LogWarning("Attempted to start non-existent output device {DeviceId}", deviceId);
            return false;
        }

        if (_activeOutputDevices.Add(deviceId))
        {
            _logger.LogDebug("Started output device {DeviceId}: {DeviceName}", deviceId, device.Name);
            return true;
        }

        _logger.LogDebug("Output device {DeviceId} was already started", deviceId);
        return true;
    }

    public bool StopInputDevice(string deviceId)
    {
        if (_disposed) return false;

        if (_activeInputDevices.Remove(deviceId))
        {
            _logger.LogDebug("Stopped input device {DeviceId}", deviceId);
            return true;
        }

        _logger.LogDebug("Input device {DeviceId} was not active", deviceId);
        return false;
    }

    public bool StopOutputDevice(string deviceId)
    {
        if (_disposed) return false;

        if (_activeOutputDevices.Remove(deviceId))
        {
            _logger.LogDebug("Stopped output device {DeviceId}", deviceId);
            return true;
        }

        _logger.LogDebug("Output device {DeviceId} was not active", deviceId);
        return false;
    }

    public bool SendMidiMessage(string deviceId, MidiOutputCommand command)
    {
        if (_disposed) return false;

        if (command == null)
        {
            _logger.LogWarning("Attempted to send null MIDI command");
            return false;
        }

        if (!_activeOutputDevices.Contains(deviceId))
        {
            _logger.LogWarning("Attempted to send MIDI message to inactive device {DeviceId}", deviceId);
            return false;
        }

        _sentCommands.Add(command);
        _logger.LogDebug("Sent MIDI command: {Command}", command);
        return true;
    }

    public IReadOnlyList<string> GetActiveDeviceIds()
    {
        var allActive = new List<string>();
        allActive.AddRange(_activeInputDevices);
        allActive.AddRange(_activeOutputDevices);
        return allActive.Distinct().ToList().AsReadOnly();
    }

    public bool IsDeviceActive(string deviceId)
    {
        return _activeInputDevices.Contains(deviceId) || _activeOutputDevices.Contains(deviceId);
    }

    public void RefreshDeviceList()
    {
        // Mock implementation - no actual refresh needed
        _logger.LogDebug("RefreshDeviceList called (mock implementation)");
    }

    /// <summary>
    /// Simulates receiving a MIDI event from a device
    /// </summary>
    public void SimulateMidiEvent(string deviceId, MidiEvent midiEvent)
    {
        if (_disposed) return;

        if (!_activeInputDevices.Contains(deviceId))
        {
            _logger.LogWarning("Attempted to simulate MIDI event from inactive device {DeviceId}", deviceId);
            return;
        }

        var eventArgs = new MidiEventArgs(deviceId, midiEvent);
        _logger.LogDebug("Simulating MIDI event from device {DeviceId}: {Event}", deviceId, midiEvent);
        MidiEventReceived?.Invoke(this, eventArgs);
    }

    /// <summary>
    /// Simulates a device connection
    /// </summary>
    public void SimulateDeviceConnected(MidiDeviceInfo deviceInfo)
    {
        if (_disposed) return;

        _logger.LogDebug("Simulating device connected: {DeviceName}", deviceInfo.Name);
        DeviceConnected?.Invoke(this, deviceInfo);
    }

    /// <summary>
    /// Simulates a device disconnection
    /// </summary>
    public void SimulateDeviceDisconnected(MidiDeviceInfo deviceInfo)
    {
        if (_disposed) return;

        _logger.LogDebug("Simulating device disconnected: {DeviceName}", deviceInfo.Name);
        DeviceDisconnected?.Invoke(this, deviceInfo);
    }

    /// <summary>
    /// Clears the list of sent commands (useful for test setup)
    /// </summary>
    public void ClearSentCommands()
    {
        _sentCommands.Clear();
    }

    /// <summary>
    /// Adds a custom input device for testing
    /// </summary>
    public void AddInputDevice(MidiDeviceInfo deviceInfo)
    {
        _inputDevices.Add(deviceInfo);
    }

    /// <summary>
    /// Adds a custom output device for testing
    /// </summary>
    public void AddOutputDevice(MidiDeviceInfo deviceInfo)
    {
        _outputDevices.Add(deviceInfo);
    }

    public void Dispose()
    {
        if (_disposed) return;

        _disposed = true;
        _activeInputDevices.Clear();
        _activeOutputDevices.Clear();
        _sentCommands.Clear();
        _inputDevices.Clear();
        _outputDevices.Clear();

        _logger.LogDebug("MockMidiHardwareAdapter disposed");
        GC.SuppressFinalize(this);
    }
}
