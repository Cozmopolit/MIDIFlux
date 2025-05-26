# Game Controller Actions

MIDIFlux supports Xbox controller emulation through the ViGEm framework, allowing you to use MIDI controllers with games that support Xbox controllers.

## Game Controller Actions

MIDIFlux provides two game controller action types:

### GameControllerButtonAction

Simulates Xbox controller button presses.

**Configuration Type**: `GameControllerButtonConfig`

**Supported Buttons**:
- **Face Buttons**: A, B, X, Y
- **Shoulder Buttons**: LeftShoulder, RightShoulder
- **System Buttons**: Back, Start, Guide
- **Thumbstick Buttons**: LeftThumb, RightThumb
- **D-Pad**: DPadUp, DPadDown, DPadLeft, DPadRight

### GameControllerAxisAction

Controls Xbox controller analog axes.

**Configuration Type**: `GameControllerAxisConfig`

**Supported Axes**:
- **Left Thumbstick**: LeftThumbX, LeftThumbY
- **Right Thumbstick**: RightThumbX, RightThumbY
- **Triggers**: LeftTrigger, RightTrigger

## Prerequisites

**ViGEm Bus Driver Installation**:
1. Download from [ViGEm GitHub releases](https://github.com/ViGEm/ViGEmBus/releases)
2. Run the installer as administrator
3. Restart your computer
4. MIDIFlux will automatically detect ViGEm availability

**Optional Feature**: Game controller emulation only works when ViGEm is installed. Core MIDIFlux functionality works without it.

## Configuration Format

### Button Configuration

```json
{
  "$type": "GameControllerButtonConfig",
  "Button": "A",
  "ControllerIndex": 0,
  "Description": "Press A button on controller 1"
}
```

### Axis Configuration

```json
{
  "$type": "GameControllerAxisConfig",
  "Axis": "LeftThumbX",
  "ControllerIndex": 0,
  "Description": "Control left thumbstick X-axis on controller 1"
}
```

## Configuration Properties

### GameControllerButtonConfig
| Property | Type | Description |
|----------|------|-------------|
| `Button` | string | Button name (A, B, X, Y, etc.) |
| `ControllerIndex` | int | Controller index (0-3) |
| `Description` | string | Optional description |

### GameControllerAxisConfig
| Property | Type | Description |
|----------|------|-------------|
| `Axis` | string | Axis name (LeftThumbX, RightThumbY, etc.) |
| `ControllerIndex` | int | Controller index (0-3) |
| `Description` | string | Optional description |

## Complete Mapping Examples

### Basic Button Mappings

```json
{
  "ProfileName": "Game Controller Example",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "button-a",
          "Description": "Xbox A button",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "A",
            "ControllerIndex": 0,
            "Description": "Press A button"
          }
        },
        {
          "Id": "button-b",
          "Description": "Xbox B button",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 37,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "B",
            "ControllerIndex": 0,
            "Description": "Press B button"
          }
        }
      ]
    }
  ]
}
```

### Axis Control Mappings

```json
{
  "Id": "left-stick-x",
  "Description": "Left thumbstick X-axis",
  "InputType": "ControlChange",
  "Channel": 1,
  "ControlNumber": 1,
  "Action": {
    "$type": "GameControllerAxisConfig",
    "Axis": "LeftThumbX",
    "ControllerIndex": 0,
    "Description": "Control left thumbstick horizontal movement"
  }
}
```

### Complete Game Setup

```json
{
  "ProfileName": "Racing Game Setup",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "accelerate",
          "Description": "Accelerate (Right Trigger)",
          "InputType": "ControlChange",
          "Channel": 1,
          "ControlNumber": 7,
          "Action": {
            "$type": "GameControllerAxisConfig",
            "Axis": "RightTrigger",
            "ControllerIndex": 0,
            "Description": "Accelerate"
          }
        },
        {
          "Id": "brake",
          "Description": "Brake (Left Trigger)",
          "InputType": "ControlChange",
          "Channel": 1,
          "ControlNumber": 8,
          "Action": {
            "$type": "GameControllerAxisConfig",
            "Axis": "LeftTrigger",
            "ControllerIndex": 0,
            "Description": "Brake"
          }
        },
        {
          "Id": "steering",
          "Description": "Steering (Left Stick X)",
          "InputType": "ControlChange",
          "Channel": 1,
          "ControlNumber": 1,
          "Action": {
            "$type": "GameControllerAxisConfig",
            "Axis": "LeftThumbX",
            "ControllerIndex": 0,
            "Description": "Steering wheel"
          }
        },
        {
          "Id": "handbrake",
          "Description": "Handbrake (A button)",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "A",
            "ControllerIndex": 0,
            "Description": "Handbrake"
          }
        }
      ]
    }
  ]
}
```

## Multiple Controllers

MIDIFlux supports up to 4 virtual Xbox controllers simultaneously:

```json
{
  "Id": "player-1-jump",
  "Description": "Player 1 jump",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "GameControllerButtonConfig",
    "Button": "A",
    "ControllerIndex": 0,
    "Description": "Player 1 jump"
  }
},
{
  "Id": "player-2-jump",
  "Description": "Player 2 jump",
  "InputType": "NoteOn",
  "Channel": 2,
  "Note": 36,
  "Action": {
    "$type": "GameControllerButtonConfig",
    "Button": "A",
    "ControllerIndex": 1,
    "Description": "Player 2 jump"
  }
}
```

## Use Cases

### Racing Games
- **Steering**: Map MIDI knobs/faders to left thumbstick X-axis
- **Acceleration/Braking**: Map MIDI faders to trigger axes
- **Gear Shifting**: Map MIDI buttons to shoulder buttons
- **Handbrake**: Map MIDI pedal to face button

### Fighting Games
- **Movement**: Map MIDI controllers to D-pad or thumbsticks
- **Attacks**: Map MIDI pads to face buttons (A, B, X, Y)
- **Special Moves**: Map MIDI buttons to shoulder buttons
- **Combos**: Use SequenceAction for complex button combinations

### Platformer Games
- **Movement**: Map MIDI controllers to left thumbstick or D-pad
- **Jump**: Map MIDI pedal to A button
- **Action**: Map MIDI buttons to other face buttons
- **Camera**: Map MIDI knobs to right thumbstick

### Flight Simulators
- **Pitch/Roll**: Map MIDI controllers to thumbstick axes
- **Throttle**: Map MIDI fader to trigger axis
- **Landing Gear**: Map MIDI button to face button
- **View Control**: Map MIDI knobs to right thumbstick

## Technical Implementation

### ViGEm Integration
- Uses ViGEm Client library for Xbox 360 controller emulation
- Automatic detection of ViGEm Bus Driver availability
- Real-time controller state updates from MIDI events
- Support for multiple simultaneous virtual controllers

### Value Mapping
- **MIDI to Axis**: MIDI values (0-127) mapped to axis range (-32768 to 32767)
- **MIDI to Trigger**: MIDI values (0-127) mapped to trigger range (0-255)
- **Button States**: MIDI Note On/Off mapped to button press/release
- **Real-time Updates**: Controller state updated immediately on MIDI events

### Performance
- Low-latency MIDI to controller mapping
- Efficient ViGEm API usage
- Minimal CPU overhead for controller emulation
- Thread-safe controller state management

## Troubleshooting

### ViGEm Issues
1. **Driver Not Detected**: Verify ViGEm Bus Driver installation
2. **Controller Not Appearing**: Check Windows Game Controllers in Control Panel
3. **Permission Issues**: Run MIDIFlux as administrator if needed
4. **Conflicting Software**: Close other controller emulation software

### Mapping Issues
1. **Wrong Buttons**: Verify button names in configuration
2. **Axis Problems**: Check MIDI CC numbers and axis names
3. **Multiple Controllers**: Ensure correct ControllerIndex values
4. **MIDI Events**: Use debug logging to verify MIDI input

### Game Compatibility
1. **Controller Not Recognized**: Some games require specific controller types
2. **Input Lag**: Adjust MIDI buffer settings if available
3. **Axis Sensitivity**: Games may have their own sensitivity settings
4. **Button Mapping**: Some games allow custom button mapping

## Related Actions

- **SequenceAction**: Create complex button combinations and macros
- **ConditionalAction**: Different controller actions based on MIDI values
- **AlternatingAction**: Toggle between different controller behaviors
- **DelayAction**: Add timing to controller sequences

## Best Practices

1. **Test Controllers**: Verify controller detection in Windows before gaming
2. **Start Simple**: Begin with basic button mappings before complex setups
3. **Use Descriptive Names**: Clear descriptions help with troubleshooting
4. **Multiple Devices**: Use different MIDI channels for different game functions
5. **Backup Configurations**: Save working configurations for different games

