using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using System.Collections.Concurrent;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Reusable component for monitoring MIDI events with flood control and filtering
    /// </summary>
    public class MidiEventMonitor : IDisposable
    {
        private readonly ILogger _logger;
        private readonly MidiDeviceManager _MidiDeviceManager;
        private readonly ConcurrentQueue<MidiEventArgs> _recentEvents = new();
        private readonly int _maxEvents;
        private string? _selectedDeviceId = null;
        private bool _isListening = false;
        private bool _listenToAllDevices = false;
        private bool _disposed = false;

        /// <summary>
        /// Event raised when a MIDI event is received and should be displayed
        /// </summary>
        public event EventHandler<MidiEventArgs>? MidiEventReceived;

        /// <summary>
        /// Gets whether the monitor is currently listening
        /// </summary>
        public bool IsListening => _isListening;

        /// <summary>
        /// Gets whether the monitor is listening to all devices
        /// </summary>
        public bool ListenToAllDevices => _listenToAllDevices;

        /// <summary>
        /// Gets the currently selected device ID (null means no specific device or all devices)
        /// </summary>
        public string? SelectedDeviceId => _selectedDeviceId;

        /// <summary>
        /// Creates a new MIDI event monitor
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="MidiDeviceManager">The MIDI manager</param>
        /// <param name="maxEvents">Maximum number of events to keep in memory</param>
        public MidiEventMonitor(ILogger logger, MidiDeviceManager MidiDeviceManager, int maxEvents = 100)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _MidiDeviceManager = MidiDeviceManager ?? throw new ArgumentNullException(nameof(MidiDeviceManager));
            _maxEvents = maxEvents;
        }

        /// <summary>
        /// Starts listening for MIDI events from the specified device
        /// </summary>
        /// <param name="deviceId">The device ID to listen to, or null for all devices</param>
        /// <param name="listenToAllDevices">Whether to listen to all devices</param>
        /// <returns>True if listening started successfully</returns>
        public bool StartListening(string? deviceId = null, bool listenToAllDevices = false)
        {
            try
            {
                if (_isListening)
                {
                    StopListening();
                }

                _selectedDeviceId = deviceId;
                _listenToAllDevices = listenToAllDevices;

                // Clear recent events
                while (_recentEvents.TryDequeue(out _)) { }

                // Subscribe to MIDI events
                _MidiDeviceManager.MidiEventReceived += MidiDeviceManager_MidiEventReceived;

                if (!listenToAllDevices && deviceId != null)
                {
                    // Start listening to specific device
                    bool success = _MidiDeviceManager.StartListening(deviceId);
                    if (!success)
                    {
                        _logger.LogWarning("Failed to start listening to MIDI device: {DeviceId}", deviceId);
                        _MidiDeviceManager.MidiEventReceived -= MidiDeviceManager_MidiEventReceived;
                        return false;
                    }
                }

                _isListening = true;
                _logger.LogInformation("Started MIDI event monitoring for device: {DeviceId} (AllDevices: {AllDevices})",
                    deviceId ?? "(all)", listenToAllDevices);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MIDI event monitoring");
                return false;
            }
        }

        /// <summary>
        /// Stops listening for MIDI events
        /// </summary>
        public void StopListening()
        {
            try
            {
                if (!_isListening)
                    return;

                // Unsubscribe from MIDI events
                _MidiDeviceManager.MidiEventReceived -= MidiDeviceManager_MidiEventReceived;

                if (!_listenToAllDevices && _selectedDeviceId != null)
                {
                    // Stop listening to specific device
                    _MidiDeviceManager.StopListening(_selectedDeviceId);
                }

                _isListening = false;
                _logger.LogInformation("Stopped MIDI event monitoring");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MIDI event monitoring");
            }
        }

        /// <summary>
        /// Gets the recent events (thread-safe copy)
        /// </summary>
        /// <returns>Array of recent MIDI events</returns>
        public MidiEventArgs[] GetRecentEvents()
        {
            return _recentEvents.ToArray();
        }

        /// <summary>
        /// Clears all recent events
        /// </summary>
        public void ClearEvents()
        {
            while (_recentEvents.TryDequeue(out _)) { }
        }

        /// <summary>
        /// Handles MIDI events from the MIDI manager
        /// </summary>
        private void MidiDeviceManager_MidiEventReceived(object? sender, MidiEventArgs e)
        {
            try
            {
                // Only process events if we're listening
                if (!_isListening)
                    return;

                // If we're not listening to all devices, check if the event is from the selected device
                if (!_listenToAllDevices && _selectedDeviceId != null && e.DeviceId != _selectedDeviceId)
                    return;

                // Add the event to the queue
                _recentEvents.Enqueue(e);

                // Trim the queue if it's too long
                while (_recentEvents.Count > _maxEvents)
                {
                    _recentEvents.TryDequeue(out _);
                }

                // Raise the event for subscribers
                MidiEventReceived?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MIDI event in monitor");
            }
        }



        /// <summary>
        /// Disposes the monitor
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                StopListening();
                _disposed = true;
            }
        }
    }
}
