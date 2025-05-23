using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.GameController;

/// <summary>
/// Processes game controller mappings from device configurations
/// </summary>
public class GameControllerMappingProcessor
{
    private readonly ILogger _logger;
    private readonly HandlerFactory _handlerFactory;
    private readonly HandlerRegistrationService _registrationService;

    /// <summary>
    /// Creates a new instance of the GameControllerMappingProcessor
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="handlerFactory">The handler factory to use</param>
    /// <param name="registrationService">The handler registration service to use</param>
    public GameControllerMappingProcessor(
        ILogger logger,
        HandlerFactory handlerFactory,
        HandlerRegistrationService registrationService)
    {
        _logger = logger;
        _handlerFactory = handlerFactory;
        _registrationService = registrationService;
    }

    /// <summary>
    /// Processes game controller mappings from a device configuration
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    public void ProcessGameControllerMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        try
        {
            if (deviceConfig.GameControllerMappings == null)
            {
                return;
            }

            _logger.LogDebug("Processing game controller mappings for device {DeviceName}", deviceConfig.DeviceName);

            // Process button mappings
            try
            {
                ProcessButtonMappings(deviceConfig, placeholderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing game controller button mappings for device {DeviceName}: {Message}",
                    deviceConfig.DeviceName, ex.Message);
            }

            // Process axis mappings
            try
            {
                ProcessAxisMappings(deviceConfig, placeholderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing game controller axis mappings for device {DeviceName}: {Message}",
                    deviceConfig.DeviceName, ex.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing game controller mappings for device {DeviceName}: {Message}",
                deviceConfig.DeviceName, ex.Message);
        }
    }

    /// <summary>
    /// Processes game controller button mappings
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    private void ProcessButtonMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.GameControllerMappings?.Buttons == null ||
            deviceConfig.GameControllerMappings.Buttons.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Registering {Count} game controller button mappings for device {DeviceName}",
            deviceConfig.GameControllerMappings.Buttons.Count, deviceConfig.DeviceName);

        // Get the default controller index for all mappings
        int defaultControllerIndex = deviceConfig.GameControllerMappings.DefaultControllerIndex;

        foreach (var mapping in deviceConfig.GameControllerMappings.Buttons)
        {
            try
            {
                // Use the mapping's controller index if specified, otherwise use the default
                int controllerIndex = mapping.ControllerIndex;

                var parameters = new Dictionary<string, object>
                {
                    ["button"] = mapping.Button,
                    ["controllerIndex"] = controllerIndex
                };

                var handler = _handlerFactory.CreateNoteHandler("GameControllerButton", parameters);

                if (handler != null)
                {
                    _registrationService.RegisterNoteHandler(placeholderId, mapping.MidiNote, handler);
                }
                else
                {
                    _logger.LogWarning("Failed to create GameControllerButton handler for note {NoteNumber} (controller {ControllerIndex})",
                        mapping.MidiNote, controllerIndex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register GameControllerButton handler for note {NoteNumber}",
                    mapping.MidiNote);
            }
        }
    }

    /// <summary>
    /// Processes game controller axis mappings
    /// </summary>
    /// <param name="deviceConfig">The device configuration to process</param>
    /// <param name="placeholderId">The placeholder device ID to use</param>
    private void ProcessAxisMappings(MidiDeviceConfiguration deviceConfig, int placeholderId)
    {
        if (deviceConfig.GameControllerMappings?.Axes == null ||
            deviceConfig.GameControllerMappings.Axes.Count == 0)
        {
            return;
        }

        _logger.LogInformation("Registering {Count} game controller axis mappings for device {DeviceName}",
            deviceConfig.GameControllerMappings.Axes.Count, deviceConfig.DeviceName);

        foreach (var mapping in deviceConfig.GameControllerMappings.Axes)
        {
            try
            {
                // Use the mapping's controller index if specified, otherwise use the default
                int controllerIndex = mapping.ControllerIndex;

                var parameters = new Dictionary<string, object>
                {
                    ["axis"] = mapping.Axis,
                    ["minValue"] = mapping.MinValue,
                    ["maxValue"] = mapping.MaxValue,
                    ["invert"] = mapping.Invert,
                    ["controllerIndex"] = controllerIndex
                };

                var handler = _handlerFactory.CreateAbsoluteHandler("GameControllerAxis", parameters);

                if (handler != null)
                {
                    _registrationService.RegisterAbsoluteHandler(placeholderId, mapping.ControlNumber, handler);
                }
                else
                {
                    _logger.LogWarning("Failed to create GameControllerAxis handler for control {ControlNumber} (controller {ControllerIndex})",
                        mapping.ControlNumber, controllerIndex);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to register GameControllerAxis handler for control {ControlNumber}",
                    mapping.ControlNumber);
            }
        }
    }
}
