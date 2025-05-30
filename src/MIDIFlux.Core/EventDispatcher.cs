using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using MIDIFlux.Core.State;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core;

/// <summary>
/// Dispatches MIDI events to actions using the new action system.
/// Completely replaces the old fragmented handler-based approach.
/// </summary>
public class EventDispatcher
{
    private readonly ILogger<EventDispatcher> _logger;
    private readonly ActionStateManager _actionStateManager;
    private readonly DeviceConfigurationManager _deviceConfigManager;
    private readonly IServiceProvider? _serviceProvider;
    private ActionEventProcessor? _eventProcessor;
    private MappingConfig? _configuration;

    /// <summary>
    /// Creates a new instance of the EventDispatcher with action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="actionStateManager">The action state manager to use</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public EventDispatcher(
        ILogger<EventDispatcher> logger,
        ActionStateManager actionStateManager,
        IServiceProvider? serviceProvider = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _actionStateManager = actionStateManager ?? throw new ArgumentNullException(nameof(actionStateManager));
        _serviceProvider = serviceProvider;

        // Create the device configuration manager with action system
        var deviceConfigLogger = LoggingHelper.CreateLogger<DeviceConfigurationManager>();
        var configurationService = serviceProvider?.GetRequiredService<ConfigurationService>() ?? throw new InvalidOperationException("ConfigurationService not registered in DI container");
        _deviceConfigManager = new DeviceConfigurationManager(deviceConfigLogger, configurationService, serviceProvider);

        _logger.LogDebug("EventDispatcher initialized with action system");
    }

    /// <summary>
    /// Sets the unified configuration to use for mapping MIDI events to actions.
    /// Completely replaces the old configuration system.
    /// </summary>
    /// <param name="configuration">The unified configuration to use</param>
    public void SetConfiguration(MappingConfig configuration)
    {
        try
        {
            // Initialize states from profile configuration (also releases all keys and clears states)
            if (configuration.InitialStates != null)
            {
                _actionStateManager.InitializeStates(configuration.InitialStates);
            }
            else
            {
                _actionStateManager.ClearAllStates();
            }

            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _logger.LogInformation("Event dispatcher configured with unified profile '{ProfileName}' containing {DeviceCount} MIDI devices",
                configuration.ProfileName, configuration.MidiDevices.Count);

            // Set the configuration in the device configuration manager
            _deviceConfigManager.SetConfiguration(configuration);

            // Create the optimized event processor with the registry and settings
            var registry = _deviceConfigManager.GetActionRegistry();
            var processorLogger = LoggingHelper.CreateLogger<ActionEventProcessor>();
            var configurationService = _serviceProvider?.GetRequiredService<ConfigurationService>() ?? throw new InvalidOperationException("ConfigurationService not registered in DI container");
            _eventProcessor = new ActionEventProcessor(processorLogger, registry, configurationService);

            _logger.LogDebug("Created ActionEventProcessor for optimized MIDI event processing");
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
    /// Handles a MIDI event using the optimized action event processor.
    /// Provides high-performance processing with lock-free registry access and async execution.
    /// Uses fire-and-forget pattern to avoid blocking the hardware event thread.
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

            // Only log MIDI events when trace logging is enabled to avoid hot path impact
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("EventDispatcher received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
                    deviceId, midiEvent.EventType, midiEvent.Channel, midiEvent.Note, midiEvent.Velocity);
            }

            // Get device name for optimized processing (pre-resolve to avoid allocation in hot path)
            var deviceName = GetDeviceNameFromId(deviceId);

            // Process the MIDI event asynchronously to avoid blocking hardware thread
            // Use fire-and-forget pattern since MIDI events should be processed independently
            _ = Task.Run(async () =>
            {
                try
                {
                    bool anyActionExecuted = await _eventProcessor.ProcessMidiEvent(deviceId, midiEvent, deviceName);

                    // Success - no logging needed for performance
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in async MIDI event processing for device {DeviceId}", deviceId);
                    // Error handling for async processing - just log, don't show UI errors for background processing
                }
            });
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

            // Prioritize specific device names over wildcards
            // First try to find a non-wildcard device name
            var specificDevice = deviceConfigs.FirstOrDefault(config => config.DeviceName != "*");
            var deviceName = specificDevice?.DeviceName ?? deviceConfigs.FirstOrDefault()?.DeviceName ?? "*";

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
            _actionStateManager.ReleaseAllPressedKeys();

            // Log the disconnection with detailed information
            _logger.LogInformation("Device {DeviceId} disconnected. All keys have been released.", deviceId);

            // With the action system, no cleanup of handlers is needed
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
    /// Gets performance statistics from the action event processor.
    /// Provides insight into processing performance and registry state.
    /// </summary>
    /// <returns>Processor statistics, or null if no processor is available</returns>
    public ProcessorStatistics? GetProcessorStatistics()
    {
        return _eventProcessor?.GetStatistics();
    }

    /// <summary>
    /// Enables latency measurement for performance analysis
    /// </summary>
    public void EnableLatencyMeasurement()
    {
        if (_eventProcessor != null)
        {
            _eventProcessor.LatencyAnalyzer.IsEnabled = true;
            _logger.LogInformation("Latency measurement enabled in EventDispatcher");
        }
        else
        {
            _logger.LogWarning("Cannot enable latency measurement - event processor not available");
        }
    }
}
