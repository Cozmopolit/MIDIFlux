using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace MIDIFlux.App.Mcp;

/// <summary>
/// Startup error diagnostic information returned by get_startup_error tool
/// </summary>
public sealed class StartupDiagnostics
{
    [JsonPropertyName("summary")]
    public string Summary { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public List<string> Messages { get; set; } = new();

    [JsonPropertyName("firstFaultingComponent")]
    public string? FirstFaultingComponent { get; set; }

    [JsonPropertyName("timestamp")]
    public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

    [JsonPropertyName("environment")]
    public EnvironmentInfo Environment { get; set; } = new();

    /// <summary>
    /// Create diagnostics from an exception
    /// </summary>
    public static StartupDiagnostics FromException(Exception ex, string exeDirectory)
    {
        var diagnostics = new StartupDiagnostics
        {
            Summary = RedactSensitiveData(ex.Message),
            Timestamp = DateTime.UtcNow.ToString("O"),
            Environment = EnvironmentInfo.Capture(exeDirectory)
        };

        // Build message chain (outer → inner)
        var messages = new List<string>();
        var current = ex;
        int depth = 0;
        while (current != null && depth < 5)
        {
            var msg = RedactSensitiveData(current.Message);
            if (!string.IsNullOrWhiteSpace(msg))
            {
                messages.Add($"{current.GetType().Name}: {msg}");
            }
            current = current.InnerException;
            depth++;
        }
        diagnostics.Messages = messages;

        // Extract first faulting component from stack trace
        diagnostics.FirstFaultingComponent = ExtractFirstFaultingComponent(ex);

        return diagnostics;
    }

    /// <summary>
    /// Redact sensitive data from strings (connection strings, tokens, API keys, passwords)
    /// </summary>
    private static string RedactSensitiveData(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        // Redact password patterns
        var result = Regex.Replace(input, @"(Password|Pwd|pwd)\s*=\s*[^;]+", "$1=REDACTED", RegexOptions.IgnoreCase);
        
        // Redact API key patterns
        result = Regex.Replace(result, @"(ApiKey|API_KEY|api-key|apikey)\s*[:=]\s*[^\s;,]+", "$1=REDACTED", RegexOptions.IgnoreCase);
        
        // Redact token patterns
        result = Regex.Replace(result, @"(Token|token|Bearer)\s+[A-Za-z0-9\-._~+/]+=*", "$1 REDACTED", RegexOptions.IgnoreCase);

        return result;
    }

    /// <summary>
    /// Extract the first faulting component from exception stack trace
    /// </summary>
    private static string? ExtractFirstFaultingComponent(Exception ex)
    {
        if (string.IsNullOrWhiteSpace(ex.StackTrace))
            return null;

        // Try to find first MIDIFlux component in stack trace
        var lines = ex.StackTrace.Split('\n');
        foreach (var line in lines)
        {
            var match = Regex.Match(line, @"at\s+MIDIFlux\.(\w+)\.(\w+)");
            if (match.Success)
            {
                return match.Groups[2].Value; // Return class name
            }
        }

        return null;
    }
}

/// <summary>
/// Environment information for diagnostics
/// </summary>
public sealed class EnvironmentInfo
{
    [JsonPropertyName("os")]
    public string OS { get; set; } = string.Empty;

    [JsonPropertyName("dotnet")]
    public string DotNet { get; set; } = string.Empty;

    [JsonPropertyName("exeDir")]
    public string ExeDir { get; set; } = string.Empty;

    public static EnvironmentInfo Capture(string exeDirectory)
    {
        return new EnvironmentInfo
        {
            OS = $"{System.Environment.OSVersion.Platform} {System.Environment.OSVersion.Version} {(System.Environment.Is64BitOperatingSystem ? "x64" : "x86")}",
            DotNet = System.Environment.Version.ToString(),
            ExeDir = exeDirectory
        };
    }
}

