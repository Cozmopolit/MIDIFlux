# MIDIFlux Unified Action System - Implementation Plan

## Overview

This document provides a step-by-step implementation plan to migrate from the current fragmented action system (3 separate action type enums) to the unified architecture specified in `UnifiedActionSystem_Specification.md`.

**Critical Requirements:**
- **Use existing error handling patterns** - Follow established ApplicationErrorHandler patterns, no new error handling approaches
- **Use existing logging patterns** - Follow established logging patterns throughout the codebase
- **GUI changes come LAST** - Core functionality first, configuration UI is "nice to have" for V1.0
- **No breaking changes to MIDI input processing** - Maintain existing MIDI event flow

## Phase 1: Foundation Infrastructure (Core Types)

### Step 1.1: Create Core Interfaces and Enums
**Files to create:**
- `Core/Actions/IUnifiedAction.cs`
- `Core/Actions/UnifiedActionType.cs`
- `Core/Actions/UnifiedActionMidiInputType.cs`

**Implementation:**
```csharp
// IUnifiedAction.cs - Sync-by-default interface for performance
public interface IUnifiedAction
{
    string Id { get; }
    string Description { get; }
    void Execute(int? midiValue = null);                    // Hot path - no Task overhead
    ValueTask ExecuteAsync(int? midiValue = null) =>        // Async adapter
        new(Execute(midiValue));
}

// UnifiedActionType.cs - Complete enum from specification
// UnifiedActionMidiInputType.cs - Complete enum from specification
```

**Dependencies:** None
**Validation:** Compiles without errors

### Step 1.2: Create Core Data Structures
**Files to create:**
- `Core/Actions/UnifiedActionMidiInput.cs`
- `Core/Actions/UnifiedActionMapping.cs`
- `Core/Actions/Configuration/UnifiedActionConfig.cs` (base class)
- `Core/Actions/Configuration/KeyPressReleaseConfig.cs`
- `Core/Actions/Configuration/MouseClickConfig.cs`
- `Core/Actions/Configuration/DelayConfig.cs`
- `Core/Actions/Configuration/SequenceConfig.cs`
- `Core/Actions/Configuration/ConditionalConfig.cs`
- (Additional config classes for each action type)

**Implementation:**
- **Strongly-typed configuration**: Create POCO classes for each action type
- **No parameter bags**: Eliminate Dictionary<string, object> approach
- Include `GetLookupKey()` method in UnifiedActionMapping for performance
- **Type safety**: All configuration properties are strongly typed
- Use existing logging patterns for any debug output

**Dependencies:** Step 1.1
**Validation:** Unit tests for GetLookupKey() method + configuration type safety

### Step 1.3: Create Registry Infrastructure
**Files to create:**
- `Core/Actions/UnifiedActionMappingRegistry.cs`

**Implementation:**
- **Immutable registry**: Use `volatile IReadOnlyDictionary<string, List<IUnifiedAction>>`
- **Lock-free reads**: No locking in hot path (MIDI event processing)
- **Atomic updates**: `LoadMappings()` method builds new registry and swaps atomically
- **Performance optimized**: Pre-compute lookup keys to minimize string allocation
- Implement 4-step lookup strategy as specified
- Use existing logging patterns for debug/trace output
- Follow existing error handling patterns

**Dependencies:** Step 1.2
**Validation:** Unit tests for registration, lookup, and thread safety

## Phase 2: Action Factory and Simple Actions

### Step 2.1: Create Action Factory
**Files to create:**
- `Core/Actions/IUnifiedActionFactory.cs`
- `Core/Actions/UnifiedActionFactory.cs`

**Implementation:**
- **Strongly-typed factory**: Accept `UnifiedActionConfig` instead of parameter bags
- **Type-safe creation**: Use pattern matching on config types
- **Compile-time validation**: No runtime parameter parsing or conversion
- **JSON schema validation**: Validate configuration structure at load time
- Action-specific validation in constructors using typed properties
- Use existing error handling patterns for invalid actions
- Use existing logging patterns

