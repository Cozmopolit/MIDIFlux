using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.App.Models;

/// <summary>
/// JSON-RPC 2.0 request model for MCP communication
/// </summary>
public class McpRequest
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("method")]
    public string Method { get; set; } = string.Empty;

    [JsonPropertyName("params")]
    public JsonElement? Params { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 response model for MCP communication
/// </summary>
public class McpResponse
{
    [JsonPropertyName("jsonrpc")]
    public string JsonRpc { get; set; } = "2.0";

    [JsonPropertyName("id")]
    public object? Id { get; set; }

    [JsonPropertyName("result")]
    public object? Result { get; set; }

    [JsonPropertyName("error")]
    public McpError? Error { get; set; }
}

/// <summary>
/// JSON-RPC 2.0 error model for MCP communication
/// </summary>
public class McpError
{
    [JsonPropertyName("code")]
    public int Code { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    [JsonPropertyName("data")]
    public object? Data { get; set; }
}

/// <summary>
/// Standard JSON-RPC 2.0 error codes
/// </summary>
public static class McpErrorCodes
{
    public const int ParseError = -32700;
    public const int InvalidRequest = -32600;
    public const int MethodNotFound = -32601;
    public const int InvalidParams = -32602;
    public const int InternalError = -32603;
    public const int ApplicationError = -1;
}

/// <summary>
/// MCP tool parameter model
/// </summary>
public class McpToolParameter
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("required")]
    public bool Required { get; set; }
}

/// <summary>
/// MCP tool definition model
/// </summary>
public class McpTool
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("inputSchema")]
    public object InputSchema { get; set; } = new { };
}

/// <summary>
/// MCP capabilities response model
/// </summary>
public class McpCapabilities
{
    [JsonPropertyName("tools")]
    public List<McpTool> Tools { get; set; } = new();

    [JsonPropertyName("prompts")]
    public List<object> Prompts { get; set; } = new();

    [JsonPropertyName("resources")]
    public List<object> Resources { get; set; } = new();
}

/// <summary>
/// MCP initialization request model
/// </summary>
public class McpInitializeRequest
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public object Capabilities { get; set; } = new { };

    [JsonPropertyName("clientInfo")]
    public McpClientInfo ClientInfo { get; set; } = new();
}

/// <summary>
/// MCP client information model
/// </summary>
public class McpClientInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// MCP server information model
/// </summary>
public class McpServerInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "MIDIFlux";

    [JsonPropertyName("version")]
    public string Version { get; set; } = "0.8.0";
}

/// <summary>
/// MCP initialization response model
/// </summary>
public class McpInitializeResponse
{
    [JsonPropertyName("protocolVersion")]
    public string ProtocolVersion { get; set; } = "2024-11-05";

    [JsonPropertyName("capabilities")]
    public McpCapabilities Capabilities { get; set; } = new();

    [JsonPropertyName("serverInfo")]
    public McpServerInfo ServerInfo { get; set; } = new();
}
