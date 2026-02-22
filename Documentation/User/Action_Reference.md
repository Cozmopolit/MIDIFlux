# Action Reference

Complete reference for all MIDIFlux action types. All actions use `$type` discriminators and strongly-typed parameters.

## Action System Overview

MIDIFlux uses a two-tier action system:
- **Simple Actions**: Direct execution for performance (keyboard, mouse, commands)
- **Complex Actions**: Orchestration and logic (sequences, conditionals, state management)

## Simple Actions

### Keyboard Actions

#### KeyPressReleaseAction
Press and release a key (most common use case).
- **Parameters**: `VirtualKeyCode` (string)
- **Example**: Press "A" key, Ctrl+C combinations

#### KeyDownAction  
Press and hold a key down.
- **Parameters**: `VirtualKeyCode` (string), `AutoReleaseAfterMs` (optional)
- **Example**: Hold modifier keys like Ctrl, Shift

#### KeyUpAction
Release a previously pressed key.
- **Parameters**: `VirtualKeyCode` (string)
- **Example**: Release modifier keys in sequences

#### KeyToggleAction
Toggle state of toggle keys.
- **Parameters**: `VirtualKeyCode` (string)
- **Example**: CapsLock, NumLock, ScrollLock

### Mouse Actions

#### MouseClickAction
Simulate mouse button clicks.
- **Parameters**: `Button` ("Left", "Right", "Middle")
- **Example**: Left click, right-click context menus

#### MouseScrollAction
Simulate mouse scroll wheel movement.
- **Parameters**: `Direction` ("Up", "Down", "Left", "Right"), `Amount` (integer)
- **Example**: Scroll up 3 units, horizontal scrolling

### System Actions

#### CommandExecutionAction
Execute shell commands.
- **Parameters**: `Command` (string), `ShellType` ("PowerShell", "CommandPrompt"), `RunHidden` (bool), `WaitForExit` (bool)
- **Example**: Launch applications, run scripts, system commands

#### SystemVolumeAction
Set Windows system master volume from MIDI value.
- **Parameters**: None — uses the incoming MIDI value (0-127) directly as volume level (0%-100%)
- **Input**: `ControlChangeAbsolute` (faders, knobs)
- **Example**: Map a fader to control system volume in real-time

#### DelayAction
Wait for specified time.
- **Parameters**: `Milliseconds` (integer)
- **Example**: Add timing between actions in sequences

### Audio Actions

#### PlaySoundAction
Play audio files with low-latency playback.
- **Parameters**: `FilePath` (string), `Volume` (float 0.0-1.0)
- **Example**: Sound effects, audio samples
- **Supported**: WAV, MP3 files

### Game Controller Actions

#### GameControllerButtonAction
Press and release Xbox controller buttons immediately (requires ViGEm).
- **Parameters**: `Button` (string), `ControllerIndex` (integer 0-3)
- **Example**: A, B, X, Y, Start, Back buttons

#### GameControllerButtonDownAction
Press and hold Xbox controller buttons down (requires ViGEm).
- **Parameters**: `Button` (string), `ControllerIndex` (integer 0-3)
- **Example**: A, B, X, Y, Start, Back buttons
- **Note**: Use with GameControllerButtonUpAction for sustained button presses

#### GameControllerButtonUpAction
Release Xbox controller buttons (requires ViGEm).
- **Parameters**: `Button` (string), `ControllerIndex` (integer 0-3)
- **Example**: A, B, X, Y, Start, Back buttons
- **Note**: Use with GameControllerButtonDownAction for sustained button presses

#### GameControllerAxisAction
Set Xbox controller axis values (requires ViGEm).
- **Parameters**: `Axis` (string), `AxisValue` (float -1.0 to 1.0), `ControllerIndex` (integer 0-3)
- **Example**: LeftThumbX, LeftThumbY, RightThumbX, RightThumbY, LeftTrigger, RightTrigger

### MIDI Output Actions

#### MidiOutputAction
Send MIDI messages to external devices.
- **Parameters**: `OutputDeviceName` (string), `Commands` (array of MIDI commands)
- **Example**: Send NoteOn/NoteOff, ControlChange to external devices

## Complex Actions

### SequenceAction
Execute multiple actions sequentially (macros).
- **Parameters**: `SubActions` (array of actions)
- **Example**: Ctrl+C copy sequence, complex workflows

