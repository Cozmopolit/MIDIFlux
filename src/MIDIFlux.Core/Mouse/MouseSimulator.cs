using System.Runtime.InteropServices;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Models;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Mouse;

/// <summary>
/// Simulates mouse input using the Windows SendInput API and mouse_event API
/// </summary>
public class MouseSimulator
{
    // Windows API constants for SendInput
    private const int INPUT_MOUSE = 0;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;
    private const uint MOUSEEVENTF_MIDDLEDOWN = 0x0020;
    private const uint MOUSEEVENTF_MIDDLEUP = 0x0040;
    private const uint MOUSEEVENTF_WHEEL = 0x0800;
    private const uint MOUSEEVENTF_HWHEEL = 0x01000;

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseSimulator"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    public MouseSimulator(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MouseSimulator"/> class without a logger
    /// </summary>
    public MouseSimulator()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }

    /// <summary>
    /// Sends a mouse click for the specified button
    /// </summary>
    /// <param name="button">The mouse button to click</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SendMouseClick(MouseButton button)
    {
        _logger.LogInformation("SendMouseClick: Button={Button}", button);

        try
        {
            // Get the down and up flags for the button
            var (downFlag, upFlag) = GetMouseButtonFlags(button);

            // Create input for mouse down
            var inputs = new INPUT[2];

            // Mouse down
            inputs[0].type = INPUT_MOUSE;
            inputs[0].union.mi.dx = 0;
            inputs[0].union.mi.dy = 0;
            inputs[0].union.mi.mouseData = 0;
            inputs[0].union.mi.dwFlags = downFlag;
            inputs[0].union.mi.time = 0;
            inputs[0].union.mi.dwExtraInfo = IntPtr.Zero;

            // Mouse up
            inputs[1].type = INPUT_MOUSE;
            inputs[1].union.mi.dx = 0;
            inputs[1].union.mi.dy = 0;
            inputs[1].union.mi.mouseData = 0;
            inputs[1].union.mi.dwFlags = upFlag;
            inputs[1].union.mi.time = 0;
            inputs[1].union.mi.dwExtraInfo = IntPtr.Zero;

            _logger.LogDebug("Sending mouse click: Button={Button}, DownFlag={DownFlag}, UpFlag={UpFlag}",
                button, downFlag, upFlag);

            uint result = SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
            bool success = result == 2;

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                _logger.LogError("SendInput failed for mouse click: Button={Button}, Error={Error}", button, error);
            }
            else
            {
                _logger.LogDebug("SendInput succeeded for mouse click: Button={Button}", button);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SendMouseClick for Button={Button}", button);
            return false;
        }
    }

    /// <summary>
    /// Sends a mouse scroll in the specified direction
    /// </summary>
    /// <param name="direction">The scroll direction</param>
    /// <param name="amount">The number of scroll steps</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SendMouseScroll(ScrollDirection direction, int amount)
    {
        _logger.LogInformation("SendMouseScroll: Direction={Direction}, Amount={Amount}", direction, amount);

        if (!OperatingSystem.IsWindows())
        {
            _logger.LogWarning("Mouse scroll is only supported on Windows");
            return false;
        }

        try
        {
            // Calculate wheel delta based on direction and amount
            // Standard wheel delta is 120 per notch
            int wheelDelta = amount * 120;
            uint flags;

            switch (direction)
            {
                case ScrollDirection.Up:
                    flags = MOUSEEVENTF_WHEEL;
                    // Positive delta for up
                    break;
                case ScrollDirection.Down:
                    flags = MOUSEEVENTF_WHEEL;
                    wheelDelta = -wheelDelta; // Negative delta for down
                    break;
                case ScrollDirection.Left:
                    flags = MOUSEEVENTF_HWHEEL;
                    wheelDelta = -wheelDelta; // Negative delta for left
                    break;
                case ScrollDirection.Right:
                    flags = MOUSEEVENTF_HWHEEL;
                    // Positive delta for right
                    break;
                default:
                    _logger.LogError("Unsupported scroll direction: {Direction}", direction);
                    return false;
            }

            _logger.LogDebug("Sending mouse scroll: Direction={Direction}, WheelDelta={WheelDelta}, Flags={Flags}",
                direction, wheelDelta, flags);

            // Use mouse_event for scrolling (consistent with existing ScrollWheelHandler)
            // Note: wheelDelta can be negative, so we pass it as signed value cast to uint
            mouse_event(flags, 0, 0, (uint)wheelDelta, IntPtr.Zero);

            _logger.LogDebug("Successfully sent mouse scroll: Direction={Direction}, Amount={Amount}", direction, amount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception in SendMouseScroll for Direction={Direction}, Amount={Amount}",
                direction, amount);
            return false;
        }
    }

    /// <summary>
    /// Gets the mouse button flags for down and up events
    /// </summary>
    /// <param name="button">The mouse button</param>
    /// <returns>A tuple containing the down flag and up flag</returns>
    private static (uint downFlag, uint upFlag) GetMouseButtonFlags(MouseButton button)
    {
        return button switch
        {
            MouseButton.Left => (MOUSEEVENTF_LEFTDOWN, MOUSEEVENTF_LEFTUP),
            MouseButton.Right => (MOUSEEVENTF_RIGHTDOWN, MOUSEEVENTF_RIGHTUP),
            MouseButton.Middle => (MOUSEEVENTF_MIDDLEDOWN, MOUSEEVENTF_MIDDLEUP),
            _ => throw new ArgumentException($"Unsupported mouse button: {button}", nameof(button))
        };
    }

    #region Windows API Imports and Structures

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, int dx, int dy, uint dwData, IntPtr dwExtraInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct INPUT
    {
        public int type;
        public InputUnion union;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MOUSEINPUT mi;
        [FieldOffset(0)]
        public KEYBDINPUT ki;
        [FieldOffset(0)]
        public HARDWAREINPUT hi;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MOUSEINPUT
    {
        public int dx;
        public int dy;
        public uint mouseData;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KEYBDINPUT
    {
        public ushort wVk;
        public ushort wScan;
        public uint dwFlags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HARDWAREINPUT
    {
        public uint uMsg;
        public ushort wParamL;
        public ushort wParamH;
    }

    #endregion
}
