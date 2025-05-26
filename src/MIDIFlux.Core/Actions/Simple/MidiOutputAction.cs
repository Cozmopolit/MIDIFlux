using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for sending MIDI output messages to specified devices.
/// Executes a sequence of MIDI commands immediately without delays.
/// For complex timing scenarios, use SequenceAction with DelayAction.
/// </summary>
public class MidiOutputAction : ActionBase<MidiOutputConfig>
{
    private readonly MidiManager _midiManager;
    private int? _resolvedDeviceId;

    /// <summary>
    /// Initializes a new instance of MidiOutputAction
    /// </summary>
    /// <param name="config">The MIDI output configuration</param>
    /// <param name="midiManager">The MIDI manager for device operations</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public MidiOutputAction(MidiOutputConfig config, MidiManager midiManager) : base(config)
    {
        _midiManager = midiManager ?? throw new ArgumentNullException(nameof(midiManager));

        // Validate device name (no wildcards allowed)
        if (string.IsNullOrWhiteSpace(config.OutputDeviceName) || config.OutputDeviceName == "*")
        {
            throw new ArgumentException("Output device name must be specified and cannot be a wildcard", nameof(config));
        }

        // Validate commands
        if (config.Commands == null || config.Commands.Count == 0)
        {
            throw new ArgumentException("At least one MIDI command must be specified", nameof(config));
        }

        // Validate all commands
        foreach (var command in config.Commands)
        {
            if (!command.IsValid())
            {
                var errors = command.GetValidationErrors();
                throw new ArgumentException($"Invalid MIDI command: {string.Join(", ", errors)}", nameof(config));
            }
        }

        Logger.LogDebug("Created MidiOutputAction: {Description}", Description);
    }

    /// <summary>
    /// Core execution logic for the MIDI output action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
            // Resolve device ID if not already resolved
            if (!_resolvedDeviceId.HasValue)
            {
                _resolvedDeviceId = ResolveOutputDeviceId();
                if (!_resolvedDeviceId.HasValue)
                {
                    var errorMsg = $"Cannot find MIDI output device '{Config.OutputDeviceName}'";
                    Logger.LogError(errorMsg);
                    ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                    return ValueTask.CompletedTask;
                }
            }

            // Ensure the output device is started
            if (!_midiManager.StartOutputDevice(_resolvedDeviceId.Value))
            {
                var errorMsg = $"Failed to start MIDI output device '{Config.OutputDeviceName}' (ID: {_resolvedDeviceId.Value})";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger);
                return ValueTask.CompletedTask;
            }

            // Execute all commands in sequence
            int commandIndex = 0;
            foreach (var command in Config.Commands)
            {
                commandIndex++;
                try
                {
                    ExecuteCommand(command, commandIndex, midiValue);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error executing MIDI command {commandIndex}/{Config.Commands.Count}: {ex.Message}";
                    Logger.LogError(ex, errorMsg);
                    ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", Logger, ex);
                    // Continue with remaining commands
                }
            }

            Logger.LogTrace("Successfully executed MidiOutputAction: {Description}", Description);

            return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type.
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        return $"MIDI Output to '{Config.OutputDeviceName}' ({Config.Commands.Count} commands)";
    }

    /// <summary>
    /// Gets the error message for this action type.
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        return "Error executing MIDI output action";
    }

    /// <summary>
    /// Resolves the output device name to a device ID
    /// </summary>
    /// <returns>The device ID if found, null otherwise</returns>
    private int? ResolveOutputDeviceId()
    {
        try
        {
            Logger.LogDebug("Resolving output device name '{DeviceName}' to device ID", Config.OutputDeviceName);

            // Get available output devices
            var outputDevices = _midiManager.GetAvailableOutputDevices();
            if (outputDevices == null || outputDevices.Count == 0)
            {
                Logger.LogWarning("No MIDI output devices available");
                return null;
            }

            // Find device by name using the helper
            var device = MidiDeviceHelper.FindDeviceByName(outputDevices, Config.OutputDeviceName, Logger);
            if (device == null)
            {
                Logger.LogError("MIDI output device '{DeviceName}' not found. Available devices: {AvailableDevices}",
                    Config.OutputDeviceName, string.Join(", ", outputDevices.Select(d => d.Name)));
                return null;
            }

            Logger.LogDebug("Resolved output device '{DeviceName}' to ID {DeviceId}", Config.OutputDeviceName, device.DeviceId);
            return device.DeviceId;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving output device name '{DeviceName}' to device ID", Config.OutputDeviceName);
            return null;
        }
    }

    /// <summary>
    /// Executes a single MIDI command
    /// </summary>
    /// <param name="command">The MIDI command to execute</param>
    /// <param name="commandIndex">The index of the command for logging</param>
    /// <param name="midiValue">Optional MIDI value that triggered this action</param>
    private void ExecuteCommand(MidiOutputCommand command, int commandIndex, int? midiValue)
    {
        Logger.LogDebug("Executing MIDI command {CommandIndex}: {Command}", commandIndex, command);

        // Use the hardware abstraction layer to send the command
        bool success = _midiManager.SendMidiMessage(_resolvedDeviceId!.Value, command);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to send MIDI command {commandIndex}: {command}");
        }

        Logger.LogTrace("Sent MIDI command {CommandIndex}: {Command}", commandIndex, command);
    }
}
