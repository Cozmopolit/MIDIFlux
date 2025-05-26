# Sequence Actions (Macros)

MIDIFlux supports complex action sequences (macros) through the SequenceAction system. This allows you to create sophisticated workflows that execute multiple actions in order when triggered by a single MIDI event.

## SequenceAction

Executes a list of actions sequentially in the order they are defined.

**Configuration Type**: `SequenceConfig`

**How It Works**:
- Actions are executed in the order they appear in the SubActions array
- Each action completes before the next one starts
- Supports any combination of simple and complex actions
- Nested sequences are supported for complex workflows

## Configuration Format

```json
{
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
  "Description": "Copy shortcut (Ctrl+C)"
}
```

## Supported Action Types in Sequences

### Simple Actions
- **KeyPressReleaseConfig**: Press and release a key
- **KeyDownConfig**: Press and hold a key
- **KeyUpConfig**: Release a key
- **KeyToggleConfig**: Toggle key state (like CapsLock)
- **MouseClickConfig**: Click mouse buttons
- **MouseScrollConfig**: Scroll mouse wheel
- **CommandExecutionConfig**: Execute shell commands
- **DelayConfig**: Wait for specified time
- **GameControllerButtonConfig**: Press game controller buttons
- **GameControllerAxisConfig**: Set game controller axis values
- **MidiOutputConfig**: Send MIDI messages

### Complex Actions
- **SequenceConfig**: Nested sequences (sub-macros)
- **ConditionalConfig**: Conditional execution within sequences
- **AlternatingActionConfig**: Alternating actions within sequences
- **StateConditionalConfig**: State-based conditional actions
- **SetStateConfig**: Set state values during sequence execution

## Complete Mapping Examples

### Basic Key Combination Sequence

```json
{
  "Id": "copy-paste-sequence",
  "Description": "Copy then paste sequence",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
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
        "Description": "Press C (Copy)"
      },
      {
        "$type": "KeyUpConfig",
        "VirtualKeyCode": 162,
        "Description": "Release Ctrl"
      },
      {
        "$type": "DelayConfig",
        "DelayMs": 100,
        "Description": "Short delay"
      },
      {
        "$type": "KeyDownConfig",
        "VirtualKeyCode": 162,
        "Description": "Press Ctrl again"
      },
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 86,
        "Description": "Press V (Paste)"
      },
      {
        "$type": "KeyUpConfig",
        "VirtualKeyCode": 162,
        "Description": "Release Ctrl"
      }
    ],
    "Description": "Copy and paste sequence"
  }
}
```

### Mixed Action Types Sequence

```json
{
  "Id": "complex-workflow",
  "Description": "Complex workflow with multiple action types",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 27,
        "Description": "Press Escape (clear selection)"
      },
      {
        "$type": "DelayConfig",
        "DelayMs": 200,
        "Description": "Wait 200ms"
      },
      {
        "$type": "MouseClickConfig",
        "Button": "Left",
        "Description": "Left click"
      },
      {
        "$type": "DelayConfig",
        "DelayMs": 100,
        "Description": "Wait 100ms"
      },
      {
        "$type": "CommandExecutionConfig",
        "Command": "echo 'Workflow executed'",
        "ShellType": "PowerShell",
        "RunHidden": true,
        "WaitForExit": false,
        "Description": "Log workflow execution"
      }
    ],
    "Description": "Multi-action workflow"
  }
}
```

### Nested Sequences

```json
{
  "Id": "nested-sequence",
  "Description": "Sequence containing sub-sequences",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 72,
        "Description": "Press H (start)"
      },
      {
        "$type": "SequenceConfig",
        "SubActions": [
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 69,
            "Description": "Press E"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 76,
            "Description": "Press L"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 76,
            "Description": "Press L"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 79,
            "Description": "Press O"
          }
        ],
        "Description": "Type 'ELLO'"
      },
      {
        "$type": "DelayConfig",
        "DelayMs": 500,
        "Description": "Wait half second"
      },
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 32,
        "Description": "Press Space"
      },
      {
        "$type": "SequenceConfig",
        "SubActions": [
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 87,
            "Description": "Press W"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 79,
            "Description": "Press O"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 82,
            "Description": "Press R"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 76,
            "Description": "Press L"
          },
          {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 68,
            "Description": "Press D"
          }
        ],
        "Description": "Type 'WORLD'"
      }
    ],
    "Description": "Type 'HELLO WORLD' with nested sequences"
  }
}
```

### Sequence with Conditional Logic

```json
{
  "Id": "conditional-sequence",
  "Description": "Sequence with conditional execution",
  "InputType": "ControlChange",
  "Channel": 1,
  "ControlNumber": 7,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "SetStateConfig",
        "StateKey": "LastCCValue",
        "StateValue": 0,
        "Description": "Initialize state (will be overwritten by MIDI value)"
      },
      {
        "$type": "ConditionalConfig",
        "Conditions": [
          {
            "ComparisonType": "GreaterThan",
            "ComparisonValue": 64
          }
        ],
        "LogicType": "Single",
        "ActionIfTrue": {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 72,
          "Description": "Press H (high value)"
        },
        "ActionIfFalse": {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 76,
          "Description": "Press L (low value)"
        },
        "Description": "Conditional key press based on CC value"
      },
      {
        "$type": "DelayConfig",
        "DelayMs": 100,
        "Description": "Brief delay"
      },
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 13,
        "Description": "Press Enter (confirm)"
      }
    ],
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
- Use DelayConfig sparingly to avoid blocking
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
- **SetStateAction**: Modify states during sequence execution
- **DelayAction**: Add timing control between actions

## Best Practices

1. **Keep Sequences Focused**: Design sequences for specific, well-defined tasks
2. **Use Descriptive Names**: Provide clear descriptions for each action in the sequence
3. **Add Appropriate Delays**: Use DelayConfig to ensure proper timing between actions
4. **Test Thoroughly**: Verify sequences work correctly in target applications
5. **Handle Edge Cases**: Consider what happens if the sequence is interrupted
6. **Document Complex Logic**: Use clear descriptions for complex nested sequences



