using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for sending MIDI Note Off messages to specified devices.
/// Specialized simple action for sending a single Note Off message.
/// </summary>
[ActionDisplayName("Send MIDI Note Off")]
[ActionCategory(ActionCategory.MidiOutput)]
public class MidiNoteOffAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string OutputDeviceNameParam = "OutputDeviceName";
    private const string ChannelParam = "Channel";
    private const string NoteParam = "Note";
    private const string VelocityParam = "Velocity";

    private MidiDeviceManager? _MidiDeviceManager;
    private int? _resolvedDeviceId;



    /// <summary>
    /// Initializes a new instance of MidiNoteOffAction with default parameters
    /// </summary>
    public MidiNoteOffAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of MidiNoteOffAction with specified parameters
    /// </summary>
    /// <param name="outputDeviceName">The output device name</param>
    /// <param name="channel">The MIDI channel (1-16)</param>
    /// <param name="note">The note number (0-127)</param>
    /// <param name="velocity">The velocity (0-127, usually 0)</param>
    public MidiNoteOffAction(string outputDeviceName, int channel, int note, int velocity = 0) : base()
    {
        SetParameterValue(OutputDeviceNameParam, outputDeviceName);
        SetParameterValue(ChannelParam, channel);
        SetParameterValue(NoteParam, note);
        SetParameterValue(VelocityParam, velocity);
    }

    /// <summary>
    /// Sets the MIDI manager service (called by service injection)
    /// </summary>
    /// <param name="MidiDeviceManager">The MIDI manager service</param>
    public void SetMidiDeviceManager(MidiDeviceManager MidiDeviceManager)
    {
        _MidiDeviceManager = MidiDeviceManager;
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add OutputDeviceName parameter with string type
        Parameters[OutputDeviceNameParam] = new Parameter(
            ParameterType.String,
            "", // No default - user must specify device
            "Output Device Name")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "maxLength", 100 }
            }
        };

        // Add Channel parameter with integer type
        Parameters[ChannelParam] = new Parameter(
            ParameterType.Integer,
            null, // No default - user must specify channel
            "MIDI Channel")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 1 },
                { "max", 16 }
            }
        };

        // Add Note parameter with integer type
        Parameters[NoteParam] = new Parameter(
            ParameterType.Integer,
            null, // No default - user must specify note
            "Note Number")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 127 }
            }
        };

        // Add Velocity parameter with integer type
        Parameters[VelocityParam] = new Parameter(
            ParameterType.Integer,
            0, // Note Off typically uses 0 velocity, but user can override
            "Velocity")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 127 }
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam);
        if (string.IsNullOrWhiteSpace(outputDeviceName) || outputDeviceName == "*")
        {
            AddValidationError("Output device name must be specified and cannot be a wildcard");
        }

        var channel = GetParameterValue<int?>(ChannelParam);
        if (!channel.HasValue || channel.Value < 1 || channel.Value > 16)
        {
            AddValidationError("Channel must be specified and between 1 and 16");
        }

        var note = GetParameterValue<int?>(NoteParam);
        if (!note.HasValue || note.Value < 0 || note.Value > 127)
        {
            AddValidationError("Note must be specified and between 0 and 127");
        }

        var velocity = GetParameterValue<int>(VelocityParam);
        if (velocity < 0 || velocity > 127)
        {
            AddValidationError("Velocity must be between 0 and 127");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the MIDI Note Off action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (_MidiDeviceManager == null)
        {
            var errorMsg = "MIDI manager service is not available";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
            return ValueTask.CompletedTask;
        }

        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam);
        var channel = GetParameterValue<int>(ChannelParam);
        var note = GetParameterValue<int>(NoteParam);
        var velocity = GetParameterValue<int>(VelocityParam);

        // Resolve device ID if not already resolved
        if (!_resolvedDeviceId.HasValue)
        {
            _resolvedDeviceId = ResolveOutputDeviceId();
            if (!_resolvedDeviceId.HasValue)
            {
                var errorMsg = $"Cannot find MIDI output device '{outputDeviceName}'";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                return ValueTask.CompletedTask;
            }
        }

        // Ensure the output device is started
        if (!_MidiDeviceManager.StartOutputDevice(_resolvedDeviceId.Value))
        {
            var errorMsg = $"Failed to start MIDI output device '{outputDeviceName}' (ID: {_resolvedDeviceId.Value})";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
            return ValueTask.CompletedTask;
        }

        // Create and send the MIDI Note Off command
        var command = new MidiOutputCommand
        {
            MessageType = MidiMessageType.NoteOff,
            Channel = channel,
            Data1 = note,
            Data2 = velocity
        };

        try
        {
            bool success = _MidiDeviceManager.SendMidiMessage(_resolvedDeviceId.Value, command);
            if (!success)
            {
                var errorMsg = $"Failed to send MIDI Note Off: Ch{channel} Note{note} Vel{velocity}";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                return ValueTask.CompletedTask;
            }

            Logger.LogTrace("Successfully sent MIDI Note Off: Ch{Channel} Note{Note} Vel{Velocity}",
                channel, note, velocity);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error sending MIDI Note Off: {ex.Message}";
            Logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger, ex);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam) ?? "";
        var channel = GetParameterValue<int>(ChannelParam);
        var note = GetParameterValue<int>(NoteParam);
        var velocity = GetParameterValue<int>(VelocityParam);
        return $"MIDI Note Off to '{outputDeviceName}' Ch{channel} Note{note} Vel{velocity}";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam) ?? "";
        return $"Error executing MIDI Note Off action to '{outputDeviceName}'";
    }

    /// <summary>
    /// Resolves the output device name to a device ID
    /// </summary>
    /// <returns>The device ID if found, null otherwise</returns>
    private int? ResolveOutputDeviceId()
    {
        try
        {
            var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam);
            Logger.LogDebug("Resolving output device name '{DeviceName}' to device ID", outputDeviceName);

            // Get available output devices
            var outputDevices = _MidiDeviceManager!.GetAvailableOutputDevices();
            if (outputDevices == null || outputDevices.Count == 0)
            {
                Logger.LogWarning("No MIDI output devices available");
                return null;
            }

            // Find device by name using the helper
            var device = MidiDeviceHelper.FindDeviceByName(outputDevices, outputDeviceName, Logger);
            if (device == null)
            {
                Logger.LogError("MIDI output device '{DeviceName}' not found. Available devices: {AvailableDevices}",
                    outputDeviceName, string.Join(", ", outputDevices.Select(d => d.Name)));
                return null;
            }

            Logger.LogDebug("Resolved output device '{DeviceName}' to ID {DeviceId}", outputDeviceName, device.DeviceId);
            return device.DeviceId;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving output device name '{DeviceName}' to device ID", GetParameterValue<string>(OutputDeviceNameParam));
            return null;
        }
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// MidiNoteOffAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
