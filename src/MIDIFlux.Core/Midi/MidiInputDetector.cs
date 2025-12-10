using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Actions;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Detects MIDI input activity for a specified duration and provides aggregated results.
/// Runs independently of existing MIDI processing configuration.
/// Operates parallel to normal MIDI processing without interference.
/// </summary>
public class MidiInputDetector
{
    private readonly MidiDeviceManager _midiDeviceManager;
    private readonly ILogger<MidiInputDetector> _logger;

    /// <summary>
    /// Initializes a new instance of the MidiInputDetector
    /// </summary>
    /// <param name="midiDeviceManager">The MIDI device manager for device access</param>
    /// <param name="logger">The logger for debug and trace output</param>
    public MidiInputDetector(MidiDeviceManager midiDeviceManager, ILogger<MidiInputDetector> logger)
    {
        _midiDeviceManager = midiDeviceManager ?? throw new ArgumentNullException(nameof(midiDeviceManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        
        _logger.LogDebug("MidiInputDetector initialized");
    }

    /// <summary>
    /// Detects MIDI input activity for the specified duration
    /// </summary>
    /// <param name="durationSeconds">Duration to listen for input (1-20 seconds)</param>
    /// <param name="deviceFilter">Optional device name filter, null for all devices</param>
    /// <returns>Aggregated summary of detected MIDI activity</returns>
    public async Task<MidiInputDetectionResult> DetectAsync(int durationSeconds, string? deviceFilter = null)
    {
        // Validate duration
        if (durationSeconds < 1 || durationSeconds > 20)
        {
            throw new ArgumentOutOfRangeException(nameof(durationSeconds), 
                "Duration must be between 1 and 20 seconds");
        }

        _logger.LogInformation("Starting MIDI input detection for {Duration} seconds with device filter: {DeviceFilter}", 
            durationSeconds, deviceFilter ?? "None");

        var result = new MidiInputDetectionResult
        {
            DurationSeconds = durationSeconds
        };

        // Thread-safe collection for aggregating detected inputs
        var detectedInputs = new ConcurrentDictionary<string, DetectedMidiInput>();
        var cancellationTokenSource = new CancellationTokenSource();

        // Event handler for MIDI events
        void OnMidiEventReceived(object? sender, MidiEventArgs e)
        {
            try
            {
                // Apply device filter if specified
                if (!string.IsNullOrWhiteSpace(deviceFilter))
                {
                    var deviceName = GetDeviceNameFromId(e.DeviceId);
                    if (!string.Equals(deviceName, deviceFilter, StringComparison.OrdinalIgnoreCase))
                    {
                        return; // Skip this event
                    }
                }

                // Convert to MidiInput for consistent processing
                var midiInput = ConvertToMidiInput(e);
                if (midiInput != null)
                {
                    // Create aggregation key
                    var key = CreateAggregationKey(midiInput);
                    
                    // Add or update detected input
                    detectedInputs.AddOrUpdate(key, 
                        // Add new entry
                        new DetectedMidiInput
                        {
                            DeviceName = midiInput.DeviceName ?? "Unknown",
                            Channel = midiInput.Channel,
                            InputType = midiInput.InputType.ToString(),
                            InputNumber = midiInput.InputNumber,
                            Count = 1
                        },
                        // Update existing entry
                        (k, existing) =>
                        {
                            existing.Count++;
                            return existing;
                        });

                    if (_logger.IsEnabled(LogLevel.Trace))
                    {
                        _logger.LogTrace("Detected MIDI input: {Key}", key);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing MIDI event during detection");
            }
        }

        try
        {
            // Subscribe to MIDI events
            _midiDeviceManager.MidiEventReceived += OnMidiEventReceived;

            // Wait for the specified duration
            await Task.Delay(TimeSpan.FromSeconds(durationSeconds), cancellationTokenSource.Token);

            // Convert results
            result.DetectedInputs = detectedInputs.Values
                .OrderBy(input => input.DeviceName)
                .ThenBy(input => input.Channel ?? 0)
                .ThenBy(input => input.InputType)
                .ThenBy(input => input.InputNumber ?? 0)
                .ToList();

            _logger.LogInformation("MIDI input detection completed. Found {Count} unique inputs", 
                result.DetectedInputs.Count);

            return result;
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("MIDI input detection was cancelled");
            return result;
        }
        finally
        {
            // Always unsubscribe from events
            _midiDeviceManager.MidiEventReceived -= OnMidiEventReceived;
            cancellationTokenSource.Dispose();
        }
    }

    /// <summary>
    /// Gets the device name from a device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>Device name or "Unknown" if not found</returns>
    private string GetDeviceNameFromId(int deviceId)
    {
        try
        {
            var devices = _midiDeviceManager.GetAvailableDevices();
            var device = devices.FirstOrDefault(d => d.DeviceId == deviceId);
            return device?.Name ?? $"Device {deviceId}";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get device name for ID {DeviceId}", deviceId);
            return $"Device {deviceId}";
        }
    }

    /// <summary>
    /// Converts a MidiEventArgs to a MidiInput for consistent processing
    /// </summary>
    /// <param name="eventArgs">The MIDI event arguments</param>
    /// <returns>MidiInput representation or null if conversion failed</returns>
    private MidiInput? ConvertToMidiInput(MidiEventArgs eventArgs)
    {
        try
        {
            var midiEvent = eventArgs.Event;
            var deviceName = GetDeviceNameFromId(eventArgs.DeviceId);

            // Convert MidiEventType to MidiInputType and determine input number
            var (inputType, inputNumber) = ConvertEventToInputType(midiEvent);

            return new MidiInput
            {
                DeviceName = deviceName,
                Channel = midiEvent.Channel,
                InputType = inputType,
                InputNumber = inputNumber,
                SysExPattern = midiEvent.SysExData
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to convert MIDI event to MidiInput");
            return null;
        }
    }

    /// <summary>
    /// Converts MidiEventType to MidiInputType and extracts input number
    /// </summary>
    /// <param name="midiEvent">The MIDI event</param>
    /// <returns>Tuple of input type and input number</returns>
    private (MidiInputType inputType, int inputNumber) ConvertEventToInputType(MidiEvent midiEvent)
    {
        return midiEvent.EventType switch
        {
            MidiEventType.NoteOn => (MidiInputType.NoteOn, midiEvent.Note ?? 0),
            MidiEventType.NoteOff => (MidiInputType.NoteOff, midiEvent.Note ?? 0),
            MidiEventType.ControlChange => (MidiInputType.ControlChangeAbsolute, midiEvent.Controller ?? 0),
            MidiEventType.SystemExclusive => (MidiInputType.SysEx, 0),
            _ => (MidiInputType.NoteOn, 0) // Default fallback
        };
    }

    /// <summary>
    /// Creates an aggregation key for grouping identical inputs
    /// </summary>
    /// <param name="input">The MIDI input</param>
    /// <returns>Aggregation key string</returns>
    private string CreateAggregationKey(MidiInput input)
    {
        // Group by: DeviceName + Channel + InputType + InputNumber
        // This matches the specification for aggregating identical inputs
        return $"{input.DeviceName ?? "Unknown"}|{input.Channel?.ToString() ?? "*"}|{input.InputType}|{input.InputNumber.ToString()}";
    }
}

/// <summary>
/// Result of MIDI input detection operation
/// </summary>
public class MidiInputDetectionResult
{
    /// <summary>
    /// Duration of the detection in seconds
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    /// List of detected MIDI inputs with aggregated counts
    /// </summary>
    public List<DetectedMidiInput> DetectedInputs { get; set; } = new();
}

/// <summary>
/// Represents a detected MIDI input with aggregated information
/// </summary>
public class DetectedMidiInput
{
    /// <summary>
    /// Name of the MIDI device that generated the input
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// MIDI channel (1-16), null for channel-independent events
    /// </summary>
    public int? Channel { get; set; }

    /// <summary>
    /// Type of MIDI input (NoteOn, NoteOff, ControlChange, etc.)
    /// </summary>
    public string InputType { get; set; } = string.Empty;

    /// <summary>
    /// Input number (note number for notes, controller number for CC)
    /// </summary>
    public int? InputNumber { get; set; }

    /// <summary>
    /// Number of times this input was detected during the detection period
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        var channel = Channel?.ToString() ?? "*";
        var inputNumber = InputNumber?.ToString() ?? "*";
        return $"{DeviceName} - Ch:{channel} - {InputType}:{inputNumber} (Count: {Count})";
    }
}
