using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Services;

/// <summary>
/// Audio playback service that handles audio device management and mixing.
/// Provides low-latency audio playback with concurrent sound support.
/// </summary>
public class AudioPlaybackService : IAudioPlaybackService
{
    private readonly ILogger<AudioPlaybackService> _logger;
    private readonly AudioFormatConverter _formatConverter;
    private WaveOutEvent? _waveOut;
    private MixingSampleProvider? _mixer;
    private readonly object _lock = new object();
    private bool _isInitialized = false;
    private bool _disposed = false;

    /// <summary>
    /// Initializes a new instance of AudioPlaybackService
    /// </summary>
    /// <param name="logger">Logger for error reporting</param>
    /// <param name="formatConverter">Format converter for audio processing</param>
    public AudioPlaybackService(ILogger<AudioPlaybackService> logger, AudioFormatConverter formatConverter)
    {
        _logger = logger;
        _formatConverter = formatConverter;
    }

    /// <summary>
    /// Initializes the audio playback service.
    /// </summary>
    public void Initialize()
    {
        lock (_lock)
        {
            if (_isInitialized || _disposed)
            {
                return;
            }

            try
            {
                _logger.LogDebug("Initializing audio playback service");

                // Create mixer with target format (44.1kHz, 2-channel, 32-bit float)
                _mixer = new MixingSampleProvider(AudioFormatConverter.TargetFormat);
                _mixer.ReadFully = true; // Ensure we read all available samples

                // Create and initialize wave output
                _waveOut = new WaveOutEvent();
                _waveOut.Init(_mixer);
                _waveOut.Play();

                _isInitialized = true;
                _logger.LogInformation("Audio playback service initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize audio playback service: {ErrorMessage}", ex.Message);
                throw;
            }
        }
    }

    /// <summary>
    /// Plays audio data with specified volume and optional device selection.
    /// </summary>
    /// <param name="audioData">Pre-converted 32-bit float audio data</param>
    /// <param name="format">Audio format (should be 44.1kHz, 2-channel, 32-bit float)</param>
    /// <param name="volume">Volume level (0-100)</param>
    /// <param name="deviceName">Optional specific audio device name (null = default device)</param>
    public void PlaySound(float[] audioData, WaveFormat format, int volume, string? deviceName = null)
    {
        lock (_lock)
        {
            if (!_isInitialized || _disposed || _mixer == null)
            {
                _logger.LogWarning("Audio playback service not initialized or disposed");
                return;
            }

            try
            {
                _logger.LogTrace("Playing sound: {SampleCount} samples, volume {Volume}", audioData.Length, volume);

                // Create sample provider from audio data
                var sampleProvider = _formatConverter.CreateSampleProvider(audioData, format);

                // Apply volume control
                var volumeProvider = _formatConverter.ApplyVolumeControl(sampleProvider, volume);

                // Add to mixer for concurrent playback
                _mixer.AddMixerInput(volumeProvider);

                _logger.LogTrace("Sound added to mixer successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to play sound: {ErrorMessage}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Checks if the specified audio device is available for playback.
    /// </summary>
    /// <param name="deviceName">The audio device name to check</param>
    /// <returns>True if the device is available, false otherwise</returns>
    public bool IsDeviceAvailable(string deviceName)
    {
        try
        {
            var devices = GetAvailableDevices();
            return devices.Contains(deviceName, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check device availability for '{DeviceName}': {ErrorMessage}", 
                deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets a list of available audio output devices.
    /// </summary>
    /// <returns>List of available audio device names</returns>
    public List<string> GetAvailableDevices()
    {
        var devices = new List<string>();

        try
        {
            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                var capabilities = WaveOut.GetCapabilities(i);
                devices.Add(capabilities.ProductName);
            }

            _logger.LogDebug("Found {DeviceCount} audio output devices", devices.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enumerate audio devices: {ErrorMessage}", ex.Message);
        }

        return devices;
    }

    /// <summary>
    /// Stops all audio playback and releases resources.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isInitialized)
            {
                return;
            }

            try
            {
                _logger.LogDebug("Stopping audio playback service");

                _waveOut?.Stop();
                _mixer?.RemoveAllMixerInputs();

                _isInitialized = false;
                _logger.LogInformation("Audio playback service stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping audio playback service: {ErrorMessage}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Disposes the audio playback service and releases all resources.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                _logger.LogDebug("Disposing audio playback service");

                Stop();

                _waveOut?.Dispose();
                _waveOut = null;
                _mixer = null;

                _disposed = true;
                _logger.LogDebug("Audio playback service disposed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error disposing audio playback service: {ErrorMessage}", ex.Message);
            }
        }
    }
}
