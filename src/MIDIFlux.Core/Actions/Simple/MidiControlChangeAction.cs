using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for sending MIDI Control Change messages to specified devices.
/// Specialized simple action for sending a single Control Change message.
/// </summary>
public class MidiControlChangeAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string OutputDeviceNameParam = "OutputDeviceName";
    private const string ChannelParam = "Channel";
    private const string ControllerParam = "Controller";
    private const string ValueParam = "Value";

    private MidiDeviceManager? _MidiDeviceManager;
    private int? _resolvedDeviceId;



    /// <summary>
    /// Initializes a new instance of MidiControlChangeAction with default parameters
    /// </summary>
    public MidiControlChangeAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of MidiControlChangeAction with specified parameters
    /// </summary>
    /// <param name="outputDeviceName">The output device name</param>
    /// <param name="channel">The MIDI channel (1-16)</param>
    /// <param name="controller">The controller number (0-127)</param>
    /// <param name="value">The controller value (0-127)</param>
    public MidiControlChangeAction(string outputDeviceName, int channel, int controller, int value) : base()
    {
        SetParameterValue(OutputDeviceNameParam, outputDeviceName);
        SetParameterValue(ChannelParam, channel);
        SetParameterValue(ControllerParam, controller);
        SetParameterValue(ValueParam, value);
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

        // Add Controller parameter with integer type
        Parameters[ControllerParam] = new Parameter(
            ParameterType.Integer,
            null, // No default - user must specify controller
            "Controller Number")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 0 },
                { "max", 127 }
            }
        };

        // Add Value parameter with integer type
        Parameters[ValueParam] = new Parameter(
            ParameterType.Integer,
            null, // No default - user must specify value
            "Controller Value")
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

        var channel = GetParameterValue<int>(ChannelParam);
        if (channel < 1 || channel > 16)
        {
            AddValidationError("Channel must be between 1 and 16");
        }

        var controller = GetParameterValue<int>(ControllerParam);
        if (controller < 0 || controller > 127)
        {
            AddValidationError("Controller must be between 0 and 127");
        }

        var value = GetParameterValue<int>(ValueParam);
        if (value < 0 || value > 127)
        {
            AddValidationError("Value must be between 0 and 127");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the MIDI Control Change action
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
        var controller = GetParameterValue<int>(ControllerParam);
        var value = GetParameterValue<int>(ValueParam);

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

        // Create and send the MIDI Control Change command
        var command = new MidiOutputCommand
        {
            MessageType = MidiMessageType.ControlChange,
            Channel = channel,
            Data1 = controller,
            Data2 = value
        };

        try
        {
            bool success = _MidiDeviceManager.SendMidiMessage(_resolvedDeviceId.Value, command);
            if (!success)
            {
                var errorMsg = $"Failed to send MIDI Control Change: Ch{channel} CC{controller} Val{value}";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                return ValueTask.CompletedTask;
            }

            Logger.LogTrace("Successfully sent MIDI Control Change: Ch{Channel} CC{Controller} Val{Value}",
                channel, controller, value);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error sending MIDI Control Change: {ex.Message}";
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
        var controller = GetParameterValue<int>(ControllerParam);
        var value = GetParameterValue<int>(ValueParam);
        return $"MIDI Control Change to '{outputDeviceName}' Ch{channel} CC{controller} Val{value}";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam) ?? "";
        return $"Error executing MIDI Control Change action to '{outputDeviceName}'";
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
    /// MidiControlChangeAction supports both trigger signals (for fixed values) and absolute value signals (for MIDI pass-through).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue };
    }
}
