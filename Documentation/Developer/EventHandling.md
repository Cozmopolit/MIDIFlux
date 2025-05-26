# MIDIFlux Event Handling Architecture

This document describes the event handling architecture in MIDIFlux, including MIDI input processing, action execution, and the unified action system integration.

## Overview

MIDIFlux uses a direct call approach for handling MIDI events with the unified action system. The flow is:

1. **MIDI Hardware** → **NAudio Abstraction Layer** → **MidiManager**
2. **MidiManager** → **EventDispatcher** (direct call)
3. **EventDispatcher** → **Unified Action System** → **Action Execution**

## Core Components

### IMidiHardwareAdapter / NAudioMidiAdapter

The abstraction layer responsible for:

- **Device Management**: Enumerating and connecting to MIDI devices
- **Event Reception**: Receiving raw MIDI messages from hardware
- **Channel Normalization**: Converting to consistent 1-based channels (1-16)
- **Error Handling**: Graceful handling of device disconnections and errors

### MidiManager

The `MidiManager` class coordinates MIDI operations:

- **Device Lifecycle**: Starting/stopping MIDI input and output devices
- **Event Conversion**: Converting NAudio events to internal `MidiEvent` format
- **Event Dispatching**: Direct calls to `EventDispatcher.HandleMidiEvent()`
- **Device Monitoring**: Hot-plug support and device status tracking

### EventDispatcher

The `EventDispatcher` class processes MIDI events:

- **Configuration Management**: Maintaining current profile and mappings
- **Event Matching**: O(1) lookup of actions based on MIDI events
- **Action Execution**: Executing matched actions through unified action system
- **State Management**: Coordinating with `ActionStateManager` for stateful actions

### Unified Action System

The action execution layer:

- **Action Factory**: Creating strongly-typed actions from configuration
- **Simple Actions**: Direct execution for performance (KeyPress, MouseClick, etc.)
- **Complex Actions**: Orchestration logic (Sequences, Conditionals, State management)
- **State Management**: `ActionStateManager` for stateful behaviors

## Event Flow Detail

### 1. MIDI Input Processing

```
Hardware Device → NAudio → IMidiHardwareAdapter → MidiManager
```

1. **Hardware generates MIDI message** (Note On, CC, etc.)
2. **NAudio receives raw MIDI data** and raises `MessageReceived` event
3. **NAudioMidiAdapter processes event**:
   - Converts channel from 0-based to 1-based
   - Creates internal `MidiEvent` object
   - Raises `MidiEventReceived` event
4. **MidiManager receives event** and forwards to EventDispatcher

### 2. Event Matching and Lookup

```
EventDispatcher → Pre-computed Mapping Dictionary → Action Lookup
```

1. **EventDispatcher receives MidiEvent** from MidiManager
2. **Generates lookup key** from event properties:
   ```csharp
   string key = $"{deviceName}|{inputType}|{channel}|{noteOrCC}";
   ```
3. **O(1) dictionary lookup** to find matching action
4. **Action execution** if match found

### 3. Action Execution

```
Action → ExecuteAsync(midiValue) → Target System (Keyboard/Mouse/Controller/etc.)
```

1. **Action.ExecuteAsync(midiValue)** called with MIDI value (0-127)
2. **Simple Actions**: Async execution with ValueTask for performance (KeyPress, MouseClick, etc.)
3. **Complex Actions**: Async orchestration logic:
   - **SequenceAction**: Execute sub-actions in order with proper async/await
   - **ConditionalAction**: Execute based on MIDI value comparison
   - **StateConditionalAction**: Execute based on state values
   - **AlternatingAction**: Toggle between two actions with async execution

## Performance Optimizations

### Pre-computed Lookup Keys

Actions are indexed by pre-computed keys for O(1) lookup:

```csharp
// Example lookup keys
"*|NoteOn|1|60"           // Any device, Note On, Channel 1, Note 60
"Launchpad|ControlChange|2|7"  // Specific device, CC, Channel 2, CC 7
```

### Unified Async Execution Model

- **Simple Actions**: Execute asynchronously with ValueTask for minimal overhead
- **Complex Actions**: Orchestrate asynchronously with proper async/await patterns
- **State Operations**: Lock-free concurrent dictionary operations
- **Hot Path Optimization**: Minimal allocations in event processing with async execution

### Memory Management

- **Action Pre-compilation**: Actions created at profile load time
- **Minimal Allocations**: Reuse objects in hot paths
- **State Cleanup**: Automatic cleanup on profile changes
- **Event Object Reuse**: Minimize garbage collection pressure

## Error Handling

### Graceful Degradation

- **Device Disconnection**: Continue with remaining devices
- **Action Failures**: Log errors but continue processing other events
- **Configuration Errors**: Validate at load time, not runtime
- **State Errors**: Default to safe values, continue execution

### Error Propagation

```
Hardware Error → Adapter → MidiManager → EventDispatcher → Logging
```

- **Hardware errors** are caught at adapter level
- **Action errors** are caught at EventDispatcher level
- **All errors** are logged with context for debugging
- **Application continues** running despite individual failures

## State Management Integration

### ActionStateManager

- **Thread-safe**: Concurrent access from MIDI processing threads
- **Profile-scoped**: States cleared on profile changes
- **Dual state types**: User-defined and internal (keyboard tracking)
- **Memory efficient**: On-demand state creation

### State Lifecycle

1. **Profile Load**: Initialize states from `InitialStates` configuration
2. **Action Execution**: Read/write states during action execution
3. **Profile Change**: Clear all states and release held keys
4. **Application Shutdown**: Clean shutdown with state cleanup

## Debugging and Monitoring

### Logging Integration

- **MIDI Events**: Debug-level logging of all received events
- **Action Execution**: Info-level logging of action execution
- **Errors**: Error-level logging with full context and stack traces
- **Performance**: Optional performance metrics logging

### Event Tracing

```
[DEBUG] MIDI Event: Device=Launchpad, Type=NoteOn, Channel=1, Note=60, Velocity=127
[INFO]  Action Executed: KeyPressRelease(VirtualKeyCode=65) - Duration: 2ms
[ERROR] Action Failed: CommandExecution failed - Command not found
```

## Configuration Integration

### Profile Loading

1. **JSON Parsing**: Parse profile configuration file
2. **Validation**: Validate all mappings and action configurations
3. **Action Creation**: Pre-compile all actions using ActionFactory
4. **Mapping Registration**: Build lookup dictionary for O(1) access
5. **State Initialization**: Initialize user-defined states

### Hot Configuration Reload

- **Profile switching** without application restart
- **Device reconnection** on configuration change
- **State cleanup** and re-initialization
- **Action re-compilation** for new mappings

## Future Enhancements

### Planned Improvements

1. **Event Batching**: Batch multiple MIDI events for efficiency
2. **Priority Queues**: Priority-based action execution
3. **Event Filtering**: Pre-filter events before processing
4. **Performance Metrics**: Built-in performance monitoring
5. **Event Recording**: Record and replay MIDI events for testing

### Scalability Considerations

- **Multiple Devices**: Current architecture supports unlimited devices
- **High Event Rates**: Optimized for real-time performance
- **Complex Actions**: Efficient orchestration of complex workflows
- **Memory Usage**: Bounded memory usage with automatic cleanup

