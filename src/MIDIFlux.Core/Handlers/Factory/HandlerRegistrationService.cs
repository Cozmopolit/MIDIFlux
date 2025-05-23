using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Handlers.Factory;

/// <summary>
/// Service for registering MIDI control handlers
/// </summary>
public class HandlerRegistrationService
{
    private readonly ILogger _logger;
    private readonly Dictionary<int, Dictionary<int, MidiControlHandler>> _deviceHandlers;

    /// <summary>
    /// Creates a new instance of the HandlerRegistrationService
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="deviceHandlers">The device handlers dictionary to use</param>
    public HandlerRegistrationService(
        ILogger logger,
        Dictionary<int, Dictionary<int, MidiControlHandler>> deviceHandlers)
    {
        _logger = logger;
        _deviceHandlers = deviceHandlers;
    }

    /// <summary>
    /// Gets the device handlers for a specific device ID
    /// </summary>
    /// <param name="deviceId">The device ID</param>
    /// <returns>The device handlers, or null if not found</returns>
    public Dictionary<int, MidiControlHandler>? GetDeviceHandlers(int deviceId)
    {
        if (_deviceHandlers.TryGetValue(deviceId, out var handlers))
        {
            return handlers;
        }
        return null;
    }

    /// <summary>
    /// Registers an absolute value handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="controlNumber">The control number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterAbsoluteHandler(int deviceId, int controlNumber, IAbsoluteValueHandler handler)
    {
        // Ensure the device dictionary exists
        if (!_deviceHandlers.TryGetValue(deviceId, out var handlers))
        {
            handlers = new Dictionary<int, MidiControlHandler>();
            _deviceHandlers[deviceId] = handlers;
        }

        var midiHandler = new MidiControlHandler
        {
            ControlNumber = controlNumber,
            HandlerType = MidiHandlerType.Absolute,
            AbsoluteHandler = handler
        };

        handlers[controlNumber] = midiHandler;
        _logger.LogInformation("Registered absolute handler for device {DeviceId}, control {ControlNumber}: {Description}",
            deviceId, controlNumber, handler.Description);
    }

    /// <summary>
    /// Registers a relative value handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="controlNumber">The control number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterRelativeHandler(int deviceId, int controlNumber, IRelativeValueHandler handler)
    {
        // Ensure the device dictionary exists
        if (!_deviceHandlers.TryGetValue(deviceId, out var handlers))
        {
            handlers = new Dictionary<int, MidiControlHandler>();
            _deviceHandlers[deviceId] = handlers;
        }

        var midiHandler = new MidiControlHandler
        {
            ControlNumber = controlNumber,
            HandlerType = MidiHandlerType.Relative,
            RelativeHandler = handler
        };

        handlers[controlNumber] = midiHandler;
        _logger.LogInformation("Registered relative handler for device {DeviceId}, control {ControlNumber}: {Description}",
            deviceId, controlNumber, handler.Description);
    }

    /// <summary>
    /// Registers a note handler
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="noteNumber">The note number to handle</param>
    /// <param name="handler">The handler to register</param>
    public void RegisterNoteHandler(int deviceId, int noteNumber, INoteHandler handler)
    {
        // Ensure the device dictionary exists
        if (!_deviceHandlers.TryGetValue(deviceId, out var handlers))
        {
            handlers = new Dictionary<int, MidiControlHandler>();
            _deviceHandlers[deviceId] = handlers;
        }

        var midiHandler = new MidiControlHandler
        {
            ControlNumber = noteNumber,
            HandlerType = MidiHandlerType.Note,
            NoteHandler = handler
        };

        handlers[noteNumber] = midiHandler;
        _logger.LogInformation("Registered note handler for device {DeviceId}, note {NoteNumber}: {Description}",
            deviceId, noteNumber, handler.Description);
    }


}
