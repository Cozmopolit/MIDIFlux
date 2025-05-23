# CC Range Mapping

CC Range Mapping allows you to map different ranges of a MIDI Control Change (CC) value to different actions. This is useful for:

- Mapping a knob or fader to a sequence of keyboard keys
- Creating complex control schemes where different positions of a control trigger different actions
- Implementing "zones" on a continuous controller

## How It Works

When you turn a knob or move a fader on your MIDI controller, it sends CC messages with values from 0 to 127. CC Range Mapping lets you define specific ranges within this 0-127 range and assign different actions to each range.

The mapping is stateful, meaning:
- It tracks the current active range
- It only triggers an action when moving from one range to another
- It prevents rapid repeated triggering of the same action

## Configuration

CC Range Mappings are defined in the `ccRangeMappings` section of your configuration file:

```json
"ccRangeMappings": [
  {
    "controlNumber": 7,
    "handlerType": "CCRange",
    "description": "Opacity control using keys 1-0",
    "ranges": [
      {
        "minValue": 0,
        "maxValue": 12,
        "action": {
          "type": "KeyPress",
          "key": "1"
        }
      },
      {
        "minValue": 13,
        "maxValue": 25,
        "action": {
          "type": "KeyPress",
          "key": "2"
        }
      }
      // More ranges...
    ]
  }
]
```

### Parameters

- **controlNumber**: The MIDI CC number (0-127)
- **handlerType**: Must be "CCRange"
- **description**: Optional description of this mapping
- **ranges**: Array of value ranges and their associated actions

Each range has:
- **minValue**: The minimum CC value for this range (inclusive)
- **maxValue**: The maximum CC value for this range (inclusive)
- **action**: The action to perform when the CC value is in this range

### Action Types

CC Range Mapping supports two types of actions:

1. **KeyPress**: Press and release a key
   ```json
   "action": {
     "type": "KeyPress",
     "key": "1"
   }
   ```
   or with virtual key codes:
   ```json
   "action": {
     "type": "KeyPress",
     "virtualKeyCode": 65,
     "modifiers": [16]
   }
   ```

2. **CommandExecution**: Execute a shell command
   ```json
   "action": {
     "type": "CommandExecution",
     "command": "echo 'Hello World'",
     "shellType": 0,
     "runHidden": false,
     "waitForExit": true
   }
   ```



## Example: Mapping Keys 1-0 to a Knob

This example maps a knob (CC #7) to the number keys 1-0, evenly distributing them across the 0-127 range:

```json
{
  "controlNumber": 7,
  "handlerType": "CCRange",
  "description": "Opacity control using keys 1-0",
  "ranges": [
    { "minValue": 0, "maxValue": 12, "action": { "type": "KeyPress", "key": "1" } },
    { "minValue": 13, "maxValue": 25, "action": { "type": "KeyPress", "key": "2" } },
    { "minValue": 26, "maxValue": 38, "action": { "type": "KeyPress", "key": "3" } },
    { "minValue": 39, "maxValue": 51, "action": { "type": "KeyPress", "key": "4" } },
    { "minValue": 52, "maxValue": 64, "action": { "type": "KeyPress", "key": "5" } },
    { "minValue": 65, "maxValue": 77, "action": { "type": "KeyPress", "key": "6" } },
    { "minValue": 78, "maxValue": 90, "action": { "type": "KeyPress", "key": "7" } },
    { "minValue": 91, "maxValue": 103, "action": { "type": "KeyPress", "key": "8" } },
    { "minValue": 104, "maxValue": 116, "action": { "type": "KeyPress", "key": "9" } },
    { "minValue": 117, "maxValue": 127, "action": { "type": "KeyPress", "key": "0" } }
  ]
}
```

## Example: Complex Mapping with Different Action Types

This example shows how to use different action types in a single mapping:

```json
{
  "controlNumber": 10,
  "handlerType": "CCRange",
  "description": "Complex mapping example",
  "ranges": [
    {
      "minValue": 0,
      "maxValue": 30,
      "action": {
        "type": "KeyPress",
        "virtualKeyCode": 65,
        "modifiers": [16]
      }
    },
    {
      "minValue": 31,
      "maxValue": 60,
      "action": {
        "type": "CommandExecution",
        "command": "echo 'CC value in middle range'",
        "shellType": 0,
        "runHidden": false,
        "waitForExit": true
      }
    },
    {
      "minValue": 61,
      "maxValue": 90,
      "action": {
        "type": "Macro",
        "macroActions": [
          {
            "virtualKeyCode": 66,
            "modifiers": [],
            "actionType": 0,
            "delayAfter": 100
          },
          {
            "virtualKeyCode": 67,
            "modifiers": [],
            "actionType": 0,
            "delayAfter": 0
          }
        ]
      }
    },
    {
      "minValue": 91,
      "maxValue": 127,
      "action": {
        "type": "KeyPress",
        "key": "Z"
      }
    }
  ]
}
```

## Important Notes

1. Ranges should not overlap. If they do, the first matching range will be used.
2. You can leave gaps between ranges if you want certain CC values to do nothing.
3. The action is only triggered when moving from one range to another, not continuously.
4. A complete example configuration is available in the `config_examples/example-cc-range.json` file.

