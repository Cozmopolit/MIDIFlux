using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Midi;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Models;

namespace MIDIFlux.GUI.Services
{
    /// <summary>
    /// Proxy service for communicating with the MIDIFlux main application
    /// </summary>
    public class MidiProcessingServiceProxy
    {
        private readonly ILogger<MidiProcessingServiceProxy> _logger;
        // Legacy ConfigLoader removed - using unified configuration system

        // Use delegates instead of direct references
        private Func<string, bool>? _loadConfigurationFunc;
        private Func<string?>? _getActiveConfigPathFunc;
        private Func<bool>? _startProcessingFunc;
        private Action? _stopProcessingFunc;
        private Func<List<MidiDeviceInfo>>? _getAvailableMidiDevicesFunc;
        private Func<MidiManager?>? _getMidiManagerFunc;



        /// <summary>
        /// Initializes a new instance of the <see cref="MidiProcessingServiceProxy"/> class
        /// </summary>
        /// <param name="logger">The logger</param>
        public MidiProcessingServiceProxy(ILogger<MidiProcessingServiceProxy> logger)
        {
            _logger = logger;

            // Legacy ConfigLoader initialization removed
        }

        /// <summary>
        /// Sets the functions to interact with the MIDI processing service
        /// </summary>
        /// <param name="loadConfigurationFunc">Function to load a configuration</param>
        /// <param name="getActiveConfigPathFunc">Function to get the active configuration path</param>
        /// <param name="startProcessingFunc">Function to start processing</param>
        /// <param name="stopProcessingFunc">Function to stop processing</param>
        /// <param name="getAvailableMidiDevicesFunc">Function to get available MIDI devices</param>
        /// <param name="getMidiManagerFunc">Function to get the MIDI manager</param>
        public void SetServiceFunctions(
            Func<string, bool> loadConfigurationFunc,
            Func<string?> getActiveConfigPathFunc,
            Func<bool> startProcessingFunc,
            Action stopProcessingFunc,
            Func<List<MidiDeviceInfo>>? getAvailableMidiDevicesFunc = null,
            Func<MidiManager?>? getMidiManagerFunc = null)
        {
            _loadConfigurationFunc = loadConfigurationFunc;
            _getActiveConfigPathFunc = getActiveConfigPathFunc;
            _startProcessingFunc = startProcessingFunc;
            _stopProcessingFunc = stopProcessingFunc;
            _getAvailableMidiDevicesFunc = getAvailableMidiDevicesFunc;
            _getMidiManagerFunc = getMidiManagerFunc;

            _logger.LogInformation("MIDI processing service functions set:");
            _logger.LogInformation("  - LoadConfiguration: {Available}", loadConfigurationFunc != null);
            _logger.LogInformation("  - GetActiveConfigPath: {Available}", getActiveConfigPathFunc != null);
            _logger.LogInformation("  - StartProcessing: {Available}", startProcessingFunc != null);
            _logger.LogInformation("  - StopProcessing: {Available}", stopProcessingFunc != null);
            _logger.LogInformation("  - GetAvailableMidiDevices: {Available}", getAvailableMidiDevicesFunc != null);
            _logger.LogInformation("  - GetMidiManager: {Available}", getMidiManagerFunc != null);
        }

        /// <summary>
        /// Sets the logger factory from the main application
        /// </summary>
        /// <param name="loggerFactory">The logger factory from the main application</param>
        /// <remarks>
        /// IMPORTANT: This method is called by the main application when launching the Configuration GUI.
        /// It ensures that all logging from MIDIFlux.GUI uses the same logging infrastructure as the main application.
        /// This provides centralized logging with consistent log levels, file paths, and rotation settings.
        /// </remarks>
        public void SetLoggerFactory(ILoggerFactory loggerFactory)
        {
            // Set the central logger factory in LoggingHelper
            LoggingHelper.SetCentralLoggerFactory(loggerFactory);

            _logger.LogInformation("Main application logger factory set and propagated to LoggingHelper");
        }



        /// <summary>
        /// Helper method to generate consistent error messages for unavailable delegates
        /// </summary>
        /// <param name="operation">The operation being attempted</param>
        /// <param name="delegateName">The name of the delegate that is not available</param>
        /// <returns>A standardized error message</returns>
        private string GetDelegateNotAvailableMessage(string operation, string delegateName)
        {
            return $"Cannot {operation}: {delegateName} delegate not available";
        }

        /// <summary>
        /// Executes a delegate function with error handling and returns a value
        /// </summary>
        /// <typeparam name="T">The return type of the delegate</typeparam>
        /// <param name="delegateFunc">The delegate function to execute</param>
        /// <param name="operation">The operation being performed (for logging)</param>
        /// <param name="delegateName">The name of the delegate (for logging)</param>
        /// <param name="defaultValue">The default value to return if delegate is null or execution fails</param>
        /// <returns>The result of the delegate execution or the default value</returns>
        private T? ExecuteDelegate<T>(Func<T>? delegateFunc, string operation, string delegateName, T? defaultValue = default)
        {
            if (delegateFunc != null)
            {
                try
                {
                    return delegateFunc();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing {DelegateName}: {ErrorMessage}", delegateName, ex.Message);
                    return defaultValue;
                }
            }

            _logger.LogError(GetDelegateNotAvailableMessage(operation, delegateName));
            return defaultValue;
        }

