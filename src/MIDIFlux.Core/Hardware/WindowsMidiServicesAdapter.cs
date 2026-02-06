using Microsoft.Extensions.Logging;
using Microsoft.Windows.Devices.Midi2;
using Microsoft.Windows.Devices.Midi2.Initialization;
using Microsoft.Windows.Devices.Midi2.Messages;
using MIDIFlux.Core.Models;

// Alias to disambiguate from SDK's MidiMessageType
using MidiFluxMessageType = MIDIFlux.Core.Models.MidiMessageType;
using SdkMidiMessageType = Microsoft.Windows.Devices.Midi2.MidiMessageType;

namespace MIDIFlux.Core.Hardware;

/// <summary>
/// Implementation of IMidiHardwareAdapter using Windows MIDI Services SDK.
/// </summary>
/// <remarks>
/// This adapter provides native Windows MIDI Services support for Windows 11 24H2+.
///
/// Key differences from NAudio adapter:
/// - Uses UMP (Universal MIDI Packet) format internally, translated to/from MIDI 1.0
/// - Endpoints are bidirectional (same device ID for input and output)
/// - Event-based device monitoring via MidiEndpointDeviceWatcher
/// - Requires SDK Runtime to be installed separately
///
/// EXPERIMENTAL: This implementation uses the Windows MIDI Services SDK which is
/// currently in Release Candidate status. API may change in future versions.
///
/// SysEx is not supported in this implementation phase.
/// </remarks>
public class WindowsMidiServicesAdapter : IMidiHardwareAdapter
{
    private readonly ILogger<WindowsMidiServicesAdapter> _logger;
    private readonly object _lock = new();

    // SDK components
    private MidiDesktopAppSdkInitializer? _initializer;
    private MidiSession? _session;
    private MidiEndpointDeviceWatcher? _deviceWatcher;

    // Connection tracking - Windows MIDI Services uses unified bidirectional endpoints
    private readonly Dictionary<string, MidiEndpointConnection> _inputConnections = new();
    private readonly Dictionary<string, MidiEndpointConnection> _outputConnections = new();
    private readonly Dictionary<string, MidiDeviceInfo> _knownDevices = new();

    private bool _disposed;

    /// <summary>
    /// Endpoint purposes to include in device enumeration.
    /// Excludes diagnostic/internal endpoints.
    /// </summary>
    private static readonly HashSet<MidiEndpointDevicePurpose> AllowedPurposes = new()
    {
        MidiEndpointDevicePurpose.NormalMessageEndpoint,
        MidiEndpointDevicePurpose.VirtualDeviceResponder,
        MidiEndpointDevicePurpose.InBoxGeneralMidiSynth
    };

    /// <inheritdoc />
    public event EventHandler<MidiEventArgs>? MidiEventReceived;

    /// <inheritdoc />
    public event EventHandler<MidiDeviceInfo>? DeviceConnected;

    /// <inheritdoc />
    public event EventHandler<MidiDeviceInfo>? DeviceDisconnected;

