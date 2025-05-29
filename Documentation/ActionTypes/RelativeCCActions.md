# Relative Control Change Actions

MIDIFlux provides specialized actions for handling relative control change (CC) messages, commonly used by DJ controllers, jog wheels, and rotary encoders. These actions interpret relative MIDI data and convert it to appropriate system responses.

## RelativeCCAction

Processes relative control change messages and converts them to mouse scroll, key presses, or other actions based on the direction and speed of the control movement.

**Configuration Type**: `RelativeCCAction`

**How It Works**:
- Interprets relative CC values (typically 1-63 for positive, 65-127 for negative)
- Converts relative movement to discrete actions
- Supports variable scroll rates based on movement speed
- Uses state machine for acceleration control

## Configuration Format

```json
{
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
    },
    "AccelerationStateKey": "ScrollAccel",
    "AccelerationThreshold": 5,
    "AccelerationMultiplier": 3
  },
  "Description": "Relative CC to mouse scroll with acceleration"
}
```

## Parameters

### Required Parameters
- **PositiveAction**: Action to execute for positive (clockwise) movement
- **NegativeAction**: Action to execute for negative (counter-clockwise) movement

### Optional Parameters
- **AccelerationStateKey**: State key for tracking acceleration (enables variable speed)
- **AccelerationThreshold**: Number of rapid movements before acceleration kicks in
- **AccelerationMultiplier**: Multiplier for accelerated actions

## Relative CC Value Interpretation

### Standard Relative CC Format
- **Values 1-63**: Positive movement (clockwise)
  - Lower values = slower movement
  - Higher values = faster movement
- **Values 65-127**: Negative movement (counter-clockwise)
  - 65 = slowest negative movement
  - 127 = fastest negative movement
- **Value 64**: Usually indicates no movement (ignored)

### Movement Speed Detection
The action automatically detects movement speed based on the CC value:
- **Slow**: Values 1-10 or 65-74
- **Medium**: Values 11-31 or 75-95
- **Fast**: Values 32-63 or 96-127

## Complete Examples

### Basic Mouse Scrolling

```json
{
  "Description": "Jog wheel to mouse scroll",
  "InputType": "ControlChangeRelative",
  "Channel": 3,
  "ControlNumber": 30,
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
    "Description": "Basic jog wheel scrolling"
  }
}
```

### Accelerated Scrolling with State Machine

```json
{
  "Description": "Jog wheel with acceleration",
  "InputType": "ControlChangeRelative",
  "Channel": 3,
  "ControlNumber": 30,
  "Action": {
    "$type": "RelativeCCAction",
    "Parameters": {
      "PositiveAction": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "StateIncreaseAction",
              "Parameters": {
                "StateKey": "ScrollStatus",
                "Value": 1
              },
              "Description": "Increase scroll momentum"
            },
            {
              "$type": "StateConditionalAction",
              "Parameters": {
                "StateKey": "ScrollStatus",
                "Conditions": [
                  {
                    "ComparisonType": "GreaterThan",
                    "ComparisonValue": 5
                  }
                ],
                "LogicType": "Single",
                "ActionIfTrue": {
                  "$type": "SequenceAction",
                  "Parameters": {
                    "SubActions": [
                      {
                        "$type": "MouseScrollAction",
                        "Parameters": {
                          "Direction": "Up",
                          "Amount": 3
                        },
                        "Description": "Fast scroll up"
                      },
                      {
                        "$type": "StateSetAction",
                        "Parameters": {
                          "StateKey": "ScrollStatus",
                          "Value": 0
                        },
                        "Description": "Reset momentum"
                      }
                    ]
                  },
                  "Description": "Accelerated scroll and reset"
                },
                "ActionIfFalse": {
                  "$type": "MouseScrollAction",
                  "Parameters": {
                    "Direction": "Up",
                    "Amount": 1
                  },
                  "Description": "Normal scroll up"
                }
              },
              "Description": "Conditional scroll based on momentum"
            }
          ]
        },
        "Description": "Positive scroll with acceleration"
      },
      "NegativeAction": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "StateIncreaseAction",
              "Parameters": {
                "StateKey": "ScrollStatus",
                "Value": 1
              },
              "Description": "Increase scroll momentum"
            },
            {
              "$type": "StateConditionalAction",
              "Parameters": {
                "StateKey": "ScrollStatus",
                "Conditions": [
                  {
                    "ComparisonType": "GreaterThan",
                    "ComparisonValue": 5
                  }
                ],
                "LogicType": "Single",
                "ActionIfTrue": {
                  "$type": "SequenceAction",
                  "Parameters": {
                    "SubActions": [
                      {
                        "$type": "MouseScrollAction",
                        "Parameters": {
                          "Direction": "Down",
                          "Amount": 3
                        },
                        "Description": "Fast scroll down"
                      },
                      {
                        "$type": "StateSetAction",
                        "Parameters": {
                          "StateKey": "ScrollStatus",
                          "Value": 0
                        },
                        "Description": "Reset momentum"
                      }
                    ]
                  },
                  "Description": "Accelerated scroll and reset"
                },
                "ActionIfFalse": {
                  "$type": "MouseScrollAction",
                  "Parameters": {
                    "Direction": "Down",
                    "Amount": 1
                  },
                  "Description": "Normal scroll down"
                }
              },
              "Description": "Conditional scroll based on momentum"
            }
          ]
        },
        "Description": "Negative scroll with acceleration"
      },
      "AccelerationStateKey": "ScrollAccel",
      "AccelerationThreshold": 3,
      "AccelerationMultiplier": 2
    },
    "Description": "Accelerated jog wheel scrolling"
  }
}
```

