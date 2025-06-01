# Sequence Actions (Macros)

MIDIFlux supports complex action sequences (macros) through the SequenceAction system. This allows you to create sophisticated workflows that execute multiple actions in order when triggered by a single MIDI event.

## SequenceAction

Executes a list of actions sequentially in the order they are defined.

**Configuration Type**: `SequenceAction`

**How It Works**:
- Actions are executed in the order they appear in the SubActions array
- Each action completes before the next one starts
- Supports any combination of simple and complex actions
- Nested sequences are supported for complex workflows

## Configuration Format

```json
{
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
  "Description": "Copy shortcut (Ctrl+C)"
}
```

## Supported Action Types in Sequences

### Simple Actions
- **KeyPressReleaseAction**: Press and release a key
- **KeyDownAction**: Press and hold a key
- **KeyUpAction**: Release a key
- **KeyToggleAction**: Toggle key state (like CapsLock)
- **MouseClickAction**: Click mouse buttons
- **MouseScrollAction**: Scroll mouse wheel
- **CommandExecutionAction**: Execute shell commands
- **DelayAction**: Wait for specified time
- **GameControllerButtonAction**: Press game controller buttons
- **GameControllerAxisAction**: Set game controller axis values
- **MidiOutputAction**: Send MIDI messages to external devices

### Complex Actions
- **SequenceAction**: Nested sequences (sub-macros)
- **ConditionalAction**: Conditional execution within sequences
- **AlternatingAction**: Alternating actions within sequences
- **StateConditionalAction**: State-based conditional actions
- **StateSetAction**, **StateIncreaseAction**, **StateDecreaseAction**: State management during sequence execution

## Complete Mapping Examples

### Basic Key Combination Sequence

```json
{
  "Description": "Copy then paste sequence",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
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
          "Description": "Press C (Copy)"
        },
        {
          "$type": "KeyUpAction",
          "Parameters": {
            "VirtualKeyCode": "ControlKey"
          },
          "Description": "Release Ctrl"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "Milliseconds": 100
          },
          "Description": "Short delay"
        },
        {
          "$type": "KeyDownAction",
          "Parameters": {
            "VirtualKeyCode": "ControlKey",
            "AutoReleaseAfterMs": null
          },
          "Description": "Press Ctrl again"
        },
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "V"
          },
          "Description": "Press V (Paste)"
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
    "Description": "Copy and paste sequence"
  }
}
```

### Mixed Action Types Sequence

```json
{
  "Description": "Complex workflow with multiple action types",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "Escape"
          },
          "Description": "Press Escape (clear selection)"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "Milliseconds": 200
          },
          "Description": "Wait 200ms"
        },
        {
          "$type": "MouseClickAction",
          "Parameters": {
            "Button": "Left"
          },
          "Description": "Left click"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "Milliseconds": 100
          },
          "Description": "Wait 100ms"
        },
        {
          "$type": "CommandExecutionAction",
          "Parameters": {
            "Command": "echo 'Workflow executed'",
            "ShellType": "PowerShell",
            "RunHidden": true,
            "WaitForExit": false
          },
          "Description": "Log workflow execution"
        }
      ]
    },
    "Description": "Multi-action workflow"
  }
}
```

### Nested Sequences

```json
{
  "Description": "Sequence containing sub-sequences",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "H"
          },
          "Description": "Press H (start)"
        },
        {
          "$type": "SequenceAction",
          "Parameters": {
            "SubActions": [
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "E"
                },
                "Description": "Press E"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "L"
                },
                "Description": "Press L"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "L"
                },
                "Description": "Press L"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "O"
                },
                "Description": "Press O"
              }
            ]
          },
          "Description": "Type 'ELLO'"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "Milliseconds": 500
          },
          "Description": "Wait half second"
        },
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "Space"
          },
          "Description": "Press Space"
        },
        {
          "$type": "SequenceAction",
          "Parameters": {
            "SubActions": [
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "W"
                },
                "Description": "Press W"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "O"
                },
                "Description": "Press O"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "R"
                },
                "Description": "Press R"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "L"
                },
                "Description": "Press L"
              },
              {
                "$type": "KeyPressReleaseAction",
                "Parameters": {
                  "VirtualKeyCode": "D"
                },
                "Description": "Press D"
              }
            ]
          },
          "Description": "Type 'WORLD'"
        }
      ]
    },
    "Description": "Type 'HELLO WORLD' with nested sequences"
  }
}
```

