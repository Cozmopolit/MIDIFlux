using MIDIFlux.Core.Config;
using MIDIFlux.Core.Handlers.Factory;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Threading;

namespace MIDIFlux.Core.Midi;

/// <summary>
/// Handles specific MIDI event types
/// </summary>
public class MidiEventHandlers
{
    private readonly ILogger _logger;
    private readonly KeyboardActionExecutor _keyboardActionExecutor;
    private readonly DeviceConfigurationManager _deviceConfigManager;
    private readonly HandlerFactory _handlerFactory;
    private readonly KeyStateManager _keyStateManager;

    /// <summary>
    /// Creates a new instance of the MidiEventHandlers
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="keyboardActionExecutor">The keyboard action executor</param>
    /// <param name="deviceConfigManager">The device configuration manager</param>
    /// <param name="handlerFactory">The handler factory</param>
    /// <param name="keyStateManager">The key state manager</param>
    public MidiEventHandlers(
        ILogger logger,
        KeyboardActionExecutor keyboardActionExecutor,
        DeviceConfigurationManager deviceConfigManager,
        HandlerFactory handlerFactory,
        KeyStateManager keyStateManager)
    {
        _logger = logger;
        _keyboardActionExecutor = keyboardActionExecutor;
        _deviceConfigManager = deviceConfigManager;
        _handlerFactory = handlerFactory;
        _keyStateManager = keyStateManager;
    }

