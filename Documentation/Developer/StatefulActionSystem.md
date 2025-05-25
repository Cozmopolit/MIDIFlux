# MIDIFlux Stateful Action System

## Overview

MIDIFlux includes a comprehensive stateful action system that enables MIDI mappings to maintain integer state between triggers. This system was implemented to support advanced use cases like toggle buttons, progressive actions, and mode-dependent behaviors while maintaining the existing unified action architecture.

## Key Components

### ActionStateManager
**Location**: `src/MIDIFlux.Core/State/ActionStateManager.cs`

The central state management service that replaces the old `KeyStateManager`. Features:
- **Thread-safe**: Uses `ConcurrentDictionary<string, int>` for concurrent access
- **Dual state types**: User-defined states and auto-generated internal states
- **Profile-scoped**: States cleared on profile change with automatic key cleanup
- **Memory efficient**: On-demand state creation, no pre-population

**State Key Types**:
- **User-defined**: Alphanumeric only (e.g., `"PlaybackMode"`, `"BankNumber"`)
- **Internal**: Asterisk prefix for keyboard tracking (e.g., `"*Key65"`, `"*Key32"`)

**State Semantics**:
- `-1`: State not defined/doesn't exist
- `0`: Inactive/not pressed/false state  
- `1`: Active/pressed/true state
- `>1`: Custom integer values for complex states

### Stateful Action Classes

#### StateConditionalAction
**Location**: `src/MIDIFlux.Core/Actions/Stateful/StateConditionalAction.cs`

Executes different actions based on state conditions using a single condition model:
- **Comparisons**: Equals, GreaterThan, LessThan
- **Execution**: Checks condition → executes sub-action → optionally updates state
- **Error handling**: Follows established patterns, continues execution on sub-action failure

#### SetStateAction  
**Location**: `src/MIDIFlux.Core/Actions/Stateful/SetStateAction.cs`

Simple action for setting state values directly. Used as building blocks for more complex stateful behaviors.

### Configuration Classes
**Location**: `src/MIDIFlux.Core/Actions/Configuration/`

- `StateConditionalConfig`: Configuration for conditional actions
- `SetStateConfig`: Configuration for state setting actions
- Both include validation for state key formats and required properties

## Integration Points

### Profile Configuration
**File**: `UnifiedMappingConfig` in `src/MIDIFlux.Core/Actions/Configuration/UnifiedActionConfig.cs`

Added `InitialStates` property for profile-level state initialization:

```json
{
  "ProfileName": "My Profile",
  "InitialStates": {
    "PlaybackMode": 0,
    "BankNumber": 1,
    "RecordingState": 0
  },
  "MidiDevices": [...]
}
```

### EventDispatcher Integration
**File**: `src/MIDIFlux.Core/EventDispatcher.cs`

- Uses `ActionStateManager` instead of old `KeyStateManager`
- Initializes states from profile configuration on profile load
- Clears all states and releases keys on profile change

### Dependency Injection
**File**: `src/MIDIFlux.App/Extensions/ServiceCollectionExtensions.cs`

- `ActionStateManager` registered as singleton
- `UnifiedActionFactory` receives service provider for dependency injection
- Keyboard actions automatically get `ActionStateManager` for internal state tracking

### Keyboard Action Integration
**Files**: `src/MIDIFlux.Core/Actions/Simple/Key*.cs`

All keyboard actions now use `ActionStateManager` with internal state keys:
- **KeyDownAction**: Checks state to prevent duplicate presses, sets state to 1
- **KeyUpAction**: Checks state before releasing, sets state to 0  
- **KeyPressReleaseAction**: Atomic press+release operation
- **KeyToggleAction**: Toggle behavior for special keys

## Usage Examples

### Toggle Button (Play/Pause)
Requires two separate mappings on the same MIDI input:

**Mapping 1 - Play Action**:
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
        "VirtualKeyCode": 32
      },
      "SetStateAfter": 1
    }
  }
}
```

**Mapping 2 - Pause Action**:
```json
{
  "Action": {
    "$type": "StateConditionalActionConfig", 
    "StateKey": "PlaybackMode",
    "Condition": {
      "StateValue": 1,
      "Comparison": "Equals",
      "Action": {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 32
      },
      "SetStateAfter": 0
    }
  }
}
```

### Progressive Counter
Multiple mappings for different state transitions:

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
        "VirtualKeyCode": 65
      },
      "SetStateAfter": 1
    }
  }
}
```

### Bank Selection
Use state for context-sensitive mappings:

```json
{
  "Action": {
    "$type": "StateConditionalActionConfig",
    "StateKey": "BankNumber",
    "Condition": {
      "StateValue": 1,
      "Comparison": "GreaterThan", 
      "Action": {
        "$type": "SetStateConfig",
        "StateKey": "BankNumber",
        "StateValue": 1
      }
    }
  }
}
```

## Testing

### Test Files
- `src/MIDIFlux.Core.Tests/Actions/StatefulActionTests.cs` - Unit tests for stateful actions
- `src/MIDIFlux.Core.Tests/Integration/StatefulSystemIntegrationTests.cs` - Integration tests

### Test Coverage
- State management operations (get, set, clear, initialize)
- Stateful action execution and condition matching
- Keyboard state tracking and cleanup
- Profile state initialization and validation
- Error handling and edge cases

## Architecture Benefits

1. **Unified Interface**: All actions implement `IUnifiedAction` - no special handling needed
2. **Performance**: Lock-free reads, minimal allocation, O(1) state access
3. **Memory Efficient**: On-demand state creation, automatic cleanup
4. **Thread-Safe**: Concurrent MIDI event processing supported
5. **Extensible**: Easy to add new stateful action types
6. **Testable**: Comprehensive test coverage with dependency injection

## Migration Notes

### Replaced Components
- **Old**: `KeyStateManager` → **New**: `ActionStateManager`
- **Old**: Manual key state tracking → **New**: Automatic internal state keys
- **Old**: No stateful actions → **New**: `StateConditionalAction`, `SetStateAction`

### Breaking Changes
- `KeyStateManager` class removed
- All keyboard actions now require `ActionStateManager` parameter
- Profile configuration supports optional `InitialStates` property

### Backward Compatibility
- Existing profiles without `InitialStates` work unchanged
- All existing action types continue to work
- No changes to MIDI event processing or action execution patterns

## Future Extensions

The stateful action system is designed for easy extension:

1. **New Comparison Types**: Add to `StateComparison` enum
2. **Complex Conditions**: Multiple conditions with AND/OR logic
3. **State Persistence**: Optional state saving between sessions
4. **State Expressions**: Mathematical operations on state values
5. **State Events**: Triggers when states change

## Performance Characteristics

- **State Access**: O(1) lookup via `ConcurrentDictionary`
- **Memory Usage**: ~64 bytes per active state (rough estimate)
- **Thread Safety**: Lock-free reads, minimal contention on writes
- **Cleanup**: Automatic on profile change, manual via `ClearAllStates()`

The stateful action system provides a solid foundation for advanced MIDI mapping scenarios while maintaining MIDIFlux's performance and architectural principles.
