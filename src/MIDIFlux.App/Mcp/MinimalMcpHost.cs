using System.Text;
using System.Text.Json;

namespace MIDIFlux.App.Mcp;

/// <summary>
/// Minimal MCP host for Startup Error Mode - provides only get_startup_error tool
/// </summary>
internal sealed class MinimalMcpHost
{
    private readonly StartupDiagnostics _diagnostics;
    private readonly SemaphoreSlim _writeLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    public MinimalMcpHost(StartupDiagnostics diagnostics)
    {
        _diagnostics = diagnostics;
    }

    /// <summary>
    /// Run the minimal MCP server on STDIO
    /// </summary>
    public async Task RunAsync(CancellationToken ct)
    {
        Console.InputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        Console.OutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);

        // Restore stdout for MCP protocol (was set to Null during startup)
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true });

        while (!ct.IsCancellationRequested)
        {
            var line = Console.ReadLine();
            if (line is null) break; // EOF

            await HandleRequestAsync(line, ct).ConfigureAwait(false);
        }
    }

    private async Task HandleRequestAsync(string line, CancellationToken ct)
    {
        JsonElement request;
        object? id = null;
        string method = string.Empty;

        try
        {
            request = JsonSerializer.Deserialize<JsonElement>(line);
            if (request.TryGetProperty("id", out var idProp))
                id = idProp.ValueKind == JsonValueKind.Number ? idProp.GetInt64() : idProp.GetString();
            if (request.TryGetProperty("method", out var methodProp))
                method = methodProp.GetString() ?? string.Empty;
        }
        catch
        {
            await WriteResponseAsync(new { jsonrpc = "2.0", id = (object?)null, error = new { code = -32700, message = "Parse error" } }, ct);
            return;
        }

        object response = method switch
        {
            "initialize" => HandleInitialize(request, id),
            "tools/list" => HandleToolsList(id),
            "tools/call" => HandleToolCall(request, id),
            _ => new { jsonrpc = "2.0", id, error = new { code = -32601, message = $"Method not found: {method}" } }
        };

        await WriteResponseAsync(response, ct).ConfigureAwait(false);
    }

    private object HandleInitialize(JsonElement request, object? id)
    {
        string protocol = "2024-11-05";
        if (request.TryGetProperty("params", out var p) && p.TryGetProperty("protocolVersion", out var pv))
            protocol = pv.GetString() ?? protocol;

        return new
        {
            jsonrpc = "2.0",
            id,
            result = new
            {
                protocolVersion = protocol,
                capabilities = new { tools = new { listChanged = false } },
                serverInfo = new { name = "MIDIFlux (StartupErrorMode)", version = "1.0.0-error" }
            }
        };
    }

    private object HandleToolsList(object? id) => new
    {
        jsonrpc = "2.0",
        id,
        result = new
        {
            tools = new[]
            {
                new
                {
                    name = "get_startup_error",
                    description = "Retrieve detailed startup error diagnostics for MIDIFlux MCP Server",
                    inputSchema = new { type = "object", properties = new { }, required = Array.Empty<string>() }
                }
            }
        }
    };

    private object HandleToolCall(JsonElement request, object? id)
    {
        if (!request.TryGetProperty("params", out var p) || !p.TryGetProperty("name", out var nameProp))
            return new { jsonrpc = "2.0", id, error = new { code = -32602, message = "Missing tool name" } };

        if (nameProp.GetString() != "get_startup_error")
            return new { jsonrpc = "2.0", id, error = new { code = -32601, message = $"Tool not found: {nameProp.GetString()}" } };

        var diagnosticsJson = JsonSerializer.Serialize(_diagnostics, new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return new
        {
            jsonrpc = "2.0",
            id,
            result = new { content = new[] { new { type = "text", text = diagnosticsJson } } }
        };
    }

    private async Task WriteResponseAsync(object response, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(response, _jsonOptions);
        await _writeLock.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            Console.WriteLine(json);
        }
        finally
        {
            _writeLock.Release();
        }
    }
}

