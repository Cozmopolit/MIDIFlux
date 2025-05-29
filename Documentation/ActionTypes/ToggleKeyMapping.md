# Toggle Key Mapping

MIDIFlux provides comprehensive toggle key functionality through multiple action types, enabling both simple toggle behaviors and complex alternating sequences. Toggle actions are essential for creating stateful MIDI control interfaces.

## Toggle Action Types

### KeyToggleAction

Toggles the state of toggle keys like CapsLock, NumLock, and ScrollLock.

**Configuration Type**: `KeyToggleAction`

```json
{
  "$type": "KeyToggleAction",
  "Parameters": {
    "VirtualKeyCode": "CapsLock"
  },
  "Description": "Toggle CapsLock state"
}
```

**Supported Toggle Keys**:
- **CapsLock**: Toggle caps lock state
- **NumLock**: Toggle number lock state
- **Scroll**: Toggle scroll lock state

### AlternatingAction

Alternates between two different actions on successive triggers.

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
      "Description": "Play/Pause"
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

### State-Based Toggle

Create custom toggle behaviors using state management.

```json
{
  "$type": "StateConditionalAction",
  "Parameters": {
    "StateKey": "ToggleState",
    "Conditions": [
      {
        "ComparisonType": "Equals",
        "ComparisonValue": 0
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
              "StateKey": "ToggleState",
              "Value": 1
            },
            "Description": "Set state to 1"
          },
          {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "F1"
            },
            "Description": "First action"
          }
        ]
      },
      "Description": "Execute first action and toggle state"
    },
    "ActionIfFalse": {
      "$type": "SequenceAction",
      "Parameters": {
        "SubActions": [
          {
            "$type": "StateSetAction",
            "Parameters": {
              "StateKey": "ToggleState",
              "Value": 0
            },
            "Description": "Set state to 0"
          },
          {
            "$type": "KeyPressReleaseAction",
            "Parameters": {
              "VirtualKeyCode": "F2"
            },
            "Description": "Second action"
          }
        ]
      },
      "Description": "Execute second action and toggle state"
    }
  },
  "Description": "Custom state-based toggle"
}
```

## Complete Examples

### Media Player Toggle Control

```json
{
  "ProfileName": "Media Toggle Controls",
  "InitialStates": {
    "PlayState": 0
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Description": "Play/Pause toggle",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
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
                  "VirtualKeyCode": "MediaPlayPause"
                },
                "Description": "Pause"
              }
            },
            "Description": "Toggle play/pause"
          }
        },
        {
          "Description": "Mute toggle",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "AlternatingAction",
            "Parameters": {
              "FirstAction": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "VolumeMute"
                },
                "Description": "Mute"
              },
              "SecondAction": {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "VolumeMute"
                },
                "Description": "Unmute"
              }
            },
            "Description": "Toggle mute"
          }
        }
      ]
    }
  ]
}
```

### Application Mode Toggle

```json
{
  "Description": "Toggle between application modes",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 40,
  "Action": {
    "$type": "StateConditionalAction",
    "Parameters": {
      "StateKey": "AppMode",
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
                "StateKey": "AppMode",
                "Value": 2
              },
              "Description": "Switch to mode 2"
            },
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
                "VirtualKeyCode": "D2"
              },
              "Description": "Press 2"
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
        "Description": "Activate mode 2 (Ctrl+2)"
      },
      "ActionIfFalse": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "StateSetAction",
              "Parameters": {
                "StateKey": "AppMode",
                "Value": 1
              },
              "Description": "Switch to mode 1"
            },
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
                "VirtualKeyCode": "D1"
              },
              "Description": "Press 1"
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
        "Description": "Activate mode 1 (Ctrl+1)"
      }
    },
    "Description": "Toggle between application modes"
  }
}
```

### Multi-State Cycle Toggle

```json
{
  "Description": "Cycle through multiple states",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 42,
  "Action": {
    "$type": "StateConditionalAction",
    "Parameters": {
      "StateKey": "CycleState",
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
                "StateKey": "CycleState",
                "Value": 2
              },
              "Description": "Move to state 2"
            },
            {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "F2"
              },
              "Description": "State 2 action"
            }
          ]
        },
        "Description": "Transition to state 2"
      },
      "ActionIfFalse": {
        "$type": "StateConditionalAction",
        "Parameters": {
          "StateKey": "CycleState",
          "Conditions": [
            {
              "ComparisonType": "Equals",
              "ComparisonValue": 2
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
                    "StateKey": "CycleState",
                    "Value": 3
                  },
                  "Description": "Move to state 3"
                },
                {
                  "$type": "KeyPressReleaseAction",
                  "Parameters": {
                    "VirtualKeyCode": "F3"
                  },
                  "Description": "State 3 action"
                }
              ]
            },
            "Description": "Transition to state 3"
          },
          "ActionIfFalse": {
            "$type": "SequenceAction",
            "Parameters": {
              "SubActions": [
                {
                  "$type": "StateSetAction",
                  "Parameters": {
                    "StateKey": "CycleState",
                    "Value": 1
                  },
                  "Description": "Reset to state 1"
                },
                {
                  "$type": "KeyPressReleaseAction",
                  "Parameters": {
                    "VirtualKeyCode": "F1"
                  },
                  "Description": "State 1 action"
                }
              ]
            },
            "Description": "Reset to state 1"
          }
        },
        "Description": "Handle states 2, 3, or reset"
      }
    },
    "Description": "Three-state cycle toggle"
  }
}
```

