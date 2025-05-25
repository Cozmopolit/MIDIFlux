using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Processing;

/// <summary>
/// High-performance MIDI event processor for the unified action system.
/// Optimized for lock-free registry access, minimal allocations, and fast execution.
/// Implements sync-by-default execution with comprehensive logging and error handling.
/// </summary>
public class UnifiedActionEventProcessor
{
    private readonly ILogger _logger;
    private readonly UnifiedActionMappingRegistry _registry;

    // Performance monitoring
    private readonly Stopwatch _stopwatch = new();

    /// <summary>
    /// Creates a new instance of the UnifiedActionEventProcessor
    /// </summary>
    /// <param name="logger">The logger to use for comprehensive logging</param>
    /// <param name="registry">The unified action mapping registry for lock-free lookups</param>
    public UnifiedActionEventProcessor(ILogger logger, UnifiedActionMappingRegistry registry)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _registry = registry ?? throw new ArgumentNullException(nameof(registry));

        _logger.LogDebug("UnifiedActionEventProcessor initialized with lock-free registry access");
    }

    /// <summary>
    /// Processes a MIDI event through the unified action system with optimized performance.
    /// Uses lock-free registry access and sync-by-default execution for maximum throughput.
    /// </summary>
    /// <param name="deviceId">The MIDI device ID that generated the event</param>
    /// <param name="midiEvent">The MIDI event to process</param>
    /// <param name="deviceName">The device name for optimized lookup (pre-resolved to avoid allocation)</param>
    /// <returns>True if any actions were executed, false otherwise</returns>
    public bool ProcessMidiEvent(int deviceId, MidiEvent midiEvent, string deviceName)
    {
        if (midiEvent == null)
        {
            _logger.LogWarning("Received null MIDI event, ignoring");
            return false;
        }

        try
        {
            // Start performance monitoring
            _stopwatch.Restart();

            _logger.LogTrace("Processing MIDI event: DeviceId={DeviceId}, DeviceName={DeviceName}, EventType={EventType}, Channel={Channel}, Note={Note}, Controller={Controller}, Value={Value}",
                deviceId, deviceName, midiEvent.EventType, midiEvent.Channel, midiEvent.Note, midiEvent.Controller, midiEvent.Value ?? midiEvent.Velocity);

            // Step 1: Convert MIDI event to unified action input (optimized - no allocations)
            var actionInput = ConvertMidiEventToActionInput(deviceName, midiEvent);
            if (actionInput == null)
            {
                _logger.LogTrace("MIDI event type {EventType} not supported by unified action system", midiEvent.EventType);
                return false;
            }

            // Step 2: O(1) lookup using pre-computed keys (lock-free operation)
            var actions = _registry.FindActions(actionInput);
            if (actions.Count == 0)
            {
                _logger.LogTrace("No actions found for MIDI input: Device={DeviceName}, Type={InputType}, InputNumber={InputNumber}, Channel={Channel}",
                    actionInput.DeviceName, actionInput.InputType, actionInput.InputNumber, actionInput.Channel);
                return false;
            }

            var lookupTime = _stopwatch.ElapsedTicks;
            _logger.LogTrace("Found {ActionCount} actions in {LookupTimeTicks} ticks ({LookupTimeMs:F3}ms)",
                actions.Count, lookupTime, lookupTime * 1000.0 / Stopwatch.Frequency);

            // Step 3: Execute actions with sync-by-default for performance
            return ExecuteActions(actions, midiEvent.Value ?? midiEvent.Velocity, midiEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing MIDI event from device {DeviceId} ({DeviceName}): {ErrorMessage}",
                deviceId, deviceName, ex.Message);

            // Use existing error handling patterns
            ApplicationErrorHandler.ShowError(
                $"Error processing MIDI event from device '{deviceName}': {ex.Message}",
                "MIDIFlux - MIDI Processing Error",
                _logger,
                ex);

            return false;
        }
    }

    /// <summary>
    /// Converts a MIDI event to a unified action input with optimized performance.
    /// Pre-computes lookup keys to minimize allocations during MIDI event processing.
    /// </summary>
    /// <param name="deviceName">The device name (pre-resolved)</param>
    /// <param name="midiEvent">The MIDI event to convert</param>
    /// <returns>The unified action input, or null if the event type is not supported</returns>
    private UnifiedActionMidiInput? ConvertMidiEventToActionInput(string deviceName, MidiEvent midiEvent)
    {
        var actionInput = new UnifiedActionMidiInput
        {
            DeviceName = deviceName,
            Channel = midiEvent.Channel
        };

        // Map MIDI event type to unified action input type
        switch (midiEvent.EventType)
        {
            case MidiEventType.NoteOn:
                actionInput.InputType = UnifiedActionMidiInputType.NoteOn;
                actionInput.InputNumber = midiEvent.Note ?? 0;
                break;

            case MidiEventType.NoteOff:
                actionInput.InputType = UnifiedActionMidiInputType.NoteOff;
                actionInput.InputNumber = midiEvent.Note ?? 0;
                break;

            case MidiEventType.ControlChange:
                actionInput.InputType = UnifiedActionMidiInputType.ControlChange;
                actionInput.InputNumber = midiEvent.Controller ?? 0;
                break;

            case MidiEventType.SystemExclusive:
                actionInput.InputType = UnifiedActionMidiInputType.SysEx;
                actionInput.InputNumber = 0; // SysEx doesn't use input number
                actionInput.SysExPattern = midiEvent.SysExData; // Store the received SysEx data for pattern matching
                break;

            default:
                _logger.LogTrace("Unsupported MIDI event type for unified actions: {EventType}", midiEvent.EventType);
                return null;
        }

        return actionInput;
    }

    /// <summary>
    /// Executes a list of actions with sync-by-default performance optimization.
    /// Uses comprehensive error handling and performance monitoring.
    /// </summary>
    /// <param name="actions">The actions to execute</param>
    /// <param name="midiValue">The MIDI value to pass to actions</param>
    /// <param name="midiEvent">The original MIDI event for logging context</param>
    /// <returns>True if any actions were executed successfully, false otherwise</returns>
    private bool ExecuteActions(List<IUnifiedAction> actions, int? midiValue, MidiEvent midiEvent)
    {
        int successCount = 0;
        int errorCount = 0;
        var executionStartTicks = _stopwatch.ElapsedTicks;

        foreach (var action in actions)
        {
            try
            {
                var actionStartTicks = _stopwatch.ElapsedTicks;

                _logger.LogTrace("Executing action: {ActionId} - {ActionDescription}", action.Id, action.Description);

                // Sync-by-default execution for performance (hot path)
                action.Execute(midiValue);

                var actionEndTicks = _stopwatch.ElapsedTicks;
                var actionDurationTicks = actionEndTicks - actionStartTicks;

                successCount++;

                _logger.LogTrace("Successfully executed action '{ActionDescription}' in {DurationTicks} ticks ({DurationMs:F3}ms)",
                    action.Description, actionDurationTicks, actionDurationTicks * 1000.0 / Stopwatch.Frequency);
            }
            catch (Exception ex)
            {
                errorCount++;

                _logger.LogError(ex, "Error executing action '{ActionId}' ({ActionDescription}): {ErrorMessage}",
                    action.Id, action.Description, ex.Message);

                // Use existing error handling patterns - continue with other actions
                ApplicationErrorHandler.ShowError(
                    $"Error executing action '{action.Description}': {ex.Message}",
                    "MIDIFlux - Action Execution Error",
                    _logger,
                    ex);
            }
        }

        var totalExecutionTicks = _stopwatch.ElapsedTicks - executionStartTicks;
        var totalProcessingTicks = _stopwatch.ElapsedTicks;

        // Comprehensive logging as specified
        if (successCount > 0 || errorCount > 0)
        {
            _logger.LogDebug("MIDI event processing completed: {SuccessCount} successful, {ErrorCount} failed actions. " +
                           "Execution time: {ExecutionTimeMs:F3}ms, Total processing time: {TotalTimeMs:F3}ms. " +
                           "Event: {EventType}, Channel={Channel}, Value={Value}",
                successCount, errorCount,
                totalExecutionTicks * 1000.0 / Stopwatch.Frequency,
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

        return new ProcessorStatistics
        {
            RegistryStatistics = registryStats,
            LastProcessingTimeMs = _stopwatch.ElapsedTicks * 1000.0 / Stopwatch.Frequency
        };
    }
}

/// <summary>
/// Performance statistics for the unified action event processor
/// </summary>
public class ProcessorStatistics
{
    /// <summary>
    /// Registry statistics
    /// </summary>
    public RegistryStatistics RegistryStatistics { get; set; } = new();

    /// <summary>
    /// Last processing time in milliseconds
    /// </summary>
    public double LastProcessingTimeMs { get; set; }
}
