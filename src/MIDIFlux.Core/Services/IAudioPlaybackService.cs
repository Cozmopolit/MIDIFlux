using NAudio.Wave;

namespace MIDIFlux.Core.Services;

/// <summary>
/// Interface for audio playback service that handles audio device management and mixing.
/// Provides low-latency audio playback for PlaySoundAction with concurrent sound support.
/// </summary>
public interface IAudioPlaybackService : IDisposable
{
    /// <summary>
    /// Plays audio data with specified volume and optional device selection.
    /// Supports concurrent playback of multiple sounds.
    /// </summary>
    /// <param name="audioData">Pre-converted 32-bit float audio data (44.1kHz stereo)</param>
    /// <param name="format">Audio format (should be 44.1kHz, 2-channel, 32-bit float)</param>
    /// <param name="volume">Volume level (0-100)</param>
    /// <param name="deviceName">Optional specific audio device name (null = default device)</param>
    void PlaySound(float[] audioData, WaveFormat format, int volume, string? deviceName = null);

    /// <summary>
    /// Checks if the specified audio device is available for playback.
    /// </summary>
    /// <param name="deviceName">The audio device name to check</param>
    /// <returns>True if the device is available, false otherwise</returns>
    bool IsDeviceAvailable(string deviceName);

    /// <summary>
    /// Gets a list of available audio output devices.
    /// </summary>
    /// <returns>List of available audio device names</returns>
    List<string> GetAvailableDevices();

    /// <summary>
    /// Initializes the audio playback service.
    /// Should be called once during application startup.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Stops all audio playback and releases resources.
    /// </summary>
    void Stop();
}
