# Keyboard Actions

MIDIFlux supports comprehensive keyboard actions. All keyboard actions use the Windows SendInput API for reliable key simulation.

## Supported Keyboard Actions

### KeyPressRelease Action

Simulates a complete key press and release cycle (most common use case).

**Configuration Type**: `KeyPressReleaseAction`

**Configuration Example**:
```json
{
  "$type": "KeyPressReleaseAction",
  "Parameters": {
    "VirtualKeyCode": "A"
  },
  "Description": "Press and release A key"
}
```

### KeyDown Action

Presses and holds a key down (useful for modifier keys or sustained actions).

**Configuration Type**: `KeyDownAction`

**Configuration Example**:
```json
{
  "$type": "KeyDownAction",
  "Parameters": {
    "VirtualKeyCode": "ControlKey",
    "AutoReleaseAfterMs": null
  },
  "Description": "Press and hold Left Ctrl"
}
```

### KeyUp Action

Releases a previously pressed key.

**Configuration Type**: `KeyUpAction`

**Configuration Example**:
```json
{
  "$type": "KeyUpAction",
  "Parameters": {
    "VirtualKeyCode": "ControlKey"
  },
  "Description": "Release Left Ctrl"
}
```

### KeyToggle Action

Toggles the state of toggle keys like CapsLock, NumLock, ScrollLock.

**Configuration Type**: `KeyToggleAction`

**Configuration Example**:
```json
{
  "$type": "KeyToggleAction",
  "Parameters": {
    "VirtualKeyCode": "CapsLock"
  },
  "Description": "Toggle CapsLock"
}
```

## Virtual Key Codes

MIDIFlux uses string-based virtual key codes for better readability and maintainability. Here are the most commonly used keys:

### Letters and Numbers
- **Letters**: "A", "B", "C", ... "Z"
- **Numbers**: "D0", "D1", "D2", ... "D9"
- **Numpad**: "NumPad0", "NumPad1", ... "NumPad9"

### Function Keys
- **Function Keys**: "F1", "F2", "F3", ... "F12"

### Navigation Keys
- **Arrow Keys**: "Left", "Up", "Right", "Down"
- **Home**: "Home"
- **End**: "End"
- **Page Up**: "PageUp"
- **Page Down**: "PageDown"
- **Insert**: "Insert"
- **Delete**: "Delete"

### Modifier Keys
- **Shift**: "ShiftKey" (generic), "LShiftKey" (left), "RShiftKey" (right)
- **Control**: "ControlKey" (generic), "LControlKey" (left), "RControlKey" (right)
- **Alt**: "Menu" (generic), "LMenu" (left), "RMenu" (right/Alt Gr)

### Special Keys
- **Backspace**: "Back"
- **Tab**: "Tab"
- **Enter**: "Return"
- **Escape**: "Escape"
- **Space**: "Space"
- **Print Screen**: "PrintScreen"

### Toggle Keys
- **CapsLock**: "CapsLock"
- **NumLock**: "NumLock"
- **ScrollLock**: "Scroll"

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
          "Id": "press-a-key",
          "Description": "Press A key",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "A"
            },
            "Description": "Press A key"
          }
        },
        {
          "Id": "toggle-capslock",
          "Description": "Toggle CapsLock",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "KeyToggleAction",
            "Parameters": {
              "VirtualKeyCode": "CapsLock"
            },
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
  "Id": "ctrl-c-copy",
  "Description": "Ctrl+C copy shortcut",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "KeyDownAction",
          "Parameters": {
            "VirtualKeyCode": "ControlKey",
            "AutoReleaseAfterMs": null
          },
          "Description": "Press Ctrl"
        },
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "C"
          },
          "Description": "Press C"
        },
        {
          "$type": "KeyUpAction",
          "Parameters": {
            "VirtualKeyCode": "ControlKey"
          },
          "Description": "Release Ctrl"
        }
      ]
    },
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
- Keyboard key states are tracked in the state system
- Internal state keys use format `*Key{VirtualKeyCode}` (e.g., `*KeyA` for A key)
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
   - Use specific left/right modifier codes (e.g., "LControlKey", "RControlKey")
   - Use SequenceAction for proper key combination timing
   - Ensure proper key down/up sequence in macros

4. **Special Key Problems**:
   - Extended keys are handled automatically
   - Print Screen has built-in special handling
   - Some applications may not respond to certain virtual keys

### Debugging

- Enable debug logging to see MIDI events and key actions
- Use the Configuration GUI to test mappings
- Check `%AppData%\MIDIFlux\Logs\` for detailed execution information
- Verify MIDI device connectivity through the system tray menu

