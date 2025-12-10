using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Services;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for playing audio files with low-latency playback.
/// Supports WAV, AIFF, and MP3 formats with pre-loading for zero-latency execution.
/// Consolidates audio configuration and runtime data into the action class.
/// </summary>
[ActionDisplayName("Play Sound")]
[ActionCategory(ActionCategory.Utility)]
public class PlaySoundAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string FilePathParam = "FilePath";
    private const string VolumeParam = "Volume";
    private const string AudioDeviceParam = "AudioDevice";

    // Runtime-only data (NOT serialized)
    [JsonIgnore]
    private float[]? _preloadedAudioData;

    [JsonIgnore]
    private WaveFormat? _audioFormat;

    [JsonIgnore]
    private IAudioPlaybackService? _audioService;

    [JsonIgnore]
    private AudioFileLoader? _audioLoader;

    /// <summary>
    /// Initializes a new instance of PlaySoundAction with default parameters
    /// </summary>
    public PlaySoundAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of PlaySoundAction with specified file path
    /// </summary>
    /// <param name="filePath">Path to the audio file</param>
    /// <param name="volume">Playback volume (0-100)</param>
    public PlaySoundAction(string filePath, int volume = 100) : base()
    {
        SetParameterValue(FilePathParam, filePath);
        SetParameterValue(VolumeParam, volume);
        UpdateDescription();
    }

    /// <summary>
    /// Sets a parameter value and updates the description
    /// </summary>
    public new void SetParameterValue<T>(string parameterName, T value)
    {
        base.SetParameterValue(parameterName, value);
        UpdateDescription();
    }

    /// <summary>
    /// Updates the description based on current parameter values
    /// </summary>
    private void UpdateDescription()
    {
        Description = GetDefaultDescription();
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add FilePath parameter (required)
        Parameters[FilePathParam] = new Parameter(
            ParameterType.String,
            null, // No default - user must specify file path
            "Audio File Path")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "placeholder", "e.g., button-click.wav or C:\\Sounds\\effect.mp3" },
                { "description", "Path to audio file (relative to %AppData%\\MIDIFlux\\sounds or absolute)" },
                { "supportsFileSelection", "sound" }
            }
        };

        // Add Volume parameter (optional, default 100)
        Parameters[VolumeParam] = new Parameter(
            ParameterType.Integer,
            100, // Default volume
            "Volume (0-100)")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 100 }
            }
        };

        // Add AudioDevice parameter (optional, null = default device)
        Parameters[AudioDeviceParam] = new Parameter(
            ParameterType.String,
            null, // Default to system default device
            "Audio Device (Optional)")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "placeholder", "Leave empty for default device" },
                { "description", "Specific audio device name (optional)" }
            }
        };
    }

    /// <summary>
    /// Gets the compatible input categories for this action
    /// </summary>
    /// <returns>Array of compatible input categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue, InputTypeCategory.RelativeValue };
    }

    /// <summary>
    /// Validates the action configuration and parameters with dual-context support
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        // Clear previous validation errors and validate base parameters
        base.IsValid();

        // Get parameter values
        var filePath = GetParameterValue<string?>(FilePathParam);
        var volume = GetParameterValue<int>(VolumeParam);
        var audioDevice = GetParameterValue<string?>(AudioDeviceParam);

        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            AddValidationError("File path is required");
            return false;
        }

        // Validate volume range
        if (volume < 0 || volume > 100)
        {
            AddValidationError("Volume must be between 0 and 100");
        }

        // Detect if we're being called from GUI context by checking calling assembly
        bool isGuiContext = IsRunningInGuiContext();
        Logger.LogDebug("PlaySoundAction validation: IsGuiContext = {IsGuiContext}", isGuiContext);

        if (isGuiContext)
        {
            // GUI Context: Only validate file existence and format support
            Logger.LogDebug("Using GUI validation for PlaySoundAction");
            return ValidateForGuiContext(filePath, audioDevice);
        }
        else
        {
            // Runtime Context: Validate pre-loaded data and services
            Logger.LogDebug("Using Runtime validation for PlaySoundAction");
            return ValidateForRuntimeContext(audioDevice);
        }
    }

    /// <summary>
    /// Validates the action in GUI context (no pre-loading)
    /// </summary>
    private bool ValidateForGuiContext(string filePath, string? audioDevice)
    {
        try
        {
            // Resolve file path
            var resolvedPath = AudioFileLoader.ResolveFilePath(filePath);

            // Check if file exists
            if (!File.Exists(resolvedPath))
            {
                AddValidationError($"Audio file not found: {resolvedPath}");
                return false;
            }

            // Check if format is supported (create temporary loader for validation)
            var tempLogger = LoggingHelper.CreateLogger<AudioFileLoader>();
            var tempConverter = new AudioFormatConverter(LoggingHelper.CreateLogger<AudioFormatConverter>());
            var tempLoader = new AudioFileLoader(tempLogger, tempConverter);

            if (!tempLoader.IsFormatSupported(resolvedPath))
            {
                AddValidationError($"Unsupported audio format: {resolvedPath}");
                return false;
            }

            // Validate audio device if specified
            if (!string.IsNullOrWhiteSpace(audioDevice))
            {
                // In GUI context, we can't check device availability without initializing audio service
                // Just validate that it's not empty if specified
                if (audioDevice.Trim().Length == 0)
                {
                    AddValidationError("Audio device name cannot be empty");
                }
            }

            return GetValidationErrors().Count == 0;
        }
        catch (Exception ex)
        {
            AddValidationError($"Validation error: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Validates the action in runtime context (checks services and file availability)
    /// </summary>
    private bool ValidateForRuntimeContext(string? audioDevice)
    {
        // Get the file path for validation
        var filePath = GetParameterValue<string>(FilePathParam);
        var resolvedPath = AudioFileLoader.ResolveFilePath(filePath);

        // Check if file exists
        if (!File.Exists(resolvedPath))
        {
            AddValidationError($"Audio file not found: {resolvedPath}");
            return false;
        }

        // Check if audio service is available
        var audioService = GetService<IAudioPlaybackService>();
        if (audioService == null)
        {
            AddValidationError("Audio playback service not available");
            return false;
        }

        // Validate audio device availability if specified
        if (!string.IsNullOrWhiteSpace(audioDevice))
        {
            if (!audioService.IsDeviceAvailable(audioDevice))
            {
                AddValidationError($"Audio device not available: {audioDevice}");
            }
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for playing the sound
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        try
        {
            // Ensure audio data is loaded for runtime execution
            EnsureAudioDataLoaded();

            if (_preloadedAudioData == null || _audioFormat == null || _audioService == null)
            {
                var errorMsg = "Audio data or service not available for playback";
                Logger.LogError(errorMsg);
                return ValueTask.CompletedTask;
            }

            // Get parameters
            var volume = GetParameterValue<int>(VolumeParam);
            var audioDevice = GetParameterValue<string?>(AudioDeviceParam);

            // Play the sound (fire-and-forget)
            _audioService.PlaySound(_preloadedAudioData, _audioFormat, volume, audioDevice);

            Logger.LogTrace("Successfully triggered sound playback: Volume={Volume}, Device={Device}",
                volume, audioDevice ?? "default");

            return ValueTask.CompletedTask;
        }
        catch (Exception ex)
        {
            // Runtime errors are logged only (no MessageBox)
            var filePath = GetParameterValue<string?>(FilePathParam) ?? "unknown";
            Logger.LogError(ex, "Failed to play sound '{FilePath}': {ErrorMessage}", filePath, ex.Message);
            return ValueTask.CompletedTask;
        }
    }

    /// <summary>
    /// Ensures audio data is loaded for runtime execution (lazy loading)
    /// </summary>
    private void EnsureAudioDataLoaded()
    {
        // Don't load audio data if we're in GUI context or already loaded
        if (IsRunningInGuiContext() || _preloadedAudioData != null)
        {
            return; // Already loaded or in GUI mode
        }

        try
        {
            // Get services
            _audioService ??= GetRequiredService<IAudioPlaybackService>();

            if (_audioLoader == null)
            {
                var formatConverter = GetRequiredService<AudioFormatConverter>();
                var loaderLogger = LoggingHelper.CreateLogger<AudioFileLoader>();
                _audioLoader = new AudioFileLoader(loaderLogger, formatConverter);
            }

            // Load and convert audio file
            var filePath = GetParameterValue<string>(FilePathParam);
            var resolvedPath = AudioFileLoader.ResolveFilePath(filePath);

            var result = _audioLoader.LoadAndConvertAudioFile(resolvedPath);
            if (result.HasValue)
            {
                _preloadedAudioData = result.Value.audioData;
                _audioFormat = result.Value.format;

                Logger.LogDebug("Audio data loaded successfully: {SampleCount} samples", _preloadedAudioData.Length);
            }
            else
            {
                // Profile load time error - show MessageBox
                var errorMsg = $"Failed to load audio file '{resolvedPath}'";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Audio Load Error", Logger);
            }
        }
        catch (Exception ex)
        {
            // Profile load time error - show MessageBox
            var filePath = GetParameterValue<string?>(FilePathParam) ?? "unknown";
            var errorMsg = $"Failed to load audio file '{filePath}'";
            Logger.LogError(ex, errorMsg + ": {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowError($"{errorMsg}:\n\n{ex.Message}", "MIDIFlux - Audio Load Error", Logger, ex);
        }
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var filePath = GetParameterValue<string?>(FilePathParam);
            var volume = GetParameterValue<int>(VolumeParam);

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var fileName = Path.GetFileName(filePath);
                return $"Play '{fileName}' at {volume}% volume";
            }

            return "Play Sound";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Play Sound";
        }
    }
}
