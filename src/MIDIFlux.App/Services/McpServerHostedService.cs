using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIDIFlux.App.Models;
using System.Text;
using System.Text.Json;

namespace MIDIFlux.App.Services;

/// <summary>
/// Hosted service for MCP server lifecycle management with stdio transport.
/// Handles JSON-RPC 2.0 communication over stdin/stdout.
/// </summary>
public class McpServerHostedService : BackgroundService
{
    private readonly ILogger<McpServerHostedService> _logger;
    private readonly MidiFluxMcpServer _mcpServer;
    private readonly JsonSerializerOptions _jsonOptions;

    /// <summary>
    /// Initializes a new instance of the McpServerHostedService
    /// </summary>
    /// <param name="logger">Logger for this service</param>
    /// <param name="mcpServer">MCP server instance</param>
    public McpServerHostedService(
        ILogger<McpServerHostedService> logger,
        MidiFluxMcpServer mcpServer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mcpServer = mcpServer ?? throw new ArgumentNullException(nameof(mcpServer));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
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

            // Set up stdin/stdout for JSON-RPC communication
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            // Process requests from stdin
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Read line from stdin
                    var line = await ReadLineAsync(stoppingToken);
                    
                    if (line == null)
                    {
                        // EOF reached, graceful shutdown
                        _logger.LogInformation("EOF reached on stdin, shutting down MCP server");
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

                    // Send response to stdout
                    var responseJson = JsonSerializer.Serialize(response, _jsonOptions);
                    await Console.Out.WriteLineAsync(responseJson);
                    await Console.Out.FlushAsync();

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
                        await Console.Out.WriteLineAsync(errorJson);
                        await Console.Out.FlushAsync();
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
    /// Stop the service gracefully
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("MCP Server stopping...");
        await base.StopAsync(cancellationToken);
        _logger.LogInformation("MCP Server stopped");
    }
}