### Sequence with Conditional Logic

```json
{
  "Description": "Sequence with conditional execution",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 7,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "StateSetAction",
          "Parameters": {
            "StateKey": "LastCCValue",
            "Value": 0
          },
          "Description": "Initialize state (will be overwritten by MIDI value)"
        },
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
                "VirtualKeyCode": "H"
              },
              "Description": "Press H (high value)"
            },
            "ActionIfFalse": {
              "$type": "KeyPressReleaseAction",
              "Parameters": {
                "VirtualKeyCode": "L"
              },
              "Description": "Press L (low value)"
            }
          },
          "Description": "Conditional key press based on CC value"
        },
        {
          "$type": "DelayAction",
          "Parameters": {
            "Milliseconds": 100
          },
          "Description": "Brief delay"
        },
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": "Return"
          },
          "Description": "Press Enter (confirm)"
        }
      ]
    },
    "Description": "Conditional sequence based on MIDI value"
  }
}
```

## Use Cases

### Productivity Workflows
- **Document Formatting**: Apply multiple formatting operations in sequence
- **File Operations**: Save, close, and open new document workflows
- **Application Switching**: Complex window management sequences

### Media Production
- **Recording Workflows**: Arm track, start recording, set levels in sequence
- **Effect Chains**: Apply multiple effects in specific order
- **Transport Control**: Complex playback control sequences

### Gaming
- **Combo Moves**: Execute complex game move sequences
- **Inventory Management**: Multi-step item management workflows
- **Communication**: Send pre-defined message sequences

### Creative Applications
- **Tool Workflows**: Switch tools and apply settings in sequence
- **Layer Operations**: Complex layer manipulation workflows
- **Rendering Pipelines**: Multi-step rendering and export sequences

## Technical Notes

### Execution Model
- Actions execute sequentially in the order defined
- Each action completes before the next one starts
- Synchronous execution for predictable timing
- Error in one action stops the sequence

### Performance Considerations
- Sequences execute on the main thread for timing accuracy
- Long-running sequences may block other MIDI processing
- Use DelayAction sparingly to avoid blocking
- Consider breaking very long sequences into smaller ones

### Error Handling
- Sequence stops if any action fails
- Error details are logged for debugging
- Failed sequences don't affect other mappings
- State changes before failure are not rolled back

### Nesting Limitations
- Nested sequences are fully supported
- No practical depth limit for nesting
- Each nested sequence is a separate action
- Complex nesting may impact readability

## Related Actions

- **ConditionalAction**: Add conditional logic within sequences
- **AlternatingAction**: Create toggle behaviors in sequences
- **StateConditionalAction**: Use state-based logic in sequences
- **StateSetAction**: Modify states during sequence execution
- **DelayAction**: Add timing control between actions

## Best Practices

1. **Keep Sequences Focused**: Design sequences for specific, well-defined tasks
2. **Use Descriptive Names**: Provide clear descriptions for each action in the sequence
3. **Add Appropriate Delays**: Use DelayAction to ensure proper timing between actions
4. **Test Thoroughly**: Verify sequences work correctly in target applications
5. **Handle Edge Cases**: Consider what happens if the sequence is interrupted
6. **Document Complex Logic**: Use clear descriptions for complex nested sequences

## Example Files

See `%AppData%\MIDIFlux\profiles\examples\advanced-macros.json` for comprehensive sequence examples.
