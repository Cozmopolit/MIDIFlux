namespace MIDIFlux.Core.Hardware;

/// <summary>
/// Specifies which MIDI hardware adapter to use.
/// </summary>
public enum MidiAdapterType
{
    /// <summary>
    /// Automatically select based on OS version and SDK Runtime availability.
    /// Uses WindowsMidiServices on Windows 11 24H2+ with SDK Runtime installed,
    /// otherwise falls back to NAudio.
    /// </summary>
    Auto,

    /// <summary>
    /// Force NAudio adapter (legacy WinMM API).
    /// Use for compatibility or troubleshooting.
    /// </summary>
    NAudio,

    /// <summary>
    /// Force Windows MIDI Services adapter.
    /// Requires Windows 11 24H2+ and SDK Runtime installed.
    /// </summary>
    WindowsMidiServices
}

