using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;

namespace MIDIFlux.App.Services;

/// <summary>
/// Handles MIDI device connection and disconnection events
/// </summary>
public class DeviceConnectionHandler
{
    private readonly ILogger _logger;
    private readonly MidiManager _midiManager;
    private readonly ConfigurationManager _configManager;
    private readonly List<int> _selectedDeviceIds = new();
    private bool _isRunning = false;

    /// <summary>
    /// Creates a new instance of the DeviceConnectionHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="midiManager">The MIDI manager</param>
    /// <param name="configManager">The configuration manager</param>
    public DeviceConnectionHandler(
        ILogger logger,
        MidiManager midiManager,
        ConfigurationManager configManager)
    {
        _logger = logger;
        _midiManager = midiManager;
        _configManager = configManager;

        // Subscribe to device connection/disconnection events
        _midiManager.DeviceConnected += MidiManager_DeviceConnected;
        _midiManager.DeviceDisconnected += MidiManager_DeviceDisconnected;
    }

    /// <summary>
    /// Gets whether the MIDI processing is running
    /// </summary>
    public bool IsRunning => _isRunning;

    /// <summary>
    /// Sets the running state
    /// </summary>
    /// <param name="isRunning">The new running state</param>
    public void SetRunningState(bool isRunning)
    {
        _isRunning = isRunning;
    }

    /// <summary>
    /// Gets the list of selected device IDs
    /// </summary>
    public IReadOnlyList<int> SelectedDeviceIds => _selectedDeviceIds;

    /// <summary>
    /// Clears the list of selected device IDs
    /// </summary>
    public void ClearSelectedDeviceIds()
    {
        _selectedDeviceIds.Clear();
    }

    /// <summary>
    /// Adds a device ID to the list of selected device IDs
    /// </summary>
    /// <param name="deviceId">The device ID to add</param>
    public void AddSelectedDeviceId(int deviceId)
    {
        if (!_selectedDeviceIds.Contains(deviceId))
        {
            _selectedDeviceIds.Add(deviceId);
        }
    }

    /// <summary>
    /// Handles a MIDI device connection event
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="deviceInfo">The connected device information</param>
    public void MidiManager_DeviceConnected(object? sender, MidiDeviceInfo deviceInfo)
    {
        try
        {
            _logger.LogInformation("MIDI device connected: {Device}", deviceInfo);

            // Check if this device should be automatically started based on configuration
            if (_isRunning)
            {
                var config = _configManager.GetActiveConfiguration();
                if (config != null)
                {
                    bool shouldStart = false;

                    // Check if this device matches any configured device
                    if (config.MidiDevices.Count > 0)
                    {
                        foreach (var deviceConfig in config.MidiDevices)
                        {
                            if (string.IsNullOrEmpty(deviceConfig.DeviceName))
                                continue;

                            // Use the same logic as in FindDeviceByName
                            if (deviceInfo.Name.Equals(deviceConfig.DeviceName, StringComparison.OrdinalIgnoreCase) ||
                                deviceInfo.Name.Contains(deviceConfig.DeviceName, StringComparison.OrdinalIgnoreCase))
                            {
                                shouldStart = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        _logger.LogDebug("No device configurations found in unified configuration for auto-connection");
                    }

                    if (shouldStart && !_selectedDeviceIds.Contains(deviceInfo.DeviceId))
                    {
                        _logger.LogInformation("Auto-starting newly connected device: {Device}", deviceInfo);

                        if (_midiManager.StartListening(deviceInfo.DeviceId))
                        {
                            _selectedDeviceIds.Add(deviceInfo.DeviceId);
                            _logger.LogInformation("Successfully started listening to reconnected device: {Device}", deviceInfo);
                        }
                        else
                        {
                            _logger.LogError("Failed to start listening to reconnected device: {Device}", deviceInfo);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device connection: {ErrorMessage}", ex.Message);
        }
    }

    /// <summary>
    /// Handles a MIDI device disconnection event
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="deviceInfo">The disconnected device information</param>
    public void MidiManager_DeviceDisconnected(object? sender, MidiDeviceInfo deviceInfo)
    {
        try
        {
            _logger.LogWarning("MIDI device disconnected: {Device}", deviceInfo);

            // If this was one of our selected devices, update our state
            if (_selectedDeviceIds.Contains(deviceInfo.DeviceId))
            {
                _logger.LogInformation("Disconnected device was in our active device list: {Device}", deviceInfo);

                // We don't remove it from _selectedDeviceIds because we want to reconnect when it comes back
                // The MidiManager will handle the actual cleanup and reconnection

                // Check if we have any devices left
                var activeDevices = _midiManager.ActiveDeviceIds;
                if (activeDevices.Count == 0 && _isRunning)
                {
                    _logger.LogWarning("All MIDI devices disconnected, but MIDIFlux will continue running and wait for reconnection");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling device disconnection: {ErrorMessage}", ex.Message);
        }
    }
}
