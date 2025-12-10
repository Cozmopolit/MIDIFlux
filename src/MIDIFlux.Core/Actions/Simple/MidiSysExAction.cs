using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for sending MIDI System Exclusive messages to specified devices.
/// Specialized simple action for sending a single SysEx message.
/// </summary>
[ActionDisplayName("Send MIDI SysEx")]
[ActionCategory(ActionCategory.MidiOutput)]
public class MidiSysExAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string OutputDeviceNameParam = "OutputDeviceName";
    private const string SysExDataParam = "SysExData";

    private MidiDeviceManager? _MidiDeviceManager;
    private int? _resolvedDeviceId;



    /// <summary>
    /// Initializes a new instance of MidiSysExAction with default parameters
    /// </summary>
    public MidiSysExAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of MidiSysExAction with specified parameters
    /// </summary>
    /// <param name="outputDeviceName">The output device name</param>
    /// <param name="sysExData">The SysEx data bytes</param>
    public MidiSysExAction(string outputDeviceName, byte[] sysExData) : base()
    {
        SetParameterValue(OutputDeviceNameParam, outputDeviceName);
        SetParameterValue(SysExDataParam, sysExData);
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

        // Add SysExData parameter with byte array type
        // Default to empty array - user must specify device-specific SysEx data
        Parameters[SysExDataParam] = new Parameter(
            ParameterType.ByteArray,
            new byte[0], // Empty array - SysEx messages are device-specific
            "SysEx Data")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "minLength", 3 },    // Minimum: F0 XX F7
                { "maxLength", 1024 }  // Maximum 1KB for SysEx
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

        var sysExData = GetParameterValue<byte[]>(SysExDataParam);
        if (sysExData == null || sysExData.Length == 0)
        {
            AddValidationError("SysEx data cannot be empty");
        }
        else
        {
            // Validate SysEx format
            if (sysExData.Length < 3)
            {
                AddValidationError("SysEx data must be at least 3 bytes (F0 ... F7)");
            }
            else
            {
                if (sysExData[0] != 0xF0)
                {
                    AddValidationError("SysEx data must start with F0");
                }
                if (sysExData[sysExData.Length - 1] != 0xF7)
                {
                    AddValidationError("SysEx data must end with F7");
                }
            }

            if (sysExData.Length > 1024)
            {
                AddValidationError("SysEx data must not exceed 1024 bytes");
            }
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the MIDI SysEx action
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
        var sysExData = GetParameterValue<byte[]>(SysExDataParam);

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

        // Create and send the MIDI SysEx command
        var command = new MidiOutputCommand
        {
            MessageType = MidiMessageType.SysEx,
            Channel = 1, // Not used for SysEx but required by the structure
            Data1 = 0,   // Not used for SysEx
            Data2 = 0,   // Not used for SysEx
            SysExData = sysExData
        };

        try
        {
            bool success = _MidiDeviceManager.SendMidiMessage(_resolvedDeviceId.Value, command);
            if (!success)
            {
                var errorMsg = $"Failed to send MIDI SysEx: {sysExData.Length} bytes";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                return ValueTask.CompletedTask;
            }

            Logger.LogTrace("Successfully sent MIDI SysEx: {Length} bytes", sysExData.Length);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error sending MIDI SysEx: {ex.Message}";
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
        var sysExData = GetParameterValue<byte[]>(SysExDataParam);
        return $"MIDI SysEx to '{outputDeviceName}' ({sysExData?.Length ?? 0} bytes)";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var outputDeviceName = GetParameterValue<string>(OutputDeviceNameParam) ?? "";
        return $"Error executing MIDI SysEx action to '{outputDeviceName}'";
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
    /// MidiSysExAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
