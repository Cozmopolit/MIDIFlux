# Note-On Only Mode

MIDIFlux supports a "Note-On Only" mode that allows mapping MIDI notes to keyboard actions while ignoring Note-Off events. This document explains how to use this feature.

## Overview

The Note-On Only mode is useful for:

- MIDI controllers that don't send Note-Off events
- Creating momentary key presses with a fixed duration
- Simulating key presses without requiring a Note-Off event

## Configuration

Note-On Only mappings are defined in your configuration file using the `ignoreNoteOff` property.

### Basic Configuration

Here's a basic example of a Note-On Only mapping:

```json
{
  "midiNote": 60,
  "virtualKeyCode": 65,
  "modifiers": [],
  "ignoreNoteOff": true,
  "autoReleaseAfterMs": 500,
  "description": "Press 'A' key for 500ms"
}
```

### Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `midiNote` | int | The MIDI note number to map |
| `virtualKeyCode` | ushort | The virtual key code to press |
| `modifiers` | ushort[] | Optional modifier keys to hold |
| `ignoreNoteOff` | bool | Set to `true` to ignore Note-Off events |
| `autoReleaseAfterMs` | int? | Optional time in milliseconds after which to automatically release the key |
| `description` | string | Optional description of the mapping |

## Auto-Release Feature

When using Note-On Only mode, you can specify an optional `autoReleaseAfterMs` value to automatically release the key after a specified duration. This is useful for creating momentary key presses with a fixed duration.

### Without Auto-Release

If you don't specify `autoReleaseAfterMs`, the key will remain pressed indefinitely (or until you change configurations or exit the application).

Example:
```json
{
  "midiNote": 60,
  "virtualKeyCode": 65,
  "modifiers": [],
  "ignoreNoteOff": true,
  "description": "Press 'A' key indefinitely"
}
```

### With Auto-Release

If you specify `autoReleaseAfterMs`, the key will be automatically released after the specified duration.

Example:
```json
{
  "midiNote": 61,
  "virtualKeyCode": 66,
  "modifiers": [],
  "ignoreNoteOff": true,
  "autoReleaseAfterMs": 1000,
  "description": "Press 'B' key for 1 second"
}
```

## Use Cases

### MIDI Controllers Without Note-Off

Some MIDI controllers, especially DIY or specialized controllers, may not send Note-Off events. The Note-On Only mode allows you to use these controllers with MIDIFlux.

### Fixed-Duration Key Presses

For some applications, you may want to press a key for a fixed duration regardless of how long the MIDI note is held. The auto-release feature allows you to specify this duration.

### Momentary Actions

For actions that should be momentary (like taking a screenshot), you can use the Note-On Only mode with auto-release to ensure the key is only pressed briefly.

## Example Configuration

A complete example configuration file is available at `config/example-note-on-only.json`.

## Important Notes

1. All keys pressed using Note-On Only mode are automatically released when:
   - The application is shut down
   - The configuration is changed
   - MIDI processing is stopped
2. If you don't specify `autoReleaseAfterMs`, the key will remain pressed indefinitely.
3. The auto-release timer starts immediately after the Note-On event is processed.

