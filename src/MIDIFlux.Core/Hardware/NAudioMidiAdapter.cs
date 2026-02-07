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
/// Channel Handling:
/// - Input events: NAudio already uses 1-based channels (1-16) since NAudio 1.0.0 - no conversion needed
/// - Output events: MIDIFlux 1-based (1-16) → NAudio 1-based (1-16) - no conversion needed
/// - Raw messages: MIDIFlux 1-based (1-16) → NAudio 0-based (0-15) for raw byte construction
/// </remarks>
public class NAudioMidiAdapter : IMidiHardwareAdapter
{
    // Internal dictionaries use string device IDs (e.g., "0", "1", "2") for consistency with interface
    private readonly Dictionary<string, MidiIn> _midiInputs = new();
    private readonly Dictionary<string, MidiOut> _midiOutputs = new();
    private readonly Dictionary<string, MidiDeviceInfo> _inputDeviceInfoCache = new();
    private readonly Dictionary<string, MidiDeviceInfo> _outputDeviceInfoCache = new();

    /// <summary>
    /// Lock object for thread-safe access to all dictionaries.
    /// Required because MonitorDevices runs on a timer thread, MIDI events arrive on NAudio threads,
    /// and main application operations can happen concurrently.
    /// </summary>
    private readonly object _lock = new();

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
            lock (_lock)
            {
                // Only refresh cache if it's empty (first call or after device changes)
                if (_inputDeviceInfoCache.Count == 0)
                {
                    RefreshInputDeviceCacheInternal();
                }

                // Update active status for all devices (DeviceId is already a string)
                var devices = _inputDeviceInfoCache.Values.ToList();
                foreach (var device in devices)
                {
                    device.IsActive = _midiInputs.ContainsKey(device.DeviceId);
                }

                return devices;
            }
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
            lock (_lock)
            {
                // Only refresh cache if it's empty (first call or after device changes)
                if (_outputDeviceInfoCache.Count == 0)
                {
                    RefreshOutputDeviceCacheInternal();
                }

                // Update active status for all devices (DeviceId is already a string)
                var devices = _outputDeviceInfoCache.Values.ToList();
                foreach (var device in devices)
                {
                    device.IsActive = _midiOutputs.ContainsKey(device.DeviceId);
                }

                return devices;
            }
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
    public bool StartInputDevice(string deviceId)
    {
        try
        {
            // Parse string device ID to integer for NAudio API
            if (!int.TryParse(deviceId, out int nativeDeviceId))
            {
                throw new ArgumentException($"Invalid input device ID format: '{deviceId}'. NAudio expects numeric device IDs.");
            }

            if (nativeDeviceId < 0 || nativeDeviceId >= MidiIn.NumberOfDevices)
            {
                throw new ArgumentException($"Invalid input device ID: {deviceId}. Valid range: 0-{MidiIn.NumberOfDevices - 1}");
            }

            lock (_lock)
            {
                // Return true if already started
                if (_midiInputs.ContainsKey(deviceId))
                {
                    _logger.LogDebug("MIDI input device {DeviceId} is already started", deviceId);
                    return true;
                }

                var midiIn = new MidiIn(nativeDeviceId);
                midiIn.MessageReceived += MidiIn_MessageReceived;
                midiIn.ErrorReceived += MidiIn_ErrorReceived;

                _logger.LogInformation("Starting MIDI input device {DeviceId}: {DeviceName}",
                    deviceId, MidiIn.DeviceInfo(nativeDeviceId).ProductName);

                midiIn.Start();

                _midiInputs[deviceId] = midiIn;

                _logger.LogInformation("Successfully started MIDI input device {DeviceId}: {DeviceName}",
                    deviceId, MidiIn.DeviceInfo(nativeDeviceId).ProductName);
                return true;
            }
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
    public bool StartOutputDevice(string deviceId)
    {
        try
        {
            // Parse string device ID to integer for NAudio API
            if (!int.TryParse(deviceId, out int nativeDeviceId))
            {
                throw new ArgumentException($"Invalid output device ID format: '{deviceId}'. NAudio expects numeric device IDs.");
            }

            if (nativeDeviceId < 0 || nativeDeviceId >= MidiOut.NumberOfDevices)
            {
                throw new ArgumentException($"Invalid output device ID: {deviceId}. Valid range: 0-{MidiOut.NumberOfDevices - 1}");
            }

            lock (_lock)
            {
                // Return true if already started
                if (_midiOutputs.ContainsKey(deviceId))
                {
                    _logger.LogDebug("MIDI output device {DeviceId} is already started", deviceId);
                    return true;
                }

                var midiOut = new MidiOut(nativeDeviceId);
                _midiOutputs[deviceId] = midiOut;

                // Update device info cache
                if (_outputDeviceInfoCache.TryGetValue(deviceId, out var deviceInfo))
                {
                    deviceInfo.IsConnected = true;
                    deviceInfo.LastSeen = DateTime.Now;
                }

                _logger.LogInformation("Started MIDI output device {DeviceId}: {DeviceName}",
                    deviceId, MidiOut.DeviceInfo(nativeDeviceId).ProductName);
                return true;
            }
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
    public bool StopInputDevice(string deviceId)
    {
        try
        {
            lock (_lock)
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

                _logger.LogInformation("Stopped MIDI input device {DeviceId}", deviceId);
                return true;
            }
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
    public bool StopOutputDevice(string deviceId)
    {
        try
        {
            lock (_lock)
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
    public bool SendMidiMessage(string deviceId, MidiOutputCommand command)
    {
        try
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            MidiOut midiOut;
            lock (_lock)
            {
                if (!_midiOutputs.TryGetValue(deviceId, out midiOut!))
                {
                    _logger.LogError("MIDI output device {DeviceId} is not started", deviceId);
                    return false;
                }
            }

            // Validate the command (outside lock - no shared state access)
            ValidateCommand(command);

            // Handle different message types (outside lock - MidiOut is thread-safe for sending)
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

        // Set disposed flag early to prevent new operations and stop timer callbacks
        _isDisposed = true;

        try
        {
            // Stop device monitoring timer - use Change to prevent new callbacks,
            // then Dispose. Any in-flight callback will see _isDisposed and exit early.
            _deviceMonitorTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            _deviceMonitorTimer?.Dispose();

            // Get snapshot of device IDs under lock, then stop each device
            // (StopInputDevice/StopOutputDevice have their own locks)
            List<string> inputDeviceIds;
            List<string> outputDeviceIds;
            lock (_lock)
            {
                inputDeviceIds = _midiInputs.Keys.ToList();
                outputDeviceIds = _midiOutputs.Keys.ToList();
            }

            // Stop and dispose all input devices
            foreach (var deviceId in inputDeviceIds)
            {
                StopInputDevice(deviceId);
            }

            // Stop and dispose all output devices
            foreach (var deviceId in outputDeviceIds)
            {
                StopOutputDevice(deviceId);
            }

            _logger.LogInformation("NAudio MIDI adapter disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing NAudio MIDI adapter");
        }

        GC.SuppressFinalize(this);
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
            if (deviceId == null)
            {
                _logger.LogWarning("Received MIDI message from unknown device");
                return;
            }

            // Create MidiEvent with channel conversion: NAudio 0-based → MIDIFlux 1-based
            var midiEvent = CreateMidiEventFromNAudio(e);

            // Create event args and raise event
            var eventArgs = new MidiEventArgs(deviceId, midiEvent);
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("MIDI Event from device {DeviceId}: {MidiEvent}", deviceId, midiEvent);
            }

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
            if (deviceId == null)
            {
                _logger.LogWarning("Received MIDI error from unknown device");
                return;
            }

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
    private string? FindDeviceIdForSender(object? sender)
    {
        if (sender is not MidiIn midiIn) return null;

        lock (_lock)
        {
            foreach (var kvp in _midiInputs)
            {
                if (ReferenceEquals(kvp.Value, midiIn))
                    return kvp.Key;
            }
            return null;
        }
    }

    /// <summary>
    /// Creates a MidiEvent from NAudio input.
    /// </summary>
    /// <remarks>
    /// NAudio's MidiEvent.Channel is already 1-based (1-16) since NAudio 1.0.0 (April 2007).
    /// See NAudio RELEASE_NOTES.md: "MIDI events report channel from 1 to 16 now rather than 0 to 15"
    /// No conversion needed - pass through directly.
    /// </remarks>
    private Models.MidiEvent CreateMidiEventFromNAudio(MidiInMessageEventArgs e)
    {
        var midiEvent = new Models.MidiEvent
        {
            Timestamp = DateTime.Now,
            RawData = BitConverter.GetBytes(e.RawMessage),
            // NAudio Channel is already 1-based (1-16), no conversion needed
            Channel = e.MidiEvent.Channel
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

            case MidiCommandCode.PatchChange:
                if (e.MidiEvent is PatchChangeEvent patchChange)
                {
                    midiEvent.EventType = MidiEventType.ProgramChange;
                    midiEvent.ProgramNumber = patchChange.Patch;
                }
                break;

            case MidiCommandCode.PitchWheelChange:
                if (e.MidiEvent is PitchWheelChangeEvent pitchBend)
                {
                    midiEvent.EventType = MidiEventType.PitchBend;
                    // NAudio's Pitch property is already 0-16383 (14-bit value)
                    midiEvent.PitchBendValue = pitchBend.Pitch;
                }
                break;

            case MidiCommandCode.ChannelAfterTouch:
                if (e.MidiEvent is ChannelAfterTouchEvent channelPressure)
                {
                    midiEvent.EventType = MidiEventType.ChannelPressure;
                    midiEvent.Pressure = channelPressure.AfterTouchPressure;
                }
                break;

            case MidiCommandCode.KeyAfterTouch:
                if (e.MidiEvent is NoteEvent keyPressure)
                {
                    midiEvent.EventType = MidiEventType.PolyphonicKeyPressure;
                    midiEvent.Note = keyPressure.NoteNumber;
                    midiEvent.Pressure = keyPressure.Velocity; // NAudio uses Velocity for pressure
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
    /// <remarks>
    /// NAudio's SysexEvent stores the complete SysEx message including F0 start byte.
    /// We export it to a MemoryStream to get the raw bytes reliably.
    /// Note: For real-time SysEx reception, consider using MidiIn.SysexMessageReceived event
    /// with CreateSysexBuffers() for proper long SysEx message handling.
    /// </remarks>
    private void ProcessSysEx(Models.MidiEvent midiEvent, SysexEvent sysEx, int rawMessage)
    {
        midiEvent.EventType = MidiEventType.SystemExclusive;

        try
        {
            // NAudio's SysexEvent can be exported to get the raw bytes
            // Export writes the event data to a stream in MIDI file format
            using var ms = new System.IO.MemoryStream();
            using var writer = new System.IO.BinaryWriter(ms);

            // Export the SysEx event - this writes F0, length, data, F7
            // Note: Export expects a ref long for position tracking
            long position = 0;
            sysEx.Export(ref position, writer);
            writer.Flush();

            var exportedData = ms.ToArray();

            // The exported data may include delta time bytes at the start (for MIDI file format)
            // For real-time events, we want just the SysEx data
            // Find the F0 start byte and extract from there
            int startIndex = Array.IndexOf(exportedData, (byte)0xF0);
            if (startIndex >= 0)
            {
                // Find F7 end byte
                int endIndex = Array.LastIndexOf(exportedData, (byte)0xF7);
                if (endIndex > startIndex)
                {
                    int length = endIndex - startIndex + 1;
                    var sysExData = new byte[length];
                    Array.Copy(exportedData, startIndex, sysExData, 0, length);
                    midiEvent.SysExData = sysExData;
                    midiEvent.RawData = sysExData;
                    return;
                }
            }

            // Fallback: if we couldn't parse the exported data, use the raw message
            _logger.LogDebug("SysEx export parsing failed, using raw message bytes");
            midiEvent.RawData = BitConverter.GetBytes(rawMessage);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract SysEx data, falling back to raw message bytes");
            // Fallback: store the raw message as 4 bytes (limited, but better than nothing)
            midiEvent.RawData = BitConverter.GetBytes(rawMessage);
        }
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

            case MidiMessageType.PitchBend:
                // PitchBend uses Data1 (LSB) and Data2 (MSB) to form 14-bit value
                // Each byte must be 0-127
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid pitch bend LSB: {command.Data1}. Valid range: 0-127");
                if (command.Data2 < 0 || command.Data2 > 127)
                    throw new ArgumentException($"Invalid pitch bend MSB: {command.Data2}. Valid range: 0-127");
                break;

            case MidiMessageType.Aftertouch:
                // Polyphonic Key Pressure: Data1 = note number, Data2 = pressure
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid note number: {command.Data1}. Valid range: 0-127");
                if (command.Data2 < 0 || command.Data2 > 127)
                    throw new ArgumentException($"Invalid pressure value: {command.Data2}. Valid range: 0-127");
                break;

            case MidiMessageType.ChannelPressure:
                // Channel Pressure: Data1 = pressure value
                if (command.Data1 < 0 || command.Data1 > 127)
                    throw new ArgumentException($"Invalid pressure value: {command.Data1}. Valid range: 0-127");
                break;

            case MidiMessageType.SysEx:
                if (command.SysExData == null || command.SysExData.Length == 0)
                    throw new ArgumentException("SysEx data cannot be null or empty");
                break;
        }
    }

    /// <summary>
    /// Refreshes both input and output device caches.
    /// Called from constructor before timer starts, so no lock needed.
    /// </summary>
    private void RefreshDeviceCache()
    {
        RefreshInputDeviceCacheInternal();
        RefreshOutputDeviceCacheInternal();
    }

    /// <summary>
    /// Refreshes the input device cache.
    /// IMPORTANT: Caller must hold _lock before calling this method.
    /// </summary>
    private void RefreshInputDeviceCacheInternal()
    {
        try
        {
            _inputDeviceInfoCache.Clear();

            for (int i = 0; i < MidiIn.NumberOfDevices; i++)
            {
                try
                {
                    var capabilities = MidiIn.DeviceInfo(i);
                    var deviceId = i.ToString();
                    var deviceInfo = CreateInputDeviceInfo(deviceId, capabilities);
                    _inputDeviceInfoCache[deviceId] = deviceInfo;
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
    /// Refreshes the output device cache.
    /// IMPORTANT: Caller must hold _lock before calling this method.
    /// </summary>
    private void RefreshOutputDeviceCacheInternal()
    {
        try
        {
            _outputDeviceInfoCache.Clear();

            for (int i = 0; i < MidiOut.NumberOfDevices; i++)
            {
                try
                {
                    var capabilities = MidiOut.DeviceInfo(i);
                    var deviceId = i.ToString();
                    var deviceInfo = CreateOutputDeviceInfo(deviceId, capabilities);
                    _outputDeviceInfoCache[deviceId] = deviceInfo;
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
    /// Creates a MidiDeviceInfo from NAudio MidiInCapabilities
    /// </summary>
    private static MidiDeviceInfo CreateInputDeviceInfo(string deviceId, MidiInCapabilities capabilities)
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
    /// Creates a MidiDeviceInfo from NAudio MidiOutCapabilities
    /// </summary>
    private static MidiDeviceInfo CreateOutputDeviceInfo(string deviceId, MidiOutCapabilities capabilities)
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
    /// Gets the list of currently active device IDs (both input and output)
    /// </summary>
    public IReadOnlyList<string> GetActiveDeviceIds()
    {
        lock (_lock)
        {
            var activeIds = new List<string>();
            activeIds.AddRange(_midiInputs.Keys);
            activeIds.AddRange(_midiOutputs.Keys);
            return activeIds.Distinct().ToList();
        }
    }

    /// <summary>
    /// Checks if a specific device is currently active (being listened to or used for output)
    /// </summary>
    /// <param name="deviceId">The device ID to check</param>
    /// <returns>True if the device is currently active, false otherwise</returns>
    public bool IsDeviceActive(string deviceId)
    {
        lock (_lock)
        {
            return _midiInputs.ContainsKey(deviceId) || _midiOutputs.ContainsKey(deviceId);
        }
    }

    /// <summary>
    /// Refreshes the internal device list cache
    /// </summary>
    public void RefreshDeviceList()
    {
        lock (_lock)
        {
            _logger.LogInformation("Refreshing MIDI device list");
            RefreshInputDeviceCacheInternal();
            RefreshOutputDeviceCacheInternal();
        }
    }

    /// <summary>
    /// Monitors for device connections and disconnections
    /// </summary>
    private void MonitorDevices(object? state)
    {
        // Early exit if disposed - prevents accessing resources during/after disposal
        if (_isDisposed) return;

        try
        {
            // Get current device counts from NAudio (outside lock - these are thread-safe NAudio calls)
            var currentInputDeviceCount = MidiIn.NumberOfDevices;
            var currentOutputDeviceCount = MidiOut.NumberOfDevices;

            // Get cached counts under lock
            int cachedInputDeviceCount;
            int cachedOutputDeviceCount;
            lock (_lock)
            {
                cachedInputDeviceCount = _inputDeviceInfoCache.Count;
                cachedOutputDeviceCount = _outputDeviceInfoCache.Count;
            }

            // Check for input device changes
            if (currentInputDeviceCount != cachedInputDeviceCount)
            {
                _logger.LogDebug("Input device count changed: {Old} -> {New}", cachedInputDeviceCount, currentInputDeviceCount);
                CheckForDeviceChanges();
            }
            // Check for output device changes (only if we didn't already detect a change)
            else if (currentOutputDeviceCount != cachedOutputDeviceCount)
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
            // Collect events to fire outside the lock to avoid potential deadlocks
            var connectedDevices = new List<MidiDeviceInfo>();
            var disconnectedDevices = new List<MidiDeviceInfo>();

            // Collect active devices that need to be stopped (their underlying hardware is gone)
            var staleInputDeviceIds = new List<string>();
            var staleOutputDeviceIds = new List<string>();

            lock (_lock)
            {
                // Store old device lists by name (device IDs can change when devices are reconnected)
                var oldInputDeviceNames = _inputDeviceInfoCache.Values.Select(d => d.Name).ToHashSet();
                var oldOutputDeviceNames = _outputDeviceInfoCache.Values.Select(d => d.Name).ToHashSet();

                // Also store old active device IDs before refresh
                var oldActiveInputIds = _midiInputs.Keys.ToList();
                var oldActiveOutputIds = _midiOutputs.Keys.ToList();

                // Refresh device caches (internal methods assume lock is held)
                RefreshInputDeviceCacheInternal();
                RefreshOutputDeviceCacheInternal();

                // Check for new input devices (by name)
                foreach (var newDevice in _inputDeviceInfoCache.Values)
                {
                    if (!oldInputDeviceNames.Contains(newDevice.Name))
                    {
                        connectedDevices.Add(newDevice);
                    }
                }

                // Check for removed input devices (by name)
                var currentInputDeviceNames = _inputDeviceInfoCache.Values.Select(d => d.Name).ToHashSet();
                foreach (var oldDeviceName in oldInputDeviceNames)
                {
                    if (!currentInputDeviceNames.Contains(oldDeviceName))
                    {
                        // Create a device info for the removed device
                        disconnectedDevices.Add(new MidiDeviceInfo
                        {
                            Name = oldDeviceName,
                            IsConnected = false,
                            SupportsInput = true,
                            SupportsOutput = false
                        });
                    }
                }

                // Check for new output devices (by name)
                foreach (var newDevice in _outputDeviceInfoCache.Values)
                {
                    if (!oldOutputDeviceNames.Contains(newDevice.Name))
                    {
                        connectedDevices.Add(newDevice);
                    }
                }

                // Check for removed output devices (by name)
                var currentOutputDeviceNames = _outputDeviceInfoCache.Values.Select(d => d.Name).ToHashSet();
                foreach (var oldDeviceName in oldOutputDeviceNames)
                {
                    if (!currentOutputDeviceNames.Contains(oldDeviceName))
                    {
                        // Create a device info for the removed device
                        disconnectedDevices.Add(new MidiDeviceInfo
                        {
                            Name = oldDeviceName,
                            IsConnected = false,
                            SupportsInput = false,
                            SupportsOutput = true
                        });
                    }
                }

                // Identify active devices whose IDs are no longer valid
                // (device IDs are indices that shift when devices are removed)
                var currentInputDeviceIds = _inputDeviceInfoCache.Keys.ToHashSet();
                var currentOutputDeviceIds = _outputDeviceInfoCache.Keys.ToHashSet();

                foreach (var activeId in oldActiveInputIds)
                {
                    if (!currentInputDeviceIds.Contains(activeId))
                    {
                        staleInputDeviceIds.Add(activeId);
                    }
                }

                foreach (var activeId in oldActiveOutputIds)
                {
                    if (!currentOutputDeviceIds.Contains(activeId))
                    {
                        staleOutputDeviceIds.Add(activeId);
                    }
                }
            }

            // Stop stale active devices outside the lock (StopInputDevice/StopOutputDevice have their own locks)
            foreach (var deviceId in staleInputDeviceIds)
            {
                _logger.LogWarning("Stopping stale input device {DeviceId} - underlying hardware no longer available", deviceId);
                StopInputDevice(deviceId);
            }

            foreach (var deviceId in staleOutputDeviceIds)
            {
                _logger.LogWarning("Stopping stale output device {DeviceId} - underlying hardware no longer available", deviceId);
                StopOutputDevice(deviceId);
            }

            // Fire events outside the lock to avoid potential deadlocks
            foreach (var device in connectedDevices)
            {
                _logger.LogInformation("New MIDI device detected: {Device}", device);
                DeviceConnected?.Invoke(this, device);
            }

            foreach (var device in disconnectedDevices)
            {
                _logger.LogInformation("MIDI device removed: {DeviceName}", device.Name);
                DeviceDisconnected?.Invoke(this, device);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking for device changes");
        }
    }
}
