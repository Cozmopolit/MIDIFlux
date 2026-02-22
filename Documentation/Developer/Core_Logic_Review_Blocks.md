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

**Focus:** Parameter infrastructure used by all actions — type definitions, enum handling, validation, value conditions, and helper extensions

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
- Is the parameter type system consistent and extensible?
- Are ValueConditions correctly implemented (edge cases)?
- Are EnumDefinition/EnumParameter robust against invalid values?
- Is the InputTypeCategory/MidiInputTypeExtensions logic correct for all MIDI event types?


---

### Block 8: Input Simulation Layer (~1392 LOC, 6 files)

The "effectors" of the system — all simulators that convert MIDI events into external inputs: keyboard (SendInput API, low-level hooks, string parsing), mouse (clicks, movement, scrolling), and game controller (ViGEm Xbox 360).

**Files:**
- `src/MIDIFlux.Core/Keyboard/KeyboardSimulator.cs`
- `src/MIDIFlux.Core/Keyboard/KeyboardListener.cs`
- `src/MIDIFlux.Core/Keyboard/KeyboardStringParser.cs`
- `src/MIDIFlux.Core/Extensions/KeyboardSimulatorExtensions.cs`
- `src/MIDIFlux.Core/Mouse/MouseSimulator.cs`
- `src/MIDIFlux.Core/GameController/GameControllerManager.cs`

**Review Questions:**
- Is the SendInput usage correct (scan codes, extended keys, modifier order)?
- Is the low-level keyboard hook correctly implemented (lifecycle, thread safety)?
- Is the string parsing for key combinations complete and error-tolerant?
- Are mouse and game controller simulation properly protected against race conditions?
- Are native resources (hooks, ViGEm client) cleaned up correctly?

---

### Block 9: Audio Pipeline (~969 LOC, 5 files)

The complete audio pipeline from action to playback: PlaySoundAction triggers the AudioPlaybackService, AudioFileLoader loads and caches files, AudioFormatConverter converts MP3→WAV. Critical aspect is sub-10ms latency through pre-loading.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/PlaySoundAction.cs`
- `src/MIDIFlux.Core/Services/AudioPlaybackService.cs`
- `src/MIDIFlux.Core/Services/IAudioPlaybackService.cs`
- `src/MIDIFlux.Core/Helpers/AudioFileLoader.cs`
- `src/MIDIFlux.Core/Helpers/AudioFormatConverter.cs`

**Review Questions:**
- Is pre-loading robust (corrupted files, missing paths)?
- Is audio playback thread-safe (parallel calls, Dispose)?
- Is the MP3→WAV conversion correct and efficient?
- Are audio resources (WaveOut, streams) cleaned up correctly?

---

### Block 10: Keyboard Actions (~983 LOC, 5 files)

All keyboard-related actions: KeyDown/KeyUp for hold/release, KeyPressRelease for press+release, KeyToggle for toggle state, KeyModified for key combinations with modifiers. All use the KeyboardSimulator from Block 8.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/KeyPressReleaseAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyDownAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyUpAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyToggleAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/KeyModifiedAction.cs`

**Review Questions:**
- Are parameters consistently defined across all keyboard actions?
- Is modifier handling in KeyModifiedAction correct (press/release order)?
- Is toggle state management thread-safe?
- Is the scan code vs. virtual key decision made correctly?

---

### Block 11: MIDI Output Actions (~1382 LOC, 7 files)

