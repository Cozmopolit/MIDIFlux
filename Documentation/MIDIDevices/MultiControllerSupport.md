# Multiple Game Controller Support

MIDIFlux now supports mapping MIDI inputs to up to 4 virtual game controllers simultaneously. This document explains how to configure and use this feature.

## Overview

The multi-controller feature allows you to:

- Map MIDI inputs to up to 4 different virtual Xbox 360 controllers
- Specify which controller each button or axis should affect
- Mix and match controllers across different MIDI devices
- Set default controller indices for each device

## Configuration

### Controller Indices

Controllers are identified by an index from 0 to 3:

- Controller 0: The primary controller (default)
- Controller 1: The second controller
- Controller 2: The third controller
- Controller 3: The fourth controller

### Configuration Format

To specify which controller a button or axis should affect, add a `controllerIndex` property to the mapping:

```json
{
  "midiNote": 52,
  "button": "A",
  "controllerIndex": 1  // Maps to the second controller (index 1)
}
```

For axis mappings:

```json
{
  "controlNumber": 42,
  "axis": "LeftTrigger",
  "minValue": 0,
  "maxValue": 127,
  "invert": false,
  "controllerIndex": 2  // Maps to the third controller (index 2)
}
```

### Default Controller Index

You can set a default controller index for all mappings in a device:

```json
"gameControllerMappings": {
  "defaultControllerIndex": 1,  // Default to the second controller
  "buttons": [
    // These will use controller index 1 unless overridden
    { "midiNote": 52, "button": "A" },
    { "midiNote": 54, "button": "B" },
    // This one overrides the default
    { "midiNote": 55, "button": "X", "controllerIndex": 0 }
  ]
}
```

## Example Configuration

Here's an example configuration that maps MIDI inputs to multiple controllers:

```json
{
  "midiDevices": [
    {
      "deviceName": "PACER",
      "midiChannels": [],
      "gameControllerMappings": {
        "defaultControllerIndex": 0,
        "buttons": [
          { "midiNote": 52, "button": "A", "controllerIndex": 0 },
          { "midiNote": 54, "button": "B", "controllerIndex": 0 },
          { "midiNote": 62, "button": "A", "controllerIndex": 1 },
          { "midiNote": 64, "button": "B", "controllerIndex": 1 }
        ],
        "axes": [
          {
            "controlNumber": 42,
            "axis": "LeftTrigger",
            "controllerIndex": 0
          },
          {
            "controlNumber": 43,
            "axis": "LeftTrigger",
            "controllerIndex": 1
          }
        ]
      }
    },
    {
      "deviceName": "Traktor Kontrol S2 MK3",
      "midiChannels": [],
      "gameControllerMappings": {
        "defaultControllerIndex": 2,
        "buttons": [
          { "midiNote": 52, "button": "DPadUp" },  // Uses default (2)
          { "midiNote": 62, "button": "A", "controllerIndex": 3 }
        ]
      }
    }
  ]
}
```

## Backward Compatibility

For backward compatibility, if no `controllerIndex` is specified, the mapping will default to controller 0 (the primary controller). This ensures that existing configurations continue to work without modification.

## Testing

To test your multi-controller configuration:

1. Load your configuration file in MIDIFlux
2. Open the Windows Game Controllers panel (`joy.cpl`)
3. You should see multiple Xbox 360 controllers listed
4. Select each controller and test the buttons and axes

## Limitations

- A maximum of 4 controllers are supported (indices 0-3)
- All controllers are Xbox 360 controllers
- Controllers are created on-demand when first used
- Invalid controller indices (outside 0-3) will be clamped to the valid range

## Troubleshooting

If you encounter issues with multiple controllers:

- Check that the ViGEm driver is installed and working
- Verify that your controller indices are within the valid range (0-3)
- Check the MIDIFlux logs for any error messages
- Ensure that your MIDI device is properly connected and recognized

