using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// action for releasing a key that was previously pressed down.
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyUpAction : IAction
{
    private readonly ushort _virtualKeyCode;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ActionStateManager? _actionStateManager;
    private readonly ILogger _logger;
    private readonly string _stateKey;

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
    /// Initializes a new instance of KeyUpAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional action state manager for state tracking</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyUpAction(KeyUpConfig config, ActionStateManager? actionStateManager = null)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "KeyUpConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid KeyUpConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? $"Release Key (VK: {config.VirtualKeyCode})";
        _virtualKeyCode = config.VirtualKeyCode;
        _actionStateManager = actionStateManager;
        _stateKey = $"*Key{config.VirtualKeyCode}";

        // Initialize keyboard simulator and logger
        _logger = LoggingHelper.CreateLogger<KeyUpAction>();
        _keyboardSimulator = new KeyboardSimulator(_logger);
    }

    /// <summary>
    /// Executes the key release action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing KeyUpAction: VirtualKeyCode={VirtualKeyCode}, MidiValue={MidiValue}",
                _virtualKeyCode, midiValue);

            // Check current state - only release if key is currently pressed
            if (_actionStateManager != null)
            {
                var currentState = _actionStateManager.GetState(_stateKey);
                if (currentState != 1)
                {
                    _logger.LogDebug("Key {VirtualKeyCode} is not pressed (state={State}), skipping key up", _virtualKeyCode, currentState);
                    return;
                }
            }

            // Release the key
            if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
            {
                var errorMsg = $"Failed to send key up for virtual key code {_virtualKeyCode}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", _logger);
                return;
            }

            // Update state to released
            _actionStateManager?.SetState(_stateKey, 0);

            _logger.LogTrace("Successfully executed KeyUpAction for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing KeyUpAction for virtual key code {_virtualKeyCode}";
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
