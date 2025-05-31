using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MIDIFlux.GUI.Services.Import.Models;

namespace MIDIFlux.GUI.Services.Import.Parsers
{
    /// <summary>
    /// Parser for MIDIKey2Key INI configuration files
    /// </summary>
    public class IniFileParser
    {
        /// <summary>
        /// Parses a MIDIKey2Key INI file and returns the configuration
        /// </summary>
        /// <param name="filePath">Path to the INI file</param>
        /// <returns>Parsed configuration</returns>
        /// <exception cref="FileNotFoundException">Thrown when the file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when the file format is invalid</exception>
        public MidiKey2KeyConfig ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"INI file not found: {filePath}");
            }

            var config = new MidiKey2KeyConfig
            {
                SourceFilePath = filePath
            };

            var lines = File.ReadAllLines(filePath, Encoding.UTF8);
            var currentSection = "";
            var currentAction = new MidiKey2KeyAction();
            var lineNumber = 0;

            foreach (var line in lines)
            {
                lineNumber++;
                var trimmedLine = line.Trim();

                // Skip empty lines and comments
                if (string.IsNullOrWhiteSpace(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                {
                    continue;
                }

                // Check for section headers [SectionName]
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    // Save previous action if it was an action section
                    if (IsActionSection(currentSection) && !string.IsNullOrWhiteSpace(currentAction.Name))
                    {
                        config.Actions.Add(currentAction);
                    }

                    // Start new section
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2);
                    
                    if (IsActionSection(currentSection))
                    {
                        currentAction = new MidiKey2KeyAction
                        {
                            SectionName = currentSection,
                            SourceLineNumber = lineNumber
                        };
                    }
                    continue;
                }

                // Parse key=value pairs
                var equalIndex = trimmedLine.IndexOf('=');
                if (equalIndex <= 0)
                {
                    continue; // Skip malformed lines
                }

                var key = trimmedLine.Substring(0, equalIndex).Trim();
                var value = trimmedLine.Substring(equalIndex + 1).Trim();

                // Process the key-value pair based on current section
                if (IsActionSection(currentSection))
                {
                    ParseActionProperty(currentAction, key, value);
                }
                else
                {
                    // Store global or device settings
                    if (currentSection.ToLowerInvariant().Contains("device"))
                    {
                        config.DeviceSettings[$"{currentSection}.{key}"] = value;
                    }
                    else
                    {
                        config.GlobalSettings[$"{currentSection}.{key}"] = value;
                    }
                }
            }

            // Don't forget the last action
            if (IsActionSection(currentSection) && !string.IsNullOrWhiteSpace(currentAction.Name))
            {
                config.Actions.Add(currentAction);
            }

            return config;
        }

        /// <summary>
        /// Checks if a section name represents an action (e.g., "Action0", "Action1")
        /// </summary>
        /// <param name="sectionName">The section name to check</param>
        /// <returns>True if it's an action section, false otherwise</returns>
        private static bool IsActionSection(string sectionName)
        {
            return !string.IsNullOrWhiteSpace(sectionName) && 
                   sectionName.StartsWith("Action", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Parses a property for an action
        /// </summary>
        /// <param name="action">The action to update</param>
        /// <param name="key">The property key</param>
        /// <param name="value">The property value</param>
        private static void ParseActionProperty(MidiKey2KeyAction action, string key, string value)
        {
            switch (key.ToLowerInvariant())
            {
                case "name":
                    action.Name = value;
                    break;
                case "comment":
                    action.Comment = value;
                    break;
                case "data":
                    action.Data = value;
                    break;
                case "keyboard":
                    action.Keyboard = value;
                    break;
                case "keyboardb":
                    action.KeyboardB = value;
                    break;
                case "keyboarddelay":
                    if (int.TryParse(value, out var delay))
                    {
                        action.KeyboardDelay = delay;
                    }
                    break;
                case "hold":
                    action.Hold = ParseBooleanValue(value);
                    break;
                case "sendmidi":
                    action.SendMidi = ParseBooleanValue(value);
                    break;
                case "sendmidicommands":
                    action.SendMidiCommands = value;
                    break;
                case "controlleraction":
                    action.ControllerAction = ParseBooleanValue(value);
                    break;
                case "start":
                    action.Start = value;
                    break;
                case "workingdirectory":
                    action.WorkingDirectory = value;
                    break;
                case "arguments":
                    action.Arguments = value;
                    break;
                // Ignore unknown properties
            }
        }

        /// <summary>
        /// Parses a boolean value from INI format (0/1, true/false, yes/no)
        /// </summary>
        /// <param name="value">The string value to parse</param>
        /// <returns>The parsed boolean value</returns>
        private static bool ParseBooleanValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            var lowerValue = value.ToLowerInvariant();
            return lowerValue == "1" || 
                   lowerValue == "true" || 
                   lowerValue == "yes" || 
                   lowerValue == "on";
        }

        /// <summary>
        /// Validates that a file appears to be a MIDIKey2Key configuration
        /// </summary>
        /// <param name="filePath">Path to the file to validate</param>
        /// <returns>True if the file appears valid, false otherwise</returns>
        public bool ValidateFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                    return false;

                var lines = File.ReadAllLines(filePath);
                var hasActionSection = false;

                foreach (var line in lines)
                {
                    var trimmedLine = line.Trim();
                    if (trimmedLine.StartsWith("[Action", StringComparison.OrdinalIgnoreCase) && 
                        trimmedLine.EndsWith("]"))
                    {
                        hasActionSection = true;
                        break;
                    }
                }

                return hasActionSection;
            }
            catch
            {
                return false;
            }
        }
    }
}
