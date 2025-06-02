using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Keyboard;
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
                // Use the actual input device name if available, otherwise fallback to wildcard
                var deviceConfig = new DeviceConfig
                {
                    DeviceName = config.InputDeviceName ?? "*", // Use actual device name or wildcard
                    Mappings = new List<MappingConfigEntry>()
                };

                // Add informational messages about device preservation
                if (!string.IsNullOrWhiteSpace(config.InputDeviceName))
                {
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Preserved input device: '{config.InputDeviceName}'",
                        LineNumber = 0
                    });
                }
                else
                {
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = "No input device specified in MIDIKey2Key configuration, using wildcard (*) to match any device",
                        LineNumber = 0
                    });
                }

                if (!string.IsNullOrWhiteSpace(config.OutputDeviceName))
                {
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = $"Preserved output device: '{config.OutputDeviceName}' for MIDI output actions",
                        LineNumber = 0
                    });
                }
                else
                {
                    result.Warnings.Add(new ImportWarning
                    {
                        Message = "No output device specified in MIDIKey2Key configuration, using 'Default MIDI Output' for MIDI output actions",
                        LineNumber = 0
                    });
                }

                // Convert each action
                foreach (var action in config.Actions)
                {
                    await ConvertSingleActionAsync(action, deviceConfig, config, options, result);
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
        /// <param name="config">MIDIKey2Key configuration for device information</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking statistics</param>
        private async Task ConvertSingleActionAsync(MidiKey2KeyAction action, DeviceConfig deviceConfig, MidiKey2KeyConfig config, ImportOptions options, ImportResult result)
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

                // Check if this is a hold mode action with keyboard input
                // Note: KeyboardB is only relevant when ControllerAction is enabled
                bool isHoldModeKeyboard = action.Hold &&
                    (!string.IsNullOrWhiteSpace(action.Keyboard) ||
                     (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB)));

                if (isHoldModeKeyboard)
                {
                    // Handle hold mode: create NoteOn->KeyDown and NoteOff->KeyUp mappings
                    await ConvertHoldModeActionAsync(action, deviceConfig, config, options, result);
                }
                else
                {
                    // Handle regular action: create single mapping
                    await ConvertRegularActionAsync(action, deviceConfig, config, options, result);
                }
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
        /// Converts a hold mode action to NoteOn->KeyDown and NoteOff->KeyUp mappings
        /// </summary>
        /// <param name="action">MIDIKey2Key action with Hold=true</param>
        /// <param name="deviceConfig">Target device configuration</param>
        /// <param name="config">MIDIKey2Key configuration for device information</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking statistics</param>
        private async Task ConvertHoldModeActionAsync(MidiKey2KeyAction action, DeviceConfig deviceConfig, MidiKey2KeyConfig config, ImportOptions options, ImportResult result)
        {
            // Parse MIDI input data to get note/channel information
            var midiData = _mappingConverter.ParseMidiData(action.Data);
            if (midiData == null || !midiData.IsValid)
            {
                result.Statistics.ActionsFailed++;
                result.Warnings.Add(new ImportWarning
                {
                    Message = $"Failed to parse MIDI data for hold mode action: {action.Name}",
                    LineNumber = 0
                });
                return;
            }

            // Only support hold mode for Note events (not SysEx or other types)
            if (midiData.IsSysEx || midiData.MessageType != MidiMessageType.NoteOn)
            {
                result.Statistics.ActionsFailed++;
                result.Warnings.Add(new ImportWarning
                {
                    Message = $"Hold mode is only supported for Note events, skipping action: {action.Name}",
                    LineNumber = 0
                });
                return;
            }

            // Convert keyboard actions to KeyDown/KeyUp actions
            var keyDownActions = await ConvertToHoldModeKeyActionsAsync(action);
            if (keyDownActions == null || keyDownActions.Count == 0)
            {
                result.Statistics.ActionsFailed++;
                result.Warnings.Add(new ImportWarning
                {
                    Message = $"Failed to convert keyboard actions for hold mode: {action.Name}",
                    LineNumber = 0
                });
                return;
            }

            // Create NoteOn mapping (KeyDown)
            var noteOnMapping = new MappingConfigEntry
            {
                Description = $"{action.Comment ?? action.Name} (Press)",
                IsEnabled = true,
                InputType = "NoteOn",
                Note = midiData.Data1,
                Channel = midiData.Channel,
                Action = keyDownActions.Count == 1 ? keyDownActions[0] : CreateSequenceAction(keyDownActions, $"{action.Name} - Key Down Sequence")
            };

            // Create NoteOff mapping (KeyUp)
            var keyUpActions = ConvertKeyDownToKeyUpActions(keyDownActions);
            var noteOffMapping = new MappingConfigEntry
            {
                Description = $"{action.Comment ?? action.Name} (Release)",
                IsEnabled = true,
                InputType = "NoteOff",
                Note = midiData.Data1,
                Channel = midiData.Channel,
                Action = keyUpActions.Count == 1 ? keyUpActions[0] : CreateSequenceAction(keyUpActions, $"{action.Name} - Key Up Sequence")
            };

            // Add both mappings to device configuration
            deviceConfig.Mappings.Add(noteOnMapping);
            deviceConfig.Mappings.Add(noteOffMapping);

            // Update statistics
            result.Statistics.ActionsConverted++;
            result.Statistics.KeyboardActionsCreated += 2; // Two mappings created

            // Add informational message about hold mode conversion
            result.Warnings.Add(new ImportWarning
            {
                Message = $"Converted hold mode action '{action.Name}' to NoteOn->KeyDown and NoteOff->KeyUp mappings",
                LineNumber = 0
            });

            await Task.CompletedTask;
        }

        /// <summary>
        /// Converts a regular (non-hold mode) action to a single mapping
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <param name="deviceConfig">Target device configuration</param>
        /// <param name="config">MIDIKey2Key configuration for device information</param>
        /// <param name="options">Import options</param>
        /// <param name="result">Import result for tracking statistics</param>
        private async Task ConvertRegularActionAsync(MidiKey2KeyAction action, DeviceConfig deviceConfig, MidiKey2KeyConfig config, ImportOptions options, ImportResult result)
        {
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

            // Convert action with device information
            var convertedAction = _actionConverter.ConvertAction(action, config.OutputDeviceName);
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
            if (!string.IsNullOrWhiteSpace(action.Keyboard) ||
                (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB)))
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

            await Task.CompletedTask;
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
            if (!string.IsNullOrWhiteSpace(action.Keyboard) ||
                (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB)))
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
            var hasKeyboard = !string.IsNullOrWhiteSpace(action.Keyboard) ||
                              (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB));
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

        /// <summary>
        /// Converts keyboard actions to KeyDown actions for hold mode
        /// </summary>
        /// <param name="action">MIDIKey2Key action</param>
        /// <returns>List of KeyDown actions</returns>
        private async Task<List<ActionBase>> ConvertToHoldModeKeyActionsAsync(MidiKey2KeyAction action)
        {
            var keyDownActions = new List<ActionBase>();

            // Convert primary keyboard action
            if (!string.IsNullOrWhiteSpace(action.Keyboard))
            {
                var keyDownAction = ConvertToKeyDownAction(action.Keyboard, action);
                if (keyDownAction != null)
                {
                    keyDownActions.Add(keyDownAction);
                }
            }

            // Convert secondary keyboard action (only when ControllerAction is enabled)
            if (action.ControllerAction && !string.IsNullOrWhiteSpace(action.KeyboardB))
            {
                var keyDownAction = ConvertToKeyDownAction(action.KeyboardB, action);
                if (keyDownAction != null)
                {
                    keyDownActions.Add(keyDownAction);
                }
            }

            await Task.CompletedTask;
            return keyDownActions;
        }

        /// <summary>
        /// Converts a keyboard string to a KeyDown action
        /// </summary>
        /// <param name="keyboardString">Keyboard string (e.g., "CTRL+C")</param>
        /// <param name="action">Original MIDIKey2Key action for context</param>
        /// <returns>KeyDown action or null if conversion failed</returns>
        private ActionBase? ConvertToKeyDownAction(string keyboardString, MidiKey2KeyAction action)
        {
            try
            {
                // Parse the keyboard string
                var keyboardParser = new KeyboardStringParser();
                var keySequence = keyboardParser.ParseKeyboardString(keyboardString);

                if (!keySequence.IsValid || keySequence.MainKeys.Count != 1)
                {
                    return null; // Only support single key + modifiers for hold mode
                }

                var mainKey = keySequence.MainKeys[0];
                var keyDownAction = ActionTypeRegistry.Instance.CreateActionInstance("KeyDownAction");
                if (keyDownAction == null)
                {
                    return null;
                }

                // Set the main key
                var keysValue = (Keys)mainKey.VirtualKeyCode;
                keyDownAction.SetParameterValue("VirtualKeyCode", keysValue);
                keyDownAction.Description = $"Hold {keyboardString}";

                // For hold mode with modifiers, we need to handle them differently
                // In hold mode, modifiers should also be held down
                if (keySequence.ModifierKeys.Count > 0)
                {
                    // Create a sequence that holds down all modifiers, then the main key
                    var sequenceActions = new List<ActionBase>();

                    // Add KeyDown actions for each modifier
                    foreach (var modifier in keySequence.ModifierKeys)
                    {
                        var modifierKeyDown = ActionTypeRegistry.Instance.CreateActionInstance("KeyDownAction");
                        if (modifierKeyDown != null)
                        {
                            var modifierKeysValue = (Keys)modifier.VirtualKeyCode;
                            modifierKeyDown.SetParameterValue("VirtualKeyCode", modifierKeysValue);
                            modifierKeyDown.Description = $"Hold {modifier.KeyName}";
                            sequenceActions.Add(modifierKeyDown);
                        }
                    }

                    // Add the main key
                    sequenceActions.Add(keyDownAction);

                    // Return a sequence action
                    return CreateSequenceAction(sequenceActions, $"Hold {keyboardString} (with modifiers)");
                }

                return keyDownAction;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Converts KeyDown actions to corresponding KeyUp actions
        /// </summary>
        /// <param name="keyDownActions">List of KeyDown actions</param>
        /// <returns>List of corresponding KeyUp actions</returns>
        private List<ActionBase> ConvertKeyDownToKeyUpActions(List<ActionBase> keyDownActions)
        {
            var keyUpActions = new List<ActionBase>();

            foreach (var keyDownAction in keyDownActions)
            {
                var keyUpAction = ConvertSingleKeyDownToKeyUp(keyDownAction);
                if (keyUpAction != null)
                {
                    keyUpActions.Add(keyUpAction);
                }
            }

            // Reverse the order for key up (release in reverse order)
            keyUpActions.Reverse();
            return keyUpActions;
        }

        /// <summary>
        /// Converts a single KeyDown action to a KeyUp action
        /// </summary>
        /// <param name="keyDownAction">KeyDown action</param>
        /// <returns>Corresponding KeyUp action</returns>
        private ActionBase? ConvertSingleKeyDownToKeyUp(ActionBase keyDownAction)
        {
            try
            {
                // Handle sequence actions (modifiers + main key)
                if (keyDownAction.GetType().Name.Contains("Sequence"))
                {
                    var subActions = keyDownAction.GetParameterValue<List<ActionBase>>("SubActions");
                    if (subActions != null)
                    {
                        var keyUpSubActions = ConvertKeyDownToKeyUpActions(subActions);
                        return CreateSequenceAction(keyUpSubActions, keyDownAction.Description?.Replace("Hold", "Release") ?? "Release keys");
                    }
                }

                // Handle single KeyDown action
                if (keyDownAction.GetType().Name.Contains("KeyDown"))
                {
                    var virtualKeyCode = keyDownAction.GetParameterValue<Keys>("VirtualKeyCode");
                    var keyUpAction = ActionTypeRegistry.Instance.CreateActionInstance("KeyUpAction");
                    if (keyUpAction != null)
                    {
                        keyUpAction.SetParameterValue("VirtualKeyCode", virtualKeyCode);
                        keyUpAction.Description = keyDownAction.Description?.Replace("Hold", "Release") ?? "Release key";
                        return keyUpAction;
                    }
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Creates a sequence action from a list of sub-actions
        /// </summary>
        /// <param name="subActions">List of sub-actions</param>
        /// <param name="description">Description for the sequence</param>
        /// <returns>Sequence action</returns>
        /// <exception cref="InvalidOperationException">Thrown when SequenceAction cannot be created</exception>
        private ActionBase CreateSequenceAction(List<ActionBase> subActions, string description)
        {
            var sequenceAction = ActionTypeRegistry.Instance.CreateActionInstance("SequenceAction");
            if (sequenceAction == null)
            {
                throw new InvalidOperationException("Failed to create SequenceAction instance during import. This indicates a serious configuration issue.");
            }

            sequenceAction.SetParameterValue("SubActions", subActions);
            sequenceAction.SetParameterValue("ErrorHandling", "ContinueOnError");
            sequenceAction.Description = description;

            return sequenceAction;
        }
    }
}