### ConditionalAction
Execute actions based on MIDI value ranges.
- **Parameters**: `Conditions` (array with MinValue, MaxValue, Action)
- **Example**: Different actions for different CC value ranges

### AlternatingAction
Alternate between two actions on repeated triggers.
- **Parameters**: `FirstAction`, `SecondAction`
- **Example**: Toggle between two different behaviors

### State Management Actions

#### StateConditionalAction
Execute actions based on state values.
- **Parameters**: `Conditions`, `LogicType`, `ActionIfTrue`, `ActionIfFalse`
- **Example**: Mode switching, complex conditional logic

#### SetStateAction
Set state values for stateful behaviors.
- **Parameters**: `StateKey` (string), `StateValue` (integer)
- **Example**: Set mode variables, counters

### Relative Control Actions

#### RelativeCCAction
Handle relative control changes (jog wheels, encoders).
- **Parameters**: `PositiveAction`, `NegativeAction`, acceleration settings
- **Example**: Scroll wheel control, timeline scrubbing

## Configuration Format

Basic action structure:
```
"Action": {
  "$type": "ActionTypeName",
  "Parameters": {
    "ParameterName": "value"
  },
  "Description": "Human-readable description"
}
```

## MIDI Input Types

- **NoteOn/NoteOff**: Piano keys, drum pads, buttons
- **ControlChange**: Knobs, faders, sliders  
- **ProgramChange**: Preset selection
- **PitchBend**: Pitch wheels
- **Aftertouch**: Pressure-sensitive keys
- **SysEx**: System exclusive messages

## Virtual Key Codes (Essential List)

### Letters and Numbers
- **Letters**: "A", "B", "C", ... "Z"
- **Numbers**: "D0", "D1", "D2", ... "D9"
- **Numpad**: "NumPad0", "NumPad1", ... "NumPad9"

### Function Keys
- **Function**: "F1", "F2", "F3", ... "F12"

### Navigation
- **Arrows**: "Left", "Up", "Right", "Down"
- **Navigation**: "Home", "End", "PageUp", "PageDown"
- **Edit**: "Insert", "Delete"

### Modifiers
- **Shift**: "ShiftKey", "LShiftKey", "RShiftKey"
- **Control**: "ControlKey", "LControlKey", "RControlKey"  
- **Alt**: "Menu", "LMenu", "RMenu"

### Special Keys
- **Common**: "Space", "Return", "Tab", "Back", "Escape"
- **Toggle**: "CapsLock", "NumLock", "Scroll"

For complete list, see [Windows Virtual Key Codes](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes).

## State Management

- **User States**: Alphanumeric keys defined in profile `InitialStates`
- **Internal States**: Auto-generated with asterisk prefix (e.g., `*Key65`)
- **Scope**: Profile-scoped, cleared on profile changes
- **Thread Safety**: Concurrent access supported

## Example Files Reference

See working examples in `%AppData%\MIDIFlux\profiles\examples\`:

### Comprehensive Examples
- **all-action-types-demo.json**: Showcase of every action type
- **basic-keyboard-shortcuts.json**: Keyboard action patterns
- **advanced-macros.json**: Complex sequence examples

### Specialized Examples  
- **game-controller-demo.json**: Game controller actions (requires ViGEm)
- **conditional-action-demo.json**: Conditional logic patterns
- **command-execution-examples.json**: Shell command patterns
- **midi-output-basic.json**: MIDI output examples
- **relative-cc-demo.json**: Relative control handling

### Use Case Examples
- **system-controls.json**: Media control patterns
- **multi-channel-demo.json**: Multi-channel configurations
- **alternating-action-demo.json**: Alternating action patterns

## Performance Notes

- **Hot Path**: Simple actions execute synchronously for minimal latency
- **State Tracking**: Key and controller states tracked automatically
- **Pre-compilation**: Actions created and validated at profile load time
- **O(1) Lookup**: Pre-computed mapping keys for fast MIDI event processing

## Related Documentation

- **[Getting Started](Getting_Started.md)**: Basic usage and configuration
- **[Developer Guide](Developer_Guide.md)**: Technical implementation details
- **[Troubleshooting](Troubleshooting.md)**: Common issues and solutions
