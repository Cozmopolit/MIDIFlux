# MIDIFlux Stateful Action System Design

This document outlines the design and implementation of an optional stateful action system for MIDIFlux, enabling actions that maintain integer state between MIDI triggers.

## Design Principles

### 1. **Optional by Design**
- **Default**: Stateless actions cover 90% of use cases
- **Opt-in**: Only specific action types require state management
- **Performance**: No state overhead for stateless actions

### 2. **Profile-Scoped State Only**
- **No Persistence**: State is cleared on application restart
- **Profile Isolation**: State is cleared when switching profiles
- **Rationale**: External device states (MIDI controllers, applications) are unknown between sessions
- **Clean Slate**: Each profile session starts with configured initial state

### 3. **Simple Integer State Model**
- **Integer Values Only**: State is a simple integer per state key
- **Simple Comparisons**: Only equals, greater than, and less than operations
- **Dual State Types**: User-defined states and auto-generated internal states
- **On-Demand Creation**: States only created when explicitly set by actions

### 4. **Unified State Management**
- **Single State Manager**: Replaces both old KeyStateManager and new stateful action needs
- **Auto-Generated Internal States**: Keyboard key tracking using "*Key{VirtualKeyCode}" format
- **User-Defined States**: Profile-configured states for stateful actions
- **Clear Separation**: Asterisk prefix distinguishes internal from user states

### 5. **Single Action Interface**
- **IUnifiedAction Only**: All actions implement the same interface
- **No Duplicates**: State complexity stays within action implementations
- **Consistent**: Same execution pattern for stateful and stateless actions

### 6. **Thread-Safe and Performant**
- **Concurrent Access**: Multiple MIDI events can trigger simultaneously
- **Lock-Free Reads**: Fast state access for hot path
- **Minimal Allocation**: On-demand state creation, no pre-population

## Core Architecture

### Unified Integer State Manager

```csharp
/// <summary>
/// Thread-safe unified state manager for both user-defined and auto-generated internal states.
/// Replaces the old KeyStateManager and provides stateful action support.
/// </summary>
public class ActionStateManager
{
    private readonly ConcurrentDictionary<string, int> _states = new();
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly ILogger _logger;

    public ActionStateManager(KeyboardSimulator keyboardSimulator, ILogger logger)
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
    /// Clears all states and releases all pressed keys (called on profile change)
    /// </summary>
    public void ClearAllStates()
    {
        // Release all pressed keys before clearing states
        ReleaseAllPressedKeys();

        var count = _states.Count;
        _states.Clear();
        _logger.LogInformation("Cleared all states ({Count} entries)", count);
    }

    /// <summary>
    /// Releases all pressed keys (internal states with value 1) and marks them as released
    /// </summary>
    public void ReleaseAllPressedKeys()
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
    /// </summary>
    public void InitializeStates(Dictionary<string, int> initialStates)
    {
        // Clear all states and release keys
        ClearAllStates();

        // Initialize only user-defined states from profile
        foreach (var (stateKey, initialValue) in initialStates)
        {
            ValidateStateKey(stateKey); // Will reject internal states starting with *
            SetState(stateKey, initialValue);
        }

        _logger.LogInformation("Initialized {Count} user-defined states from profile configuration", initialStates.Count);
    }

    /// <summary>
    /// Validates state key format. Internal states (*Key...) and user-defined states have different rules.
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
            // User-defined state validation: alphanumeric only
            if (!stateKey.All(char.IsLetterOrDigit))
                throw new ArgumentException($"User-defined state key '{stateKey}' must contain only alphanumeric characters", nameof(stateKey));
        }
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
```

## Stateful Action Types

### State-Conditional Actions

**Primary Use Cases:**
- Toggle between Play/Pause commands based on current state
- Multi-mode buttons (Record/Play/Stop with different behaviors per mode)
- Progressive actions (1st press = A, 2nd press = B, 3rd press = C, reset)
- Context-sensitive behaviors based on application state

**Configuration Example - Play/Pause Toggle:**
```json
{
  "Action": {
    "$type": "StateConditionalActionConfig",
    "StateKey": "PlaybackMode",
    "Condition": {
      "StateValue": 0,
      "Comparison": "Equals",
      "Action": {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 32,
        "Description": "Play (Spacebar)"
      },
      "SetStateAfter": 1,
      "Description": "Start playback when stopped"
    }
  }
}
```

**Note**: For toggle behavior, you would need two separate mappings - one for state 0→1 (play) and another for state 1→0 (pause), or use a more complex action that handles both cases internally.

**Configuration Example - Progressive Counter (First Press):**
```json
{
  "Action": {
    "$type": "StateConditionalActionConfig",
    "StateKey": "ButtonPressCount",
    "Condition": {
      "StateValue": 0,
      "Comparison": "Equals",
      "Action": {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 65,
        "Description": "Press A"
      },
      "SetStateAfter": 1,
      "Description": "First press: A"
    }
  }
}
```

**Note**: Progressive counter behavior requires multiple mappings on the same MIDI input - one for each state transition (0→1, 1→2, >1→0).

