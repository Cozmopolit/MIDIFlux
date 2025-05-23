
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
        // Set up global exception handler
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        Application.ThreadException += Application_ThreadException;
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        try
        {
            // Parse command-line arguments
            string? configPath = null;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "--config" && i + 1 < args.Length)
                {
                    configPath = args[i + 1];
                    i++; // Skip the next argument
                }
            }
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

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

                    // Get the app settings path
                    string appSettingsPath = MIDIFlux.Core.Helpers.AppDataHelper.GetAppSettingsPath();

                    // Add appsettings.json from AppData directory
                    config.AddJsonFile(appSettingsPath, optional: true, reloadOnChange: true);
                })
                .ConfigureServices((context, services) =>
                {
                    // Add MIDIFlux services
                    services.AddMIDIFluxServices();
                })
                .ConfigureLogging((context, logging) =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                    logging.AddDebug();

                    // Get the logs directory from AppDataHelper
                    string logDirectory = MIDIFlux.Core.Helpers.AppDataHelper.GetLogsDirectory();

                    // Create logs directory if it doesn't exist
                    Directory.CreateDirectory(logDirectory);

                    // Add file logging with explicit minimum level set to Debug
                    logging.AddFile(Path.Combine(logDirectory, "MIDIFlux.log"),
                        minimumLevel: LogLevel.Debug,  // Explicitly set minimum level to Debug
                        fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                        retainedFileCountLimit: 5);

                    // Explicitly set debug logging for our namespaces
                    logging.AddFilter("MIDIFlux", LogLevel.Debug);
                    logging.AddFilter("MIDIFlux.Core", LogLevel.Debug);
                    logging.AddFilter("MIDIFlux.App", LogLevel.Debug);
                    logging.AddFilter("MIDIFlux.GUI", LogLevel.Debug);

                    // Use the log level from configuration
                    // The default will be "None" (logging turned off) as set in the default appsettings.json
                });

            // Build the host
            var host = builder.Build();

            // Set the central logger factory
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            LoggingHelper.SetCentralLoggerFactory(loggerFactory);

            // Create a test logger and log a debug message to verify debug logging is working
            var testLogger = loggerFactory.CreateLogger("MIDIFlux.DebugTest");
            testLogger.LogDebug("This is a test debug message to verify debug logging is working");

            // Start the host
            host.StartAsync().Wait();

            // Display available MIDI devices
            var midiManager = host.Services.GetRequiredService<MidiManager>();
            var devices = midiManager.GetAvailableDevices();
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

            try
            {
                // Get the MidiProcessingService
                var midiProcessingService = host.Services.GetRequiredService<MidiProcessingService>();

                // If no config path was specified via command line, try to load the last used configuration
                if (string.IsNullOrEmpty(configPath))
                {
                    logger.LogInformation("No configuration specified via command line, checking for last used configuration");
                    configPath = midiProcessingService.LoadLastUsedConfigurationPath();

                    if (!string.IsNullOrEmpty(configPath))
                    {
                        logger.LogInformation("Found last used configuration: {ConfigPath}", configPath);
                    }
                    else
                    {
                        logger.LogInformation("No last used configuration found");
                    }
                }

                // If a config path is available (either from command line or last used), load it
                if (!string.IsNullOrEmpty(configPath))
                {
                    // Resolve the path if it's relative
                    if (!Path.IsPathRooted(configPath))
                    {
                        // Try relative to current directory
                        string currentDir = Directory.GetCurrentDirectory();
                        string resolvedPath = Path.Combine(currentDir, configPath);

                        if (!File.Exists(resolvedPath))
                        {
                            // Try relative to executable directory
                            string executableDir = Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppDomain.CurrentDomain.BaseDirectory;
                            resolvedPath = Path.Combine(executableDir, configPath);

                            if (!File.Exists(resolvedPath))
                            {
                                // Try relative to AppData profiles directory
                                string appDataProfilesDir = Path.Combine(
                                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                                    "MIDIFlux",
                                    "profiles");
                                resolvedPath = Path.Combine(appDataProfilesDir, configPath);
                            }
                        }

                        configPath = resolvedPath;
                    }

                    logger.LogInformation("Loading configuration from: {ConfigPath}", configPath);

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

                // Run the application
                Application.Run(new SystemTrayForm(host));
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
                logger = LoggingHelper.CreateLogger("MIDIFlux.CriticalError");
            }
            catch
            {
                // If creating the logger fails, use the fallback logger
                logger = LoggingHelper.CreateFallbackLogger("MIDIFlux.CriticalError");
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
            logger = LoggingHelper.CreateLogger("MIDIFlux.CriticalError");
        }
        catch
        {
            // If creating the logger fails, use the fallback logger
            logger = LoggingHelper.CreateFallbackLogger("MIDIFlux.CriticalError");
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
            logger = LoggingHelper.CreateLogger("MIDIFlux.CriticalError");
        }
        catch
        {
            // If creating the logger fails, use the fallback logger
            logger = LoggingHelper.CreateFallbackLogger("MIDIFlux.CriticalError");
        }

        ApplicationErrorHandler.HandleCriticalException(e.Exception, "UI thread", logger);
    }
}
