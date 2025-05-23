using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// Represents the application settings for the MIDIFlux GUI
    /// </summary>
    public class ApplicationSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to load the last used profile on startup
        /// </summary>
        public bool LoadLastProfile { get; set; } = true;

        /// <summary>
        /// Gets or sets the path to the last used profile
        /// </summary>
        public string? LastProfilePath { get; set; }

        /// <summary>
        /// Gets or sets the application theme
        /// </summary>
        public string Theme { get; set; } = "Default";

        /// <summary>
        /// Gets or sets the application language
        /// </summary>
        public string Language { get; set; } = "English";

        /// <summary>
        /// Gets or sets the logging settings
        /// </summary>
        public LoggingSettings Logging { get; set; } = new LoggingSettings();

        /// <summary>
        /// Gets or sets the MIDI settings
        /// </summary>
        public MidiSettings Midi { get; set; } = new MidiSettings();

        /// <summary>
        /// Gets or sets the recently used profiles
        /// </summary>
        public List<string> RecentProfiles { get; set; } = new List<string>();
    }

    /// <summary>
    /// Represents the logging settings for the application
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// Gets or sets the log level
        /// </summary>
        public string LogLevel { get; set; } = "Information";

        /// <summary>
        /// Gets or sets the maximum log file size in megabytes
        /// </summary>
        public int MaxLogSizeMB { get; set; } = 50;

        /// <summary>
        /// Gets or sets the number of days to retain logs
        /// </summary>
        public int RetainLogDays { get; set; } = 14;
    }

    /// <summary>
    /// Represents the MIDI settings for the application
    /// </summary>
    public class MidiSettings
    {
        /// <summary>
        /// Gets or sets a value indicating whether to automatically reconnect to MIDI devices
        /// </summary>
        public bool AutoReconnect { get; set; } = true;

        /// <summary>
        /// Gets or sets the interval in seconds for scanning for MIDI devices
        /// </summary>
        public int ScanIntervalSeconds { get; set; } = 5;
    }
}