    /// <summary>
    /// Handles a Note On/Off event
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="midiEvent">The MIDI event to handle</param>
    /// <param name="deviceConfig">The device configuration</param>
    public void HandleNoteEvent(int deviceId, MidiEvent midiEvent, MidiDeviceConfiguration deviceConfig)
    {
        if (midiEvent.Note == null)
        {
            _logger.LogWarning("Note event missing note number");
            return;
        }

        int noteNumber = midiEvent.Note.Value;

        _logger.LogInformation("HandleNoteEvent: DeviceId={DeviceId}, Note={NoteNumber}, EventType={EventType}, DeviceName={DeviceName}",
            deviceId, noteNumber, midiEvent.EventType, deviceConfig.DeviceName);

        // Check if we have a note handler for this device and note
        var deviceHandlers = _deviceConfigManager.GetDeviceHandlers(deviceId);
        if (deviceHandlers != null &&
            deviceHandlers.TryGetValue(noteNumber, out var handler) &&
            handler.HandlerType == MidiHandlerType.Note &&
            handler.NoteHandler != null)
        {
            if (midiEvent.EventType == MidiEventType.NoteOn)
            {
                _logger.LogInformation("Device {DeviceId}: Note {NoteNumber} press → Executing note handler", deviceId, noteNumber);
                handler.NoteHandler.HandleNoteOn(midiEvent.Velocity ?? 127);
                return;
            }
            else if (midiEvent.EventType == MidiEventType.NoteOff)
            {
                _logger.LogInformation("Device {DeviceId}: Note {NoteNumber} release → Executing note handler", deviceId, noteNumber);
                handler.NoteHandler.HandleNoteOff();
                return;
            }
        }

        // Find the mapping for this note in the device configuration
        _logger.LogDebug("Looking for mapping with MidiNote={NoteNumber} in {MappingCount} mappings",
            noteNumber, deviceConfig.Mappings.Count);

        foreach (var m in deviceConfig.Mappings)
        {
            _logger.LogDebug("Available mapping: MidiNote={MidiNote}, VirtualKeyCode={VirtualKeyCode}, ActionType={ActionType}",
                m.MidiNote, m.VirtualKeyCode, m.ActionType);
        }

        var mapping = deviceConfig.Mappings.FirstOrDefault(m => m.MidiNote == noteNumber);
        if (mapping == null)
        {
            _logger.LogWarning("No mapping found for device {DeviceId}, note {NoteNumber}", deviceId, noteNumber);
            return;
        }

        _logger.LogInformation("Found mapping for DeviceId={DeviceId}, Note={NoteNumber}, VirtualKeyCode={VirtualKeyCode}, ActionType={ActionType}",
            deviceId, noteNumber, mapping.VirtualKeyCode, mapping.ActionType);

        // Handle Note On/Off events
        if (midiEvent.EventType == MidiEventType.NoteOn)
        {
            _logger.LogInformation("Device {DeviceId}: Mapping: Note {NoteNumber} press → Executing keyboard action", deviceId, noteNumber);

            // Check if this is a toggle mapping
            if (mapping.ActionType == KeyActionType.Toggle)
            {
                // For toggle mappings, we only need to handle note on events
                _logger.LogDebug("Toggle mapping detected for note {NoteNumber}", noteNumber);

                // Create a toggle key handler if one doesn't exist
                if (deviceHandlers == null || !deviceHandlers.TryGetValue(noteNumber, out _))
                {
                    var parameters = new Dictionary<string, object>
                    {
                        ["keyStateManager"] = _keyStateManager,
                        ["virtualKeyCode"] = mapping.VirtualKeyCode,
                        ["modifiers"] = mapping.Modifiers
                    };

                    var toggleHandler = _handlerFactory.CreateNoteHandler("ToggleKey", parameters);

                    if (toggleHandler != null)
                    {
                        _deviceConfigManager.RegisterNoteHandler(deviceId, noteNumber, toggleHandler);
                        toggleHandler.HandleNoteOn(midiEvent.Velocity ?? 127);
                    }
                    else
                    {
                        _logger.LogError("Failed to create toggle key handler for note {NoteNumber}", noteNumber);
                    }
                }
                else if (deviceHandlers.TryGetValue(noteNumber, out var noteHandler) && noteHandler.NoteHandler != null)
                {
                    // Handler already exists, just forward the event
                    noteHandler.NoteHandler.HandleNoteOn(midiEvent.Velocity ?? 127);
                }
            }
            // Check if this is a command execution mapping
            else if (mapping.ActionType == KeyActionType.CommandExecution)
            {
                // For command execution mappings, we only need to handle note on events
                _logger.LogDebug("Command execution mapping detected for note {NoteNumber}", noteNumber);

                // Create a command execution handler if one doesn't exist
                if (deviceHandlers == null || !deviceHandlers.TryGetValue(noteNumber, out _))
                {
                    var parameters = new Dictionary<string, object>
                    {
                        ["command"] = mapping.Command ?? string.Empty,
                        ["shellType"] = mapping.ShellType,
                        ["runHidden"] = mapping.RunHidden,
                        ["waitForExit"] = mapping.WaitForExit,
                        ["description"] = mapping.Description ?? string.Empty
                    };

                    var commandHandler = _handlerFactory.CreateNoteHandler("CommandExecution", parameters);

                    if (commandHandler != null)
                    {
                        _deviceConfigManager.RegisterNoteHandler(deviceId, noteNumber, commandHandler);
                        commandHandler.HandleNoteOn(midiEvent.Velocity ?? 127);
                    }
                    else
                    {
                        _logger.LogError("Failed to create command execution handler for note {NoteNumber}", noteNumber);
                    }
                }
                else if (deviceHandlers.TryGetValue(noteNumber, out var noteHandler) && noteHandler.NoteHandler != null)
                {
                    // Handler already exists, just forward the event
                    noteHandler.NoteHandler.HandleNoteOn(midiEvent.Velocity ?? 127);
                }
            }
            else
            {
                // Standard format
                _logger.LogInformation("Executing key down for VirtualKeyCode={VirtualKeyCode}, Modifiers={Modifiers}",
                    mapping.VirtualKeyCode, string.Join(",", mapping.Modifiers));
                _keyboardActionExecutor.ExecuteKeyDown(mapping.VirtualKeyCode, mapping.Modifiers);

                // If this is a Note-On Only mapping with auto-release, schedule the key release
                if (mapping.IgnoreNoteOff && mapping.AutoReleaseAfterMs.HasValue && mapping.AutoReleaseAfterMs.Value > 0)
                {
                    _logger.LogDebug("Scheduling auto-release for key {VirtualKeyCode} after {Delay}ms",
                        mapping.VirtualKeyCode, mapping.AutoReleaseAfterMs.Value);

                    // Use a timer to release the key after the specified delay
                    var timer = new System.Threading.Timer(
                        _ =>
                        {
                            _logger.LogDebug("Auto-releasing key {VirtualKeyCode}", mapping.VirtualKeyCode);
                            _keyboardActionExecutor.ExecuteKeyUp(mapping.VirtualKeyCode, mapping.Modifiers);
                        },
                        null,
                        mapping.AutoReleaseAfterMs.Value,
                        Timeout.Infinite);
                }
            }
        }
        else if (midiEvent.EventType == MidiEventType.NoteOff)
        {
            _logger.LogInformation("Device {DeviceId}: Mapping: Note {NoteNumber} release → Executing keyboard action", deviceId, noteNumber);

            // For toggle mappings, command execution mappings, and note-on only mappings, we ignore note off events
            if (mapping.ActionType == KeyActionType.Toggle ||
                mapping.ActionType == KeyActionType.CommandExecution ||
                mapping.IgnoreNoteOff)
            {
                _logger.LogDebug("Ignoring note off event for {ActionType} mapping {NoteNumber} (IgnoreNoteOff: {IgnoreNoteOff})",
                    mapping.ActionType, noteNumber, mapping.IgnoreNoteOff);

                // Forward to handler if it exists
                if (deviceHandlers != null &&
                    deviceHandlers.TryGetValue(noteNumber, out var noteHandler) &&
                    noteHandler.NoteHandler != null)
                {
                    noteHandler.NoteHandler.HandleNoteOff();
                }

                return;
            }

            // Standard format
            _keyboardActionExecutor.ExecuteKeyUp(mapping.VirtualKeyCode, mapping.Modifiers);
        }
    }

