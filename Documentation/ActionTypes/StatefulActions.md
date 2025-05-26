# Stateful Actions

MIDIFlux includes a unified state management system that enables stateful actions and conditional behaviors based on state values.

## State Management System

### State Types

**User-defined States**:
- Alphanumeric keys only (letters, numbers, no special characters)
- Defined in profile configuration under `InitialStates`
- Persistent across MIDI events within a profile session
- Cleared when profile changes

**Internal States**:
- Auto-generated with asterisk prefix (e.g., `*Key65` for keyboard state)
- Managed automatically by the system
- Used for tracking keyboard key states, controller states, etc.

### State Scope
- **Profile-scoped**: States are initialized per profile
- **Thread-safe**: Concurrent access supported for real-time MIDI processing
- **Atomic operations**: State reads and writes are atomic

## Stateful Action Types

### StateConditionalAction

Executes actions based on state value comparisons.

**Configuration Type**: `StateConditionalConfig`

**Supported Comparisons**:
- `Equals`: State value equals specified value
- `GreaterThan`: State value is greater than specified value
- `LessThan`: State value is less than specified value

**Logic Types**:
- `Single`: Single condition check
- `And`: All conditions must be true (multiple conditions)

**Configuration Example**:
```json
{
  "$type": "StateConditionalConfig",
  "Conditions": [
    {
      "StateKey": "PlaybackMode",
      "ComparisonType": "Equals",
      "ComparisonValue": 1
    }
  ],
  "LogicType": "Single",
  "ActionIfTrue": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 32,
    "Description": "Press Space (Play)"
  },
  "ActionIfFalse": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 27,
    "Description": "Press Escape (Stop)"
  },
  "Description": "Play if mode=1, stop otherwise"
}
```

### SetStateAction

Sets state values directly.

**Configuration Type**: `SetStateConfig`

**Configuration Example**:
```json
{
  "$type": "SetStateConfig",
  "StateKey": "BankNumber",
  "StateValue": 2,
  "Description": "Set bank to 2"
}
```

### AlternatingAction

Convenience wrapper around the stateful action system for simple toggle behaviors.

**Configuration Type**: `AlternatingActionConfig`

**Configuration Example**:
```json
{
  "$type": "AlternatingActionConfig",
  "FirstAction": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 32,
    "Description": "Play"
  },
  "SecondAction": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 27,
    "Description": "Stop"
  },
  "Description": "Toggle Play/Stop"
}
```

## Complete Configuration Examples

### Profile with Initial States

```json
{
  "ProfileName": "Stateful Control Example",
  "Description": "Demonstrates stateful actions and state management",
  "InitialStates": {
    "PlaybackMode": 0,
    "BankNumber": 1,
    "RecordingState": 0
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "set-playback-mode",
          "Description": "Set playback mode to 1",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "SetStateConfig",
            "StateKey": "PlaybackMode",
            "StateValue": 1,
            "Description": "Enable playback mode"
          }
        },
        {
          "Id": "conditional-playback",
          "Description": "Play if mode=1, stop otherwise",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "StateConditionalConfig",
            "Conditions": [
              {
                "StateKey": "PlaybackMode",
                "ComparisonType": "Equals",
                "ComparisonValue": 1
              }
            ],
            "LogicType": "Single",
            "ActionIfTrue": {
              "$type": "KeyPressReleaseConfig",
              "VirtualKeyCode": 32,
              "Description": "Press Space (Play)"
            },
            "ActionIfFalse": {
              "$type": "KeyPressReleaseConfig",
              "VirtualKeyCode": 27,
              "Description": "Press Escape (Stop)"
            },
            "Description": "Conditional playback control"
          }
        }
      ]
    }
  ]
}
```

### Complex State Logic with Multiple Conditions

```json
{
  "Id": "complex-conditional",
  "Description": "Multiple condition example",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "StateConditionalConfig",
    "Conditions": [
      {
        "StateKey": "BankNumber",
        "ComparisonType": "GreaterThan",
        "ComparisonValue": 0
      },
      {
        "StateKey": "RecordingState",
        "ComparisonType": "Equals",
        "ComparisonValue": 1
      }
    ],
    "LogicType": "And",
    "ActionIfTrue": {
      "$type": "SequenceConfig",
      "SubActions": [
        {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 82,
          "Description": "Press R (Record)"
        },
        {
          "$type": "SetStateConfig",
          "StateKey": "RecordingState",
          "StateValue": 2,
          "Description": "Set recording active"
        }
      ],
      "Description": "Start recording sequence"
    },
    "ActionIfFalse": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 27,
      "Description": "Press Escape (Cancel)"
    },
    "Description": "Start recording if bank>0 AND recording=1"
  }
}
```

## Use Cases

### Media Control
- **Transport States**: Track play/pause/stop states for context-aware controls
- **Bank Selection**: Switch between different control banks or modes
- **Recording States**: Manage recording, overdub, and playback states

### Gaming
- **Weapon Selection**: Track current weapon and provide context-aware actions
- **Game Mode**: Different key mappings based on current game mode
- **Inventory Management**: Track inventory states for context-sensitive actions

### Productivity
- **Application Modes**: Different shortcuts based on current application mode
- **Document States**: Track document state (saved/unsaved) for conditional actions
- **Workspace Management**: Switch between different workspace configurations

### Creative Applications
- **Tool Selection**: Track current tool and provide tool-specific shortcuts
- **Layer Management**: Track active layers for layer-specific operations
- **Effect States**: Track effect on/off states for toggle behaviors

## Technical Implementation

### State Storage
- States are stored in a thread-safe `ConcurrentDictionary<string, int>`
- All state values are integers (use enums or constants for readability)
- State keys are case-sensitive

### Performance
- State operations are O(1) lookup and update
- No locks required for individual state operations
- Atomic read-modify-write operations supported

### Validation
- State keys are validated at configuration load time
- User-defined keys must be alphanumeric only
- Internal keys are automatically generated and managed

## Related Actions

- **SequenceAction**: Combine state actions with other actions in sequences
- **ConditionalAction**: Use MIDI values for conditions instead of states
- **AlternatingAction**: Simple two-state toggle without explicit state management
- **SetStateAction**: Building block for complex stateful behaviors

## Best Practices

1. **Use Descriptive State Names**: Choose clear, meaningful state key names
2. **Initialize States**: Always define initial states in profile configuration
3. **Document State Values**: Use comments or descriptions to explain state meanings
4. **Keep States Simple**: Use integers and avoid complex state hierarchies
5. **Test State Logic**: Verify conditional logic with different state combinations