### Toggle with Visual Feedback

```json
{
  "Description": "Toggle with command execution feedback",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 44,
  "Action": {
    "$type": "AlternatingAction",
    "Parameters": {
      "FirstAction": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "CommandExecutionAction",
              "Parameters": {
                "Command": "echo 'Feature Enabled'",
                "ShellType": "PowerShell",
                "RunHidden": false,
                "WaitForExit": false
              },
              "Description": "Show enable message"
            },
            {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "F9"
              },
              "Description": "Enable feature"
            }
          ]
        },
        "Description": "Enable with feedback"
      },
      "SecondAction": {
        "$type": "SequenceAction",
        "Parameters": {
          "SubActions": [
            {
              "$type": "CommandExecutionAction",
              "Parameters": {
                "Command": "echo 'Feature Disabled'",
                "ShellType": "PowerShell",
                "RunHidden": false,
                "WaitForExit": false
              },
              "Description": "Show disable message"
            },
            {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "F10"
              },
              "Description": "Disable feature"
            }
          ]
        },
        "Description": "Disable with feedback"
      }
    },
    "Description": "Toggle with visual feedback"
  }
}
```

## Use Cases

### Media Control
- **Play/Pause**: Toggle between play and pause states
- **Mute/Unmute**: Toggle audio mute state
- **Record**: Toggle recording on/off
- **Loop**: Toggle loop mode in media players

### Application Control
- **Mode Switching**: Toggle between different application modes
- **Tool Selection**: Cycle through tools in creative applications
- **View Modes**: Toggle between different view layouts
- **Feature Toggles**: Enable/disable application features

### Gaming
- **Weapon Cycling**: Rotate through available weapons
- **Ability Toggles**: Toggle special abilities on/off
- **View Modes**: Switch between first/third person views
- **HUD Elements**: Toggle UI elements visibility

### System Control
- **CapsLock**: Toggle caps lock state
- **NumLock**: Toggle number lock state
- **Display Modes**: Toggle between monitor configurations
- **Network Connections**: Toggle WiFi/Bluetooth on/off

## Technical Notes

### AlternatingAction Implementation
- Uses internal state management automatically
- State key is generated based on mapping configuration
- No manual state initialization required
- Thread-safe for concurrent access

### State-Based Toggles
- Require explicit state initialization in profile
- Provide more control over toggle behavior
- Support complex multi-state cycles
- Allow custom state logic and conditions

### KeyToggleAction Behavior
- Only works with actual toggle keys (CapsLock, NumLock, ScrollLock)
- Toggles the system state of these keys
- Independent of current key state
- May not work in all applications

### Performance Considerations
- Toggle actions are lightweight and efficient
- State operations add minimal overhead
- Suitable for real-time MIDI control
- No significant latency impact

## Related Actions

- **StateConditionalAction**: Essential for custom toggle logic
- **StateSetAction**: Manage toggle states manually
- **SequenceAction**: Combine toggle with other actions
- **ConditionalAction**: Add conditional logic to toggles
- **DelayAction**: Add timing to toggle sequences

## Best Practices

1. **Choose Appropriate Toggle Type**: Use AlternatingAction for simple toggles, state-based for complex logic
2. **Initialize States**: Set initial state values for state-based toggles
3. **Provide Feedback**: Consider visual or audio feedback for toggle state changes
4. **Test Thoroughly**: Verify toggle behavior works as expected in target applications
5. **Document States**: Clearly document what each toggle state represents
6. **Handle Edge Cases**: Consider what happens if toggle gets out of sync

## Common Toggle Patterns

### Binary Toggle
- **Two States**: On/Off, Enable/Disable, Show/Hide
- **Implementation**: AlternatingAction or simple state toggle
- **Use Cases**: Most common toggle scenario

### Cycle Toggle
- **Multiple States**: State 1 → State 2 → State 3 → State 1
- **Implementation**: State-based with conditional logic
- **Use Cases**: Tool selection, mode cycling, preset switching

### Conditional Toggle
- **Context-Dependent**: Different toggle behavior based on conditions
- **Implementation**: StateConditionalAction with nested logic
- **Use Cases**: Mode-dependent toggles, adaptive interfaces

### Momentary Toggle
- **Temporary State**: Toggle on press, revert on release
- **Implementation**: Combine with NoteOff mapping or timer
- **Use Cases**: Push-to-talk, temporary mode switches

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\toggle-actions-demo.json` for comprehensive toggle examples.
```
