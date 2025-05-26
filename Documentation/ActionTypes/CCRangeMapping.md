# Conditional Actions (CC Range Mapping)

Conditional Actions allow you to execute different actions based on MIDI value ranges. This is particularly useful for Control Change (CC) messages where you want different behaviors based on knob or fader positions.

## ConditionalAction

Executes different actions based on MIDI value comparisons.

**Configuration Type**: `ConditionalConfig`

**How It Works**:
- Receives MIDI values (0-127 for CC, 0-127 for velocity, etc.)
- Compares the value against defined conditions
- Executes the appropriate action based on the comparison result

## Configuration Format

```json
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
    "VirtualKeyCode": 65,
    "Description": "Press A key"
  },
  "ActionIfFalse": {
    "$type": "KeyPressReleaseConfig",
    "VirtualKeyCode": 66,
    "Description": "Press B key"
  },
  "Description": "A if value > 64, B otherwise"
}
```

## Condition Types

### Comparison Types
- **Equals**: MIDI value equals the comparison value
- **GreaterThan**: MIDI value is greater than the comparison value
- **LessThan**: MIDI value is less than the comparison value

### Logic Types
- **Single**: Single condition check
- **And**: All conditions must be true (for multiple conditions)

## Complete Mapping Examples

### Basic CC Range Mapping

Map a knob to different keys based on position:

```json
{
  "Id": "knob-to-keys",
  "Description": "Map knob position to different keys",
  "InputType": "ControlChange",
  "Channel": 1,
  "ControlNumber": 7,
  "Action": {
    "$type": "ConditionalConfig",
    "Conditions": [
      {
        "ComparisonType": "LessThan",
        "ComparisonValue": 32
      }
    ],
    "LogicType": "Single",
    "ActionIfTrue": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 49,
      "Description": "Press 1 key (low range)"
    },
    "ActionIfFalse": {
      "$type": "ConditionalConfig",
      "Conditions": [
        {
          "ComparisonType": "LessThan",
          "ComparisonValue": 64
        }
      ],
      "LogicType": "Single",
      "ActionIfTrue": {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 50,
        "Description": "Press 2 key (mid range)"
      },
      "ActionIfFalse": {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 51,
        "Description": "Press 3 key (high range)"
      },
      "Description": "Nested condition for mid/high range"
    },
    "Description": "Three-way knob mapping"
  }
}



### Multiple Conditions with AND Logic

```json
{
  "Id": "complex-conditional",
  "Description": "Multiple condition example",
  "InputType": "ControlChange",
  "Channel": 1,
  "ControlNumber": 10,
  "Action": {
    "$type": "ConditionalConfig",
    "Conditions": [
      {
        "ComparisonType": "GreaterThan",
        "ComparisonValue": 32
      },
      {
        "ComparisonType": "LessThan",
        "ComparisonValue": 96
      }
    ],
    "LogicType": "And",
    "ActionIfTrue": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 77,
      "Description": "Press M key (middle range)"
    },
    "ActionIfFalse": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 69,
      "Description": "Press E key (edge ranges)"
    },
    "Description": "M if 32 < value < 96, E otherwise"
  }
}
```

### Velocity-Based Actions

Use conditional actions with Note On/Off events based on velocity:

```json
{
  "Id": "velocity-conditional",
  "Description": "Different actions based on velocity",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "ConditionalConfig",
    "Conditions": [
      {
        "ComparisonType": "GreaterThan",
        "ComparisonValue": 100
      }
    ],
    "LogicType": "Single",
    "ActionIfTrue": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 72,
      "Description": "Press H key (hard hit)"
    },
    "ActionIfFalse": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 83,
      "Description": "Press S key (soft hit)"
    },
    "Description": "H for hard hits (>100), S for soft hits"
  }
}
```

## Use Cases

### Media Control
- **Volume Zones**: Different volume control behaviors based on fader position
- **Transport Control**: Different playback speeds based on knob position
- **Effect Control**: Enable/disable effects based on threshold values

### Gaming
- **Weapon Selection**: Different weapons based on controller position
- **Speed Control**: Walking vs running based on pressure/velocity
- **Difficulty Scaling**: Different game behaviors based on performance metrics

### Productivity
- **Zoom Levels**: Different zoom increments based on control position
- **Tool Selection**: Different tools based on knob/fader zones
- **Opacity Control**: Discrete opacity levels from continuous control

### Creative Applications
- **Brush Size**: Different brush sizes based on pressure/velocity
- **Layer Selection**: Different layers based on controller position
- **Color Palette**: Different colors based on control zones

## Technical Notes

### Performance
- Conditional actions execute synchronously for minimal latency
- Comparison operations are optimized for real-time MIDI processing
- Nested conditionals are supported for complex logic

### Value Handling
- MIDI values are always integers (0-127 for most message types)
- Comparison values must be within valid MIDI range
- Exact equality comparisons work reliably with integer values

### Logic Evaluation
- Single condition: Simple true/false evaluation
- AND logic: All conditions must be true for ActionIfTrue
- No OR logic support (use nested conditionals instead)

## Related Actions

- **StateConditionalAction**: Use state values instead of MIDI values for conditions
- **SequenceAction**: Combine conditional logic with action sequences
- **AlternatingAction**: Simple two-state toggle without value-based conditions
- **SetStateAction**: Set states based on conditional results

## Best Practices

1. **Use Clear Thresholds**: Choose comparison values that create distinct zones
2. **Test Edge Cases**: Verify behavior at boundary values
3. **Document Logic**: Use clear descriptions for complex conditional logic
4. **Avoid Overlap**: Design conditions to avoid ambiguous ranges
5. **Consider Hysteresis**: For noisy controls, use state-based logic to prevent flickering

