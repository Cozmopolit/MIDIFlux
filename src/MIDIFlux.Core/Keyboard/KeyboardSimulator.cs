using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Keyboard;

/// <summary>
/// Simulates keyboard input using the Windows SendInput API
/// </summary>
public class KeyboardSimulator
{
    // Windows API constants
    private const int INPUT_KEYBOARD = 1;
    private const uint KEYEVENTF_KEYUP = 0x0002;
    private const uint KEYEVENTF_EXTENDEDKEY = 0x0001;
    private const uint KEYEVENTF_SCANCODE = 0x0008;
    private const uint KEYEVENTF_UNICODE = 0x0004;

    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardSimulator"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    public KeyboardSimulator(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyboardSimulator"/> class without a logger
    /// </summary>
    public KeyboardSimulator()
    {
        _logger = Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
    }

    // Virtual key codes for special keys
    public const ushort VK_SHIFT = 0x10;
    public const ushort VK_CONTROL = 0x11;
    public const ushort VK_MENU = 0x12; // Alt
    public const ushort VK_LSHIFT = 0xA0;
    public const ushort VK_RSHIFT = 0xA1;
    public const ushort VK_LCONTROL = 0xA2;
    public const ushort VK_RCONTROL = 0xA3;
    public const ushort VK_LMENU = 0xA4; // Left Alt
    public const ushort VK_RMENU = 0xA5; // Right Alt (Alt Gr)
    public const ushort VK_SNAPSHOT = 0x2C; // Print Screen

    /// <summary>
    /// Sends a key down event for the specified virtual key code
    /// </summary>
    /// <param name="keyCode">The virtual key code</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SendKeyDown(ushort keyCode)
    {
        _logger.LogInformation("SendKeyDown: KeyCode={KeyCode} (0x{KeyCodeHex})", keyCode, keyCode.ToString("X2"));

        // Special case for Print Screen key
        if (keyCode == VK_SNAPSHOT)
        {
            _logger.LogDebug("Using special handling for Print Screen key");
            bool printScreenResult = SendPrintScreenDown();
            _logger.LogInformation("SendPrintScreenDown result: {Result}", printScreenResult);
            return printScreenResult;
        }

        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = keyCode;
        bool isExtended = IsExtendedKey(keyCode);
        inputs[0].union.ki.dwFlags = isExtended ? KEYEVENTF_EXTENDEDKEY : 0;
        inputs[0].union.ki.time = 0;
        inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

        _logger.LogDebug("Sending key down: KeyCode={KeyCode}, IsExtended={IsExtended}", keyCode, isExtended);

        uint result = SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT)));
        bool success = result == 1;

        if (!success)
        {
            int error = Marshal.GetLastWin32Error();
            _logger.LogError("SendInput failed for key down: KeyCode={KeyCode}, Error={Error}", keyCode, error);
        }
        else
        {
            _logger.LogDebug("SendInput succeeded for key down: KeyCode={KeyCode}", keyCode);
        }

        return success;
    }

    /// <summary>
    /// Sends a key up event for the specified virtual key code
    /// </summary>
    /// <param name="keyCode">The virtual key code</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SendKeyUp(ushort keyCode)
    {
        // Special case for Print Screen key
        if (keyCode == VK_SNAPSHOT)
        {
            return SendPrintScreenUp();
        }

        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = keyCode;
        inputs[0].union.ki.dwFlags = KEYEVENTF_KEYUP | (IsExtendedKey(keyCode) ? KEYEVENTF_EXTENDEDKEY : 0);
        inputs[0].union.ki.time = 0;
        inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

        return SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 1;
    }

    /// <summary>
    /// Sends a Print Screen key down event
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    private bool SendPrintScreenDown()
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = VK_SNAPSHOT;
        inputs[0].union.ki.dwFlags = KEYEVENTF_EXTENDEDKEY;
        inputs[0].union.ki.time = 0;
        inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

        return SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 1;
    }

    /// <summary>
    /// Sends a Print Screen key up event
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    private bool SendPrintScreenUp()
    {
        var inputs = new INPUT[1];
        inputs[0].type = INPUT_KEYBOARD;
        inputs[0].union.ki.wVk = VK_SNAPSHOT;
        inputs[0].union.ki.dwFlags = KEYEVENTF_KEYUP | KEYEVENTF_EXTENDEDKEY;
        inputs[0].union.ki.time = 0;
        inputs[0].union.ki.dwExtraInfo = IntPtr.Zero;

        return SendInput(1, inputs, Marshal.SizeOf(typeof(INPUT))) == 1;
    }

    /// <summary>
    /// Determines if a key is an extended key
    /// </summary>
    /// <param name="keyCode">The virtual key code</param>
    /// <returns>True if the key is an extended key, false otherwise</returns>
    private bool IsExtendedKey(ushort keyCode)
    {
        // Extended keys include: INS, DEL, HOME, END, PGUP, PGDN, arrow keys,
        // right-side modifier keys (Alt, Ctrl), and the Windows key
        switch (keyCode)
        {
            // Navigation keys
            case 0x21: // Page Up
            case 0x22: // Page Down
            case 0x23: // End
            case 0x24: // Home
            case 0x25: // Left Arrow
            case 0x26: // Up Arrow
            case 0x27: // Right Arrow
            case 0x28: // Down Arrow
            case 0x2D: // Insert
            case 0x2E: // Delete

            // Windows and Menu keys
            case 0x5B: // Left Windows
            case 0x5C: // Right Windows
            case 0x5D: // Applications (Menu)

            // Right-side modifier keys
            case 0xA3: // Right Control
            case 0xA5: // Right Alt (Alt Gr)

            // Media keys
            case 0xAD: // Volume Mute
            case 0xAE: // Volume Down
            case 0xAF: // Volume Up
            case 0xB0: // Next Track
            case 0xB1: // Previous Track
            case 0xB2: // Stop
            case 0xB3: // Play/Pause

            // Numpad keys (when NumLock is off)
            case 0x6F: // Divide
            case 0x6A: // Multiply
            case 0x6D: // Subtract
            case 0x6B: // Add
            case 0x6E: // Decimal

            // Special keys
            case 0x2C: // Print Screen
            case 0x91: // Scroll Lock
            case 0x13: // Pause/Break
                return true;

            default:
                return false;
        }
    }

    #region Windows API Imports and Structures

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

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
