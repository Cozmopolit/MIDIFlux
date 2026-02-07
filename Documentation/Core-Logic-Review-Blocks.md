# MIDIFlux Core Logic Review Blocks

## Overview

This document defines logical groupings of core MIDIFlux source files for systematic code review.
Each block represents a cohesive functional area that can be reviewed together.

## Review Status

| Block | Name | Status | Findings |
|-------|------|--------|----------|
| 1 | MIDI Event Pipeline | Done | - |
| 2 | Action System (Basis) | Done | - |
| 3 | Action System (Serialization) | Done | - |
| 4 | Hardware Abstraction | Done | - |
| 5 | Configuration & State | Done | - |
| 6 | MIDI Input/Models | Done | - |

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

## Reviewer Endpoints

Reviews are executed using the following LLM endpoints:
- `google-gemini-3-pro-preview` (Google Gemini 3 Pro)
- `azure-claude-opus-4-5-sweden` (Claude Opus 4.5)
- `openrouter-openai-gpt-5.2-pro` (OpenAI GPT-5.2 Pro)
- `openrouter-deepseek-deepseek-v3.2` (DeepSeek v3.2)
- `openrouter-moonshotai-kimi-k2.5` (Moonshot Kimi K2.5)

