# MIDIFlux Unified Action System Specification

## Overview

This document specifies the complete redesign of MIDIFlux's action and mapping system to eliminate the current fragmented architecture (3 separate action type enums) and create a unified, extensible foundation for V1.0.

**Key Design Principles:**
- **No backward compatibility** - Clean slate for V1.0
- **Unified naming** - All classes, enums, file names ... use "UnifiedAction" prefix (to be renamed to "Action" after legacy cleanup)
- **Clear boundaries** - Distinct separation between mapping (MIDI input → action trigger) and action (what gets executed)
- **No parallel execution** - Sequential and conditional execution supported in action layer
- **Extensible design** - Support complex use cases like fader-to-multiple-buttons, macro sequences

## Architecture Overview

**Clean Separation: Simple Mapping Layer + Complex Action Layer**

```
MIDI Input → UnifiedActionMapping → IUnifiedAction → Execution
     ↓              ↓                      ↓              ↓
  Device/Note    Simple Lookup        Action Interface   Simple/Complex Actions
  Channel/CC     (O(1) fast)          (Uniform)         (Hot Path/Orchestration)
```

### Design Principles
- **Mapping Layer**: Simple, fast, uniform - only handles WHEN to execute
- **Action Layer**: All complexity lives here - handles WHAT and HOW to execute
  - **Value Transformation**: Actions receive raw MIDI values and transform them as needed
  - **Behavioral Logic**: Actions handle their own timing, release behavior, etc.
- **Hot Path**: Simple actions execute directly with minimal overhead
- **Orchestration**: Complex actions coordinate multiple actions with logic
- **Unrestricted Composition**: Actions can call other actions without restrictions

## Core Components

### 1. Unified Action Interface

**Purpose:** Single interface for all actions - simple and complex. Encapsulates all complexity.

```csharp
public interface IUnifiedAction
{
    string Id { get; }
    string Description { get; }

    // Sync by default for performance - most actions are synchronous
    void Execute(int? midiValue = null);

    // Async adapter - complex actions can override for true async behavior
    ValueTask ExecuteAsync(int? midiValue = null) => new(Execute(midiValue));
}
```

### 2. UnifiedActionType Enum

**Purpose:** Identifies action types for simple actions and complex action categories.

```csharp
public enum UnifiedActionType
{
    // Simple Actions (Hot Path)
    KeyPressRelease,       // Press and release a key
    KeyDown,               // Press and hold a key
    KeyUp,                 // Release a key
    KeyToggle,             // Toggle key state (like CapsLock)
    MouseClick,            // Click mouse button (Left/Right/Middle)
    MouseScroll,           // Scroll wheel (Up/Down/Left/Right)
    CommandExecution,      // Execute shell command
    Delay,                 // Wait for specified time
    GameControllerButton,  // Press game controller button
    GameControllerAxis,    // Set game controller axis value

    // Complex Actions (Orchestration)
    SequenceAction,        // Execute actions sequentially (macros)
    ConditionalAction,     // Execute actions based on MIDI value (fader-to-buttons)

    // POST-V1.0: ValueTransformAction - Transform MIDI value then execute action

    // Future extensibility
    MidiOutput,
    AudioControl,
    SystemIntegration

    // REMOVED: MouseMove (too complex for V1.0)
    // REMOVED: MouseDown/MouseUp (use MouseClick instead)
}
```

### 3. Simplified Mapping Structure

**Purpose:** Fast, simple MIDI input → action trigger. No complexity here.

```csharp
public class UnifiedActionMapping
{
    // MIDI Input specification (WHEN to trigger)
    public UnifiedActionMidiInput Input { get; set; } = new();

    // Action to execute (WHAT to execute - simple or complex)
    public IUnifiedAction Action { get; set; }

    // Optional description
    public string? Description { get; set; }

    // Computed lookup key for O(1) registry performance
    public string GetLookupKey()
    {
        return $"{Input.DeviceName ?? "*"}|{Input.Channel?.ToString() ?? "*"}|{Input.InputNumber}|{Input.InputType}";
    }

    // NO IsEnabled - disabled mappings shouldn't be in registry
    // NO IgnoreNoteOff - this is action behavior, not mapping behavior
    // NO AutoReleaseAfterMs - this is action behavior, not mapping behavior
    // NO ValueRanges - complexity moved to ConditionalAction
    // NO ValueMapping - complexity moved to ValueTransformAction
}
```

### 4. UnifiedActionMidiInput Class

**Purpose:** Specifies what MIDI input triggers this mapping.

