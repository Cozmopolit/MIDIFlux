# PlaySound Action

MIDIFlux supports high-quality, low-latency audio playback through the PlaySoundAction. This action enables triggering sound effects, audio cues, and musical samples from MIDI events with sub-10ms latency.

## PlaySoundAction

Plays audio files with pre-loading for zero-latency execution. Supports concurrent playback of multiple sounds with volume control and optional audio device selection.

**Configuration Type**: `PlaySoundAction`

**Supported Formats**:
- **WAV**: Uncompressed audio (recommended for lowest latency)
- **AIFF**: Audio Interchange File Format
- **MP3**: Compressed audio (automatically converted during profile loading)

## Configuration Format

```json
{
  "$type": "PlaySoundAction",
  "Parameters": {
    "FilePath": "button-click.wav",
    "Volume": 100,
    "AudioDevice": null
  },
  "Description": "Play button click sound"
}
```

## Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `FilePath` | string | Path to audio file (relative to %AppData%\MIDIFlux\sounds or absolute) |
| `Volume` | int | Playback volume (0-100, default: 100) |
| `AudioDevice` | string | Optional specific audio device name (null = default device) |
| `Description` | string | Optional description of the action |

### File Path Resolution

The `FilePath` parameter supports multiple path formats:

1. **Relative to sounds directory**: `"button-click.wav"` → `%AppData%\MIDIFlux\sounds\button-click.wav`
2. **Absolute path**: `"C:\MyProject\Sounds\effect.mp3"`
3. **Environment variables**: `"%USERPROFILE%\Music\sample.wav"`

**File Path Examples**:
```json
{
  "FilePath": "drum-kick.wav"           // Relative to sounds directory
}
```

```json
{
  "FilePath": "C:\\Sounds\\effect.mp3"  // Absolute path
}
```

### Volume Control

Volume is specified as an integer from 0 to 100:
- **0**: Silent (muted)
- **50**: Half volume
- **100**: Full volume (default)

### Audio Device Selection

The `AudioDevice` parameter allows targeting specific audio outputs:
- **null or empty**: Use system default audio device
- **Device name**: Target specific audio device (e.g., "Speakers", "Headphones")

## Complete Mapping Examples

### Basic Sound Playback

```json
{
  "Description": "Play drum kick on pad hit",
  "InputType": "NoteOn",
  "Channel": 10,
  "Note": 36,
  "Action": {
    "$type": "PlaySoundAction",
    "Parameters": {
      "FilePath": "drum-kick.wav",
      "Volume": 100,
      "AudioDevice": null
    },
    "Description": "Kick drum sample"
  }
}
```

### Volume-Controlled Playback

```json
{
  "Description": "Variable volume based on velocity",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "ConditionalAction",
    "Parameters": {
      "Conditions": [
        {
          "MinValue": 0,
          "MaxValue": 42,
          "Action": {
            "$type": "PlaySoundAction",
            "Parameters": {
              "FilePath": "piano-soft.wav",
              "Volume": 30,
              "AudioDevice": null
            },
            "Description": "Soft piano note"
          }
        },
        {
          "MinValue": 43,
          "MaxValue": 84,
          "Action": {
            "$type": "PlaySoundAction",
            "Parameters": {
              "FilePath": "piano-medium.wav",
              "Volume": 65,
              "AudioDevice": null
            },
            "Description": "Medium piano note"
          }
        },
        {
          "MinValue": 85,
          "MaxValue": 127,
          "Action": {
            "$type": "PlaySoundAction",
            "Parameters": {
              "FilePath": "piano-loud.wav",
              "Volume": 100,
              "AudioDevice": null
            },
            "Description": "Loud piano note"
          }
        }
      ]
    },
    "Description": "Velocity-sensitive piano"
  }
}
```

### Specific Audio Device

```json
{
  "Description": "Play notification to headphones",
  "InputType": "ControlChange",
  "Channel": 1,
  "Controller": 64,
  "Action": {
    "$type": "PlaySoundAction",
    "Parameters": {
      "FilePath": "notification.wav",
      "Volume": 80,
      "AudioDevice": "Headphones"
    },
    "Description": "Notification sound to headphones"
  }
}
```

### Multiple Sound Layers

```json
{
  "Description": "Layered drum sounds",
  "InputType": "NoteOn",
  "Channel": 10,
  "Note": 38,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "PlaySoundAction",
          "Parameters": {
            "FilePath": "snare-main.wav",
            "Volume": 100,
            "AudioDevice": null
          },
          "Description": "Main snare sound"
        },
        {
          "$type": "PlaySoundAction",
          "Parameters": {
            "FilePath": "snare-reverb.wav",
            "Volume": 40,
            "AudioDevice": null
          },
          "Description": "Snare reverb tail"
        }
      ]
    },
    "Description": "Layered snare drum"
  }
}
```