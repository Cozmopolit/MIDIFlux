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

    private MidiManager? _midiManager;
    private int? _resolvedDeviceId;

    /// <summary>
    /// Gets the output device name for this action
    /// </summary>
    [JsonIgnore]
    public string OutputDeviceName => GetParameterValue<string>(OutputDeviceNameParam);

    /// <summary>
    /// Gets the MIDI channel for this action
    /// </summary>
    [JsonIgnore]
    public int Channel => GetParameterValue<int>(ChannelParam);

    /// <summary>
    /// Gets the controller number for this action
    /// </summary>
    [JsonIgnore]
    public int Controller => GetParameterValue<int>(ControllerParam);

    /// <summary>
    /// Gets the controller value for this action
    /// </summary>
    [JsonIgnore]
    public int Value => GetParameterValue<int>(ValueParam);

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
    /// <param name="midiManager">The MIDI manager service</param>
    public void SetMidiManager(MidiManager midiManager)
    {
        _midiManager = midiManager;
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add OutputDeviceName parameter with string type
        Parameters[OutputDeviceNameParam] = new Parameter(
            ParameterType.String,
            "Default Device", // Default device name
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
            1, // Default to channel 1
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
            7, // Default to volume controller
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
            127, // Default to maximum value
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
        if (_midiManager == null)
        {
            var errorMsg = "MIDI manager service is not available";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
            return ValueTask.CompletedTask;
        }

        var outputDeviceName = OutputDeviceName;
        var channel = Channel;
        var controller = Controller;
        var value = Value;

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
        if (!_midiManager.StartOutputDevice(_resolvedDeviceId.Value))
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
            bool success = _midiManager.SendMidiMessage(_resolvedDeviceId.Value, command);
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
        return $"MIDI Control Change to '{OutputDeviceName}' Ch{Channel} CC{Controller} Val{Value}";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return $"Error executing MIDI Control Change action to '{OutputDeviceName}'";
    }

    /// <summary>
    /// Resolves the output device name to a device ID
    /// </summary>
    /// <returns>The device ID if found, null otherwise</returns>
    private int? ResolveOutputDeviceId()
    {
        try
        {
            var outputDeviceName = OutputDeviceName;
            Logger.LogDebug("Resolving output device name '{DeviceName}' to device ID", outputDeviceName);

            // Get available output devices
            var outputDevices = _midiManager!.GetAvailableOutputDevices();
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
            Logger.LogError(ex, "Error resolving output device name '{DeviceName}' to device ID", OutputDeviceName);
            return null;
        }
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// MidiControlChangeAction supports both trigger signals (for fixed values) and absolute value signals (for MIDI pass-through).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public static InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue };
    }
}
