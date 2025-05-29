# Control Change Range Mapping

MIDIFlux provides sophisticated control change (CC) mapping capabilities for both absolute and relative CC messages. This enables precise control of applications using faders, knobs, and other continuous controllers.

## Control Change Input Types

### ControlChangeAbsolute

Handles standard MIDI CC messages with absolute values (0-127).

**Input Type**: `ControlChangeAbsolute`

**How It Works**:
- Receives MIDI CC values from 0-127
- Maps values to actions or system controls
- Suitable for faders, knobs, and sliders
- Provides direct value mapping

### ControlChangeRelative

Handles relative MIDI CC messages for endless encoders and jog wheels.

**Input Type**: `ControlChangeRelative`

**How It Works**:
- Interprets relative movement from encoders
- Values 1-63 = positive movement (clockwise)
- Values 65-127 = negative movement (counter-clockwise)
- Suitable for endless rotary encoders

## Configuration Format

```json
{
  "Description": "CC to action mapping",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 7,
  "Action": {
    "$type": "KeyPressReleaseAction",
    "Parameters": {
      "VirtualKeyCode": "VolumeUp"
    },
    "Description": "Volume control"
  }
}
```

## MIDI CC Numbers

### Standard MIDI CC Assignments
- **CC 1**: Modulation Wheel
- **CC 7**: Volume
- **CC 10**: Pan
- **CC 11**: Expression
- **CC 64**: Sustain Pedal (0-63 = off, 64-127 = on)
- **CC 71-74**: Filter controls
- **CC 91-93**: Reverb, Chorus, Delay

### Controller-Specific Assignments
- **DJ Controllers**: Often use CC 30-50 for jog wheels and knobs
- **MIDI Keyboards**: CC 1 (mod wheel), CC 64 (sustain)
- **Control Surfaces**: CC 7-15 for faders, CC 16-23 for knobs

## Complete Examples

### Volume Control with CC

```json
{
  "Description": "CC7 to system volume",
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
}
```

### Multi-Range CC Mapping

```json
{
  "Description": "CC with multiple value ranges",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 16,
  "Action": {
    "$type": "ConditionalAction",
    "Parameters": {
      "Conditions": [
        {
          "ComparisonType": "LessThan",
          "ComparisonValue": 32
        }
      ],
      "LogicType": "Single",
      "ActionIfTrue": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "F1"
        },
        "Description": "Low range (0-31) - F1"
      },
      "ActionIfFalse": {
        "$type": "ConditionalAction",
        "Parameters": {
          "Conditions": [
            {
              "ComparisonType": "LessThan",
              "ComparisonValue": 64
            }
          ],
          "LogicType": "Single",
          "ActionIfTrue": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "F2"
            },
            "Description": "Mid-low range (32-63) - F2"
          },
          "ActionIfFalse": {
            "$type": "ConditionalAction",
            "Parameters": {
              "Conditions": [
                {
                  "ComparisonType": "LessThan",
                  "ComparisonValue": 96
                }
              ],
              "LogicType": "Single",
              "ActionIfTrue": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "F3"
                },
                "Description": "Mid-high range (64-95) - F3"
              },
              "ActionIfFalse": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "F4"
                },
                "Description": "High range (96-127) - F4"
              }
            },
            "Description": "Mid-high or high range"
          }
        },
        "Description": "Mid-low, mid-high, or high range"
      }
    },
    "Description": "Four-range CC mapping"
  }
}
```

### Relative CC for Navigation

```json
{
  "Description": "Relative CC for menu navigation",
  "InputType": "ControlChangeRelative",
  "Channel": 1,
  "ControlNumber": 30,
  "Action": {
    "$type": "RelativeCCAction",
    "Parameters": {
      "PositiveAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "Down"
        },
        "Description": "Navigate down"
      },
      "NegativeAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "Up"
        },
        "Description": "Navigate up"
      }
    },
    "Description": "Encoder navigation"
  }
}
```

### CC to MIDI Output

```json
{
  "Description": "CC passthrough with modification",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 1,
  "Action": {
    "$type": "MidiControlChangeAction",
    "Parameters": {
      "OutputDevice": "Software Synthesizer",
      "OutputChannel": 2,
      "Controller": 1,
      "Value": 0
    },
    "Description": "Route CC1 from channel 1 to channel 2"
  }
}
```

### State-Based CC Mapping

```json
{
  "Description": "CC behavior changes based on state",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 10,
  "Action": {
    "$type": "StateConditionalAction",
    "Parameters": {
      "StateKey": "ControlMode",
      "Conditions": [
        {
          "ComparisonType": "Equals",
          "ComparisonValue": 1
        }
      ],
      "LogicType": "Single",
      "ActionIfTrue": {
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
              "VirtualKeyCode": "Right"
            },
            "Description": "Mode 1: Navigate right"
          },
          "ActionIfFalse": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "Left"
            },
            "Description": "Mode 1: Navigate left"
          }
        },
        "Description": "Mode 1: Navigation"
      },
      "ActionIfFalse": {
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
            "Description": "Mode 2: Volume up"
          },
          "ActionIfFalse": {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "VolumeDown"
            },
            "Description": "Mode 2: Volume down"
          }
        },
        "Description": "Mode 2: Volume control"
      }
    },
    "Description": "Mode-dependent CC behavior"
  }
}
```

