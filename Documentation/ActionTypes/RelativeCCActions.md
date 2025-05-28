# Relative CC Actions

RelativeCCAction is a complex action that handles relative MIDI controllers like scratch wheels, jog wheels, and endless encoders. These controllers send incremental values rather than absolute positions.

## Overview

Relative controllers typically send values around a center point (usually 64) to indicate direction and magnitude:
- Values above center = positive/increase direction
- Values below center = negative/decrease direction
- Distance from center = magnitude/speed of movement

The RelativeCCAction decodes these values using SignMagnitude encoding and executes increase/decrease actions multiple times based on the magnitude.

## Configuration

**Configuration Type**: `RelativeCCConfig`

**Required Properties**:
- `IncreaseAction`: Action to execute for positive values
- `DecreaseAction`: Action to execute for negative values

## Value Interpretation (SignMagnitude Encoding)

- Values 1-63: Positive direction (magnitude 1-63)
- Value 64: No change (ignored)
- Values 65-127: Negative direction (magnitude 1-63)

**Example**: Value 67 = +3, executes IncreaseAction 3 times

## Basic Examples

### Mouse Scroll Wheel
```json
{
  "InputType": "ControlChange",
  "ControllerNumber": 16,
  "Action": {
    "$type": "RelativeCCConfig",
    "IncreaseAction": {
      "$type": "MouseScrollConfig",
      "Direction": "Up",
      "Amount": 1
    },
    "DecreaseAction": {
      "$type": "MouseScrollConfig",
      "Direction": "Down",
      "Amount": 1
    }
  }
}
```

### System Volume Control
```json
{
  "InputType": "ControlChange",
  "ControllerNumber": 17,
  "Action": {
    "$type": "RelativeCCConfig",
    "IncreaseAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 175,
      "Description": "Volume Up"
    },
    "DecreaseAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 174,
      "Description": "Volume Down"
    }
  }
}
```

## Advanced Examples

### Complex Action Sequences
You can use any action type as increase/decrease actions, including complex sequences:

```json
{
  "InputType": "ControlChange",
  "ControllerNumber": 18,
  "Action": {
    "$type": "RelativeCCConfig",
    "IncreaseAction": {
      "$type": "SequenceConfig",
      "SubActions": [
        {
          "$type": "KeyDownConfig",
          "VirtualKeyCode": 162,
          "Description": "Press Ctrl"
        },
        {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 187,
          "Description": "Press Plus (Zoom In)"
        },
        {
          "$type": "KeyUpConfig",
          "VirtualKeyCode": 162,
          "Description": "Release Ctrl"
        }
      ]
    },
    "DecreaseAction": {
      "$type": "SequenceConfig",
      "SubActions": [
        {
          "$type": "KeyDownConfig",
          "VirtualKeyCode": 162,
          "Description": "Press Ctrl"
        },
        {
          "$type": "KeyPressReleaseConfig",
          "VirtualKeyCode": 189,
          "Description": "Press Minus (Zoom Out)"
        },
        {
          "$type": "KeyUpConfig",
          "VirtualKeyCode": 162,
          "Description": "Release Ctrl"
        }
      ]
    }
  }
}
```

## Behavior Details

### Magnitude Mapping
- **Fast movement**: Higher magnitude values execute the action multiple times
- **Slow movement**: Lower magnitude values execute the action fewer times
- **No movement**: Zero magnitude is ignored (no action executed)

### Example Value Mappings
- MIDI value 65 (+1) → Execute IncreaseAction 1 time
- MIDI value 67 (+3) → Execute IncreaseAction 3 times
- MIDI value 63 (-1) → Execute DecreaseAction 1 time
- MIDI value 61 (-3) → Execute DecreaseAction 3 times
- MIDI value 64 (0) → No action (ignored)

### Error Handling
- If one iteration fails, remaining iterations continue
- Each failed iteration shows a separate error message
- Actions are executed sequentially, not in parallel

## Common Use Cases

1. **Mouse scrolling** with scratch wheels
2. **Volume control** with endless encoders
3. **Zoom control** in applications
4. **Brightness adjustment**
5. **Timeline scrubbing** in media applications
6. **Parameter adjustment** in software

## Tips

- Use simple actions (like MouseScroll) for best performance
- Test with your specific controller to verify the value ranges
- Consider the natural feel - fast wheel movement should result in multiple rapid actions
- Most scratch wheels and encoders work with the standard SignMagnitude encoding
