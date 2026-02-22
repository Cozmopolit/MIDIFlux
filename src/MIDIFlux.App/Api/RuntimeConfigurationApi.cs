using Microsoft.Extensions.Logging;
using MIDIFlux.App.Services;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.App.Api;

/// <summary>
/// API for runtime configuration inspection and live device/mapping management with full type safety.
/// Provides comprehensive device and mapping management functionality.
/// </summary>
public class RuntimeConfigurationApi
{
    private readonly ILogger<RuntimeConfigurationApi> _logger;
    private readonly MidiProcessingService _midiProcessingService;
    private readonly MidiInputDetector _midiInputDetector;

    /// <summary>
    /// Initializes a new instance of the RuntimeConfigurationApi
    /// </summary>
    /// <param name="logger">Logger for this API</param>
    /// <param name="midiProcessingService">MIDI processing service for runtime operations</param>
    /// <param name="midiInputDetector">MIDI input detector for input detection</param>
    public RuntimeConfigurationApi(
        ILogger<RuntimeConfigurationApi> logger, 
        MidiProcessingService midiProcessingService,
        MidiInputDetector midiInputDetector)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _midiProcessingService = midiProcessingService ?? throw new ArgumentNullException(nameof(midiProcessingService));
        _midiInputDetector = midiInputDetector ?? throw new ArgumentNullException(nameof(midiInputDetector));
    }

    /// <summary>
    /// Gets current runtime configuration as MappingConfig object (same type as GetProfileContent).
    /// Returns the current configuration representing the runtime state.
    /// </summary>
    /// <returns>MappingConfig representing current configuration, or null if no configuration loaded</returns>
    public MappingConfig? GetCurrentConfiguration()
    {
        try
        {
            var config = _midiProcessingService.GetCurrentRuntimeConfiguration();
            
            if (config != null)
            {
                _logger.LogDebug("Retrieved current runtime configuration: {ProfileName} with {DeviceCount} devices",
                    config.ProfileName, config.MidiDevices?.Count ?? 0);
            }
            else
            {
                _logger.LogDebug("No current runtime configuration available");
            }

            return config;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get current configuration: {ErrorMessage}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Gets all currently configured devices.
    /// Returns type-safe list of device configurations.
    /// </summary>
    /// <returns>List of DeviceConfig with device names and their mappings</returns>
    public List<DeviceConfig> GetDevices()
    {
        try
        {
            var config = GetCurrentConfiguration();
            var devices = config?.MidiDevices ?? new List<DeviceConfig>();
            
            _logger.LogDebug("Retrieved {DeviceCount} configured devices", devices.Count);
            return devices;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get devices: {ErrorMessage}", ex.Message);
            return new List<DeviceConfig>();
        }
    }

    /// <summary>
    /// Gets specific device configuration by name.
    /// Returns type-safe device configuration object.
    /// </summary>
    /// <param name="deviceName">The device name to search for</param>
    /// <returns>DeviceConfig or null if not found</returns>
    public DeviceConfig? GetDevice(string deviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Device name is null or empty");
                return null;
            }

            var devices = GetDevices();
            var device = devices.FirstOrDefault(d => 
                string.Equals(d.DeviceName, deviceName, StringComparison.OrdinalIgnoreCase));

            if (device != null)
            {
                _logger.LogDebug("Found device {DeviceName} with {MappingCount} mappings", 
                    deviceName, device.Mappings?.Count ?? 0);
            }
            else
            {
                _logger.LogDebug("Device {DeviceName} not found", deviceName);
            }

            return device;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get device {DeviceName}: {ErrorMessage}", deviceName, ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Adds new device configuration to runtime.
    /// Wrapper with validation around core functionality.
    /// </summary>
    /// <param name="device">The device configuration to add</param>
    /// <returns>True if successful, false if device already exists or operation failed</returns>
    public bool AddDevice(DeviceConfig device)
    {
        try
        {
            if (device == null)
            {
                _logger.LogWarning("Cannot add null device");
                return false;
            }

            if (string.IsNullOrWhiteSpace(device.DeviceName))
            {
                _logger.LogWarning("Cannot add device with null or empty name");
                return false;
            }

            // Check if device already exists
            var existingDevice = GetDevice(device.DeviceName);
            if (existingDevice != null)
            {
                _logger.LogWarning("Device {DeviceName} already exists", device.DeviceName);
                return false;
            }

            var success = _midiProcessingService.AddDevice(device);
            
            if (success)
            {
                _logger.LogInformation("Successfully added device {DeviceName} with {MappingCount} mappings",
                    device.DeviceName, device.Mappings?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("Failed to add device {DeviceName}", device.DeviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding device {DeviceName}: {ErrorMessage}", 
                device?.DeviceName ?? "null", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Removes device and all its mappings from runtime.
    /// Wrapper around core functionality.
    /// </summary>
    /// <param name="deviceName">The device name to remove</param>
    /// <returns>True if device was found and removed, false otherwise</returns>
    public bool RemoveDevice(string deviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot remove device with null or empty name");
                return false;
            }

            // Check if device exists
            var existingDevice = GetDevice(deviceName);
            if (existingDevice == null)
            {
                _logger.LogWarning("Device {DeviceName} not found for removal", deviceName);
                return false;
            }

            var success = _midiProcessingService.RemoveDevice(deviceName);
            
            if (success)
            {
                _logger.LogInformation("Successfully removed device {DeviceName}", deviceName);
            }
            else
            {
                _logger.LogWarning("Failed to remove device {DeviceName}", deviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing device {DeviceName}: {ErrorMessage}", deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Adds typed mapping to specific device.
    /// Wrapper around core functionality.
    /// </summary>
    /// <param name="deviceName">The device name to add the mapping to</param>
    /// <param name="mapping">The mapping configuration to add</param>
    /// <returns>True if successful, false if device not found or operation failed</returns>
    public bool AddMapping(string deviceName, MappingConfigEntry mapping)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot add mapping to device with null or empty name");
                return false;
            }

            if (mapping == null)
            {
                _logger.LogWarning("Cannot add null mapping to device {DeviceName}", deviceName);
                return false;
            }

            // Check if device exists
            var existingDevice = GetDevice(deviceName);
            if (existingDevice == null)
            {
                _logger.LogWarning("Device {DeviceName} not found for adding mapping", deviceName);
                return false;
            }

            var success = _midiProcessingService.AddMapping(deviceName, mapping);
            
            if (success)
            {
                _logger.LogInformation("Successfully added mapping to device {DeviceName}: {Description}",
                    deviceName, mapping.Description ?? "Unnamed mapping");
            }
            else
            {
                _logger.LogWarning("Failed to add mapping to device {DeviceName}", deviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding mapping to device {DeviceName}: {ErrorMessage}",
                deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Removes specific mapping from device by matching input configuration.
    /// Wrapper around core functionality.
    /// </summary>
    /// <param name="deviceName">The device name to remove the mapping from</param>
    /// <param name="mapping">The mapping configuration to remove</param>
    /// <returns>True if mapping was found and removed, false otherwise</returns>
    public bool RemoveMapping(string deviceName, MappingConfigEntry mapping)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot remove mapping from device with null or empty name");
                return false;
            }

            if (mapping == null)
            {
                _logger.LogWarning("Cannot remove null mapping from device {DeviceName}", deviceName);
                return false;
            }

            // Check if device exists
            var existingDevice = GetDevice(deviceName);
            if (existingDevice == null)
            {
                _logger.LogWarning("Device {DeviceName} not found for removing mapping", deviceName);
                return false;
            }

            var success = _midiProcessingService.RemoveMapping(deviceName, mapping);

            if (success)
            {
                _logger.LogInformation("Successfully removed mapping from device {DeviceName}: {Description}",
                    deviceName, mapping.Description ?? "Unnamed mapping");
            }
            else
            {
                _logger.LogWarning("Failed to remove mapping from device {DeviceName}", deviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing mapping from device {DeviceName}: {ErrorMessage}",
                deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates an existing mapping in a device by replacing it atomically.
    /// Identifies the existing mapping by input configuration, then replaces it with the new mapping.
    /// </summary>
    /// <param name="deviceName">The device name containing the mapping</param>
    /// <param name="oldMapping">The mapping to find (matched by input configuration)</param>
    /// <param name="newMapping">The new mapping to replace it with</param>
    /// <returns>True if mapping was found and updated, false otherwise</returns>
    public bool UpdateMapping(string deviceName, MappingConfigEntry oldMapping, MappingConfigEntry newMapping)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot update mapping in device with null or empty name");
                return false;
            }

            if (oldMapping == null)
            {
                _logger.LogWarning("Cannot update mapping: oldMapping is null for device {DeviceName}", deviceName);
                return false;
            }

            if (newMapping == null)
            {
                _logger.LogWarning("Cannot update mapping: newMapping is null for device {DeviceName}", deviceName);
                return false;
            }

            // Check if device exists
            var existingDevice = GetDevice(deviceName);
            if (existingDevice == null)
            {
                _logger.LogWarning("Device {DeviceName} not found for updating mapping", deviceName);
                return false;
            }

            var success = _midiProcessingService.UpdateMapping(deviceName, oldMapping, newMapping);

            if (success)
            {
                _logger.LogInformation("Successfully updated mapping in device {DeviceName}: '{OldDescription}' -> '{NewDescription}'",
                    deviceName, oldMapping.Description ?? "Unnamed", newMapping.Description ?? "Unnamed");
            }
            else
            {
                _logger.LogWarning("Failed to update mapping in device {DeviceName}", deviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating mapping in device {DeviceName}: {ErrorMessage}",
                deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates the description of an existing device.
    /// Wrapper with validation around core functionality.
    /// </summary>
    /// <param name="deviceName">The device name to find</param>
    /// <param name="description">The new description</param>
    /// <returns>True if device was found and updated, false otherwise</returns>
    public bool UpdateDeviceDescription(string deviceName, string description)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot update device with null or empty name");
                return false;
            }

            if (description == null)
            {
                _logger.LogWarning("Cannot set null description for device {DeviceName}", deviceName);
                return false;
            }

            var existingDevice = GetDevice(deviceName);
            if (existingDevice == null)
            {
                _logger.LogWarning("Device {DeviceName} not found for description update", deviceName);
                return false;
            }

            var success = _midiProcessingService.UpdateDeviceDescription(deviceName, description);

            if (success)
            {
                _logger.LogInformation("Successfully updated description for device {DeviceName}", deviceName);
            }
            else
            {
                _logger.LogWarning("Failed to update description for device {DeviceName}", deviceName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating description for device {DeviceName}: {ErrorMessage}",
                deviceName, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Updates the metadata (ProfileName and/or Description) of the active profile.
    /// At least one parameter must be non-null.
    /// </summary>
    /// <param name="profileName">New profile name, or null to keep current</param>
    /// <param name="description">New description, or null to keep current</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool UpdateProfileMetadata(string? profileName, string? description)
    {
        try
        {
            if (profileName == null && description == null)
            {
                _logger.LogWarning("At least one of profileName or description must be provided");
                return false;
            }

            var success = _midiProcessingService.UpdateProfileMetadata(profileName, description);

            if (success)
            {
                _logger.LogInformation("Successfully updated profile metadata");
            }
            else
            {
                _logger.LogWarning("Failed to update profile metadata");
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile metadata: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Sets, updates, or removes an initial state value.
    /// If value is null, the key is removed. If value is not null, the key is set/updated.
    /// </summary>
    /// <param name="key">The state key (must be alphanumeric)</param>
    /// <param name="value">The value to set, or null to remove the key</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SetInitialState(string key, int? value)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                _logger.LogWarning("Cannot set initial state with null or empty key");
                return false;
            }

            // Validate key is alphanumeric (no asterisk prefix for internal states)
            if (!System.Text.RegularExpressions.Regex.IsMatch(key, @"^[a-zA-Z0-9]+$"))
            {
                _logger.LogWarning("Invalid state key '{Key}': must be alphanumeric only", key);
                return false;
            }

            var success = _midiProcessingService.SetInitialState(key, value);

            if (success)
            {
                if (value == null)
                    _logger.LogInformation("Removed initial state key '{Key}'", key);
                else
                    _logger.LogInformation("Set initial state '{Key}' = {Value}", key, value.Value);
            }
            else
            {
                _logger.LogWarning("Failed to set initial state for key '{Key}'", key);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting initial state for key '{Key}': {ErrorMessage}", key, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Gets all mappings for specific device.
    /// Returns type-safe list of mapping configurations.
    /// </summary>
    /// <param name="deviceName">The device name to get mappings for</param>
    /// <returns>List of MappingConfigEntry or empty list if device not found</returns>
    public List<MappingConfigEntry> GetMappings(string deviceName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                _logger.LogWarning("Cannot get mappings for device with null or empty name");
                return new List<MappingConfigEntry>();
            }

            var device = GetDevice(deviceName);
            var mappings = device?.Mappings ?? new List<MappingConfigEntry>();

            _logger.LogDebug("Retrieved {MappingCount} mappings for device {DeviceName}",
                mappings.Count, deviceName);

            return mappings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting mappings for device {DeviceName}: {ErrorMessage}",
                deviceName, ex.Message);
            return new List<MappingConfigEntry>();
        }
    }

    /// <summary>
    /// Detects MIDI input activity for specified duration and returns aggregated summary.
    /// Wrapper around MidiInputDetector functionality.
    /// </summary>
    /// <param name="durationSeconds">Duration to listen for input (1-20 seconds)</param>
    /// <param name="deviceFilter">Optional device name filter, null for all devices</param>
    /// <returns>Task with aggregated MIDI activity summary</returns>
    public async Task<MidiInputDetectionResult> DetectMidiInput(int durationSeconds, string? deviceFilter = null)
    {
        try
        {
            if (durationSeconds < 1 || durationSeconds > 20)
            {
                _logger.LogWarning("Invalid duration {DurationSeconds}. Must be between 1 and 20 seconds", durationSeconds);
                throw new ArgumentOutOfRangeException(nameof(durationSeconds), "Duration must be between 1 and 20 seconds");
            }

            _logger.LogInformation("Starting MIDI input detection for {DurationSeconds} seconds with device filter: {DeviceFilter}",
                durationSeconds, deviceFilter ?? "none");

            var result = await _midiInputDetector.DetectAsync(durationSeconds, deviceFilter);

            _logger.LogInformation("MIDI input detection completed. Detected {InputCount} unique inputs",
                result.DetectedInputs.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during MIDI input detection: {ErrorMessage}", ex.Message);

            // Return empty result on error
            return new MidiInputDetectionResult
            {
                DurationSeconds = durationSeconds,
                DetectedInputs = new List<DetectedMidiInput>()
            };
        }
    }
}