## Integration with MIDIFlux Architecture

### EventDispatcher Integration

```csharp
public class EventDispatcher
{
    private readonly ActionStateManager _actionStateManager; // ← REPLACES KeyStateManager

    public void SetConfiguration(UnifiedMappingConfig configuration)
    {
        // Initialize states from profile configuration (also releases all keys and clears states)
        if (configuration.InitialStates != null)
        {
            _actionStateManager.InitializeStates(configuration.InitialStates);
        }
        else
        {
            _actionStateManager.ClearAllStates();
        }
    }

    public void HandleMidiEvent(MidiEventArgs eventArgs)
    {
        var mappings = FindMatchingMappings(eventArgs);

        foreach (var mapping in mappings)
        {
            // All actions implement IUnifiedAction - no special handling needed
            // Stateful actions internally access _actionStateManager via DI
            // Keyboard actions automatically use internal state keys like "*Key65"
            mapping.Action.ExecuteAsync(eventArgs.Event.Velocity);
        }
    }
}
```

### Dependency Injection Setup

```csharp
// In Program.cs or DI container setup
services.AddSingleton<ActionStateManager>();

// Remove old KeyStateManager registration - no longer needed
// services.AddSingleton<KeyStateManager>(); // ← DELETE THIS
```

### Migration from KeyStateManager

**Files to Update:**
1. **Remove**: `src/MIDIFlux.Core/Keyboard/KeyStateManager.cs`
2. **Update**: All keyboard actions to use `ActionStateManager` with internal state keys
3. **Update**: `EventDispatcher` to use `ActionStateManager` instead of `KeyStateManager`
4. **Update**: All DI registrations and constructor parameters
5. **Update**: All test files that reference `KeyStateManager`

## Profile Configuration with Initial States

### Profile-Level State Configuration

**Configuration Structure:**

```json
{
  "ProfileName": "Recording Studio Control",
  "Description": "Multi-mode recording control with LED feedback",
  "InitialStates": {
    "PlaybackMode": 0,
    "RecordingMode": 0,
    "TransportMode": 0,
    "MasterMode": 0,
    "SessionCount": 0,
    "BankNumber": 1
  },
  "MidiDevices": [
    // ... device configurations
  ]
}
```

**State Key Documentation:**
```
// User-defined state meanings (alphanumeric keys only)
PlaybackMode: 0=Stopped, 1=Playing
RecordingMode: 0=Ready, 1=Recording, 2=Paused
TransportMode: 0=Stop, 1=Play, 2=Record
MasterMode: 0=Normal, 1=Recording, 2=Mixing
SessionCount: Number of recording sessions (incremented)
BankNumber: Current bank selection (1-8)
```

### State Key Types and Validation Rules

#### User-Defined State Keys (Profile Configuration)
- **Alphanumeric Only**: Must contain only letters and numbers
- **No Spaces**: Use camelCase for multi-word keys
- **Profile-Configured**: Only states defined in `InitialStates` are user-manageable
- **Profile-Scoped**: Cleared when switching profiles

**Valid User-Defined State Keys:**
```
✅ "PlaybackMode"
✅ "RecordingMode"
✅ "BankNumber"
✅ "SessionCount"
✅ "Mode1"
✅ "Button36State"
```

**Invalid User-Defined State Keys:**
```
❌ "Playback Mode" (contains space)
❌ "Recording-Mode" (contains hyphen)
❌ "Mode@1" (contains special character)
❌ "*PlaybackMode" (starts with asterisk - reserved for internal)
❌ "" (empty string)
```

#### Internal State Keys (Auto-Generated)
- **Asterisk Prefix**: Must start with "*" to distinguish from user-defined
- **Keyboard States**: Format "*Key{VirtualKeyCode}" (e.g., "*Key65", "*Key16")
- **Numeric Suffix**: Everything after "*Key" must be numeric
- **System-Managed**: Cannot be defined in profile configuration

**Valid Internal State Keys:**
```
✅ "*Key65" (A key)
✅ "*Key16" (Shift key)
✅ "*Key32" (Spacebar)
✅ "*Key123" (Any valid virtual key code)
```

**Invalid Internal State Keys:**
```
❌ "*KeyA" (non-numeric after Key)
❌ "*Key" (missing key code)
❌ "*Button65" (wrong prefix)
❌ "Key65" (missing asterisk)
```

### Benefits of Profile-Level State Configuration

1. **Predictable Behavior**: All stateful actions start in known state
2. **User Experience**: Controllers show correct initial status
3. **Complex Workflows**: Multi-step processes start correctly
4. **Documentation**: Initial states serve as configuration documentation
5. **Testing**: Consistent starting point for testing
6. **No Surprises**: Only pre-configured states are managed (no on-demand creation)

## Implementation Strategy

### Phase 1: Unified State Infrastructure
- Implement unified `ActionStateManager` with dual state support (user-defined + internal)
- Add state validation for both user-defined and internal state keys
- Migrate all keyboard actions to use `ActionStateManager` with "*Key{VirtualKeyCode}" format
- Update `EventDispatcher` to use `ActionStateManager` instead of `KeyStateManager`
- Update all DI registrations and test files
- Add `InitialStates` property to `UnifiedMappingConfig`

