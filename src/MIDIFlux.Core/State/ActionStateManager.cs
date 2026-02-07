using System.Collections.Concurrent;
using MIDIFlux.Core.Keyboard;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.State;

/// <summary>
/// Thread-safe unified state manager for both user-defined and auto-generated internal states.
/// Replaces the old KeyStateManager and provides stateful action support.
/// </summary>
public class ActionStateManager
{
    private readonly ConcurrentDictionary<string, int> _states = new();
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ILogger _logger;

    /// <summary>
    /// Lock for compound operations (ClearAllStates, InitializeStates) that require atomicity
    /// beyond what ConcurrentDictionary provides for individual operations.
    /// </summary>
    private readonly object _compoundOperationLock = new();

    public ActionStateManager(KeyboardSimulator keyboardSimulator, ILogger<ActionStateManager> logger)
    {
        _keyboardSimulator = keyboardSimulator;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current state for a state key. Returns -1 if state doesn't exist.
    /// State semantics: -1 = not defined, 0 = not pressed/inactive, 1 = pressed/active
    /// </summary>
    public int GetState(string stateKey)
    {
        return _states.GetValueOrDefault(stateKey, -1);
    }

    /// <summary>
    /// Sets the state for a state key. Creates the state if it doesn't exist.
    /// </summary>
    public void SetState(string stateKey, int state)
    {
        ValidateStateKey(stateKey);
        _states[stateKey] = state;
        _logger.LogTrace("Set state for {StateKey}: {State}", stateKey, state);
    }

    /// <summary>
    /// Clears state for a specific state key
    /// </summary>
    public void ClearState(string stateKey)
    {
        if (_states.TryRemove(stateKey, out var removedState))
        {
            _logger.LogDebug("Cleared state for {StateKey}: {State}", stateKey, removedState);
        }
    }

    /// <summary>
    /// Clears all states and releases all pressed keys (called on profile change).
    /// This operation is atomic with respect to other compound operations.
    /// </summary>
    public void ClearAllStates()
    {
        lock (_compoundOperationLock)
        {
            // Release all pressed keys before clearing states
            ReleaseAllPressedKeysInternal();

            var count = _states.Count;
            _states.Clear();
            _logger.LogInformation("Cleared all states ({Count} entries)", count);
        }
    }

    /// <summary>
    /// Releases all pressed keys (internal states with value 1) and marks them as released.
    /// This is a public entry point that acquires the compound operation lock.
    /// </summary>
    public void ReleaseAllPressedKeys()
    {
        lock (_compoundOperationLock)
        {
            ReleaseAllPressedKeysInternal();
        }
    }

    /// <summary>
    /// Internal implementation of ReleaseAllPressedKeys. Must be called while holding _compoundOperationLock.
    /// </summary>
    private void ReleaseAllPressedKeysInternal()
    {
        var pressedKeys = _states.Where(s => s.Key.StartsWith("*Key") && s.Value == 1).ToList();

        foreach (var (stateKey, _) in pressedKeys)
        {
            var keyCodeStr = stateKey.Substring(4); // Remove "*Key" prefix
            if (ushort.TryParse(keyCodeStr, out var keyCode))
            {
                _keyboardSimulator.SendKeyUp(keyCode);
                _states[stateKey] = 0; // Mark as released but keep in dictionary
                _logger.LogDebug("Released pressed key {KeyCode}", keyCode);
            }
        }

        if (pressedKeys.Count > 0)
        {
            _logger.LogInformation("Released {Count} pressed keys", pressedKeys.Count);
        }
    }

    /// <summary>
    /// Initializes states from profile configuration. Only user-defined states are initialized.
    /// Internal states (starting with *) are rejected to prevent profile injection attacks.
    /// </summary>
    public void InitializeStates(Dictionary<string, int> initialStates)
    {
        // Clear all states and release keys
        ClearAllStates();

        // Initialize only user-defined states from profile
        foreach (var (stateKey, initialValue) in initialStates)
        {
            ValidateUserStateKey(stateKey); // Explicitly rejects internal states starting with *
            SetState(stateKey, initialValue);
        }

        _logger.LogInformation("Initialized {Count} user-defined states from profile configuration", initialStates.Count);
    }

    /// <summary>
    /// Validates state key format for any state (internal or user-defined).
    /// Internal states (*Key...) and user-defined states have different rules.
    /// </summary>
    private void ValidateStateKey(string stateKey)
    {
        if (string.IsNullOrWhiteSpace(stateKey))
            throw new ArgumentException("State key cannot be null or whitespace", nameof(stateKey));

        if (stateKey.StartsWith("*"))
        {
            // Internal state validation: *Key{digits} format
            if (!stateKey.StartsWith("*Key") || stateKey.Length <= 4)
                throw new ArgumentException($"Internal state key '{stateKey}' must follow '*Key{{VirtualKeyCode}}' format", nameof(stateKey));

            var keyCodePart = stateKey.Substring(4);
            if (!keyCodePart.All(char.IsDigit))
                throw new ArgumentException($"Internal state key '{stateKey}' must have numeric virtual key code after '*Key'", nameof(stateKey));
        }
        else
        {
            ValidateUserStateKeyFormat(stateKey);
        }
    }

    /// <summary>
    /// Validates that a state key is a valid user-defined key (not internal).
    /// Used by InitializeStates to prevent profile injection of internal states.
    /// </summary>
    private void ValidateUserStateKey(string stateKey)
    {
        if (string.IsNullOrWhiteSpace(stateKey))
            throw new ArgumentException("State key cannot be null or whitespace", nameof(stateKey));

        // Explicitly reject internal state keys - profiles must not inject internal states
        if (stateKey.StartsWith("*"))
            throw new ArgumentException($"State key '{stateKey}' is reserved for internal use. User-defined state keys must not start with '*'", nameof(stateKey));

        ValidateUserStateKeyFormat(stateKey);
    }

    /// <summary>
    /// Validates the format of a user-defined state key.
    /// Allows alphanumeric characters and underscores.
    /// </summary>
    private void ValidateUserStateKeyFormat(string stateKey)
    {
        // User-defined state validation: alphanumeric and underscores allowed
        if (!stateKey.All(c => char.IsLetterOrDigit(c) || c == '_'))
            throw new ArgumentException($"User-defined state key '{stateKey}' must contain only alphanumeric characters and underscores", nameof(stateKey));
    }

    /// <summary>
    /// Gets statistics about current state usage
    /// </summary>
    public StateManagerStatistics GetStatistics()
    {
        var userStates = _states.Count(s => !s.Key.StartsWith("*"));
        var internalStates = _states.Count(s => s.Key.StartsWith("*"));

        return new StateManagerStatistics
        {
            ActiveStates = _states.Count,
            UserDefinedStates = userStates,
            InternalStates = internalStates,
            MemoryUsage = _states.Count * 64 // Rough estimation
        };
    }
}

/// <summary>
/// Statistics about state manager usage
/// </summary>
public class StateManagerStatistics
{
    public int ActiveStates { get; set; }
    public int UserDefinedStates { get; set; }
    public int InternalStates { get; set; }
    public long MemoryUsage { get; set; }
}
