# MIDIFlux Documentation

Welcome to the MIDIFlux documentation. MIDIFlux is a powerful MIDI-to-action mapping system that converts MIDI events into keyboard actions, mouse actions, game controller inputs, system commands, and MIDI output.

## Getting Started

- [Usage Guide](UsageGuide.md) - How to use MIDIFlux
- [MIDI Channel Handling](MIDIDevices/MIDI_Channel_Handling.md) - How MIDIFlux handles MIDI channels and troubleshooting

## Action System

MIDIFlux uses an action system where MIDI events trigger actions. All actions implement the `IAction` interface and use strongly-typed configuration classes.

### Action Types

Documentation for all supported action types:

- [Action Types Overview](ActionTypes/README.md) - Complete overview of the action system
- [Keyboard Actions](ActionTypes/KeyboardMapping.md) - Key press, key down/up, and key toggle actions
- [Mouse Actions](ActionTypes/MouseActions.md) - Mouse clicks and scroll wheel actions
- [Game Controller Actions](ActionTypes/GameControllerActions.md) - Xbox controller emulation via ViGEm
- [System Actions](ActionTypes/CommandExecution.md) - Execute shell commands and delays
- [MIDI Output Actions](ActionTypes/MidiOutput.md) - Send MIDI messages to external devices
- [Complex Actions](ActionTypes/MacroActions.md) - Sequences, conditionals, and alternating actions
- [Stateful Actions](ActionTypes/StatefulActions.md) - State-based conditional actions and toggles

## MIDI Input Support

MIDIFlux supports all standard MIDI message types:

- **Note On/Off**: Piano keys, drum pads, buttons
- **Control Change (CC)**: Knobs, faders, sliders
- **Program Change**: Preset selection
- **Pitch Bend**: Pitch wheels
- **Aftertouch**: Pressure-sensitive keys
- **SysEx**: System exclusive messages

## Configuration

MIDIFlux uses JSON configuration files with strongly-typed action configurations:

### Example Configuration Files

Located in `%AppData%\MIDIFlux\profiles\examples\`:

- `basic-keyboard-shortcuts.json`: Basic keyboard shortcuts (Copy, Paste, Cut, etc.)
- `game-controller-demo.json`: Game controller emulation (requires ViGEm)
- `command-execution-examples.json`: Shell command execution examples
- `midi-output-basic.json`: Basic MIDI output examples
- `advanced-macros.json`: Complex action sequences and macros
- `system-controls.json`: Media controls (play/pause, track navigation)

### Configuration Format

All configurations use the same format with `$type` discriminators:

```json
{
  "ProfileName": "My Profile",
  "Description": "Profile description",
  "InitialStates": {
    "UserState1": 0,
    "UserState2": 1
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Description": "Any MIDI device",
      "Mappings": [
        {
          "Id": "unique-mapping-id",
          "Description": "Human-readable description",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 65,
            "Description": "Press A key"
          }
        }
      ]
    }
  ]
}
```

## MIDI Device Support

- [MIDI Devices Overview](MIDIDevices/README.md) - Supported controllers and device-specific information
- [Controller Mappings](MIDIDevices/ControllerMappings.md) - Device-specific mapping examples
- [MIDI Channel Handling](MIDIDevices/MIDI_Channel_Handling.md) - How MIDI channels are handled throughout MIDIFlux

## Game Controller Support

- [ViGEm Status](GameController/ViGEmStatus.md) - ViGEm Bus Driver installation and compatibility

## Development

For technical documentation and architecture details:

- [Developer Documentation](Developer/README.md) - Architecture overview and development guide
- [Stateful Action System](Developer/StatefulActionSystem.md) - State management implementation
- [NAudio Abstraction Layer](Developer/NAudio_Abstraction_Layer.md) - MIDI hardware abstraction

