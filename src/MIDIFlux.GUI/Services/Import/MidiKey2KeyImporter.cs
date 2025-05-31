using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MIDIFlux.Core.Configuration;
using MIDIFlux.GUI.Services.Import.Converters;
using MIDIFlux.GUI.Services.Import.Models;
using MIDIFlux.GUI.Services.Import.Parsers;

namespace MIDIFlux.GUI.Services.Import
{
    /// <summary>
    /// Main orchestrator for importing MIDIKey2Key configuration files
    /// </summary>
    public class MidiKey2KeyImporter : IMidiKey2KeyImporter
    {
        private readonly IniFileParser _iniParser;
        private readonly MidiMappingConverter _mappingConverter;
        private readonly ActionConverter _actionConverter;

        /// <summary>
        /// Initializes a new instance of the MidiKey2KeyImporter class
        /// </summary>
        public MidiKey2KeyImporter()
        {
            _iniParser = new IniFileParser();
            _mappingConverter = new MidiMappingConverter();
            _actionConverter = new ActionConverter();
        }

        /// <summary>
        /// Imports a MIDIKey2Key configuration file and converts it to MIDIFlux format
        /// </summary>
        /// <param name="iniFilePath">Path to the MIDIKey2Key INI file</param>
        /// <param name="options">Import options</param>
        /// <returns>Import result with statistics and any errors/warnings</returns>
        public async Task<ImportResult> ImportConfigurationAsync(string iniFilePath, ImportOptions options)
        {
            var result = new ImportResult
            {
                Statistics = new ImportStatistics()
            };

            try
            {
                // Validate input
                if (!ValidateIniFile(iniFilePath))
                {
                    result.Success = false;
                    result.Errors.Add(new ImportError
                    {
                        Message = "Invalid or inaccessible INI file",
                        LineNumber = 0
                    });
                    return result;
                }

                // Parse the INI file
                var config = await ParseConfigurationAsync(iniFilePath);
                if (config == null)
                {
                    result.Success = false;
                    result.Errors.Add(new ImportError
                    {
                        Message = "Failed to parse INI file",
                        LineNumber = 0
                    });
                    return result;
                }

                result.Statistics.TotalActionsFound = config.Actions.Count;

                // Convert to MIDIFlux format
                var midiFluxConfig = await ConvertToMidiFluxConfigAsync(config, options, result);
                if (midiFluxConfig == null)
                {
                    result.Success = false;
                    result.Errors.Add(new ImportError
                    {
                        Message = "Failed to convert configuration to MIDIFlux format",
                        LineNumber = 0
                    });
                    return result;
                }

                // Save the converted configuration
                var outputPath = await SaveConfigurationAsync(midiFluxConfig, options, result);
                if (string.IsNullOrEmpty(outputPath))
                {
                    result.Success = false;
                    result.Errors.Add(new ImportError
                    {
                        Message = "Failed to save converted configuration",
                        LineNumber = 0
                    });
                    return result;
                }

                result.Success = true;
                result.OutputFilePath = outputPath;
                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Errors.Add(new ImportError
                {
                    Message = $"Unexpected error during import: {ex.Message}",
                    LineNumber = 0
                });
                return result;
            }
        }

        /// <summary>
        /// Validates that the specified INI file is a valid MIDIKey2Key configuration
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file to validate</param>
        /// <returns>True if the file is valid, false otherwise</returns>
        public bool ValidateIniFile(string iniFilePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(iniFilePath) || !File.Exists(iniFilePath))
                {
                    return false;
                }

