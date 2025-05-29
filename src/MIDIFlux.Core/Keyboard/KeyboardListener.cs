using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Keyboard;

/// <summary>
/// Event arguments for keyboard events
/// </summary>
public class KeyboardEventArgs : EventArgs
{
    /// <summary>
    /// The key that was pressed
    /// </summary>
    public Keys Key { get; set; }

    /// <summary>
    /// Whether this was a key down event (true) or key up event (false)
    /// </summary>
    public bool IsKeyDown { get; set; }

    /// <summary>
    /// Initializes a new instance of KeyboardEventArgs
    /// </summary>
    /// <param name="key">The key that was pressed</param>
    /// <param name="isKeyDown">Whether this was a key down event</param>
    public KeyboardEventArgs(Keys key, bool isKeyDown)
    {
        Key = key;
        IsKeyDown = isKeyDown;
    }
}

/// <summary>
/// Service for listening to global keyboard input using Windows API hooks.
/// Used for key detection in the profile editor GUI.
/// </summary>
public class KeyboardListener : IDisposable
{
    #region Windows API Constants and Delegates

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    #endregion

    #region Fields and Properties

    private readonly ILogger _logger;
    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookID = IntPtr.Zero;
    private bool _isListening = false;
    private bool _disposed = false;

    /// <summary>
    /// Event raised when a key is pressed or released
    /// </summary>
    public event EventHandler<KeyboardEventArgs>? KeyboardEvent;

    /// <summary>
    /// Gets whether the keyboard listener is currently active
    /// </summary>
    public bool IsListening => _isListening;

    #endregion

    #region Constructor and Initialization

    /// <summary>
    /// Initializes a new instance of the KeyboardListener
    /// </summary>
    /// <param name="logger">Logger for error handling</param>
    public KeyboardListener(ILogger logger)
    {
        _logger = logger;
        _proc = HookCallback;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts listening for keyboard events
    /// </summary>
    /// <returns>True if listening started successfully, false otherwise</returns>
    public bool StartListening()
    {
        if (_disposed)
        {
            _logger.LogWarning("Cannot start listening on disposed KeyboardListener");
            return false;
        }

        if (_isListening)
        {
            _logger.LogWarning("KeyboardListener is already listening");
            return true;
        }

        try
        {
            _hookID = SetHook(_proc);
            if (_hookID != IntPtr.Zero)
            {
                _isListening = true;
                _logger.LogDebug("Started keyboard listening");
                return true;
            }
            else
            {
                _logger.LogError("Failed to set keyboard hook");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting keyboard listening");
            return false;
        }
    }

    /// <summary>
    /// Stops listening for keyboard events
    /// </summary>
    public void StopListening()
    {
        if (!_isListening)
            return;

        try
        {
            if (_hookID != IntPtr.Zero)
            {
                UnhookWindowsHookEx(_hookID);
                _hookID = IntPtr.Zero;
            }

            _isListening = false;
            _logger.LogDebug("Stopped keyboard listening");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping keyboard listening");
        }
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Sets up the low-level keyboard hook
    /// </summary>
    /// <param name="proc">The callback procedure</param>
    /// <returns>Handle to the hook</returns>
    private IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule? curModule = curProcess.MainModule)
        {
            if (curModule?.ModuleName == null)
            {
                _logger.LogError("Could not get current module name for keyboard hook");
                return IntPtr.Zero;
            }

            return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    /// <summary>
    /// Callback procedure for the keyboard hook
    /// </summary>
    /// <param name="nCode">Hook code</param>
    /// <param name="wParam">Message identifier</param>
    /// <param name="lParam">Pointer to keyboard input structure</param>
    /// <returns>Result of calling next hook</returns>
    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        try
        {
            if (nCode >= 0 && _isListening)
            {
                bool isKeyDown = wParam.ToInt32() == WM_KEYDOWN || wParam.ToInt32() == WM_SYSKEYDOWN;
                bool isKeyUp = wParam.ToInt32() == WM_KEYUP || wParam.ToInt32() == WM_SYSKEYUP;

                if (isKeyDown || isKeyUp)
                {
                    var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                    var key = (Keys)hookStruct.vkCode;

                    _logger.LogTrace("Keyboard event: Key={Key}, IsKeyDown={IsKeyDown}", key, isKeyDown);

                    // Raise the event
                    KeyboardEvent?.Invoke(this, new KeyboardEventArgs(key, isKeyDown));
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in keyboard hook callback");
        }

        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    #endregion

    #region IDisposable Implementation

    /// <summary>
    /// Disposes the keyboard listener and unhooks the keyboard hook
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Protected dispose method
    /// </summary>
    /// <param name="disposing">Whether disposing from Dispose() call</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                StopListening();
            }

            _disposed = true;
        }
    }

    /// <summary>
    /// Finalizer to ensure cleanup
    /// </summary>
    ~KeyboardListener()
    {
        Dispose(false);
    }

    #endregion
}