**Dependencies:** Step 1.3
**Validation:** Unit tests for factory creation, type safety, and error handling

### Step 2.2: Implement Simple Keyboard Actions
**Files to create:**
- `Core/Actions/Simple/KeyPressReleaseAction.cs`
- `Core/Actions/Simple/KeyDownAction.cs`
- `Core/Actions/Simple/KeyUpAction.cs`
- `Core/Actions/Simple/KeyToggleAction.cs`

**Implementation:**
- **Strongly-typed constructors**: Take specific config classes (e.g., `KeyPressReleaseConfig`)
- **Sync-by-default**: Implement `Execute()` method, not `ExecuteAsync()`
- **Zero allocation**: No Task creation for simple synchronous actions
- **Type-safe validation**: Use strongly-typed properties from config
- Use existing keyboard simulation code
- Use existing error handling patterns
- Use existing logging patterns

**Dependencies:** Step 2.1
**Validation:** Unit tests + manual testing with MIDI input + performance validation

### Step 2.3: Implement Simple Mouse Actions
**Files to create:**
- `Core/Actions/Simple/MouseClickAction.cs`
- `Core/Actions/Simple/MouseScrollAction.cs`

**Implementation:**
- **Strongly-typed constructors**: Take `MouseClickConfig`, `MouseScrollConfig`
- **Sync-by-default**: Implement `Execute()` method for zero allocation
- Use existing mouse simulation code (MouseScrollAction already exists)
- Implement MouseClickAction using existing patterns
- **Type-safe validation**: Use strongly-typed properties from config
- Use existing error handling and logging patterns

**Dependencies:** Step 2.2
**Validation:** Unit tests + manual testing + performance validation

### Step 2.4: Implement System Actions
**Files to create:**
- `Core/Actions/Simple/CommandExecutionAction.cs`
- `Core/Actions/Simple/DelayAction.cs`

**Implementation:**
- **Strongly-typed constructors**: Take `CommandExecutionConfig`, `DelayConfig`
- **Mixed async behavior**:
  - DelayAction: Override `ExecuteAsync()` for true async with `await Task.Delay()`
  - CommandExecution: May be truly async depending on implementation
- **Type-safe validation**: Use strongly-typed properties from config
- Use existing command execution code
- Use existing delay/timing patterns
- Use existing error handling and logging patterns

**Dependencies:** Step 2.3
**Validation:** Unit tests + manual testing + async behavior validation

### Step 2.5: Implement Game Controller Actions (Optional for V1.0)
**Files to create:**
- `Core/Actions/Simple/GameControllerButtonAction.cs`
- `Core/Actions/Simple/GameControllerAxisAction.cs`

**Implementation:**
- Use existing ViGEm integration
- Parameter validation in constructors
- Use existing error handling and logging patterns

**Dependencies:** Step 2.4
**Note:** Can be deferred to post-V1.0
**Validation:** Unit tests + manual testing with game controller

## Phase 3: Complex Actions

### Step 3.1: Implement SequenceAction
**Files to create:**
- `Core/Actions/Complex/SequenceAction.cs`
- `Core/Actions/Complex/SequenceErrorHandling.cs`

**Implementation:**
- **Strongly-typed constructor**: Take `SequenceConfig` and `IUnifiedActionFactory`
- **Override ExecuteAsync()**: Complex actions need true async behavior
- **Type-safe configuration**: Use `SequenceConfig.SubActions` list
- Recursive action creation for sub-actions
- Configurable error handling (StopOnError/ContinueOnError)
- **Error Aggregation**: Collect all exceptions with step index and action description
- **Detailed Diagnostics**: Include failing action index and description in exceptions
- Use existing error handling and logging patterns

**Dependencies:** Step 2.5
**Validation:** Unit tests for Ctrl+C sequence + manual testing + error handling scenarios

