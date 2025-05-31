using System;
using System.Collections.Generic;

namespace MIDIFlux.GUI.Services.Import.Models
{
    /// <summary>
    /// Result of a MIDIKey2Key import operation
    /// </summary>
    public class ImportResult
    {
        /// <summary>
        /// Gets or sets whether the import was successful
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Gets or sets the path to the output MIDIFlux configuration file
        /// </summary>
        public string OutputFilePath { get; set; } = "";

        /// <summary>
        /// Gets or sets the import statistics
        /// </summary>
        public ImportStatistics Statistics { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of warnings encountered during import
        /// </summary>
        public List<ImportWarning> Warnings { get; set; } = new();

        /// <summary>
        /// Gets or sets the list of errors encountered during import
        /// </summary>
        public List<ImportError> Errors { get; set; } = new();

        /// <summary>
        /// Gets or sets the exception that caused the import to fail (if any)
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Statistics about the import operation
    /// </summary>
    public class ImportStatistics
    {
        /// <summary>
        /// Gets or sets the total number of actions found in the source file
        /// </summary>
        public int TotalActionsFound { get; set; }

        /// <summary>
        /// Gets or sets the number of actions successfully converted
        /// </summary>
        public int ActionsConverted { get; set; }

        /// <summary>
        /// Gets or sets the number of actions skipped due to unsupported features
        /// </summary>
        public int ActionsSkipped { get; set; }

        /// <summary>
        /// Gets or sets the number of actions that failed to convert
        /// </summary>
        public int ActionsFailed { get; set; }

        /// <summary>
        /// Gets or sets the number of MIDI mappings created
        /// </summary>
        public int MidiMappingsCreated { get; set; }

        /// <summary>
        /// Gets or sets the number of keyboard actions created
        /// </summary>
        public int KeyboardActionsCreated { get; set; }

        /// <summary>
        /// Gets or sets the number of SysEx patterns created
        /// </summary>
        public int SysExPatternsCreated { get; set; }

        /// <summary>
        /// Gets or sets the number of command executions created
        /// </summary>
        public int CommandExecutionsCreated { get; set; }

        /// <summary>
        /// Gets or sets the time taken for the import operation
        /// </summary>
        public TimeSpan ImportDuration { get; set; }
    }

    /// <summary>
    /// Warning encountered during import
    /// </summary>
    public class ImportWarning
    {
        /// <summary>
        /// Gets or sets the line number in the source file where the warning occurred
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the action name that caused the warning
        /// </summary>
        public string ActionName { get; set; } = "";

        /// <summary>
        /// Gets or sets the warning message
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets the category of the warning
        /// </summary>
        public WarningCategory Category { get; set; }
    }

    /// <summary>
    /// Error encountered during import
    /// </summary>
    public class ImportError
    {
        /// <summary>
        /// Gets or sets the line number in the source file where the error occurred
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the action name that caused the error
        /// </summary>
        public string ActionName { get; set; } = "";

        /// <summary>
        /// Gets or sets the error message
        /// </summary>
        public string Message { get; set; } = "";

        /// <summary>
        /// Gets or sets the category of the error
        /// </summary>
        public ErrorCategory Category { get; set; }

        /// <summary>
        /// Gets or sets the exception that caused the error (if any)
        /// </summary>
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Categories of import warnings
    /// </summary>
    public enum WarningCategory
    {
        /// <summary>
        /// Feature not supported and will be skipped
        /// </summary>
        UnsupportedFeature,

        /// <summary>
        /// Data format issue that was automatically corrected
        /// </summary>
        DataFormatCorrected,

        /// <summary>
        /// Mapping approximation due to differences between systems
        /// </summary>
        MappingApproximation,

        /// <summary>
        /// Performance consideration
        /// </summary>
        Performance
    }

    /// <summary>
    /// Categories of import errors
    /// </summary>
    public enum ErrorCategory
    {
        /// <summary>
        /// File format or parsing error
        /// </summary>
        ParseError,

        /// <summary>
        /// Invalid data in the source file
        /// </summary>
        InvalidData,

        /// <summary>
        /// Conversion logic error
        /// </summary>
        ConversionError,

        /// <summary>
        /// File system or I/O error
        /// </summary>
        FileSystemError
    }
}
