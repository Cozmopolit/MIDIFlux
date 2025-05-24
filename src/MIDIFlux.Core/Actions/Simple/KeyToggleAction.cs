using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Unified action for toggling the state of a key (like CapsLock, NumLock, etc.).
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyToggleAction : IUnifiedAction
{
    private readonly ushort _virtualKeyCode;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ILogger _logger;

    /// <summary>
    /// Gets the unique identifier for this action instance
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Gets a human-readable description of this action
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the virtual key code for this action
    /// </summary>
    public ushort VirtualKeyCode => _virtualKeyCode;

    /// <summary>
    /// Initializes a new instance of KeyToggleAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyToggleAction(KeyToggleConfig config)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "KeyToggleConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid KeyToggleConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Toggle Key (VK: {config.VirtualKeyCode})";
        _virtualKeyCode = config.VirtualKeyCode;

        // Initialize keyboard simulator and logger
        _logger = LoggingHelper.CreateLogger<KeyToggleAction>();
        _keyboardSimulator = new KeyboardSimulator(_logger);
    }

    /// <summary>
    /// Executes the key toggle action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing KeyToggleAction: VirtualKeyCode={VirtualKeyCode}, MidiValue={MidiValue}",
                _virtualKeyCode, midiValue);

            // For toggle keys, we need to press and release to toggle the state
            // This works for keys like CapsLock, NumLock, ScrollLock, etc.
            if (!_keyboardSimulator.SendKeyDown(_virtualKeyCode))
            {
                var errorMsg = $"Failed to send key down for toggle key virtual key code {_virtualKeyCode}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", _logger);
                return;
            }

            if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
            {
                var errorMsg = $"Failed to send key up for toggle key virtual key code {_virtualKeyCode}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", _logger);
                return;
            }

            _logger.LogTrace("Successfully executed KeyToggleAction for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing KeyToggleAction for virtual key code {_virtualKeyCode}";
            _logger.LogError(ex, errorMsg);
            ApplicationErrorHandler.ShowError(errorMsg, "MIDIFlux - Error", _logger, ex);
        }
    }

    /// <summary>
    /// Async adapter for the synchronous Execute method.
    /// Uses ValueTask for zero allocation when the operation is synchronous.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A completed ValueTask</returns>
    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        Execute(midiValue);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Returns a string representation of this action
    /// </summary>
    public override string ToString()
    {
        return Description;
    }
}