```csharp
public class UnifiedActionMidiInput
{
    // Device specification
    public string? DeviceName { get; set; }  // null = any device
    public int? DeviceId { get; set; }       // null = any device

    // Channel specification
    public int? Channel { get; set; }        // null = any channel (simplified from List)

    // Input type and number
    public UnifiedActionMidiInputType InputType { get; set; }
    public int InputNumber { get; set; } // Note number or Controller number

    // Value constraints (optional - for future use)
    public int? MinValue { get; set; }
    public int? MaxValue { get; set; }
}

public enum UnifiedActionMidiInputType
{
    NoteOn,
    NoteOff,
    ControlChange,
    RelativeControlChange

    // NO NoteOnOff - complicates lookup logic
    // If action needs both, create two separate mappings
}
```

### 5. Complex Action Types

**Purpose:** Handle orchestration and logic - all complexity lives here.

#### SequenceAction (Macros)
```csharp
public class SequenceAction : IUnifiedAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = "";
    public List<IUnifiedAction> Actions { get; set; } = new();

    public async Task ExecuteAsync(int? midiValue = null)
    {
        // Execute actions sequentially
        foreach (var action in Actions)
        {
            await action.ExecuteAsync(midiValue);
        }
    }

    public List<IUnifiedAction> GetChildActions() => Actions;
    public bool ContainsAction(string actionId) => Actions.Any(a => a.Id == actionId);
}
```

#### ConditionalAction (Fader-to-Buttons)
```csharp
public class ConditionalAction : IUnifiedAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = "";
    public List<ValueCondition> Conditions { get; set; } = new();

    public async Task ExecuteAsync(int? midiValue = null)
    {
        if (!midiValue.HasValue) return;

        // Execute first matching condition only
        foreach (var condition in Conditions)
        {
            if (midiValue >= condition.MinValue && midiValue <= condition.MaxValue)
            {
                await condition.Action.ExecuteAsync(midiValue);
                break;
            }
        }
    }

    public List<IUnifiedAction> GetChildActions() => Conditions.Select(c => c.Action).ToList();
    public bool ContainsAction(string actionId) => Conditions.Any(c => c.Action.Id == actionId);
}

public class ValueCondition
{
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public IUnifiedAction Action { get; set; }
    public string? Description { get; set; }
}
```

#### ValueTransformAction (Value Mapping)
```csharp
public class ValueTransformAction : IUnifiedAction
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Description { get; set; } = "";
    public IUnifiedAction TargetAction { get; set; }
    public ValueTransformation Transformation { get; set; } = new();

    public async Task ExecuteAsync(int? midiValue = null)
    {
        if (!midiValue.HasValue)
        {
            await TargetAction.ExecuteAsync(null);
            return;
        }

        // Transform the MIDI value
        var transformedValue = Transformation.Transform(midiValue.Value);
        await TargetAction.ExecuteAsync(transformedValue);
    }

    public List<IUnifiedAction> GetChildActions() => new() { TargetAction };
    public bool ContainsAction(string actionId) => TargetAction.Id == actionId;
}

public class ValueTransformation
{
    public int InputMinValue { get; set; } = 0;
    public int InputMaxValue { get; set; } = 127;
    public int OutputMinValue { get; set; } = 0;
    public int OutputMaxValue { get; set; } = 127;
    public bool InvertMapping { get; set; } = false;
    public ValueTransformationType TransformationType { get; set; } = ValueTransformationType.Linear;

    public int Transform(int inputValue)
    {
        // Clamp input to range
        inputValue = Math.Max(InputMinValue, Math.Min(InputMaxValue, inputValue));

        // Normalize to 0-1
        var inputRange = InputMaxValue - InputMinValue;
        if (inputRange == 0) return OutputMinValue;

        var normalizedInput = (double)(inputValue - InputMinValue) / inputRange;
        if (InvertMapping) normalizedInput = 1.0 - normalizedInput;

        // Apply transformation
        var transformedValue = TransformationType switch
        {
            ValueTransformationType.Linear => normalizedInput,
            ValueTransformationType.Logarithmic => Math.Log10(1 + 9 * normalizedInput),
            ValueTransformationType.Stepped => Math.Floor(normalizedInput * 10) / 10,
            _ => normalizedInput
        };

        // Scale to output range
        var outputRange = OutputMaxValue - OutputMinValue;
        return (int)(OutputMinValue + (transformedValue * outputRange));
    }
}

public enum ValueTransformationType
{
    Linear,
    Logarithmic,
    Stepped
}
```

## Complex Use Case Support

