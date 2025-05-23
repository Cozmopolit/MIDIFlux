# Toggle Key Mapping

MIDIFlux supports a special "toggle" mode for key mappings, similar to how the CapsLock key works on a keyboard. This allows you to use MIDI notes to toggle the state of keys rather than just sending simple key press/release events.

## How Toggle Mode Works

In toggle mode:

1. When a MIDI note-on event is received, it toggles the state of the target key
2. If the key is currently released, it will be pressed
3. If the key is currently pressed, it will be released
4. MIDI note-off events are ignored in toggle mode
5. The key state is maintained until explicitly toggled again or the application is shut down

This is particularly useful for:
- Foot pedals where you want to toggle a key state with a single press
- Creating toggle switches for various functions
- Implementing "sticky keys" functionality

## Configuration

To use toggle mode, set the `actionType` property to `3` (Toggle) in your key mapping:

```json
{
  "midiNote": 60,
  "virtualKeyCode": 65,
  "modifiers": [],
  "actionType": 3
}
```

The `actionType` values are:
- `0`: PressAndRelease (default)
- `1`: KeyDown
- `2`: KeyUp
- `3`: Toggle

## Example Configuration

Here's an example configuration that maps MIDI note 60 to toggle the 'A' key (virtual key code 65):

```json
{
  "midiDevices": [
    {
      "deviceName": "Your MIDI Device Name",
      "midiChannels": [],
      "mappings": [
        {
          "midiNote": 60,
          "virtualKeyCode": 65,
          "modifiers": [],
          "actionType": 3
        }
      ]
    }
  ]
}
```

A complete example is available in the `config/toggle-example.json` file.

## Important Notes

1. All toggled keys are automatically released when:
   - The application is shut down
   - The configuration is changed
   - MIDI processing is stopped

2. The toggle state is stored "at the target key" level, not per device. This means:
   - Different MIDI inputs can control the same key's state
   - One MIDI input could toggle a key on, and another could toggle it off
   - This allows for flexible control schemes with multiple MIDI devices

3. Toggle mode only works with simple key mappings, not with action sequences.

4. You can mix toggle mappings with regular mappings in the same configuration file.

