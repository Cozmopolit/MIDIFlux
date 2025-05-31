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
public class ProfileManager
{
    private readonly ILogger<ProfileManager> _logger;
    private readonly ActionStateManager _actionStateManager;
    private readonly DeviceConfigurationManager _deviceConfigManager;
    private readonly IServiceProvider? _serviceProvider;
    private MidiActionEngine? _eventProcessor;
    private MappingConfig? _configuration;

    /// <summary>
    /// Creates a new instance of the ProfileManager with action system
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="actionStateManager">The action state manager to use</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public ProfileManager(
        ILogger<ProfileManager> logger,
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

        _logger.LogDebug("ProfileManager initialized with action system");
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
            var processorLogger = LoggingHelper.CreateLogger<MidiActionEngine>();
            var configurationService = _serviceProvider?.GetRequiredService<ConfigurationService>() ?? throw new InvalidOperationException("ConfigurationService not registered in DI container");
            _eventProcessor = new MidiActionEngine(processorLogger, registry, configurationService, _deviceConfigManager);

            _logger.LogDebug("Created MidiActionEngine for optimized MIDI event processing");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting unified configuration in ProfileManager: {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowError(
                $"Failed to set configuration: {ex.Message}",
                "MIDIFlux - Configuration Error",
                _logger,
                ex);
            throw;
        }
    }

    /// <summary>
    /// Handles a MIDI event by delegating to the action event processor.
    /// Simple delegation to the core processing engine.
    /// </summary>
    /// <param name="eventArgs">The MIDI event arguments</param>
    public void HandleMidiEvent(MidiEventArgs eventArgs)
    {
        if (_eventProcessor == null)
        {
            _logger.LogWarning("No event processor available, ignoring MIDI event");
            return;
        }

        // Delegate to the action event processor which handles the complete pipeline
        _eventProcessor.HandleMidiEvent(eventArgs);
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
            _logger.LogInformation("Latency measurement enabled in ProfileManager");
        }
        else
        {
            _logger.LogWarning("Cannot enable latency measurement - event processor not available");
        }
    }
}