### Use Case 1: Fader Controls Multiple Buttons Based on Value

**Scenario:** CC7 fader controls opacity - different ranges trigger different number keys (1-0).

**New Implementation (Clean Architecture):**
```csharp
// Simple mapping - no complexity here
var mapping = new UnifiedActionMapping
{
    Input = new UnifiedActionMidiInput
    {
        InputType = UnifiedActionMidiInputType.ControlChange,
        InputNumber = 7
    },
    Action = new ConditionalAction
    {
        Description = "Opacity control using keys 1-0",
        Conditions = new List<ValueCondition>
        {
            new() {
                MinValue = 0, MaxValue = 12,
                Action = new KeyPressReleaseAction { VirtualKeyCode = 49 } // '1' key
            },
            new() {
                MinValue = 13, MaxValue = 25,
                Action = new KeyPressReleaseAction { VirtualKeyCode = 50 } // '2' key
            }
            // ... more ranges
        }
    }
};
```

### Use Case 2: Complex Macro Sequence

**Scenario:** Single note triggers a sequence of actions with delays.

**New Implementation (Clean Architecture):**
```csharp
// Simple mapping - no complexity here
var mapping = new UnifiedActionMapping
{
    Input = new UnifiedActionMidiInput
    {
        InputType = UnifiedActionMidiInputType.NoteOn,
        InputNumber = 60
    },
    Action = new SequenceAction
    {
        Description = "Complex macro sequence",
        Actions = new List<IUnifiedAction>
        {
            new KeyPressReleaseAction { VirtualKeyCode = 65 }, // 'A' key
            new DelayAction { Milliseconds = 100 },
            new KeyPressReleaseAction { VirtualKeyCode = 66 }  // 'B' key
        }
    }
};

// If you need both NoteOn and NoteOff behavior, create two mappings:
var noteOffMapping = new UnifiedActionMapping
{
    Input = new UnifiedActionMidiInput
    {
        InputType = UnifiedActionMidiInputType.NoteOff,
        InputNumber = 60
    },
    Action = new KeyUpAction { VirtualKeyCode = 65 } // Release 'A' key on note off
};
```

### Use Case 3: Value Passthrough with Transformation

**Scenario:** CC1 modulation wheel controls mouse X position with scaling.

**New Implementation (Clean Architecture):**
```csharp
// Simple mapping - no complexity here
var mapping = new UnifiedActionMapping
{
    Input = new UnifiedActionMidiInput
    {
        InputType = UnifiedActionMidiInputType.ControlChange,
        InputNumber = 1
    },
    Action = new ValueTransformAction
    {
        Description = "CC1 to mouse X with scaling",
        Transformation = new ValueTransformation
        {
            InputMinValue = 0,
            InputMaxValue = 127,
            OutputMinValue = 0,
            OutputMaxValue = 1920, // Screen width
            TransformationType = ValueTransformationType.Linear
        },
        TargetAction = new MouseMoveAction
        {
            MouseX = 0, // Will be overridden by transformed value
            MouseY = 540 // Fixed Y position
        }
    }
};
```

### Use Case 4: Nested Complex Actions

**Scenario:** Fader controls different macro sequences based on value ranges.

**New Implementation (Unrestricted Composition):**
```csharp
var mapping = new UnifiedActionMapping
{
    Input = new UnifiedActionMidiInput
    {
        InputType = UnifiedActionMidiInputType.ControlChange,
        InputNumber = 7
    },
    Action = new ConditionalAction
    {
        Description = "Fader controls different macros",
        Conditions = new List<ValueCondition>
        {
            new() {
                MinValue = 0, MaxValue = 63,
                Action = new SequenceAction
                {
                    Description = "Low value macro",
                    Actions = new List<IUnifiedAction>
                    {
                        new KeyPressReleaseAction { VirtualKeyCode = 65 }, // 'A'
                        new DelayAction { Milliseconds = 50 },
                        new KeyPressReleaseAction { VirtualKeyCode = 66 }  // 'B'
                    }
                }
            },
            new() {
                MinValue = 64, MaxValue = 127,
                Action = new SequenceAction
                {
                    Description = "High value macro",
                    Actions = new List<IUnifiedAction>
                    {
                        new KeyPressReleaseAction { VirtualKeyCode = 88 }, // 'X'
                        new DelayAction { Milliseconds = 100 },
                        new KeyPressReleaseAction { VirtualKeyCode = 89 }  // 'Y'
                    }
                }
            }
        }
    }
};
```

## Unified Action Instantiation System

