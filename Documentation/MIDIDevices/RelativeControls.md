# Relative Controls in MIDIFlux

MIDIFlux supports relative controls like jog wheels and endless encoders. This document explains how to configure and use relative controls.

## What are Relative Controls?

Relative controls send incremental values rather than absolute positions. For example, a jog wheel might send a value of 65 when turned clockwise and 63 when turned counterclockwise, rather than an absolute position like a fader would.

## Relative Value Encodings

MIDI devices use different encoding methods for relative values. MIDIFlux supports the following encodings:

### Sign-Magnitude (Signed Bit)

- Values 0-63 represent positive increments (1 to 64)
- Values 65-127 represent negative increments (-1 to -63)
- Value 64 represents no change

This is the most common encoding method for relative controls.

### Two's Complement

- Values 0-64 represent positive increments (0 to 64)
- Values 65-127 represent negative increments (-63 to -1)

### Binary Offset

- Values 0-63 represent negative increments (-64 to -1)
- Values 65-127 represent positive increments (1 to 63)
- Value 64 represents no change

The Traktor Kontrol S2 MK3 jog wheel uses this encoding method.

## Configuring Relative Controls

To configure a relative control, add it to the `relativeControlMappings` section of your configuration file:

```json
"relativeControlMappings": [
  {
    "controlNumber": 30,
    "handlerType": "ScrollWheel",
    "sensitivity": 2,
    "parameters": {}
  }
]
```

### Configuration Options

- `controlNumber`: The MIDI control number (0-127)
- `handlerType`: The type of handler to use (e.g., "ScrollWheel")
- `sensitivity`: The sensitivity of the control (higher values make the control more responsive)
- `parameters`: Additional parameters for the handler

## Available Handlers

### ScrollWheelHandler

Maps a relative control to the mouse scroll wheel.

```json
{
  "controlNumber": 30,
  "handlerType": "ScrollWheel",
  "sensitivity": 2,
  "parameters": {}
}
```

## Example: Traktor Kontrol S2 MK3 Jog Wheel

The Traktor Kontrol S2 MK3 jog wheel sends Control Change messages on channel 4 with controller number 30. It uses Binary Offset encoding.

### Configuration Example

```json
{
  "midiDevices": [
    {
      "deviceName": "Traktor Kontrol S2 MK3 MIDI",
      "midiChannels": [4],
      "mappings": [],
      "absoluteControlMappings": [],
      "relativeControlMappings": [
        {
          "controlNumber": 30,
          "handlerType": "ScrollWheel",
          "sensitivity": 2,
          "parameters": {}
        }
      ]
    },
    {
      "deviceName": "Another MIDI Device",
      "midiChannels": [1],
      "mappings": [],
      "absoluteControlMappings": [],
      "relativeControlMappings": [
        {
          "controlNumber": 30,  // Same control number but from a different device
          "handlerType": "ScrollWheel",
          "sensitivity": 1,     // Different sensitivity
          "parameters": {}
        }
      ]
    }
  ]
}
```

This configuration maps the jog wheel to the mouse scroll wheel with a sensitivity of 2.

## Detecting Relative Controls

MIDIFlux automatically detects some relative controls, such as the Traktor Kontrol S2 MK3 jog wheel. For other controls, you may need to specify the `isRelative` and `relativeEncoding` properties in your configuration.

## Troubleshooting

If your relative control is not working correctly:

1. Check the log files in the `Logs` directory to see the raw MIDI values
2. Determine the encoding method by observing the values when turning the control
3. Update your configuration with the correct encoding method
4. Adjust the sensitivity if the control is too sensitive or not sensitive enough
5. For multi-device configurations, ensure each device has the correct `deviceName` that matches what MIDIFlux detects
6. Use the "Show MIDI Devices" option in the system tray menu to verify your devices are detected

### Multi-Device Troubleshooting

When using multiple MIDI devices with relative controls:

1. Make sure each device is properly connected and recognized by Windows
2. Check that the device names in your configuration match the actual device names
3. MIDIFlux will attempt partial matching if exact matches aren't found
4. If a device isn't found, MIDIFlux will log a warning but continue with other configured devices
5. Verify that the control numbers are correct for each device