                // Basic validation - check if file contains MIDIKey2Key action sections
                var content = File.ReadAllText(iniFilePath);
                return content.Contains("[Action") || content.Contains("Data=") || content.Contains("Keyboard=");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Previews the import without actually creating the configuration file
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file to preview</param>
        /// <returns>Preview information about what would be imported</returns>
        public ImportPreview PreviewImport(string iniFilePath)
        {
            var preview = new ImportPreview();

            try
            {
                if (!ValidateIniFile(iniFilePath))
                {
                    preview.IsValid = false;
                    preview.ValidationErrors.Add("Invalid or inaccessible INI file");
                    return preview;
                }

                // Parse the configuration for preview
                var config = ParseConfigurationAsync(iniFilePath).GetAwaiter().GetResult();
                if (config == null)
                {
                    preview.IsValid = false;
                    preview.ValidationErrors.Add("Failed to parse INI file");
                    return preview;
                }

                preview.IsValid = true;
                preview.TotalActions = config.Actions.Count;

                // Analyze actions for convertibility
                foreach (var action in config.Actions)
                {
                    var actionType = GetActionType(action);
                    if (!preview.ActionTypes.Contains(actionType))
                    {
                        preview.ActionTypes.Add(actionType);
                    }

                    if (CanConvertAction(action))
                    {
                        preview.ConvertibleActions++;
                    }
                    else
                    {
                        preview.SkippedActions++;
                    }
                }

                return preview;
            }
            catch (Exception ex)
            {
                preview.IsValid = false;
                preview.ValidationErrors.Add($"Error during preview: {ex.Message}");
                return preview;
            }
        }

        /// <summary>
        /// Parses the MIDIKey2Key INI file into a configuration object
        /// </summary>
        /// <param name="iniFilePath">Path to the INI file</param>
        /// <returns>Parsed configuration</returns>
        private async Task<MidiKey2KeyConfig?> ParseConfigurationAsync(string iniFilePath)
        {
            try
            {
                return await Task.Run(() => _iniParser.ParseFile(iniFilePath));
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts MIDIKey2Key configuration to MIDIFlux format
        /// </summary>
        /// <param name="config">MIDIKey2Key configuration</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking statistics</param>
        /// <returns>MIDIFlux configuration</returns>
        private async Task<MappingConfig?> ConvertToMidiFluxConfigAsync(MidiKey2KeyConfig config, ImportOptions options, ImportResult result)
        {
            try
            {
                var midiFluxConfig = new MappingConfig
                {
                    ProfileName = !string.IsNullOrWhiteSpace(options.ProfileName) ? options.ProfileName : "Imported from MIDIKey2Key",
                    Description = $"Imported from {Path.GetFileName(config.SourceFilePath)} on {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                    MidiDevices = new List<DeviceConfig>()
                };

                // Create a single device configuration for all mappings
                var deviceConfig = new DeviceConfig
                {
                    DeviceName = "*", // Use wildcard to match any device
                    Mappings = new List<MappingConfigEntry>()
                };

                // Convert each action
                foreach (var action in config.Actions)
                {
                    await ConvertSingleActionAsync(action, deviceConfig, options, result);
                }

                midiFluxConfig.MidiDevices.Add(deviceConfig);
                return midiFluxConfig;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts a single MIDIKey2Key action to MIDIFlux format
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <param name="deviceConfig">Target device configuration</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking statistics</param>
        private async Task ConvertSingleActionAsync(MidiKey2KeyAction action, DeviceConfig deviceConfig, ImportOptions options, ImportResult result)
        {
            try
            {
                // Skip startup actions
                if (action.Data == "STARTUP")
                {
                    result.Statistics.ActionsSkipped++;
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Skipped startup action: {action.Name}",
                        LineNumber = 0
                    });
                    return;
                }

                // Skip Train Simulator features if requested
                if (options.SkipTrainSimulatorFeatures && IsTrainSimulatorAction(action))
                {
                    result.Statistics.ActionsSkipped++;
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Skipped Train Simulator action: {action.Name}",
                        LineNumber = 0
                    });
                    return;
                }

                // Convert MIDI input mapping
                var mappingEntry = _mappingConverter.ConvertToMidiInput(action);
                if (mappingEntry == null)
                {
                    result.Statistics.ActionsFailed++;
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Failed to convert MIDI input for action: {action.Name}",
                        LineNumber = 0
                    });
                    return;
                }

