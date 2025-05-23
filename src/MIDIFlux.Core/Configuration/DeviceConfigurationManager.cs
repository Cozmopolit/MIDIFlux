using MIDIFlux.Core.Actions;
using MIDIFlux.Core.GameController;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Config;

/// <summary>
/// Manages device configurations and mappings
/// </summary>
public class DeviceConfigurationManager
{
    private readonly ILogger _logger;
    private readonly HandlerFactory _handlerFactory;
    private readonly ActionFactory _actionFactory;
    private readonly IServiceProvider? _serviceProvider;
    private Models.Configuration? _configuration;
    private readonly Dictionary<int, Dictionary<int, MidiControlHandler>> _deviceHandlers = new();
    private readonly HandlerRegistrationService _registrationService;
    private readonly GameControllerMappingProcessor _gameControllerProcessor;

    /// <summary>
    /// Creates a new instance of the DeviceConfigurationManager
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="handlerFactory">The handler factory to use</param>
    /// <param name="actionFactory">The action factory to use</param>
    /// <param name="serviceProvider">The service provider to use for resolving dependencies</param>
    public DeviceConfigurationManager(
        ILogger logger,
        HandlerFactory handlerFactory,
        ActionFactory actionFactory,
        IServiceProvider? serviceProvider = null)
    {
        _logger = logger;
        _handlerFactory = handlerFactory;
        _actionFactory = actionFactory;
        _serviceProvider = serviceProvider;

        // Create the handler registration service
        _registrationService = new HandlerRegistrationService(logger, _deviceHandlers);

        // Create the game controller mapping processor
        _gameControllerProcessor = new GameControllerMappingProcessor(logger, handlerFactory, _registrationService);
    }

    /// <summary>
    /// Sets the configuration to use for mapping MIDI events to keyboard actions
    /// </summary>
    /// <param name="configuration">The configuration to use</param>
    public void SetConfiguration(Models.Configuration configuration)
    {
        _configuration = configuration;
        _logger.LogInformation("Device configuration manager configured with {DeviceCount} MIDI devices", configuration.MidiDevices.Count);

        // Clear existing handlers
        _deviceHandlers.Clear();

        // Process each device configuration
        foreach (var deviceConfig in configuration.MidiDevices)
        {
            ProcessDeviceConfiguration(deviceConfig);
        }
    }

    /// <summary>
    /// Gets the device handlers for a specific device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device handlers, or null if not found</returns>
    public Dictionary<int, MidiControlHandler>? GetDeviceHandlers(int deviceId)
    {
        return _registrationService.GetDeviceHandlers(deviceId);
    }

    /// <summary>
    /// Registers an absolute value handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="controlNumber">The control number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterAbsoluteHandler(int deviceId, int controlNumber, IAbsoluteValueHandler handler)
    {
        _registrationService.RegisterAbsoluteHandler(deviceId, controlNumber, handler);
    }

    /// <summary>
    /// Registers a relative value handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="controlNumber">The control number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterRelativeHandler(int deviceId, int controlNumber, IRelativeValueHandler handler)
    {
        _registrationService.RegisterRelativeHandler(deviceId, controlNumber, handler);
    }

    /// <summary>
    /// Registers a note handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="noteNumber">The note number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterNoteHandler(int deviceId, int noteNumber, INoteHandler handler)
    {
        _registrationService.RegisterNoteHandler(deviceId, noteNumber, handler);
    }

