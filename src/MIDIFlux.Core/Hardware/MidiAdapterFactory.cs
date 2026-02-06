using Microsoft.Extensions.Logging;
using Microsoft.Windows.Devices.Midi2.Initialization;

namespace MIDIFlux.Core.Hardware;

/// <summary>
/// Factory for creating the appropriate MIDI hardware adapter based on configuration and system capabilities.
/// </summary>
/// <remarks>
/// This factory supports automatic adapter selection based on OS version and SDK Runtime availability,
/// as well as explicit adapter selection via configuration.
/// 
/// Windows MIDI Services requires:
/// - Windows 11 24H2 or later (Build 26100+)
/// - Windows MIDI Services SDK Runtime installed
/// </remarks>
public static class MidiAdapterFactory
{
    /// <summary>
    /// Windows 11 24H2 build number threshold for Windows MIDI Services support.
    /// </summary>
    private const int WindowsMidiServicesBuildThreshold = 26100;

    /// <summary>
    /// Creates the appropriate MIDI hardware adapter based on the specified preference.
    /// </summary>
    /// <param name="preferredType">The preferred adapter type (Auto, NAudio, or WindowsMidiServices)</param>
    /// <param name="logger">Logger for diagnostic output</param>
    /// <returns>An instance of IMidiHardwareAdapter</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when WindowsMidiServices is explicitly requested but not available on the system.
    /// </exception>
    public static IMidiHardwareAdapter Create(MidiAdapterType preferredType, ILogger logger)
    {
        logger.LogInformation("MIDI adapter selection: Preferred type = {PreferredType}", preferredType);

        switch (preferredType)
        {
            case MidiAdapterType.NAudio:
                logger.LogInformation("Using NAudio adapter (explicitly configured)");
                return new NAudioMidiAdapter();

            case MidiAdapterType.WindowsMidiServices:
                // Explicit request - fail if not available
                if (!IsWindowsMidiServicesAvailable())
                {
                    throw new InvalidOperationException(
                        $"Windows MIDI Services adapter was explicitly requested but is not available. " +
                        $"Requires Windows 11 24H2+ (Build {WindowsMidiServicesBuildThreshold}+). " +
                        $"Current OS: {Environment.OSVersion}");
                }
                if (!IsWindowsMidiServicesRuntimeInstalled())
                {
                    throw new InvalidOperationException(
                        "Windows MIDI Services adapter was explicitly requested but the SDK Runtime is not installed. " +
                        "Please install the Windows MIDI Services SDK Runtime from https://aka.ms/windowsmidiservices");
                }
                logger.LogInformation("Using Windows MIDI Services adapter (explicitly configured)");
                return CreateWindowsMidiServicesAdapter();

            case MidiAdapterType.Auto:
            default:
                return CreateAutoSelectedAdapter(logger);
        }
    }

    /// <summary>
    /// Automatically selects the best available adapter based on system capabilities.
    /// </summary>
    private static IMidiHardwareAdapter CreateAutoSelectedAdapter(ILogger logger)
    {
        var osAvailable = IsWindowsMidiServicesAvailable();
        var runtimeInstalled = IsWindowsMidiServicesRuntimeInstalled();

        logger.LogInformation(
            "Auto-selecting MIDI adapter: OS supports Windows MIDI Services = {OsAvailable}, Runtime installed = {RuntimeInstalled}",
            osAvailable, runtimeInstalled);

        if (osAvailable && runtimeInstalled)
        {
            logger.LogInformation("Auto-selected: Windows MIDI Services adapter");
            return CreateWindowsMidiServicesAdapter();
        }

        logger.LogInformation("Auto-selected: NAudio adapter (fallback)");
        return new NAudioMidiAdapter();
    }

    /// <summary>
    /// Creates a Windows MIDI Services adapter instance.
    /// </summary>
    private static WindowsMidiServicesAdapter CreateWindowsMidiServicesAdapter()
    {
        var logger = Helpers.LoggingHelper.CreateLogger<WindowsMidiServicesAdapter>();
        return new WindowsMidiServicesAdapter(logger);
    }