### Navigation Controls

```json
{
  "Description": "Jog wheel for navigation",
  "InputType": "ControlChangeRelative",
  "Channel": 5,
  "ControlNumber": 30,
  "Action": {
    "$type": "RelativeCCAction",
    "Parameters": {
      "PositiveAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "Right"
        },
        "Description": "Navigate right"
      },
      "NegativeAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "Left"
        },
        "Description": "Navigate left"
      }
    },
    "Description": "Jog wheel navigation"
  }
}
```

### Volume Control

```json
{
  "Description": "Jog wheel volume control",
  "InputType": "ControlChangeRelative",
  "Channel": 1,
  "ControlNumber": 7,
  "Action": {
    "$type": "RelativeCCAction",
    "Parameters": {
      "PositiveAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "VolumeUp"
        },
        "Description": "Increase volume"
      },
      "NegativeAction": {
        "$type": "KeyPressReleaseAction",
        "Parameters": {
          "VirtualKeyCode": "VolumeDown"
        },
        "Description": "Decrease volume"
      }
    },
    "Description": "Relative volume control"
  }
}
```

## Use Cases

### Media Applications
- **Timeline Scrubbing**: Navigate through audio/video timelines
- **Parameter Control**: Adjust effect parameters with relative precision
- **Track Selection**: Navigate through track lists
- **Zoom Control**: Zoom in/out of waveforms or timelines

### Document Editing
- **Text Navigation**: Move cursor through documents
- **Zoom Control**: Zoom in/out of documents
- **Page Navigation**: Navigate through pages
- **Selection Control**: Extend/reduce text selections

### Gaming
- **Camera Control**: Rotate camera view
- **Menu Navigation**: Navigate through game menus
- **Weapon Selection**: Cycle through weapons
- **Inventory Browsing**: Navigate inventory items

### System Control
- **Volume Control**: System volume adjustment
- **Window Switching**: Navigate between open windows
- **Virtual Desktop**: Switch between virtual desktops
- **Brightness Control**: Adjust screen brightness

## Technical Notes

### Input Type Requirement
- Must use `ControlChangeRelative` as the InputType
- The system automatically converts absolute CC to relative interpretation
- Standard CC values (0-127) are interpreted as relative movement

### Performance Considerations
- Relative CC actions are optimized for high-frequency input
- State-based acceleration adds minimal overhead
- Actions execute immediately without buffering
- Suitable for real-time control applications

### State Management
- Acceleration states are automatically managed
- State keys should be unique per control
- States reset when profile changes
- No manual state cleanup required

### Error Handling
- Invalid CC values are ignored
- Missing actions default to no operation
- State errors don't affect CC processing
- Robust handling of rapid input

## Related Actions

- **MouseScrollAction**: Common target for relative CC
- **KeyPressReleaseAction**: Alternative target for navigation
- **StateConditionalAction**: Add conditional logic to relative actions
- **SequenceAction**: Combine multiple actions per movement

## Best Practices

1. **Use Appropriate Actions**: Match action types to the intended control behavior
2. **Consider Acceleration**: Add acceleration for better user experience with fast movements
3. **Test Sensitivity**: Verify the control feels responsive but not too sensitive
4. **Unique State Keys**: Use unique state keys for each relative control
5. **Handle Both Directions**: Always provide both positive and negative actions
6. **Document Behavior**: Clearly describe the expected control behavior

## Common Controller Mappings

### DJ Controllers
- **Jog Wheels**: Scratch/scrub control with RelativeCCAction
- **Browse Knobs**: Track/playlist navigation
- **Parameter Knobs**: Effect parameter control

### MIDI Controllers
- **Rotary Encoders**: Continuous parameter control
- **Jog Wheels**: Navigation and scrubbing
- **Data Entry**: Value adjustment controls

### Hardware Surfaces
- **Control Surfaces**: DAW parameter control
- **Mixing Consoles**: Fader and knob control
- **Video Controllers**: Timeline and parameter control

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\relative-cc-demo.json` for comprehensive relative CC action examples.
```