    /// <summary>
    /// Finds all device configurations that match a given device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>A list of matching device configurations, or an empty list if none found</returns>
    public List<MidiDeviceConfiguration> FindDeviceConfigsForId(int deviceId)
    {
        if (_configuration == null)
        {
            return new List<MidiDeviceConfiguration>();
        }

        // Get the device name from the device ID
        var deviceName = GetDeviceNameFromId(deviceId);
        if (string.IsNullOrEmpty(deviceName))
        {
            _logger.LogWarning("Could not find device name for device ID {DeviceId}", deviceId);
            // Fall back to the first device configuration for backward compatibility
            var firstConfig = _configuration.MidiDevices.FirstOrDefault();
            return firstConfig != null ? new List<MidiDeviceConfiguration> { firstConfig } : new List<MidiDeviceConfiguration>();
        }

        // Find all configurations that match this device name
        var matchingConfigs = _configuration.MidiDevices
            .Where(config => deviceName.Equals(config.DeviceName, StringComparison.OrdinalIgnoreCase) ||
                             deviceName.Contains(config.DeviceName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (matchingConfigs.Count == 0)
        {
            _logger.LogDebug("No configuration found for device name {DeviceName}", deviceName);
            // Fall back to the first device configuration for backward compatibility
            var firstConfig = _configuration.MidiDevices.FirstOrDefault();
            return firstConfig != null ? new List<MidiDeviceConfiguration> { firstConfig } : new List<MidiDeviceConfiguration>();
        }

        return matchingConfigs;
    }

    /// <summary>
    /// Gets the device name from a device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device name, or an empty string if not found</returns>
    private string GetDeviceNameFromId(int deviceId)
    {
        // This method would typically use a service to get the device name
        // For now, we'll use a simple approach that assumes the MidiManager is accessible

        // Try to get the device info from the MidiManager
        var midiManager = _serviceProvider?.GetService(typeof(MidiManager)) as MidiManager;
        if (midiManager != null)
        {
            var deviceInfo = midiManager.GetDeviceInfo(deviceId);
            if (deviceInfo != null)
            {
                return deviceInfo.Name;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Processes a device-specific configuration
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    private void ProcessDeviceConfiguration(MidiDeviceConfiguration deviceConfig)
    {
        // For now, we'll use a placeholder device ID of 0 for backward compatibility
        // The actual device ID will be determined at runtime when events are received
        int placeholderId = 0;

        _logger.LogInformation("Processing configuration for device: {DeviceName}", deviceConfig.DeviceName);

        // Register key mappings
        if (deviceConfig.Mappings != null && deviceConfig.Mappings.Count > 0)
        {
            _logger.LogInformation("Registering {Count} key mappings for device {DeviceName}",
                deviceConfig.Mappings.Count, deviceConfig.DeviceName);

            // These will be registered when note events are received
        }

        // Process absolute control mappings
        ProcessAbsoluteControlMappings(deviceConfig, placeholderId);

        // Process relative control mappings
        ProcessRelativeControlMappings(deviceConfig, placeholderId);

        // Process CC range mappings
        ProcessCCRangeMappings(deviceConfig, placeholderId);

        // Process macro mappings
        ProcessMacroMappings(deviceConfig, placeholderId);

        // Process game controller mappings
        _gameControllerProcessor.ProcessGameControllerMappings(deviceConfig, placeholderId);
    }

    /// <summary>
    /// Processes absolute control mappings
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    private void ProcessAbsoluteControlMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.AbsoluteControlMappings == null || deviceConfig.AbsoluteControlMappings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Registering {Count} absolute control handlers for device {DeviceName}",
            deviceConfig.AbsoluteControlMappings.Count, deviceConfig.DeviceName);

        foreach (var mapping in deviceConfig.AbsoluteControlMappings)
        {
            try
            {
                // Create handler using the factory
                var handler = _handlerFactory.CreateAbsoluteHandler(mapping.HandlerType, mapping.Parameters);

                if (handler != null)
                {
                    _registrationService.RegisterAbsoluteHandler(placeholderId, mapping.ControlNumber, handler);
                }
                else
                {
                    _logger.LogWarning("Failed to create handler of type {HandlerType} for control {ControlNumber}",
                        mapping.HandlerType, mapping.ControlNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register handler for control {ControlNumber}", mapping.ControlNumber);
            }
        }
    }

    /// <summary>
    /// Processes relative control mappings
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    private void ProcessRelativeControlMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.RelativeControlMappings == null || deviceConfig.RelativeControlMappings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Registering {Count} relative control handlers for device {DeviceName}",
            deviceConfig.RelativeControlMappings.Count, deviceConfig.DeviceName);

        foreach (var mapping in deviceConfig.RelativeControlMappings)
        {
            try
            {
                // Create handler using the factory
                // Create a new dictionary to avoid modifying the original
                var parameters = new Dictionary<string, object>();

                // Add all parameters from the mapping
                if (mapping.Parameters != null)
                {
                    foreach (var param in mapping.Parameters)
                    {
                        parameters[param.Key] = param.Value;
                    }
                }

                // Add sensitivity if specified
                if (mapping.Sensitivity > 0)
                {
                    parameters["sensitivity"] = mapping.Sensitivity;
                }

                var handler = _handlerFactory.CreateRelativeHandler(mapping.HandlerType, parameters);

                if (handler != null)
                {
                    _registrationService.RegisterRelativeHandler(placeholderId, mapping.ControlNumber, handler);
                }
                else
                {
                    _logger.LogWarning("Failed to create handler of type {HandlerType} for control {ControlNumber}",
                        mapping.HandlerType, mapping.ControlNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register handler for control {ControlNumber}", mapping.ControlNumber);
            }
        }
    }

    /// <summary>
    /// Processes CC range mappings
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    private void ProcessCCRangeMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.CCRangeMappings == null || deviceConfig.CCRangeMappings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Registering {Count} CC range mappings for device {DeviceName}",
            deviceConfig.CCRangeMappings.Count, deviceConfig.DeviceName);

        foreach (var mapping in deviceConfig.CCRangeMappings)
        {
            try
            {
                // Create parameters for the handler
                var parameters = new Dictionary<string, object>
                {
                    ["ranges"] = mapping.Ranges,
                    ["description"] = mapping.Description ?? $"CC Range Mapping for controller {mapping.ControlNumber}"
                };

                // Create handler using the factory
                var handler = _handlerFactory.CreateAbsoluteHandler("CCRange", parameters);

                if (handler != null)
                {
                    _registrationService.RegisterAbsoluteHandler(placeholderId, mapping.ControlNumber, handler);
                }
                else
                {
                    _logger.LogWarning("Failed to create CCRange handler for control {ControlNumber}",
                        mapping.ControlNumber);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register CCRange handler for control {ControlNumber}",
                    mapping.ControlNumber);
            }
        }
    }

    /// <summary>
    /// Processes macro mappings for a device configuration
    /// </summary>
    /// <param name="deviceConfig">The device configuration</param>
    /// <param name="placeholderId">The placeholder device ID</param>
    private void ProcessMacroMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.MacroMappings == null || deviceConfig.MacroMappings.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Processing {Count} macro mappings for device {DeviceName}",
            deviceConfig.MacroMappings.Count, deviceConfig.DeviceName);

        foreach (var macroMapping in deviceConfig.MacroMappings)
        {
            try
            {
                // Create a macro handler for this mapping
                var handler = CreateMacroHandler(macroMapping);

                if (handler != null)
                {
                    _registrationService.RegisterNoteHandler(placeholderId, macroMapping.MidiNote, handler);
                    _logger.LogDebug("Registered macro mapping for note {MidiNote}: {Description}",
                        macroMapping.MidiNote, macroMapping.Description);
                }
                else
                {
                    _logger.LogWarning("Failed to create macro handler for note {MidiNote}",
                        macroMapping.MidiNote);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register macro mapping for note {MidiNote}",
                    macroMapping.MidiNote);
            }
        }
    }

    /// <summary>
    /// Creates a macro handler from a macro mapping
    /// </summary>
    /// <param name="macroMapping">The macro mapping</param>
    /// <returns>The created macro handler, or null if creation failed</returns>
    private INoteHandler? CreateMacroHandler(MacroMapping macroMapping)
    {
        try
        {
            // Convert MacroActionDefinitions to IAction objects
            var actions = new List<IAction>();

            foreach (var actionDef in macroMapping.Actions)
            {
                // Skip nested macro actions to prevent recursion
                if (actionDef.Type == ActionType.Macro)
                {
                    _logger.LogWarning("Skipping nested macro action in macro mapping for note {MidiNote} - nested macros are not supported",
                        macroMapping.MidiNote);
                    continue;
                }

                var action = CreateActionFromDefinition(actionDef);
                if (action != null)
                {
                    actions.Add(action);
                }
            }

            if (actions.Count > 0)
            {
                // Create a macro action
                var macro = new Actions.MacroAction(_logger, actions, macroMapping.Description);

                // Create a macro handler
                return new Handlers.MacroHandler(_logger, macro, macroMapping.Description);
            }

            _logger.LogWarning("No valid actions found in macro mapping for note {MidiNote}",
                macroMapping.MidiNote);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating macro handler for note {MidiNote}",
                macroMapping.MidiNote);
            return null;
        }
    }