        /// <summary>
        /// Executes a delegate action with error handling
        /// </summary>
        /// <param name="delegateAction">The delegate action to execute</param>
        /// <param name="operation">The operation being performed (for logging)</param>
        /// <param name="delegateName">The name of the delegate (for logging)</param>
        private void ExecuteDelegate(Action? delegateAction, string operation, string delegateName)
        {
            if (delegateAction != null)
            {
                try
                {
                    delegateAction();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing {DelegateName}: {ErrorMessage}", delegateName, ex.Message);
                }
            }
            else
            {
                _logger.LogError(GetDelegateNotAvailableMessage(operation, delegateName));
            }
        }

        /// <summary>
        /// Executes a delegate action with error handling and returns success status
        /// </summary>
        /// <param name="delegateAction">The delegate action to execute</param>
        /// <param name="operation">The operation being performed (for logging)</param>
        /// <param name="delegateName">The name of the delegate (for logging)</param>
        /// <returns>True if the delegate executed successfully, false if delegate is null or execution failed</returns>
        private bool ExecuteDelegateWithResult(Action? delegateAction, string operation, string delegateName)
        {
            if (delegateAction != null)
            {
                try
                {
                    delegateAction();
                    return true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error executing {DelegateName}: {ErrorMessage}", delegateName, ex.Message);
                    return false;
                }
            }

            _logger.LogError(GetDelegateNotAvailableMessage(operation, delegateName));
            return false;
        }

        /// <summary>
        /// Checks if the MIDI processing service is available
        /// </summary>
        /// <returns>True if the service is available, false otherwise</returns>
        public bool IsServiceAvailable()
        {
            return _loadConfigurationFunc != null &&
                   _getActiveConfigPathFunc != null &&
                   _startProcessingFunc != null &&
                   _stopProcessingFunc != null;
        }

        /// <summary>
        /// Activates a profile by directly calling the MidiProcessingService
        /// </summary>
        /// <param name="configPath">The path to the configuration file</param>
        /// <returns>True if successful, false otherwise</returns>
        public bool ActivateProfile(string configPath)
        {
            try
            {
                if (!File.Exists(configPath))
                {
                    _logger.LogError("Configuration file not found: {ConfigPath}", configPath);
                    return false;
                }

                // If we have the service functions, use them
                if (_loadConfigurationFunc != null)
                {
                    bool result = _loadConfigurationFunc(configPath);
                    if (result)
                    {
                        _logger.LogInformation("Activated profile directly: {ConfigPath}", configPath);

                        // Create active profile information for the GUI
                        var activeProfileInfo = ConfigurationHelper.CreateActiveProfileInfo(configPath);

                        return true;
                    }
                    else
                    {
                        _logger.LogError("Failed to activate profile directly: {ConfigPath}", configPath);
                        return false;
                    }
                }
                else
                {
                    // Legacy file-based communication removed - unified system requires direct service communication
                    _logger.LogError("Cannot activate profile: service functions not available and legacy file-based activation removed");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating profile: {ConfigPath}", configPath);
                return false;
            }
        }

        /// <summary>
        /// Gets the currently active configuration path
        /// </summary>
        /// <returns>The active configuration path, or null if not available</returns>
        public string? GetActiveConfigurationPath()
        {
            var configPath = ExecuteDelegate(_getActiveConfigPathFunc, "get active configuration path", "GetActiveConfigPath", (string?)null);

            // Log the result for debugging
            if (string.IsNullOrEmpty(configPath))
            {
                _logger.LogDebug("No active configuration path available from service");
            }
            else
            {
                _logger.LogDebug("Active configuration path from service: {ConfigPath}", configPath);
            }

            return configPath;
        }

        /// <summary>
        /// Starts MIDI processing
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool StartProcessing()
        {
            return ExecuteDelegate(_startProcessingFunc, "start processing", "StartProcessing", false);
        }

        /// <summary>
        /// Stops MIDI processing
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public bool StopProcessing()
        {
            return ExecuteDelegateWithResult(_stopProcessingFunc, "stop processing", "StopProcessing");
        }

        /// <summary>
        /// Gets a list of available MIDI devices from the main application
        /// </summary>
        /// <returns>A list of available MIDI devices</returns>
        public List<MidiDeviceInfo> GetAvailableMidiDevices()
        {
            _logger.LogDebug("Getting available MIDI devices");

            if (_getAvailableMidiDevicesFunc != null)
            {
                _logger.LogDebug("Using main application's MidiManager to get MIDI devices");
            }

            return ExecuteDelegate(_getAvailableMidiDevicesFunc, "get MIDI devices", "GetAvailableMidiDevices", new List<MidiDeviceInfo>()) ?? new List<MidiDeviceInfo>();
        }

        /// <summary>
        /// Gets the MIDI manager from the main application (for dialog compatibility)
        /// </summary>
        /// <returns>The MIDI manager, or null if not available</returns>
        public MidiManager? GetMidiManager()
        {
            _logger.LogInformation("GetMidiManager called - checking function availability");

            if (_getMidiManagerFunc != null)
            {
                _logger.LogInformation("MidiManager function is available, calling it");
                try
                {
                    var midiManager = _getMidiManagerFunc();
                    _logger.LogInformation("MidiManager function returned: {Result}", midiManager != null ? "Valid MidiManager" : "null");
                    return midiManager;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calling MidiManager function: {Message}", ex.Message);
                    return null;
                }
            }

            _logger.LogError("MidiManager function not available - returning null");
            _logger.LogError("This indicates SetServiceFunctions was not called or getMidiManagerFunc was null");
            return null;
        }
    }
}
