using System;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Models;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for managing configuration files and settings
    /// </summary>
    public static class ConfigurationHelper
    {
        private static ConfigurationFileManager? _fileManager;

        /// <summary>
        /// Gets or creates the configuration file manager instance
        /// </summary>
        private static ConfigurationFileManager GetFileManager()
        {
            if (_fileManager == null)
            {
                var logger = LoggingHelper.CreateLogger("MIDIFlux.GUI.ConfigurationHelper");
                _fileManager = new ConfigurationFileManager(logger);
            }
            return _fileManager;
        }

        /// <summary>
        /// Gets the root application data directory
        /// </summary>
        /// <returns>The path to the application data directory</returns>
        public static string GetAppDataDirectory()
        {
            return AppDataHelper.GetAppDataDirectory();
        }

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
        /// Opens the profiles directory in Windows Explorer
        /// </summary>
        public static void OpenProfilesDirectory()
        {
            string profilesDir = AppDataHelper.GetProfilesDirectory();
            AppDataHelper.EnsureDirectoriesExist();
            Process.Start("explorer.exe", profilesDir);
        }

        /// <summary>
        /// Loads the application settings
        /// </summary>
        /// <returns>The application settings</returns>
        public static ApplicationSettings LoadSettings()
        {
            string settingsPath = AppDataHelper.GetGuiSettingsPath();
            var fileManager = GetFileManager();

            // Try to load existing settings
            var settings = fileManager.ReadJsonFile<ApplicationSettings>(settingsPath, "application settings");

            if (settings == null)
            {
                // If the settings file doesn't exist or failed to load, create default settings
                var defaultSettings = new ApplicationSettings();
                SaveSettings(defaultSettings);
                return defaultSettings;
            }

            return settings;
        }

        /// <summary>
        /// Saves the application settings
        /// </summary>
        /// <param name="settings">The settings to save</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool SaveSettings(ApplicationSettings settings)
        {
            string settingsPath = AppDataHelper.GetGuiSettingsPath();
            var fileManager = GetFileManager();

            return fileManager.WriteJsonFile(settings, settingsPath, "application settings");
        }

        /// <summary>
        /// Gets all profile files in the profiles directory
        /// </summary>
        /// <returns>An array of profile file paths</returns>
        public static string[] GetProfileFiles()
        {
            string profilesDir = AppDataHelper.GetProfilesDirectory();
            var fileManager = GetFileManager();

            return fileManager.GetFiles(profilesDir, "*.json", SearchOption.AllDirectories, true);
        }

        /// <summary>
        /// Creates a default configuration file if it doesn't exist
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public static bool EnsureDefaultConfigExists()
        {
            string profilesDir = AppDataHelper.GetProfilesDirectory();
            string defaultConfigPath = Path.Combine(profilesDir, "default.json");
            var fileManager = GetFileManager();

            return fileManager.EnsureDefaultConfigurationExists(defaultConfigPath);
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
        /// Gets the active profile model
        /// </summary>
        /// <param name="profilePath">The path to the active profile</param>
        /// <returns>The active profile model, or null if not found</returns>
        public static ProfileModel? GetActiveProfile(string profilePath)
        {
            var fileManager = GetFileManager();

            if (!fileManager.ValidateFileExists(profilePath, "profile file"))
            {
                return null;
            }

            var profile = new ProfileModel(profilePath, AppDataHelper.GetProfilesDirectory());
            profile.IsActive = true;
            return profile;
        }
    }
}

