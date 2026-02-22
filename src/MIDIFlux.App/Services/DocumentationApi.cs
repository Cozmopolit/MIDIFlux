using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using System.Reflection;
using System.Text.Json;

namespace MIDIFlux.App.Services;

/// <summary>
/// API for MCP-specific documentation and capability discovery.
/// Provides structured information about MIDIFlux capabilities for LLM consumption.
/// </summary>
public class DocumentationApi
{
    private readonly ILogger<DocumentationApi> _logger;
    private readonly MidiDeviceManager _midiDeviceManager;
    private readonly IMidiHardwareAdapter _hardwareAdapter;

    /// <summary>
    /// Initializes a new instance of the DocumentationApi
    /// </summary>
    /// <param name="logger">Logger for this API</param>
    /// <param name="midiDeviceManager">MIDI device manager for device information</param>
    /// <param name="hardwareAdapter">MIDI hardware adapter for backend detection</param>
    public DocumentationApi(
        ILogger<DocumentationApi> logger,
        MidiDeviceManager midiDeviceManager,
        IMidiHardwareAdapter hardwareAdapter)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _midiDeviceManager = midiDeviceManager ?? throw new ArgumentNullException(nameof(midiDeviceManager));
        _hardwareAdapter = hardwareAdapter ?? throw new ArgumentNullException(nameof(hardwareAdapter));
    }

    /// <summary>
    /// Get comprehensive system information about MIDIFlux capabilities.
    /// Essential starting point for understanding what MIDIFlux can do.
    /// </summary>
    /// <returns>Structured capability information</returns>
    public object GetCapabilities()
    {
        try
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.8.0";
            
            var capabilities = new
            {
                version = version,
                apiVersion = "1.0",
                features = new[]
                {
                    "profile_management",
                    "runtime_configuration",
                    "midi_input_detection",
                    "action_system",
                    "device_management",
                    "mapping_management"
                },
                supportedTransports = new[] { "stdio" },
                maxDetectionDuration = 20,
                profilesDirectory = "%AppData%/MIDIFlux/profiles",
                examplesDirectory = "%AppData%/MIDIFlux/profiles/examples",
                examplesNote = "Use midi_list_profiles to discover examples, filter by directory='examples', then use midi_get_profile_content to load specific examples",
                configurationFormat = "JSON",
                midiChannelRange = "1-16",
                supportedFileFormats = new[] { "json" },
                audioFormats = new[] { "wav", "mp3", "aiff" },
                operatingSystem = "Windows",
                architecture = "x64"
            };

            _logger.LogDebug("Retrieved system capabilities");
            return capabilities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting capabilities: {ErrorMessage}", ex.Message);
            return new { error = "Failed to retrieve capabilities" };
        }
    }

    /// <summary>
    /// List all available action types with categories and descriptions.
    /// Use to discover what actions you can configure.
    /// </summary>
    /// <returns>List of action types with metadata</returns>
    public object GetActionTypes()
    {
        try
        {
            var actionTypes = new List<object>();
            var categories = new Dictionary<string, string>
            {
                ["keyboard"] = "Keyboard input simulation and shortcuts",
                ["mouse"] = "Mouse control and clicking",
                ["audio"] = "Audio playback and sound effects",
                ["midi"] = "MIDI output and device control",
                ["system"] = "System commands and application control",
                ["gamepad"] = "Game controller emulation (requires ViGEm)",
                ["complex"] = "Advanced logic and orchestration"
            };

            // Get all action types from the registry
            var allActionTypes = ActionTypeRegistry.Instance.GetAllActionTypes();

            foreach (var actionTypeKvp in allActionTypes)
            {
                var typeName = actionTypeKvp.Key;
                var type = actionTypeKvp.Value;

                var actionInfo = new
                {
                    type = typeName,
                    displayName = GetDisplayName(typeName),
                    description = GetActionDescription(typeName),
                    category = GetActionCategory(typeName),
                    compatibleInputs = GetCompatibleInputs(typeName),
                    parameters = GetActionParameters(type),
                    examples = GetActionExamples(typeName)
                };

                actionTypes.Add(actionInfo);
            }

            var result = new
            {
                actionTypes = actionTypes,
                categories = categories
            };

            _logger.LogDebug("Retrieved {Count} action types", actionTypes.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action types: {ErrorMessage}", ex.Message);
            return new { error = "Failed to retrieve action types" };
        }
    }

    /// <summary>
    /// Get detailed parameter schema for a specific action type.
    /// Use to understand how to configure a particular action.
    /// </summary>
    /// <param name="actionType">Action type name</param>
    /// <returns>JSON Schema for the action type's parameter structure</returns>
    public object GetActionSchema(string actionType)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(actionType))
            {
                _logger.LogWarning("Action type is null or empty");
                return new { error = "Action type is required" };
            }

            var type = ActionTypeRegistry.Instance.GetActionType(actionType);
            if (type == null)
            {
                _logger.LogWarning("Action type not found: {ActionType}", actionType);
                return new { error = $"Action type '{actionType}' not found" };
            }

            var schema = new
            {
                actionType = actionType,
                displayName = GetDisplayName(actionType),
                description = GetActionDescription(actionType),
                category = GetActionCategory(actionType),
                compatibleInputs = GetCompatibleInputs(actionType),
                parameters = GetDetailedParameters(type),
                examples = GetActionExamples(actionType)
            };

            _logger.LogDebug("Retrieved schema for action type: {ActionType}", actionType);
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting action schema for {ActionType}: {ErrorMessage}", actionType, ex.Message);
            return new { error = $"Failed to retrieve schema for action type '{actionType}'" };
        }
    }

    /// <summary>
    /// Learn about different types of MIDI input events and their characteristics.
    /// Essential for understanding what MIDI events can trigger actions.
    /// </summary>
    /// <returns>MIDI input type documentation</returns>
    public object GetInputTypes()
    {
        try
        {
            var inputTypes = new[]
            {
                new
                {
                    type = "NoteOn",
                    displayName = "Note On",
                    description = "MIDI note press events (key press on keyboard/pad)",
                    category = "trigger",
                    parameters = new[] { "channel", "note", "velocity" },
                    valueRange = "0-127",
                    examples = new[] { "Piano key press", "Drum pad hit", "Button press" }
                },
                new
                {
                    type = "NoteOff",
                    displayName = "Note Off",
                    description = "MIDI note release events (key release on keyboard/pad)",
                    category = "trigger",
                    parameters = new[] { "channel", "note", "velocity" },
                    valueRange = "0-127",
                    examples = new[] { "Piano key release", "Pad release" }
                },
                new
                {
                    type = "ControlChangeAbsolute",
                    displayName = "Control Change (Absolute)",
                    description = "Absolute controllers like faders and absolute knobs",
                    category = "absoluteValue",
                    parameters = new[] { "channel", "controller", "value" },
                    valueRange = "0-127",
                    examples = new[] { "Volume fader", "Pan knob", "Filter cutoff" }
                },
                new
                {
                    type = "ControlChangeRelative",
                    displayName = "Control Change (Relative)",
                    description = "Relative movement controllers like endless encoders",
                    category = "relativeValue",
                    parameters = new[] { "channel", "controller", "value" },
                    valueRange = "0-127 (delta values)",
                    examples = new[] { "Endless encoder", "Jog wheel", "Scratch wheel" }
                }
            };

            var categories = new Dictionary<string, string>
            {
                ["trigger"] = "Discrete events for 'do something now' actions",
                ["absoluteValue"] = "Continuous values representing positions/levels",
                ["relativeValue"] = "Delta movements for relative control"
            };

            var channelInfo = new
            {
                range = "1-16",
                wildcardSupport = true,
                description = "MIDI channels 1-16, null for all channels"
            };

            var result = new
            {
                inputTypes = inputTypes,
                categories = categories,
                channelInfo = channelInfo
            };

            _logger.LogDebug("Retrieved MIDI input types information");
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting input types: {ErrorMessage}", ex.Message);
            return new { error = "Failed to retrieve input types" };
        }
    }

    /// <summary>
    /// Get information about MIDI devices including connection status and configuration state.
    /// Use to check what MIDI hardware is available and properly connected.
    /// </summary>
    /// <returns>MIDI device information</returns>
    public object GetDeviceInfo()
    {
        try
        {
            var availableDevices = new List<object>();
            var devices = _midiDeviceManager.GetAvailableDevices();
            
            foreach (var device in devices)
            {
                availableDevices.Add(new
                {
                    name = device,
                    isConnected = true, // If it's in the list, it's connected
                    isConfigured = false, // Would need to check against current configuration
                    type = "input"
                });
            }

            var deviceSupport = new
            {
                wildcardDevice = "*",
                wildcardDescription = "Matches any connected MIDI input device",
                deviceNaming = "Uses Windows MIDI device names",
                hotPlugSupport = true
            };

            var configuration = new
            {
                inputOnly = true,
                outputSupport = "Via MidiNoteOnAction and MidiControlChangeAction",
                channelSupport = "1-16 plus wildcard (null)",
                deviceFiltering = "By exact name or wildcard (*)"
            };

            var result = new
            {
                deviceSupport = deviceSupport,
                availableDevices = availableDevices,
                configuration = configuration
            };

            _logger.LogDebug("Retrieved device information for {Count} devices", availableDevices.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting device info: {ErrorMessage}", ex.Message);
            return new { error = "Failed to retrieve device information" };
        }
    }

    #region Helper Methods

    private string GetDisplayName(string actionType)
    {
        return actionType switch
        {
            "KeyPressReleaseAction" => "Key Press Release",
            "KeyModifiedAction" => "Key Modified",
            "MouseClickAction" => "Mouse Click",
            "PlaySoundAction" => "Play Sound",
            "ConditionalAction" => "Conditional (CC Range)",
            _ => actionType.Replace("Action", "")
        };
    }

    private string GetActionDescription(string actionType)
    {
        return actionType switch
        {
            "KeyPressReleaseAction" => "Press and release a key (most common keyboard action)",
            "KeyModifiedAction" => "Execute modified key combinations (Ctrl+C, Alt+Tab, etc.)",
            "MouseClickAction" => "Simulate mouse button clicks",
            "PlaySoundAction" => "Play audio files with low-latency playback",
            "ConditionalAction" => "Execute different actions based on MIDI value ranges",
            _ => $"Execute {actionType.Replace("Action", "")} action"
        };
    }

    private string GetActionCategory(string actionType)
    {
        return actionType switch
        {
            "KeyPressReleaseAction" or "KeyModifiedAction" => "keyboard",
            "MouseClickAction" => "mouse",
            "PlaySoundAction" => "audio",
            "ConditionalAction" => "complex",
            _ when actionType.Contains("Midi") => "midi",
            _ when actionType.Contains("Gamepad") => "gamepad",
            _ => "system"
        };
    }

    private string[] GetCompatibleInputs(string actionType)
    {
        return actionType switch
        {
            "ConditionalAction" => new[] { "absoluteValue" },
            _ => new[] { "trigger" }
        };
    }

    private string[] GetActionParameters(Type actionType)
    {
        // This would need reflection to get actual parameters
        // For now, return basic parameter names
        return actionType.Name switch
        {
            "KeyPressReleaseAction" => new[] { "VirtualKeyCode" },
            "KeyModifiedAction" => new[] { "MainKey", "Modifier1", "Modifier2", "Modifier3", "Modifier4" },
            "MouseClickAction" => new[] { "Button" },
            "PlaySoundAction" => new[] { "FilePath", "Volume", "AudioDevice" },
            "ConditionalAction" => new[] { "Conditions" },
            _ => new[] { "Parameters" }
        };
    }

    private object GetDetailedParameters(Type actionType)
    {
        // This would need reflection to get actual parameter details
        // For now, return basic parameter schema
        return actionType.Name switch
        {
            "KeyPressReleaseAction" => new
            {
                VirtualKeyCode = new
                {
                    type = "enum",
                    displayName = "Key",
                    description = "The key to press and release",
                    required = true,
                    enumValues = new[] { "A", "B", "C", "Enter", "Space", "Escape", "F1", "F2" },
                    supportsKeyListening = true
                }
            },
            _ => new { }
        };
    }

    private string[] GetActionExamples(string actionType)
    {
        return actionType switch
        {
            "KeyPressReleaseAction" => new[] { "Press 'A' key", "Ctrl+C shortcut trigger" },
            "KeyModifiedAction" => new[] { "Ctrl+C", "Ctrl+Shift+A", "Alt+Tab" },
            "MouseClickAction" => new[] { "Left click", "Right click", "Middle click" },
            "PlaySoundAction" => new[] { "Play drum sample", "Sound effect trigger" },
            "ConditionalAction" => new[] { "Fader to buttons", "CC value-based switching" },
            _ => new[] { $"Example {actionType.Replace("Action", "")}" }
        };
    }

    #endregion

    #region System Info

    /// <summary>
    /// Get runtime system information including MIDI backend and ViGEm driver status.
    /// Separates dynamic system state from static capabilities.
    /// </summary>
    /// <returns>Structured system information</returns>
    public object GetSystemInfo()
    {
        try
        {
            // Detect active MIDI backend from runtime adapter type
            var midiBackend = _hardwareAdapter is WindowsMidiServicesAdapter
                ? "WindowsMidiServices"
                : "NAudio";

            // Get Windows MIDI Services availability status
            var adapterStatus = MidiAdapterFactory.GetAdapterStatus();

            // Check ViGEm availability via singleton (creates instance on first call)
            bool vigemInstalled;
            try
            {
                var gameControllerManager = GameControllerManager.GetInstance(_logger);
                vigemInstalled = gameControllerManager.IsViGEmAvailable;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to check ViGEm status: {Message}", ex.Message);
                vigemInstalled = false;
            }

            var result = new
            {
                midiBackend,
                windowsMidiServices = new
                {
                    osSupported = adapterStatus.OsSupportsWindowsMidiServices,
                    runtimeInstalled = adapterStatus.WindowsMidiServicesRuntimeInstalled
                },
                vigemInstalled
            };

            _logger.LogDebug("Retrieved system info: backend={Backend}, vigem={ViGEm}", midiBackend, vigemInstalled);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting system info: {ErrorMessage}", ex.Message);
            return new { error = "Failed to retrieve system info" };
        }
    }

    #endregion
}