### Step 3.2: Implement ConditionalAction
**Files to create:**
- `Core/Actions/Complex/ConditionalAction.cs`
- `Core/Actions/Complex/ValueCondition.cs`

**Implementation:**
- **Strongly-typed constructor**: Take `ConditionalConfig` and `IUnifiedActionFactory`
- **Type-safe configuration**: Use `ConditionalConfig.Conditions` list
- **Override ExecuteAsync()**: May need async behavior for sub-actions
- Value range checking logic using typed `ValueConditionConfig`
- First-match-wins execution
- Use existing error handling and logging patterns

**Dependencies:** Step 3.1
**Validation:** Unit tests for fader-to-buttons scenario + manual testing

## Phase 4: Configuration System Integration

### Step 4.1: Create Configuration Loading
**Files to create:**
- `Configuration/UnifiedActionConfigurationLoader.cs`

**Implementation:**
- **Strongly-typed JSON deserialization**: Deserialize directly to `UnifiedActionConfig` POCOs
- **JSON schema validation**: Validate structure at load time, not runtime
- **Type-safe configuration**: No parameter bags or runtime conversion
- **Immutable registry loading**: Use `LoadMappings()` for atomic registry updates
- Integration with existing DeviceConfigurationManager patterns
- Graceful error handling for invalid mappings
- Use existing error handling patterns (ApplicationErrorHandler)
- Use existing logging patterns

**Dependencies:** Step 3.2
**Validation:** Load test configurations, verify error handling, validate type safety

### Step 4.2: Create Configuration Saving
**Files to modify:**
- Extend `Configuration/UnifiedActionConfigurationLoader.cs`

**Implementation:**
- **Strongly-typed JSON serialization**: Serialize from `UnifiedActionConfig` POCOs
- **Type-safe round-trip**: Ensure serialization/deserialization consistency
- Maintain existing configuration file patterns
- Use existing error handling and logging patterns

**Dependencies:** Step 4.1
**Validation:** Save/load round-trip testing with type safety validation

### Step 4.3: Integration with Profile Management
**Files to modify:**
- `Configuration/DeviceConfigurationManager.cs` (extend existing)

**Implementation:**
- **Atomic registry updates**: Use `LoadMappings()` for thread-safe profile loading
- **Strongly-typed configuration**: Load typed configs and create actions
- Add unified action loading alongside existing systems
- Maintain backward compatibility during transition
- Use existing error handling patterns
- Use existing logging patterns

**Dependencies:** Step 4.2
**Validation:** Profile loading with both old and new systems + thread safety validation

## Phase 5: MIDI Event Processing Integration

### Step 5.1: Create Unified Event Processor
**Files to create:**
- `Core/Processing/UnifiedActionEventProcessor.cs`

**Implementation:**
- **Lock-free registry access**: Use immutable registry for zero-lock reads
- **Performance optimized**: Pre-compute lookup keys to minimize allocations
- **Sync-by-default execution**: Call `Execute()` for simple actions, `ExecuteAsync()` for complex
- Integration with existing MIDI event flow
- Registry lookup and action execution
- Performance logging and monitoring
- Use existing error handling patterns
- Use existing logging patterns (comprehensive logging as specified)

**Dependencies:** Step 4.3
**Validation:** End-to-end MIDI input to action execution + performance validation

### Step 5.2: Integration with Existing MIDI Handlers
**Files to modify:**
- Existing MIDI event handler classes (identify during implementation)

**Implementation:**
- Route MIDI events to UnifiedActionEventProcessor
- Maintain existing event processing patterns
- Gradual migration approach - both systems can coexist
- Use existing error handling and logging patterns

**Dependencies:** Step 5.1
**Validation:** Full MIDI input processing with unified actions

### Step 5.3: Create Sample Configuration for Testing
**Files to create:**
- `TestData/sample_unified_profile.json`

