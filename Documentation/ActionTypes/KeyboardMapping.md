# Keyboard Actions

MIDIFlux supports comprehensive keyboard actions through the unified action system. All keyboard actions use the Windows SendInput API for reliable key simulation.

## Supported Keyboard Actions

### KeyPressRelease Action

Simulates a complete key press and release cycle (most common use case).

**Configuration Type**: `KeyPressReleaseConfig`

**Configuration Example**:
```json
{
  "$type": "KeyPressReleaseConfig",
  "VirtualKeyCode": 65,
  "Description": "Press and release A key"
}
```

### KeyDown Action

Presses and holds a key down (useful for modifier keys or sustained actions).

**Configuration Type**: `KeyDownConfig`

**Configuration Example**:
```json
{
  "$type": "KeyDownConfig",
  "VirtualKeyCode": 162,
  "Description": "Press and hold Left Ctrl"
}
```

### KeyUp Action

Releases a previously pressed key.

**Configuration Type**: `KeyUpConfig`

**Configuration Example**:
```json
{
  "$type": "KeyUpConfig",
  "VirtualKeyCode": 162,
  "Description": "Release Left Ctrl"
}
```

### KeyToggle Action

Toggles the state of toggle keys like CapsLock, NumLock, ScrollLock.

**Configuration Type**: `KeyToggleConfig`

**Configuration Example**:
```json
{
  "$type": "KeyToggleConfig",
  "VirtualKeyCode": 20,
  "Description": "Toggle CapsLock"
}
```

## Virtual Key Codes

Windows uses virtual key codes to identify keys. Here are the most commonly used codes:

### Letters and Numbers
- **Letters**: A-Z (65-90)
- **Numbers**: 0-9 (48-57)
- **Numpad**: 0-9 (96-105)

### Function Keys
- **Function Keys**: F1-F12 (112-123)

### Navigation Keys
- **Arrow Keys**: Left (37), Up (38), Right (39), Down (40)
- **Home**: 36
- **End**: 35
- **Page Up**: 33
- **Page Down**: 34
- **Insert**: 45
- **Delete**: 46

### Modifier Keys (Recommended Specific Codes)
- **Left Shift**: 160
- **Right Shift**: 161
- **Left Ctrl**: 162
- **Right Ctrl**: 163
- **Left Alt**: 164
- **Right Alt (Alt Gr)**: 165

### Special Keys
- **Backspace**: 8
- **Tab**: 9
- **Enter**: 13
- **Escape**: 27
- **Space**: 32
- **Print Screen**: 44

### Toggle Keys
- **CapsLock**: 20
- **NumLock**: 144
- **ScrollLock**: 145

For a complete reference, see [Windows Virtual Key Codes](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes).

## Complete Mapping Examples

### Basic Key Mappings

```json
{
  "ProfileName": "Basic Keyboard Example",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "press-a",
          "Description": "Press A key",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 65,
            "Description": "Press A key"
          }
        },
        {
          "Id": "toggle-caps",
          "Description": "Toggle CapsLock",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "KeyToggleConfig",
            "VirtualKeyCode": 20,
            "Description": "Toggle CapsLock"
          }
        }
      ]
    }
  ]
}
```

### Key Combinations with Sequences

For key combinations like Ctrl+C, use SequenceAction:

```json
{
  "Id": "copy-shortcut",
  "Description": "Ctrl+C copy shortcut",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
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
  }
}
```

## Use Cases

### Productivity Applications
- **Copy/Paste**: Map MIDI pads to Ctrl+C, Ctrl+V shortcuts
- **Undo/Redo**: Map MIDI buttons to Ctrl+Z, Ctrl+Y
- **Save**: Map MIDI pedal to Ctrl+S for quick saving
- **Tab Navigation**: Map MIDI controllers to Ctrl+Tab, Ctrl+Shift+Tab

### Media and Creative Applications
- **Playback Control**: Map MIDI buttons to Space (play/pause), Home (beginning), End (end)
- **Timeline Navigation**: Map MIDI controllers to arrow keys for frame-by-frame editing
- **Tool Selection**: Map MIDI pads to number keys for tool shortcuts
- **Modifier Keys**: Map MIDI pedals to Shift, Ctrl, Alt for modifier combinations

### Gaming
- **WASD Movement**: Map MIDI controllers to movement keys
- **Action Keys**: Map MIDI pads to Space (jump), Shift (run), Ctrl (crouch)
- **Function Keys**: Map MIDI buttons to F1-F12 for game functions
- **Quick Actions**: Map MIDI pedals to frequently used keys

### Accessibility
- **Large Buttons**: Use MIDI pads as large, easy-to-press alternatives to small keys
- **Foot Control**: Use MIDI pedals for hands-free key operations
- **Custom Layouts**: Create ergonomic key layouts using MIDI controllers

## Technical Notes

### Performance
- All keyboard actions execute synchronously for minimal latency
- Key state is tracked internally to prevent stuck keys
- Extended keys (Insert, Delete, etc.) are handled automatically
- Print Screen and other special keys have built-in support

### Compatibility
- Works with all Windows applications that accept keyboard input
- Supports both standard and extended virtual key codes
- Handles modifier key combinations correctly
- Compatible with international keyboard layouts

### State Management
- Keyboard key states are tracked in the unified state system
- Internal state keys use format `*Key{VirtualKeyCode}` (e.g., `*Key65` for A key)
- Key states are automatically cleared when profiles change
- Prevents stuck keys through automatic state cleanup

## Related Actions

- **Delay**: Add timing between key presses in sequences
- **SequenceAction**: Create complex key combinations and macros
- **ConditionalAction**: Trigger different keys based on MIDI values
- **AlternatingAction**: Toggle between two different key actions
- **StateConditionalAction**: Execute keys based on current state values

## Troubleshooting

### Common Issues

1. **Keys Not Working**:
   - Verify the virtual key code is correct
   - Check that the target application has focus
   - Ensure MIDI events are being received (check logs)

2. **Stuck Keys**:
   - Key states are automatically managed and cleared
   - Profile changes clear all key states
   - Application restart clears all states

3. **Modifier Key Issues**:
   - Use specific left/right modifier codes (160-165)
   - Use SequenceAction for proper key combination timing
   - Ensure proper key down/up sequence in macros

4. **Special Key Problems**:
   - Extended keys are handled automatically
   - Print Screen has built-in special handling
   - Some applications may not respond to certain virtual keys

### Debugging

- Enable debug logging to see MIDI events and key actions
- Use the Configuration GUI to test mappings
- Check the `Logs` directory for detailed execution information
- Verify MIDI device connectivity through the system tray menu

