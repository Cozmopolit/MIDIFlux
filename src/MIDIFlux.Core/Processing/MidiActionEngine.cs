using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Performance;
using MIDIFlux.Core.Config;

namespace MIDIFlux.Core.Processing;

/// <summary>
/// High-performance MIDI event processor for the action system.
/// Handles the complete MIDI processing pipeline from event receipt to action execution.
/// Optimized for lock-free registry access, minimal allocations, and fast execution.
/// Implements sync-by-default execution with comprehensive logging and error handling.
/// </summary>
public class MidiActionEngine
{
    private readonly ILogger _logger;
    private readonly ActionMappingRegistry _registry;
    private readonly MidiLatencyAnalyzer _latencyAnalyzer;
    private readonly DeviceConfigurationManager _deviceConfigManager;

    /// <summary>
    /// Creates a new instance of the MidiActionEngine
    /// </summary>
    /// <param name="logger">The logger to use for comprehensive logging</param>
    /// <param name="registry">The action mapping registry for lock-free lookups</param>
    /// <param name="configurationService">The configuration service for configuration</param>
    /// <param name="deviceConfigManager">The device configuration manager for device name lookups</param>
    public MidiActionEngine(ILogger logger, ActionMappingRegistry registry, ConfigurationService configurationService, DeviceConfigurationManager deviceConfigManager)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));
        _deviceConfigManager = deviceConfigManager ?? throw new ArgumentNullException(nameof(deviceConfigManager));
        _latencyAnalyzer = new MidiLatencyAnalyzer();

        // Configure latency analyzer from settings
        _latencyAnalyzer.IsEnabled = configurationService.GetSetting("Performance.EnableLatencyMeasurement", true);
        _latencyAnalyzer.MaxMeasurements = configurationService.GetSetting("Performance.MaxLatencyMeasurements", 1000);

        _logger.LogDebug("MidiActionEngine initialized with lock-free registry access and performance settings: Enabled={LatencyEnabled}, MaxMeasurements={MaxMeasurements}",
            _latencyAnalyzer.IsEnabled, _latencyAnalyzer.MaxMeasurements);
    }

    /// <summary>
    /// Gets the latency analyzer for performance monitoring
    /// </summary>
    public MidiLatencyAnalyzer LatencyAnalyzer => _latencyAnalyzer;

    /// <summary>
    /// Handles a MIDI event with complete processing pipeline.
    /// Provides high-performance processing with lock-free registry access and async execution.
    /// Uses fire-and-forget pattern to avoid blocking the hardware event thread.
    /// </summary>
    /// <param name="eventArgs">The MIDI event arguments</param>
    public void HandleMidiEvent(MidiEventArgs eventArgs)
    {
        try
        {
            int deviceId = eventArgs.DeviceId;
            var midiEvent = eventArgs.Event;

            // Only log MIDI events when trace logging is enabled to avoid hot path impact
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("MidiActionEngine received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
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
                    bool anyActionExecuted = await ProcessMidiEvent(deviceId, midiEvent, deviceName);

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

            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("Resolved device ID {DeviceId} to device name '{DeviceName}'", deviceId, deviceName);
            }

            return deviceName;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error resolving device name for device ID {DeviceId}, using wildcard", deviceId);
            return "*";
        }
    }

    /// <summary>
    /// Processes a MIDI event through the action system with optimized performance.
    /// Uses lock-free registry access and async execution for proper action behavior.
    /// </summary>
    /// <param name="deviceId">The MIDI device ID that generated the event</param>
    /// <param name="midiEvent">The MIDI event to process</param>
    /// <param name="deviceName">The device name for optimized lookup (pre-resolved to avoid allocation)</param>
    /// <returns>True if any actions were executed, false otherwise</returns>
    public async Task<bool> ProcessMidiEvent(int deviceId, MidiEvent midiEvent, string deviceName)
    {
        if (midiEvent == null)
        {
            _logger.LogWarning("Received null MIDI event, ignoring");
            return false;
        }

        // Start performance monitoring using thread-safe timestamp
        var startTicks = Stopwatch.GetTimestamp();

        // Conditional logging to avoid expensive operations when trace logging is disabled
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Processing MIDI event: DeviceId={DeviceId}, DeviceName={DeviceName}, EventType={EventType}, Channel={Channel}, Note={Note}, Controller={Controller}, Value={Value}",
                deviceId, deviceName, midiEvent.EventType, midiEvent.Channel, midiEvent.Note, midiEvent.Controller, midiEvent.Value ?? midiEvent.Velocity);
        }

        // Step 1: Convert MIDI event to action input (optimized - no allocations)
        var actionInput = ConvertMidiEventToActionInput(deviceName, midiEvent);
        if (actionInput == null)
        {
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("MIDI event type {EventType} not supported by action system", midiEvent.EventType);
            }
            return false;
        }

        // Step 2: O(1) lookup using pre-computed keys (lock-free operation)
        var actions = _registry.FindActions(actionInput);

        if (actions.Count == 0)
        {
            // Only log when trace logging is enabled to avoid hot path impact
            if (_logger.IsEnabled(LogLevel.Trace))
            {
                _logger.LogTrace("No actions found for MIDI input: Device={DeviceName}, Type={InputType}, InputNumber={InputNumber}, Channel={Channel}",
                    actionInput.DeviceName, actionInput.InputType, actionInput.InputNumber, actionInput.Channel);
            }
            return false;
        }

        var lookupTicks = Stopwatch.GetTimestamp() - startTicks;
        if (_logger.IsEnabled(LogLevel.Trace))
        {
            _logger.LogTrace("Found {ActionCount} actions in {LookupTimeTicks} ticks ({LookupTimeMs:F3}ms)",
                actions.Count, lookupTicks, lookupTicks * 1000.0 / Stopwatch.Frequency);
        }

        // Step 3: Execute actions with async execution for proper behavior
        var result = await ExecuteActions(actions, midiEvent.Value ?? midiEvent.Velocity, midiEvent, startTicks);

        // End latency measurement (pass startTicks for thread-safe measurement)
        _latencyAnalyzer.EndMeasurement(startTicks, actions.Count);

        return result;
    }

    /// <summary>
    /// Converts a MIDI event to a action input with optimized performance.
    /// Pre-computes lookup keys to minimize allocations during MIDI event processing.
    /// </summary>
    /// <param name="deviceName">The device name (pre-resolved)</param>
    /// <param name="midiEvent">The MIDI event to convert</param>
    /// <returns>The action input, or null if the event type is not supported</returns>
    private MidiInput? ConvertMidiEventToActionInput(string deviceName, MidiEvent midiEvent)
    {
        var actionInput = new MidiInput
        {
            DeviceName = deviceName,
            Channel = midiEvent.Channel
        };

        // Map MIDI event type to action input type
        switch (midiEvent.EventType)
        {
            case MidiEventType.NoteOn:
                actionInput.InputType = MidiInputType.NoteOn;
                actionInput.InputNumber = midiEvent.Note ?? 0;
                break;

            case MidiEventType.NoteOff:
                actionInput.InputType = MidiInputType.NoteOff;
                actionInput.InputNumber = midiEvent.Note ?? 0;
                break;

            case MidiEventType.ControlChange:
                // Use ControlChangeAbsolute as the canonical runtime type
                // The lookup key generation handles mapping both absolute and relative configurations
                // to the same lookup key, so this will find mappings regardless of GUI configuration
                actionInput.InputType = MidiInputType.ControlChangeAbsolute;
                actionInput.InputNumber = midiEvent.Controller ?? 0;
                break;

            case MidiEventType.SystemExclusive:
                actionInput.InputType = MidiInputType.SysEx;
                actionInput.InputNumber = 0; // SysEx doesn't use input number
                actionInput.SysExPattern = midiEvent.SysExData; // Store the received SysEx data for pattern matching
                break;

            default:
                if (_logger.IsEnabled(LogLevel.Trace))
                {
                    _logger.LogTrace("Unsupported MIDI event type for actions: {EventType}", midiEvent.EventType);
                }
                return null;
        }

        return actionInput;
    }

    /// <summary>
    /// Executes a list of actions with async execution for proper behavior.
    /// Uses comprehensive error handling and performance monitoring.
    /// </summary>
    /// <param name="actions">The actions to execute</param>
    /// <param name="midiValue">The MIDI value to pass to actions</param>
    /// <param name="midiEvent">The original MIDI event for logging context</param>
    /// <param name="processingStartTicks">The timestamp when processing started (for thread-safe timing)</param>
    /// <returns>True if any actions were executed successfully, false otherwise</returns>
    private async Task<bool> ExecuteActions(List<IAction> actions, int? midiValue, MidiEvent midiEvent, long processingStartTicks)
    {
        int successCount = 0;
        int errorCount = 0;
        var executionStartTicks = Stopwatch.GetTimestamp();

        foreach (var action in actions)
        {
            try
            {
                // Async execution for proper behavior (especially DelayAction)
                await action.ExecuteAsync(midiValue);
                successCount++;
            }
            catch (Exception ex)
            {
                errorCount++;

                _logger.LogError(ex, "Error executing action '{ActionType}' ({ActionDescription}): {ErrorMessage}",
                    action.GetType().Name, action.Description, ex.Message);

                // Continue with other actions even if one fails
                // Individual action errors are logged but don't stop processing
            }
        }

        // Only log detailed performance info when trace logging is enabled
        if (_logger.IsEnabled(LogLevel.Trace) && (successCount > 0 || errorCount > 0))
        {
            var currentTicks = Stopwatch.GetTimestamp();
            var executionTicks = currentTicks - executionStartTicks;
            var totalProcessingTicks = currentTicks - processingStartTicks;

            _logger.LogTrace("MIDI event processing completed: {SuccessCount} successful, {ErrorCount} failed actions. " +
                           "Execution time: {ExecutionTimeMs:F3}ms, Total processing time: {TotalTimeMs:F3}ms. " +
                           "Event: {EventType}, Channel={Channel}, Value={Value}",
                successCount, errorCount,
                executionTicks * 1000.0 / Stopwatch.Frequency,
                totalProcessingTicks * 1000.0 / Stopwatch.Frequency,
                midiEvent.EventType, midiEvent.Channel, midiValue);
        }

        return successCount > 0;
    }

    /// <summary>
    /// Gets performance statistics for monitoring and debugging
    /// </summary>
    /// <returns>Performance statistics</returns>
    public ProcessorStatistics GetStatistics()
    {
        var registryStats = _registry.GetStatistics();
        var latencyStats = _latencyAnalyzer.GetStatistics();

        return new ProcessorStatistics
        {
            RegistryStatistics = registryStats,
            LatencyStatistics = latencyStats
        };
    }
}

/// <summary>
/// Performance statistics for the action event processor
/// </summary>
public class ProcessorStatistics
{
    /// <summary>
    /// Registry statistics
    /// </summary>
    public RegistryStatistics RegistryStatistics { get; set; } = new();

    /// <summary>
    /// Latency statistics
    /// </summary>
    public LatencyStatistics LatencyStatistics { get; set; } = new();
}
