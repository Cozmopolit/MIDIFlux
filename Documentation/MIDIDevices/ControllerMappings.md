# MIDI Controller Mappings

MIDIFlux supports mapping MIDI controllers to various system functions. MIDIFlux supports multiple MIDI devices simultaneously.

## Basic MIDI Note Mapping

The most common use case is mapping MIDI notes (from pads, keys, or buttons) to keyboard keys:

```json
{
  "ProfileName": "Basic Controller Mapping",
  "MidiDevices": [
    {
      "DeviceName": "PACER",
      "Mappings": [
        {
          "Id": "pacer-note-52",
          "Description": "PACER Note 52 to A key",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 52,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "A"
            },
            "Description": "Press A key"
          }
        },
        {
          "Id": "pacer-note-54",
          "Description": "PACER Note 54 to B key",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 54,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "B"
            },
            "Description": "Press B key"
          }
        },
        {
          "Id": "pacer-note-55",
          "Description": "PACER Note 55 to Shift+C",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 55,
          "Action": {
            "$type": "SequenceAction",
            "Parameters": {
              "SubActions": [
                {
                  "$type": "KeyDownAction",
                  "Parameters": {
                    "VirtualKeyCode": "ShiftKey"
                  },
                  "Description": "Press Shift"
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
                    "VirtualKeyCode": "ShiftKey"
                  },
                  "Description": "Release Shift"
                }
              ]
            },
            "Description": "Press Shift+C"
          }
        }
      ]
    }
  ]
}
```

This maps:
- MIDI note 52 to the 'A' key
- MIDI note 54 to the 'B' key
- MIDI note 55 to Shift+C key combination

## Supported Controller Types

### Absolute Value Controls

Absolute value controls send a specific value (0-127) when moved. These include:

- Faders
- Knobs
- Sliders
- Buttons with variable pressure

Use `ControlChangeAbsolute` as the InputType for these controls.

### Relative Value Controls

Relative value controls send incremental changes when moved. These include:

- Jog wheels
- Endless rotary encoders

Use `ControlChangeRelative` as the InputType for these controls.

## Control Change Mapping

Here's how to map control change messages to actions:

```json
{
  "ProfileName": "Multi-Device Controller Mapping",
  "MidiDevices": [
    {
      "DeviceName": "PACER",
      "Mappings": [
        {
          "Id": "pacer-volume-control",
          "Description": "Volume control with CC7",
          "InputType": "ControlChangeAbsolute",
          "Channel": 1,
          "ControlNumber": 7,
          "Action": {
            "$type": "ConditionalAction",
            "Parameters": {
              "Conditions": [
                {
                  "ComparisonType": "GreaterThan",
                  "ComparisonValue": 64
                }
              ],
              "LogicType": "Single",
              "ActionIfTrue": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "VolumeUp"
                },
                "Description": "Increase volume"
              },
              "ActionIfFalse": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "VolumeDown"
                },
                "Description": "Decrease volume"
              }
            },
            "Description": "Volume control based on CC value"
          }
        },
        {
          "Id": "pacer-scroll-control",
          "Description": "Scroll control with relative CC",
          "InputType": "ControlChangeRelative",
          "Channel": 1,
          "ControlNumber": 16,
          "Action": {
            "$type": "RelativeCCAction",
            "Parameters": {
              "PositiveAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Up",
                  "Amount": 2
                },
                "Description": "Scroll up"
              },
              "NegativeAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Down",
                  "Amount": 2
                },
                "Description": "Scroll down"
              }
            },
            "Description": "Relative scroll control"
          }
        }
      ]
    },
    {
      "DeviceName": "Traktor Kontrol S2 MK3",
      "Mappings": [
        {
          "Id": "traktor-note-52",
          "Description": "Traktor Note 52 to D key",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 52,
          "Action": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "D"
            },
            "Description": "Press D key"
          }
        },
        {
          "Id": "traktor-scroll-control",
          "Description": "Traktor scroll with different sensitivity",
          "InputType": "ControlChangeRelative",
          "Channel": 1,
          "ControlNumber": 16,
          "Action": {
            "$type": "RelativeCCAction",
            "Parameters": {
              "PositiveAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Up",
                  "Amount": 1
                },
                "Description": "Scroll up"
              },
              "NegativeAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Down",
                  "Amount": 1
                },
                "Description": "Scroll down"
              }
            },
            "Description": "Relative scroll control (lower sensitivity)"
          }
        }
      ]
    }
  ]
}
```

## Input Types and Parameters

### ControlChangeAbsolute

Used for faders, knobs, and sliders that send absolute values (0-127).

**Required Parameters**:
- **InputType**: `"ControlChangeAbsolute"`
- **Channel**: MIDI channel (1-16, or `null` for any channel)
- **ControlNumber**: MIDI CC number (0-127)
- **Action**: The action to execute

### ControlChangeRelative

Used for jog wheels and endless encoders that send relative movement data.

**Required Parameters**:
- **InputType**: `"ControlChangeRelative"`
- **Channel**: MIDI channel (1-16, or `null` for any channel)
- **ControlNumber**: MIDI CC number (0-127)
- **Action**: Usually a `RelativeCCAction` with positive and negative actions

### NoteOn

Used for drum pads, keys, and trigger buttons.

**Required Parameters**:
- **InputType**: `"NoteOn"`
- **Channel**: MIDI channel (1-16, or `null` for any channel)
- **Note**: MIDI note number (0-127)
- **Action**: The action to execute

## Common Action Types for Controllers

### ConditionalAction
Perfect for absolute controls where different value ranges trigger different actions.

### RelativeCCAction
Specifically designed for relative controls like jog wheels and encoders.

### KeyPressReleaseAction
Simple key press for note triggers and simple controls.

### MouseScrollAction
For scroll wheel control via relative encoders.

## Finding Control Numbers

To find the control numbers for your MIDI controller:

1. Run MIDIFlux
2. Check the application logs for MIDI input detection
3. Move the controls on your MIDI controller
4. Note the controller numbers and input types displayed in the logs
5. Use these values in your profile configuration

## Troubleshooting

If your control mappings aren't working:

1. **Check Input Type**: Verify you're using the correct InputType (`NoteOn`, `ControlChangeAbsolute`, or `ControlChangeRelative`)
2. **Verify Control Numbers**: Make sure you're using the correct note numbers or CC numbers
3. **Check Channel**: Ensure the MIDI channel matches (use `null` for any channel)
4. **Action Configuration**: Verify your action configuration uses the correct format with `Parameters` wrapper
5. **Device Name**: Ensure the `DeviceName` matches what MIDIFlux detects (use `"*"` for any device)
6. **Profile Loading**: Check that your profile loads without validation errors

### Device Name Matching

MIDIFlux attempts to match device names in the following order:
1. Exact case-insensitive match
2. Partial match (if the configured name is contained within the actual device name)
3. Use `"*"` as DeviceName to match any device
4. If no match is found, the mapping will be ignored

### Common Issues

- **Wrong InputType**: Using `ControlChangeAbsolute` for relative encoders or vice versa
- **Missing Parameters**: Forgetting the `Parameters` wrapper in action configuration
- **Invalid Virtual Key Codes**: Using numeric codes instead of string names (use `"A"` not `65`)
- **Channel Mismatch**: Controller sending on different channel than configured
