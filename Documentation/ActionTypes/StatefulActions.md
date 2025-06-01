# Stateful Actions

MIDIFlux includes a comprehensive state management system that enables complex conditional behaviors, toggles, and stateful workflows. States are profile-scoped and thread-safe for real-time MIDI processing.

## State Management System

### State Types
- **User-defined states**: Alphanumeric keys defined in profile configuration
- **Internal states**: Auto-generated with asterisk prefix (e.g., `*KeyA` for keyboard state)
- **Profile-scoped**: States are initialized per profile and cleared on profile changes
- **Thread-safe**: Concurrent access supported for real-time MIDI processing

### State Keys
- **User states**: Must be fully alphanumeric (e.g., "ScrollRate", "Mode1", "Counter")
- **Internal states**: Use asterisk prefix (e.g., "*KeyA", "*Button1")
- **Case sensitive**: "Mode" and "mode" are different states

## State Actions

### StateSetAction

Sets a state to a specific value.

**Configuration Type**: `StateSetAction`

```json
{
  "$type": "StateSetAction",
  "Parameters": {
    "StateKey": "ScrollRate",
    "Value": 5
  },
  "Description": "Set scroll rate to 5"
}
```

### StateIncreaseAction

Increases a state value by a specified amount.

**Configuration Type**: `StateIncreaseAction`

```json
{
  "$type": "StateIncreaseAction",
  "Parameters": {
    "StateKey": "Counter",
    "Value": 1
  },
  "Description": "Increment counter by 1"
}
```

### StateDecreaseAction

Decreases a state value by a specified amount.

**Configuration Type**: `StateDecreaseAction`

```json
{
  "$type": "StateDecreaseAction",
  "Parameters": {
    "StateKey": "Volume",
    "Value": 10
  },
  "Description": "Decrease volume by 10"
}
```

## Conditional Actions

### StateConditionalAction

Executes actions based on state values using flexible comparison logic.

**Configuration Type**: `StateConditionalAction`

```json
{
  "$type": "StateConditionalAction",
  "Parameters": {
    "StateKey": "Mode",
    "Conditions": [
      {
        "ComparisonType": "Equals",
        "ComparisonValue": 1
      }
    ],
    "LogicType": "Single",
    "ActionIfTrue": {
      "$type": "KeyPressReleaseAction",
      "Parameters": {
        "VirtualKeyCode": "A"
      },
      "Description": "Press A in mode 1"
    },
    "ActionIfFalse": {
      "$type": "KeyPressReleaseAction",
      "Parameters": {
        "VirtualKeyCode": "B"
      },
      "Description": "Press B in other modes"
    }
  },
  "Description": "Mode-based key selection"
}
```

### ConditionalAction

Executes actions based on MIDI input values.

**Configuration Type**: `ConditionalAction`

```json
{
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
        "VirtualKeyCode": "Up"
      },
      "Description": "Scroll up for high values"
    },
    "ActionIfFalse": {
      "$type": "KeyPressReleaseAction",
      "Parameters": {
        "VirtualKeyCode": "Down"
      },
      "Description": "Scroll down for low values"
    }
  },
  "Description": "Value-based scrolling"
}
```

## Comparison Types

- **Equals**: State/value equals the comparison value
- **GreaterThan**: State/value is greater than the comparison value
- **LessThan**: State/value is less than the comparison value

## Logic Types

- **Single**: Use only the first condition
- **And**: All conditions must be true
- **Or**: At least one condition must be true

## Complete Examples

### Toggle Mode System

```json
{
  "ProfileName": "Toggle Mode Example",
  "InitialStates": {
    "Mode": 1
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Description": "Toggle between modes",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "StateConditionalAction",
            "Parameters": {
              "StateKey": "Mode",
              "Conditions": [
                {
                  "ComparisonType": "Equals",
                  "ComparisonValue": 1
                }
              ],
              "LogicType": "Single",
              "ActionIfTrue": {
                "$type": "StateSetAction",
                "Parameters": {
                  "StateKey": "Mode",
                  "Value": 2
                },
                "Description": "Switch to mode 2"
              },
              "ActionIfFalse": {
                "$type": "StateSetAction",
                "Parameters": {
                  "StateKey": "Mode",
                  "Value": 1
                },
                "Description": "Switch to mode 1"
              }
            },
            "Description": "Toggle between mode 1 and 2"
          }
        },
        {
          "Description": "Mode-dependent action",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "StateConditionalAction",
            "Parameters": {
              "StateKey": "Mode",
              "Conditions": [
                {
                  "ComparisonType": "Equals",
                  "ComparisonValue": 1
                }
              ],
              "LogicType": "Single",
              "ActionIfTrue": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "A"
                },
                "Description": "Press A in mode 1"
              },
              "ActionIfFalse": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "B"
                },
                "Description": "Press B in mode 2"
              }
            },
            "Description": "Different actions per mode"
          }
        }
      ]
    }
  ]
}
```

### Counter System

