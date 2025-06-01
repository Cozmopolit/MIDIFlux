# Game Controller Actions

MIDIFlux provides game controller emulation through ViGEm, allowing MIDI controllers to function as Xbox 360 controllers.

## Prerequisites

The ViGEm Bus Driver must be installed. See `Documentation/GameController/ViGEmStatus.md` for installation details.

## GameControllerButtonAction

Emulates Xbox 360 controller button presses.

**Configuration Type**: `GameControllerButtonAction`

**Parameters**:
- `Button` (string): The button to press
- `PressType` (string): How to press the button
- `ControllerIndex` (int): Controller index (0-3, default: 0)

**Supported Buttons**:
- Face Buttons: `A`, `B`, `X`, `Y`
- Shoulder Buttons: `LeftShoulder`, `RightShoulder`
- Triggers: `LeftTrigger`, `RightTrigger` (as buttons)
- D-Pad: `DPadUp`, `DPadDown`, `DPadLeft`, `DPadRight`
- Stick Buttons: `LeftThumb`, `RightThumb`
- System Buttons: `Start`, `Back`, `Guide`

**Press Types**:
- `PressRelease`: Press and immediately release (default)
- `Press`: Press and hold
- `Release`: Release button

**Example**:
```json
{
  "$type": "GameControllerButtonAction",
  "Parameters": {
    "Button": "A",
    "PressType": "PressRelease",
    "ControllerIndex": 0
  },
  "Description": "Press A button"
}
```

## GameControllerAxisAction

Controls Xbox 360 controller analog sticks and triggers.

**Configuration Type**: `GameControllerAxisAction`

**Parameters**:
- `Axis` (string): The axis to control
- `Value` (int): The axis value (0 = use MIDI input value)
- `Duration` (int): Time in milliseconds to hold the value (0 = permanent)
- `ControllerIndex` (int): Controller index (0-3, default: 0)

**Supported Axes**:
- Left Stick: `LeftStickX`, `LeftStickY`
- Right Stick: `RightStickX`, `RightStickY`
- Triggers: `LeftTrigger`, `RightTrigger`

**Value Ranges**:
- Sticks: -32768 to 32767 (center = 0)
- Triggers: 0 to 255 (0 = not pressed, 255 = fully pressed)

**Automatic Value Mapping** (when Value = 0):
- MIDI CC values (0-127) are automatically mapped to controller ranges
- Sticks: 0-127 → -32768 to 32767 (64 = center)
- Triggers: 0-127 → 0 to 255

**Example**:
```json
{
  "$type": "GameControllerAxisAction",
  "Parameters": {
    "Axis": "LeftStickX",
    "Value": 0,
    "Duration": 0,
    "ControllerIndex": 0
  },
  "Description": "Control left stick X-axis with MIDI input"
}
```

## Usage Examples

### Basic Button Mapping
```json
{
  "Id": "xbox-a-button",
  "Description": "MIDI note to Xbox A button",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "GameControllerButtonAction",
    "Parameters": {
      "Button": "A",
      "PressType": "PressRelease",
      "ControllerIndex": 0
    },
    "Description": "Press A button"
  }
}
```

### Analog Control
```json
{
  "Id": "left-stick-control",
  "Description": "CC to left stick",
  "InputType": "ControlChangeAbsolute",
  "Channel": 1,
  "ControlNumber": 1,
  "Action": {
    "$type": "GameControllerAxisAction",
    "Parameters": {
      "Axis": "LeftStickX",
      "Value": 0,
      "Duration": 0,
      "ControllerIndex": 0
    },
    "Description": "Control left stick with modulation wheel"
  }
}
```

### Fixed Value Control
```json
{
  "Id": "right-trigger-press",
  "Description": "Full right trigger press",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 42,
  "Action": {
    "$type": "GameControllerAxisAction",
    "Parameters": {
      "Axis": "RightTrigger",
      "Value": 255,
      "Duration": 200,
      "ControllerIndex": 0
    },
    "Description": "Full trigger press for 200ms"
  }
}
```

## Notes

- MIDIFlux supports up to 4 virtual Xbox 360 controllers (indices 0-3)
- Controllers appear in Windows Game Controllers panel
- Compatible with all Xbox 360 controller-supported games
- Game controller features are automatically disabled if ViGEm is not installed
