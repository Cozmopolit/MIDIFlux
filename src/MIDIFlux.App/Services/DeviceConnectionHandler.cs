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
    private readonly ILogger<DeviceConnectionHandler> _logger;
    private readonly MidiDeviceManager _MidiDeviceManager;
    private readonly ConfigurationManager _configManager;
    private readonly List<string> _selectedDeviceIds = new();
    private readonly object _deviceIdsLock = new();
    private bool _isRunning = false;

    /// <summary>
    /// Creates a new instance of the DeviceConnectionHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="MidiDeviceManager">The MIDI manager</param>
    /// <param name="configManager">The configuration manager</param>
    public DeviceConnectionHandler(
        ILogger<DeviceConnectionHandler> logger,
        MidiDeviceManager MidiDeviceManager,
        ConfigurationManager configManager)
    {
        _logger = logger;
        _MidiDeviceManager = MidiDeviceManager;
        _configManager = configManager;

        // Subscribe to device connection/disconnection events
        _MidiDeviceManager.DeviceConnected += MidiDeviceManager_DeviceConnected;
        _MidiDeviceManager.DeviceDisconnected += MidiDeviceManager_DeviceDisconnected;
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
    /// Gets a snapshot of the selected device IDs (thread-safe)
    /// </summary>
    public IReadOnlyList<string> SelectedDeviceIds
    {
        get
        {
            lock (_deviceIdsLock)
            {
                return _selectedDeviceIds.ToList();
            }
        }
    }

    /// <summary>
    /// Clears the list of selected device IDs (thread-safe)
    /// </summary>
    public void ClearSelectedDeviceIds()
    {
        lock (_deviceIdsLock)
        {
            _selectedDeviceIds.Clear();
        }
    }

    /// <summary>
    /// Adds a device ID to the list of selected device IDs (thread-safe)
    /// </summary>
    /// <param name="deviceId">The device ID to add (format depends on adapter implementation)</param>
    public void AddSelectedDeviceId(string deviceId)
    {
        lock (_deviceIdsLock)
        {
            if (!_selectedDeviceIds.Contains(deviceId))
            {
                _selectedDeviceIds.Add(deviceId);
            }
        }
    }

    /// <summary>
    /// Handles a MIDI device connection event
    /// </summary>
    /// <param name="sender">The event sender</param>
    /// <param name="deviceInfo">The connected device information</param>
    public void MidiDeviceManager_DeviceConnected(object? sender, MidiDeviceInfo deviceInfo)
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

                    if (shouldStart)
                    {
                        bool alreadySelected;
                        lock (_deviceIdsLock)
                        {
                            alreadySelected = _selectedDeviceIds.Contains(deviceInfo.DeviceId);
                        }

                        if (!alreadySelected)
                        {
                            _logger.LogInformation("Auto-starting newly connected device: {Device}", deviceInfo);

                            if (_MidiDeviceManager.StartListening(deviceInfo.DeviceId))
                            {
                                lock (_deviceIdsLock)
                                {
                                    _selectedDeviceIds.Add(deviceInfo.DeviceId);
                                }
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
    public void MidiDeviceManager_DeviceDisconnected(object? sender, MidiDeviceInfo deviceInfo)
    {
        try
        {
            _logger.LogWarning("MIDI device disconnected: {Device}", deviceInfo);

            // If this was one of our selected devices, update our state
            bool wasSelected;
            lock (_deviceIdsLock)
            {
                wasSelected = _selectedDeviceIds.Contains(deviceInfo.DeviceId);
            }

            if (wasSelected)
            {
                _logger.LogInformation("Disconnected device was in our active device list: {Device}", deviceInfo);

                // We don't remove it from _selectedDeviceIds because we want to reconnect when it comes back
                // The MidiDeviceManager will handle the actual cleanup and reconnection

                // Check if we have any devices left
                var activeDevices = _MidiDeviceManager.ActiveDeviceIds;
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
