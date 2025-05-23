using System;
using System.IO;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// Represents information about the currently active profile
    /// </summary>
    public class ActiveProfileInfo
    {
        /// <summary>
        /// Gets or sets the full path to the profile file
        /// </summary>
        public string ProfilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the profile
        /// </summary>
        public string ProfileName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the activation timestamp
        /// </summary>
        public DateTime ActivatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Creates a new instance of the ActiveProfileInfo class
        /// </summary>
        public ActiveProfileInfo()
        {
        }

        /// <summary>
        /// Creates a new instance of the ActiveProfileInfo class with the specified profile path
        /// </summary>
        /// <param name="profilePath">The full path to the profile file</param>
        public ActiveProfileInfo(string profilePath)
        {
            ProfilePath = profilePath;
            ProfileName = Path.GetFileNameWithoutExtension(profilePath);
            ActivatedAt = DateTime.Now;
        }
    }
}

