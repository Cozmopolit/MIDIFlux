using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using NAudio.Midi;

namespace MIDIFlux.Core.Hardware;

/// <summary>
/// NAudio implementation of the MIDI hardware adapter.
/// Centralizes all NAudio complexity and channel conversion logic in one place.
/// </summary>
/// <remarks>
/// Channel Conversion Strategy:
/// - Input events: NAudio 0-based (0-15) → MIDIFlux 1-based (1-16)
/// - Output events: MIDIFlux 1-based (1-16) → NAudio 1-based (1-16) [no conversion]
/// - Raw messages: MIDIFlux 1-based (1-16) → NAudio 0-based (0-15)
/// </remarks>
public class NAudioMidiAdapter : IMidiHardwareAdapter
{
    private readonly Dictionary<int, MidiIn> _midiInputs = new();
    private readonly Dictionary<int, MidiOut> _midiOutputs = new();
    private readonly Dictionary<int, MidiDeviceInfo> _inputDeviceInfoCache = new();
    private readonly Dictionary<int, MidiDeviceInfo> _outputDeviceInfoCache = new();
    private readonly ILogger _logger;
    private readonly System.Threading.Timer _deviceMonitorTimer;
    private bool _isDisposed;

    /// <summary>
    /// Event raised when a MIDI event is received from any started input device.
    /// All channel numbers are converted to 1-based (1-16) for MIDIFlux consistency.
    /// </summary>
    public event EventHandler<MidiEventArgs>? MidiEventReceived;

    /// <summary>
    /// Event raised when a MIDI device is connected.
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceConnected;

    /// <summary>
    /// Event raised when a MIDI device is disconnected.
    /// </summary>
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected;