    /// <summary>
    /// Handles a Control Change event
    /// </summary>
    /// <param name="deviceId">The MIDI device ID</param>
    /// <param name="midiEvent">The MIDI event to handle</param>
    /// <param name="deviceConfig">The device configuration</param>
    public void HandleControlChangeEvent(int deviceId, MidiEvent midiEvent, MidiDeviceConfiguration deviceConfig)
    {
        if (midiEvent.Controller == null || midiEvent.Value == null)
        {
            _logger.LogWarning("Control Change event missing controller or value");
            return;
        }

        int controlNumber = midiEvent.Controller.Value;
        int value = midiEvent.Value.Value;

        _logger.LogInformation("Device {DeviceId}: Control Change: Controller={ControlNumber}, Value={Value} {RelativeInfo}",
            deviceId, controlNumber, value, midiEvent.IsRelative ? "(Relative)" : "");

        // Check if we have a handler for this device and control
        var deviceHandlers = _deviceConfigManager.GetDeviceHandlers(deviceId);
        if (deviceHandlers == null || !deviceHandlers.TryGetValue(controlNumber, out var handler))
        {
            _logger.LogDebug("No handler registered for device {DeviceId}, controller {ControlNumber}", deviceId, controlNumber);
            return;
        }

        // Handle based on handler type and whether the control is relative
        if (midiEvent.IsRelative && handler.HandlerType == MidiHandlerType.Relative && handler.RelativeHandler != null)
        {
            // Handle relative controls
            var encoding = midiEvent.RelativeEncoding ?? RelativeValueEncoding.SignMagnitude;
            int increment = ConvertToRelativeIncrement(value, encoding);
            _logger.LogDebug("Relative increment: {Increment}", increment);
            handler.RelativeHandler.HandleIncrement(increment);
        }
        else if (handler.HandlerType == MidiHandlerType.Absolute && handler.AbsoluteHandler != null)
        {
            // Handle absolute controls
            _logger.LogDebug("Absolute value: {Value}", value);
            handler.AbsoluteHandler.HandleValue(value);
        }
        else
        {
            _logger.LogDebug("Handler type mismatch for device {DeviceId}, controller {ControlNumber}", deviceId, controlNumber);
        }
    }

    /// <summary>
    /// Converts a MIDI value to a relative increment based on the encoding method
    /// </summary>
    /// <param name="value">The MIDI value (0-127)</param>
    /// <param name="encoding">The encoding method</param>
    /// <returns>The relative increment (positive or negative)</returns>
    private int ConvertToRelativeIncrement(int value, RelativeValueEncoding encoding)
    {
        return encoding switch
        {
            RelativeValueEncoding.SignMagnitude => value <= 63 ? value : value - 128,
            RelativeValueEncoding.TwosComplement => value <= 64 ? value : value - 128,
            RelativeValueEncoding.BinaryOffset => value - 64,
            _ => 0
        };
    }
}
