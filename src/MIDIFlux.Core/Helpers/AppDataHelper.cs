using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace MIDIFlux.Core.Helpers
{
    /// <summary>
    /// Helper class for managing application data paths
    /// </summary>
    public static class AppDataHelper
    {
        /// <summary>
        /// Gets the root application data directory
        /// </summary>
        /// <returns>The path to the application data directory</returns>
        public static string GetAppDataDirectory()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIDIFlux");
        }

        /// <summary>
        /// Gets the profiles directory
        /// </summary>
        /// <returns>The path to the profiles directory</returns>
        public static string GetProfilesDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), "profiles");
        }

        /// <summary>
        /// Gets the logs directory
        /// </summary>
        /// <returns>The path to the logs directory</returns>
        public static string GetLogsDirectory()
        {
            return Path.Combine(GetAppDataDirectory(), "logs");
        }

        /// <summary>
        /// Gets the path to the application settings file
        /// </summary>
        /// <returns>The path to the application settings file</returns>
        public static string GetAppSettingsPath()
        {
            return Path.Combine(GetAppDataDirectory(), "appsettings.json");
        }

        /// <summary>
        /// Gets the path to the current configuration file
        /// </summary>
        /// <returns>The path to the current configuration file</returns>
        public static string GetCurrentConfigFilePath()
        {
            return Path.Combine(GetAppDataDirectory(), "current.json");
        }

        /// <summary>
        /// Gets the path to the GUI settings file
        /// </summary>
        /// <returns>The path to the GUI settings file</returns>
        public static string GetGuiSettingsPath()
        {
            return Path.Combine(GetAppDataDirectory(), "settings.json");
        }

        /// <summary>
        /// Ensures all application data directories exist
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void EnsureDirectoriesExist(ILogger logger)
        {
            // Ensure AppData directories exist
            string appDataDir = GetAppDataDirectory();
            string profilesDir = GetProfilesDirectory();
            string logsDir = GetLogsDirectory();

            // Check if this is a first-time setup (AppData directory doesn't exist)
            bool isFirstTimeSetup = !Directory.Exists(appDataDir);

            // Create the app data directory if it doesn't exist
            if (isFirstTimeSetup)
            {
                try
                {
                    Directory.CreateDirectory(appDataDir);
                    logger.LogInformation("Created application data directory: {AppDataDir}", appDataDir);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create application data directory: {ErrorMessage}", ex.Message);
                }
            }

            // Create the profiles directory if it doesn't exist
            if (!Directory.Exists(profilesDir))
            {
                try
                {
                    Directory.CreateDirectory(profilesDir);
                    logger.LogInformation("Created profiles directory: {ProfilesDir}", profilesDir);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create profiles directory: {ErrorMessage}", ex.Message);
                }
            }

            // Create the logs directory if it doesn't exist
            if (!Directory.Exists(logsDir))
            {
                try
                {
                    Directory.CreateDirectory(logsDir);
                    logger.LogInformation("Created logs directory: {LogsDir}", logsDir);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create logs directory: {ErrorMessage}", ex.Message);
                }
            }


        }

        /// <summary>
        /// Ensures all application data directories exist (without logging)
        /// </summary>
        public static void EnsureDirectoriesExist()
        {
            // Ensure AppData directories exist
            string appDataDir = GetAppDataDirectory();
            string profilesDir = GetProfilesDirectory();
            string logsDir = GetLogsDirectory();

            // Check if this is a first-time setup (AppData directory doesn't exist)
            bool isFirstTimeSetup = !Directory.Exists(appDataDir);

            // Create the app data directory if it doesn't exist
            if (isFirstTimeSetup)
            {
                Directory.CreateDirectory(appDataDir);
            }

            // Create the profiles directory if it doesn't exist
            if (!Directory.Exists(profilesDir))
            {
                Directory.CreateDirectory(profilesDir);
            }

            // Create the logs directory if it doesn't exist
            if (!Directory.Exists(logsDir))
            {
                Directory.CreateDirectory(logsDir);
            }


        }

        /// <summary>
        /// Opens the profiles directory in Windows Explorer
        /// </summary>
        public static void OpenProfilesDirectory()
        {
            string profilesDir = GetProfilesDirectory();
            EnsureDirectoriesExist();
            Process.Start("explorer.exe", profilesDir);
        }

        /// <summary>
        /// Copies a file from the executable directory to the AppData directory if it doesn't exist
        /// </summary>
        /// <param name="fileName">The name of the file to copy</param>
        /// <param name="logger">The logger to use</param>
        public static void CopyFileFromExecutableToAppData(string fileName, ILogger logger)
        {
            string executableDir = Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppDomain.CurrentDomain.BaseDirectory;
            string sourceFilePath = Path.Combine(executableDir, fileName);
            string destFilePath = Path.Combine(GetAppDataDirectory(), fileName);

            if (File.Exists(sourceFilePath) && !File.Exists(destFilePath))
            {
                try
                {
                    File.Copy(sourceFilePath, destFilePath);
                    logger.LogInformation("Copied {FileName} from executable directory to AppData directory", fileName);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to copy {FileName} to AppData directory: {ErrorMessage}", fileName, ex.Message);
                }
            }
        }

        /// <summary>
        /// Ensures that the appsettings.json file exists in the AppData directory
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void EnsureAppSettingsExist(ILogger logger)
        {
            string appSettingsPath = GetAppSettingsPath();

            if (!File.Exists(appSettingsPath))
            {
                try
                {
                    // Create a default appsettings.json file with logging turned off
                    string defaultAppSettings = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""None"",
      ""Microsoft"": ""None"",
      ""System"": ""None""
    }
  },
  ""Application"": {
    ""AutoStartLastConfig"": true,
    ""ScanIntervalSeconds"": 5
  },
  ""MIDI"": {
    ""AutoReconnect"": true
  }
}";
                    File.WriteAllText(appSettingsPath, defaultAppSettings);
                    logger.LogInformation("Created default appsettings.json in AppData directory");
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to create appsettings.json in AppData directory: {ErrorMessage}", ex.Message);
                }
            }
        }
    }
}