### Strongly-Typed Action Configuration

**Purpose:** Type-safe action configuration with compile-time validation.

```csharp
// Base configuration for all actions
public abstract class UnifiedActionConfig
{
    public UnifiedActionType Type { get; set; }
    public string? Description { get; set; }
}

// Strongly-typed configurations for each action type
public class KeyPressReleaseConfig : UnifiedActionConfig
{
    public ushort VirtualKeyCode { get; set; }
}

public class KeyDownConfig : UnifiedActionConfig
{
    public ushort VirtualKeyCode { get; set; }
    public int? AutoReleaseAfterMs { get; set; }
}

public class MouseClickConfig : UnifiedActionConfig
{
    public MouseButton Button { get; set; }
}

public class MouseScrollConfig : UnifiedActionConfig
{
    public ScrollDirection Direction { get; set; }
    public int Amount { get; set; } = 1;
}

public class CommandExecutionConfig : UnifiedActionConfig
{
    public string Command { get; set; } = "";
    public CommandShellType ShellType { get; set; } = CommandShellType.PowerShell;
}

public class DelayConfig : UnifiedActionConfig
{
    public int Milliseconds { get; set; }
}

public class SequenceConfig : UnifiedActionConfig
{
    public SequenceErrorHandling ErrorHandling { get; set; } = SequenceErrorHandling.ContinueOnError;
    public List<UnifiedActionConfig> SubActions { get; set; } = new();
}

public class ConditionalConfig : UnifiedActionConfig
{
    public List<ValueConditionConfig> Conditions { get; set; } = new();
}

public class ValueConditionConfig
{
    public int MinValue { get; set; }
    public int MaxValue { get; set; }
    public UnifiedActionConfig Action { get; set; } = new KeyPressReleaseConfig();
    public string? Description { get; set; }
}

// Enums for configuration
public enum MouseButton { Left, Right, Middle }
public enum ScrollDirection { Up, Down, Left, Right }
public enum CommandShellType { PowerShell, CMD }
```

### Unified Action Factory

**Purpose:** Single point for creating all actions from strongly-typed configuration.

```csharp
public interface IUnifiedActionFactory
{
    IUnifiedAction CreateAction(UnifiedActionConfig config);
}

public class UnifiedActionFactory : IUnifiedActionFactory
{
    private readonly ILogger _logger;

    public UnifiedActionFactory(ILogger logger)
    {
        _logger = logger;
    }

    public IUnifiedAction CreateAction(UnifiedActionConfig config)
    {
        try
        {
            // Type-safe creation - no runtime parameter parsing needed
            return config switch
            {
                KeyPressReleaseConfig keyConfig => new KeyPressReleaseAction(keyConfig),
                KeyDownConfig keyDownConfig => new KeyDownAction(keyDownConfig),
                KeyUpConfig keyUpConfig => new KeyUpAction(keyUpConfig),
                KeyToggleConfig keyToggleConfig => new KeyToggleAction(keyToggleConfig),
                MouseClickConfig mouseClickConfig => new MouseClickAction(mouseClickConfig),
                MouseScrollConfig mouseScrollConfig => new MouseScrollAction(mouseScrollConfig),
                CommandExecutionConfig cmdConfig => new CommandExecutionAction(cmdConfig),
                DelayConfig delayConfig => new DelayAction(delayConfig),

                // Complex actions
                SequenceConfig seqConfig => new SequenceAction(seqConfig, this),
                ConditionalConfig condConfig => new ConditionalAction(condConfig, this),

                _ => throw new NotSupportedException($"Action config type {config.GetType().Name} not supported")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create action of type {Type}: {Error}", config.GetType().Name, ex.Message);
            throw;
        }
    }
}
```

### Action Constructor Pattern

**Purpose:** Consistent constructor pattern using strongly-typed configuration.

