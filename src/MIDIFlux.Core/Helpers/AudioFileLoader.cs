using NAudio.Wave;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Helpers;

/// <summary>
/// Helper class for loading and converting audio files to the target format.
/// Handles file I/O and format validation for PlaySoundAction.
/// </summary>
public class AudioFileLoader
{
    private readonly ILogger<AudioFileLoader> _logger;
    private readonly AudioFormatConverter _formatConverter;

    /// <summary>
    /// Initializes a new instance of AudioFileLoader
    /// </summary>
    /// <param name="logger">Logger for error reporting</param>
    /// <param name="formatConverter">Format converter for audio processing</param>
    public AudioFileLoader(ILogger<AudioFileLoader> logger, AudioFormatConverter formatConverter)
    {
        _logger = logger;
        _formatConverter = formatConverter;
    }

    /// <summary>
    /// Loads an audio file and converts it to the target format for zero-latency playback.
    /// </summary>
    /// <param name="filePath">Full path to the audio file</param>
    /// <returns>Tuple containing converted audio data and format, or null if loading failed</returns>
    public (float[] audioData, WaveFormat format)? LoadAndConvertAudioFile(string filePath)
    {
        try
        {
            _logger.LogDebug("Loading audio file: {FilePath}", filePath);

            // Validate file exists
            if (!File.Exists(filePath))
            {
                _logger.LogError("Audio file not found: {FilePath}", filePath);
                return null;
            }

            // Load the audio file using NAudio's AudioFileReader
            using var audioFileReader = new AudioFileReader(filePath);
            
            _logger.LogDebug("Audio file info: {SampleRate}Hz, {Channels} channels, {Duration}s", 
                audioFileReader.WaveFormat.SampleRate, 
                audioFileReader.WaveFormat.Channels,
                audioFileReader.TotalTime.TotalSeconds);

            // Read all audio data into memory
            var audioData = new List<byte>();
            var buffer = new byte[4096];
            int bytesRead;

            while ((bytesRead = audioFileReader.Read(buffer, 0, buffer.Length)) > 0)
            {
                for (int i = 0; i < bytesRead; i++)
                {
                    audioData.Add(buffer[i]);
                }
            }

            var originalData = audioData.ToArray();
            var originalFormat = audioFileReader.WaveFormat;

            _logger.LogDebug("Read {ByteCount} bytes from audio file", originalData.Length);

            // Convert to target format
            var convertedData = _formatConverter.ConvertToTargetFormat(originalData, originalFormat);
            
            if (convertedData == null)
            {
                _logger.LogError("Failed to convert audio file to target format: {FilePath}", filePath);
                return null;
            }

            _logger.LogDebug("Successfully loaded and converted audio file: {FilePath}", filePath);
            return (convertedData, AudioFormatConverter.TargetFormat);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load audio file '{FilePath}': {ErrorMessage}", filePath, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Checks if the specified audio file format is supported.
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <returns>True if the format is supported, false otherwise</returns>
    public bool IsFormatSupported(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            // Try to open the file with AudioFileReader
            using var audioFileReader = new AudioFileReader(filePath);
            
            // If we can open it and read the format, it's supported
            var format = audioFileReader.WaveFormat;
            _logger.LogDebug("Audio format check for {FilePath}: {SampleRate}Hz, {Channels} channels", 
                filePath, format.SampleRate, format.Channels);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug("Audio format not supported for {FilePath}: {ErrorMessage}", filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Resolves a file path from either absolute path or relative to sounds directory.
    /// </summary>
    /// <param name="inputPath">Input path (absolute or relative)</param>
    /// <returns>Resolved absolute file path</returns>
    public static string ResolveFilePath(string inputPath)
    {
        // If absolute path, use as-is
        if (Path.IsPathRooted(inputPath))
        {
            return inputPath;
        }

        // Otherwise, resolve relative to sounds directory
        string soundsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "MIDIFlux", 
            "sounds");
        
        return Path.Combine(soundsDir, inputPath);
    }

    /// <summary>
    /// Ensures the sounds directory exists and creates it if necessary.
    /// </summary>
    public static void EnsureSoundsDirectoryExists()
    {
        string soundsDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "MIDIFlux", 
            "sounds");

        if (!Directory.Exists(soundsDir))
        {
            Directory.CreateDirectory(soundsDir);
        }
    }

    /// <summary>
    /// Gets the sounds directory path.
    /// </summary>
    /// <returns>Full path to the sounds directory</returns>
    public static string GetSoundsDirectory()
    {
        return Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), 
            "MIDIFlux", 
            "sounds");
    }
}
