# Macro Actions in MIDIFlux

MIDIFlux supports advanced macro capabilities, allowing you to create complex sequences of actions that can be triggered by a single MIDI note.

## Overview

Macros in MIDIFlux are sequences of actions that are executed in order. Each action can be one of the following types:

- **KeyPressRelease**: Press and release a key
- **KeyDown**: Press a key down but don't release it
- **KeyUp**: Release a key that was previously pressed
- **KeyToggle**: Toggle a key state (like CapsLock)
- **CommandExecution**: Execute a command (PowerShell or Command Prompt)
- **Delay**: Wait for a specified time

Note: Nested macros are not supported to prevent recursion issues.

## Configuration

Macros are configured using the `macroMappings` section in the JSON configuration file. Here's an example:

```json
{
  "midiDevices": [
    {
      "inputProfile": "MacroExample",
      "deviceName": "MIDI Controller",
      "midiChannels": null,
      "mappings": [],
      "absoluteControlMappings": [],
      "relativeControlMappings": [],
      "macroMappings": [
        {
          "midiNote": 61,
          "description": "Complex macro example",
          "ignoreNoteOff": true,
          "actions": [
            {
              "type": "KeyDown",
              "virtualKeyCode": 162,
              "description": "Press Ctrl key"
            },
            {
              "type": "KeyPressRelease",
              "virtualKeyCode": 65,
              "description": "Press and release A key (with Ctrl held)"
            },
            {
              "type": "KeyUp",
              "virtualKeyCode": 162,
              "description": "Release Ctrl key"
            },
            {
              "type": "Delay",
              "milliseconds": 1000,
              "description": "Wait for 1 second"
            },
            {
              "type": "CommandExecution",
              "command": "Get-Date",
              "shellType": "PowerShell",
              "description": "Run PowerShell command"
            }
          ]
        }
      ]
    }
  ]
}
```

## Action Types

### KeyPressRelease

Presses and releases a key.

```json
{
  "type": "KeyPressRelease",
  "virtualKeyCode": 65,
  "modifiers": [162],
  "description": "Press and release A key with Ctrl"
}
```

### KeyDown

Presses a key down but doesn't release it.

```json
{
  "type": "KeyDown",
  "virtualKeyCode": 162,
  "description": "Press Ctrl key"
}
```

### KeyUp

Releases a key that was previously pressed.

```json
{
  "type": "KeyUp",
  "virtualKeyCode": 162,
  "description": "Release Ctrl key"
}
```

### KeyToggle

Toggles a key state (like CapsLock).

```json
{
  "type": "KeyToggle",
  "virtualKeyCode": 20,
  "description": "Toggle CapsLock"
}
```

### CommandExecution

Executes a command (PowerShell or Command Prompt).

```json
{
  "type": "CommandExecution",
  "command": "Get-Date",
  "shellType": "PowerShell",
  "runHidden": false,
  "waitForExit": true,
  "description": "Run PowerShell command"
}
```

### Delay

Waits for a specified time.

```json
{
  "type": "Delay",
  "milliseconds": 1000,
  "description": "Wait for 1 second"
}
```

## Examples

See the `config_examples/example-advanced-macros.json` file for examples of how to use macros in MIDIFlux.



