# MIDIFlux Feature Enhancement Specification

This document specifies three key feature enhancements that would significantly expand MIDIFlux capabilities for professional MIDI control workflows.

## Executive Summary

Three feature areas would enhance MIDIFlux for professional use cases:

1. **MIDI Output Actions** - Send MIDI messages to control external devices
2. **Alternating Actions** - Toggle between different actions on repeated triggers
3. **SysEx Support** - Handle System Exclusive messages for advanced devices

These features should be designed as native MIDIFlux capabilities using NAudio for MIDI output, following existing MIDIFlux architecture patterns.

## 1. MIDI Output Actions ❌ **CRITICAL GAP**

### Use Cases That Need MIDI Output

**Device Control:**
- Control LED feedback on MIDI controllers (light up pressed buttons)
- Send configuration messages to devices
- Trigger actions on other MIDI devices in a chain

**Creative Workflows:**
- MIDI input triggers MIDI output to different devices
- Complex MIDI routing and transformation
- Synchronized device control (multiple controllers working together)

### Current MIDIFlux Status
- ❌ **No MIDI output capability**
- ❌ **No MIDI device output management**
- ❌ **No MIDI message construction/sending**

### Technical Requirements

**Core Functionality:**
- Send MIDI messages to external devices for control and feedback
- Support multiple output devices simultaneously
- Handle timing between multiple MIDI commands
- Integrate seamlessly with existing MIDIFlux action system

**Existing MIDIFlux Infrastructure to Reuse:**
- `MidiManager` - Device management and connection handling patterns
- `MidiDeviceMonitor` - Device enumeration and monitoring for output devices
- `MidiEventConverter` - Reverse the conversion logic for output (MidiEvent → NAudio)
- `MidiDeviceInfo` - Device information structure for output devices
- NAudio integration patterns already established

### MIDIFlux Design Specification

**MIDI Output Action Implementation:**
```csharp
// Add to existing UnifiedActionType enum
public enum UnifiedActionType
{
    // ... existing types
    MidiOutput,  // ← NEW
}

// Configuration classes following MIDIFlux patterns
public class MidiOutputConfig : UnifiedActionConfig
{
    public string OutputDeviceName { get; set; } = "*";
    public List<MidiOutputCommand> Commands { get; set; } = new();
}

public class MidiOutputCommand
{
    public MidiMessageType MessageType { get; set; }
    public int Channel { get; set; } = 1;        // 1-16
    public int Data1 { get; set; } = 0;          // Note/Controller number
    public int Data2 { get; set; } = 0;          // Velocity/Value
    public int DelayAfterMs { get; set; } = 0;
    public string Description { get; set; } = "";
}

public enum MidiMessageType
{
    NoteOn,
    NoteOff,
    ControlChange,
    ProgramChange,
    PitchBend,
    // Add others as needed
}
```

**Reusing Existing MIDIFlux Infrastructure:**

**1. Device Management Pattern:**
- Extend `MidiManager` to handle both input and output devices
- Reuse device enumeration logic from `MidiDeviceMonitor`
- Apply same connection/disconnection handling patterns
- Use existing `MidiDeviceInfo` structure for output devices

**2. Event Conversion Pattern:**
- Create `MidiOutputConverter` as reverse of `MidiEventConverter`
- Reuse MIDI message type handling (NoteOn, ControlChange, etc.)
- Apply same channel conversion logic (1-based user facing, 0-based NAudio)
- Maintain consistent error handling patterns

**3. Architecture Integration:**
- Follow existing dependency injection patterns
- Use same logging infrastructure and patterns
- Apply existing disposal patterns for NAudio resources
- Integrate with existing error handling framework

**Example Configuration:**
```json
{
  "Action": {
    "$type": "MidiOutputConfig",
    "Commands": [
      {
        "MessageType": "NoteOn",
        "Channel": 1,
        "Data1": 60,
        "Data2": 127,
        "Description": "Light up C4 button"
      },
      {
        "MessageType": "NoteOff",
        "Channel": 1,
        "Data1": 60,
        "Data2": 0,
        "DelayAfterMs": 100,
        "Description": "Turn off C4 button after 100ms"
      }
    ]
  }
}
```

### Implementation Effort
**Estimated Time:** 3-4 days
**Complexity:** Medium
**Dependencies:** NAudio MIDI output integration

---

## 2. Alternating Actions ⚠️ **MODERATE GAP**

