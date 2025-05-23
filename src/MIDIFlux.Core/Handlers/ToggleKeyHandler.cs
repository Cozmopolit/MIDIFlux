using MIDIFlux.Core.Interfaces;
using MIDIFlux.Core.Keyboard;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Handlers;

/// <summary>
/// Handles toggling a key state when a MIDI note is pressed
/// </summary>
public class ToggleKeyHandler : INoteHandler
{
    private readonly KeyStateManager _keyStateManager;
    private readonly ILogger _logger;
    private readonly ushort _virtualKeyCode;
    private readonly List<ushort> _modifiers;

    /// <summary>
    /// Creates a new instance of the ToggleKeyHandler
    /// </summary>
    /// <param name="keyStateManager">The key state manager</param>
    /// <param name="logger">The logger to use</param>
    /// <param name="virtualKeyCode">The virtual key code to toggle</param>
    /// <param name="modifiers">Optional modifier keys</param>
    public ToggleKeyHandler(
        KeyStateManager keyStateManager,
        ILogger logger,
        ushort virtualKeyCode,
        List<ushort>? modifiers = null)
    {
        _keyStateManager = keyStateManager;
        _logger = logger;
        _virtualKeyCode = virtualKeyCode;
        _modifiers = modifiers ?? new List<ushort>();
    }

    /// <summary>
    /// Gets a description of this handler for UI and logging
    /// </summary>
    public string Description => $"Toggle Key: {_virtualKeyCode}";

    /// <summary>
    /// Handles a note on event by toggling the key state
    /// </summary>
    /// <param name="velocity">The note velocity (0-127)</param>
    public void HandleNoteOn(int velocity)
    {
        _logger.LogDebug("Note On event received for toggle key {KeyCode}", _virtualKeyCode);
        // Toggle the key state
        _keyStateManager.ToggleKey(_virtualKeyCode, _modifiers);
    }

    /// <summary>
    /// Handles a note off event (does nothing for toggle keys)
    /// </summary>
    public void HandleNoteOff()
    {
        // Do nothing for note off events in toggle mode
        _logger.LogDebug("Note Off event ignored for toggle key {KeyCode}", _virtualKeyCode);
    }
}
