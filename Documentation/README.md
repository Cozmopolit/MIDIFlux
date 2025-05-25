# MIDIFlux Documentation

Welcome to the MIDIFlux documentation. This document provides links to all the documentation files for the MIDIFlux project.

## Getting Started

- [Usage Guide](UsageGuide.md) - How to use MIDIFlux

## MIDI Devices and Controllers

Documentation about MIDI devices, controllers, and how to map from them:

- [Controller Mappings](MIDIDevices/ControllerMappings.md) - How to map MIDI controllers to various functions
- [Relative Controls](MIDIDevices/RelativeControls.md) - How to use jog wheels and other relative controls
- [Multiple Controller Support](MIDIDevices/MultiControllerSupport.md) - How to use multiple MIDI devices simultaneously
- [MIDI Channel Handling](MIDI_Channel_Handling.md) - How MIDIFlux handles MIDI channels and troubleshooting guide

## Action Types and Mappings

Documentation about what you can map MIDI events to:

- [Keyboard Mapping](ActionTypes/KeyboardMapping.md) - How to map MIDI notes to keyboard keys
- [Toggle Key Mapping](ActionTypes/ToggleKeyMapping.md) - How to use MIDI notes to toggle key states (like CapsLock)
- [Game Controller Integration](ActionTypes/GameControllerIntegration.md) - How to use MIDIFlux with ViGEm for game controller emulation
- [Command Execution](ActionTypes/CommandExecution.md) - How to execute shell commands (PowerShell or CMD)
- [Note-On Only Mode](ActionTypes/NoteOnOnly.md) - How to use MIDIFlux with controllers that don't send Note-Off events
- [Macro Actions](ActionTypes/MacroActions.md) - How to create complex sequences of actions

## Game Controller Support

- [ViGEm Status](GameController/ViGEmStatus.md) - Information about ViGEm compatibility and installation

## Development

For technical documentation and architecture details, see the [Developer Documentation](Developer/README.md).

## Supported Controllers

For information about supported MIDI controllers, see the [MIDI Devices and Controllers](MIDIDevices/README.md) documentation.

## Configuration Files

MIDIFlux uses JSON configuration files to define mappings. Example configuration files are provided in the `config_examples` directory:

- `example-basic-keys.json`: Basic keyboard shortcuts (Copy, Paste, Cut, etc.)
- `example-macros.json`: Advanced key sequences and macros
- `example-advanced-macros.json`: Complex macro sequences
- `example-system-controls.json`: System volume control and mouse scroll wheel
- `example-game-controller.json`: Game controller emulation (requires ViGEm)
- `example-cc-range.json`: CC range mapping examples
- `example-command-execution.json`: Command execution examples
- `example-multi-channel.json`: Multi-channel MIDI device examples
- `example-multiple-midi-channels.json`: Multiple MIDI channel configurations
- `example-note-on-only.json`: Note-on only mode examples

For detailed information about configuration:
- See [MIDI Devices and Controllers](MIDIDevices/README.md) for how to map from MIDI devices
- See [Action Types and Mappings](ActionTypes/README.md) for what you can map MIDI events to