### Use Cases for Alternating Actions

**Toggle Behaviors:**
- **Play/Pause Control:** Same button alternates between play and pause
- **Mode Switching:** Toggle between different application modes
- **State Management:** On/Off states for features or devices

**Advanced Scenarios:**
- **Multi-State Cycling:** Cycle through multiple states with configurable patterns
- **Context-Sensitive Actions:** Different actions based on current state
- **Feedback Control:** Alternate between sending different MIDI feedback

### Current MIDIFlux Status
- ❌ **No alternating action support**
- ❌ **No per-mapping state management**
- ✅ **Has basic toggle functionality** (KeyToggleAction - different concept)

### Technical Requirements

**Core Functionality:**
- Execute different actions on repeated triggers of the same MIDI input
- Maintain state per mapping across application sessions
- Support any combination of action types (keyboard, MIDI output, etc.)
- Thread-safe state management for concurrent MIDI events

### MIDIFlux Design Specification

**Alternating Action Implementation:**
```csharp
// New action type
public enum UnifiedActionType
{
    // ... existing types
    AlternatingAction,  // ← NEW
}

public class AlternatingActionConfig : UnifiedActionConfig
{
    public UnifiedActionConfig PrimaryAction { get; set; } = null!;
    public UnifiedActionConfig SecondaryAction { get; set; } = null!;
    public bool StartWithPrimary { get; set; } = true;
    public int CycleCount { get; set; } = 1; // How many times to execute each action before switching
}

// State management service
public interface IAlternatingActionStateManager
{
    bool GetNextActionIsPrimary(string mappingId, int cycleCount = 1);
    void ResetState(string mappingId);
    void ResetAllStates();
}
```

**Implementation Notes:**
- Follow existing MIDIFlux action composition patterns
- Use dependency injection for state management service
- Implement state persistence for session continuity
- Add proper validation for nested action configurations

**Example Configuration:**
```json
{
  "Action": {
    "$type": "AlternatingActionConfig",
    "PrimaryAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 32,
      "Description": "Play (Spacebar)"
    },
    "SecondaryAction": {
      "$type": "KeyPressReleaseConfig",
      "VirtualKeyCode": 32,
      "Description": "Pause (Spacebar)"
    },
    "StartWithPrimary": true,
    "CycleCount": 1
  }
}
```

**Advanced Example with MIDI Output:**
```json
{
  "Action": {
    "$type": "AlternatingActionConfig",
    "PrimaryAction": {
      "$type": "MidiOutputConfig",
      "Commands": [{"MessageType": "NoteOn", "Channel": 1, "Data1": 60, "Data2": 127}]
    },
    "SecondaryAction": {
      "$type": "MidiOutputConfig",
      "Commands": [{"MessageType": "NoteOff", "Channel": 1, "Data1": 60, "Data2": 0}]
    }
  }
}
```

### Implementation Effort
**Estimated Time:** 1-2 days
**Complexity:** Low-Medium
**Dependencies:** State management system

---

## 3. SysEx Support ⚠️ **MODERATE GAP**

### Use Cases for SysEx Support

**LED Feedback Control (Primary Use Case):**
- Light up controller buttons to show current state
- Change button colors based on application status
- Provide visual feedback for user actions

**Device Display Updates:**
- Send text messages to controller LCD/OLED displays
- Update device status information
- Show current mode or configuration

**Device Configuration:**
- Set controller modes and behaviors
- Configure device-specific features
- Synchronize device state with application

### Current MIDIFlux Status
- ❌ **No SysEx input pattern matching**
- ❌ **No SysEx output actions**
- ❌ **No SysEx-specific configuration**

### Pattern-Based Approach

**Core Concept:** SysEx support as **opaque binary pattern matching** - no parsing or understanding of message content required.

**Input:** Match specific SysEx byte patterns to trigger actions
**Output:** Send specific SysEx byte patterns to control devices

### Technical Requirements

**Core Functionality:**
- Match exact SysEx byte patterns for input triggers
- Send exact SysEx byte patterns for output actions
- Handle variable-length SysEx data (minimum 3 bytes: F0...F7)
- No parsing or interpretation of SysEx content required

### MIDIFlux Design Specification

