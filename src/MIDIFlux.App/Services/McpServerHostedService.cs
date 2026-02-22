using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIDIFlux.App.Models;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.App.Services;

/// <summary>
/// Hosted service for MCP server lifecycle management with stdio transport.
/// Handles JSON-RPC 2.0 communication over stdin/stdout.
/// </summary>
public class McpServerHostedService : BackgroundService
{
    private readonly ILogger<McpServerHostedService> _logger;
    private readonly MidiFluxMcpServer _mcpServer;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly JsonSerializerOptions _jsonOptions;

    // Direct stream access - bypasses Console.Out which may be redirected
    private StreamWriter? _stdout;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the McpServerHostedService
    /// </summary>
    /// <param name="logger">Logger for this service</param>
    /// <param name="mcpServer">MCP server instance</param>
    /// <param name="lifetime">Application lifetime for triggering shutdown on stdin EOF</param>
    public McpServerHostedService(
        ILogger<McpServerHostedService> logger,
        MidiFluxMcpServer mcpServer,
        IHostApplicationLifetime lifetime)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mcpServer = mcpServer ?? throw new ArgumentNullException(nameof(mcpServer));
        _lifetime = lifetime ?? throw new ArgumentNullException(nameof(lifetime));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false,
            // Omit null fields so responses don't include both "result":null and "error":null
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    /// <summary>
    /// Execute the MCP server stdio transport
    /// </summary>
    /// <param name="stoppingToken">Cancellation token</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("MCP Server starting stdio transport");

            // CRITICAL: Open stdout directly - bypasses Console.Out which may have been redirected
            // to TextWriter.Null during startup to prevent Host.CreateDefaultBuilder() corruption
            // Console.OutputEncoding would invalidate our StreamWriter, so we set encoding on stream directly
            Console.InputEncoding = Encoding.UTF8;
            _stdout = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(false)) { AutoFlush = true };

            // Process requests from stdin
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Read line from stdin
                    var line = await ReadLineAsync(stoppingToken);
                    
                    if (line == null)
                    {
                        // EOF on stdin means the MCP host closed the connection.
                        // Trigger application shutdown so the process exits cleanly.
                        _logger.LogInformation("EOF reached on stdin, triggering application shutdown");
                        _lifetime.StopApplication();
                        break;
                    }

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    _logger.LogDebug("Received MCP request: {Request}", line);

                    // Parse JSON-RPC request
                    McpRequest? request = null;
                    McpResponse response;

                    try
                    {
                        request = JsonSerializer.Deserialize<McpRequest>(line, _jsonOptions);
                        if (request == null)
                        {
                            throw new JsonException("Failed to deserialize request");
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to parse JSON-RPC request: {Line}", line);
                        
                        response = new McpResponse
                        {
                            Id = null,
                            JsonRpc = "2.0",
                            Error = new McpError
                            {
                                Code = McpErrorCodes.ParseError,
                                Message = "Parse error",
                                Data = new { details = ex.Message }
                            }
                        };
                    }

                    if (request != null)
                    {
                        // JSON-RPC notifications have no id and must NOT receive a response.
                        // MCP uses "notifications/<name>" for these (e.g. notifications/initialized).
                        if (request.Method.StartsWith("notifications/", StringComparison.OrdinalIgnoreCase))
                        {
                            _logger.LogDebug("Received MCP notification: {Method} (no response sent)", request.Method);
                            continue;
                        }

                        // Handle the request
                        response = await _mcpServer.HandleRequest(request);
                    }
                    else
                    {
                        // Use the error response created above
                        response = new McpResponse
                        {
                            Id = null,
                            JsonRpc = "2.0",
                            Error = new McpError
                            {
                                Code = McpErrorCodes.InvalidRequest,
                                Message = "Invalid request"
                            }
                        };
                    }

                    // Send response to stdout (using direct stream, not Console.Out)
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                    await WriteLineAsync(responseJson);

                    _logger.LogDebug("Sent MCP response: {Response}", responseJson);
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing MCP request: {ErrorMessage}", ex.Message);

                    // Send error response
                    var errorResponse = new McpResponse
                    {
                        Id = null,
                        JsonRpc = "2.0",
                        Error = new McpError
                        {
                            Code = McpErrorCodes.InternalError,
                            Message = "Internal server error",
                            Data = new { details = ex.Message }
                        }
                    };

                    try
                    {
                        var errorJson = JsonSerializer.Serialize(errorResponse, _jsonOptions);
                        await WriteLineAsync(errorJson);
                    }
                    catch (Exception writeEx)
                    {
                        _logger.LogError(writeEx, "Failed to write error response: {ErrorMessage}", writeEx.Message);
                    }
                }
            }

            _logger.LogInformation("MCP Server stdio transport stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error in MCP server: {ErrorMessage}", ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Read a line from stdin asynchronously with cancellation support
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Line from stdin or null if EOF</returns>
    private async Task<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Use a task to wrap the synchronous Console.ReadLine
            // This is not ideal but Console.ReadLine doesn't have an async version
            var readTask = Task.Run(() => Console.ReadLine(), cancellationToken);

            return await readTask;
        }
        catch (OperationCanceledException)
        {
            return null;
        }
    }

    /// <summary>
    /// Write a line to stdout with thread-safety (uses direct stream, not Console.Out)
    /// </summary>
    private async Task WriteLineAsync(string line)
    {
        if (_stdout == null) return;

        await _writeLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await _stdout.WriteLineAsync(line).ConfigureAwait(false);
            await _stdout.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            _writeLock.Release();
        }
    }

    /// <summary>
    /// Stop the service gracefully
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MCP Server stopping...");
        await base.StopAsync(cancellationToken);
        _stdout?.Dispose();
        _logger.LogInformation("MCP Server stopped");
    }
}
