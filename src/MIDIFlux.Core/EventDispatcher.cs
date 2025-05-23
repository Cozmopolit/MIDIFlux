using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core;

/// <summary>
/// Dispatches MIDI events to keyboard actions
/// </summary>
public class EventDispatcher
{
    private readonly ILogger _logger;
    private readonly KeyStateManager _keyStateManager;
    private readonly DeviceConfigurationManager _deviceConfigManager;
    private readonly MidiEventHandlers _eventHandlers;
    private Models.Configuration? _configuration;

    /// <summary>
    /// Creates a new instance of the EventDispatcher
    /// </summary>
    /// <param name="keyboardSimulator">The keyboard simulator to use</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="handlerFactory">The handler factory to use</param>
    /// <param name="keyStateManager">The key state manager to use</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public EventDispatcher(
        KeyboardSimulator keyboardSimulator,
        ILogger<EventDispatcher> logger,
        HandlerFactory handlerFactory,
        KeyStateManager keyStateManager,
        IServiceProvider? serviceProvider = null)
    {
        _logger = logger;
        _keyStateManager = keyStateManager;

        // Create the keyboard action executor
        var keyboardActionExecutor = new KeyboardActionExecutor(keyboardSimulator, logger, keyStateManager);

        // Create the action factory
        var actionFactory = new ActionFactory(
            LoggingHelper.CreateLogger<ActionFactory>(),
            keyboardSimulator,
            keyStateManager);

        // Create the device configuration manager
        _deviceConfigManager = new DeviceConfigurationManager(logger, handlerFactory, actionFactory, serviceProvider);

        // Create the event handlers
        _eventHandlers = new MidiEventHandlers(logger, keyboardActionExecutor, _deviceConfigManager, handlerFactory, keyStateManager);
    }

    /// <summary>
    /// Sets the configuration to use for mapping MIDI events to keyboard actions
    /// </summary>
    /// <param name="configuration">The configuration to use</param>
    public void SetConfiguration(Models.Configuration configuration)
    {
        // Release all toggled keys when switching configurations
        _keyStateManager.ReleaseAllKeys();

        _configuration = configuration;
        _logger.LogInformation("Event dispatcher configured with {DeviceCount} MIDI devices", configuration.MidiDevices.Count);

        // Set the configuration in the device configuration manager
        _deviceConfigManager.SetConfiguration(configuration);
    }

    /// <summary>
    /// Handles a MIDI event
    /// </summary>
    /// <param name="eventArgs">The MIDI event arguments</param>
    public void HandleMidiEvent(MidiEventArgs eventArgs)
    {
        if (_configuration == null)
        {
            _logger.LogWarning("No configuration set, ignoring MIDI event");
            return;
        }

        int deviceId = eventArgs.DeviceId;
        var midiEvent = eventArgs.Event;

        _logger.LogInformation("EventDispatcher received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
            deviceId, midiEvent.EventType, midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);

        // Find all matching device configurations
        var matchingConfigs = _deviceConfigManager.FindDeviceConfigsForId(deviceId);
        if (matchingConfigs.Count == 0)
        {
            _logger.LogWarning("No device configurations available for DeviceId={DeviceId}, ignoring MIDI event", deviceId);
            return;
        }

        _logger.LogDebug("Found {Count} matching device configurations for DeviceId={DeviceId}", matchingConfigs.Count, deviceId);

        bool eventHandled = false;

        // Process the event for each matching configuration
        foreach (var deviceConfig in matchingConfigs)
        {
            // Check if we should handle events on this channel
            if (deviceConfig.MidiChannels != null && deviceConfig.MidiChannels.Count > 0 &&
                !deviceConfig.MidiChannels.Contains(midiEvent.Channel))
            {
                _logger.LogDebug("Skipping configuration for device {DeviceName} - event on channel {Channel} not in configured channels {ConfiguredChannels}",
                    deviceConfig.DeviceName, midiEvent.Channel, string.Join(", ", deviceConfig.MidiChannels));
                continue;
            }

            // Handle different types of MIDI events
            switch (midiEvent.EventType)
            {
                case MidiEventType.NoteOn:
                case MidiEventType.NoteOff:
                    _eventHandlers.HandleNoteEvent(deviceId, midiEvent, deviceConfig);
                    eventHandled = true;
                    break;

                case MidiEventType.ControlChange:
                    _eventHandlers.HandleControlChangeEvent(deviceId, midiEvent, deviceConfig);
                    eventHandled = true;
                    break;

                default:
                    _logger.LogDebug("Unsupported event type: {EventType}", midiEvent.EventType);
                    break;
            }
        }

        if (!eventHandled)
        {
            _logger.LogDebug("Event not handled by any configuration: Device ID {DeviceId}, Channel {Channel}, Event Type {EventType}",
                deviceId, midiEvent.Channel, midiEvent.EventType);
        }
    }

    /// <summary>
    /// Handles a device disconnection event
    /// </summary>
    /// <param name="deviceId">The ID of the disconnected device</param>
    /// <param name="eventArgs">The MIDI event arguments</param>
    public void HandleDeviceDisconnection(int deviceId, MidiEventArgs eventArgs)
    {
        try
        {
            _logger.LogWarning("Handling device disconnection for device {DeviceId}", deviceId);

            // Release all keys that might be held down by this device
            _keyStateManager.ReleaseAllKeys();

            // Log the disconnection with detailed information
            _logger.LogInformation("Device {DeviceId} disconnected. All keys have been released.", deviceId);

            // We could also clean up any device-specific resources here
            // We don't remove the handlers as they'll be needed when the device reconnects
            _logger.LogDebug("Keeping handlers for device {DeviceId} for potential reconnection", deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device disconnection for device {DeviceId}", deviceId);
        }
    }
}
