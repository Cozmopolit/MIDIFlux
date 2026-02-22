using System.Text.Json;

namespace MIDIFlux.App.Mcp;

/// <summary>
/// File-based logging for startup errors with rotation
/// </summary>
internal static class StartupErrorLogger
{
    private const int MaxLogFiles = 10;

    /// <summary>
    /// Write startup diagnostics to JSON file in exe directory with rotation
    /// </summary>
    public static void WriteToFile(StartupDiagnostics diagnostics, string exeDirectory)
    {
        try
        {
            var logDir = Path.Combine(exeDirectory, "startup-errors");
            Directory.CreateDirectory(logDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var logFile = Path.Combine(logDir, $"startup-error-{timestamp}.json");

            var json = JsonSerializer.Serialize(diagnostics, new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            File.WriteAllText(logFile, json);

            // Rotate old files
            RotateLogFiles(logDir);
        }
        catch
        {
            // Silent failure - don't cause recursion or additional errors
        }
    }

    /// <summary>
    /// Keep only the most recent MaxLogFiles files
    /// </summary>
    private static void RotateLogFiles(string logDir)
    {
        try
        {
            var files = Directory.GetFiles(logDir, "startup-error-*.json")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTimeUtc)
                .ToList();

            // Delete files beyond the limit
            foreach (var file in files.Skip(MaxLogFiles))
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // Ignore individual file deletion errors
                }
            }
        }
        catch
        {
            // Silent failure
        }
    }
}