    /// <summary>
    /// Creates a new instance of the Windows MIDI Services adapter.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output</param>
    /// <exception cref="InvalidOperationException">Thrown if SDK initialization fails</exception>
    public WindowsMidiServicesAdapter(ILogger<WindowsMidiServicesAdapter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            InitializeSdk();
            StartDeviceWatcher();
            _logger.LogInformation("Windows MIDI Services adapter initialized (experimental)");
        }
        catch
        {
            // Clean up partially initialized resources before rethrowing
            CleanupSdkResources();
            throw;
        }
    }

    /// <summary>
    /// Cleans up SDK resources. Safe to call even if resources are null or partially initialized.
    /// </summary>
    private void CleanupSdkResources()
    {
        try
        {
            _session?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error disposing MIDI session during cleanup");
        }
        finally
        {
            _session = null;
        }

        try
        {
            _initializer?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Error disposing SDK initializer during cleanup");
        }
        finally
        {
            _initializer = null;
        }
    }

    /// <summary>
    /// Initializes the Windows MIDI Services SDK.
    /// </summary>
    private void InitializeSdk()
    {
        _initializer = MidiDesktopAppSdkInitializer.Create();

        if (_initializer == null)
        {
            throw new InvalidOperationException(
                "Failed to create Windows MIDI Services SDK initializer. " +
                "Please ensure the SDK Runtime is installed from https://aka.ms/windowsmidiservices");
        }

        if (!_initializer.InitializeSdkRuntime())
        {
            throw new InvalidOperationException(
                "Failed to initialize Windows MIDI Services SDK Runtime. " +
                "Please ensure the SDK Runtime is installed from https://aka.ms/windowsmidiservices");
        }

        if (!_initializer.EnsureServiceAvailable())
        {
            throw new InvalidOperationException(
                "Windows MIDI Service is not available. " +
                "Please ensure the Windows MIDI Services are running.");
        }

        _session = MidiSession.Create("MIDIFlux");
        if (_session == null)
        {
            throw new InvalidOperationException("Failed to create MIDI session");
        }

        _logger.LogDebug("Windows MIDI Services SDK initialized successfully");
    }

    /// <summary>
    /// Starts the device watcher for hot-plug detection.
    /// </summary>
    private void StartDeviceWatcher()
    {
        _deviceWatcher = MidiEndpointDeviceWatcher.Create();
        _deviceWatcher.Added += OnDeviceAdded;
        _deviceWatcher.Removed += OnDeviceRemoved;
        _deviceWatcher.Updated += OnDeviceUpdated;
        _deviceWatcher.Start();

        _logger.LogDebug("Device watcher started");
    }

    /// <summary>
    /// Handles device added events from the watcher.
    /// </summary>
    private void OnDeviceAdded(MidiEndpointDeviceWatcher sender, MidiEndpointDeviceInformationAddedEventArgs args)
    {
        try
        {
            var device = args.AddedDevice;
            if (!AllowedPurposes.Contains(device.EndpointPurpose))
            {
                _logger.LogDebug("Ignoring device with purpose {Purpose}: {Name}",
                    device.EndpointPurpose, device.Name);
                return;
            }

            var deviceInfo = CreateDeviceInfo(device);

            lock (_lock)
            {
                _knownDevices[device.EndpointDeviceId] = deviceInfo;
            }

            _logger.LogInformation("MIDI device connected: {Name} ({DeviceId})",
                device.Name, device.EndpointDeviceId);

            // Raise event outside lock
            DeviceConnected?.Invoke(this, deviceInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device added event");
        }
    }

    /// <summary>
    /// Handles device removed events from the watcher.
    /// </summary>
    private void OnDeviceRemoved(MidiEndpointDeviceWatcher sender, MidiEndpointDeviceInformationRemovedEventArgs args)
    {
        try
        {
            var deviceId = args.EndpointDeviceId;
            MidiDeviceInfo? deviceInfo = null;

            lock (_lock)
            {
                // Get device info before removing
                _knownDevices.TryGetValue(deviceId, out deviceInfo);
                _knownDevices.Remove(deviceId);

                // Close any open connections
                if (_inputConnections.TryGetValue(deviceId, out var inputConnection))
                {
                    try { _session?.DisconnectEndpointConnection(inputConnection.ConnectionId); } catch { /* ignore */ }
                    _inputConnections.Remove(deviceId);
                }
                if (_outputConnections.TryGetValue(deviceId, out var outputConnection))
                {
                    try { _session?.DisconnectEndpointConnection(outputConnection.ConnectionId); } catch { /* ignore */ }
                    _outputConnections.Remove(deviceId);
                }
            }

            _logger.LogInformation("MIDI device disconnected: {DeviceId}", deviceId);

            // Raise event outside lock
            if (deviceInfo != null)
            {
                deviceInfo.IsConnected = false;
                DeviceDisconnected?.Invoke(this, deviceInfo);
            }
            else
            {
                // Create minimal device info for unknown device
                DeviceDisconnected?.Invoke(this, new MidiDeviceInfo
                {
                    DeviceId = deviceId,
                    IsConnected = false
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device removed event");
        }
    }

    /// <summary>
    /// Handles device updated events from the watcher.
    /// </summary>
    private void OnDeviceUpdated(MidiEndpointDeviceWatcher sender, MidiEndpointDeviceInformationUpdatedEventArgs args)
    {
        // Device properties updated - refresh our cached info
        try
        {
            var deviceId = args.EndpointDeviceId;
            var device = MidiEndpointDeviceInformation.CreateFromEndpointDeviceId(deviceId);
            if (device != null && AllowedPurposes.Contains(device.EndpointPurpose))
            {
                var deviceInfo = CreateDeviceInfo(device);
                lock (_lock)
                {
                    _knownDevices[deviceId] = deviceInfo;
                }
                _logger.LogDebug("MIDI device updated: {Name}", device.Name);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device updated event");
        }
    }

    /// <summary>
    /// Creates a MidiDeviceInfo from SDK endpoint information.
    /// </summary>
    private static MidiDeviceInfo CreateDeviceInfo(MidiEndpointDeviceInformation device)
    {
        return new MidiDeviceInfo
        {
            DeviceId = device.EndpointDeviceId,
            Name = device.Name,
            Manufacturer = "", // SDK doesn't expose manufacturer directly
            DriverVersion = "", // SDK doesn't expose driver version
            IsConnected = true,
            LastSeen = DateTime.Now,
            // Windows MIDI Services endpoints are bidirectional
            SupportsInput = true,
            SupportsOutput = true,
            IsActive = false
        };
    }

    /// <inheritdoc />
    public IEnumerable<MidiDeviceInfo> GetInputDevices()
    {
        return GetFilteredEndpoints()
            .Select(e => CreateDeviceInfo(e))
            .ToList();
    }

    /// <inheritdoc />
    public IEnumerable<MidiDeviceInfo> GetOutputDevices()
    {
        // Windows MIDI Services endpoints are bidirectional - return same list
        return GetFilteredEndpoints()
            .Select(e => CreateDeviceInfo(e))
            .ToList();
    }

    /// <summary>
    /// Gets filtered endpoints excluding diagnostic/internal endpoints.
    /// </summary>
    private IEnumerable<MidiEndpointDeviceInformation> GetFilteredEndpoints()
    {
        return MidiEndpointDeviceInformation.FindAll()
            .Where(e => AllowedPurposes.Contains(e.EndpointPurpose));
    }

    /// <inheritdoc />
    public bool StartInputDevice(string deviceId)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogError("Cannot start input device: device ID is null or empty");
                return false;
            }

            lock (_lock)
            {
                // Already started?
                if (_inputConnections.ContainsKey(deviceId))
                {
                    _logger.LogDebug("MIDI input device already started: {DeviceId}", deviceId);
                    return true;
                }

                if (_session == null)
                {
                    _logger.LogError("Cannot start input device: session not initialized");
                    return false;
                }

                var connection = _session.CreateEndpointConnection(deviceId);
                if (connection == null)
                {
                    _logger.LogError("Failed to create connection to device {DeviceId}", deviceId);
                    return false;
                }

                // Capture deviceId in closure for message handler
                connection.MessageReceived += (sender, args) => OnMessageReceived(deviceId, args);

                if (!connection.Open())
                {
                    _logger.LogError("Failed to open connection to device {DeviceId}", deviceId);
                    // Dispose the connection that failed to open
                    try { _session.DisconnectEndpointConnection(connection.ConnectionId); } catch { /* ignore */ }
                    return false;
                }

                _inputConnections[deviceId] = connection;
            }

            _logger.LogInformation("Started MIDI input device: {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MIDI input device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <inheritdoc />
    public bool StartOutputDevice(string deviceId)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogError("Cannot start output device: device ID is null or empty");
                return false;
            }

            lock (_lock)
            {
                // Already started?
                if (_outputConnections.ContainsKey(deviceId))
                {
                    _logger.LogDebug("MIDI output device already started: {DeviceId}", deviceId);
                    return true;
                }

                if (_session == null)
                {
                    _logger.LogError("Cannot start output device: session not initialized");
                    return false;
                }

                var connection = _session.CreateEndpointConnection(deviceId);
                if (connection == null)
                {
                    _logger.LogError("Failed to create connection to device {DeviceId}", deviceId);
                    return false;
                }

                if (!connection.Open())
                {
                    _logger.LogError("Failed to open connection to device {DeviceId}", deviceId);
                    // Dispose the connection that failed to open
                    try { _session.DisconnectEndpointConnection(connection.ConnectionId); } catch { /* ignore */ }
                    return false;
                }

                _outputConnections[deviceId] = connection;
            }

            _logger.LogInformation("Started MIDI output device: {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start MIDI output device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <inheritdoc />
    public bool StopInputDevice(string deviceId)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogError("Cannot stop input device: device ID is null or empty");
                return false;
            }

            lock (_lock)
            {
                if (!_inputConnections.TryGetValue(deviceId, out var connection))
                {
                    _logger.LogDebug("MIDI input device not started: {DeviceId}", deviceId);
                    return true; // Already stopped
                }

                try
                {
                    _session?.DisconnectEndpointConnection(connection.ConnectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting input connection for {DeviceId}", deviceId);
                }

                _inputConnections.Remove(deviceId);
            }

            _logger.LogInformation("Stopped MIDI input device: {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MIDI input device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <inheritdoc />
    public bool StopOutputDevice(string deviceId)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogError("Cannot stop output device: device ID is null or empty");
                return false;
            }

            lock (_lock)
            {
                if (!_outputConnections.TryGetValue(deviceId, out var connection))
                {
                    _logger.LogDebug("MIDI output device not started: {DeviceId}", deviceId);
                    return true; // Already stopped
                }

                try
                {
                    _session?.DisconnectEndpointConnection(connection.ConnectionId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error disconnecting output connection for {DeviceId}", deviceId);
                }

                _outputConnections.Remove(deviceId);
            }

            _logger.LogInformation("Stopped MIDI output device: {DeviceId}", deviceId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop MIDI output device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <inheritdoc />
    public bool SendMidiMessage(string deviceId, MidiOutputCommand command)
    {
        try
        {
            if (string.IsNullOrEmpty(deviceId))
            {
                _logger.LogError("Cannot send MIDI: device ID is null or empty");
                return false;
            }

            if (command == null)
            {
                _logger.LogError("Cannot send MIDI: command is null");
                return false;
            }

            MidiEndpointConnection? connection;
            lock (_lock)
            {
                if (!_outputConnections.TryGetValue(deviceId, out connection))
                {
                    _logger.LogError("Cannot send MIDI: device not connected: {DeviceId}", deviceId);
                    return false;
                }
            }

            // Build UMP based on message type
            var ump = BuildUmpFromCommand(command);
            if (ump == null)
            {
                _logger.LogWarning("Message type not supported for output: {MessageType}", command.MessageType);
                return false;
            }

            var result = connection.SendSingleMessagePacket(ump);
            if (!MidiEndpointConnection.SendMessageSucceeded(result))
            {
                _logger.LogError("Failed to send MIDI message: {Result}", result);
                return false;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Sent MIDI message to {DeviceId}: {Command}", deviceId, command);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send MIDI message to device {DeviceId}", deviceId);
            return false;
        }
    }

    /// <summary>
    /// Builds a UMP from a MidiOutputCommand.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if channel is not in valid range 1-16</exception>
    private IMidiUniversalPacket? BuildUmpFromCommand(MidiOutputCommand command)
    {
        // Validate channel is in valid MIDI range (1-16 for MIDIFlux, 0-15 for SDK)
        if (command.Channel < 1 || command.Channel > 16)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                command.Channel,
                $"MIDI channel must be between 1 and 16, but was {command.Channel}");
        }

        // Convert 1-based MIDIFlux channel to 0-based SDK channel
        var channel = new MidiChannel((byte)(command.Channel - 1));
        var group = new MidiGroup(0);
        var timestamp = MidiClock.Now;

        return command.MessageType switch
        {
            MidiFluxMessageType.NoteOff => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.NoteOff,
                channel, (byte)command.Data1, (byte)command.Data2),

            MidiFluxMessageType.NoteOn => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.NoteOn,
                channel, (byte)command.Data1, (byte)command.Data2),

            MidiFluxMessageType.ControlChange => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.ControlChange,
                channel, (byte)command.Data1, (byte)command.Data2),

            MidiFluxMessageType.ProgramChange => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.ProgramChange,
                channel, (byte)command.Data1, 0),

            MidiFluxMessageType.PitchBend => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.PitchBend,
                channel, (byte)command.Data1, (byte)command.Data2),

            MidiFluxMessageType.ChannelPressure => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.ChannelPressure,
                channel, (byte)command.Data1, 0),

            MidiFluxMessageType.Aftertouch => MidiMessageBuilder.BuildMidi1ChannelVoiceMessage(
                timestamp, group, Midi1ChannelVoiceMessageStatus.PolyPressure,
                channel, (byte)command.Data1, (byte)command.Data2),

            MidiFluxMessageType.SysEx => null, // SysEx not supported in Phase 1

            _ => null
        };
    }

    /// <summary>
    /// Handles incoming MIDI messages from the SDK.
    /// </summary>
    private void OnMessageReceived(string deviceId, MidiMessageReceivedEventArgs args)
    {
        try
        {
            // Get the first word for inspection
            uint word0 = args.PeekFirstWord();

            // Check if this is a MIDI 1.0 Channel Voice message (Type 0x2)
            var messageType = args.MessageType;
            if (messageType != SdkMidiMessageType.Midi1ChannelVoice32)
            {
                // Log non-MIDI1 messages at debug level
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug("Ignoring non-MIDI1 message type: {MessageType}", messageType);
                }
                return;
            }

            // Extract status from UMP word0
            var status = MidiMessageHelper.GetStatusFromMidi1ChannelVoiceMessage(word0);

            // Extract channel (0-based from SDK, convert to 1-based for MIDIFlux)
            var sdkChannel = MidiMessageHelper.GetChannelFromMessageFirstWord(word0);
            int midiFluxChannel = sdkChannel.Index + 1;

            // Extract data bytes via bit manipulation
            byte data1 = (byte)((word0 & 0x0000FF00) >> 8);
            byte data2 = (byte)(word0 & 0x000000FF);

            // Map SDK status to MIDIFlux MidiEventType
            MidiEventType eventType = status switch
            {
                Midi1ChannelVoiceMessageStatus.NoteOff => MidiEventType.NoteOff,
                Midi1ChannelVoiceMessageStatus.NoteOn => MidiEventType.NoteOn,
                Midi1ChannelVoiceMessageStatus.PolyPressure => MidiEventType.PolyphonicKeyPressure,
                Midi1ChannelVoiceMessageStatus.ControlChange => MidiEventType.ControlChange,
                Midi1ChannelVoiceMessageStatus.ProgramChange => MidiEventType.ProgramChange,
                Midi1ChannelVoiceMessageStatus.ChannelPressure => MidiEventType.ChannelPressure,
                Midi1ChannelVoiceMessageStatus.PitchBend => MidiEventType.PitchBend,
                _ => MidiEventType.Other
            };

            // Create MIDIFlux event
            var midiEvent = new MidiEvent
            {
                EventType = eventType,
                Channel = midiFluxChannel,
                Timestamp = DateTime.Now,
                RawData = BitConverter.GetBytes(word0)
            };

            // Populate type-specific fields
            switch (eventType)
            {
                case MidiEventType.NoteOn:
                case MidiEventType.NoteOff:
                    midiEvent.Note = data1;
                    midiEvent.Velocity = data2;
                    break;

                case MidiEventType.ControlChange:
                    midiEvent.Controller = data1;
                    midiEvent.Value = data2;
                    break;

                case MidiEventType.ProgramChange:
                    midiEvent.ProgramNumber = data1;
                    break;

                case MidiEventType.PitchBend:
                    // PitchBend: data1=LSB, data2=MSB -> 14-bit value (0-16383, center=8192)
                    midiEvent.PitchBendValue = data1 | (data2 << 7);
                    break;

                case MidiEventType.ChannelPressure:
                    midiEvent.Pressure = data1;
                    break;

                case MidiEventType.PolyphonicKeyPressure:
                    midiEvent.Note = data1;
                    midiEvent.Pressure = data2;
                    break;
            }

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("MIDI Event from {DeviceId}: {MidiEvent}", deviceId, midiEvent);
            }

            // Raise event
            MidiEventReceived?.Invoke(this, new MidiEventArgs(deviceId, midiEvent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI message from {DeviceId}", deviceId);
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<string> GetActiveDeviceIds()
    {
        lock (_lock)
        {
            // Combine input and output device IDs (may overlap for bidirectional endpoints)
            return _inputConnections.Keys
                .Union(_outputConnections.Keys)
                .ToList()
                .AsReadOnly();
        }
    }

    /// <inheritdoc />
    public bool IsDeviceActive(string deviceId)
    {
        if (string.IsNullOrEmpty(deviceId))
            return false;

        lock (_lock)
        {
            return _inputConnections.ContainsKey(deviceId) ||
                   _outputConnections.ContainsKey(deviceId);
        }
    }

    /// <inheritdoc />
    public void RefreshDeviceList()
    {
        // Device watcher handles this automatically via events
        // Force a refresh of known devices from current enumeration
        try
        {
            var currentDevices = GetFilteredEndpoints().ToList();

            lock (_lock)
            {
                _knownDevices.Clear();
                foreach (var device in currentDevices)
                {
                    _knownDevices[device.EndpointDeviceId] = CreateDeviceInfo(device);
                }
            }

            _logger.LogDebug("Refreshed device list: {Count} devices", currentDevices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing device list");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _logger.LogDebug("Disposing WindowsMidiServicesAdapter");

        // Stop device watcher
        try
        {
            if (_deviceWatcher != null)
            {
                _deviceWatcher.Stop();
                _deviceWatcher.Added -= OnDeviceAdded;
                _deviceWatcher.Removed -= OnDeviceRemoved;
                _deviceWatcher.Updated -= OnDeviceUpdated;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error stopping device watcher");
        }

        // Disconnect all connections
        lock (_lock)
        {
            foreach (var connection in _inputConnections.Values)
            {
                try { _session?.DisconnectEndpointConnection(connection.ConnectionId); } catch { /* ignore */ }
            }
            _inputConnections.Clear();

            foreach (var connection in _outputConnections.Values)
            {
                try { _session?.DisconnectEndpointConnection(connection.ConnectionId); } catch { /* ignore */ }
            }
            _outputConnections.Clear();

            _knownDevices.Clear();
        }

        // Dispose session
        try
        {
            _session?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing MIDI session");
        }

        // Dispose initializer
        try
        {
            _initializer?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disposing SDK initializer");
        }

        _logger.LogDebug("WindowsMidiServicesAdapter disposed");
        GC.SuppressFinalize(this);
    }
}