### Phase 2: State-Conditional Actions
- Create `StateConditionalActionConfig` and `StateConditionalAction` classes
- Add to `UnifiedActionType` enum and factory
- Support equals, greater than, less than comparisons
- Implement condition matching and state modification logic

### Phase 3: Cleanup
- Delete `src/MIDIFlux.Core/Keyboard/KeyStateManager.cs`
- Remove all references to `KeyStateManager` from codebase
- Verify keyboard state tracking and stateful actions work correctly

## Key Technical Details

### State Value Semantics
- **-1**: State not defined/doesn't exist (returned when state key not in dictionary)
- **0**: Inactive/not pressed/false state
- **1**: Active/pressed/true state
- **Comparison logic**: `state < 1` for "not pressed", `state == 1` for "pressed"

### State Statistics
```csharp
public class StateManagerStatistics
{
    public int ActiveStates { get; set; }
    public int UserDefinedStates { get; set; }
    public int InternalStates { get; set; }
    public long MemoryUsage { get; set; }
}
```

## State-Conditional Action Implementation

### State Comparison Types

```csharp
public enum StateComparison
{
    Equals,        // state == value
    GreaterThan,   // state > value
    LessThan       // state < value
}
```

**Note**: Three comparison types are sufficient for integer states. Complex comparisons like `>=1` are equivalent to `>0`.

### Configuration Classes

```csharp
public class StateConditionalEntry
{
    public int StateValue { get; set; } = 0;
    public StateComparison Comparison { get; set; } = StateComparison.Equals;
    public UnifiedActionConfig Action { get; set; } = null!;
    public int SetStateAfter { get; set; } = -1; // -1 = no change
    public string Description { get; set; } = "";
}

public class StateConditionalActionConfig : UnifiedActionConfig
{
    public string StateKey { get; set; } = "";
    public StateConditionalEntry Condition { get; set; } = new(); // Single condition only
}
```

### StateConditionalAction Key Logic

The action checks current state value, finds matching condition, executes the sub-action, and optionally updates state:

```csharp
public async ValueTask ExecuteAsync(int? midiValue = null)
{
    var currentState = _stateManager.GetState(_stateKey);
    var matches = DoesConditionMatch(currentState, _condition);

    if (matches)
    {
        await _condition.Action.ExecuteAsync(midiValue);
        if (_condition.SetStateAfter != -1)
            _stateManager.SetState(_stateKey, _condition.SetStateAfter);
    }
}
```

### Condition Matching Logic

**Single Condition Model**: Only one condition is allowed per `StateConditionalActionConfig` to keep the logic simple and predictable. Multiple conditions would require complex AND/OR logic that adds unnecessary complexity for the initial implementation.

**Execution Flow**:
1. Get current state value for the state key
2. Check if the single condition matches the current state
3. If match: execute the sub-action, then optionally update state
4. If no match: do nothing (no error)

**Error Handling**: Follows established patterns using `ApplicationErrorHandler` for sub-action failures. State updates continue even if sub-action fails to maintain consistency with existing "continue to next step" behavior.

**State Key Case Sensitivity**: State keys are case-sensitive for simplicity. `"PlaybackMode"` and `"playbackmode"` are treated as different states.

**Validation**: Minimal validation for initial implementation. Complex validation can be added later without affecting the core architecture.

## Keyboard Action Integration

All keyboard actions use internal state keys like `"*Key65"` for automatic state tracking and cleanup.

### Key Implementation Points

**KeyDownAction**: Check `state < 1` before pressing, set state to `1` after successful press
**KeyUpAction**: Check `state == 1` before releasing, set state to `0` after successful release
**KeyToggleAction**: Toggle between press/release based on current state

### Benefits
- Automatic cleanup on profile change/app exit
- No key leakage (stuck keys)
- State awareness prevents duplicate presses
- Memory efficient (only creates entries for used keys)

## Use Case Examples

1. **Toggle Actions**: State "PlaybackMode" - 0=stopped (play action, set to 1), 1=playing (pause action, set to 0)
2. **Progressive Actions**: State "ButtonPressCount" - 1st press=A, 2nd press=B, 3rd press=C+reset
3. **Multi-Mode Buttons**: State "RecordingMode" - 0=Ready, 1=Recording, 2=Paused with different actions per mode
4. **Bank Selection**: State "BankNumber" - Different MIDI mappings based on current bank (1-8)

## Key Benefits

- **Unified Architecture**: Single `ActionStateManager` replaces `KeyStateManager` and provides stateful actions
- **Clear Separation**: Asterisk prefix (`*Key65`) distinguishes internal from user-defined states
- **Automatic Cleanup**: All pressed keys released on profile change/app exit
- **Memory Efficient**: On-demand state creation, no pre-population
- **Thread Safe**: `ConcurrentDictionary` handles concurrent MIDI events
- **Single Interface**: All actions implement `IUnifiedAction` consistently
