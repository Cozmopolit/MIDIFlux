# MIDIFlux Core Logic Review Blocks

## Overview

This document defines logical groupings of core MIDIFlux source files for systematic code review.
Each block represents a cohesive functional area that can be reviewed together.

## Review Blocks

| Block | Name |
|-------|------|
| 1 | MIDI Event Pipeline |
| 2 | Action System (Basis) |
| 3 | Action System (Serialization) |
| 4 | Hardware Abstraction |
| 5 | Configuration & State |
| 6 | MIDI Input/Models |
| 7 | Action Parameter System |
| 8 | Input Simulation Layer |
| 9 | Audio Pipeline |
| 10 | Keyboard Actions |
| 11 | MIDI Output Actions |
| 12 | Mouse & GameController Actions |
| 13 | Complex & Stateful Actions |
| 14 | Utility Actions |
| 15 | MIDI Utilities |
| 16 | Windows MIDI Services Adapter |
| 17 | Infrastructure & Helpers |
| 18 | Device Configuration & Remaining Models |

---

## Block 1: MIDI Event Pipeline

**Focus:** Event flow from hardware to action execution (the heart of MIDIFlux)

**Files:**
- `src/MIDIFlux.Core/Midi/MidiDeviceManager.cs`
- `src/MIDIFlux.Core/Processing/MidiActionEngine.cs`
- `src/MIDIFlux.Core/ProfileManager.cs`

**Review Questions:**
- Is the event flow clear and efficient?
- Are there any threading issues or race conditions?
- Is error handling consistent?
- Are there any performance bottlenecks on the hot path?

---

## Block 2: Action System (Basis)

**Focus:** Action interface, base class, and mapping structure

**Files:**
- `src/MIDIFlux.Core/Actions/IAction.cs`
- `src/MIDIFlux.Core/Actions/ActionBase.cs`
- `src/MIDIFlux.Core/Actions/ActionMapping.cs`
- `src/MIDIFlux.Core/Actions/ActionMappingRegistry.cs`

**Review Questions:**
- Is the IAction interface minimal and focused?
- Does ActionBase provide appropriate shared functionality?
- Is the mapping lookup efficient?
- Are there any design issues with the registry pattern?

---

## Block 3: Action System (Serialization & Types)

**Focus:** JSON deserialization and type discovery

**Files:**
- `src/MIDIFlux.Core/Actions/ActionJsonConverter.cs`
- `src/MIDIFlux.Core/Actions/ActionTypeRegistry.cs`
- `src/MIDIFlux.Core/Actions/ParametersJsonConverter.cs`

**Review Questions:**
- Is JSON deserialization robust and error-tolerant?
- Are error messages helpful for debugging profile issues?
- Is type discovery reliable?
- Are there any security concerns with type instantiation?

---

## Block 4: Hardware Abstraction

**Focus:** Hardware interface and NAudio implementation

**Files:**
- `src/MIDIFlux.Core/Hardware/IMidiHardwareAdapter.cs`
- `src/MIDIFlux.Core/Hardware/NAudioMidiAdapter.cs`
- `src/MIDIFlux.Core/Hardware/MidiAdapterFactory.cs`

**Review Questions:**
- Is the interface well-designed for multiple implementations?
- Is resource cleanup (IDisposable) handled correctly?
- Are device hot-plug scenarios handled properly?
- Is channel conversion (0-based vs 1-based) consistent?

---

## Block 5: Configuration & State

**Focus:** Profile loading, settings, and state management

**Files:**
- `src/MIDIFlux.Core/Configuration/ActionConfigurationLoader.cs`
- `src/MIDIFlux.Core/Configuration/AppSettingsManager.cs`
- `src/MIDIFlux.Core/State/ActionStateManager.cs`

**Review Questions:**
- Is configuration loading robust against malformed files?
- Are settings changes applied correctly?
- Is state management thread-safe?
- Are there any issues with state key validation?

---

## Block 6: MIDI Input/Models

**Focus:** MIDI data models and input matching

**Files:**
- `src/MIDIFlux.Core/Actions/MidiInput.cs`
- `src/MIDIFlux.Core/Actions/MidiInputType.cs`
- `src/MIDIFlux.Core/Models/MidiEvent.cs`
- `src/MIDIFlux.Core/Models/MidiEventArgs.cs`

**Review Questions:**
- Are the data models complete and well-structured?
- Is input matching logic correct for all MIDI event types?
- Are there any edge cases not handled?
- Is the 1-based channel convention consistently applied?

---

## Block 7: Action Parameter System