    /// <summary>
    /// Checks if the current OS version supports Windows MIDI Services.
    /// </summary>
    /// <returns>True if Windows 11 24H2 (Build 26100) or later is detected</returns>
    public static bool IsWindowsMidiServicesAvailable()
    {
        // Windows 11 24H2+ = Build 26100 or higher
        var version = Environment.OSVersion.Version;
        return version.Major >= 10 && version.Build >= WindowsMidiServicesBuildThreshold;
    }

    /// <summary>
    /// Checks if the Windows MIDI Services SDK Runtime is installed.
    /// </summary>
    /// <returns>True if the SDK Runtime is detected and can be initialized</returns>
    /// <remarks>
    /// Attempts to initialize the SDK Runtime to verify it's properly installed.
    /// This is the most reliable detection method as it validates the entire SDK stack.
    /// </remarks>
    public static bool IsWindowsMidiServicesRuntimeInstalled()
    {
        try
        {
            // Try to initialize the SDK - this will fail if runtime not installed
            using var initializer = MidiDesktopAppSdkInitializer.Create();

            // Create() returns null if the COM object cannot be created (runtime not installed)
            if (initializer == null)
            {
                return false;
            }

            return initializer.InitializeSdkRuntime();
        }
        catch
        {
            // Any exception means the runtime is not available
            return false;
        }
    }

    /// <summary>
    /// Parses a string configuration value to MidiAdapterType.
    /// </summary>
    /// <param name="value">The string value from configuration (e.g., "Auto", "NAudio", "WindowsMidiServices")</param>
    /// <param name="defaultType">The default type to return if parsing fails</param>
    /// <returns>The parsed MidiAdapterType</returns>
    public static MidiAdapterType ParseAdapterType(string? value, MidiAdapterType defaultType = MidiAdapterType.Auto)
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultType;

        if (Enum.TryParse<MidiAdapterType>(value, ignoreCase: true, out var result))
            return result;

        return defaultType;
    }

    /// <summary>
    /// Gets the current MIDI adapter status for user notification purposes.
    /// </summary>
    /// <returns>A status object indicating OS support and runtime installation state</returns>
    public static MidiAdapterStatus GetAdapterStatus()
    {
        return new MidiAdapterStatus
        {
            OsSupportsWindowsMidiServices = IsWindowsMidiServicesAvailable(),
            WindowsMidiServicesRuntimeInstalled = IsWindowsMidiServicesRuntimeInstalled()
        };
    }

    /// <summary>
    /// The download URL for Windows MIDI Services SDK Runtime.
    /// </summary>
    public const string WindowsMidiServicesDownloadUrl = "https://github.com/microsoft/MIDI/releases";
}

/// <summary>
/// Status information about MIDI adapter availability.
/// </summary>
public class MidiAdapterStatus
{
    /// <summary>
    /// True if the OS version supports Windows MIDI Services (Windows 11 24H2+).
    /// </summary>
    public bool OsSupportsWindowsMidiServices { get; init; }

    /// <summary>
    /// True if the Windows MIDI Services SDK Runtime is installed.
    /// </summary>
    public bool WindowsMidiServicesRuntimeInstalled { get; init; }

    /// <summary>
    /// True if Windows MIDI Services can be used (OS supports and runtime installed).
    /// </summary>
    public bool CanUseWindowsMidiServices => OsSupportsWindowsMidiServices && WindowsMidiServicesRuntimeInstalled;

    /// <summary>
    /// True if the OS supports Windows MIDI Services but the runtime is not installed.
    /// This is the case where we should prompt the user to install the runtime.
    /// </summary>
    public bool ShouldPromptForRuntimeInstall => OsSupportsWindowsMidiServices && !WindowsMidiServicesRuntimeInstalled;
}

