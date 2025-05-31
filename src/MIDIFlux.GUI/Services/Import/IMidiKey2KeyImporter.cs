using System.Collections.Generic;
using System.Threading.Tasks;
using MIDIFlux.GUI.Services.Import.Models;

namespace MIDIFlux.GUI.Services.Import
{
    /// <summary>
    /// Interface for importing MIDIKey2Key configuration files
    /// </summary>
    public interface IMidiKey2KeyImporter
    {
        /// <summary>
        /// Imports a MIDIKey2Key configuration file and converts it to MIDIFlux format
        /// </summary>
        /// <param name="iniFilePath">Path to the MIDIKey2Key INI file</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result with statistics and any errors/warnings</returns>
        Task<ImportResult> ImportConfigurationAsync(string iniFilePath, ImportOptions options);

        /// <summary>
        /// Validates that the specified INI file is a valid MIDIKey2Key configuration
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file to validate</param>
        /// <returns>True if the file is valid, false otherwise</returns>
        bool ValidateIniFile(string iniFilePath);

        /// <summary>
        /// Previews the import without actually creating the configuration file
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file to preview</param>
        /// <returns>Preview information about what would be imported</returns>
        ImportPreview PreviewImport(string iniFilePath);
    }

    /// <summary>
    /// Options for controlling the import process
    /// </summary>
    public class ImportOptions
    {
        /// <summary>
        /// Gets or sets whether to skip Train Simulator specific features
        /// </summary>
        public bool SkipTrainSimulatorFeatures { get; set; } = true;

        /// <summary>
        /// Gets or sets whether to convert SysEx patterns to use wildcards
        /// </summary>
        public bool ConvertSysExToWildcards { get; set; } = true;

        /// <summary>
        /// Gets or sets the output directory for the converted profile
        /// </summary>
        public string OutputDirectory { get; set; } = "";

        /// <summary>
        /// Gets or sets the name for the imported profile
        /// </summary>
        public string ProfileName { get; set; } = "";
    }

    /// <summary>
    /// Preview information for an import operation
    /// </summary>
    public class ImportPreview
    {
        /// <summary>
        /// Gets or sets whether the file is valid for import
        /// </summary>
        public bool IsValid { get; set; }

        /// <summary>
        /// Gets or sets the total number of actions found
        /// </summary>
        public int TotalActions { get; set; }

        /// <summary>
        /// Gets or sets the number of actions that can be converted
        /// </summary>
        public int ConvertibleActions { get; set; }

        /// <summary>
        /// Gets or sets the number of actions that will be skipped
        /// </summary>
        public int SkippedActions { get; set; }

        /// <summary>
        /// Gets or sets the list of action types found
        /// </summary>
        public List<string> ActionTypes { get; set; } = new();

        /// <summary>
        /// Gets or sets any validation errors
        /// </summary>
        public List<string> ValidationErrors { get; set; } = new();

        /// <summary>
        /// Gets or sets any warnings about the import
        /// </summary>
        public List<string> Warnings { get; set; } = new();
    }
}