Actions that send MIDI messages to output devices: ControlChange, NoteOn, NoteOff, and SysEx. Includes the associated data models MidiOutputCommand, MidiControlType, and MidiMessageType.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/MidiControlChangeAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiNoteOnAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiNoteOffAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/MidiSysExAction.cs`
- `src/MIDIFlux.Core/Models/MidiOutputCommand.cs`
- `src/MIDIFlux.Core/Models/MidiControlType.cs`
- `src/MIDIFlux.Core/Models/MidiMessageType.cs`

**Review Questions:**
- Is the MIDI message construction correct (status bytes, data bytes)?
- Is the 1-based channel convention consistently applied?
- Is output device validation robust?
- Does the SysEx format conform to the MIDI standard (F0..F7)?

---

### Block 12: Mouse & GameController Actions (~1437 LOC, 8 files)

Mouse actions (click, scroll) and game controller actions (Button Press/Down/Up, Axis). Also includes configuration enums ScrollDirection and MouseButton.

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
- Is the MIDI-to-axis/trigger mapping correct (value ranges, sign)?
- Is button state management consistent across Button/ButtonDown/ButtonUp?
- Is the ScrollDirection configuration complete?
- Are game controller resources properly released?

---

### Block 13: Complex & Stateful Actions (~1418 LOC, 10 files)

Orchestration actions: SequenceAction executes sub-actions sequentially, ConditionalAction selects based on MIDI value, AlternatingAction cycles between alternatives, RelativeCCAction processes relative encoder values. Also includes state management actions (Set, Increase, Decrease, Conditional) and associated enums.

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
- Is the error handling in SequenceAction correct (Continue vs. Stop)?
- Do ConditionalAction/StateConditionalAction evaluate boundary values correctly?
- Is the alternating behavior correct (cycling, state reset)?
- Is state management thread-safe (concurrent reads/writes)?
- Is the relative CC encoding logic correct for all formats (TwosComplement, SignBit, etc.)?

---

### Block 14: Utility Actions (~395 LOC, 3 files)

CommandExecutionAction launches external processes (cmd, PowerShell, Bash), DelayAction pauses execution. CommandShellType defines the available shell types.

**Files:**
- `src/MIDIFlux.Core/Actions/Simple/CommandExecutionAction.cs`
- `src/MIDIFlux.Core/Actions/Simple/DelayAction.cs`
- `src/MIDIFlux.Core/Models/CommandShellType.cs`

**Review Questions:**
- Are there command injection risks in CommandExecutionAction?
- Is shell type handling correct (argument escaping per shell)?
- Is the delay precision sufficient for MIDI scenarios?
- Are external processes cleaned up correctly (timeout, kill)?

---

### Block 15: MIDI Utilities (~793 LOC, 4 files)

MIDI-specific helper functions: MidiInputDetector detects incoming MIDI messages (for the GUI), SysExPatternMatcher matches SysEx messages against patterns (wildcards), HexByteConverter converts between hex strings and byte arrays, MidiDeviceHelper provides device information.

**Files:**
- `src/MIDIFlux.Core/Midi/MidiInputDetector.cs`
- `src/MIDIFlux.Core/Midi/SysExPatternMatcher.cs`
- `src/MIDIFlux.Core/Helpers/HexByteConverter.cs`
- `src/MIDIFlux.Core/Helpers/MidiDeviceHelper.cs`

**Review Questions:**
- Is the SysEx pattern matching logic correct (wildcards, variable length)?
- Does the HexByteConverter handle all edge cases (odd lengths, invalid characters)?
- Is the MidiInputDetector thread-safe (concurrent device access)?
- Are timeouts and cancellation correctly implemented?

---

### Block 16: Windows MIDI Services Adapter (~959 LOC, 2 files)

The Windows MIDI Services integration — a complete alternative hardware abstraction implementation alongside NAudioMidiAdapter. Uses WinRT/COM APIs for MIDI 2.0 support. MidiAdapterType defines the available adapter types.

**Files:**
- `src/MIDIFlux.Core/Hardware/WindowsMidiServicesAdapter.cs`
- `src/MIDIFlux.Core/Hardware/MidiAdapterType.cs`

**Review Questions:**
- Is the WinRT API usage correct (async lifecycle, threading)?
- Is device enumeration robust (hot-plug, missing devices)?
- Are native COM resources cleaned up correctly?
- Is error handling consistent with the NAudioMidiAdapter (Block 4)?
- Is the MIDI 1.0/2.0 message conversion performed correctly?

---

### Block 17: Infrastructure & Helpers (~1090 LOC, 5 files)

Application infrastructure: AppDataHelper manages file paths and directory structure, ApplicationErrorHandler centralizes error handling and display, SafeActivator creates types safely via reflection, LoggingHelper configures Serilog, MidiLatencyAnalyzer measures and analyzes MIDI latency.

**Files:**
- `src/MIDIFlux.Core/Helpers/AppDataHelper.cs`
- `src/MIDIFlux.Core/Helpers/ApplicationErrorHandler.cs`
- `src/MIDIFlux.Core/Helpers/SafeActivator.cs`
- `src/MIDIFlux.Core/Helpers/LoggingHelper.cs`
- `src/MIDIFlux.Core/Performance/MidiLatencyAnalyzer.cs`

**Review Questions:**
- Are file path operations robust (special characters, long paths, permissions)?
- Is error handling user-friendly while also informative for debugging?
- Is SafeActivator secure against type injection?
- Is latency measurement precise enough (timer resolution)?

---

### Block 18: Device Configuration & Remaining Models (~497 LOC, 4 files)

DeviceConfigurationManager manages device configurations (wildcards, input/output mapping). MidiDeviceInfo models device information. ValidationResult is the unified validation result. IMidiProcessingService defines the interface for the MIDI processing service.

**Files:**
- `src/MIDIFlux.Core/Configuration/DeviceConfigurationManager.cs`
- `src/MIDIFlux.Core/Models/MidiDeviceInfo.cs`
- `src/MIDIFlux.Core/Models/ValidationResult.cs`
- `src/MIDIFlux.Core/Interfaces/IMidiProcessingService.cs`

**Review Questions:**
- Is the wildcard device logic ('*') correct for input and output?
- Is the ValidationResult pattern consistently applied?
- Is IMidiProcessingService minimal and focused?
- Are missing/invalid device configurations handled cleanly?