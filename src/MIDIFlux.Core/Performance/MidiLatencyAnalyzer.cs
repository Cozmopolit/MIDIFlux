using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Performance;

/// <summary>
/// Simple MIDI latency analyzer for performance monitoring.
/// Measures timing from MIDI input to action execution completion.
/// </summary>
public class MidiLatencyAnalyzer
{
    private readonly ILogger _logger;
    private readonly ConcurrentQueue<double> _latencyMeasurements = new();
    private int _maxMeasurements = 1000;
    private bool _isEnabled = false;
    private long _startTicks;

    public MidiLatencyAnalyzer()
    {
        _logger = LoggingHelper.CreateLogger<MidiLatencyAnalyzer>();
    }

    /// <summary>
    /// Enables or disables latency measurement
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    /// <summary>
    /// Maximum number of measurements to keep in memory
    /// </summary>
    public int MaxMeasurements
    {
        get => _maxMeasurements;
        set => _maxMeasurements = Math.Max(100, value);
    }

    /// <summary>
    /// Starts measuring latency for a MIDI event
    /// </summary>
    public void StartMeasurement()
    {
        if (_isEnabled)
        {
            _startTicks = Stopwatch.GetTimestamp();
        }
    }

    /// <summary>
    /// Ends measurement and records the latency
    /// </summary>
    /// <param name="actionCount">Number of actions executed</param>
    public void EndMeasurement(int actionCount)
    {
        if (!_isEnabled)
            return;

        var endTicks = Stopwatch.GetTimestamp();
        var latencyTicks = endTicks - _startTicks;
        var latencyMs = latencyTicks * 1000.0 / Stopwatch.Frequency;

        _latencyMeasurements.Enqueue(latencyMs);

        // Trim old measurements if we exceed the limit
        while (_latencyMeasurements.Count > _maxMeasurements)
        {
            _latencyMeasurements.TryDequeue(out _);
        }

        // Log high latency events
        if (latencyMs > 10.0)
        {
            _logger.LogWarning("High MIDI latency detected: {LatencyMs:F3}ms, {ActionCount} actions executed", latencyMs, actionCount);
        }
    }

    /// <summary>
    /// Gets current latency statistics
    /// </summary>
    public LatencyStatistics GetStatistics()
    {
        var measurements = _latencyMeasurements.ToArray();
        if (measurements.Length == 0)
        {
            return new LatencyStatistics();
        }

        Array.Sort(measurements);

        int p95Index = Math.Max(0, Math.Min(measurements.Length - 1, (int)Math.Ceiling(measurements.Length * 0.95) - 1));
        int highLatencyCount = measurements.Count(m => m > 10.0);

        return new LatencyStatistics
        {
            TotalMeasurements = measurements.Length,
            AverageLatencyMs = measurements.Average(),
            MinLatencyMs = measurements.Min(),
            MaxLatencyMs = measurements.Max(),
            P95LatencyMs = measurements[p95Index],
            HighLatencyCount = highLatencyCount
        };
    }

    /// <summary>
    /// Clears all measurements
    /// </summary>
    public void ClearMeasurements()
    {
        while (_latencyMeasurements.TryDequeue(out _)) { }
    }
}

/// <summary>
/// Simple latency statistics summary
/// </summary>
public class LatencyStatistics
{
    public int TotalMeasurements { get; set; }
    public double AverageLatencyMs { get; set; }
    public double MinLatencyMs { get; set; }
    public double MaxLatencyMs { get; set; }
    public double P95LatencyMs { get; set; }
    public int HighLatencyCount { get; set; }
}
