# Game Controller Implementation in MIDIFlux

This document provides technical details about the implementation of game controller emulation in MIDIFlux using the ViGEm framework.

## Overview

MIDIFlux can emulate an Xbox 360 controller using the ViGEm framework, allowing MIDI controllers to be used with games that support Xbox controllers. This feature is optional and only available when the ViGEm Bus Driver is installed.

## Implementation Details

### Dependencies

- **Nefarius.ViGEm.Client**: .NET wrapper for the ViGEm Bus Driver
- **ViGEm Bus Driver**: Required system driver for virtual controller emulation

### Class Hierarchy

The game controller implementation uses a base class to share common functionality:

```
GameControllerBase (abstract)
├── GameControllerAxisHandler
└── GameControllerButtonHandler
```

### GameControllerBase

The `GameControllerBase` class provides common functionality for all game controller handlers:

- Initializes the ViGEm client and controller
- Handles connection and disconnection
- Provides methods for mapping button and axis names
- Converts MIDI values to controller values
- Handles error conditions when ViGEm is not available

### GameControllerButtonHandler

The `GameControllerButtonHandler` class implements the `INoteHandler` interface and maps MIDI notes to controller buttons:

- Handles note on/off events
- Maps note events to button presses/releases
- Supports all Xbox 360 controller buttons

### GameControllerAxisHandler

The `GameControllerAxisHandler` class implements both `IAbsoluteValueHandler` and `IRelativeValueHandler` interfaces:

- Handles absolute MIDI control values (0-127)
- Handles relative MIDI control values (increments/decrements)
- Maps MIDI values to controller axes and triggers
- Supports value range mapping and inversion

### Configuration

Game controller mappings can be defined for multiple MIDI devices in the configuration file:

#### Multi-Device Configuration (Recommended)

```json
"midiDevices": [
  {
    "deviceName": "PACER",
    "midiChannels": [1],
    "gameControllerMappings": {
      "buttons": [
        { "midiNote": 52, "button": "A" },
        { "midiNote": 54, "button": "B" }
      ],
      "axes": []
    }
  },
  {
    "deviceName": "Traktor Kontrol S2 MK3",
    "midiChannels": [4],
    "gameControllerMappings": {
      "buttons": [
        { "midiNote": 20, "button": "LeftShoulder" }
      ],
      "axes": [
        {
          "controlNumber": 42,
          "axis": "LeftThumbX",
          "minValue": 0,
          "maxValue": 127,
          "invert": false
        }
      ]
    }
  }
]
```

#### Legacy Single-Device Configuration

```json
"gameControllerMappings": {
  "buttons": [
    {
      "midiNote": 20,
      "button": "A"
    }
  ],
  "axes": [
    {
      "controlNumber": 42,
      "axis": "LeftThumbX",
      "minValue": 0,
      "maxValue": 127,
      "invert": false
    }
  ]
}
```

### Initialization Process

1. The `GameControllerBase` constructor attempts to initialize the ViGEm client
2. If the ViGEm Bus Driver is not installed, initialization fails gracefully
3. The `IsViGEmAvailable` property indicates whether ViGEm is available
4. All handlers check this property before attempting to use the controller

### Value Conversion

- MIDI values (0-127) are converted to appropriate controller values:
  - Axes: -32768 to 32767
  - Triggers: 0 to 255
- Value ranges can be customized with `minValue` and `maxValue`
- Values can be inverted with the `invert` parameter

## Supported Controller Elements

### Buttons

- A, B, X, Y
- LeftShoulder, RightShoulder
- Back, Start
- LeftThumb, RightThumb
- DPadUp, DPadDown, DPadLeft, DPadRight
- Guide

### Axes

- LeftThumbX, LeftThumbY
- RightThumbX, RightThumbY
- LeftTrigger, RightTrigger

## Error Handling

The implementation includes robust error handling:

- Graceful degradation when ViGEm is not available
- Logging of all errors and warnings
- Validation of button and axis names
- Protection against invalid value ranges

## Future Enhancements

Planned enhancements for game controller integration:

1. Support for multiple virtual controllers (one per configuration)
2. DualShock 4 (PlayStation) controller emulation
3. Advanced mapping options (combinations, macros)
4. Configuration UI for easier setup
5. Enhanced device-specific mapping options
6. Support for device-specific button combinations

## Testing

The implementation has been tested with:

- Various MIDI controllers
- Different games that support Xbox controllers
- Different mapping configurations
- Error conditions (ViGEm not installed, invalid mappings)

## Troubleshooting

Common issues and solutions:

- **Controller not detected**: Verify ViGEm Bus Driver is installed
- **Buttons not working**: Check MIDI note numbers in configuration
- **Axes not working**: Check control numbers and value ranges
- **Inverted controls**: Set `invert` to true for the axis
- **Device not recognized**: Verify the device name in the configuration matches the actual device name
- **Multiple devices not working**: Check that each device is properly connected and recognized by Windows

### Multi-Device Troubleshooting

When using multiple MIDI devices:

1. Make sure each device is properly connected and recognized by Windows
2. Check that the device names in your configuration match the actual device names
3. MIDIFlux will attempt partial matching if exact matches aren't found
4. If a device isn't found, MIDIFlux will log a warning but continue with other configured devices