                // Convert action
                var convertedAction = _actionConverter.ConvertAction(action);
                if (convertedAction == null)
                {
                    result.Statistics.ActionsFailed++;
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Failed to convert action: {action.Name}",
                        LineNumber = 0
                    });
                    return;
                }

                // Set the converted action
                mappingEntry.Action = convertedAction;

                // Add to device configuration
                deviceConfig.Mappings.Add(mappingEntry);

                // Update statistics
                result.Statistics.ActionsConverted++;
                if (!string.IsNullOrWhiteSpace(action.Keyboard) || !string.IsNullOrWhiteSpace(action.KeyboardB))
                {
                    result.Statistics.KeyboardActionsCreated++;
                }
                if (action.SendMidi && !string.IsNullOrWhiteSpace(action.SendMidiCommands))
                {
                    // Note: No MidiOutputActionsCreated property in ImportStatistics
                }
                if (!string.IsNullOrWhiteSpace(action.Start))
                {
                    result.Statistics.CommandExecutionsCreated++;
                }

                await Task.CompletedTask; // Make method async-compatible
            }
            catch (Exception ex)
            {
                result.Statistics.ActionsFailed++;
                result.Errors.Add(new ImportError
                {
                    Message = $"Error converting action '{action.Name}': {ex.Message}",
                    LineNumber = 0
                });
            }
        }

        /// <summary>
        /// Saves the converted MIDIFlux configuration to file
        /// </summary>
        /// <param name="config">MIDIFlux configuration</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking</param>
        /// <returns>Output file path, or null if failed</returns>
        private async Task<string?> SaveConfigurationAsync(MappingConfig config, ImportOptions options, ImportResult result)
        {
            try
            {
                // Determine output directory
                var outputDir = !string.IsNullOrWhiteSpace(options.OutputDirectory)
                    ? options.OutputDirectory
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIDIFlux", "profiles");

                // Ensure directory exists
                Directory.CreateDirectory(outputDir);

                // Generate filename
                var profileName = !string.IsNullOrWhiteSpace(options.ProfileName) ? options.ProfileName : "imported_profile";
                var fileName = $"{SanitizeFileName(profileName)}.json";
                var outputPath = Path.Combine(outputDir, fileName);

                // Handle file conflicts
                var counter = 1;
                var originalPath = outputPath;
                while (File.Exists(outputPath))
                {
                    var nameWithoutExt = Path.GetFileNameWithoutExtension(originalPath);
                    var extension = Path.GetExtension(originalPath);
                    outputPath = Path.Combine(outputDir, $"{nameWithoutExt}_{counter}{extension}");
                    counter++;
                }

                // Serialize and save
                var options_json = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                };

                var json = JsonSerializer.Serialize(config, options_json);
                await File.WriteAllTextAsync(outputPath, json);

                return outputPath;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Determines the action type for preview purposes
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <returns>Action type description</returns>
        private string GetActionType(MidiKey2KeyAction action)
        {
            if (!string.IsNullOrWhiteSpace(action.Start))
                return "Program Execution";
            if (!string.IsNullOrWhiteSpace(action.Keyboard) || !string.IsNullOrWhiteSpace(action.KeyboardB))
                return "Keyboard Action";
            if (action.SendMidi && !string.IsNullOrWhiteSpace(action.SendMidiCommands))
                return "MIDI Output";
            if (action.Data == "STARTUP")
                return "Startup Action";
            return "Unknown";
        }

        /// <summary>
        /// Checks if an action can be converted
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <returns>True if convertible</returns>
        private bool CanConvertAction(MidiKey2KeyAction action)
        {
            // Startup actions cannot be converted
            if (action.Data == "STARTUP")
                return false;

            // Must have some convertible content
            var hasProgram = !string.IsNullOrWhiteSpace(action.Start);
            var hasKeyboard = !string.IsNullOrWhiteSpace(action.Keyboard) || !string.IsNullOrWhiteSpace(action.KeyboardB);
            var hasMidiOutput = action.SendMidi && !string.IsNullOrWhiteSpace(action.SendMidiCommands);

            return hasProgram || hasKeyboard || hasMidiOutput;
        }

        /// <summary>
        /// Checks if an action is Train Simulator specific
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <returns>True if Train Simulator specific</returns>
        private bool IsTrainSimulatorAction(MidiKey2KeyAction action)
        {
            // Check for Train Simulator specific patterns
            var name = action.Name?.ToLowerInvariant() ?? "";
            var comment = action.Comment?.ToLowerInvariant() ?? "";
            var start = action.Start?.ToLowerInvariant() ?? "";

            return name.Contains("train") || name.Contains("simulator") ||
                   comment.Contains("train") || comment.Contains("simulator") ||
                   start.Contains("trainsim") || start.Contains("railworks");
        }

        /// <summary>
        /// Sanitizes a filename by removing invalid characters
        /// </summary>
        /// <param name="fileName">Original filename</param>
        /// <returns>Sanitized filename</returns>
        private string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;

            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }

            return sanitized;
        }
    }
}