**Focus:** Parameter-Infrastruktur, die von allen Actions genutzt wird – Typdefinitionen, Enum-Handling, Validierung, Value Conditions und Hilfs-Extensions

**Files:**
- `src/MIDIFlux.Core/Actions/Parameters/Parameter.cs`
- `src/MIDIFlux.Core/Actions/Parameters/ParameterInfo.cs`
- `src/MIDIFlux.Core/Actions/Parameters/ParameterType.cs`
- `src/MIDIFlux.Core/Actions/Parameters/EnumDefinition.cs`
- `src/MIDIFlux.Core/Actions/Parameters/ValueCondition.cs`
- `src/MIDIFlux.Core/Actions/ActionHelper.cs`
- `src/MIDIFlux.Core/Actions/InputTypeCategory.cs`
- `src/MIDIFlux.Core/Actions/MidiInputTypeExtensions.cs`

**Review Questions:**
- Ist das Parameter-Typsystem konsistent und erweiterbar?
- Sind ValueConditions korrekt implementiert (Edge Cases)?
- Sind EnumDefinition/EnumParameter robust gegen ungültige Werte?
- Ist die InputTypeCategory/MidiInputTypeExtensions-Logik korrekt für alle MIDI-Eventtypen?


---

### Block 8: Input Simulation Layer (~1392 LOC, 6 files)

Die "Effektoren" des Systems — alle Simulatoren, die MIDI-Events in externe Eingaben umwandeln: Tastatur (SendInput API, Low-Level Hooks, String-Parsing), Maus (Klicks, Bewegung, Scrolling) und GameController (ViGEm Xbox360).

**Files:**
- `src/MIDIFlux.Core/Keyboard/KeyboardSimulator.cs`
- `src/MIDIFlux.Core/Keyboard/KeyboardListener.cs`
- `src/MIDIFlux.Core/Keyboard/KeyboardStringParser.cs`
- `src/MIDIFlux.Core/Extensions/KeyboardSimulatorExtensions.cs`
- `src/MIDIFlux.Core/Mouse/MouseSimulator.cs`
- `src/MIDIFlux.Core/GameController/GameControllerManager.cs`

**Review Questions:**
- Ist die SendInput-Nutzung korrekt (Scan-Codes, Extended Keys, Modifier-Reihenfolge)?
- Ist der Low-Level Keyboard Hook korrekt implementiert (Lifecycle, Thread-Safety)?
- Ist das String-Parsing für Tastenkombinationen vollständig und fehlertolerant?
- Sind Mouse- und GameController-Simulation korrekt gegen Race Conditions geschützt?
- Werden native Ressourcen (Hooks, ViGEm-Client) korrekt aufgeräumt?

---

### Block 9: Audio Pipeline (~969 LOC, 5 files)

Die komplette Audio-Pipeline von der Action bis zur Wiedergabe: PlaySoundAction triggert den AudioPlaybackService, der AudioFileLoader lädt und cached Dateien, AudioFormatConverter konvertiert MP3→WAV. Kritisch ist die Sub-10ms-Latenz durch Pre-Loading.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/PlaySoundAction.cs`
- `src/MIDIFlux.Core/Services/AudioPlaybackService.cs`
- `src/MIDIFlux.Core/Services/IAudioPlaybackService.cs`
- `src/MIDIFlux.Core/Helpers/AudioFileLoader.cs`
- `src/MIDIFlux.Core/Helpers/AudioFormatConverter.cs`

**Review Questions:**
- Ist das Pre-Loading robust (fehlerhafte Dateien, fehlende Pfade)?
- Ist die Audio-Wiedergabe thread-safe (parallele Aufrufe, Dispose)?
- Ist die MP3→WAV-Konvertierung korrekt und effizient?
- Werden Audio-Ressourcen (WaveOut, Streams) korrekt aufgeräumt?

---

### Block 10: Keyboard Actions (~983 LOC, 5 files)

Alle Keyboard-bezogenen Actions: KeyDown/KeyUp für gedrückt halten/loslassen, KeyPressRelease für Drücken+Loslassen, KeyToggle für Toggle-State, KeyModified für Tastenkombinationen mit Modifiern. Alle nutzen den KeyboardSimulator aus Block 8.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/KeyPressReleaseAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyDownAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyUpAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyToggleAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyModifiedAction.cs`

**Review Questions:**
- Sind Parameter konsistent definiert über alle Keyboard-Actions?
- Ist die Modifier-Behandlung in KeyModifiedAction korrekt (Press/Release-Reihenfolge)?
- Ist das Toggle-State-Management thread-safe?
- Wird die Scan-Code vs. Virtual-Key Entscheidung korrekt getroffen?