### Threshold-Based CC Actions

```json
{
  "Description": "CC threshold triggers",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 64,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "StateSetAction",
          "Parameters": {
            "StateKey": "SustainValue",
            "Value": 0
          },
          "Description": "Store CC value in state (overwritten by MIDI)"
        },
        {
          "$type": "StateConditionalAction",
          "Parameters": {
            "StateKey": "SustainValue",
            "Conditions": [
              {
                "ComparisonType": "GreaterThan",
                "ComparisonValue": 63
              }
            ],
            "LogicType": "Single",
            "ActionIfTrue": {
              "$type": "KeyDownAction",
              "Parameters": {
                "VirtualKeyCode": "Space",
                "AutoReleaseAfterMs": null
              },
              "Description": "Sustain on - hold Space"
            },
            "ActionIfFalse": {
              "$type": "KeyUpAction",
              "Parameters": {
                "VirtualKeyCode": "Space"
              },
              "Description": "Sustain off - release Space"
            }
          },
          "Description": "Sustain pedal behavior"
        }
      ]
    },
    "Description": "CC64 sustain pedal emulation"
  }
}
```

## Use Cases

### Audio Production
- **Mixer Control**: Map faders to DAW track volumes
- **Effect Parameters**: Control reverb, delay, and filter settings
- **Transport Control**: Use encoders for timeline scrubbing
- **Parameter Automation**: Record CC movements as automation

### Live Performance
- **Real-time Control**: Adjust effects and levels during performance
- **Scene Selection**: Use CC ranges to switch between presets
- **Crossfading**: Control transitions between audio sources
- **Lighting Control**: Map CC to lighting intensity and color

### Gaming
- **Analog Controls**: Map CC to game controller analog sticks
- **Variable Actions**: Different actions based on CC value ranges
- **Sensitivity Control**: Adjust game control sensitivity with CC
- **Mode Switching**: Use CC ranges for different game modes

### System Control
- **Volume Control**: System volume adjustment with faders
- **Application Control**: Control media players and system settings
- **Window Management**: Navigate and control application windows
- **Accessibility**: Custom control schemes for users with disabilities

## Technical Notes

### Value Ranges
- **MIDI CC Range**: 0-127 (7-bit resolution)
- **Center Value**: 64 (for bipolar controls)
- **Binary Controls**: 0-63 = off, 64-127 = on
- **Fine Control**: Use multiple CCs for 14-bit resolution

### Performance Considerations
- CC actions are optimized for high-frequency updates
- Conditional logic adds minimal overhead
- State-based CC mapping is efficient for mode switching
- Suitable for real-time control applications

### Channel Filtering
- **Specific Channel**: `"Channel": 1` (only channel 1)
- **Any Channel**: `"Channel": null` (all channels)
- **Multi-Channel**: Use separate mappings for different channels

### Value Mapping
- MIDI CC values are automatically passed to actions
- Use ConditionalAction for range-based behavior
- StateSetAction can store CC values for later use
- RelativeCCAction handles encoder-style input

## Related Actions

- **ConditionalAction**: Essential for range-based CC mapping
- **StateConditionalAction**: Add mode-dependent CC behavior
- **RelativeCCAction**: Specialized for relative CC input
- **MidiControlChangeAction**: Route CC to MIDI output
- **KeyPressReleaseAction**: Common target for CC triggers

## Best Practices

1. **Use Appropriate Input Types**: ControlChangeAbsolute for faders, ControlChangeRelative for encoders
2. **Implement Range Logic**: Use ConditionalAction for value-based behavior
3. **Consider Hysteresis**: Avoid rapid switching at threshold boundaries
4. **Test Responsiveness**: Verify CC response feels natural and responsive
5. **Document CC Assignments**: Clearly label which CCs control which functions
6. **Handle Edge Cases**: Consider behavior at minimum and maximum values

## Common CC Mappings

### Standard MIDI Controllers
- **Modulation Wheel**: CC1 for vibrato/tremolo effects
- **Volume Fader**: CC7 for track or master volume
- **Pan Knob**: CC10 for stereo positioning
- **Sustain Pedal**: CC64 for note sustain

### DJ Controllers
- **Crossfader**: CC assignment varies by manufacturer
- **EQ Knobs**: Often CC16-23 for bass/mid/treble
- **Effect Knobs**: CC24-31 for effect parameters
- **Jog Wheels**: Relative CC for scratching/scrubbing

### Control Surfaces
- **Fader Banks**: CC7-15 for 8-channel mixing
- **Rotary Encoders**: CC16-23 for parameter control
- **Transport**: CC91-95 for play/stop/record
- **Bank Select**: CC0/32 for preset/bank switching

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\cc-range-demo.json` for comprehensive CC mapping examples.
```