```csharp
// Simple Action Example
public class KeyPressReleaseAction : IUnifiedAction
{
    public string Id { get; private set; }
    public string Description { get; private set; }
    private readonly ushort _virtualKeyCode;

    public KeyPressReleaseAction(KeyPressReleaseConfig config)
    {
        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? "Key Press/Release";

        // Type-safe validation - no runtime parsing needed
        if (config.VirtualKeyCode == 0)
            throw new ArgumentException("VirtualKeyCode is required and must be > 0");

        _virtualKeyCode = config.VirtualKeyCode;
    }

    public void Execute(int? midiValue = null)
    {
        // Synchronous implementation - no Task overhead
        KeyboardSimulator.SendKeyPress(_virtualKeyCode);
    }
}

// Complex Action Example
public class SequenceAction : IUnifiedAction
{
    public string Id { get; private set; }
    public string Description { get; private set; }
    public List<IUnifiedAction> Actions { get; private set; }
    public SequenceErrorHandling ErrorHandling { get; private set; }

    public SequenceAction(SequenceConfig config, IUnifiedActionFactory factory)
    {
        Id = Guid.NewGuid().ToString();
        Description = config.Description ?? "Sequence Action";
        ErrorHandling = config.ErrorHandling;

        // Create sub-actions recursively
        Actions = new List<IUnifiedAction>();
        foreach (var subActionConfig in config.SubActions)
        {
            try
            {
                var subAction = factory.CreateAction(subActionConfig);
                Actions.Add(subAction);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to create sub-action: {ex.Message}", ex);
            }
        }

        if (Actions.Count == 0)
            throw new ArgumentException("SequenceAction requires at least one sub-action");
    }

    // Override async for true async behavior
    public override async ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Complex actions may need true async behavior
        var exceptions = new List<Exception>();

        for (int i = 0; i < Actions.Count; i++)
        {
            try
            {
                await Actions[i].ExecuteAsync(midiValue);
            }
            catch (Exception ex)
            {
                var indexedException = new InvalidOperationException($"Action {i} ({Actions[i].Description}) failed: {ex.Message}", ex);
                exceptions.Add(indexedException);

                if (ErrorHandling == SequenceErrorHandling.StopOnError)
                {
                    if (exceptions.Count == 1)
                        throw indexedException;
                    else
                        throw new AggregateException($"SequenceAction failed at step {i}", exceptions);
                }
            }
        }
    }




}

public enum SequenceErrorHandling
{
    ContinueOnError,  // Continue executing remaining actions if one fails
    StopOnError       // Stop sequence execution if any action fails
}
```

### Standard Action Parameters

#### Keyboard Actions (Simplified)
- `VirtualKeyCode` (ushort): The key to press/release/hold

**Note:** No `Modifiers` parameter - use SequenceAction for key combinations:
```json
// Ctrl+C example
{
  "type": "SequenceAction",
  "description": "Copy to clipboard (Ctrl+C)",
  "subActions": [
    {
      "type": "KeyDown",
      "description": "Press Ctrl key",
      "parameters": { "VirtualKeyCode": 17 }
    },
    {
      "type": "KeyPressRelease",
      "description": "Press and release C key",
      "parameters": { "VirtualKeyCode": 67 }
    },
    {
      "type": "KeyUp",
      "description": "Release Ctrl key",
      "parameters": { "VirtualKeyCode": 17 }
    }
  ]
}
```

#### Mouse Actions (Limited Scope)
- `MouseButton` (MouseButton): Left, Right, Middle (for MouseClick actions)
- `ScrollDirection` (ScrollDirection): Up, Down, Left, Right
- `ScrollAmount` (int): Number of scroll steps

**Note:** No MouseMove - too complex for V1.0. Focus on MouseClick and MouseScroll.

#### Game Controller Actions
- `Button` (string): Button name (A, B, X, Y, etc.)
- `AxisValue` (float): Axis value (-1.0 to 1.0)
- `ControllerIndex` (int): Controller index (0-3)

#### System Actions
- `Milliseconds` (int): For Delay actions
- `Command` (string): For CommandExecution actions
- `ShellType` (CommandShellType): PowerShell or CMD (for CommandExecution actions)

## Configuration File Format

### New JSON Structure

```json
{
  "midiDevices": [
    {
      "inputProfile": "Main-Controller",
      "deviceName": "MIDI Controller",
      "mappings": [
        {
          "description": "Note 60 to A key",
          "input": {
            "inputType": "NoteOn",
            "inputNumber": 60,
            "channel": 1
          },
          "action": {
            "type": "KeyPressRelease",
            "description": "Press A key",
            "parameters": {
              "VirtualKeyCode": 65
            }
          }
        },
        {
          "description": "CC7 opacity control with ranges",
          "input": {
            "inputType": "ControlChange",
            "inputNumber": 7
          },
          "action": {
            "type": "ConditionalAction",
            "description": "Opacity control using keys 1-0",
            "conditions": [
              {
                "minValue": 0,
                "maxValue": 12,
                "action": {
                  "type": "KeyPressRelease",
                  "parameters": { "VirtualKeyCode": 49 }
                }
              },
              {
                "minValue": 13,
                "maxValue": 25,
                "action": {
                  "type": "KeyPressRelease",
                  "parameters": { "VirtualKeyCode": 50 }
                }
              }
            ]
          }
        },
        {
          "description": "Ctrl+C macro sequence",
          "input": {
            "inputType": "NoteOn",
            "inputNumber": 60
          },
          "action": {
            "type": "SequenceAction",
            "description": "Copy to clipboard (Ctrl+C)",
            "parameters": {
              "ErrorHandling": "StopOnError"
            },
            "subActions": [
              {
                "type": "KeyDown",
                "description": "Press Ctrl key",
                "parameters": { "VirtualKeyCode": 17 }
              },
              {
                "type": "KeyPressRelease",
                "description": "Press and release C key",
                "parameters": { "VirtualKeyCode": 67 }
              },
              {
                "type": "KeyUp",
                "description": "Release Ctrl key",
                "parameters": { "VirtualKeyCode": 17 }
              }
            ]
          }
        }
      ]
    }
  ]
}
```

