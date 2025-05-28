using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.GUI.Models;

namespace MIDIFlux.GUI.Helpers;

/// <summary>
/// Helper class for profile management functionality.
/// Replaces the profile-related methods from the old ConfigurationHelper.
/// </summary>
public static class ProfileHelper
{
    /// <summary>
    /// Gets the profiles directory
    /// </summary>
    /// <returns>The path to the profiles directory</returns>
    public static string GetProfilesDirectory()
    {
        return AppDataHelper.GetProfilesDirectory();
    }

    /// <summary>
    /// Ensures all application data directories exist
    /// </summary>
    public static void EnsureDirectoriesExist()
    {
        AppDataHelper.EnsureDirectoriesExist();
    }

    /// <summary>
    /// Creates an active profile info object
    /// </summary>
    /// <param name="profilePath">The path to the active profile</param>
    /// <returns>The active profile info object</returns>
    public static ActiveProfileInfo CreateActiveProfileInfo(string profilePath)
    {
        return new ActiveProfileInfo(profilePath);
    }

    /// <summary>
    /// Gets the active profile from a configuration path
    /// </summary>
    /// <param name="configPath">The path to the configuration file</param>
    /// <returns>The active profile info</returns>
    public static ActiveProfileInfo GetActiveProfile(string configPath)
    {
        return new ActiveProfileInfo(configPath);
    }

    /// <summary>
    /// Updates the last used profile path in app settings
    /// Note: This functionality is now handled by the ConfigurationManager's current.json system
    /// </summary>
    /// <param name="profilePath">The path to the profile</param>
    /// <returns>True if successful</returns>
    public static bool UpdateLastProfilePath(string? profilePath)
    {
        // This is now handled by ConfigurationManager, so we just return true
        // The actual last profile tracking is done via current.json
        return true;
    }

    /// <summary>
    /// Gets the last used profile path from app settings
    /// Note: This functionality is now handled by the ConfigurationManager's current.json system
    /// </summary>
    /// <returns>The last used profile path, or null if not set</returns>
    public static string? GetLastProfilePath()
    {
        // This is now handled by ConfigurationManager, so we return null
        // The actual last profile tracking is done via current.json
        return null;
    }

    /// <summary>
    /// Creates a default configuration file if it doesn't exist
    /// </summary>
    /// <param name="configurationService">The configuration service to use</param>
    /// <returns>True if successful, false otherwise</returns>
    public static bool EnsureDefaultConfigExists(ConfigurationService configurationService)
    {
        string profilesDir = GetProfilesDirectory();
        string defaultConfigPath = Path.Combine(profilesDir, "default.json");

        try
        {
            if (File.Exists(defaultConfigPath))
            {
                return true;
            }

            // Create a minimal default configuration
            var defaultConfig = new MappingConfig
            {
                ProfileName = "Default Profile",
                MidiDevices = new List<DeviceConfig>()
            };

            return configurationService.SaveProfileConfiguration(defaultConfig, defaultConfigPath);
        }
        catch
        {
            return false;
        }
    }
}
