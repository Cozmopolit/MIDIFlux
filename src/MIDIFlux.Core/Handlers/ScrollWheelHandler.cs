using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Handlers;

/// <summary>
/// Handles mouse scroll wheel
/// </summary>
[SupportedOSPlatform("windows")]
public class ScrollWheelHandler : IRelativeValueHandler
{
    private readonly ILogger _logger;
    private readonly int _sensitivity;

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => "Mouse Scroll Wheel";

    /// <summary>
    /// Creates a new instance of the ScrollWheelHandler
    /// </summary>
    /// <param name="logger">The logger to use</param>
    /// <param name="sensitivity">The sensitivity multiplier (default: 1)</param>
    public ScrollWheelHandler(ILogger logger, int sensitivity = 1)
    {
        _logger = logger;
        _sensitivity = sensitivity;
    }

    /// <summary>
    /// Handles a relative change from a MIDI control
    /// </summary>
    /// <param name="increment">The relative change (positive or negative)</param>
    public void HandleIncrement(int increment)
    {
        // Apply sensitivity
        int scrollAmount = increment * _sensitivity;

        _logger.LogInformation($"Scrolling wheel by {scrollAmount} units");

        if (OperatingSystem.IsWindows())
        {
            try
            {
                // The wheel delta is typically 120 per notch
                int wheelDelta = scrollAmount * 120;

                // Call the mouse_event function to scroll the wheel
                mouse_event(MOUSEEVENTF_WHEEL, 0, 0, (uint)wheelDelta, IntPtr.Zero);

                _logger.LogDebug($"Sent mouse wheel event with delta {wheelDelta}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send mouse wheel event: {ex.Message}");
            }
        }
        else
        {
            _logger.LogWarning("Mouse wheel control is only supported on Windows");
        }
    }

    #region Windows API Imports

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    private const uint MOUSEEVENTF_WHEEL = 0x0800;

    #endregion
}
