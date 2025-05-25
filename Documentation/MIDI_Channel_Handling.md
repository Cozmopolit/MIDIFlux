# MIDI Channel Handling in MIDIFlux

## Overview

MIDIFlux implements a consistent **1-based MIDI channel convention (1-16)** throughout the entire application. This document explains how MIDI channels are handled across different layers of the system.

## Channel Convention

### User-Facing Convention
- **All user interfaces** display channels as 1-16
- **All configuration files** use channels 1-16
- **All documentation** refers to channels 1-16
- **All error messages** report channels 1-16

### Internal Processing
- **All internal logic** uses 1-based channels (1-16)
- **Event processing** maintains 1-based channels
- **Action mapping** uses 1-based channels
- **State management** uses 1-based channels

## Hardware Abstraction Layer

The `IMidiHardwareAdapter` interface provides a clean abstraction over the NAudio library, handling all necessary channel conversions internally.

### NAudioMidiAdapter Implementation

The `NAudioMidiAdapter` class handles the complexity of NAudio's channel expectations:

```csharp
/// Channel Conversion Strategy:
/// - Input events: NAudio 0-based (0-15) → MIDIFlux 1-based (1-16)
/// - Output events: MIDIFlux 1-based (1-16) → NAudio 1-based (1-16) [no conversion]
/// - Raw messages: MIDIFlux 1-based (1-16) → NAudio 0-based (0-15)
```

### Key Benefits

1. **Consistency**: All application layers use the same channel numbering
2. **Isolation**: NAudio quirks are contained within the adapter
3. **Maintainability**: Channel conversion logic is centralized
4. **Testability**: Mock adapter can simulate any channel behavior

## Configuration Examples

### Basic Channel Configuration
```json
{
  "device": "My MIDI Controller",
  "mappings": [
    {
      "input": {
        "type": "NoteOn",
        "channel": 1,
        "note": 60
      },
      "action": {
        "type": "KeyPressRelease",
        "virtualKeyCode": 65
      }
    }
  ]
}
```

### Multi-Channel Configuration
```json
{
  "device": "Multi-Channel Controller",
  "mappings": [
    {
      "input": {
        "type": "ControlChange",
        "channel": 1,
        "controller": 7
      },
      "action": {
        "type": "MidiOutput",
        "outputDeviceName": "Synthesizer",
        "commands": [
          {
            "messageType": "ControlChange",
            "channel": 2,
            "data1": 7,
            "data2": "{{value}}"
          }
        ]
      }
    }
  ]
}
```

## Troubleshooting

### Common Channel Issues

1. **No response from MIDI device**
   - Check that the device is sending on the expected channel (1-16)
   - Verify the channel number in your configuration matches the device

2. **Wrong channel in output**
   - Ensure MIDI output actions specify the correct target channel (1-16)
   - Check that the receiving device expects the channel you're sending to

3. **Channel confusion with other software**
   - Remember that some MIDI software uses 0-based channels (0-15)
   - MIDIFlux always uses 1-based channels (1-16) in all configurations

### Debugging Channel Issues

1. **Enable debug logging** to see channel information in MIDI events
2. **Use MIDI monitor tools** to verify what channels your device is actually sending
3. **Check configuration files** for correct channel numbers (1-16)
4. **Verify device settings** to ensure it's transmitting on expected channels

## Migration from Other MIDI Software

If migrating from software that uses 0-based channels:
- Add 1 to all channel numbers in your configurations
- Channel 0 becomes Channel 1
- Channel 15 becomes Channel 16

## Implementation Details

### Event Processing
All MIDI events maintain 1-based channels throughout the processing pipeline:
1. Hardware adapter converts incoming events to 1-based channels
2. Event dispatcher processes events with 1-based channels
3. Action system receives events with 1-based channels
4. Output actions send with 1-based channels (converted by adapter if needed)

### Error Handling
All error messages and logging use 1-based channel numbers for consistency with user expectations.

### Testing
The mock MIDI adapter in the test infrastructure also uses 1-based channels, ensuring consistency across development and testing environments.
