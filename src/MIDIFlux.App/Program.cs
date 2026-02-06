
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using MIDIFlux.App.Extensions;
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

        // Set up global exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Application.ThreadException += Application_ThreadException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

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

                    // Create a temporary logger for initialization
                    var tempLoggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
                    var tempLogger = tempLoggerFactory.CreateLogger("MIDIFlux");

                    // Ensure all AppData directories and example files exist
                    MIDIFlux.Core.Helpers.AppDataHelper.EnsureDirectoriesExist(tempLogger);

                    // Ensure the appsettings.json file exists in the AppData directory
                    MIDIFlux.Core.Helpers.AppDataHelper.EnsureAppSettingsExist(tempLogger);

                    // Ensure example profiles exist (first-time setup only)
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

                var audioLogger = loggerFactory.CreateLogger("MIDIFlux.Audio");
                audioLogger.LogInformation("Audio playback service initialized successfully");
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

            // Display available MIDI devices
            var MidiDeviceManager = host.Services.GetRequiredService<MidiDeviceManager>();
            var devices = MidiDeviceManager.GetAvailableDevices();
            var logger = loggerFactory.CreateLogger("MIDIFlux");

            logger.LogInformation("[MIDIFlux] Verfügbare MIDI-Input-Geräte:");
            if (devices.Count > 0)
            {
                foreach (var device in devices)
                {
                    logger.LogInformation(" - {Device}", device);
                }
            }
            else
            {
                logger.LogInformation(" - Keine MIDI-Geräte gefunden");
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
                        logger.LogInformation("Found last used configuration: {ConfigPath}", configPath);

                        // Load the configuration
                        if (midiProcessingService.LoadConfiguration(configPath))
                        {
                            logger.LogInformation("Configuration loaded successfully");

                            // Start MIDI processing
                            if (midiProcessingService.Start())
                            {
                                logger.LogInformation("MIDI processing started");
                            }
                            else
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
