using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Midi;
using MIDIFlux.GUI.Services;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for diagnosing MIDI processing issues
    /// </summary>
    public static class MidiDiagnosticHelper
    {
        /// <summary>
        /// Generates a comprehensive diagnostic report of the current MIDI system status
        /// </summary>
        /// <param name="serviceProxy">The MIDI processing service proxy</param>
        /// <param name="logger">The logger to use</param>
        /// <returns>A formatted diagnostic report</returns>
        public static string GenerateDiagnosticReport(MidiProcessingServiceProxy serviceProxy, ILogger logger)
        {
            var report = new StringBuilder();
            report.AppendLine("=== MIDIFlux Diagnostic Report ===");
            report.AppendLine($"Generated at: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            report.AppendLine();

            try
            {
                // Check service availability
                report.AppendLine("1. Service Status:");
                bool serviceAvailable = serviceProxy.IsServiceAvailable();
                report.AppendLine($"   Service Available: {serviceAvailable}");

                if (serviceAvailable)
                {
                    // Check active configuration
                    string? activeConfig = serviceProxy.GetActiveConfigurationPath();
                    report.AppendLine($"   Active Configuration: {activeConfig ?? "None"}");

                    // Note: Cannot directly check if processing is running from proxy
                    // This would require additional service function setup
                    report.AppendLine($"   Processing Status: Check main application logs");
                }
                else
                {
                    report.AppendLine("   ❌ Service functions not configured!");
                    report.AppendLine("   This means the Configuration GUI is not connected to MIDIFlux.exe");
                    report.AppendLine("   Please ensure:");
                    report.AppendLine("   - MIDIFlux.exe is running");
                    report.AppendLine("   - Open this GUI from the system tray menu (not standalone)");
                }
                report.AppendLine();

                // Check MIDI devices
                report.AppendLine("2. MIDI Devices:");
                if (serviceAvailable)
                {
                    var devices = serviceProxy.GetAvailableMidiDevices();
                    if (devices.Count == 0)
                    {
                        report.AppendLine("   No MIDI devices found");
                    }
                    else
                    {
                        report.AppendLine($"   Found {devices.Count} MIDI device(s):");
                        foreach (var device in devices)
                        {
                            report.AppendLine($"   - ID: {device.DeviceId}, Name: {device.Name}, Connected: {device.IsConnected}");
                        }
                    }
                }
                else
                {
                    report.AppendLine("   ❌ Cannot check MIDI devices - service not connected");
                }
                report.AppendLine();

                // Check MIDI manager
                report.AppendLine("3. MIDI Manager:");
                if (serviceAvailable)
                {
                    var midiManager = serviceProxy.GetMidiManager();
                    if (midiManager != null)
                    {
                        var activeDeviceIds = midiManager.ActiveDeviceIds;
                        report.AppendLine($"   Active Device IDs: [{string.Join(", ", activeDeviceIds)}]");

                        var availableDevices = midiManager.GetAvailableDevices();
                        report.AppendLine($"   Available Devices: {availableDevices.Count}");
                    }
                    else
                    {
                        report.AppendLine("   ❌ MIDI Manager not available from service");
                    }
                }
                else
                {
                    report.AppendLine("   ❌ Cannot check MIDI Manager - service not connected");
                }
                report.AppendLine();

                // Log level check
                report.AppendLine("4. Logging:");
                report.AppendLine($"   Logger Category: {logger.GetType().Name}");
                report.AppendLine($"   Debug Enabled: {logger.IsEnabled(LogLevel.Debug)}");
                report.AppendLine($"   Information Enabled: {logger.IsEnabled(LogLevel.Information)}");
                report.AppendLine();

                // Recommendations
                report.AppendLine("5. Recommendations:");
                if (!serviceAvailable)
                {
                    report.AppendLine("   🔧 CRITICAL: Service connection missing!");
                    report.AppendLine("   - Close this Configuration GUI");
                    report.AppendLine("   - Ensure MIDIFlux.exe is running (check system tray)");
                    report.AppendLine("   - Right-click MIDIFlux system tray icon → 'Configuration'");
                    report.AppendLine("   - Do NOT run MIDIFlux.GUI.exe directly");
                }
                else if (string.IsNullOrEmpty(serviceProxy.GetActiveConfigurationPath()))
                {
                    report.AppendLine("   📁 No active configuration. Load a profile to enable MIDI processing.");
                }
                else
                {
                    var devices = serviceProxy.GetAvailableMidiDevices();
                    if (devices.Count == 0)
                    {
                        report.AppendLine("   🎹 No MIDI devices found. Check device connections and drivers.");
                    }
                    else
                    {
                        report.AppendLine("   ✅ System appears to be configured correctly.");
                        report.AppendLine("   - If MIDI events are not working, check the profile mappings.");
                        report.AppendLine("   - Try the 'MIDI Input Detection' dialog to test MIDI reception.");
                    }
                }
            }
            catch (Exception ex)
            {
                report.AppendLine($"Error generating diagnostic report: {ex.Message}");
                logger.LogError(ex, "Error generating MIDI diagnostic report");
            }

            return report.ToString();
        }

        /// <summary>
        /// Logs a diagnostic report to the specified logger
        /// </summary>
        /// <param name="serviceProxy">The MIDI processing service proxy</param>
        /// <param name="logger">The logger to use</param>
        public static void LogDiagnosticReport(MidiProcessingServiceProxy serviceProxy, ILogger logger)
        {
            string report = GenerateDiagnosticReport(serviceProxy, logger);
            logger.LogInformation("MIDI Diagnostic Report:\n{Report}", report);
        }

        /// <summary>
        /// Performs a quick health check of the MIDI system
        /// </summary>
        /// <param name="serviceProxy">The MIDI processing service proxy</param>
        /// <returns>True if the system appears healthy, false otherwise</returns>
        public static bool PerformHealthCheck(MidiProcessingServiceProxy serviceProxy)
        {
            try
            {
                // Check basic service availability
                if (!serviceProxy.IsServiceAvailable())
                    return false;

                // Check if we have an active configuration
                if (string.IsNullOrEmpty(serviceProxy.GetActiveConfigurationPath()))
                    return false;

                // Check if we have MIDI devices
                var devices = serviceProxy.GetAvailableMidiDevices();
                if (devices.Count == 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
