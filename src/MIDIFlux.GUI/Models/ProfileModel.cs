using System;
using System.IO;
using MIDIFlux.Core.Models;

namespace MIDIFlux.GUI.Models
{
    /// <summary>
    /// Represents a profile in the MIDIFlux GUI
    /// </summary>
    public class ProfileModel
    {
        /// <summary>
        /// Gets or sets the name of the profile
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the full path to the profile file
        /// </summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the relative path to the profile file (relative to the profiles directory)
        /// </summary>
        public string RelativePath { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether this profile is active
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets the directory containing the profile file
        /// </summary>
        public string Directory => Path.GetDirectoryName(FilePath) ?? string.Empty;

        /// <summary>
        /// Gets the file name of the profile
        /// </summary>
        public string FileName => Path.GetFileName(FilePath);

        /// <summary>
        /// Gets the last modified date of the profile file
        /// </summary>
        public DateTime LastModified => File.Exists(FilePath) ? File.GetLastWriteTime(FilePath) : DateTime.MinValue;

        /// <summary>
        /// Gets the size of the profile file in bytes
        /// </summary>
        public long FileSize => File.Exists(FilePath) ? new FileInfo(FilePath).Length : 0;

        /// <summary>
        /// Gets a value indicating whether the profile file exists
        /// </summary>
        public bool Exists => File.Exists(FilePath);

        /// <summary>
        /// Creates a new instance of the ProfileModel class
        /// </summary>
        public ProfileModel()
        {
        }

        /// <summary>
        /// Creates a new instance of the ProfileModel class with the specified file path
        /// </summary>
        /// <param name="filePath">The full path to the profile file</param>
        /// <param name="profilesDirectory">The profiles directory to calculate the relative path</param>
        public ProfileModel(string filePath, string profilesDirectory)
        {
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
            
            // Calculate the relative path
            if (filePath.StartsWith(profilesDirectory))
            {
                RelativePath = filePath.Substring(profilesDirectory.Length).TrimStart(Path.DirectorySeparatorChar);
            }
            else
            {
                RelativePath = Path.GetFileName(filePath);
            }
        }

        /// <summary>
        /// Returns a string representation of the profile
        /// </summary>
        public override string ToString()
        {
            return Name + (IsActive ? " (Active)" : string.Empty);
        }
    }
}