## Performance Optimization Integration

### Hybrid Lookup Table Architecture

**Goal:** Achieve <5ms latency by optimizing MIDI event → action execution path.

**Strategy:** Build performance optimization directly into the unified mapping system using a hybrid lookup table approach.

#### Simplified Lookup Strategy
```csharp
// Use string keys for O(1) lookup - much simpler than complex structs
// Format: "DeviceName|Channel|InputNumber|InputType"
// Examples:
//   "MIDI Controller|1|60|NoteOn"
//   "*|*|7|ControlChange"  (wildcard device and channel)
//   "MIDI Controller|*|60|NoteOn"  (wildcard channel only)

public static class UnifiedActionLookupKeyBuilder
{
    public static string BuildKey(string? deviceName, int? channel, int inputNumber, UnifiedActionMidiInputType inputType)
    {
        return $"{deviceName ?? "*"}|{channel?.ToString() ?? "*"}|{inputNumber}|{inputType}";
    }

    public static List<string> BuildLookupKeys(string deviceName, int? channel, int inputNumber, UnifiedActionMidiInputType inputType)
    {
        // Generate all possible lookup combinations for fallback matching
        var keys = new List<string>();

        // 1. Exact match
        keys.Add(BuildKey(deviceName, channel, inputNumber, inputType));

        // 2. Wildcard channel
        if (channel.HasValue)
            keys.Add(BuildKey(deviceName, null, inputNumber, inputType));

        // 3. Wildcard device
        keys.Add(BuildKey(null, channel, inputNumber, inputType));

        // 4. Wildcard both
        if (channel.HasValue)
            keys.Add(BuildKey(null, null, inputNumber, inputType));

        return keys;
    }
}
```

#### Optimized Mapping Registry
```csharp
public class UnifiedActionMappingRegistry
{
    // Immutable registry for lock-free reads
    private volatile IReadOnlyDictionary<string, List<IUnifiedAction>> _mappings =
        new Dictionary<string, List<IUnifiedAction>>();
    private readonly ILogger _logger;

    public UnifiedActionMappingRegistry(ILogger logger)
    {
        _logger = logger;
    }

    // Thread-safe registry replacement - called during profile loading
    public void LoadMappings(IEnumerable<UnifiedActionMapping> mappings)
    {
        _logger.LogDebug("Loading {Count} mappings into registry", mappings.Count());

        // Build new immutable dictionary off the hot path
        var newMappings = new Dictionary<string, List<IUnifiedAction>>();

        foreach (var mapping in mappings)
        {
            var lookupKey = mapping.GetLookupKey();

            if (!newMappings.ContainsKey(lookupKey))
            {
                newMappings[lookupKey] = new List<IUnifiedAction>();
            }

            newMappings[lookupKey].Add(mapping.Action);
        }

        // Atomic swap - all readers instantly see new registry
        _mappings = newMappings;
        _logger.LogDebug("Registry updated with {Count} lookup keys", newMappings.Count);
    }

    public List<IUnifiedAction> FindActions(string deviceName, int? channel, int inputNumber, UnifiedActionMidiInputType inputType)
    {
        var results = new List<IUnifiedAction>();

        // Pre-compute lookup keys once to avoid string allocation on each lookup
        var channelStr = channel?.ToString();
        var inputTypeStr = inputType.ToString();

        // Try lookups in priority order (exact → wildcard) - build strings only once
        var lookupKeys = new[] {
            BuildLookupKey(deviceName, channelStr, inputNumber, inputTypeStr),     // Exact
            BuildLookupKey(deviceName, "*", inputNumber, inputTypeStr),           // Device + wildcard channel
            BuildLookupKey("*", channelStr, inputNumber, inputTypeStr),           // Wildcard device + channel
            BuildLookupKey("*", "*", inputNumber, inputTypeStr)                   // Both wildcards
        };

        foreach (var key in lookupKeys)
        {
            if (_mappings.TryGetValue(key, out var actions))
            {
                results.AddRange(actions);
                _logger.LogTrace("Found {Count} actions for key: {Key}", actions.Count, key);
            }
        }

        _logger.LogDebug("Found {Count} total actions for {Device}|{Channel}|{Input}|{Type}",
            results.Count, deviceName, channel, inputNumber, inputType);

        return results;
    }

    // Optimized key building to minimize allocations
    private static string BuildLookupKey(string? deviceName, string? channel, int inputNumber, string inputType)
    {
        return $"{deviceName ?? "*"}|{channel ?? "*"}|{inputNumber}|{inputType}";
    }
}
```

