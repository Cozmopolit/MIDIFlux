# Action Type Registry and Extensibility System

MIDIFlux uses a reflection-based Action Type Registry for automatic action discovery and creation, providing a truly self-contained extensibility system. This document explains how the ActionTypeRegistry works and enables seamless addition of new action types.

## ActionTypeRegistry Overview

The ActionTypeRegistry automatically discovers all action types using reflection, eliminating the need for hardcoded mappings or switch statements. This creates a truly extensible system where adding new actions requires zero code changes elsewhere.

### Action System Architecture

MIDIFlux uses an action system with automatic discovery:

1. **Simple Actions**: Direct execution for performance (hot path)
   - KeyPressReleaseAction, KeyDownAction, KeyUpAction, KeyToggleAction
   - MouseClickAction, MouseScrollAction
   - GameControllerButtonAction, GameControllerAxisAction
   - CommandExecutionAction, DelayAction, MidiOutput actions

2. **Complex Actions**: Orchestration and logic
   - SequenceAction, ConditionalAction, AlternatingAction
   - StateConditionalAction, StateSetAction, StateIncreaseAction, StateDecreaseAction

### Built-in Action Types

The ActionTypeRegistry automatically discovers the following built-in action types:

| Action Class | Display Name | Description | Category |
|-------------|-------------|-------------|----------|
| KeyPressReleaseAction | Key Press/Release | Press and release a key | Simple |
| KeyDownAction | Key Down | Press and hold a key | Simple |
| KeyUpAction | Key Up | Release a key | Simple |
| KeyToggleAction | Key Toggle | Toggle key state | Simple |
| MouseClickAction | Mouse Click | Click mouse buttons | Simple |
| MouseScrollAction | Mouse Scroll | Scroll mouse wheel | Simple |
| GameControllerButtonAction | Game Controller Button | Press controller button | Simple |
| GameControllerAxisAction | Game Controller Axis | Control controller axis | Simple |
| CommandExecutionAction | Command Execution | Execute shell commands | Simple |
| DelayAction | Delay | Wait for specified time | Simple |
| SequenceAction | Sequence (Macro) | Execute actions in sequence | Complex |
| ConditionalAction | Conditional (CC Range) | Execute based on MIDI value | Complex |
| AlternatingAction | Alternating Toggle | Toggle between two actions | Complex |
| StateSetAction | State Set | Set state values | Stateful |
| StateIncreaseAction | State Increase | Increase state values | Stateful |
| StateDecreaseAction | State Decrease | Decrease state values | Stateful |
| StateConditionalAction | State Conditional | Execute based on state values | Stateful |

## Using the ActionTypeRegistry

The ActionTypeRegistry is used throughout MIDIFlux for automatic action discovery and creation:

```csharp
// Get the singleton registry instance
var registry = ActionTypeRegistry.Instance;

// Create actions dynamically by type name
var keyAction = registry.CreateActionInstance("KeyPressReleaseAction");
if (keyAction != null)
{
    keyAction.SetParameter("VirtualKeyCode", 65);
    keyAction.SetParameter("Description", "Press A key");
}

// Get display names for GUI
var displayNames = registry.GetAllActionDisplayNames();
foreach (var kvp in displayNames)
{
    Console.WriteLine($"{kvp.Key}: {kvp.Value}");
}

// Get display name for an action instance
var displayName = registry.GetActionDisplayName(keyAction);
```

### JSON Serialization with $type Discriminators

Actions use the parameter system with `$type` discriminators for JSON serialization:

```json
{
  "Id": "example-action",
  "Description": "Example action mapping",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "SequenceAction",
    "Parameters": {
      "SubActions": [
        {
          "$type": "KeyDownAction",
          "Parameters": {
            "VirtualKeyCode": 162,
            "Description": "Press Ctrl"
          }
        },
        {
          "$type": "KeyPressReleaseAction",
          "Parameters": {
            "VirtualKeyCode": 67,
            "Description": "Press C"
          }
        },
        {
          "$type": "KeyUpAction",
          "Parameters": {
            "VirtualKeyCode": 162,
            "Description": "Release Ctrl"
          }
        }
      ],
      "Description": "Copy shortcut (Ctrl+C)"
    }
  }
}
```

The ActionJsonConverter uses the `$type` property and ActionTypeRegistry to automatically instantiate the correct action class.

## Extensibility Foundation

The ActionTypeRegistry provides automatic extensibility without requiring any registration or factory modifications. Adding new action types is completely self-contained.

### Automatic Action Discovery

New action types are automatically discovered by the registry using reflection. No registration is required:

```csharp
// Simply create a new action class - it will be automatically discovered!
[ActionDisplayName("My Custom Action")]
public class MyCustomAction : ActionBase
{
    public override async ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Custom action logic here
        var customProperty = GetParameter<string>("CustomProperty");
        var customValue = GetParameter<int>("CustomValue");

        // For simple synchronous operations, return ValueTask.CompletedTask
        // For async operations, use async/await pattern

        await SomeAsyncOperation();
    }
}
```

### Custom Action Implementation

Custom actions inherit from `ActionBase` and use the parameter system:

