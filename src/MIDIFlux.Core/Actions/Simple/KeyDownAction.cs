using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Unified action for pressing a key down (and optionally auto-releasing it).
/// Implements sync-by-default execution for performance.
/// </summary>
public class KeyDownAction : IUnifiedAction
{
    private readonly ushort _virtualKeyCode;
    private readonly int? _autoReleaseAfterMs;
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
    /// Gets the auto-release time in milliseconds, if configured
    /// </summary>
    public int? AutoReleaseAfterMs => _autoReleaseAfterMs;

    /// <summary>
    /// Initializes a new instance of KeyDownAction
    /// </summary>
    /// <param name="config">The strongly-typed configuration for this action</param>
    /// <param name="actionStateManager">Optional action state manager for state tracking</param>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="ArgumentException">Thrown when config is invalid</exception>
    public KeyDownAction(KeyDownConfig config, ActionStateManager? actionStateManager = null)
    {
        if (config == null)
            throw new ArgumentNullException(nameof(config), "KeyDownConfig cannot be null");

        if (!config.IsValid())
        {
            var errors = config.GetValidationErrors();
            throw new ArgumentException($"Invalid KeyDownConfig: {string.Join(", ", errors)}", nameof(config));
        }

        Id = Guid.NewGuid().ToString();
        var autoReleaseText = config.AutoReleaseAfterMs.HasValue ? $" (auto-release after {config.AutoReleaseAfterMs}ms)" : "";
        Description = config.Description ?? $"Press Key Down (VK: {config.VirtualKeyCode}){autoReleaseText}";
        _virtualKeyCode = config.VirtualKeyCode;
        _autoReleaseAfterMs = config.AutoReleaseAfterMs;
        _actionStateManager = actionStateManager;
        _stateKey = $"*Key{config.VirtualKeyCode}";

        // Initialize keyboard simulator and logger
        _logger = LoggingHelper.CreateLogger<KeyDownAction>();
        _keyboardSimulator = new KeyboardSimulator(_logger);
    }

    /// <summary>
    /// Executes the key down action synchronously.
    /// This is the hot path implementation with no Task overhead.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    public void Execute(int? midiValue = null)
    {
        try
        {
            _logger.LogDebug("Executing KeyDownAction: VirtualKeyCode={VirtualKeyCode}, AutoRelease={AutoRelease}, MidiValue={MidiValue}",
                _virtualKeyCode, _autoReleaseAfterMs, midiValue);

            // Check current state to avoid duplicate key presses
            if (_actionStateManager != null)
            {
                var currentState = _actionStateManager.GetState(_stateKey);
                if (currentState == 1)
                {
                    _logger.LogDebug("Key {VirtualKeyCode} is already pressed (state={State}), skipping key down", _virtualKeyCode, currentState);
                    return;
                }
            }

            // Press the key down
            if (!_keyboardSimulator.SendKeyDown(_virtualKeyCode))
            {
                var errorMsg = $"Failed to send key down for virtual key code {_virtualKeyCode}";
                _logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", _logger);
                return;
            }

            // Update state to pressed
            _actionStateManager?.SetState(_stateKey, 1);

            _logger.LogTrace("Successfully sent key down for VirtualKeyCode={VirtualKeyCode}", _virtualKeyCode);

            // Handle auto-release if specified
            if (_autoReleaseAfterMs.HasValue && _autoReleaseAfterMs.Value > 0)
            {
                // Schedule auto-release on a background thread to avoid blocking
                Task.Run(async () =>
                {
                    try
                    {
                        await Task.Delay(_autoReleaseAfterMs.Value);

                        if (!_keyboardSimulator.SendKeyUp(_virtualKeyCode))
                        {
                            _logger.LogError("Failed to auto-release key for virtual key code {VirtualKeyCode}", _virtualKeyCode);
                        }
                        else
                        {
                            // Update state to released
                            _actionStateManager?.SetState(_stateKey, 0);
                            _logger.LogTrace("Successfully auto-released key for VirtualKeyCode={VirtualKeyCode} after {Delay}ms",
                                _virtualKeyCode, _autoReleaseAfterMs.Value);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error during auto-release for virtual key code {VirtualKeyCode}", _virtualKeyCode);
                    }
                });
            }
        }
        catch (Exception ex)
        {
            var errorMsg = $"Error executing KeyDownAction for virtual key code {_virtualKeyCode}";
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
