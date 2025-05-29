# Mouse Actions

MIDIFlux supports mouse click and scroll wheel actions.

## Supported Mouse Actions

### MouseClick Action

Simulates mouse button clicks.

**Configuration Type**: `MouseClickAction`

**Supported Buttons**:
- `Left`: Left mouse button
- `Right`: Right mouse button
- `Middle`: Middle mouse button (scroll wheel click)

**Configuration Example**:
```json
{
  "$type": "MouseClickAction",
  "Parameters": {
    "Button": "Left"
  },
  "Description": "Left mouse click"
}
```

### MouseScroll Action

Simulates mouse scroll wheel movement.

**Configuration Type**: `MouseScrollAction`

**Supported Directions**:
- `Up`: Scroll up
- `Down`: Scroll down
- `Left`: Scroll left (horizontal scrolling)
- `Right`: Scroll right (horizontal scrolling)

**Configuration Example**:
```json
{
  "$type": "MouseScrollAction",
  "Parameters": {
    "Direction": "Up",
    "Amount": 3
  },
  "Description": "Scroll up 3 units"
}
```

**Properties**:
- `Direction`: The scroll direction (Up/Down/Left/Right)
- `Amount`: Number of scroll units (default: 1)
- `Description`: Optional human-readable description

## Complete Mapping Examples

### Basic Mouse Actions

```json
{
  "ProfileName": "Mouse Control Example",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Description": "Left mouse click",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "MouseClickAction",
            "Parameters": {
              "Button": "Left"
            },
            "Description": "Left click"
          }
        },
        {
          "Description": "Right mouse click",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "MouseClickAction",
            "Parameters": {
              "Button": "Right"
            },
            "Description": "Right click"
          }
        },
        {
          "Description": "Scroll up",
          "InputType": "ControlChangeAbsolute",
          "Channel": 1,
          "ControlNumber": 1,
          "Action": {
            "$type": "MouseScrollAction",
            "Parameters": {
              "Direction": "Up",
              "Amount": 3
            },
            "Description": "Scroll up 3 units"
          }
        }
      ]
    }
  ]
}
```

### Advanced Mouse Control with Sequences

```json
{
  "Description": "Double-click sequence",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "MouseClickAction",
          "Parameters": {
            "Button": "Left"
          },
          "Description": "First click"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "DelayMs": 50
          },
          "Description": "Short delay"
        },
        {
          "$type": "MouseClickAction",
          "Parameters": {
            "Button": "Left"
          },
          "Description": "Second click"
        }
      ]
    },
    "Description": "Double-click sequence"
  }
}
```

## Use Cases

### Media Control
- Map MIDI faders to scroll wheel for timeline scrubbing
- Map MIDI buttons to mouse clicks for play/pause buttons

### Gaming
- Map MIDI pads to mouse clicks for weapon selection
- Map MIDI controllers to scroll wheel for weapon switching

### Productivity
- Map MIDI buttons to right-click for context menus
- Map MIDI controllers to horizontal scrolling for timeline navigation

### Accessibility
- Use MIDI foot pedals for mouse clicks when hands are busy
- Map large MIDI pads to mouse actions for easier access

## Technical Notes

### Performance
- Mouse actions execute synchronously for minimal latency
- No cursor movement is supported (use dedicated mouse hardware instead)
- Scroll wheel actions respect system scroll settings

### Compatibility
- Works with all Windows applications that accept mouse input
- Horizontal scrolling requires application support
- Middle mouse button may have application-specific behavior

### Limitations
- No mouse movement/positioning support (by design)
- No drag-and-drop operations
- No mouse button hold/release (use sequences if needed)

## Related Actions

- **Delay**: Add timing between mouse actions in sequences
- **SequenceAction**: Create complex mouse interaction patterns
- **ConditionalAction**: Trigger different mouse actions based on MIDI values
