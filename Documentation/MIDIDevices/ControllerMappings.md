# MIDI Controller Mappings

MIDIFlux supports mapping MIDI controllers to various system functions. MIDIFlux supports multiple MIDI devices simultaneously.

## Basic MIDI Note Mapping

The most common use case is mapping MIDI notes (from pads, keys, or buttons) to keyboard keys:

```json
{
  "midiDevices": [
    {
      "deviceName": "PACER",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 52,
          "virtualKeyCode": 65,  // 'A' key
          "modifiers": []
        },
        {
          "midiNote": 54,
          "virtualKeyCode": 66,  // 'B' key
          "modifiers": []
        },
        {
          "midiNote": 55,
          "virtualKeyCode": 67,  // 'C' key
          "modifiers": [16]      // With Shift (16) modifier
        }
      ]
    }
  ]
}
```

This maps:
- MIDI note 52 to the 'A' key
- MIDI note 54 to the 'B' key
- MIDI note 55 to the 'C' key with Shift held down

## Supported Controller Types

### Absolute Value Controls

Absolute value controls send a specific value (0-127) when moved. These include:

- Faders
- Knobs
- Sliders
- Buttons with variable pressure

### Relative Value Controls

Relative value controls send incremental changes when moved. These include:

- Jog wheels
- Endless rotary encoders

## Configuration Format

The configuration format supports multiple MIDI devices, each with its own mappings:

```json
{
  "midiDevices": [
    {
      "deviceName": "PACER",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 52,
          "virtualKeyCode": 65,
          "modifiers": []
        }
      ],
      "absoluteControlMappings": [
        {
          "controlNumber": 7,
          "handlerType": "SystemVolume",
          "minValue": 0,
          "maxValue": 127,
          "invert": false,
          "parameters": {}
        }
      ],
      "relativeControlMappings": [
        {
          "controlNumber": 16,
          "handlerType": "ScrollWheel",
          "sensitivity": 2,
          "invert": false,
          "encoding": 0,
          "parameters": {}
        }
      ]
    },
    {
      "deviceName": "Traktor Kontrol S2 MK3",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 52,  // Same note as PACER but maps to a different key
          "virtualKeyCode": 68,  // 'D' key
          "modifiers": []
        }
      ],
      "absoluteControlMappings": [
        {
          "controlNumber": 7,
          "handlerType": "SystemVolume",
          "minValue": 0,
          "maxValue": 127,
          "invert": false,
          "parameters": {}
        }
      ],
      "relativeControlMappings": [
        {
          "controlNumber": 16,
          "handlerType": "ScrollWheel",
          "sensitivity": 1,  // Different sensitivity than PACER
          "invert": false,
          "encoding": 0,
          "parameters": {}
        }
      ]
    }
  ]
}
```

### Absolute Control Mapping Properties

- **controlNumber**: The MIDI Control Change number (0-127)
- **handlerType**: The type of handler to use (e.g., "SystemVolume")
- **minValue**: The minimum MIDI value to consider (default: 0)
- **maxValue**: The maximum MIDI value to consider (default: 127)
- **invert**: Whether to invert the value (default: false)
- **parameters**: Additional parameters for the handler

### Relative Control Mapping Properties

- **controlNumber**: The MIDI Control Change number (0-127)
- **handlerType**: The type of handler to use (e.g., "ScrollWheel")
- **sensitivity**: The sensitivity multiplier (default: 1)
- **invert**: Whether to invert the direction (default: false)
- **encoding**: The encoding method for relative values:
  - 0: SignMagnitude (values 1-63 are positive, 65-127 are negative)
  - 1: TwosComplement (values 1-64 are positive, 127-65 are negative)
  - 2: BinaryOffset (64 is zero, above is positive, below is negative)
- **parameters**: Additional parameters for the handler

## Available Handlers

### Absolute Value Handlers

- **SystemVolume**: Controls the system volume
  - Parameters: None

### Relative Value Handlers

- **ScrollWheel**: Controls the mouse scroll wheel
  - Parameters:
    - **sensitivity**: The sensitivity multiplier (default: 1)

## Finding Control Numbers

To find the control numbers for your MIDI controller:

1. Run MIDIFlux: `dotnet run --project src\MIDIFlux.App`
2. Right-click the system tray icon and select "MIDI Input Detection"
3. Select your MIDI device and click "Start Listening"
4. Move the controls on your MIDI controller
5. Note the controller numbers displayed in the detection dialog

## Troubleshooting

If your control mappings aren't working:

1. Check that your MIDI controller is sending Control Change messages
2. Verify that you're using the correct control numbers
3. Make sure the handler type is correctly specified
4. For relative controls, try different encoding methods if the default doesn't work
5. For multi-device configurations, ensure each device has the correct `deviceName` that matches what MIDIFlux detects
6. Check the console output to see if your device is being detected and connected properly

### Device Name Matching

MIDIFlux attempts to match device names in the following order:
1. Exact case-insensitive match
2. Partial match (if the configured name is contained within the actual device name)
3. If no match is found, it will log a warning but continue with other configured devices

## Future Enhancements

Future versions of MIDIFlux will include:

- More handler types (mouse movement, media controls, etc.)
- Support for custom handler parameters
- A plugin system for third-party handlers
- Improved device name matching and selection
- GUI for configuring multi-device mappings

