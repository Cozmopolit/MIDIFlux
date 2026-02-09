using System;
using System.Drawing;
using System.IO;
using System.Reflection;

namespace MIDIFlux.GUI.Helpers;

/// <summary>
/// Helper class for loading application icons from embedded resources
/// </summary>
public static class IconHelper
{
    private static Icon? _applicationIcon;
    private static readonly object _lock = new object();

    /// <summary>
    /// Gets the application icon from embedded resources
    /// </summary>
    /// <returns>The application icon, or null if it cannot be loaded</returns>
    public static Icon? GetApplicationIcon()
    {
        if (_applicationIcon != null)
        {
            return _applicationIcon;
        }

        lock (_lock)
        {
            if (_applicationIcon != null)
            {
                return _applicationIcon;
            }

            try
            {
                // Try to load from MIDIFlux.GUI assembly first
                var assembly = Assembly.GetExecutingAssembly();
                var resourceName = "MIDIFlux.GUI.Assets.MIDIFlux.ico";

                using var stream = assembly.GetManifestResourceStream(resourceName);
                if (stream != null)
                {
                    _applicationIcon = new Icon(stream);
                    return _applicationIcon;
                }

                // Fallback: Try to load from MIDIFlux.App assembly
                var appAssembly = Assembly.Load("MIDIFlux.App");
                resourceName = "MIDIFlux.App.Assets.MIDIFlux.ico";

                using var appStream = appAssembly.GetManifestResourceStream(resourceName);
                if (appStream != null)
                {
                    _applicationIcon = new Icon(appStream);
                    return _applicationIcon;
                }
            }
            catch (Exception)
            {
                // If loading fails, return null
                // The caller should handle this gracefully
            }

            return null;
        }
    }

    /// <summary>
    /// Gets the application icon or a fallback icon if the application icon cannot be loaded
    /// </summary>
    /// <returns>The application icon or a system default icon</returns>
    public static Icon GetApplicationIconOrDefault()
    {
        var icon = GetApplicationIcon();
        if (icon != null)
        {
            return icon;
        }

        // Fallback to application executable icon
        try
        {
            var executableIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
            if (executableIcon != null)
            {
                return executableIcon;
            }
        }
        catch
        {
            // Ignore errors
        }

        // Last resort: Use system application icon
        return SystemIcons.Application;
    }
}

