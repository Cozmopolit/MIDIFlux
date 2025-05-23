# MIDIFlux Keyboard Mapping

This document explains how to use the keyboard mapping feature of MIDIFlux to map MIDI events from your devices to keyboard actions.

## Basic Concepts

MIDIFlux can map MIDI Note On/Off events to keyboard actions. When a mapped note is received, the application simulates the corresponding keyboard input using the Windows SendInput API.

### Mapping

MIDIFlux maps MIDI events to keyboard actions based on the configuration files you provide. When a mapped MIDI event is received, the application simulates the corresponding keyboard input.

## Configuration

Keyboard mappings are defined in JSON configuration files. The default configuration file is `config/default-mappings.json`.

### Configuration Format

MIDIFlux supports several configuration formats, including the new multi-device format:

#### 1. Multi-Device Format

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
          "modifiers": [],
          "actionType": 0
        }
      ]
    },
    {
      "deviceName": "Traktor Kontrol S2 MK3",
      "midiChannels": [1],
      "mappings": [
        {
          "midiNote": 52,  // Same note as PACER but maps to a different key
          "virtualKeyCode": 66,
          "modifiers": [],
          "actionType": 0
        }
      ]
    }
  ]
}
```

This format allows you to:
- Map multiple MIDI devices in a single configuration
- Map the same MIDI note from different devices to different actions
- Specify different MIDI channels for each device

#### 2. Simple Format

```json
{
  "midiDeviceName": "PACER",
  "midiChannels": [1],
  "mappings": [
    {
      "midiNote": 52,
      "virtualKeyCode": 65,
      "modifiers": [],
      "actionType": 0
    }
  ]
}
```

- **midiDeviceName**: The name of the MIDI device to use. If not specified or the device is not found, the user will be prompted to select a device.
- **midiChannels**: An array of MIDI channels to listen to (1-16). If empty or null, all channels are accepted.
- **mappings**: An array of key mappings.
  - **midiNote**: The MIDI note number to map.
  - **virtualKeyCode**: The Windows virtual key code to press/release.
  - **modifiers**: An array of modifier key codes to hold while pressing/releasing the main key.
  - **actionType**: The type of action to perform (0=PressAndRelease, 1=KeyDown, 2=KeyUp, 3=Toggle).



### Virtual Key Codes

Windows uses virtual key codes to identify keys. Here are some common codes:

- Letters: A-Z (65-90)
- Numbers: 0-9 (48-57)
- Function keys: F1-F12 (112-123)
- Backspace: 8
- Tab: 9
- Enter: 13
- Escape: 27
- Space: 32
- Print Screen: 44
- Insert: 45
- Delete: 46

#### Modifier Keys

For modifier keys, it's recommended to use the specific left/right key codes rather than the generic ones:

- Left Shift: 160 (preferred over generic Shift: 16)
- Right Shift: 161
- Left Ctrl: 162 (preferred over generic Ctrl: 17)
- Right Ctrl: 163
- Left Alt: 164 (preferred over generic Alt: 18)
- Right Alt (Alt Gr): 165

Using the specific left/right key codes provides better compatibility with special keys and ensures proper key simulation.

For a complete list, see the [Windows Virtual Key Codes](https://docs.microsoft.com/en-us/windows/win32/inputdev/virtual-key-codes) documentation.

## Sample Configurations

MIDIFlux comes with several sample configuration files in the `config_examples` directory:

1. **example-basic-keys.json**: Maps MIDI notes to common keyboard shortcuts:
   - MIDI Note 52: Ctrl+C (Copy)
   - MIDI Note 54: Ctrl+V (Paste)
   - MIDI Note 55: Ctrl+X (Cut)
   - MIDI Note 57: Ctrl+Z (Undo)
   - MIDI Note 59: Ctrl+Y (Redo)
   - MIDI Note 60: Ctrl+S (Save)

3. **simplified-modifier-keys.json**: Maps the footswitches to modifier and special keys using the simplified format:
   - Pedal 1: Left Shift
   - Pedal 2: Left Ctrl
   - Pedal 3: Left Alt
   - Pedal 4: Right Alt (Alt Gr)
   - Pedal 5: Print Screen
   - Pedal 6: Backspace

4. **modifier-keys-mappings.json**: Same as above but using the legacy format.

5. **key-combinations-and-macros.json**: Demonstrates advanced key combinations and macros:
   - Pedal 1: Ctrl+C (Copy) using the sequence format
   - Pedal 2: Ctrl+V (Paste) using the sequence format
   - Pedal 3: Ctrl+I (Italic) using the sequence format
   - Pedal 4: Types "HELLO WORLD" with delays between keystrokes
   - Pedal 5: Alt+Tab (Switch windows)
   - Pedal 6: Ctrl+Shift+Esc (Task Manager)

## Usage

To use keyboard mapping in MIDIFlux:

1. **Run the application**:
   ```
   dotnet run --project src\MIDIFlux.App
   ```

2. **Load a configuration** with keyboard mappings:
   ```
   dotnet run --project src\MIDIFlux.App --config config_examples/example-basic-keys.json
   ```

3. **Use the GUI**: Right-click the system tray icon and select "Configuration Editor" to create and edit keyboard mappings.

Command-line options:
- `--config <path>`: Specifies the configuration file to use (enables mapping mode)
- `--device <id>`: Specifies the MIDI device ID to use
- `--log <path>`: Enables file logging to the specified path
- `--silent`: Suppresses detailed mapping information in the console

## Creating Custom Mappings

To create your own mappings:

1. Start with one of the sample configuration files
2. Modify the mappings to suit your needs
3. Save the file with a descriptive name
4. Run MIDIFlux with the `--config` option pointing to your custom configuration file

## Configuration Restrictions

To ensure consistent behavior, MIDIFlux enforces the following restrictions:

1. **No Duplicate Key Mappings**: In the simple mapping format, you cannot map multiple MIDI notes to the same key. This prevents inconsistent behavior when multiple notes are pressed and released in different orders. If duplicate mappings are detected, the application will exit with an error message.

   This restriction only applies to the simple mapping format. Key combinations and macros using the sequence format can use the same keys in different sequences.

## Troubleshooting

If your mappings aren't working:

1. Check the log files in the `Logs` directory to see MIDI events (debug level logging is enabled by default)
2. Check your configuration file for errors
3. Make sure you're using the correct virtual key codes
4. Verify that the MIDI channel settings match your device's output
5. Check for duplicate key mappings in the simple format
6. For multi-device configurations, ensure each device has the correct `deviceName` that matches what MIDIFlux detects
7. Use the "Show MIDI Devices" option in the system tray menu to verify your devices are detected

### Multi-Device Troubleshooting

When using multiple MIDI devices:

1. Make sure each device is properly connected and recognized by Windows
2. Check that the device names in your configuration match the actual device names
3. MIDIFlux will attempt partial matching if exact matches aren't found
4. If a device isn't found, MIDIFlux will log a warning but continue with other configured devices

### Special Keys Issues

If you're having trouble with specific keys:

1. **Modifier Keys**: Use the specific left/right key codes (160-165) instead of the generic ones (16-18)
2. **Print Screen**: This key requires special handling, which is built into the application
3. **Alt Gr (Right Alt)**: Use key code 165 specifically for this key
4. **Extended Keys**: Some keys like Insert, Delete, Home, End, etc. are extended keys and require special handling, which is built into the application

### Multiple Key Combinations

When using modifier keys with other keys:

1. Make sure the modifier key is pressed first, then the main key
2. Make sure the main key is released first, then the modifier key
3. This sequence is handled automatically by the application

## Future Enhancements

Future versions of MIDIFlux will include:

### Application Enhancements
- Support for multiple named profiles
- Support for MIDI Control Change messages
- Hot-plugging support for MIDI devices
- Enhanced logging and diagnostics
- Improved device name matching and selection
- Support for device-specific key combinations
- Enhanced toggle key functionality with visual feedback

### GUI Enhancements
As outlined in the design document, the GUI will be enhanced to:
- Provide an intuitive interface for creating mappings
- Offer "learn" functionality to detect MIDI inputs
- Allow testing configurations before saving
- Generate the JSON configuration files
- Support for configuring multiple MIDI devices in a visual interface
- Visual feedback for connected devices

