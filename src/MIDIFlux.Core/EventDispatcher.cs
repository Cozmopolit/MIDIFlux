using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core;

/// <summary>
/// Dispatches MIDI events to unified actions using the new unified action system.
/// Completely replaces the old fragmented handler-based approach.
/// </summary>
public class EventDispatcher
{
    private readonly ILogger _logger;
    private readonly KeyStateManager _keyStateManager;
    private readonly DeviceConfigurationManager _deviceConfigManager;
    private UnifiedActionEventProcessor? _eventProcessor;
    private UnifiedMappingConfig? _configuration;

    /// <summary>
    /// Creates a new instance of the EventDispatcher with unified action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyStateManager">The key state manager to use</param>
    /// <param name="actionFactory">The unified action factory to use</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public EventDispatcher(
        ILogger<EventDispatcher> logger,
        KeyStateManager keyStateManager,
        IUnifiedActionFactory actionFactory,
        IServiceProvider? serviceProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _keyStateManager = keyStateManager ?? throw new ArgumentNullException(nameof(keyStateManager));

        // Create the device configuration manager with unified action system
        _deviceConfigManager = new DeviceConfigurationManager(logger, actionFactory, serviceProvider);

        _logger.LogDebug("EventDispatcher initialized with unified action system");
    }

    /// <summary>
    /// Sets the unified configuration to use for mapping MIDI events to actions.
    /// Completely replaces the old configuration system.
    /// </summary>
    /// <param name="configuration">The unified configuration to use</param>
    public void SetConfiguration(UnifiedMappingConfig configuration)
    {
        try
        {
            // Release all toggled keys when switching configurations
            _keyStateManager.ReleaseAllKeys();

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogInformation("Event dispatcher configured with unified profile '{ProfileName}' containing {DeviceCount} MIDI devices",
                configuration.ProfileName, configuration.MidiDevices.Count);

            // Set the configuration in the device configuration manager
            _deviceConfigManager.SetConfiguration(configuration);

            // Create the optimized event processor with the registry
            var registry = _deviceConfigManager.GetActionRegistry();
            var processorLogger = LoggingHelper.CreateLogger<UnifiedActionEventProcessor>();
            _eventProcessor = new UnifiedActionEventProcessor(processorLogger, registry);

            _logger.LogDebug("Created UnifiedActionEventProcessor for optimized MIDI event processing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting unified configuration in EventDispatcher: {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowError(
                $"Failed to set configuration: {ex.Message}",
                "MIDIFlux - Configuration Error",
                _logger,
                ex);
            throw;
        }
    }

    /// <summary>
    /// Handles a MIDI event using the optimized unified action event processor.
    /// Provides high-performance processing with lock-free registry access and sync-by-default execution.
    /// </summary>
    /// <param name="eventArgs">The MIDI event arguments</param>
    public void HandleMidiEvent(MidiEventArgs eventArgs)
    {
        if (_eventProcessor == null)
        {
            _logger.LogWarning("No unified event processor available, ignoring MIDI event");
            return;
        }

        try
        {
            int deviceId = eventArgs.DeviceId;
            var midiEvent = eventArgs.Event;

            _logger.LogInformation("EventDispatcher received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
                deviceId, midiEvent.EventType, midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);

            // Get device name for optimized processing (pre-resolve to avoid allocation in hot path)
            var deviceName = GetDeviceNameFromId(deviceId);

            // Process the MIDI event through the optimized processor
            bool anyActionExecuted = _eventProcessor.ProcessMidiEvent(deviceId, midiEvent, deviceName);

            if (anyActionExecuted)
            {
                _logger.LogDebug("MIDI event processed successfully by UnifiedActionEventProcessor");
            }
            else
            {
                _logger.LogTrace("No actions executed for MIDI event");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling MIDI event: {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowError(
                $"Error handling MIDI event: {ex.Message}",
                "MIDIFlux - MIDI Event Error",
                _logger,
                ex);
        }
    }

    /// <summary>
    /// Gets the device name from a device ID for optimized processing.
    /// Pre-resolves device names to avoid allocation in the hot path.
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <returns>The device name, or "*" if not found</returns>
    private string GetDeviceNameFromId(int deviceId)
    {
        try
        {
            // Get device configurations for this device ID
            var deviceConfigs = _deviceConfigManager.FindDeviceConfigsForId(deviceId);
            var deviceName = deviceConfigs.FirstOrDefault()?.DeviceName ?? "*";

            _logger.LogTrace("Resolved device ID {DeviceId} to device name '{DeviceName}'", deviceId, deviceName);
            return deviceName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving device name for device ID {DeviceId}, using wildcard: {ErrorMessage}", deviceId, ex.Message);
            return "*";
        }
    }

    /// <summary>
    /// Handles a device disconnection event.
    /// Releases all keys and logs the disconnection.
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

            // With the unified action system, no cleanup of handlers is needed
            // Actions are looked up dynamically from the registry
            _logger.LogDebug("Device {DeviceId} disconnection handled. Actions will be available when device reconnects.", deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device disconnection for device {DeviceId}: {ErrorMessage}", deviceId, ex.Message);
            ApplicationErrorHandler.ShowError(
                $"Error handling device disconnection: {ex.Message}",
                "MIDIFlux - Device Disconnection Error",
                _logger,
                ex);
        }
    }

    /// <summary>
    /// Gets performance statistics from the unified action event processor.
    /// Provides insight into processing performance and registry state.
    /// </summary>
    /// <returns>Processor statistics, or null if no processor is available</returns>
    public ProcessorStatistics? GetProcessorStatistics()
    {
        return _eventProcessor?.GetStatistics();
    }
}
