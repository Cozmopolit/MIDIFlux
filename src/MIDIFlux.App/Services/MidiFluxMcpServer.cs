using System.Reflection;
using Microsoft.Extensions.Logging;
using MIDIFlux.App.Api;
using MIDIFlux.App.Models;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Configuration;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.App.Services;

/// <summary>
/// MIDIFlux MCP Server implementation providing JSON-RPC 2.0 over stdio transport.
/// Wraps existing API classes and provides comprehensive documentation and capability discovery.
/// </summary>
public class MidiFluxMcpServer
{
    private readonly ILogger<MidiFluxMcpServer> _logger;

    // Direct API dependencies (already implemented)
    private readonly ProfileManagementApi _profileApi;
    private readonly RuntimeConfigurationApi _runtimeApi;
    private readonly ProfileSwitchingApi _switchingApi;

    // MCP-specific documentation service
    private readonly DocumentationApi _documentationApi;

    private readonly JsonSerializerOptions _jsonOptions;

    // Options for deserializing configuration objects (includes ActionJsonConverter for action types)
    private static readonly JsonSerializerOptions _configOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        Converters = {
            new ActionJsonConverter(),
            new JsonStringEnumConverter()
        }
    };

    /// <summary>
    /// Initializes a new instance of the MidiFluxMcpServer
    /// </summary>
    public MidiFluxMcpServer(
        ILogger<MidiFluxMcpServer> logger,
        ProfileManagementApi profileApi,
        RuntimeConfigurationApi runtimeApi,
        ProfileSwitchingApi switchingApi,
        DocumentationApi documentationApi)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _profileApi = profileApi ?? throw new ArgumentNullException(nameof(profileApi));
        _runtimeApi = runtimeApi ?? throw new ArgumentNullException(nameof(runtimeApi));
        _switchingApi = switchingApi ?? throw new ArgumentNullException(nameof(switchingApi));
        _documentationApi = documentationApi ?? throw new ArgumentNullException(nameof(documentationApi));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Handle incoming MCP request and return appropriate response
    /// </summary>
    /// <param name="request">MCP request to handle</param>
    /// <returns>MCP response</returns>
    public async Task<McpResponse> HandleRequest(McpRequest request)
    {
        try
        {
            _logger.LogDebug("Handling MCP request: {Method}", request.Method);

            var response = new McpResponse
            {
                Id = request.Id,
                JsonRpc = "2.0"
            };

            try
            {
                response.Result = request.Method switch
                {
                    // MCP Protocol methods
                    "initialize" => await HandleInitialize(request),
                    "tools/list" => await HandleToolsList(),
                    "tools/call" => await HandleToolsCall(request),

                    // Part 1: Direct API wrapper tools
                    "midi_list_profiles" => await HandleListProfiles(),
                    "midi_get_profile_content" => await HandleGetProfileContent(request),
                    "midi_get_current_config" => await HandleGetCurrentConfig(),
                    "midi_get_devices" => await HandleGetDevices(),
                    "midi_get_device" => await HandleGetDevice(request),
                    "midi_add_device" => await HandleAddDevice(request),
                    "midi_remove_device" => await HandleRemoveDevice(request),
                    "midi_add_mapping" => await HandleAddMapping(request),
                    "midi_remove_mapping" => await HandleRemoveMapping(request),
                    "midi_get_mappings" => await HandleGetMappings(request),
                    "midi_detect_input" => await HandleDetectInput(request),
                    "midi_switch_profile" => await HandleSwitchProfile(request),
                    "midi_save_config" => await HandleSaveConfig(request),
                    "midi_get_active_profile_info" => await HandleGetActiveProfileInfo(),

                    // Part 2: MCP-specific documentation tools
                    "midi_get_capabilities" => await HandleGetCapabilities(),
                    "midi_get_action_types" => await HandleGetActionTypes(),
                    "midi_get_action_schema" => await HandleGetActionSchema(request),
                    "midi_get_input_types" => await HandleGetInputTypes(),
                    "midi_get_device_info" => await HandleGetDeviceInfo(),

                    _ => throw new InvalidOperationException($"Unknown method: {request.Method}")
                };

                _logger.LogDebug("Successfully handled MCP request: {Method}", request.Method);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MCP request {Method}: {ErrorMessage}", request.Method, ex.Message);
                
                response.Error = new McpError
                {
                    Code = McpErrorCodes.ApplicationError,
                    Message = ex.Message,
                    Data = new { method = request.Method, details = ex.ToString() }
                };
                response.Result = null;
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error handling MCP request: {ErrorMessage}", ex.Message);
            
            return new McpResponse
            {
                Id = request.Id,
                JsonRpc = "2.0",
                Error = new McpError
                {
                    Code = McpErrorCodes.InternalError,
                    Message = "Internal server error",
                    Data = new { details = ex.Message }
                }
            };
        }
    }

    #region MCP Protocol Handlers

    private Task<object> HandleInitialize(McpRequest request)
    {
        var response = new McpInitializeResponse
        {
            ProtocolVersion = "2024-11-05",
            ServerInfo = new McpServerInfo
            {
                Name = "MIDIFlux",
                Version = Assembly.GetExecutingAssembly()
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                    ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                    ?? "unknown"
            },
            Capabilities = new McpCapabilities
            {
                Tools = GetToolDefinitions()
            }
        };

        _logger.LogInformation("MCP server initialized");
        return Task.FromResult<object>(response);
    }

    private Task<object> HandleToolsList()
    {
        var tools = GetToolDefinitions();
        _logger.LogDebug("Returning {Count} tool definitions", tools.Count);
        return Task.FromResult<object>(new { tools });
    }

    private async Task<object> HandleToolsCall(McpRequest request)
    {
        if (request.Params == null)
        {
            throw new ArgumentException("Missing parameters for tools/call");
        }

        var toolCall = JsonSerializer.Deserialize<JsonElement>(request.Params.Value);
        var toolName = toolCall.GetProperty("name").GetString();
        var arguments = toolCall.TryGetProperty("arguments", out var args) ? args : (JsonElement?)null;

        if (string.IsNullOrEmpty(toolName))
        {
            throw new ArgumentException("Tool name is required");
        }

        // Create a new request for the tool
        var toolRequest = new McpRequest
        {
            Method = toolName,
            Params = arguments,
            Id = request.Id
        };

        var result = await HandleRequest(toolRequest);
        return new { content = new[] { new { type = "text", text = JsonSerializer.Serialize(result.Result, _jsonOptions) } } };
    }

    #endregion

    #region Part 1: Direct API Wrapper Tool Handlers

    private Task<object> HandleListProfiles()
    {
        var profiles = _profileApi.GetAvailableProfiles();
        return Task.FromResult<object>(new { profiles });
    }

    private Task<object> HandleGetProfileContent(McpRequest request)
    {
        var profilePath = GetStringParameter(request, "profilePath");
        var content = _profileApi.GetProfileContent(profilePath);
        return Task.FromResult<object>(content != null ? (object)content : new { error = "Profile not found" });
    }

    private Task<object> HandleGetCurrentConfig()
    {
        var config = _runtimeApi.GetCurrentConfiguration();
        return Task.FromResult<object>(config != null ? (object)config : new { error = "No configuration loaded" });
    }

    private Task<object> HandleGetDevices()
    {
        var devices = _runtimeApi.GetDevices();
        return Task.FromResult<object>(devices);
    }

    private Task<object> HandleGetDevice(McpRequest request)
    {
        var deviceName = GetStringParameter(request, "deviceName");
        var device = _runtimeApi.GetDevice(deviceName);
        return Task.FromResult<object>(device != null ? (object)device : new { error = "Device not found" });
    }

    private Task<object> HandleAddDevice(McpRequest request)
    {
        try
        {
            var deviceJson = GetObjectParameter(request, "device");
            if (deviceJson == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Missing required parameter: device" });
            }

            // Deserialize the JsonElement to DeviceConfig
            var device = JsonSerializer.Deserialize<DeviceConfig>(deviceJson.Value.GetRawText(), _configOptions);
            if (device == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Failed to deserialize device configuration" });
            }

            var success = _runtimeApi.AddDevice(device);
            return Task.FromResult<object>(new { success });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error in HandleAddDevice: {Message}", ex.Message);
            return Task.FromResult<object>(new { success = false, error = $"Invalid device JSON: {ex.Message}" });
        }
    }

    private Task<object> HandleRemoveDevice(McpRequest request)
    {
        var deviceName = GetStringParameter(request, "deviceName");
        var success = _runtimeApi.RemoveDevice(deviceName);
        return Task.FromResult<object>(new { success });
    }

    private Task<object> HandleAddMapping(McpRequest request)
    {
        try
        {
            var deviceName = GetStringParameter(request, "deviceName");
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return Task.FromResult<object>(new { success = false, error = "Missing required parameter: deviceName" });
            }

            var mappingJson = GetObjectParameter(request, "mapping");
            if (mappingJson == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Missing required parameter: mapping" });
            }

            // Deserialize the JsonElement to MappingConfigEntry
            var mapping = JsonSerializer.Deserialize<MappingConfigEntry>(mappingJson.Value.GetRawText(), _configOptions);
            if (mapping == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Failed to deserialize mapping configuration" });
            }

            var success = _runtimeApi.AddMapping(deviceName, mapping);
            return Task.FromResult<object>(new { success });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error in HandleAddMapping: {Message}", ex.Message);
            return Task.FromResult<object>(new { success = false, error = $"Invalid mapping JSON: {ex.Message}" });
        }
    }

    private Task<object> HandleRemoveMapping(McpRequest request)
    {
        try
        {
            var deviceName = GetStringParameter(request, "deviceName");
            if (string.IsNullOrWhiteSpace(deviceName))
            {
                return Task.FromResult<object>(new { success = false, error = "Missing required parameter: deviceName" });
            }

            var mappingJson = GetObjectParameter(request, "mapping");
            if (mappingJson == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Missing required parameter: mapping" });
            }

            // Deserialize the JsonElement to MappingConfigEntry
            var mapping = JsonSerializer.Deserialize<MappingConfigEntry>(mappingJson.Value.GetRawText(), _configOptions);
            if (mapping == null)
            {
                return Task.FromResult<object>(new { success = false, error = "Failed to deserialize mapping configuration" });
            }

            var success = _runtimeApi.RemoveMapping(deviceName, mapping);
            return Task.FromResult<object>(new { success });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON deserialization error in HandleRemoveMapping: {Message}", ex.Message);
            return Task.FromResult<object>(new { success = false, error = $"Invalid mapping JSON: {ex.Message}" });
        }
    }

    private Task<object> HandleGetMappings(McpRequest request)
    {
        var deviceName = GetStringParameter(request, "deviceName");
        var mappings = _runtimeApi.GetMappings(deviceName);
        return Task.FromResult<object>(mappings);
    }

    private async Task<object> HandleDetectInput(McpRequest request)
    {
        var durationSeconds = GetIntParameter(request, "durationSeconds");
        var deviceFilter = GetOptionalStringParameter(request, "deviceFilter");
        var result = await _runtimeApi.DetectMidiInput(durationSeconds, deviceFilter);
        return result;
    }

    private Task<object> HandleSwitchProfile(McpRequest request)
    {
        var profilePath = GetStringParameter(request, "profilePath");
        var success = _switchingApi.SwitchToProfile(profilePath);
        return Task.FromResult<object>(new { success });
    }

    private Task<object> HandleSaveConfig(McpRequest request)
    {
        var filePath = GetStringParameter(request, "filePath");
        var profileName = GetStringParameter(request, "profileName");
        var description = GetOptionalStringParameter(request, "description");
        var success = _switchingApi.SaveCurrentConfiguration(filePath, profileName, description);
        return Task.FromResult<object>(new { success });
    }

    private Task<object> HandleGetActiveProfileInfo()
    {
        var info = _switchingApi.GetActiveProfileInfo();
        return Task.FromResult<object>(info ?? new { error = "No active profile" });
    }

    #endregion

    #region Part 2: MCP-Specific Documentation Tool Handlers

    private Task<object> HandleGetCapabilities()
    {
        return Task.FromResult(_documentationApi.GetCapabilities());
    }

    private Task<object> HandleGetActionTypes()
    {
        return Task.FromResult(_documentationApi.GetActionTypes());
    }

    private Task<object> HandleGetActionSchema(McpRequest request)
    {
        var actionType = GetStringParameter(request, "actionType");
        return Task.FromResult(_documentationApi.GetActionSchema(actionType));
    }

    private Task<object> HandleGetInputTypes()
    {
        return Task.FromResult(_documentationApi.GetInputTypes());
    }

    private Task<object> HandleGetDeviceInfo()
    {
        return Task.FromResult(_documentationApi.GetDeviceInfo());
    }

    #endregion

    #region Helper Methods

    private string GetStringParameter(McpRequest request, string paramName)
    {
        if (request.Params == null)
            throw new ArgumentException($"Missing parameter: {paramName}");

        var element = request.Params.Value;
        if (element.TryGetProperty(paramName, out var prop))
        {
            return prop.GetString() ?? throw new ArgumentException($"Parameter {paramName} is null");
        }
        
        throw new ArgumentException($"Missing parameter: {paramName}");
    }

    private string? GetOptionalStringParameter(McpRequest request, string paramName)
    {
        if (request.Params == null) return null;

        var element = request.Params.Value;
        if (element.TryGetProperty(paramName, out var prop))
        {
            return prop.GetString();
        }
        
        return null;
    }

    private int GetIntParameter(McpRequest request, string paramName)
    {
        if (request.Params == null)
            throw new ArgumentException($"Missing parameter: {paramName}");

        var element = request.Params.Value;
        if (element.TryGetProperty(paramName, out var prop))
        {
            return prop.GetInt32();
        }
        
        throw new ArgumentException($"Missing parameter: {paramName}");
    }

    private JsonElement? GetObjectParameter(McpRequest request, string paramName)
    {
        if (request.Params == null)
            return null;

        var element = request.Params.Value;
        if (element.TryGetProperty(paramName, out var prop))
        {
            return prop;
        }

        return null;
    }

    private List<McpTool> GetToolDefinitions()
    {
        return new List<McpTool>
        {
            // Part 1: Direct API Wrappers
            new() { Name = "midi_list_profiles", Description = "Discover available MIDI configuration profiles to load, inspect, or use as templates. Essential for profile management and finding examples.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_profile_content", Description = "Load and inspect a specific MIDI profile's complete configuration including devices, mappings, and actions. Use to understand existing setups or copy configurations.", InputSchema = new { type = "object", properties = new { profilePath = new { type = "string", description = "Relative path to profile file" } }, required = new[] { "profilePath" } } },
            new() { Name = "midi_get_current_config", Description = "Get the currently active MIDI configuration that's processing MIDI events. Use to see what's currently running or before making changes.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_devices", Description = "List all MIDI devices currently configured in the active profile. Use to see what devices are set up and how many mappings each has.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_device", Description = "Get detailed configuration for a specific MIDI device including all its mappings. Use to inspect or modify a particular device's setup.", InputSchema = new { type = "object", properties = new { deviceName = new { type = "string", description = "Name of device to retrieve" } }, required = new[] { "deviceName" } } },
            new() { Name = "midi_add_device", Description = "Add a new MIDI device to the current configuration. Use when setting up a new controller or expanding your MIDI setup.", InputSchema = new { type = "object", properties = new { device = new { type = "object", description = "DeviceConfig object" } }, required = new[] { "device" } } },
            new() { Name = "midi_remove_device", Description = "Remove a MIDI device and all its mappings from the current configuration. Use when disconnecting devices or cleaning up configurations.", InputSchema = new { type = "object", properties = new { deviceName = new { type = "string", description = "Name of device to remove" } }, required = new[] { "deviceName" } } },
            new() { Name = "midi_add_mapping", Description = "Add a new MIDI input-to-action mapping to a specific device. Use to create new MIDI triggers for keyboard shortcuts, sounds, or other actions.", InputSchema = new { type = "object", properties = new { deviceName = new { type = "string", description = "Target device name" }, mapping = new { type = "object", description = "MappingConfigEntry object" } }, required = new[] { "deviceName", "mapping" } } },
            new() { Name = "midi_remove_mapping", Description = "Remove a specific MIDI input-to-action mapping from a device. Use to delete unwanted triggers or clean up configurations.", InputSchema = new { type = "object", properties = new { deviceName = new { type = "string", description = "Target device name" }, mapping = new { type = "object", description = "MappingConfigEntry to remove" } }, required = new[] { "deviceName", "mapping" } } },
            new() { Name = "midi_get_mappings", Description = "List all MIDI input-to-action mappings configured for a specific device. Use to see what triggers are set up on a particular controller.", InputSchema = new { type = "object", properties = new { deviceName = new { type = "string", description = "Target device name" } }, required = new[] { "deviceName" } } },
            new() { Name = "midi_detect_input", Description = "Monitor and detect MIDI input activity from connected devices for a specified time period. Essential for discovering what MIDI messages your device sends when you press buttons or move controls.", InputSchema = new { type = "object", properties = new { durationSeconds = new { type = "integer", description = "Detection duration (1-20)", minimum = 1, maximum = 20 }, deviceFilter = new { type = "string", description = "Filter by device name (optional)" } }, required = new[] { "durationSeconds" } } },
            new() { Name = "midi_switch_profile", Description = "Activate a different MIDI configuration profile, replacing the current active configuration. Use to switch between different setups (e.g., gaming vs. music production).", InputSchema = new { type = "object", properties = new { profilePath = new { type = "string", description = "Relative path to profile file" } }, required = new[] { "profilePath" } } },
            new() { Name = "midi_save_config", Description = "Save the currently active MIDI configuration to a new profile file. Use to preserve your current setup or create new profiles from your working configuration.", InputSchema = new { type = "object", properties = new { filePath = new { type = "string", description = "Target file path" }, profileName = new { type = "string", description = "Profile name" }, description = new { type = "string", description = "Profile description (optional)" } }, required = new[] { "filePath", "profileName" } } },
            new() { Name = "midi_get_active_profile_info", Description = "Get details about the currently active MIDI profile including name, file path, and load time. Use to check what configuration is currently running.", InputSchema = new { type = "object", properties = new { } } },

            // Part 2: MCP-Specific Documentation Tools
            new() { Name = "midi_get_capabilities", Description = "Get comprehensive system information about MIDIFlux capabilities, version, supported features, and file locations. Essential starting point for understanding what MIDIFlux can do.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_action_types", Description = "List all available action types that can be triggered by MIDI input (keyboard shortcuts, mouse clicks, sounds, etc.). Use to discover what actions you can configure.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_action_schema", Description = "Get detailed parameter schema and configuration options for a specific action type. Use to understand how to configure a particular action (e.g., what parameters KeyPressReleaseAction needs).", InputSchema = new { type = "object", properties = new { actionType = new { type = "string", description = "Action type name" } }, required = new[] { "actionType" } } },
            new() { Name = "midi_get_input_types", Description = "Learn about different types of MIDI input events (note presses, fader movements, etc.) and their characteristics. Essential for understanding what MIDI events can trigger actions.", InputSchema = new { type = "object", properties = new { } } },
            new() { Name = "midi_get_device_info", Description = "Get information about MIDI devices including connection status and configuration state. Use to check what MIDI hardware is available and properly connected.", InputSchema = new { type = "object", properties = new { } } }
        };
    }

    #endregion
}