**Pattern-Based SysEx Implementation:**
```csharp
// Add to existing enums
public enum MidiMessageType
{
    // ... existing types
    SystemExclusive,  // ← NEW
}

public enum UnifiedActionMidiInputType
{
    // ... existing types
    SysEx,  // ← NEW: For SysEx pattern triggers
}

// SysEx Output Action
public class SysExOutputConfig : UnifiedActionConfig
{
    public byte[] Data { get; set; } = Array.Empty<byte>();
    public string Description { get; set; } = ""; // Optional, for documentation
}

// SysEx Input Specification
public class UnifiedActionMidiInput
{
    // ... existing properties
    public byte[]? SysExPattern { get; set; } // For SysEx input matching
}
```

**Example Configuration:**
```json
{
  "InputType": "NoteOn",
  "Note": 36,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 32,
        "Description": "Press Spacebar"
      },
      {
        "$type": "SysExOutputConfig",
        "Data": [240, 0, 32, 41, 2, 12, 3, 36, 127, 247],
        "Description": "Light up Launchpad button red"
      }
    ]
  }
}
```

### Implementation Requirements

**Pattern Matching Logic:**
```csharp
public class SysExPatternMatcher
{
    public bool Matches(byte[] receivedSysEx, byte[] configuredPattern)
    {
        return receivedSysEx.SequenceEqual(configuredPattern);
        // Simple byte-for-byte comparison - no parsing required
    }
}
```

**Basic Validation:**
- Validate SysEx structure (starts with F0, ends with F7)
- Minimum length validation (at least 3 bytes)
- No content parsing or interpretation required

**NAudio Integration:**
- Extend existing MIDI input handling for SysEx message reception
- Use NAudio's SysEx output capabilities for sending
- Follow existing MIDIFlux error handling patterns

### Implementation Effort
**Estimated Time:** 1-2 days
**Complexity:** Low-Medium
**Priority:** Low (specialized use cases)

---

## Implementation Priority Recommendation

### Phase 1: Essential Features (Week 1)
1. **MIDI Output Actions** - Enables 80% of missing functionality
2. **Basic Alternating Actions** - Covers common toggle use cases

### Phase 2: Complete Feature Parity (Week 2)
3. **Advanced Alternating Features** - Multiplier support
4. **SysEx Actions** - For specialized devices

### Phase 3: Import Functionality (Week 3)
5. **MIDIKey2Key Import** - Now with 95%+ coverage

## Impact on Import Functionality

### Before Missing Features Implementation
- **Import Coverage:** ~70%
- **Lost Features:** MIDI output, alternating actions, SysEx
- **User Experience:** Significant feature loss, manual reconfiguration required

### After Missing Features Implementation
- **Import Coverage:** 95%+
- **Lost Features:** Only Train Simulator integration (out of scope)
- **User Experience:** Near-seamless migration path

## Implementation Roadmap for Modern MIDIFlux Design

### Phase 1: MIDI Output Actions (3-4 days)

**Step 1.1: Core Infrastructure**
- Add `MidiOutput` to `UnifiedActionType` enum
- Create `MidiOutputConfig` and `MidiOutputCommand` classes
- Implement MIDI output device enumeration and management

**Step 1.2: Extend Existing MIDI Infrastructure**
```csharp
// Extend MidiManager to support output
public partial class MidiManager
{
    private readonly Dictionary<int, MidiOut> _midiOutputs = new();

    public bool StartOutputDevice(int deviceId) { /* Similar to StartListening */ }
    public void SendMidiMessage(int deviceId, MidiOutputCommand command) { /* New functionality */ }
}

// Extend MidiDeviceMonitor for output devices
public partial class MidiDeviceMonitor
{
    public List<MidiDeviceInfo> GetOutputDevices() { /* Similar to input enumeration */ }
}

// Create reverse converter
public class MidiOutputConverter
{
    public MidiEvent ConvertToNAudioEvent(MidiOutputCommand command) { /* Reverse of MidiEventConverter */ }
}
```

**Step 1.3: Action Execution**
- Create `MidiOutputAction` class implementing `IUnifiedAction`
- Integrate with existing action factory and configuration loader
- Add error handling for device disconnection and invalid parameters

**Step 1.4: Configuration Integration**
- Update JSON schema to support strongly-typed MIDI messages
- Add to action factory type mapping
- Create example configurations demonstrating common use cases

### Phase 2: Alternating Actions (1-2 days)

