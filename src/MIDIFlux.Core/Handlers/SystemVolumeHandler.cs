using System.Runtime.Versioning;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace MIDIFlux.Core.Handlers;

/// <summary>
/// Handles system volume control using NAudio
/// </summary>
[SupportedOSPlatform("windows")]
public class SystemVolumeHandler : IAbsoluteValueHandler, IDisposable
{
    private readonly ILogger _logger;
    private MMDevice? _device;
    private bool _initialized;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => "System Volume Control";

    /// <summary>
    /// Creates a new instance of the SystemVolumeHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    public SystemVolumeHandler(ILogger logger)
    {
        _logger = logger;
        InitializeAudio();
    }

    /// <summary>
    /// Initializes the audio endpoint volume interface
    /// </summary>
    private void InitializeAudio()
    {
        try
        {
            // Create a device enumerator
            var deviceEnumerator = new MMDeviceEnumerator();

            // Get the default audio endpoint
            _device = deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);

            if (_device != null)
            {
                _initialized = true;
                _logger.LogInformation("Audio endpoint volume interface initialized successfully");
            }
            else
            {
                _logger.LogError("Failed to get default audio endpoint");
                _initialized = false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to initialize audio endpoint volume interface: {ex.Message}");
            _initialized = false;
        }
    }

    /// <summary>
    /// Handles an absolute value from a MIDI control
    /// </summary>
    /// <param name="value">The value (0-127)</param>
    public void HandleValue(int value)
    {
        // Convert 0-127 to 0.0-1.0
        float volumeLevel = value / 127.0f;

        if (!_initialized || _device == null)
        {
            _logger.LogWarning($"Cannot set volume to {volumeLevel:P0}: Audio endpoint not initialized");
            return;
        }

        try
        {
            // Set the master volume level
            _device.AudioEndpointVolume.MasterVolumeLevelScalar = volumeLevel;
            _logger.LogInformation($"Set system volume to {volumeLevel:P0}");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to set volume: {ex.Message}");
        }
    }

    /// <summary>
    /// Disposes the audio endpoint volume interface
    /// </summary>
    public void Dispose()
    {
        _device?.Dispose();
        _device = null;

        GC.SuppressFinalize(this);
    }
}
