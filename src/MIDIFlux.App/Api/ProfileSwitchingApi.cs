using Microsoft.Extensions.Logging;
using MIDIFlux.App.Services;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.App.Api;

/// <summary>
/// API for profile switching and configuration persistence.
/// Provides simple wrapper methods around core profile management functionality.
/// </summary>
public class ProfileSwitchingApi
{
    private readonly ILogger<ProfileSwitchingApi> _logger;
    private readonly MidiProcessingService _midiProcessingService;
    private readonly ConfigurationService _configurationService;

    /// <summary>
    /// Initializes a new instance of the ProfileSwitchingApi
    /// </summary>
    /// <param name="logger">Logger for this API</param>
    /// <param name="midiProcessingService">MIDI processing service for profile operations</param>
    /// <param name="configurationService">Configuration service for file operations</param>
    public ProfileSwitchingApi(
        ILogger<ProfileSwitchingApi> logger, 
        MidiProcessingService midiProcessingService,
        ConfigurationService configurationService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _midiProcessingService = midiProcessingService ?? throw new ArgumentNullException(nameof(midiProcessingService));
        _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
    }

    /// <summary>
    /// Switches to the specified profile by loading its configuration.
    /// Wrapper around MidiProcessingService.LoadConfiguration().
    /// </summary>
    /// <param name="profilePath">Relative path to profile file (e.g., "examples/keyboard.json")</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SwitchToProfile(string profilePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(profilePath))
            {
                _logger.LogWarning("Profile path is null or empty");
                return false;
            }

            var profilesDirectory = AppDataHelper.GetProfilesDirectory();
            var fullPath = Path.Combine(profilesDirectory, profilePath);

            // Security validation: ensure path is within profiles directory
            var normalizedProfilesDir = Path.GetFullPath(profilesDirectory);
            var normalizedFullPath = Path.GetFullPath(fullPath);

            if (!normalizedFullPath.StartsWith(normalizedProfilesDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Profile path {ProfilePath} is outside allowed profiles directory", profilePath);
                return false;
            }

            if (!File.Exists(fullPath))
            {
                _logger.LogWarning("Profile file does not exist: {FullPath}", fullPath);
                return false;
            }

            _logger.LogInformation("Switching to profile: {ProfilePath}", profilePath);

            // Stop current MIDI processing if running (to re-open devices for new profile)
            if (_midiProcessingService.IsRunning)
            {
                _logger.LogInformation("Stopping current MIDI processing before profile switch");
                _midiProcessingService.Stop();
            }

            var success = _midiProcessingService.LoadConfiguration(fullPath);

            if (success)
            {
                // Start MIDI processing to open devices and activate the profile
                if (_midiProcessingService.Start())
                {
                    _logger.LogInformation("Successfully switched to profile and started MIDI processing: {ProfilePath}", profilePath);
                }
                else
                {
                    _logger.LogWarning("Profile loaded but MIDI processing failed to start: {ProfilePath}", profilePath);
                    // Still return true - config was loaded successfully, Start() failure is non-fatal
                    // (e.g., no MIDI devices connected)
                }
            }
            else
            {
                _logger.LogWarning("Failed to switch to profile: {ProfilePath}", profilePath);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error switching to profile {ProfilePath}: {ErrorMessage}", profilePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets current configuration and saves to specified file with metadata.
    /// Overwrites existing files. Validates that save paths are within allowed directories.
    /// </summary>
    /// <param name="filePath">Relative path for the new profile file (e.g., "my-config.json")</param>
    /// <param name="profileName">Name for the profile</param>
    /// <param name="description">Optional description for the profile</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SaveCurrentConfiguration(string filePath, string profileName, string? description = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                _logger.LogWarning("File path is null or empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(profileName))
            {
                _logger.LogWarning("Profile name is null or empty");
                return false;
            }

            var profilesDirectory = AppDataHelper.GetProfilesDirectory();
            var fullPath = Path.Combine(profilesDirectory, filePath);

            // Security validation: ensure path is within profiles directory
            var normalizedProfilesDir = Path.GetFullPath(profilesDirectory);
            var normalizedFullPath = Path.GetFullPath(fullPath);

            if (!normalizedFullPath.StartsWith(normalizedProfilesDir, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Save path {FilePath} is outside allowed profiles directory", filePath);
                return false;
            }

            // Ensure the directory exists
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                _logger.LogDebug("Created directory: {Directory}", directory);
            }

            // Get current configuration
            var currentConfig = _midiProcessingService.GetCurrentRuntimeConfiguration();
            if (currentConfig == null)
            {
                _logger.LogWarning("No current configuration available to save");
                return false;
            }

            // Update metadata
            currentConfig.ProfileName = profileName;
            currentConfig.Description = description ?? string.Empty;

            _logger.LogInformation("Saving current configuration to: {FilePath} with profile name: {ProfileName}", 
                filePath, profileName);

            var success = _configurationService.SaveProfileConfiguration(currentConfig, fullPath);
            
            if (success)
            {
                _logger.LogInformation("Successfully saved configuration to: {FilePath}", filePath);
            }
            else
            {
                _logger.LogWarning("Failed to save configuration to: {FilePath}", filePath);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving current configuration to {FilePath}: {ErrorMessage}", 
                filePath, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets information about currently loaded profile (if any).
    /// Returns comprehensive information about the active profile.
    /// </summary>
    /// <returns>Object with profile name, path, load time, etc., or null if no profile loaded</returns>
    public object? GetActiveProfileInfo()
    {
        try
        {
            var currentConfig = _midiProcessingService.GetCurrentRuntimeConfiguration();
            if (currentConfig == null)
            {
                _logger.LogDebug("No active profile - no configuration loaded");
                return null;
            }

            // Get the last used configuration path
            var activeConfigPath = _midiProcessingService.LoadLastUsedConfigurationPath();
            
            var profileInfo = new
            {
                ProfileName = currentConfig.ProfileName ?? "Unnamed Profile",
                Description = currentConfig.Description ?? string.Empty,
                ConfigurationPath = activeConfigPath ?? "Unknown",
                DeviceCount = currentConfig.MidiDevices?.Count ?? 0,
                MappingCount = currentConfig.MidiDevices?.Sum(d => d.Mappings?.Count ?? 0) ?? 0,
                LoadTime = DateTime.Now, // Note: Actual load time tracking would require additional state management
                IsValid = currentConfig.IsValid()
            };

            _logger.LogDebug("Retrieved active profile info: {ProfileName} with {DeviceCount} devices", 
                profileInfo.ProfileName, profileInfo.DeviceCount);

            return profileInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active profile info: {ErrorMessage}", ex.Message);
            return null;
        }
    }
}