**Step 2.1: State Management Service**
```csharp
public class AlternatingActionStateManager : IAlternatingActionStateManager
{
    private readonly ConcurrentDictionary<string, AlternatingState> _states = new();

    public bool GetNextActionIsPrimary(string mappingId, int cycleCount = 1)
    {
        // Clean state management with cycle count support
        // Thread-safe implementation
        // Return true for Primary, false for Secondary
    }
}
```

**Step 2.2: Alternating Action Implementation**
- Create `AlternatingActionConfig` and `AlternatingAction` classes
- Implement clean Primary/Secondary action execution
- Add cycle count support for advanced patterns

**Step 2.3: Integration**
- Add `AlternatingAction` to action type enum and factory
- Update configuration loader with proper JSON deserialization
- Ensure compatibility with all existing action types

### Phase 3: SysEx Pattern Support (1-2 days)

**Step 3.1: SysEx Input Pattern Matching**
- Add `SysEx` to `MidiEventType` and `UnifiedActionMidiInputType` enums
- Extend `MidiEventConverter` to handle NAudio SysEx events
- Implement `SysExPatternMatcher` for byte-array comparison
- Update `UnifiedActionEventProcessor` to process SysEx pattern triggers

**Step 3.2: SysEx Output Actions**
- Implement `SysExOutputConfig` with byte array data storage
- Create `SysExOutputAction` for sending exact byte patterns
- Integrate with MIDI output infrastructure from Phase 1
- Add basic validation (F0 start, F7 end, minimum length)

## Testing Strategy

### MIDI Output Testing
- **Unit Tests:** Command parsing, message validation
- **Integration Tests:** Device enumeration, message sending
- **Hardware Tests:** Real MIDI devices with LED feedback

### Alternating Actions Testing
- **Unit Tests:** State management, multiplier logic
- **Integration Tests:** Action execution alternation
- **User Tests:** Common use cases (play/pause, mode switching)

### SysEx Pattern Testing
- **Unit Tests:** Pattern matching logic, byte array comparison
- **Integration Tests:** SysEx input/output with NAudio
- **Hardware Tests:** Real devices with SysEx capabilities (Launchpad, etc.)
- **Pattern Tests:** Various SysEx message lengths and formats

## Conclusion

These three feature enhancements would significantly strengthen MIDIFlux as a professional MIDI control solution:

1. **MIDI Output Actions** - Essential for device control and feedback
2. **Alternating Actions** - Important for toggle behaviors and state management
3. **SysEx Support** - Valuable for advanced device integration

**Design Philosophy:**
- **Modern, Clean APIs** - Strongly-typed, self-documenting configurations
- **MIDIFlux Native** - Designed to fit MIDIFlux architecture and patterns
- **NAudio Integration** - Leverage existing MIDI infrastructure
- **Extensibility** - Easy to add new MIDI message types and action patterns

**Implementation Benefits:**
- **Type Safety** - Compile-time validation and IDE support
- **Maintainability** - Clean, testable code architecture following MIDIFlux patterns
- **User Experience** - Clear, readable configuration files
- **Performance** - Efficient NAudio-based MIDI handling

**Total Implementation Time:** 5-8 days for complete feature enhancement
**Immediate Priority:** MIDI Output Actions (enables 80% of professional use cases)
**Result:** Professional-grade MIDI control capabilities with modern, maintainable code

## Import Compatibility Impact

### **Before These Features: 70% Import Coverage**
- ❌ MIDI Output commands lost (converted to comments)
- ❌ Alternating actions lost (only first action imported)
- ❌ SysEx patterns lost (unsupported)

### **After These Features: 95%+ Import Coverage**
- ✅ **MIDI Output**: Convert hex strings to structured `MidiOutputCommand` objects
- ✅ **Alternating Actions**: Convert to `AlternatingActionConfig` with Primary/Secondary actions
- ✅ **SysEx Patterns**: Convert hex strings to byte arrays for pattern matching/sending

### **Import Conversion Examples**

**MIDIKey2Key Format → MIDIFlux Format:**
```
// MIDIKey2Key INI
SendMidiCommands=903C7F P100 803C00

// Converts to MIDIFlux JSON
"Commands": [
  {"MessageType": "NoteOn", "Channel": 1, "Data1": 60, "Data2": 127},
  {"MessageType": "NoteOff", "Channel": 1, "Data1": 60, "Data2": 0, "DelayAfterMs": 100}
]
```

**Result:** Complete migration path with minimal feature loss, enabling users to seamlessly transition from MIDIKey2Key to MIDIFlux.