---

### Block 11: MIDI Output Actions (~1382 LOC, 7 files)

Actions, die MIDI-Nachrichten an Output-Geräte senden: ControlChange, NoteOn, NoteOff und SysEx. Dazu die zugehörigen Datenmodelle MidiOutputCommand, MidiControlType und MidiMessageType.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/MidiControlChangeAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiNoteOnAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiNoteOffAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiSysExAction.cs`
- `src/MIDIFlux.Core/Models/MidiOutputCommand.cs`
- `src/MIDIFlux.Core/Models/MidiControlType.cs`
- `src/MIDIFlux.Core/Models/MidiMessageType.cs`

**Review Questions:**
- Ist die MIDI-Nachrichtenkonstruktion korrekt (Status-Bytes, Data-Bytes)?
- Wird die 1-basierte Kanal-Konvention konsistent angewandt?
- Ist die Output-Geräte-Validierung robust?
- Entspricht das SysEx-Format dem MIDI-Standard (F0..F7)?

---

### Block 12: Mouse & GameController Actions (~1437 LOC, 8 files)

Maus-Actions (Klick, Scroll) und GameController-Actions (Button Press/Down/Up, Axis). Umfasst auch die Konfigurations-Enums ScrollDirection und MouseButton.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/MouseClickAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MouseScrollAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/GameControllerAxisAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/GameControllerButtonAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/GameControllerButtonDownAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/GameControllerButtonUpAction.cs`
- `src/MIDIFlux.Core/Actions/Configuration/ScrollDirection.cs`
- `src/MIDIFlux.Core/Models/MouseButton.cs`

**Review Questions:**
- Ist das MIDI-zu-Achse/Trigger-Mapping korrekt (Wertebereiche, Vorzeichen)?
- Ist das Button-State-Management konsistent über Button/ButtonDown/ButtonUp?
- Ist die ScrollDirection-Konfiguration vollständig?
- Werden GameController-Ressourcen korrekt freigegeben?

---

### Block 13: Complex & Stateful Actions (~1418 LOC, 10 files)

Orchestrierungs-Actions: SequenceAction führt Sub-Actions sequenziell aus, ConditionalAction wählt basierend auf MIDI-Wert, AlternatingAction wechselt zwischen Alternativen, RelativeCCAction verarbeitet relative Encoder-Werte. Dazu State-Management-Actions (Set, Increase, Decrease, Conditional) und zugehörige Enums.

**Files:**
- `src/MIDIFlux.Core/Actions/Complex/SequenceAction.cs`
- `src/MIDIFlux.Core/Actions/Complex/ConditionalAction.cs`
- `src/MIDIFlux.Core/Actions/Complex/AlternatingAction.cs`
- `src/MIDIFlux.Core/Actions/Complex/RelativeCCAction.cs`
- `src/MIDIFlux.Core/Actions/Stateful/StateConditionalAction.cs`
- `src/MIDIFlux.Core/Actions/Stateful/StateSetAction.cs`
- `src/MIDIFlux.Core/Actions/Stateful/StateIncreaseAction.cs`
- `src/MIDIFlux.Core/Actions/Stateful/StateDecreaseAction.cs`
- `src/MIDIFlux.Core/Actions/Configuration/SequenceErrorHandling.cs`
- `src/MIDIFlux.Core/Models/RelativeValueEncoding.cs`

**Review Questions:**
- Ist die Fehlerbehandlung in SequenceAction korrekt (Continue vs. Stop)?
- Evaluieren ConditionalAction/StateConditionalAction Grenzwerte korrekt?
- Ist das Alternating-Verhalten korrekt (Cycling, State-Reset)?
- Ist das State-Management thread-safe (concurrent reads/writes)?
- Ist die Relative-CC-Encoding-Logik korrekt für alle Formate (TwosComplement, SignBit, etc.)?

---

### Block 14: Utility Actions (~395 LOC, 3 files)

