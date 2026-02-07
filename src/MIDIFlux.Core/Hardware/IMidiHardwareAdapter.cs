using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Hardware;

/// <summary>
/// Hardware abstraction layer for MIDI input and output operations.
/// Provides a clean interface that hides implementation complexity and ensures consistent 1-based channel numbering (1-16) throughout MIDIFlux.
/// </summary>
/// <remarks>
/// <para>
/// <strong>Channel Numbering Contract:</strong>
/// All channel numbers at the adapter boundary are 1-based (1-16). This applies to:
/// - All MidiEventArgs.MidiEvent.Channel values from input events
/// - All MidiOutputCommand.Channel values for output
/// </para>
/// <para>
/// <strong>Implementation Notes:</strong>
/// How each adapter achieves 1-based channels internally is implementation-specific:
/// - NAudioMidiAdapter: NAudio uses 1-based channels since v1.0.0, so no conversion is needed for most operations.
///   Only raw message construction requires internal conversion to 0-based wire format.
/// - WindowsMidiServicesAdapter: May have different internal handling.
/// </para>
/// <para>
/// Channel validation ensures channels are in the valid range of 1-16.
/// </para>
/// </remarks>
public interface IMidiHardwareAdapter : IDisposable
{
    /// <summary>
    /// Gets all available MIDI input devices.
    /// </summary>
    /// <returns>Collection of available MIDI input devices with their capabilities and status</returns>
    /// <remarks>
    /// Device IDs returned are stable for the current session and can be used with StartInputDevice/StopInputDevice.
    /// Device availability may change due to hardware connections/disconnections.
    /// </remarks>
    IEnumerable<MidiDeviceInfo> GetInputDevices();

    /// <summary>
    /// Gets all available MIDI output devices.
    /// </summary>
    /// <returns>Collection of available MIDI output devices with their capabilities and status</returns>
    /// <remarks>
    /// Device IDs returned are stable for the current session and can be used with StartOutputDevice/StopOutputDevice.
    /// Device availability may change due to hardware connections/disconnections.
    /// </remarks>
    IEnumerable<MidiDeviceInfo> GetOutputDevices();

    /// <summary>
    /// Starts listening for MIDI events from the specified input device.
    /// </summary>
    /// <param name="deviceId">The device ID to start listening to (format depends on adapter implementation)</param>
    /// <returns>True if the device was started successfully, false if the operation failed</returns>
    /// <remarks>
    /// Starting an already started device returns true without error.
    /// MIDI events from this device will be raised through the MidiEventReceived event.
    /// All channel numbers in received events will be 1-based (1-16).
    /// Device ID format: NAudio uses "0", "1", "2"...; Windows MIDI Services uses endpoint strings.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when deviceId is invalid</exception>
    bool StartInputDevice(string deviceId);

    /// <summary>
    /// Starts the specified output device for sending MIDI messages.
    /// </summary>
    /// <param name="deviceId">The device ID to start for output (format depends on adapter implementation)</param>
    /// <returns>True if the device was started successfully, false if the operation failed</returns>
    /// <remarks>
    /// Starting an already started device returns true without error.
    /// The device must be started before calling SendMidiMessage.
    /// Device ID format: NAudio uses "0", "1", "2"...; Windows MIDI Services uses endpoint strings.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when deviceId is invalid</exception>
    bool StartOutputDevice(string deviceId);

    /// <summary>
    /// Stops listening for MIDI events from the specified input device.
    /// </summary>
    /// <param name="deviceId">The device ID to stop listening to</param>
    /// <returns>True if the device was stopped successfully, false if the operation failed</returns>
    /// <remarks>
    /// Stopping an already stopped device returns true without error.
    /// No more MIDI events will be received from this device after stopping.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when deviceId is invalid</exception>
    bool StopInputDevice(string deviceId);

    /// <summary>
    /// Stops the specified output device.
    /// </summary>
    /// <param name="deviceId">The device ID to stop</param>
    /// <returns>True if the device was stopped successfully, false if the operation failed</returns>
    /// <remarks>
    /// Stopping an already stopped device returns true without error.
    /// SendMidiMessage calls will fail for stopped devices.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when deviceId is invalid</exception>
    bool StopOutputDevice(string deviceId);

    /// <summary>
    /// Sends a MIDI message to the specified output device.
    /// </summary>
    /// <param name="deviceId">The output device ID to send the message to</param>
    /// <param name="command">The MIDI output command containing message details and 1-based channel (1-16)</param>
    /// <returns>True if the message was sent successfully, false if the operation failed</returns>
    /// <remarks>
    /// The output device must be started with StartOutputDevice before sending messages.
    /// Channel numbers in the command must be 1-based (1-16) - conversion to native format is handled internally.
    /// SysEx messages, raw MIDI data, and all standard MIDI message types are supported.
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when deviceId is invalid or command contains invalid data</exception>
    /// <exception cref="ArgumentNullException">Thrown when command is null</exception>
    bool SendMidiMessage(string deviceId, MidiOutputCommand command);

    /// <summary>
    /// Gets the list of currently active device IDs (both input and output).
    /// </summary>
    /// <returns>Read-only list of device IDs that are currently started</returns>
    /// <remarks>
    /// Returns device IDs for all devices that have been started with StartInputDevice or StartOutputDevice
    /// and have not been stopped. Used for diagnostics and UI display.
    /// </remarks>
    IReadOnlyList<string> GetActiveDeviceIds();

    /// <summary>
    /// Checks if a specific device is currently active (being listened to or used for output).
    /// </summary>
    /// <param name="deviceId">The device ID to check</param>
    /// <returns>True if the device is currently active, false otherwise</returns>
    /// <remarks>
    /// Returns true if the device has been started with StartInputDevice or StartOutputDevice
    /// and has not been stopped. Used to distinguish between available and actively used devices.
    /// </remarks>
    bool IsDeviceActive(string deviceId);

    /// <summary>
    /// Refreshes the internal device list cache.
    /// </summary>
    /// <remarks>
    /// Forces a refresh of the available device list to detect newly connected or disconnected devices.
    /// Should be called when device availability may have changed due to hardware connections/disconnections.
    /// </remarks>
    void RefreshDeviceList();

    /// <summary>
    /// Event raised when a MIDI event is received from any started input device.
    /// </summary>
    /// <remarks>
    /// All channel numbers in event data are 1-based (1-16) for consistency with MIDIFlux conventions.
    /// Events include device ID to identify the source device.
    /// Error events may be raised for input device failures or invalid MIDI data.
    /// </remarks>
    event EventHandler<MidiEventArgs> MidiEventReceived;

    /// <summary>
    /// Event raised when a MIDI device is connected.
    /// </summary>
    /// <remarks>
    /// This event is raised when a new MIDI device becomes available.
    /// Device monitoring must be enabled for this event to be raised.
    /// </remarks>
    event EventHandler<MidiDeviceInfo> DeviceConnected;

    /// <summary>
    /// Event raised when a MIDI device is disconnected.
    /// </summary>
    /// <remarks>
    /// This event is raised when a MIDI device becomes unavailable.
    /// Device monitoring must be enabled for this event to be raised.
    /// </remarks>
    event EventHandler<MidiDeviceInfo> DeviceDisconnected;
}