    /// <summary>
    /// Creates an IAction from a MacroActionDefinition
    /// </summary>
    /// <param name="actionDef">The action definition</param>
    /// <returns>The created action, or null if creation failed</returns>
    private IAction? CreateActionFromDefinition(MacroActionDefinition actionDef)
    {
        try
        {
            // Convert MacroActionDefinition to parameters dictionary for the action factory
            var parameters = new Dictionary<string, object>();

            // Add common properties
            if (actionDef.VirtualKeyCode.HasValue)
                parameters["virtualKeyCode"] = actionDef.VirtualKeyCode.Value;

            if (actionDef.Modifiers != null && actionDef.Modifiers.Count > 0)
                parameters["modifiers"] = actionDef.Modifiers;

            if (actionDef.Milliseconds.HasValue)
                parameters["milliseconds"] = actionDef.Milliseconds.Value;

            if (!string.IsNullOrEmpty(actionDef.Command))
                parameters["command"] = actionDef.Command;

            if (actionDef.ShellType.HasValue)
                parameters["shellType"] = actionDef.ShellType.Value.ToString();

            parameters["runHidden"] = actionDef.RunHidden;
            parameters["waitForExit"] = actionDef.WaitForExit;

            if (actionDef.MouseX.HasValue)
                parameters["mouseX"] = actionDef.MouseX.Value;

            if (actionDef.MouseY.HasValue)
                parameters["mouseY"] = actionDef.MouseY.Value;

            if (actionDef.MouseButton.HasValue)
                parameters["mouseButton"] = actionDef.MouseButton.Value.ToString();

            if (!string.IsNullOrEmpty(actionDef.Description))
                parameters["description"] = actionDef.Description;

            // Use the action factory to create the action
            return _actionFactory.CreateAction(actionDef.Type, parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating action from definition: {ActionType}",
                actionDef.Type);
            return null;
        }
    }
}