# Action Types and Mappings

This directory contains documentation about the unified action system in MIDIFlux. All actions implement the `IAction` interface and use strongly-typed configuration classes with `$type` discriminators.

## Action System Architecture

MIDIFlux uses a two-tier action system:

### Simple Actions (Hot Path)
Optimized for performance with direct execution:
- **KeyPressRelease**: Press and release a key
- **KeyDown**: Press and hold a key
- **KeyUp**: Release a key
- **KeyToggle**: Toggle key state (like CapsLock)
- **MouseClick**: Click mouse buttons (Left/Right/Middle)
- **MouseScroll**: Scroll wheel (Up/Down/Left/Right)
- **CommandExecution**: Execute shell commands
- **Delay**: Wait for specified time
- **GameControllerButton**: Press game controller button
- **GameControllerAxis**: Set game controller axis value
- **MidiOutput**: Send MIDI messages to external devices

### Complex Actions (Orchestration)
Handle logic and sequencing:
- **SequenceAction**: Execute actions sequentially (macros)
- **ConditionalAction**: Execute actions based on MIDI value (CC ranges)
- **AlternatingAction**: Alternate between two actions on repeated triggers
- **StateConditionalAction**: Execute actions based on state values
- **SetStateAction**: Set state values for stateful behaviors

## Documentation Contents

### Simple Actions
- [Keyboard Actions](KeyboardMapping.md) - Key press, key down/up, and key toggle actions
- [Mouse Actions](MouseActions.md) - Mouse clicks and scroll wheel actions
- [Game Controller Actions](GameControllerIntegration.md) - Xbox controller emulation via ViGEm
- [System Actions](CommandExecution.md) - Execute shell commands and delays
- [MIDI Output Actions](MidiOutput.md) - Send MIDI messages to external devices

### Complex Actions
- [Sequence Actions](MacroActions.md) - Create complex sequences of actions (macros)
- [Conditional Actions](CCRangeMapping.md) - Execute actions based on MIDI value ranges
- [Alternating Actions](ToggleKeyMapping.md) - Toggle between two actions
- [Stateful Actions](StatefulActions.md) - State-based conditional actions and state management

### Legacy Documentation
- [Note-On Only Mode](NoteOnOnly.md) - Legacy documentation (may be obsolete)
- [Game Controller Implementation](GameControllerImplementation.md) - Technical implementation details

## Configuration Format

All actions use the unified configuration format with `$type` discriminators:

```json
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
```

## MIDI Input Types

MIDIFlux supports all standard MIDI message types:

- **NoteOn**: Piano keys, drum pads, buttons (key press)
- **NoteOff**: Piano keys, drum pads, buttons (key release)
- **ControlChange**: Knobs, faders, sliders
- **ProgramChange**: Preset selection
- **PitchBend**: Pitch wheels
- **Aftertouch**: Pressure-sensitive keys
- **SysEx**: System exclusive messages

## State Management

MIDIFlux includes a unified state management system:

- **User-defined states**: Alphanumeric keys defined in profile configuration
- **Internal states**: Auto-generated with asterisk prefix (e.g., `*Key65` for keyboard state)
- **Profile-scoped**: States are initialized per profile and cleared on profile changes
- **Thread-safe**: Concurrent access supported for real-time MIDI processing

## Performance Considerations

- **O(1) Lookup**: Pre-computed mapping keys for fast MIDI event processing
- **Sync-by-default**: Most actions execute synchronously for minimal overhead
- **Pre-compiled Actions**: Actions are created and validated at profile load time
- **Hot Path Optimization**: Simple actions bypass complex orchestration logic