**Implementation:**
- Manually create a sample profile using the new unified JSON format
- Include examples of all action types:
  - Simple keyboard actions (KeyPressRelease, KeyDown, KeyUp)
  - Mouse actions (MouseClick, MouseScroll)
  - System actions (CommandExecution, Delay)
  - Complex actions (SequenceAction for Ctrl+C, ConditionalAction for fader-to-buttons)
- Use realistic MIDI input mappings for testing
- Follow the exact JSON structure from the specification
- Include both exact device mappings and wildcard mappings

**Dependencies:** Step 5.2
**Validation:** Configuration loads successfully, all actions can be executed

## Phase 6: Testing and Validation

### Step 6.1: Comprehensive Unit Testing
**Files to create:**
- Unit tests for all new classes
- Integration tests for end-to-end scenarios

**Implementation:**
- Test all action types individually
- Test complex action scenarios (Ctrl+C, fader-to-buttons)
- Test error handling scenarios
- Test performance with typical mapping counts (<30)

**Dependencies:** Step 5.3
**Validation:** All tests pass, coverage >80%

### Step 6.2: Performance Validation
**Implementation:**
- Benchmark MIDI event processing latency
- Validate <5ms target for typical scenarios
- Memory usage validation
- Profile loading time validation

**Dependencies:** Step 6.1
**Validation:** Performance targets met

### Step 6.3: Manual Testing and Bug Fixes
**Implementation:**
- Real-world testing with MIDI controllers
- Test all action types with actual hardware
- Test complex scenarios and edge cases
- Bug fixes and refinements

**Dependencies:** Step 6.2
**Validation:** Stable, reliable operation

## Phase 7: Legacy System Migration (Optional for V1.0)

### Step 7.1: Deprecate Old Action Types
**Files to modify:**
- Mark old enums as obsolete
- Add migration warnings
- Update documentation

**Implementation:**
- Add [Obsolete] attributes to old action type enums
- Provide migration guidance
- Maintain functionality during transition

**Dependencies:** Step 6.3
**Validation:** Compilation warnings for old usage

### Step 7.2: Remove Legacy Code (Post-V1.0)
**Implementation:**
- Remove old ActionType, KeyActionType, CCRangeActionType enums
- Remove old mapping classes
- Clean up unused code
- Rename "UnifiedAction*" to "Action*"

**Dependencies:** Step 7.1
**Note:** Post-V1.0 task
**Validation:** Clean codebase, no legacy references

## Phase 8: GUI Integration (LAST - Optional for V1.0)

### Step 8.1: Basic Configuration Support
**Files to modify:**
- Existing profile editor dialogs (if time permits)

**Implementation:**
- Basic support for creating unified action mappings
- Simple action type selection
- Basic parameter configuration

**Dependencies:** Step 6.3
**Note:** Can be deferred to post-V1.0
**Validation:** Can create and edit basic mappings

### Step 8.2: Complex Action Dialogs (Post-V1.0)
**Implementation:**
- Separate dialogs for SequenceAction and ConditionalAction
- Advanced configuration options
- User-friendly templates (Ctrl+C, etc.)

**Dependencies:** Step 8.1
**Note:** Post-V1.0 task
**Validation:** Full GUI configuration capability

## Success Criteria for V1.0

1. **Core Functionality**: All simple and complex actions implemented and working
2. **Configuration**: Can load/save unified action configurations
3. **Performance**: <5ms MIDI event processing latency
4. **Reliability**: Stable operation with comprehensive error handling
5. **Testing**: Comprehensive test coverage and validation
6. **Documentation**: Updated specification and implementation docs

**GUI configuration is explicitly NOT required for V1.0 success.**

## Implementation Approach

**Focus on incremental progress with validation at each step. The team's collaborative approach consistently delivers faster results than individual time estimates suggest.**

**V1.0 Core**: Phases 1-6 (Foundation through Testing)
**Post-V1.0**: Phases 7-8 (Legacy cleanup and GUI enhancement)
