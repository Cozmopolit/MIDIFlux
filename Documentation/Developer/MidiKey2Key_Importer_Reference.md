# MIDIKey2Key Importer — Developer Reference

## MIDIKey2Key INI Format

MIDIKey2Key uses the Windows INI file format (`GetPrivateProfileString`/`WritePrivateProfileString` via P/Invoke). Each action is stored in a `[ActionN]` section (0-based index).

### Global Sections

| Section | Key | Description |
|---------|-----|-------------|
| `[WindowPosition]` | `X`, `Y` | GUI window position |
| `[MidiDevice]` | `MidiIn` | Input device name |
| `[MidiDevice]` | `MidiOut` | Output device name |
| `[Switches]` | `SysEx` | SysEx matching enabled (0/1) |
| `[Switches]` | `Ch1`–`Ch16` | Per-channel enable (0/1) |
| `[Switches]` | `MirrorMidi` | MIDI thru (0/1) |

### Action Section Fields (`[ActionN]`)

| INI Key | Type | Description | MIDIFlux Model Property |
|---------|------|-------------|------------------------|
| `Name` | string | Action label | `MidiKey2KeyAction.Name` |
| `Comment` | string | User description | `.Comment` |
| `Data` | hex string | MIDI trigger data (e.g., `903C7F`, `B00A xx`, `STARTUP`) | `.Data` |
| `Keyboard` | string | Primary keyboard action (e.g., `Control+A`, `F5`) | `.Keyboard` |
| `KeyboardB` | string | Secondary keyboard action (used with ControllerAction) | `.KeyboardB` |
| `KeyboardDelay` | int (ms) | Delay between key press/release | `.KeyboardDelay` |
| `Hold` | 0/1 | Hold mode: NoteOn=KeyDown, NoteOff=KeyUp | `.Hold` |
| `ControllerAction` | 0/1 | CC direction mode: increase→Keyboard, decrease→KeyboardB | `.ControllerAction` |
| `SendMidi` | 0/1 | Enable MIDI output | `.SendMidi` |
| `SendMidiCommands` | hex string | MIDI bytes to send (space-separated, `P`=pause) | `.SendMidiCommands` |
| `Start` | string | Program path to execute | `.Start` |
| `Arguments` | string | Program arguments | `.Arguments` |
| `WorkingDirectory` | string | Program working directory | `.WorkingDirectory` |

### MIDI Data Format

The `Data` field is a hex string encoding a 3-byte MIDI message: `SSDDVV` where:
- `SS` = Status byte (message type + channel, e.g., `90` = NoteOn Ch1, `B0` = CC Ch1)
- `DD` = Data1 (note number or CC number)
- `VV` = Data2 (velocity or CC value), `xx` = wildcard (any value)

Special values: `STARTUP` = run on profile load, `xxxxxx` = placeholder (used with Name/Comment=`STARTUP`).

### Keyboard String Format

Keys are `.NET Keys` enum names joined with `+` or `,` as combination delimiters. Semicolons separate sequential key presses. Examples:
- `Control+A` → Ctrl+A
- `Shift,A` → Shift+A (comma = combination, not sequence)
- `A;B` → Press A, then press B
- `NUMPADRETURN` → Special token for Numpad Enter (mapped to VK 0x0D)

## MIDIFlux Importer Code Structure

```
src/MIDIFlux.GUI/Services/Import/
├── IMidiKey2KeyImporter.cs          — Interface
├── MidiKey2KeyImporter.cs           — Orchestrator: parse → convert → save
├── Parsers/
│   ├── IniFileParser.cs             — Reads INI sections into MidiKey2KeyAction objects
│   └── MidiDataParser.cs            — Parses hex MIDI strings (903C7F → NoteOn, Ch1, Note 60)
├── Converters/
│   ├── ActionConverter.cs           — Converts MK2K actions → MIDIFlux ActionBase instances
│   ├── MidiMappingConverter.cs      — Converts MK2K MIDI data → MIDIFlux MappingConfigEntry
│   └── SysExConverter.cs            — Handles SysEx pattern conversion
└── Models/
    ├── MidiKey2KeyAction.cs         — Data model for a parsed MK2K action
    ├── MidiKey2KeyConfig.cs         — Top-level config (device info + list of actions)
    └── ImportResult.cs              — Result with errors, warnings, statistics
```

### Key Conversion Logic (ActionConverter)

1. **Program execution** (`Start` non-empty) → `CommandExecutionAction`
2. **Keyboard + ControllerAction** → `AbsoluteCCDirectionAction` (Keyboard→Increase, KeyboardB→Decrease)
3. **Keyboard** → `KeyPressReleaseAction` (or `SequenceAction` for semicolon-separated sequences)
4. **Hold mode** → Two mappings: NoteOn→`KeyDownAction`, NoteOff→`KeyUpAction`
5. **MIDI output** → `MidiOutputAction`
6. **Startup** → Skipped (not convertible to MIDI-triggered mapping)
## MIDIKey2Key Reference Source Code

**Repository location:** `Referenz-Projekte/MIDIKey2Key/MidiKey2Key/` (relative to VSC_Projekte)

### Key Source Files

| Relative Path | Purpose |
|---------------|---------|
| `PureMidi/Monitor/Monitor.cs` | **Core execution engine.** Contains `Spass2()` — the main MIDI message handler that processes incoming data against configured actions. All runtime behavior (hold mode, controller action, key sending, MIDI output) lives here. |
| `Ini/IniFile.cs` | INI read/write via Win32 `GetPrivateProfileString`. Simple P/Invoke wrapper. |
| `MouseKeyboardLibrary/KeyboardSimulator.cs` | Low-level keyboard simulation. Contains `sendSpecial()` for extended keys (Numpad Enter). |
| `EncDeSer/EDeSe.cs` | Encryption/deserialization utilities. |

### Critical Method: `Spass2()` in Monitor.cs

This is where MIDIKey2Key processes incoming MIDI messages. The `strArray` variable holds the pipe-delimited action data with these indices:

| Index | Content | Notes |
|-------|---------|-------|
| 0 | MIDI Data hex (`903C7F`) | Status + Data1 + Data2 |
| 1 | Keyboard string | Primary keyboard action |
| 2 | Start (program path) | Program to execute |
| 3 | Arguments | Program arguments |
| 4 | Window style | `0`=Normal, `1`=Maximized, `2`=Minimized, `3`=Hidden |
| 5 | SendMidiCommands | MIDI output commands |
| 6 | SendMidiCommandsB | Toggle MIDI output (alternate) |
| 7 | WorkingDirectory | For program execution |
| 8 | KeyboardB | Secondary keyboard action (controller action decrease) |
| 9 | ControllerAction flag | `"1"` = direction-aware CC mapping |
| 10 | KeyboardDelay | Delay string (ms), or `"HOLD"` for hold mode |
| 11–19 | TrackStar (TS) fields | TouchStrip/lever integration (not imported) |
| 20 | Hold flag | `"1"` = hold mode enabled |

## Known Limitations & Future Work

- **Extended key flag:** `NUMPADRETURN` is mapped to VK 0x0D but loses the `KEYEVENTF_EXTENDEDKEY` distinction. Revisit if users request Numpad Enter vs Main Enter differentiation.
- **Multiplier:** MIDIKey2Key's Controller Action multiplier (repeat count per CC change) is not imported.
- **TrackStar fields:** Indices 11–19 relate to a niche hardware integration and are not imported.
- **Toggle MIDI output:** `SendMidiCommandsB` (toggle alternate) is not imported.

