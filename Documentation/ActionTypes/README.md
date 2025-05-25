# Action Types and Mappings

This directory contains documentation about the different types of actions that MIDI events can be mapped to in MIDIFlux.

## Contents

- [Keyboard Mapping](KeyboardMapping.md) - How to map MIDI notes to keyboard keys
- [Toggle Key Mapping](ToggleKeyMapping.md) - How to use MIDI notes to toggle key states (like CapsLock)
- [CC Range Mapping](CCRangeMapping.md) - How to map different ranges of a CC value to different actions
- [Game Controller Integration](GameControllerIntegration.md) - How to use MIDIFlux with ViGEm for game controller emulation
- [Game Controller Implementation](GameControllerImplementation.md) - Technical details about the game controller implementation
- [Command Execution](CommandExecution.md) - How to execute shell commands (PowerShell or CMD)
- [Note-On Only Mode](NoteOnOnly.md) - How to use MIDIFlux with controllers that don't send Note-Off events
- [Macro Actions](MacroActions.md) - How to create complex sequences of actions
- [MIDI Output](MidiOutput.md) - How to send MIDI messages to external devices

## Overview

MIDIFlux can map MIDI events to various types of actions:

### Keyboard Actions

- **Basic Key Presses**: Map MIDI notes to simple keyboard keys
- **Modifier Keys**: Map MIDI notes to modifier keys like Shift, Ctrl, and Alt
- **Key Combinations**: Map MIDI notes to key combinations like Ctrl+C
- **Toggle Keys**: Map MIDI notes to toggle key states like CapsLock

### Game Controller Actions

- **Button Presses**: Map MIDI notes to game controller buttons
- **Axis Controls**: Map MIDI controllers to game controller axes
- **Multiple Controllers**: Map MIDI inputs to up to 4 virtual game controllers

### Other Actions

- **Command Execution**: Execute shell commands (PowerShell or CMD)
- **Note-On Only Mode**: Create momentary key presses with a fixed duration
- **Macro Actions**: Create complex sequences of actions
- **CC Range Mapping**: Map different ranges of a CC value to different actions
- **MIDI Output**: Send MIDI messages to external devices for routing and feedback

