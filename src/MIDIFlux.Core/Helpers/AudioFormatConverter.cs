using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Helpers;

/// <summary>
/// Helper class for audio format conversion and sample provider creation.
/// Handles conversion to target format (44.1kHz, 2-channel, 32-bit float) for zero-latency playback.
/// </summary>
public class AudioFormatConverter
{
    private readonly ILogger<AudioFormatConverter> _logger;

    /// <summary>
    /// Target audio format for all playback (44.1kHz, 2-channel, 32-bit float)
    /// </summary>
    public static readonly WaveFormat TargetFormat = WaveFormat.CreateIeeeFloatWaveFormat(44100, 2);

    /// <summary>
    /// Initializes a new instance of AudioFormatConverter
    /// </summary>
    /// <param name="logger">Logger for error reporting</param>
    public AudioFormatConverter(ILogger<AudioFormatConverter> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Converts audio data to the target format (44.1kHz, 2-channel, 32-bit float).
    /// Performs all necessary sample rate conversion and channel conversion.
    /// </summary>
    /// <param name="originalData">Original audio data as byte array</param>
    /// <param name="originalFormat">Original audio format</param>
    /// <returns>Converted audio data as float array, or null if conversion failed</returns>
    public float[]? ConvertToTargetFormat(byte[] originalData, WaveFormat originalFormat)
    {
        try
        {
            _logger.LogDebug("Converting audio from {OriginalFormat} to target format", originalFormat);

            // Create a wave provider from the original data
            var rawProvider = new RawSourceWaveStream(new MemoryStream(originalData), originalFormat);
            
            // Convert to sample provider
            ISampleProvider sampleProvider = rawProvider.ToSampleProvider();

            // Handle sample rate conversion if needed
            if (originalFormat.SampleRate != TargetFormat.SampleRate)
            {
                _logger.LogDebug("Converting sample rate from {OriginalRate} to {TargetRate}", 
                    originalFormat.SampleRate, TargetFormat.SampleRate);
                
                // Use WdlResamplingSampleProvider for high-quality resampling
                sampleProvider = new WdlResamplingSampleProvider(sampleProvider, TargetFormat.SampleRate);
            }

            // Handle channel conversion if needed
            if (sampleProvider.WaveFormat.Channels != TargetFormat.Channels)
            {
                _logger.LogDebug("Converting channels from {OriginalChannels} to {TargetChannels}", 
                    sampleProvider.WaveFormat.Channels, TargetFormat.Channels);

                if (sampleProvider.WaveFormat.Channels == 1 && TargetFormat.Channels == 2)
                {
                    // Mono to stereo
                    sampleProvider = new MonoToStereoSampleProvider(sampleProvider);
                }
                else if (sampleProvider.WaveFormat.Channels > 2 && TargetFormat.Channels == 2)
                {
                    // Multi-channel to stereo (mix down)
                    sampleProvider = new MultiplexingSampleProvider(new[] { sampleProvider }, 2);
                }
                else
                {
                    _logger.LogWarning("Unsupported channel conversion from {OriginalChannels} to {TargetChannels}", 
                        sampleProvider.WaveFormat.Channels, TargetFormat.Channels);
                }
            }

            // Read all samples into memory
            var samples = new List<float>();
            var buffer = new float[4096];
            int samplesRead;

            while ((samplesRead = sampleProvider.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < samplesRead; i++)
                {
                    samples.Add(buffer[i]);
                }
            }

            rawProvider.Dispose();

            _logger.LogDebug("Successfully converted audio: {SampleCount} samples", samples.Count);
            return samples.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to convert audio format: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Creates a sample provider from pre-converted audio data.
    /// </summary>
    /// <param name="audioData">Pre-converted 32-bit float audio data</param>
    /// <param name="format">Audio format (should match TargetFormat)</param>
    /// <returns>Sample provider for playback</returns>
    public ISampleProvider CreateSampleProvider(float[] audioData, WaveFormat format)
    {
        return new ArraySampleProvider(audioData, format);
    }

    /// <summary>
    /// Applies volume control to a sample provider.
    /// </summary>
    /// <param name="source">Source sample provider</param>
    /// <param name="volume">Volume level (0-100)</param>
    /// <returns>Volume-controlled sample provider</returns>
    public ISampleProvider ApplyVolumeControl(ISampleProvider source, int volume)
    {
        // Convert 0-100 scale to 0.0-1.0 scale
        float volumeLevel = Math.Max(0, Math.Min(100, volume)) / 100.0f;
        
        return new VolumeSampleProvider(source)
        {
            Volume = volumeLevel
        };
    }
}

/// <summary>
/// Sample provider that reads from a pre-loaded float array.
/// Used for zero-latency playback of cached audio data.
/// </summary>
internal class ArraySampleProvider : ISampleProvider
{
    private readonly float[] _audioData;
    private int _position;

    public WaveFormat WaveFormat { get; }

    public ArraySampleProvider(float[] audioData, WaveFormat waveFormat)
    {
        _audioData = audioData;
        WaveFormat = waveFormat;
        _position = 0;
    }

    public int Read(float[] buffer, int offset, int count)
    {
        int samplesAvailable = _audioData.Length - _position;
        int samplesToCopy = Math.Min(count, samplesAvailable);

        if (samplesToCopy > 0)
        {
            Array.Copy(_audioData, _position, buffer, offset, samplesToCopy);
            _position += samplesToCopy;
        }

        return samplesToCopy;
    }
}