CommandExecutionAction startet externe Prozesse (cmd, PowerShell, Bash), DelayAction pausiert die Ausführung. CommandShellType definiert die verfügbaren Shell-Typen.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/CommandExecutionAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/DelayAction.cs`
- `src/MIDIFlux.Core/Models/CommandShellType.cs`

**Review Questions:**
- Gibt es Command-Injection-Risiken in CommandExecutionAction?
- Ist die Shell-Typ-Behandlung korrekt (Argument-Escaping pro Shell)?
- Ist die Delay-Präzision ausreichend für MIDI-Szenarien?
- Werden externe Prozesse korrekt aufgeräumt (Timeout, Kill)?

---

### Block 15: MIDI Utilities (~793 LOC, 4 files)

MIDI-spezifische Hilfsfunktionen: MidiInputDetector erkennt eingehende MIDI-Nachrichten (für die GUI), SysExPatternMatcher gleicht SysEx-Nachrichten gegen Muster ab (Wildcards), HexByteConverter konvertiert zwischen Hex-Strings und Byte-Arrays, MidiDeviceHelper bietet Geräte-Informationen.

**Files:**
- `src/MIDIFlux.Core/Midi/MidiInputDetector.cs`
- `src/MIDIFlux.Core/Midi/SysExPatternMatcher.cs`
- `src/MIDIFlux.Core/Helpers/HexByteConverter.cs`
- `src/MIDIFlux.Core/Helpers/MidiDeviceHelper.cs`

**Review Questions:**
- Ist die SysEx-Pattern-Matching-Logik korrekt (Wildcards, Längenvarianz)?
- Behandelt der HexByteConverter alle Edge Cases (ungerade Längen, ungültige Zeichen)?
- Ist der MidiInputDetector thread-safe (concurrent device access)?
- Sind Timeouts und Cancellation korrekt implementiert?

---

### Block 16: Windows MIDI Services Adapter (~959 LOC, 2 files)

Die Windows MIDI Services Integration — eine vollständige alternative Hardware-Abstraktions-Implementierung neben NAudioMidiAdapter. Nutzt WinRT/COM-APIs für MIDI 2.0 Support. MidiAdapterType definiert die verfügbaren Adapter-Typen.

**Files:**
- `src/MIDIFlux.Core/Hardware/WindowsMidiServicesAdapter.cs`
- `src/MIDIFlux.Core/Hardware/MidiAdapterType.cs`

**Review Questions:**
- Ist die WinRT-API-Nutzung korrekt (async Lifecycle, Threading)?
- Ist die Device-Enumeration robust (Hot-Plug, fehlende Geräte)?
- Werden native COM-Ressourcen korrekt aufgeräumt?
- Ist die Fehlerbehandlung konsistent mit dem NAudioMidiAdapter (Block 4)?
- Wird die MIDI 1.0/2.0 Nachrichtenkonvertierung korrekt durchgeführt?

---

### Block 17: Infrastructure & Helpers (~1090 LOC, 5 files)

Anwendungs-Infrastruktur: AppDataHelper verwaltet Dateipfade und -struktur, ApplicationErrorHandler zentralisiert Fehlerbehandlung und -anzeige, SafeActivator erzeugt Typen sicher via Reflection, LoggingHelper konfiguriert Serilog, MidiLatencyAnalyzer misst und analysiert MIDI-Latenz.

**Files:**
- `src/MIDIFlux.Core/Helpers/AppDataHelper.cs`
- `src/MIDIFlux.Core/Helpers/ApplicationErrorHandler.cs`
- `src/MIDIFlux.Core/Helpers/SafeActivator.cs`
- `src/MIDIFlux.Core/Helpers/LoggingHelper.cs`
- `src/MIDIFlux.Core/Performance/MidiLatencyAnalyzer.cs`

**Review Questions:**
- Sind Dateipfad-Operationen robust (Sonderzeichen, lange Pfade, Berechtigungen)?
- Ist die Fehlerbehandlung user-friendly und gleichzeitig informativ für Debugging?
- Ist SafeActivator sicher gegen Type-Injection?
- Ist die Latenz-Messung präzise genug (Timer-Auflösung)?

---

### Block 18: Device Configuration & Remaining Models (~497 LOC, 4 files)

DeviceConfigurationManager verwaltet Geräte-Konfigurationen (Wildcards, Input/Output-Mapping). MidiDeviceInfo modelliert Geräteinformationen. ValidationResult ist das einheitliche Validierungsergebnis. IMidiProcessingService definiert die Schnittstelle für den MIDI-Verarbeitungsservice.

**Files:**
- `src/MIDIFlux.Core/Configuration/DeviceConfigurationManager.cs`
- `src/MIDIFlux.Core/Models/MidiDeviceInfo.cs`
- `src/MIDIFlux.Core/Models/ValidationResult.cs`
- `src/MIDIFlux.Core/Interfaces/IMidiProcessingService.cs`

**Review Questions:**
- Ist die Wildcard-Geräte-Logik ('*') korrekt für Input und Output?
- Ist das ValidationResult-Pattern konsistent angewandt?
- Ist IMidiProcessingService minimal und fokussiert?
- Werden fehlende/ungültige Geräte-Konfigurationen sauber behandelt?