# Game Controller Integration with MIDIFlux

MIDIFlux can emulate a game controller using the ViGEm framework, allowing you to use your MIDI controllers with games that support Xbox controllers.

## Prerequisites

1. **Install ViGEm Bus Driver**: This is required for game controller emulation.
   - Download from the [official GitHub repository](https://github.com/ViGEm/ViGEmBus/releases)
   - Install the driver by running the installer
   - Restart your computer after installation

## Optional Feature

Game controller integration is an **optional feature** in MIDIFlux:
- The core keyboard/mouse mapping functionality works without any driver installation
- Game controller emulation is only available when the ViGEm driver is installed
- MIDIFlux will automatically detect if ViGEm is available and enable the feature

## Configuration

To use game controller emulation with MIDIFlux, you need to create a configuration file that maps MIDI controls to controller inputs.

### Multi-Device Configuration (Recommended)

You can now map multiple MIDI devices to a single game controller. This allows you to use different MIDI controllers for different aspects of gameplay. Here's an example configuration:

```json
{
  "midiDevices": [
    {
      "deviceName": "PACER",
      "midiChannels": [1],
      "gameControllerMappings": {
        "buttons": [
          { "midiNote": 52, "button": "A" },
          { "midiNote": 54, "button": "B" },
          { "midiNote": 55, "button": "X" },
          { "midiNote": 57, "button": "Y" }
        ],
        "axes": []
      }
    },
    {
      "deviceName": "Traktor Kontrol S2 MK3",
      "midiChannels": [4],
      "gameControllerMappings": {
        "buttons": [
          { "midiNote": 20, "button": "LeftShoulder" },
          { "midiNote": 21, "button": "RightShoulder" }
        ],
        "axes": [
          {
            "controlNumber": 42,
            "axis": "LeftThumbX",
            "minValue": 0,
            "maxValue": 127,
            "invert": false
          },
          {
            "controlNumber": 30,
            "axis": "LeftThumbY",
            "minValue": 0,
            "maxValue": 127,
            "invert": true
          }
        ]
      }
    }
  ]
}
```

### Button Mapping

Map MIDI notes to controller buttons:

- `midiNote`: The MIDI note number (0-127)
- `button`: The controller button to emulate
  - Valid values: `A`, `B`, `X`, `Y`, `LeftShoulder`, `RightShoulder`, `Back`, `Start`, `LeftThumb`, `RightThumb`, `DPadUp`, `DPadDown`, `DPadLeft`, `DPadRight`, `Guide`

### Axis Mapping

Map MIDI control changes to controller axes:

- `controlNumber`: The MIDI control number (0-127)
- `axis`: The controller axis to emulate
  - Valid values: `LeftThumbX`, `LeftThumbY`, `RightThumbX`, `RightThumbY`, `LeftTrigger`, `RightTrigger`
- `minValue`: The minimum MIDI value (default: 0)
- `maxValue`: The maximum MIDI value (default: 127)
- `invert`: Whether to invert the axis (default: false)

## Usage

1. Install the ViGEm Bus Driver
2. Create a configuration file with game controller mappings
3. Run MIDIFlux with your configuration
4. The game controller will be automatically created when MIDIFlux starts
5. Your MIDI controller will now function as an Xbox controller in games

## Troubleshooting

### Game Controller Not Detected

If games don't detect the emulated controller:

1. Verify that the ViGEm Bus Driver is properly installed
2. Check Windows Game Controllers in Control Panel to see if the Xbox controller appears
3. Try restarting your computer
4. Ensure no conflicting software is running

### Incorrect Mapping

If buttons or axes don't work as expected:

1. Check the log files in the `Logs` directory to verify the MIDI note and control numbers
2. Check your configuration file for errors
3. Try different axis settings (invert, min/max values)

## Technical Details

MIDIFlux uses the ViGEm framework to create a virtual Xbox 360 controller. This provides excellent compatibility with modern games, as most games support Xbox controllers natively.

The implementation:
- Creates the virtual controller only when ViGEm is available
- Maps MIDI notes directly to controller buttons
- Converts MIDI control values (0-127) to the appropriate range for each axis
- Updates the controller state in real-time as MIDI events are received

## Future Enhancements

Planned enhancements for game controller integration:

1. Support for multiple virtual controllers (one per configuration)
2. DualShock 4 (PlayStation) controller emulation
3. Advanced mapping options (combinations, macros)
4. Configuration UI for easier setup
5. Enhanced device-specific mapping options
6. Support for device-specific button combinations

