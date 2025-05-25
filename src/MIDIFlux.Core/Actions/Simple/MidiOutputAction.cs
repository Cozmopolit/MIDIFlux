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
public class MidiOutputAction : IAction
{
    private readonly MidiOutputConfig _config;
    private readonly MidiManager _midiManager;

    private readonly ILogger _logger;
    private int? _resolvedDeviceId;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Initializes a new instance of MidiOutputAction
    /// </summary>
    /// <param name="config">The MIDI output configuration</param>
    /// <param name="midiManager">The MIDI manager for device operations</param>
    /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
    /// <exception cref="ArgumentException">Thrown when configuration is invalid</exception>
    public MidiOutputAction(MidiOutputConfig config, MidiManager midiManager)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _midiManager = midiManager ?? throw new ArgumentNullException(nameof(midiManager));

        // Validate configuration
        if (!_config.IsValid())
        {
            throw new ArgumentException("Invalid MIDI output configuration", nameof(config));
        }

        // Validate device name (no wildcards allowed)
        if (string.IsNullOrWhiteSpace(_config.OutputDeviceName) || _config.OutputDeviceName == "*")
        {
            throw new ArgumentException("Output device name must be specified and cannot be a wildcard", nameof(config));
        }

        // Validate commands
        if (_config.Commands == null || _config.Commands.Count == 0)
        {
            throw new ArgumentException("At least one MIDI command must be specified", nameof(config));
        }

        // Validate all commands
        foreach (var command in _config.Commands)
        {
            if (!command.IsValid())
            {
                var errors = command.GetValidationErrors();
                throw new ArgumentException($"Invalid MIDI command: {string.Join(", ", errors)}", nameof(config));
            }
        }

        Id = Guid.NewGuid().ToString();
        Description = _config.Description ?? $"MIDI Output to '{_config.OutputDeviceName}' ({_config.Commands.Count} commands)";

        // Initialize logger
        _logger = LoggingHelper.CreateLogger<MidiOutputAction>();

        _logger.LogDebug("Created MidiOutputAction: {Description}", Description);
    }

    /// <summary>
    /// Executes the MIDI output action synchronously.
    /// Sends all configured MIDI commands to the specified output device immediately.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing MidiOutputAction: {Description}, MidiValue={MidiValue}", Description, midiValue);

            // Resolve device ID if not already resolved
            if (!_resolvedDeviceId.HasValue)
            {
                _resolvedDeviceId = ResolveOutputDeviceId();
                if (!_resolvedDeviceId.HasValue)
                {
                    var errorMsg = $"Cannot find MIDI output device '{_config.OutputDeviceName}'";
                    _logger.LogError(errorMsg);
                    ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", _logger);
                    return;
                }
            }

            // Ensure the output device is started
            if (!_midiManager.StartOutputDevice(_resolvedDeviceId.Value))
            {
                var errorMsg = $"Failed to start MIDI output device '{_config.OutputDeviceName}' (ID: {_resolvedDeviceId.Value})";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", _logger);
                return;
            }

            // Execute all commands in sequence
            int commandIndex = 0;
            foreach (var command in _config.Commands)
            {
                commandIndex++;
                try
                {
                    ExecuteCommand(command, commandIndex, midiValue);
                }
                catch (Exception ex)
                {
                    var errorMsg = $"Error executing MIDI command {commandIndex}/{_config.Commands.Count}: {ex.Message}";
                    _logger.LogError(ex, errorMsg);
                    ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - MIDI Output Error", _logger, ex);
                    // Continue with remaining commands
                }
            }

            _logger.LogTrace("Successfully executed MidiOutputAction: {Description}", Description);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing MIDI output action: {ex.Message}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Async wrapper for Execute method to satisfy IAction interface
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>Completed ValueTask</returns>
    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Resolves the output device name to a device ID
    /// </summary>
    /// <returns>The device ID if found, null otherwise</returns>
    private int? ResolveOutputDeviceId()
    {
        try
        {
            _logger.LogDebug("Resolving output device name '{DeviceName}' to device ID", _config.OutputDeviceName);

            // Get available output devices
            var outputDevices = _midiManager.GetAvailableOutputDevices();
            if (outputDevices == null || outputDevices.Count == 0)
            {
                _logger.LogWarning("No MIDI output devices available");
                return null;
            }

            // Find device by name using the helper
            var device = MidiDeviceHelper.FindDeviceByName(outputDevices, _config.OutputDeviceName, _logger);
            if (device == null)
            {
                _logger.LogError("MIDI output device '{DeviceName}' not found. Available devices: {AvailableDevices}",
                    _config.OutputDeviceName, string.Join(", ", outputDevices.Select(d => d.Name)));
                return null;
            }

            _logger.LogDebug("Resolved output device '{DeviceName}' to ID {DeviceId}", _config.OutputDeviceName, device.DeviceId);
            return device.DeviceId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving output device name '{DeviceName}' to device ID", _config.OutputDeviceName);
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
        _logger.LogDebug("Executing MIDI command {CommandIndex}: {Command}", commandIndex, command);

        // Use the hardware abstraction layer to send the command
        bool success = _midiManager.SendMidiMessage(_resolvedDeviceId!.Value, command);
        if (!success)
        {
            throw new InvalidOperationException($"Failed to send MIDI command {commandIndex}: {command}");
        }

        _logger.LogTrace("Sent MIDI command {CommandIndex}: {Command}", commandIndex, command);
    }



    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