```json
{
  "Description": "Increment counter and conditional action",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "StateIncreaseAction",
          "Parameters": {
            "StateKey": "PressCount",
            "Value": 1
          },
          "Description": "Increment press counter"
        },
        {
          "$type": "StateConditionalAction",
          "Parameters": {
            "StateKey": "PressCount",
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
                    "$type": "KeyPressReleaseAction",
                    "Parameters": {
                      "VirtualKeyCode": "Return"
                    },
                    "Description": "Press Enter after 5 presses"
                  },
                  {
                    "$type": "StateSetAction",
                    "Parameters": {
                      "StateKey": "PressCount",
                      "Value": 0
                    },
                    "Description": "Reset counter"
                  }
                ]
              },
              "Description": "Execute and reset after threshold"
            },
            "ActionIfFalse": {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "Space"
              },
              "Description": "Press Space for normal presses"
            }
          },
          "Description": "Conditional action based on press count"
        }
      ]
    },
    "Description": "Counter-based conditional execution"
  }
}
```

### Variable Scroll Rate System

```json
{
  "Description": "Increase scroll rate with state machine",
  "InputType": "ControlChangeRelative",
  "Channel": 1,
  "ControlNumber": 30,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "StateIncreaseAction",
          "Parameters": {
            "StateKey": "ScrollStatus",
            "Value": 1
          },
          "Description": "Increase scroll status"
        },
        {
          "$type": "StateConditionalAction",
          "Parameters": {
            "StateKey": "ScrollStatus",
            "Conditions": [
              {
                "ComparisonType": "GreaterThan",
                "ComparisonValue": 10
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
                      "Amount": 5
                    },
                    "Description": "Fast scroll (5 units)"
                  },
                  {
                    "$type": "StateSetAction",
                    "Parameters": {
                      "StateKey": "ScrollStatus",
                      "Value": 0
                    },
                    "Description": "Reset scroll status"
                  }
                ]
              },
              "Description": "Fast scroll and reset"
            },
            "ActionIfFalse": {
              "$type": "MouseScrollAction",
              "Parameters": {
                "Direction": "Up",
                "Amount": 1
              },
              "Description": "Normal scroll (1 unit)"
            }
          },
          "Description": "Variable rate scrolling"
        }
      ]
    },
    "Description": "State-based scroll rate control"
  }
}
```

## AlternatingAction

A convenience wrapper around the stateful action system for simple toggle behaviors.

**Configuration Type**: `AlternatingAction`

```json
{
  "$type": "AlternatingAction",
  "Parameters": {
    "FirstAction": {
      "$type": "KeyPressReleaseAction",
      "Parameters": {
        "VirtualKeyCode": "MediaPlayPause"
      },
      "Description": "Play"
    },
    "SecondAction": {
      "$type": "KeyPressReleaseAction",
      "Parameters": {
        "VirtualKeyCode": "MediaStop"
      },
      "Description": "Stop"
    }
  },
  "Description": "Alternate between play and stop"
}
```

## Use Cases

### Media Control
- **Transport States**: Play/pause/stop state management
- **Track Selection**: Cycle through tracks with state tracking
- **Effect Toggles**: Enable/disable effects based on state

### Gaming
- **Weapon Cycling**: Rotate through weapons with state tracking
- **Mode Switching**: Toggle between game modes
- **Combo Counters**: Track button press sequences

### Productivity
- **Application Modes**: Switch between different application states
- **Tool Selection**: Cycle through tools in creative applications
- **Workspace Management**: Toggle between different workspace configurations

### Creative Workflows
- **Layer Management**: Toggle layer visibility based on state
- **Preset Cycling**: Rotate through effect presets
- **Recording States**: Manage complex recording state machines

## Technical Notes

### State Storage
- States are stored in memory during profile execution
- State values are integers (positive, negative, or zero)
- States persist until profile change or application restart
- No automatic state persistence between sessions

### Performance
- State operations are optimized for real-time MIDI processing
- Thread-safe concurrent access to state values
- Minimal overhead for state reads and writes
- State comparisons use efficient integer operations

### Initialization
- User-defined states can be initialized in profile configuration
- Internal states are automatically initialized to 0
- Uninitialized states default to 0 when first accessed
- State initialization happens at profile load time

## Related Actions

- **SequenceAction**: Combine state actions with other actions
- **ConditionalAction**: Use MIDI values for conditional logic
- **DelayAction**: Add timing to state-based workflows
- **AlternatingAction**: Simple two-state toggle behavior

## Best Practices

1. **Use Descriptive State Keys**: Choose clear, meaningful names for states
2. **Initialize Important States**: Set initial values in profile configuration
3. **Document State Logic**: Provide clear descriptions for complex state behaviors
4. **Keep States Simple**: Avoid overly complex state machines
5. **Test State Transitions**: Verify state changes work as expected
6. **Consider State Cleanup**: Remember states persist until profile change

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\conditional-action-demo.json` and `%AppData%\MIDIFlux\profiles\examples\alternating-action-demo.json` for comprehensive stateful action examples.
```