#### Performance Benefits
- **O(1) lookup** for exact device/channel/input combinations
- **Pre-compiled handlers** eliminate runtime handler creation overhead
- **Minimal value range processing** - only when needed
- **Optimized for common case** - single mapping per lookup key

#### Handler Interface for Performance
```csharp
public interface IUnifiedActionHandler
{
    string Id { get; }
    UnifiedActionType ActionType { get; }

    // Fast execution - no parameter parsing at runtime
    Task ExecuteAsync(int? midiValue = null);

    // For debugging/logging
    string Description { get; }
}

public abstract class UnifiedActionHandlerBase : IUnifiedActionHandler
{
    public string Id { get; protected set; } = "";
    public abstract UnifiedActionType ActionType { get; }
    public abstract string Description { get; }

    public abstract Task ExecuteAsync(int? midiValue = null);

    // Helper for value transformation
    protected int TransformValue(int inputValue, UnifiedActionValueMapping? valueMapping)
    {
        if (valueMapping == null) return inputValue;

        // Fast linear transformation
        var inputRange = (valueMapping.InputMaxValue ?? 127) - (valueMapping.InputMinValue ?? 0);
        var outputRange = (valueMapping.OutputMaxValue ?? 127) - (valueMapping.OutputMinValue ?? 0);

        if (inputRange == 0) return valueMapping.OutputMinValue ?? 0;

        var normalizedInput = (double)(inputValue - (valueMapping.InputMinValue ?? 0)) / inputRange;
        if (valueMapping.InvertMapping) normalizedInput = 1.0 - normalizedInput;

        return (int)((valueMapping.OutputMinValue ?? 0) + (normalizedInput * outputRange));
    }
}
```

#### Optimized Event Processing Flow
```csharp
// Current: ~2-5ms processing time
// Target: ~0.1-1ms processing time

public class OptimizedEventProcessor
{
    private readonly UnifiedActionMappingRegistry _registry;
    private readonly ILogger _logger;

    public async Task ProcessMidiEvent(string deviceName, int channel, int inputNumber,
        UnifiedActionMidiInputType inputType, int? value = null)
    {
        // Step 1: O(1) lookup (target: <0.1ms)
        var mappings = _registry.FindMappings(deviceName, channel, inputNumber, inputType, value);

        // Step 2: Execute pre-compiled handlers (target: <0.5ms per handler)
        foreach (var mapping in mappings)
        {
            if (!mapping.IsEnabled) continue;

            var handler = _registry.GetCompiledHandler(mapping.Id);
            if (handler != null)
            {
                await handler.ExecuteAsync(value);
            }
        }
    }
}
```

## Implementation Strategy

### Phase 1: Core Infrastructure + Performance Foundation
1. Create all UnifiedAction* classes with performance optimization built-in
2. Create UnifiedActionMappingRegistry with hybrid lookup table
3. Create UnifiedActionFactory with pre-compilation support
4. Create configuration loading/saving for new format
5. Implement optimized action execution engine

### Phase 2: Action Implementation
1. Implement all keyboard actions with IUnifiedActionHandler interface
2. Implement mouse actions (complete the missing backend)
3. Implement system actions (delay, command execution)
4. Implement game controller actions
5. Implement sequence actions with optimized sub-action execution

### Phase 3: UI Migration
1. Create base UnifiedActionMappingDialog class for simple mappings
2. Create separate dialogs for complex actions:
   - SequenceActionDialog (for macro creation)
   - ConditionalActionDialog (for fader-to-buttons)
3. Migrate existing dialogs to use unified system
4. Remove old dialog classes
5. Update ProfileEditor to use unified mappings and registry

