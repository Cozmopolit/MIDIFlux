# Note-On Only Behavior (KeyDown Actions)

MIDIFlux supports "Note-On Only" behavior through KeyDown actions with auto-release functionality. This allows mapping MIDI Note-On events to key presses while ignoring Note-Off events.

## Overview

Note-On Only behavior is useful for:

- MIDI controllers that don't send Note-Off events
- Creating momentary key presses with fixed duration
- Simulating sustained key presses triggered by single MIDI events
- Implementing "press and hold" functionality

## Current Implementation

The current unified action system provides Note-On Only functionality through:

### KeyDownAction with Auto-Release

Use `KeyDownConfig` with `AutoReleaseAfterMs` property for timed key presses:

```json
{
  "Id": "momentary-key-press",
  "Description": "Press A key for 500ms on Note-On only",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 65,
    "AutoReleaseAfterMs": 500,
    "Description": "Press A key for 500ms"
  }
}
```

### KeyDownAction without Auto-Release

Use `KeyDownConfig` without `AutoReleaseAfterMs` for sustained key presses:

```json
{
  "Id": "sustained-key-press",
  "Description": "Press and hold Shift key on Note-On",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 61,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 160,
    "Description": "Hold Left Shift key"
  }
}
```

## Configuration Properties

### KeyDownConfig
| Property | Type | Description |
|----------|------|-------------|
| `VirtualKeyCode` | ushort | The virtual key code to press |
| `AutoReleaseAfterMs` | int? | Optional auto-release time in milliseconds |
| `Description` | string | Optional description of the action |

## Complete Examples

### Momentary Screenshot Key

```json
{
  "Id": "screenshot",
  "Description": "Take screenshot with Print Screen",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 44,
    "AutoReleaseAfterMs": 100,
    "Description": "Press Print Screen for 100ms"
  }
}
```

### Sustained Modifier Key

```json
{
  "Id": "hold-ctrl",
  "Description": "Hold Ctrl key for modifier combinations",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 162,
    "Description": "Hold Left Ctrl key"
  }
}
```

### Release Modifier Key

```json
{
  "Id": "release-ctrl",
  "Description": "Release Ctrl key",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "KeyUpConfig",
    "VirtualKeyCode": 162,
    "Description": "Release Left Ctrl key"
  }
}
```

## Use Cases

### MIDI Controllers Without Note-Off Events

Some MIDI controllers, especially DIY or specialized controllers, may not send Note-Off events. KeyDown actions allow you to use these controllers effectively:

```json
{
  "Id": "diy-controller-button",
  "Description": "DIY controller button without Note-Off",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 32,
    "AutoReleaseAfterMs": 200,
    "Description": "Press Space for 200ms"
  }
}
```

### Fixed-Duration Key Presses

For applications requiring consistent key press duration regardless of MIDI note length:

```json
{
  "Id": "fixed-duration-action",
  "Description": "Always press key for exactly 1 second",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 13,
    "AutoReleaseAfterMs": 1000,
    "Description": "Press Enter for exactly 1 second"
  }
}
```

### Momentary Actions

For actions that should be brief and consistent (screenshots, quick commands):

```json
{
  "Id": "quick-screenshot",
  "Description": "Quick screenshot action",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 44,
    "AutoReleaseAfterMs": 50,
    "Description": "Quick Print Screen press"
  }
}
```

### Modifier Key Management

For complex workflows requiring sustained modifier keys:

```json
{
  "ProfileName": "Modifier Key Management",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "enable-shift-mode",
          "Description": "Enable Shift mode",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "KeyDownConfig",
            "VirtualKeyCode": 160,
            "Description": "Hold Left Shift"
          }
        },
        {
          "Id": "disable-shift-mode",
          "Description": "Disable Shift mode",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "KeyUpConfig",
            "VirtualKeyCode": 160,
            "Description": "Release Left Shift"
          }
        }
      ]
    }
  ]
}
```

## Technical Implementation

### Auto-Release Mechanism
- Auto-release runs on background thread to avoid blocking MIDI processing
- Timer starts immediately after key press
- Key state is properly tracked and updated
- Failed auto-release attempts are logged for debugging

### State Management
- Key states are tracked in unified state system
- Internal state keys use format `*Key{VirtualKeyCode}`
- Duplicate key presses are prevented through state checking
- All key states are cleared on profile changes

### Performance Considerations
- KeyDown actions execute synchronously for minimal latency
- Auto-release uses background tasks to avoid blocking
- State tracking is O(1) lookup and update
- Memory efficient with automatic cleanup

## Related Actions

- **KeyUpAction**: Explicitly release keys pressed with KeyDown
- **KeyPressReleaseAction**: Complete press/release cycle for normal key presses
- **SequenceAction**: Combine KeyDown/KeyUp actions in complex sequences
- **DelayAction**: Add timing control in sequences with key actions

## Best Practices

1. **Use Auto-Release**: Always specify `AutoReleaseAfterMs` for momentary actions
2. **Pair KeyDown/KeyUp**: For sustained actions, provide explicit release mappings
3. **State Awareness**: Consider how sustained keys affect other mappings
4. **Clear Descriptions**: Document the intended behavior and duration
5. **Test Thoroughly**: Verify auto-release timing works correctly in target applications

## Migration from Legacy Format

If you have old "Note-On Only" configurations, update them to use the current format:

**Old Format** (no longer supported):
```json
{
  "midiNote": 60,
  "virtualKeyCode": 65,
  "ignoreNoteOff": true,
  "autoReleaseAfterMs": 500
}
```

**New Format** (current):
```json
{
  "Id": "updated-mapping",
  "Description": "Updated to current format",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "KeyDownConfig",
    "VirtualKeyCode": 65,
    "AutoReleaseAfterMs": 500,
    "Description": "Press A key for 500ms"
  }
}
```

