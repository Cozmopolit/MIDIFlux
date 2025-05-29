# Note On Only Actions

MIDIFlux provides specialized handling for MIDI Note On messages, which are commonly used by drum pads, keyboard keys, and trigger buttons on MIDI controllers. Note On actions respond only to the initial key press and ignore the corresponding Note Off messages.

## Note On Input Type

**Input Type**: `NoteOn`

**How It Works**:
- Triggers when a MIDI Note On message is received
- Ignores corresponding Note Off messages
- Suitable for momentary actions and triggers
- Commonly used with drum pads and trigger buttons

## Configuration Format

```json
{
  "Description": "Note On trigger example",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "KeyPressReleaseAction",
    "Parameters": {
      "VirtualKeyCode": "Space"
    },
    "Description": "Press Space bar"
  }
}
```

## MIDI Note Numbers

### Standard MIDI Note Layout
MIDI notes are numbered 0-127, with middle C (C4) typically being note 60.

### Common Controller Mappings
- **Drum Pads**: Usually start at note 36 (C2)
  - Kick: 36, Snare: 38, Hi-hat: 42, Crash: 49
- **Piano Keys**: Middle C = 60, C3 = 48, C5 = 72
- **Trigger Buttons**: Often use notes 36-51 (C2-D#3)

### Note Number Reference
```
C-1 = 0    C0 = 12   C1 = 24   C2 = 36   C3 = 48
C#-1 = 1   C#0 = 13  C#1 = 25  C#2 = 37  C#3 = 49
D-1 = 2    D0 = 14   D1 = 26   D2 = 38   D3 = 50
...
C4 = 60 (Middle C)
C5 = 72
C6 = 84
C7 = 96
C8 = 108
G9 = 127 (Highest MIDI note)
```

## Complete Examples

### Basic Drum Pad Mapping

```json
{
  "ProfileName": "Drum Pad to Keyboard",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Description": "Kick drum to Space",
          "InputType": "NoteOn",
          "Channel": 10,
          "Note": 36,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "Space"
            },
            "Description": "Kick drum trigger"
          }
        },
        {
          "Description": "Snare to Enter",
          "InputType": "NoteOn",
          "Channel": 10,
          "Note": 38,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "Return"
            },
            "Description": "Snare trigger"
          }
        },
        {
          "Description": "Hi-hat to Shift",
          "InputType": "NoteOn",
          "Channel": 10,
          "Note": 42,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "ShiftKey"
            },
            "Description": "Hi-hat trigger"
          }
        },
        {
          "Description": "Crash to Ctrl",
          "InputType": "NoteOn",
          "Channel": 10,
          "Note": 49,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "ControlKey"
            },
            "Description": "Crash trigger"
          }
        }
      ]
    }
  ]
}
```

### Piano Key to Application Control

```json
{
  "Description": "Piano keys to application shortcuts",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
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
            "VirtualKeyCode": "S"
          },
          "Description": "Press S"
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
    "Description": "Save shortcut (Ctrl+S)"
  }
}
```

### Note-Based Mode Switching

```json
{
  "Description": "Note triggers mode change",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 48,
  "Action": {
    "$type": "StateConditionalAction",
    "Parameters": {
      "StateKey": "CurrentMode",
      "Conditions": [
        {
          "ComparisonType": "Equals",
          "ComparisonValue": 1
        }
      ],
      "LogicType": "Single",
      "ActionIfTrue": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "StateSetAction",
              "Parameters": {
                "StateKey": "CurrentMode",
                "Value": 2
              },
              "Description": "Switch to mode 2"
            },
            {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "F2"
              },
              "Description": "Press F2 for mode 2"
            }
          ]
        },
        "Description": "Activate mode 2"
      },
      "ActionIfFalse": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "StateSetAction",
              "Parameters": {
                "StateKey": "CurrentMode",
                "Value": 1
              },
              "Description": "Switch to mode 1"
            },
            {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "F1"
              },
              "Description": "Press F1 for mode 1"
            }
          ]
        },
        "Description": "Activate mode 1"
      }
    },
    "Description": "Toggle between modes"
  }
}
```

### Velocity-Sensitive Actions

```json
{
  "Description": "Velocity-sensitive note trigger",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "ConditionalAction",
    "Parameters": {
      "Conditions": [
        {
          "ComparisonType": "GreaterThan",
          "ComparisonValue": 100
        }
      ],
      "LogicType": "Single",
      "ActionIfTrue": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "A"
        },
        "Description": "Hard hit - Press A"
      },
      "ActionIfFalse": {
        "$type": "ConditionalAction",
        "Parameters": {
          "Conditions": [
            {
              "ComparisonType": "GreaterThan",
              "ComparisonValue": 50
            }
          ],
          "LogicType": "Single",
          "ActionIfTrue": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "B"
            },
            "Description": "Medium hit - Press B"
          },
          "ActionIfFalse": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "C"
            },
            "Description": "Soft hit - Press C"
          }
        },
        "Description": "Medium or soft hit"
      }
    },
    "Description": "Velocity-based key selection"
  }
}
```

### Multi-Channel Note Mapping

```json
{
  "ProfileName": "Multi-Channel Note Example",
  "MidiDevices": [
    {
      "DeviceName": "MIDI Keyboard",
      "Mappings": [
        {
          "Description": "Channel 1 notes to letters",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 60,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "A"
            },
            "Description": "Channel 1 Middle C"
          }
        },
        {
          "Description": "Channel 2 notes to numbers",
          "InputType": "NoteOn",
          "Channel": 2,
          "Note": 60,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "D1"
            },
            "Description": "Channel 2 Middle C"
          }
        },
        {
          "Description": "Any channel wildcard",
          "InputType": "NoteOn",
          "Channel": null,
          "Note": 72,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "Space"
            },
            "Description": "C5 on any channel"
          }
        }
      ]
    }
  ]
}
```

## Use Cases

### Music Production
- **Drum Triggers**: Map drum pads to software drum samples
- **Transport Control**: Use piano keys for play/stop/record
- **Track Selection**: Navigate tracks with keyboard notes
- **Effect Triggers**: Activate effects with note presses

### Gaming
- **Action Triggers**: Map drum pads to game actions
- **Combo Inputs**: Use note sequences for complex moves
- **Quick Commands**: Assign frequently used commands to notes
- **Mode Switching**: Change game modes with specific notes

### Live Performance
- **Cue Triggers**: Start audio/video cues with note presses
- **Lighting Control**: Trigger lighting changes
- **Scene Changes**: Switch between performance scenes
- **Emergency Stops**: Quick access to stop commands

### Accessibility
- **Alternative Input**: Use large drum pads for easier access
- **Custom Layouts**: Create personalized note-to-action mappings
- **Simplified Control**: Reduce complex key combinations to single notes
- **Adaptive Interfaces**: Support for users with mobility limitations

## Technical Notes

### Note On vs Note Off
- **Note On Only**: Triggers once when note is pressed
- **Note Off**: Would trigger when note is released (not covered by this action type)
- **Velocity**: Note On messages include velocity (0-127) for pressure sensitivity
- **Channel**: Notes can be filtered by MIDI channel (1-16)

### Velocity Handling
- Velocity values range from 1-127 (0 is treated as Note Off)
- Use ConditionalAction to create velocity-sensitive responses
- Higher velocity = harder/faster key press
- Lower velocity = softer/slower key press

### Channel Filtering
- **Specific Channel**: `"Channel": 1` (only channel 1)
- **Any Channel**: `"Channel": null` (all channels)
- **Multiple Devices**: Use separate mappings for different channels

### Performance Considerations
- Note On actions are optimized for low latency
- Suitable for real-time performance applications
- Minimal processing overhead per note
- Efficient lookup for large note mappings

## Related Actions

- **KeyPressReleaseAction**: Most common target for note triggers
- **SequenceAction**: Create complex note-triggered sequences
- **ConditionalAction**: Add velocity or value-based logic
- **StateConditionalAction**: Implement note-based mode switching
- **GameControllerButtonAction**: Map notes to game controller buttons

## Best Practices

1. **Use Standard Note Numbers**: Follow common MIDI conventions for drum pads
2. **Consider Velocity**: Implement velocity-sensitive responses where appropriate
3. **Group Related Notes**: Organize note mappings logically by function
4. **Test Latency**: Verify note response time for performance applications
5. **Document Mappings**: Clearly label which notes trigger which actions
6. **Handle Overlaps**: Ensure note ranges don't conflict between mappings

## Common Note Ranges

### Drum Kits (General MIDI)
- **Kick Drums**: 35-36
- **Snare Drums**: 37-40
- **Hi-Hats**: 42-44
- **Toms**: 41, 43, 45, 47, 48, 50
- **Cymbals**: 49, 51, 52, 55, 57, 59

### Piano Keyboard
- **Bass Range**: 21-47 (A0-B2)
- **Middle Range**: 48-71 (C3-B4)
- **Treble Range**: 72-96 (C5-C7)
- **Extended Range**: 97-127 (C#7-G9)

### Controller Pads
- **Pad Banks**: Often use 16-note ranges (e.g., 36-51, 52-67)
- **Trigger Buttons**: Usually 36-43 for 8-button layouts
- **Scene Triggers**: Often 44-51 for scene selection

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\note-triggers-demo.json` for comprehensive Note On examples.