```csharp
[ActionDisplayName("My Custom Action")]
public class MyCustomAction : ActionBase
{
    public override async ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Get parameters using the parameter system
        var customProperty = GetParameter<string>("CustomProperty");
        var customValue = GetParameter<int>("CustomValue", defaultValue: 42);
        var description = GetParameter<string>("Description", "Default description");

        // Custom action logic here
        Console.WriteLine($"Executing {description} with {customProperty} = {customValue}");

        // For async operations
        await Task.Delay(100);
    }
}
```

### No Configuration Classes Needed

Custom actions use the parameter system instead of separate configuration classes:

```csharp
// Set parameters directly on the action instance
var customAction = new MyCustomAction();
customAction.SetParameter("CustomProperty", "Hello World");
customAction.SetParameter("CustomValue", 123);
customAction.SetParameter("Description", "My custom action instance");
```

### Plugin Discovery (Future)

Future plugin system will automatically discover actions from plugin assemblies:

```csharp
// Plugin actions will be automatically discovered when assemblies are loaded
// No explicit registration required - just inherit from ActionBase!

// Load plugin assembly
Assembly.LoadFrom("MyPlugin.dll");

// ActionTypeRegistry will automatically discover new action types
var newActionTypes = ActionTypeRegistry.Instance.GetAllActionTypes();
```

### Plugin Configuration (Future)

Plugins will use the same parameter system:

```json
{
  "Id": "custom-action",
  "Description": "Custom plugin action",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "MyCustomAction",
    "Parameters": {
      "CustomProperty": "value1",
      "CustomValue": 42,
      "Description": "Custom action instance"
    }
  }
}
```

## Dependency Injection Integration

Actions can access services through the global service provider pattern:

### Service Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<ActionStateManager>();
services.AddSingleton<IMidiHardwareAdapter, NAudioMidiAdapter>();
services.AddSingleton<IKeyboardSimulator, KeyboardSimulator>();
```

### Action Dependencies

Actions access dependencies through the global service provider:

```csharp
[ActionDisplayName("Key Down")]
public class KeyDownAction : ActionBase
{
    public override async ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Access services through the global service provider
        var keyboardSimulator = ServiceProvider.GetRequiredService<IKeyboardSimulator>();
        var stateManager = ServiceProvider.GetRequiredService<ActionStateManager>();

        var virtualKeyCode = GetParameter<int>("VirtualKeyCode");
        keyboardSimulator.KeyDown((VirtualKeyCode)virtualKeyCode);
    }
}
```

### No Factory Dependencies

The ActionTypeRegistry is self-contained and doesn't require dependency injection:

```csharp
// ActionTypeRegistry is a singleton that manages itself
var registry = ActionTypeRegistry.Instance;

// Actions are created directly without factory dependencies
var action = registry.CreateActionInstance("KeyDownAction");

// Services are accessed globally within actions when needed
```

## Performance Considerations

### Action Pre-compilation

- **Profile Load Time**: Actions are created once when profile loads using reflection
- **Runtime Performance**: No reflection during MIDI processing - actions are pre-instantiated
- **Memory Efficiency**: Actions are reused for multiple MIDI events
- **Type Safety**: Compile-time checking of action implementations

### Registry Optimization

- **Cached Reflection**: Type information cached in ConcurrentDictionary for thread safety
- **Minimal Allocations**: Action instances reused, reflection performed once at startup
- **Fast Dispatch**: Direct method calls on pre-instantiated actions
- **Thread Safety**: ConcurrentDictionary ensures safe multi-threaded access

### Automatic Discovery Benefits

- **Zero Configuration**: No manual registration or factory updates required
- **Self-Contained**: Adding new actions requires zero code changes elsewhere
- **Compile-Time Safety**: Missing actions cause compilation errors, not runtime failures
- **Immediate Availability**: New action types are automatically available in GUI

## Future Enhancements

### Planned Plugin Features

1. **Plugin Assembly Loading**: Automatic discovery from plugin assemblies
2. **Plugin Configuration UI**: Visual configuration for custom actions using parameter metadata
3. **Plugin Versioning**: Compatibility checking through action attributes
4. **Plugin Dependencies**: Service injection for plugin actions
5. **Plugin Hot-Reloading**: Runtime assembly reloading with registry refresh
6. **Plugin Marketplace**: Distribution and discovery of community plugins

### Extensibility Improvements

1. **Action Composition**: Enhanced SubAction support for complex behaviors
2. **Action Templates**: Parameter presets and action templates
3. **Action Validation**: Parameter validation through attributes and metadata
4. **Action Documentation**: Auto-generated documentation from action attributes
5. **Action Metadata**: Rich metadata system for GUI hints and validation

### Performance Enhancements

1. **Lazy Loading**: On-demand action instantiation for large plugin sets
2. **Action Pooling**: Object pooling for frequently used action types
3. **Batch Execution**: Optimized batch processing for sequence actions
4. **Async Optimization**: Enhanced async/await patterns with ValueTask

The ActionTypeRegistry provides a truly self-contained foundation for MIDIFlux's extensibility while maintaining optimal performance and complete type safety throughout the system.