    /// <summary>
    /// Initializes a new instance of NAudioMidiAdapter
    /// </summary>
    public NAudioMidiAdapter()
    {
        _logger = LoggingHelper.CreateLogger<NAudioMidiAdapter>();
        _logger.LogInformation("NAudio MIDI adapter initialized");

        // Perform initial device enumeration
        RefreshDeviceCache();

        // Start device monitoring timer (check every 5 seconds)
        _deviceMonitorTimer = new System.Threading.Timer(MonitorDevices, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
        _logger.LogDebug("Device monitoring timer started");
    }

    /// <summary>
    /// Gets all available MIDI input devices
    /// </summary>
    public IEnumerable<MidiDeviceInfo> GetInputDevices()
    {
        try
        {
            RefreshInputDeviceCache();
            return _inputDeviceInfoCache.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MIDI input devices");
            return Enumerable.Empty<MidiDeviceInfo>();
        }
    }

    /// <summary>
    /// Gets all available MIDI output devices
    /// </summary>
    public IEnumerable<MidiDeviceInfo> GetOutputDevices()
    {
        try
        {
            RefreshOutputDeviceCache();
            return _outputDeviceInfoCache.Values.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MIDI output devices");
            return Enumerable.Empty<MidiDeviceInfo>();
        }
    }

    /// <summary>
    /// Starts listening for MIDI events from the specified input device
    /// </summary>
    public bool StartInputDevice(int deviceId)
    {
        try
        {
            if (deviceId < 0 || deviceId >= MidiIn.NumberOfDevices)
            {
                throw new ArgumentException($"Invalid input device ID: {deviceId}. Valid range: 0-{MidiIn.NumberOfDevices - 1}");
            }

            // Return true if already started
            if (_midiInputs.ContainsKey(deviceId))
            {
                _logger.LogDebug("MIDI input device {DeviceId} is already started", deviceId);
                return true;
            }

            var midiIn = new MidiIn(deviceId);
            midiIn.MessageReceived += MidiIn_MessageReceived;
            midiIn.ErrorReceived += MidiIn_ErrorReceived;
            midiIn.Start();

            _midiInputs[deviceId] = midiIn;

            // Update device info cache
            if (_inputDeviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
            {
                deviceInfo.IsConnected = true;
                deviceInfo.LastSeen = DateTime.Now;
            }

            _logger.LogInformation("Started MIDI input device {DeviceId}: {DeviceName}",
                deviceId, MidiIn.DeviceInfo(deviceId).ProductName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MIDI input device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Starts the specified output device for sending MIDI messages
    /// </summary>
    public bool StartOutputDevice(int deviceId)
    {
        try
        {
            if (deviceId < 0 || deviceId >= MidiOut.NumberOfDevices)
            {
                throw new ArgumentException($"Invalid output device ID: {deviceId}. Valid range: 0-{MidiOut.NumberOfDevices - 1}");
            }

            // Return true if already started
            if (_midiOutputs.ContainsKey(deviceId))
            {
                _logger.LogDebug("MIDI output device {DeviceId} is already started", deviceId);
                return true;
            }

            var midiOut = new MidiOut(deviceId);
            _midiOutputs[deviceId] = midiOut;

            // Update device info cache
            if (_outputDeviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
            {
                deviceInfo.IsConnected = true;
                deviceInfo.LastSeen = DateTime.Now;
            }

            _logger.LogInformation("Started MIDI output device {DeviceId}: {DeviceName}",
                deviceId, MidiOut.DeviceInfo(deviceId).ProductName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MIDI output device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Stops listening for MIDI events from the specified input device
    /// </summary>
    public bool StopInputDevice(int deviceId)
    {
        try
        {
            if (!_midiInputs.TryGetValue(deviceId, out var midiIn))
            {
                _logger.LogDebug("MIDI input device {DeviceId} is not started", deviceId);
                return true; // Already stopped
            }

            midiIn.MessageReceived -= MidiIn_MessageReceived;
            midiIn.ErrorReceived -= MidiIn_ErrorReceived;
            midiIn.Stop();
            midiIn.Dispose();

            _midiInputs.Remove(deviceId);

            // Update device info cache
            if (_inputDeviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
            {
                deviceInfo.IsConnected = false;
            }

            _logger.LogInformation("Stopped MIDI input device {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MIDI input device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Stops the specified output device
    /// </summary>
    public bool StopOutputDevice(int deviceId)
    {
        try
        {
            if (!_midiOutputs.TryGetValue(deviceId, out var midiOut))
            {
                _logger.LogDebug("MIDI output device {DeviceId} is not started", deviceId);
                return true; // Already stopped
            }

            midiOut.Dispose();
            _midiOutputs.Remove(deviceId);

            // Update device info cache
            if (_outputDeviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
            {
                deviceInfo.IsConnected = false;
            }

            _logger.LogInformation("Stopped MIDI output device {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MIDI output device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Sends a MIDI message to the specified output device
    /// </summary>
    public bool SendMidiMessage(int deviceId, MidiOutputCommand command)
    {
        try
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (!_midiOutputs.TryGetValue(deviceId, out var midiOut))
            {
                _logger.LogError("MIDI output device {DeviceId} is not started", deviceId);
                return false;
            }

            // Validate the command
            ValidateCommand(command);

            // Handle different message types
            switch (command.MessageType)
            {
                case MidiMessageType.SysEx:
                    return SendSysExMessage(midiOut, command);

                case MidiMessageType.Aftertouch:
                case MidiMessageType.ChannelPressure:
                    return SendRawMessage(midiOut, command);

                default:
                    return SendNAudioEvent(midiOut, command);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MIDI message to device {DeviceId}: {Command}", deviceId, command);
            return false;
        }
    }

    /// <summary>
    /// Disposes all MIDI resources
    /// </summary>
    public void Dispose()
    {
        if (_isDisposed) return;

        try
        {
            // Stop device monitoring timer
            _deviceMonitorTimer?.Dispose();

            // Stop and dispose all input devices
            foreach (var kvp in _midiInputs.ToList())
            {
                StopInputDevice(kvp.Key);
            }

            // Stop and dispose all output devices
            foreach (var kvp in _midiOutputs.ToList())
            {
                StopOutputDevice(kvp.Key);
            }

            _isDisposed = true;
            _logger.LogInformation("NAudio MIDI adapter disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing NAudio MIDI adapter");
        }
    }

    /// <summary>
    /// Handles MIDI input messages and converts channels from 0-based to 1-based
    /// </summary>
    private void MidiIn_MessageReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            // Find the device ID for this sender
            var deviceId = FindDeviceIdForSender(sender);
            if (deviceId == -1)
            {
                _logger.LogWarning("Received MIDI message from unknown device");
                return;
            }

            // Create MidiEvent with channel conversion: NAudio 0-based → MIDIFlux 1-based
            var midiEvent = CreateMidiEventFromNAudio(e);

            // Create event args and raise event
            var eventArgs = new MidiEventArgs(deviceId, midiEvent);
            _logger.LogDebug("MIDI Event from device {DeviceId}: {MidiEvent}", deviceId, midiEvent);

            MidiEventReceived?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI input message");
        }
    }

    /// <summary>
    /// Handles MIDI input errors
    /// </summary>
    private void MidiIn_ErrorReceived(object? sender, MidiInMessageEventArgs e)
    {
        try
        {
            var deviceId = FindDeviceIdForSender(sender);
            _logger.LogError("MIDI input error from device {DeviceId}: {Error}", deviceId, e);

            // Create error event
            var errorEvent = new Models.MidiEvent
            {
                EventType = MidiEventType.Error,
                Timestamp = DateTime.Now,
                RawData = BitConverter.GetBytes(e.RawMessage),
                ErrorType = MidiErrorType.DeviceError
            };

            var eventArgs = new MidiEventArgs(deviceId, errorEvent);
            MidiEventReceived?.Invoke(this, eventArgs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI input error");
        }
    }

    /// <summary>
    /// Finds the device ID for a MIDI input sender
    /// </summary>
    private int FindDeviceIdForSender(object? sender)
    {
        if (sender is not MidiIn midiIn) return -1;

        foreach (var kvp in _midiInputs)
        {
            if (ReferenceEquals(kvp.Value, midiIn))
                return kvp.Key;
        }
        return -1;
    }

    /// <summary>
    /// Creates a MidiEvent from NAudio input with channel conversion
    /// </summary>
    private Models.MidiEvent CreateMidiEventFromNAudio(MidiInMessageEventArgs e)
    {
        var midiEvent = new Models.MidiEvent
        {
            Timestamp = DateTime.Now,
            RawData = BitConverter.GetBytes(e.RawMessage),
            // CRITICAL: Convert NAudio 0-based channel to MIDIFlux 1-based channel
            Channel = e.MidiEvent.Channel + 1
        };

        // Process different types of MIDI events
        switch (e.MidiEvent.CommandCode)
        {
            case MidiCommandCode.NoteOn:
                if (e.MidiEvent is NoteEvent noteEvent)
                {
                    ProcessNoteOn(midiEvent, noteEvent);
                }
                break;

            case MidiCommandCode.NoteOff:
                if (e.MidiEvent is NoteEvent noteOff)
                {
                    ProcessNoteOff(midiEvent, noteOff);
                }
                break;

            case MidiCommandCode.ControlChange:
                if (e.MidiEvent is ControlChangeEvent cc)
                {
                    ProcessControlChange(midiEvent, cc);
                }
                break;

            case MidiCommandCode.Sysex:
                if (e.MidiEvent is SysexEvent sysEx)
                {
                    ProcessSysEx(midiEvent, sysEx, e.RawMessage);
                }
                break;

            default:
                midiEvent.EventType = MidiEventType.Other;
                break;
        }

        return midiEvent;
    }

    /// <summary>
    /// Processes a Note On MIDI event
    /// </summary>
    private void ProcessNoteOn(Models.MidiEvent midiEvent, NoteEvent noteEvent)
    {
        // Check if it's a Note On or Note Off event (Note On with velocity 0 is treated as Note Off)
        bool isNoteOn = noteEvent is NoteOnEvent noteOn && noteOn.Velocity > 0;

        if (isNoteOn)
        {
            midiEvent.EventType = MidiEventType.NoteOn;
            midiEvent.Note = noteEvent.NoteNumber;
            midiEvent.Velocity = noteEvent is NoteOnEvent on ? on.Velocity : 127;
        }
        else
        {
            midiEvent.EventType = MidiEventType.NoteOff;
            midiEvent.Note = noteEvent.NoteNumber;
            midiEvent.Velocity = 0;
        }
    }

    /// <summary>
    /// Processes a Note Off MIDI event
    /// </summary>
    private void ProcessNoteOff(Models.MidiEvent midiEvent, NoteEvent noteEvent)
    {
        midiEvent.EventType = MidiEventType.NoteOff;
        midiEvent.Note = noteEvent.NoteNumber;
        midiEvent.Velocity = noteEvent.Velocity;
    }

    /// <summary>
    /// Processes a Control Change MIDI event
    /// </summary>
    private void ProcessControlChange(Models.MidiEvent midiEvent, ControlChangeEvent cc)
    {
        midiEvent.EventType = MidiEventType.ControlChange;
        midiEvent.Controller = (int)cc.Controller;
        midiEvent.Value = cc.ControllerValue;
    }

    /// <summary>
    /// Processes a SysEx MIDI event
    /// </summary>
    private void ProcessSysEx(Models.MidiEvent midiEvent, SysexEvent sysEx, int rawMessage)
    {
        midiEvent.EventType = MidiEventType.SystemExclusive;
        midiEvent.RawData = sysEx.GetAsShortMessage() != 0
            ? BitConverter.GetBytes(sysEx.GetAsShortMessage())
            : BitConverter.GetBytes(rawMessage);
    }

    /// <summary>
    /// Sends a MIDI message using NAudio events (for standard message types)
    /// </summary>
    private bool SendNAudioEvent(MidiOut midiOut, MidiOutputCommand command)
    {
        try
        {
            // NAudio event constructors expect 1-based channels (1-16), same as MIDIFlux
            // No conversion needed - pass through directly
            int naudioChannel = command.Channel;

            NAudio.Midi.MidiEvent? midiEvent = command.MessageType switch
            {
                MidiMessageType.NoteOn => new NoteOnEvent(0, naudioChannel, command.Data1, command.Data2, 0),
                MidiMessageType.NoteOff => new NoteEvent(0, naudioChannel, MidiCommandCode.NoteOff, command.Data1, command.Data2),
                MidiMessageType.ControlChange => new ControlChangeEvent(0, naudioChannel, (MidiController)command.Data1, command.Data2),
                MidiMessageType.ProgramChange => new PatchChangeEvent(0, naudioChannel, command.Data1),
                MidiMessageType.PitchBend => new PitchWheelChangeEvent(0, naudioChannel, (command.Data2 << 7) | command.Data1),
                _ => throw new NotSupportedException($"MIDI message type '{command.MessageType}' is not supported for NAudio events")
            };

            if (midiEvent != null)
            {
                var rawMessage = midiEvent.GetAsShortMessage();
                midiOut.Send(rawMessage);
                _logger.LogDebug("Sent MIDI event: {MessageType} Ch{Channel} Data1={Data1} Data2={Data2}",
                    command.MessageType, command.Channel, command.Data1, command.Data2);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending NAudio MIDI event: {Command}", command);
            return false;
        }
    }

    /// <summary>
    /// Sends a MIDI message using raw format (for message types without NAudio events)
    /// </summary>
    private bool SendRawMessage(MidiOut midiOut, MidiOutputCommand command)
    {
        try
        {
            // NAudio raw messages expect 0-based channels (0-15)
            // Convert from MIDIFlux 1-based to NAudio 0-based
            int naudioChannel = command.Channel - 1;

            int rawMessage = command.MessageType switch
            {
                MidiMessageType.Aftertouch => CreateAftertouchRawMessage(naudioChannel, command.Data1, command.Data2),
                MidiMessageType.ChannelPressure => CreateChannelPressureRawMessage(naudioChannel, command.Data1),
                _ => throw new NotSupportedException($"MIDI message type '{command.MessageType}' does not support raw message format")
            };

            midiOut.Send(rawMessage);
            _logger.LogDebug("Sent raw MIDI message: {MessageType} Ch{Channel} Data1={Data1} Data2={Data2}",
                command.MessageType, command.Channel, command.Data1, command.Data2);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending raw MIDI message: {Command}", command);
            return false;
        }
    }

    /// <summary>
    /// Sends a SysEx message
    /// </summary>
    private bool SendSysExMessage(MidiOut midiOut, MidiOutputCommand command)
    {
        try
        {
            if (command.SysExData == null || command.SysExData.Length == 0)
            {
                _logger.LogError("SysEx data is null or empty");
                return false;
            }

            // Send SysEx data directly
            midiOut.SendBuffer(command.SysExData);
            _logger.LogDebug("Sent SysEx message: {Length} bytes", command.SysExData.Length);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SysEx message: {Command}", command);
            return false;
        }
    }

    /// <summary>
    /// Creates a raw MIDI message for Aftertouch
    /// </summary>
    private int CreateAftertouchRawMessage(int channel, int note, int pressure)
    {
        return (0xA0 | channel) | (note << 8) | (pressure << 16);
    }

    /// <summary>
    /// Creates a raw MIDI message for Channel Pressure
    /// </summary>
    private int CreateChannelPressureRawMessage(int channel, int pressure)
    {
        return (0xD0 | channel) | (pressure << 8);
    }

    /// <summary>
    /// Validates a MIDI output command
    /// </summary>
    private void ValidateCommand(MidiOutputCommand command)
    {
        if (command.Channel < 1 || command.Channel > 16)
            throw new ArgumentException($"Invalid MIDI channel: {command.Channel}. Valid range: 1-16");

        // Validate data bytes based on message type
        switch (command.MessageType)
        {
            case MidiMessageType.NoteOn:
            case MidiMessageType.NoteOff:
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid note number: {command.Data1}. Valid range: 0-127");
                if (command.Data2 < 0 || command.Data2 > 127)
                    throw new ArgumentException($"Invalid velocity: {command.Data2}. Valid range: 0-127");
                break;

            case MidiMessageType.ControlChange:
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid controller number: {command.Data1}. Valid range: 0-127");
                if (command.Data2 < 0 || command.Data2 > 127)
                    throw new ArgumentException($"Invalid controller value: {command.Data2}. Valid range: 0-127");
                break;

            case MidiMessageType.ProgramChange:
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid program number: {command.Data1}. Valid range: 0-127");
                break;

            case MidiMessageType.SysEx:
                if (command.SysExData == null || command.SysExData.Length == 0)
                    throw new ArgumentException("SysEx data cannot be null or empty");
                break;
        }
    }

    /// <summary>
    /// Refreshes both input and output device caches
    /// </summary>
    private void RefreshDeviceCache()
    {
        RefreshInputDeviceCache();
        RefreshOutputDeviceCache();
    }

    /// <summary>
    /// Refreshes the input device cache
    /// </summary>
    private void RefreshInputDeviceCache()
    {
        try
        {
            // Store current connection states before clearing cache
            var currentConnectionStates = _inputDeviceInfoCache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.IsConnected);

            _inputDeviceInfoCache.Clear();

            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                try
                {
                    var capabilities = MidiIn.DeviceInfo(i);
                    var deviceInfo = MidiDeviceInfo.FromCapabilities(i, capabilities);

                    // Preserve connection status if device was previously active
                    deviceInfo.IsConnected = currentConnectionStates.ContainsKey(i)
                        ? currentConnectionStates[i]
                        : _midiInputs.ContainsKey(i); // Check if device is currently active

                    _inputDeviceInfoCache[i] = deviceInfo;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get info for MIDI input device {DeviceId}", i);
                }
            }

            _logger.LogDebug("Refreshed input device cache: {Count} devices", _inputDeviceInfoCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing input device cache");
        }
    }

    /// <summary>
    /// Refreshes the output device cache
    /// </summary>
    private void RefreshOutputDeviceCache()
    {
        try
        {
            // Store current connection states before clearing cache
            var currentConnectionStates = _outputDeviceInfoCache.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.IsConnected);

            _outputDeviceInfoCache.Clear();

            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                try
                {
                    var capabilities = MidiOut.DeviceInfo(i);
                    var deviceInfo = MidiDeviceInfo.FromOutputCapabilities(i, capabilities);

                    // Preserve connection status if device was previously active
                    deviceInfo.IsConnected = currentConnectionStates.ContainsKey(i)
                        ? currentConnectionStates[i]
                        : _midiOutputs.ContainsKey(i); // Check if device is currently active

                    _outputDeviceInfoCache[i] = deviceInfo;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to get info for MIDI output device {DeviceId}", i);
                }
            }

            _logger.LogDebug("Refreshed output device cache: {Count} devices", _outputDeviceInfoCache.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing output device cache");
        }
    }

    /// <summary>
    /// Gets the list of currently active device IDs (both input and output)
    /// </summary>
    public IReadOnlyList<int> GetActiveDeviceIds()
    {
        var activeIds = new List<int>();
        activeIds.AddRange(_midiInputs.Keys);
        activeIds.AddRange(_midiOutputs.Keys);
        return activeIds.Distinct().ToList();
    }

    /// <summary>
    /// Refreshes the internal device list cache
    /// </summary>
    public void RefreshDeviceList()
    {
        _logger.LogInformation("Refreshing MIDI device list");
        RefreshInputDeviceCache();
        RefreshOutputDeviceCache();
    }

    /// <summary>
    /// Monitors for device connections and disconnections
    /// </summary>
    private void MonitorDevices(object? state)
    {
        try
        {
            // Check for new input devices
            var currentInputDeviceCount = MidiIn.NumberOfDevices;
            var cachedInputDeviceCount = _inputDeviceInfoCache.Count;

            if (currentInputDeviceCount != cachedInputDeviceCount)
            {
                _logger.LogDebug("Input device count changed: {Old} -> {New}", cachedInputDeviceCount, currentInputDeviceCount);
                CheckForDeviceChanges();
            }

            // Check for new output devices
            var currentOutputDeviceCount = MidiOut.NumberOfDevices;
            var cachedOutputDeviceCount = _outputDeviceInfoCache.Count;

            if (currentOutputDeviceCount != cachedOutputDeviceCount)
            {
                _logger.LogDebug("Output device count changed: {Old} -> {New}", cachedOutputDeviceCount, currentOutputDeviceCount);
                CheckForDeviceChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error monitoring MIDI devices");
        }
    }

    /// <summary>
    /// Checks for device connections and disconnections
    /// </summary>
    private void CheckForDeviceChanges()
    {
        try
        {
            // Store old device lists
            var oldInputDevices = _inputDeviceInfoCache.Values.ToList();
            var oldOutputDevices = _outputDeviceInfoCache.Values.ToList();

            // Refresh device caches
            RefreshInputDeviceCache();
            RefreshOutputDeviceCache();

            // Check for new input devices
            foreach (var newDevice in _inputDeviceInfoCache.Values)
            {
                if (!oldInputDevices.Any(d => d.DeviceId == newDevice.DeviceId && d.Name == newDevice.Name))
                {
                    _logger.LogInformation("New MIDI input device detected: {Device}", newDevice);
                    DeviceConnected?.Invoke(this, newDevice);
                }
            }

            // Check for removed input devices
            foreach (var oldDevice in oldInputDevices)
            {
                if (!_inputDeviceInfoCache.Values.Any(d => d.DeviceId == oldDevice.DeviceId && d.Name == oldDevice.Name))
                {
                    _logger.LogInformation("MIDI input device removed: {Device}", oldDevice);
                    DeviceDisconnected?.Invoke(this, oldDevice);
                }
            }

            // Check for new output devices
            foreach (var newDevice in _outputDeviceInfoCache.Values)
            {
                if (!oldOutputDevices.Any(d => d.DeviceId == newDevice.DeviceId && d.Name == newDevice.Name))
                {
                    _logger.LogInformation("New MIDI output device detected: {Device}", newDevice);
                    DeviceConnected?.Invoke(this, newDevice);
                }
            }

            // Check for removed output devices
            foreach (var oldDevice in oldOutputDevices)
            {
                if (!_outputDeviceInfoCache.Values.Any(d => d.DeviceId == oldDevice.DeviceId && d.Name == oldDevice.Name))
                {
                    _logger.LogInformation("MIDI output device removed: {Device}", oldDevice);
                    DeviceDisconnected?.Invoke(this, oldDevice);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for device changes");
        }
    }
}
