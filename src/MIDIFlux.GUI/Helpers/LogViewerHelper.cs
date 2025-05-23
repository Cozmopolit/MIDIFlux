using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for launching and managing a log viewer
    /// </summary>
    public static class LogViewerHelper
    {
        private static Process? _logViewerProcess;

        /// <summary>
        /// Opens a PowerShell window that displays the log file in real-time
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <returns>True if successful, false otherwise</returns>
        public static bool OpenLogViewer(ILogger logger)
        {
            try
            {
                // Check if a log viewer is already running
                if (_logViewerProcess != null && !_logViewerProcess.HasExited)
                {
                    // Bring the existing window to the front
                    if (_logViewerProcess.MainWindowHandle != IntPtr.Zero)
                    {
                        NativeMethods.SetForegroundWindow(_logViewerProcess.MainWindowHandle);
                    }

                    logger.LogInformation("Log viewer is already running");
                    return true;
                }

                // Get the logs directory
                string logsDirectory = AppDataHelper.GetLogsDirectory();

                // Ensure the logs directory exists
                if (!Directory.Exists(logsDirectory))
                {
                    Directory.CreateDirectory(logsDirectory);
                    logger.LogInformation("Created logs directory: {LogsDirectory}", logsDirectory);
                }

                // Find the most recent log file
                string? logFilePath = null;
                try
                {
                    var logFiles = Directory.GetFiles(logsDirectory, "MIDIFlux*.log")
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .ToArray();

                    if (logFiles.Length > 0)
                    {
                        logFilePath = logFiles[0];
                        logger.LogInformation("Found most recent log file: {LogFilePath}", logFilePath);
                    }
                    else
                    {
                        // If no log files found, create a default one
                        logFilePath = Path.Combine(logsDirectory, "MIDIFlux.log");
                        File.WriteAllText(logFilePath, $"Log file created at {DateTime.Now}\r\n");
                        logger.LogInformation("No log files found, created new log file: {LogFilePath}", logFilePath);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error finding log files");
                    // Fall back to default log file
                    logFilePath = Path.Combine(logsDirectory, "MIDIFlux.log");
                    logger.LogWarning("Using default log file path: {LogFilePath}", logFilePath);
                }

                // Create a PowerShell command to tail the log file
                // Using Get-Content with -Wait to continuously monitor the file
                // Check if the file exists first, and create an empty file if it doesn't
                string psCommand = $@"
if (-not (Test-Path -Path '{logFilePath}' -PathType Leaf)) {{
    New-Item -Path '{logFilePath}' -ItemType File -Force | Out-Null
    Write-Host 'Log file created at {logFilePath}'
}}
Get-Content -Path '{logFilePath}' -Wait -Tail 50";

                // Start PowerShell with the command
                var startInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoExit -Command \"{psCommand}\"",
                    UseShellExecute = true,
                    CreateNoWindow = false,
                    WindowStyle = ProcessWindowStyle.Normal
                };

                // Start the process
                _logViewerProcess = Process.Start(startInfo);

                if (_logViewerProcess == null)
                {
                    logger.LogError("Failed to start log viewer process");
                    return false;
                }

                logger.LogInformation("Opened log viewer for file: {LogFilePath}", logFilePath);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error opening log viewer: {ErrorMessage}", ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Closes the log viewer if it's running
        /// </summary>
        /// <param name="logger">The logger to use</param>
        public static void CloseLogViewer(ILogger logger)
        {
            try
            {
                if (_logViewerProcess != null && !_logViewerProcess.HasExited)
                {
                    _logViewerProcess.Kill();
                    _logViewerProcess = null;
                    logger.LogInformation("Closed log viewer");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error closing log viewer: {ErrorMessage}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Native methods for window management
    /// </summary>
    internal static class NativeMethods
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}

