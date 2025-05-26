# Alternating Actions (Toggle Mapping)

MIDIFlux supports alternating actions that toggle between two different actions on repeated MIDI triggers. This provides toggle functionality and is useful for play/pause controls, on/off switches, and other binary state behaviors.

## AlternatingAction

Alternates between two actions on repeated triggers.

**Configuration Type**: `AlternatingActionConfig`

**How It Works**:
- First trigger executes the first action
- Second trigger executes the second action
- Third trigger executes the first action again
- Pattern continues alternating

## Configuration Format

```json
{
  "$type": "AlternatingActionConfig",
  "FirstAction": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 32,
    "Description": "Press Space (Play)"
  },
  "SecondAction": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 27,
    "Description": "Press Escape (Stop)"
  },
  "Description": "Toggle Play/Stop"
}
```

## Key Toggle Actions

For actual key toggle behavior (like CapsLock), use the `KeyToggleAction`:

```json
{
  "$type": "KeyToggleConfig",
  "VirtualKeyCode": 20,
  "Description": "Toggle CapsLock"
}
```

## Complete Mapping Examples

### Basic Play/Pause Toggle

```json
{
  "Id": "play-pause-toggle",
  "Description": "Toggle between play and pause",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "AlternatingActionConfig",
    "FirstAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 32,
      "Description": "Press Space (Play)"
    },
    "SecondAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 32,
      "Description": "Press Space (Pause)"
    },
    "Description": "Play/Pause toggle"
  }
}
```

### Toggle Between Different Keys

```json
{
  "Id": "mode-toggle",
  "Description": "Toggle between two different modes",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "AlternatingActionConfig",
    "FirstAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 49,
      "Description": "Press 1 (Mode A)"
    },
    "SecondAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 50,
      "Description": "Press 2 (Mode B)"
    },
    "Description": "Toggle between Mode A and Mode B"
  }
}
```

### CapsLock Toggle

```json
{
  "Id": "caps-lock-toggle",
  "Description": "Toggle CapsLock state",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "KeyToggleConfig",
    "VirtualKeyCode": 20,
    "Description": "Toggle CapsLock"
  }
}
```

### Complex Action Alternating

```json
{
  "Id": "complex-toggle",
  "Description": "Toggle between complex action sequences",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 39,
  "Action": {
    "$type": "AlternatingActionConfig",
    "FirstAction": {
      "$type": "SequenceConfig",
      "SubActions": [
        {
          "$type": "KeyDownConfig",
          "VirtualKeyCode": 162,
          "Description": "Press Ctrl"
        },
        {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 67,
          "Description": "Press C"
        },
        {
          "$type": "KeyUpConfig",
          "VirtualKeyCode": 162,
          "Description": "Release Ctrl"
        }
      ],
      "Description": "Copy (Ctrl+C)"
    },
    "SecondAction": {
      "$type": "SequenceConfig",
      "SubActions": [
        {
          "$type": "KeyDownConfig",
          "VirtualKeyCode": 162,
          "Description": "Press Ctrl"
        },
        {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 86,
          "Description": "Press V"
        },
        {
          "$type": "KeyUpConfig",
          "VirtualKeyCode": 162,
          "Description": "Release Ctrl"
        }
      ],
      "Description": "Paste (Ctrl+V)"
    },
    "Description": "Toggle between Copy and Paste"
  }
}
```

## Use Cases

### Media Control
- **Play/Pause**: Single button toggles between play and pause
- **Record/Stop**: Toggle recording state with one control
- **Mute/Unmute**: Toggle audio mute state

### Gaming
- **Weapon Toggle**: Switch between two favorite weapons
- **Mode Toggle**: Switch between different game modes
- **Ability Toggle**: Alternate between two special abilities

### Productivity
- **View Toggle**: Switch between two different views
- **Tool Toggle**: Alternate between two frequently used tools
- **Window Toggle**: Switch between two applications

### Creative Applications
- **Layer Toggle**: Switch between two active layers
- **Brush Toggle**: Alternate between two brush types
- **Effect Toggle**: Turn effects on and off

## State Management

### AlternatingAction State
- Uses internal state system with auto-generated keys
- State is automatically managed and persists during session
- State is cleared when profile changes or application restarts

### KeyToggle State
- Integrates with unified state management system
- Key states are tracked internally with `*Key{VirtualKeyCode}` format
- Automatic cleanup prevents stuck keys

## Technical Notes

### Performance
- Alternating actions execute synchronously for minimal latency
- State tracking is O(1) lookup and update
- No memory leaks from state accumulation

### Compatibility
- Works with all action types (simple and complex)
- Can alternate between any two actions
- Supports nested complex actions in alternating patterns

### State Persistence
- Alternating state persists during MIDI session
- State is reset when configuration changes
- Clean shutdown releases all toggle states

## Related Actions

- **StateConditionalAction**: Use explicit state values for more complex logic
- **SetStateAction**: Manually control state values
- **SequenceAction**: Create complex actions to use in alternating patterns
- **KeyToggleAction**: Built-in toggle for specific keys like CapsLock

## Best Practices

1. **Clear Action Names**: Use descriptive names for both actions in alternating pairs
2. **Consistent Behavior**: Ensure both actions make sense as alternating pairs
3. **State Awareness**: Consider how alternating state affects other mappings
4. **User Feedback**: Provide visual or audio feedback for toggle state changes
5. **Reset Mechanisms**: Consider providing ways to reset toggle states to known positions

