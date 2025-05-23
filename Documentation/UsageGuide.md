# MIDIFlux Usage Guide

This guide provides an overview of how to use MIDIFlux to map MIDI events from your devices to keyboard actions.

## Basic Concepts

MIDIFlux works by:
1. Capturing MIDI events from connected devices
2. Mapping those events to keyboard actions based on your configuration
3. Simulating keyboard keypresses on your computer

### MIDI Events

MIDI devices generate various types of events:

- **Note On/Off**: When keys or pads are pressed and released
- **Control Change**: When knobs, sliders, or pedals are adjusted
- **Program Change**: When presets are changed
- **Other Events**: Pitch bend, aftertouch, etc.

MIDIFlux primarily focuses on Note On/Off and Control Change events for mapping to keyboard actions.

### Keyboard Actions

MIDIFlux can simulate:

- Single key presses (e.g., A, B, C)
- Modifier keys (Shift, Ctrl, Alt)
- Key combinations (e.g., Ctrl+C, Alt+Tab)
- Key sequences (pressing multiple keys in order)

## Getting Started

### Installation

1. Download the latest release from the GitHub repository
2. Extract the files to a directory of your choice
3. Run the application using the command line or by double-clicking the executable

### First Run

When you first run MIDIFlux:

1. It will scan for available MIDI devices
2. The application will appear in the system tray
3. Right-click the system tray icon to see available options
4. You can view connected MIDI devices by selecting "Show MIDI Devices"
5. Select a configuration from the menu to start mapping MIDI events

## Configuration

MIDIFlux uses JSON configuration files to define mappings between MIDI events and keyboard actions.

### Configuration File Structure

Configuration files use the following structure:

```json
{
  "midiDevices": [
    {
      "inputProfile": "Default",
      "deviceName": "MIDI Controller",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 60,
          "virtualKeyCode": 77,
          "modifiers": [],
          "description": "YouTube mute toggle (M key)"
        }
      ],
      "absoluteControlMappings": [],
      "relativeControlMappings": [],
      "gameControllerMappings": null
    }
  ]
}
```

### Device Configurations

Each MIDI device configuration defines:

- **inputProfile**: A unique identifier for this configuration
- **deviceName**: The name of the MIDI device to use
- **midiChannels**: Optional MIDI channel filtering (null or empty means all channels)
- **mappings**: List of MIDI note to keyboard key mappings
- **absoluteControlMappings**: Mappings for faders, knobs, etc.
- **relativeControlMappings**: Mappings for jog wheels, etc.
- **gameControllerMappings**: Game controller button and axis mappings

### Mappings

Each mapping defines:

- **midiNote**: The MIDI note number to respond to (0-127)
- **virtualKeyCode**: The Windows virtual key code to press
- **modifiers**: Optional modifier keys (Shift, Ctrl, Alt, etc.)
- **actionType**: The type of action (PressAndRelease, KeyDown, KeyUp, Toggle, CommandExecution)
- **description**: Optional description of the mapping

### Virtual Key Codes

Keyboard actions use Windows Virtual Key Codes to specify which keys to press. Common codes include:

- Letters: A-Z (65-90)
- Numbers: 0-9 (48-57)
- Function keys: F1-F12 (112-123)
- Modifiers: Shift (16), Ctrl (17), Alt (18)

A full list of virtual key codes can be found in the [Windows documentation](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes).

## Advanced Usage

### Multiple Devices

MIDIFlux now supports using multiple MIDI devices simultaneously in a single configuration. This allows you to:

1. Map different MIDI devices to different functions
2. Map the same MIDI notes from different devices to different actions
3. Create more complex control setups with specialized controllers

To use multiple devices, create a configuration file with multiple device entries:

```json
{
  "midiDevices": [
    {
      "inputProfile": "PACER-BasicKeys",
      "deviceName": "PACER",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 52,
          "virtualKeyCode": 67,
          "modifiers": [162],
          "description": "Copy (Ctrl+C)"
        }
      ],
      "absoluteControlMappings": [],
      "relativeControlMappings": []
    },
    {
      "inputProfile": "Traktor-Controls",
      "deviceName": "Traktor Kontrol S2 MK3",
      "midiChannels": [4],
      "mappings": [
        {
          "midiNote": 20,
          "virtualKeyCode": 65,
          "modifiers": [],
          "description": "Press A key"
        }
      ],
      "absoluteControlMappings": [],
      "relativeControlMappings": []
    }
  ]
}
```

Each device can have its own set of mappings, MIDI channels, and controller configurations.

### Channel Filtering

You can configure MIDIFlux to only respond to events on specific MIDI channels. This is useful if your device sends data on multiple channels and you only want to map certain channels to keyboard actions.

### Hot-Plugging

MIDIFlux supports hot-plugging, which means you can disconnect and reconnect your MIDI devices without restarting the application.

### Logging

MIDIFlux logs all events to the `Logs` directory in the application folder. The log files contain detailed information about:

1. MIDI devices that are detected
2. Configuration files that are loaded
3. MIDI events that are received (at Debug log level)
4. Any errors or warnings that occur

Log files are useful for troubleshooting issues with your MIDI devices or configurations. The application automatically rotates log files to prevent them from growing too large.

## Troubleshooting

### Device Not Detected

If your MIDI device is not detected:

1. Make sure it's properly connected to your computer
2. Check if it requires drivers and that they are installed
3. Try a different USB port
4. Restart the application

### Mappings Not Working

If your mappings aren't triggering keyboard actions:

1. Check the log files in the `Logs` directory to see MIDI events (debug level logging is enabled by default)
2. Check your configuration file for errors
3. Make sure you're using the correct virtual key codes
4. Verify that the MIDI channel settings match your device's output
5. For multi-device configurations, ensure each device has the correct `deviceName` that matches what MIDIFlux detects
6. Use the "Show MIDI Devices" option in the system tray menu to verify your devices are detected

### Multi-Device Troubleshooting

When using multiple MIDI devices:

1. Make sure each device is properly connected and recognized by Windows
2. Check that the device names in your configuration match the actual device names
3. MIDIFlux will attempt partial matching if exact matches aren't found
4. If a device isn't found, MIDIFlux will log a warning but continue with other configured devices

## Future Development

MIDIFlux is under active development. Future versions will include:

- A graphical user interface for easier configuration
- More advanced mapping options
- Support for additional MIDI event types
- Macro recording and playback
- Enhanced multi-device support and management
- Improved device name matching and selection
- Visual feedback for connected devices
- Support for device-specific key combinations

