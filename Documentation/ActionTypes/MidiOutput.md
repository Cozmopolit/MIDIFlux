# MIDI Output Configuration Documentation

## Overview

MIDIFlux supports sending MIDI messages to external devices using the `MidiOutputAction` action type. This allows you to create complex workflows where MIDI input triggers MIDI output to other devices, creating powerful routing and feedback systems.

## Basic Configuration Format

```json
{
  "$type": "MidiOutputAction",
  "Parameters": {
    "OutputDeviceName": "Device Name",
    "Commands": [
      {
        "MessageType": "NoteOn",
        "Channel": 1,
        "Data1": 60,
        "Data2": 127
      }
    ]
  },
  "Description": "Optional description"
}
```

## Configuration Properties

### OutputDeviceName (Required)
- **Type**: String
- **Description**: The exact name of the MIDI output device
- **Important**: Must be a specific device name - wildcards are NOT supported
- **Example**: `"Launchpad Pro"`, `"Roland JV-1000"`

### Commands (Required)
- **Type**: Array of MidiOutputCommand objects
- **Description**: List of MIDI messages to send in sequence
- **Note**: All commands are sent immediately without delays

### Description (Optional)
- **Type**: String
- **Description**: Human-readable description of the action

## MIDI Message Types

### NoteOn
Sends a MIDI Note On message (key press).
```json
{
  "MessageType": "NoteOn",
  "Channel": 1,
  "Data1": 60,    // Note number (0-127)
  "Data2": 127    // Velocity (0-127)
}
```

### NoteOff
Sends a MIDI Note Off message (key release).
```json
{
  "MessageType": "NoteOff",
  "Channel": 1,
  "Data1": 60,    // Note number (0-127)
  "Data2": 0      // Release velocity (usually 0)
}
```

### ControlChange
Sends a MIDI Control Change message (knobs, faders, buttons).
```json
{
  "MessageType": "ControlChange",
  "Channel": 1,
  "Data1": 7,     // Controller number (0-127)
  "Data2": 100    // Controller value (0-127)
}
```

### ProgramChange
Sends a MIDI Program Change message (preset selection).
```json
{
  "MessageType": "ProgramChange",
  "Channel": 1,
  "Data1": 42,    // Program number (0-127)
  "Data2": 0      // Unused for Program Change
}
```

### PitchBend
Sends a MIDI Pitch Bend message.
```json
{
  "MessageType": "PitchBend",
  "Channel": 1,
  "Data1": 0,     // LSB (0-127)
  "Data2": 64     // MSB (0-127) - 64 is center
}
```

### Aftertouch
Sends a MIDI Aftertouch message (key pressure).
```json
{
  "MessageType": "Aftertouch",
  "Channel": 1,
  "Data1": 60,    // Note number (0-127)
  "Data2": 80     // Pressure value (0-127)
}
```

### ChannelPressure
Sends a MIDI Channel Pressure message.
```json
{
  "MessageType": "ChannelPressure",
  "Channel": 1,
  "Data1": 90,    // Pressure value (0-127)
  "Data2": 0      // Unused for Channel Pressure
}
```

### SysEx
Sends a MIDI System Exclusive message.
```json
{
  "MessageType": "SysEx",
  "Channel": 1,
  "Data1": 0,
  "Data2": 0,
  "SysExData": [240, 0, 32, 41, 2, 16, 14, 0, 247]
}
```

## Timing Control

MIDI Output actions send all commands immediately. For timing control, use `SequenceAction` with `DelayAction`:

```json
{
  "$type": "SequenceAction",
  "Parameters": {
    "SubActions": [
      {
        "$type": "MidiOutputAction",
        "Parameters": {
          "OutputDeviceName": "Launchpad Pro",
          "Commands": [{"MessageType": "NoteOn", "Channel": 1, "Data1": 60, "Data2": 127}]
        }
      },
      {
        "$type": "DelayAction",
        "Parameters": {
          "Milliseconds": 500
        }
      },
      {
        "$type": "MidiOutputAction",
        "Parameters": {
          "OutputDeviceName": "Launchpad Pro",
          "Commands": [{"MessageType": "NoteOff", "Channel": 1, "Data1": 60, "Data2": 0}]
        }
      }
    ]
  }
}
```

## Multiple Device Support

You can send to different devices in the same sequence:

```json
{
  "$type": "SequenceAction",
  "Parameters": {
    "SubActions": [
      {
        "$type": "MidiOutputAction",
        "Parameters": {
          "OutputDeviceName": "Launchpad Pro",
          "Commands": [{"MessageType": "NoteOn", "Channel": 1, "Data1": 60, "Data2": 127}]
        }
      },
      {
        "$type": "MidiOutputAction",
        "Parameters": {
          "OutputDeviceName": "Roland JV-1000",
          "Commands": [{"MessageType": "NoteOn", "Channel": 1, "Data1": 60, "Data2": 100}]
        }
      }
    ]
  }
}
```

## Error Handling

- If the specified output device is not available, the action will fail with an error
- Device names must match exactly (case-sensitive)
- Invalid MIDI data will be rejected during validation
- Use `ErrorHandling` in `SequenceAction` to control behavior on errors

## Example Files

- `%AppData%\MIDIFlux\profiles\examples\midi-output-basic.json` - Simple MIDI output examples
- `%AppData%\MIDIFlux\profiles\examples\advanced-macros.json` - Complex sequences with timing
- `%AppData%\MIDIFlux\profiles\examples\complete-profile-sample.json` - Integration with other action types

## Best Practices

1. **Device Names**: Use exact device names, check available devices in MIDIFlux
2. **Channel Numbers**: Use 1-16 (not 0-15)
3. **Data Validation**: Ensure Data1 and Data2 are in valid ranges (0-127)
4. **Timing**: Use SequenceAction + DelayAction for precise timing
5. **Error Handling**: Set appropriate error handling for sequences
6. **Testing**: Test configurations with actual hardware before deployment
