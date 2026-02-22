
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Text.Json;
using System.Text.Json.Serialization;
using MIDIFlux.App.Extensions;
using MIDIFlux.App.Mcp;
using MIDIFlux.App.Models;
using MIDIFlux.App.Services;
using MIDIFlux.Core;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.App;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        // Check for MCP server mode
        bool isMcpServerMode = args.Contains("--mcp-server");

        // CRITICAL: In MCP mode, redirect stdout IMMEDIATELY before any other code executes
        // MCP protocol requires stdout to be exclusively for JSON-RPC protocol frames
        // All diagnostic output MUST go to stderr (Console.Error)
        // Any data written to stdout will corrupt the protocol stream
        // Host.CreateDefaultBuilder() adds Console logger that writes to stdout before ConfigureLogging runs
        if (isMcpServerMode)
        {
            Console.SetOut(TextWriter.Null);
        }

        // Capture exe directory for error handling before anything else
        var exeDirectory = AppContext.BaseDirectory;

        // In GUI mode, hide the console window that OutputType=Exe creates.
        // In MCP server mode the console window is intentionally kept for stdio transport.
        if (!isMcpServerMode)
        {
            var consoleWindow = GetConsoleWindow();
            if (consoleWindow != IntPtr.Zero)
                ShowWindow(consoleWindow, SW_HIDE);
        }

        // Set up global exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Application.ThreadException += Application_ThreadException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        // In MCP mode, wrap startup in try-catch for Startup Error Mode
        if (isMcpServerMode)
        {
            try
            {
                RunNormalMode(args, isMcpServerMode);
            }
            catch (Exception ex)
            {
                RunStartupErrorMode(ex, exeDirectory);
            }
            return;
        }

        // GUI mode - existing exception handling
        try
        {
            // Initialize WinForms only in GUI mode
            if (!isMcpServerMode)
            {
                // To customize application configuration such as set high DPI settings or default font,
                // see https://aka.ms/applicationconfiguration.
                ApplicationConfiguration.Initialize();
            }

            // Create the host builder
            var builder = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    // Get the executable directory
                    string executableDir = Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppDomain.CurrentDomain.BaseDirectory;

                    // Create a temporary logger for initialization.
                    // In MCP server mode, do not add Console to avoid corrupting the stdio protocol stream.
                    var tempLoggerFactory = LoggerFactory.Create(builder =>
                    {
                        if (!isMcpServerMode) builder.AddConsole();
                    });
                    var tempLogger = tempLoggerFactory.CreateLogger("MIDIFlux");

                    // Ensure all AppData directories and example files exist
                    MIDIFlux.Core.Helpers.AppDataHelper.EnsureDirectoriesExist(tempLogger);

                    // Ensure the appsettings.json file exists in the AppData directory
                    MIDIFlux.Core.Helpers.AppDataHelper.EnsureAppSettingsExist(tempLogger);

                    // Ensure example profiles are up-to-date (always overwrites with latest embedded versions)
                    MIDIFlux.Core.Helpers.AppDataHelper.EnsureExampleProfilesExist(tempLogger);

                    // Get the app settings path
                    string appSettingsPath = MIDIFlux.Core.Helpers.AppDataHelper.GetAppSettingsPath();

                    // Add appsettings.json from AppData directory
                    config.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Add MIDIFlux services
                    services.AddMIDIFluxServices();

                    // Add MCP server services only in MCP mode
                    if (isMcpServerMode)
                    {
                        services.AddMcpServerServices();
                    }
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddDebug();

                    // Read settings from configuration
                    var enableFileLogging = context.Configuration.GetValue<bool>("Logging:EnableFileLogging", true);
                    var logLevel = context.Configuration.GetValue<string>("Logging:LogLevel", "Information");

                    // Parse log level
                    if (!Enum.TryParse<LogLevel>(logLevel, out var parsedLogLevel))
                    {
                        parsedLogLevel = LogLevel.Information;
                    }

                    // Add file logging if enabled
                    if (enableFileLogging)
                    {
                        // Get the logs directory from AppDataHelper
                        string logDirectory = MIDIFlux.Core.Helpers.AppDataHelper.GetLogsDirectory();

                        // Create logs directory if it doesn't exist
                        Directory.CreateDirectory(logDirectory);

                        // Add file logging with configured minimum level
                        logging.AddFile(Path.Combine(logDirectory, "MIDIFlux.log"),
                            minimumLevel: parsedLogLevel,
                            fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                            retainedFileCountLimit: 5);
                    }

                    // Set logging level for our namespaces
                    logging.AddFilter("MIDIFlux", parsedLogLevel);
                    logging.AddFilter("MIDIFlux.Core", parsedLogLevel);
                    logging.AddFilter("MIDIFlux.App", parsedLogLevel);
                    logging.AddFilter("MIDIFlux.GUI", parsedLogLevel);
                });

            // Build the host
            var host = builder.Build();

            // Set the central logger factory
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            LoggingHelper.SetCentralLoggerFactory(loggerFactory);

            // Set the static service provider for the action system
            ServiceCollectionExtensions.SetActionServiceProvider(host.Services);

            // Initialize audio playback service
            try
            {
                var audioService = host.Services.GetRequiredService<MIDIFlux.Core.Services.IAudioPlaybackService>();
                audioService.Initialize();

                // Ensure sounds directory exists
                MIDIFlux.Core.Helpers.AudioFileLoader.EnsureSoundsDirectoryExists();
            }
            catch (Exception ex)
            {
                var audioLogger = loggerFactory.CreateLogger("MIDIFlux.Audio");
                audioLogger.LogError(ex, "Failed to initialize audio playback service: {ErrorMessage}", ex.Message);
                // Continue without audio - PlaySoundAction will handle gracefully
            }

            // Create a test logger and log a debug message to verify debug logging is working
            var testLogger = loggerFactory.CreateLogger("MIDIFlux.DebugTest");
            testLogger.LogDebug("This is a test debug message to verify debug logging is working");

            // Start the host
            host.StartAsync().Wait();

            // Log startup information
            var logger = loggerFactory.CreateLogger("MIDIFlux");
            var assemblyVersion = Assembly.GetExecutingAssembly()
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
                ?? "unknown";
            logger.LogInformation("MIDIFlux v{Version} starting", assemblyVersion);
            logger.LogInformation("Runtime: {Framework}, OS: {OS}",
                RuntimeInformation.FrameworkDescription, RuntimeInformation.OSDescription);

            // Display available MIDI devices
            var MidiDeviceManager = host.Services.GetRequiredService<MidiDeviceManager>();
            var devices = MidiDeviceManager.GetAvailableDevices();

            logger.LogInformation("[MIDIFlux] Available MIDI input devices:");
            if (devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    logger.LogInformation(" - {Device}", device);
                }
            }
            else
            {
                logger.LogInformation(" - No MIDI devices found");
            }

            // Check Windows MIDI Services status and notify user if runtime is missing
            if (!isMcpServerMode)
            {
                CheckWindowsMidiServicesStatus(logger);
            }

            try
            {
                if (isMcpServerMode)
                {
                    // MCP Server mode - run as console application
                    logger.LogInformation("Starting MIDIFlux MCP Server...");
                    host.WaitForShutdown();
                }
                else
                {
                    // GUI mode - existing logic
                    // Get the MidiProcessingService
                    var midiProcessingService = host.Services.GetRequiredService<MidiProcessingService>();

                    // Try to load the last used configuration from %AppData%\MIDIFlux\profiles
                    logger.LogInformation("Checking for last used configuration in %AppData%\\MIDIFlux\\profiles");
                    var configPath = midiProcessingService.LoadLastUsedConfigurationPath();

                    if (!string.IsNullOrEmpty(configPath))
                    {
                        // Load the configuration (ConfigurationManager already logs the path)
                        if (midiProcessingService.LoadConfiguration(configPath))
                        {
                            logger.LogInformation("Configuration loaded successfully");

                            // Start MIDI processing (MidiProcessingService.Start() already logs success)
                            if (!midiProcessingService.Start())
                            {
                                logger.LogError("Failed to start MIDI processing");
                            }
                        }
                        else
                        {
                            logger.LogError("Failed to load configuration from: {ConfigPath}", configPath);
                        }
                    }
                    else
                    {
                        logger.LogInformation("No last used configuration found. Use the GUI to select a profile from %AppData%\\MIDIFlux\\profiles");
                    }

                    // Run the application
                    Application.Run(new SystemTrayForm(host));
                }
            }
            finally
            {
                // Log clean shutdown
                try
                {
                    var shutdownLogger = loggerFactory.CreateLogger("MIDIFlux");
                    shutdownLogger.LogInformation("MIDIFlux shutting down");
                }
                catch { /* logger may already be disposed */ }

                // Stop the host when the application exits
                host.StopAsync().Wait();
                host.Dispose();
            }
        }
        catch (Exception ex)
        {
            // Handle the critical exception
            // Create a logger for the critical error
            ILogger logger;
            try
            {
                logger = LoggingHelper.CreateLogger<SystemTrayForm>();
            }
            catch
            {
                // If creating the logger fails, use the fallback logger
                logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SystemTrayForm>();
            }

            ApplicationErrorHandler.HandleCriticalException(ex, "main application", logger);
        }
    }

    /// <summary>
    /// Handles unhandled exceptions in the current domain
    /// </summary>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        // Create a logger for the critical error
        ILogger logger;
        try
        {
            logger = LoggingHelper.CreateLogger<SystemTrayForm>();
        }
        catch
        {
            // If creating the logger fails, use the fallback logger
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SystemTrayForm>();
        }

        ApplicationErrorHandler.HandleCriticalException(e.ExceptionObject as Exception, "AppDomain", logger);
    }

    /// <summary>
    /// Handles unhandled exceptions in the UI thread
    /// </summary>
    private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // Create a logger for the critical error
        ILogger logger;
        try
        {
            logger = LoggingHelper.CreateLogger<SystemTrayForm>();
        }
        catch
        {
            // If creating the logger fails, use the fallback logger
            logger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<SystemTrayForm>();
        }

        ApplicationErrorHandler.HandleCriticalException(e.Exception, "UI thread", logger);
    }

    #region Native console management (GUI/MCP dual-mode)

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;

    #endregion

    #region MCP Startup Error Mode

    /// <summary>
    /// Run normal MCP mode (called from Main with try-catch wrapper)
    /// </summary>
    private static void RunNormalMode(string[] args, bool isMcpServerMode)
    {
        // Create the host builder
        var builder = Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                // Get the executable directory
                string executableDir = Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppDomain.CurrentDomain.BaseDirectory;

                // Create a temporary logger for initialization (no Console in MCP mode)
                var tempLoggerFactory = LoggerFactory.Create(b => { /* no providers - stdout is null */ });
                var tempLogger = tempLoggerFactory.CreateLogger("MIDIFlux");

                // Ensure all AppData directories and example files exist
                MIDIFlux.Core.Helpers.AppDataHelper.EnsureDirectoriesExist(tempLogger);
                MIDIFlux.Core.Helpers.AppDataHelper.EnsureAppSettingsExist(tempLogger);
                MIDIFlux.Core.Helpers.AppDataHelper.EnsureExampleProfilesExist(tempLogger);

                // Add appsettings.json from AppData directory
                string appSettingsPath = MIDIFlux.Core.Helpers.AppDataHelper.GetAppSettingsPath();
                config.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddMIDIFluxServices();
                services.AddMcpServerServices();
            })
            .ConfigureLogging((context, logging) =>
            {
                logging.ClearProviders();
                logging.AddDebug();

                var enableFileLogging = context.Configuration.GetValue<bool>("Logging:EnableFileLogging", true);
                var logLevel = context.Configuration.GetValue<string>("Logging:LogLevel", "Information");
                if (!Enum.TryParse<LogLevel>(logLevel, out var parsedLogLevel))
                    parsedLogLevel = LogLevel.Information;

                if (enableFileLogging)
                {
                    string logDirectory = MIDIFlux.Core.Helpers.AppDataHelper.GetLogsDirectory();
                    Directory.CreateDirectory(logDirectory);
                    logging.AddFile(Path.Combine(logDirectory, "MIDIFlux.log"),
                        minimumLevel: parsedLogLevel,
                        fileSizeLimitBytes: 10 * 1024 * 1024,
                        retainedFileCountLimit: 5);
                }

                logging.AddFilter("MIDIFlux", parsedLogLevel);
                logging.AddFilter("MIDIFlux.Core", parsedLogLevel);
                logging.AddFilter("MIDIFlux.App", parsedLogLevel);
            });

        var host = builder.Build();
        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        LoggingHelper.SetCentralLoggerFactory(loggerFactory);
        ServiceCollectionExtensions.SetActionServiceProvider(host.Services);

        // Initialize audio
        try
        {
            var audioService = host.Services.GetRequiredService<MIDIFlux.Core.Services.IAudioPlaybackService>();
            audioService.Initialize();
            MIDIFlux.Core.Helpers.AudioFileLoader.EnsureSoundsDirectoryExists();
        }
        catch (Exception ex)
        {
            var audioLogger = loggerFactory.CreateLogger("MIDIFlux.Audio");
            audioLogger.LogError(ex, "Failed to initialize audio playback service: {ErrorMessage}", ex.Message);
        }

        // Start the host (starts MIDI services, audio, etc.)
        host.StartAsync().Wait();

        var logger = loggerFactory.CreateLogger("MIDIFlux");
        var assemblyVersion = Assembly.GetExecutingAssembly()
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "unknown";
        logger.LogInformation("MIDIFlux MCP Server v{Version} starting", assemblyVersion);
        logger.LogInformation("Runtime: {Framework}, OS: {OS}",
            RuntimeInformation.FrameworkDescription, RuntimeInformation.OSDescription);

        var midiDeviceManager = host.Services.GetRequiredService<MidiDeviceManager>();
        var devices = midiDeviceManager.GetAvailableDevices();
        logger.LogInformation("Available MIDI input devices: {Count}", devices.Count);
        foreach (var device in devices)
            logger.LogInformation(" - {Device}", device);

        // Resolve MCP server from DI (no BackgroundService - we run the stdio loop directly)
        var mcpServer = host.Services.GetRequiredService<MidiFluxMcpServer>();

        // Set up JSON serialization for MCP protocol
        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Set up stdio for MCP protocol
        // Restore stdout via Console.SetOut so Console.WriteLine sends JSON-RPC responses
        // stdout was set to TextWriter.Null at the very start to prevent Host.CreateDefaultBuilder() corruption
        Console.InputEncoding = new System.Text.UTF8Encoding(false);
        Console.SetOut(new StreamWriter(Console.OpenStandardOutput(), new System.Text.UTF8Encoding(false)) { AutoFlush = true });

        logger.LogInformation("MCP Server ready, entering stdio loop");

        // Direct stdio loop - read JSON-RPC requests from stdin, write responses to stdout
        try
        {
            while (true)
            {
                var line = Console.ReadLine();
                if (line == null)
                {
                    // EOF on stdin means the MCP host closed the connection
                    logger.LogInformation("EOF on stdin, shutting down");
                    break;
                }

                if (string.IsNullOrWhiteSpace(line))
                    continue;

                logger.LogDebug("Received MCP request: {Request}", line);

                // Parse JSON-RPC request
                McpRequest? request;
                try
                {
                    request = JsonSerializer.Deserialize<McpRequest>(line, jsonOptions);
                    if (request == null)
                        throw new JsonException("Deserialized request is null");
                }
                catch (JsonException ex)
                {
                    logger.LogError(ex, "Failed to parse JSON-RPC request: {Line}", line);
                    var parseError = new McpResponse
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
                    Console.WriteLine(JsonSerializer.Serialize(parseError, jsonOptions));
                    continue;
                }

                // JSON-RPC notifications have no id and must NOT receive a response
                if (request.Method.StartsWith("notifications/", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogDebug("Received MCP notification: {Method} (no response sent)", request.Method);
                    continue;
                }

                // Handle the request via MidiFluxMcpServer
                McpResponse response;
                try
                {
                    response = mcpServer.HandleRequest(request).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error handling MCP request {Method}: {Error}", request.Method, ex.Message);
                    response = new McpResponse
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

                var responseJson = JsonSerializer.Serialize(response, jsonOptions);
                Console.WriteLine(responseJson);
                logger.LogDebug("Sent MCP response: {Response}", responseJson);
            }
        }
        finally
        {
            logger.LogInformation("MIDIFlux MCP Server shutting down");
            host.StopAsync().Wait();
            host.Dispose();
        }
    }

    /// <summary>
    /// Run Startup Error Mode - provides minimal MCP server with get_startup_error tool
    /// </summary>
    private static void RunStartupErrorMode(Exception ex, string exeDirectory)
    {
        // Write to stderr (visible in MCP client logs)
        Console.Error.WriteLine($"[MIDIFlux] FATAL: Startup failed - entering Startup Error Mode");
        Console.Error.WriteLine($"[MIDIFlux] Error: {ex.Message}");

        // Create diagnostics
        var diagnostics = StartupDiagnostics.FromException(ex, exeDirectory);

        // Write to file for offline debugging
        StartupErrorLogger.WriteToFile(diagnostics, exeDirectory);

        // Run minimal MCP host with only get_startup_error tool
        var minimalHost = new MinimalMcpHost(diagnostics);
        minimalHost.RunAsync(CancellationToken.None).GetAwaiter().GetResult();
    }

    #endregion

    /// <summary>
    /// Checks Windows MIDI Services status and shows a notification if the runtime is missing.
    /// </summary>
    /// <param name="logger">Logger for diagnostic output</param>
    private static void CheckWindowsMidiServicesStatus(ILogger logger)
    {
        try
        {
            var status = MIDIFlux.Core.Hardware.MidiAdapterFactory.GetAdapterStatus();

            logger.LogInformation(
                "Windows MIDI Services status: OS supports = {OsSupports}, Runtime installed = {RuntimeInstalled}",
                status.OsSupportsWindowsMidiServices,
                status.WindowsMidiServicesRuntimeInstalled);

            if (status.ShouldPromptForRuntimeInstall)
            {
                logger.LogWarning(
                    "Windows MIDI Services runtime not installed. Using NAudio adapter as fallback. " +
                    "For best compatibility on Windows 11 24H2+, install the runtime from: {Url}",
                    MIDIFlux.Core.Hardware.MidiAdapterFactory.WindowsMidiServicesDownloadUrl);

                var result = MessageBox.Show(
                    "Your Windows version supports Windows MIDI Services, but the runtime is not installed.\n\n" +
                    "Windows MIDI Services provides better MIDI support on Windows 11 24H2 and later.\n\n" +
                    "MIDIFlux will use the legacy NAudio adapter instead, which may have compatibility issues " +
                    "on newer Windows versions.\n\n" +
                    "Would you like to open the download page for Windows MIDI Services?\n\n" +
                    "(You can install it later and restart MIDIFlux)",
                    "Windows MIDI Services Runtime Not Found",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = MIDIFlux.Core.Hardware.MidiAdapterFactory.WindowsMidiServicesDownloadUrl,
                            UseShellExecute = true
                        });
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Failed to open download URL");
                        MessageBox.Show(
                            $"Could not open the browser. Please visit:\n\n{MIDIFlux.Core.Hardware.MidiAdapterFactory.WindowsMidiServicesDownloadUrl}",
                            "Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Don't let this check crash the application
            logger.LogWarning(ex, "Failed to check Windows MIDI Services status");
        }
    }
}