### Phase 4: Legacy Cleanup + Performance Validation
1. Remove old ActionType, KeyActionType, CCRangeActionType enums
2. Remove old mapping classes (KeyMapping, MacroMapping, etc.)
3. Rename all "UnifiedAction*" to "Action*"
4. Update all references throughout codebase
5. Benchmark and validate <5ms latency target

#### Performance Monitoring Integration
```csharp
public class UnifiedActionPerformanceMonitor
{
    private readonly Dictionary<string, PerformanceMetrics> _metrics = new();

    public void RecordEventProcessingTime(string deviceName, TimeSpan processingTime)
    {
        // Track processing times to validate <5ms target
        // Log warnings if processing exceeds target
    }

    public void RecordLookupTime(TimeSpan lookupTime)
    {
        // Monitor lookup performance - should be <0.1ms
    }

    public PerformanceReport GenerateReport()
    {
        // Generate performance reports for optimization validation
    }
}
```

## Design Decisions (Finalized)

### 1. **Value Ranges**: Not supported in mappings
- **Decision**: Mappings do NOT support value ranges
- **Rationale**: Keep mapping layer simple and fast
- **Implementation**: Use ConditionalAction for value-based logic

### 2. **Pre-compilation Strategy**: Pre-compile on profile load
- **Decision**: Pre-compile all actions when profile is loaded (including startup)
- **Rationale**: Optimize runtime performance, acceptable load-time cost
- **Implementation**: Actions created and cached during profile loading

### 3. **Parameter Validation**: At profile load time
- **Decision**: Validate action parameters during profile loading/mapping creation
- **Rationale**: Catch errors early, ensure runtime reliability
- **Implementation**: Factory validates, actions validate in constructors

### 4. **Device Matching Priority**: Exact first, then wildcards
- **Decision**: Check exact matches first, then wildcard fallbacks
- **Priority Order**:
  1. Exact device + exact channel
  2. Exact device + wildcard channel
  3. Wildcard device + exact channel
  4. Wildcard device + wildcard channel

### 5. **Invalid Mapping Handling**: Graceful degradation
- **Decision**: Load valid mappings, show errors for invalid ones
- **Implementation**: Continue profile loading, display warning dialog with errors
- **User Experience**: Partial functionality better than complete failure

### 6. **Performance Optimization Target**: <30 mappings typical
- **Decision**: Optimize for speed over memory usage
- **Assumption**: Typical users have <30 mappings, not <1000
- **Implementation**: Pre-compilation and caching justified for this scale

### 7. **Logging Strategy**: Comprehensive logging initially
- **Decision**: Full logging in critical path during development/testing
- **Rationale**: Debugging and validation more important than micro-optimizations
- **Future**: Review and optimize logging after system is proven stable

### 8. **Lookup Strategy**: Single dictionary with fallback search
- **Decision**: One dictionary, search with 4 lookup attempts
- **Implementation**:
  ```csharp
  // Try lookups in priority order
  var keys = new[] {
      $"{deviceName}|{channel}|{inputNumber}|{inputType}",     // Exact
      $"{deviceName}|*|{inputNumber}|{inputType}",             // Device + wildcard channel
      $"*|{channel}|{inputNumber}|{inputType}",                // Wildcard device + channel
      $"*|*|{inputNumber}|{inputType}"                         // Both wildcards
  };

  foreach (var key in keys) {
      if (_mappings.TryGetValue(key, out var action)) {
          results.Add(action);
      }
  }
  ```
- **Rationale**: Simple architecture, predictable performance, easy to understand

## Performance Validation Plan

### Benchmarking Requirements
1. **Latency Measurement**: End-to-end MIDI input to action execution
2. **Throughput Testing**: High-frequency MIDI events (e.g., rapid note sequences)
3. **Memory Usage**: Handler pre-compilation memory overhead
4. **Startup Time**: Configuration loading and handler compilation time

### Performance Targets
- **Total Latency**: <5ms (MIDI input to Windows SendInput)
- **Lookup Time**: <0.1ms (registry lookup)
- **Handler Execution**: <0.5ms (per handler)
- **Memory Overhead**: <50MB for 1000 mappings
- **Startup Time**: <2s for typical configuration

### Fallback Strategy
If performance targets aren't met:
1. **Reduce logging** in critical path
2. **Simplify value transformation** calculations
3. **Cache frequently used handlers** in hot path
4. **Consider native code** for critical operations

This specification provides the foundation for discussion and refinement before implementation begins.
