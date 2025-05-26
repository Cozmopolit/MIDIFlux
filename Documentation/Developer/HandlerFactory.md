# Action Factory and Extensibility System

MIDIFlux uses the unified Action Factory pattern for creating actions from configuration, which provides a foundation for extensibility and future plugin systems. This document explains how the Action Factory works and how it enables the unified action system.

## Unified Action Factory

The Action Factory is responsible for creating strongly-typed actions from configuration objects. It centralizes action creation and registration, making it easier to add new action types while maintaining type safety and performance.

### Action System Architecture

MIDIFlux uses a two-tier action system:

1. **Simple Actions**: Direct execution for performance (hot path)
   - KeyPressRelease, KeyDown, KeyUp, KeyToggle
   - MouseClick, MouseScroll
   - GameControllerButton, GameControllerAxis
   - CommandExecution, Delay, MidiOutput

2. **Complex Actions**: Orchestration and logic
   - SequenceAction, ConditionalAction, AlternatingAction
   - StateConditionalAction, SetStateAction

### Built-in Action Types

The Action Factory supports the following built-in action types:

| Action Type | Configuration Class | Description | Category |
|-------------|-------------------|-------------|----------|
| KeyPressRelease | `KeyPressReleaseConfig` | Press and release a key | Simple |
| KeyDown | `KeyDownConfig` | Press and hold a key | Simple |
| KeyUp | `KeyUpConfig` | Release a key | Simple |
| KeyToggle | `KeyToggleConfig` | Toggle key state | Simple |
| MouseClick | `MouseClickConfig` | Click mouse buttons | Simple |
| MouseScroll | `MouseScrollConfig` | Scroll mouse wheel | Simple |
| GameControllerButton | `GameControllerButtonConfig` | Press controller button | Simple |
| GameControllerAxis | `GameControllerAxisConfig` | Control controller axis | Simple |
| CommandExecution | `CommandExecutionConfig` | Execute shell commands | Simple |
| Delay | `DelayConfig` | Wait for specified time | Simple |
| MidiOutput | `MidiOutputConfig` | Send MIDI messages | Simple |
| Sequence | `SequenceConfig` | Execute actions in sequence | Complex |
| Conditional | `ConditionalConfig` | Execute based on MIDI value | Complex |
| Alternating | `AlternatingActionConfig` | Toggle between two actions | Complex |
| StateConditional | `StateConditionalConfig` | Execute based on state values | Complex |
| SetState | `SetStateConfig` | Set state values | Complex |

## Using the Action Factory

The Action Factory is used throughout MIDIFlux to create actions from configuration:

```csharp
// Create a factory with dependency injection
var actionFactory = new UnifiedActionFactory(serviceProvider, logger);

// Create actions from configuration
var keyAction = actionFactory.CreateAction(new KeyPressReleaseConfig
{
    VirtualKeyCode = 65,
    Description = "Press A key"
});

var sequenceAction = actionFactory.CreateAction(new SequenceConfig
{
    SubActions = new List<ActionConfigBase>
    {
        new KeyDownConfig { VirtualKeyCode = 162 },
        new KeyPressReleaseConfig { VirtualKeyCode = 67 },
        new KeyUpConfig { VirtualKeyCode = 162 }
    },
    Description = "Ctrl+C sequence"
});
```

### Configuration with $type Discriminators

Actions use strongly-typed configuration with `$type` discriminators:

```json
{
  "Id": "example-action",
  "Description": "Example action mapping",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "KeyDownConfig",
        "VirtualKeyCode": 162,
        "Description": "Press Ctrl"
      },
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 67,
        "Description": "Press C"
      },
      {
        "$type": "KeyUpConfig",
        "VirtualKeyCode": 162,
        "Description": "Release Ctrl"
      }
    ],
    "Description": "Copy shortcut (Ctrl+C)"
  }
}
```

The Action Factory uses the `$type` property to determine which action class to instantiate.

## Extensibility Foundation

The Action Factory provides a foundation for future extensibility and plugin systems. Here's how it enables extension:

### Action Registration

New action types can be registered with the Action Factory:

```csharp
// Register a custom action type
actionFactory.RegisterActionType<MyCustomAction, MyCustomActionConfig>("MyCustomAction");
```

### Custom Action Implementation

Custom actions implement the `IAction` interface:

```csharp
public class MyCustomAction : IAction
{
    public string Id { get; }
    public string Description { get; }

    public MyCustomAction(MyCustomActionConfig config)
    {
        Id = config.Id ?? Guid.NewGuid().ToString();
        Description = config.Description ?? "Custom action";
    }

    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Custom action logic here
        // For simple synchronous operations, return ValueTask.CompletedTask
        // For async operations, use async/await pattern

        return ValueTask.CompletedTask;
    }
}
```

### Custom Configuration Classes

Custom actions use strongly-typed configuration:

```csharp
public class MyCustomActionConfig : ActionConfigBase
{
    public string CustomProperty { get; set; }
    public int CustomValue { get; set; }
}
```

### Plugin Discovery (Future)

Future plugin system will support:

```csharp
// Load plugins from a directory
actionFactory.LoadPlugins("plugins");

// Discover actions from assemblies
actionFactory.DiscoverActions(assembly);
```

### Plugin Configuration (Future)

Plugins will provide their own configuration schema:

```json
{
  "Id": "custom-action",
  "Description": "Custom plugin action",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 60,
  "Action": {
    "$type": "MyCustomActionConfig",
    "CustomProperty": "value1",
    "CustomValue": 42,
    "Description": "Custom action instance"
  }
}
```

## Dependency Injection Integration

The Action Factory integrates with the dependency injection system:

### Service Registration

```csharp
// In ServiceCollectionExtensions.cs
services.AddSingleton<IActionFactory, UnifiedActionFactory>();
services.AddSingleton<ActionStateManager>();
services.AddSingleton<IMidiHardwareAdapter, NAudioMidiAdapter>();
```

### Action Dependencies

Actions can receive dependencies through constructor injection:

```csharp
public class KeyDownAction : IAction
{
    private readonly ActionStateManager _stateManager;

    public KeyDownAction(KeyDownConfig config, ActionStateManager stateManager)
    {
        _stateManager = stateManager;
        // Initialize action
    }
}
```

### Factory Dependencies

The Action Factory receives the service provider for dependency resolution:

```csharp
public class UnifiedActionFactory : IActionFactory
{
    private readonly IServiceProvider _serviceProvider;

    public UnifiedActionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IAction CreateAction(ActionConfigBase config)
    {
        // Use service provider to resolve dependencies
        return config switch
        {
            KeyDownConfig keyConfig => new KeyDownAction(keyConfig,
                _serviceProvider.GetRequiredService<ActionStateManager>()),
            // ... other action types
        };
    }
}
```

## Performance Considerations

### Action Pre-compilation

- **Profile Load Time**: Actions are created once when profile loads
- **Runtime Performance**: No reflection or factory calls during MIDI processing
- **Memory Efficiency**: Actions are reused for multiple MIDI events
- **Type Safety**: Compile-time checking of action configurations

### Factory Optimization

- **Cached Reflection**: Type information cached for performance
- **Minimal Allocations**: Reuse objects where possible
- **Fast Dispatch**: Switch expressions for optimal performance
- **Dependency Caching**: Service resolution optimized

## Future Enhancements

### Planned Plugin Features

1. **Plugin Assembly Loading**: Dynamic loading of plugin assemblies
2. **Plugin Configuration UI**: Visual configuration for custom actions
3. **Plugin Versioning**: Compatibility checking and version management
4. **Plugin Dependencies**: Inter-plugin dependency resolution
5. **Plugin Hot-Reloading**: Runtime plugin updates without restart
6. **Plugin Marketplace**: Distribution and discovery of community plugins

### Extensibility Improvements

1. **Action Composition**: Combine simple actions into complex behaviors
2. **Action Templates**: Reusable action patterns and templates
3. **Action Validation**: Enhanced validation for custom action types
4. **Action Documentation**: Auto-generated documentation for actions
5. **Action Testing**: Built-in testing framework for custom actions

### Performance Enhancements

1. **JIT Compilation**: Just-in-time compilation of action sequences
2. **Action Pooling**: Object pooling for frequently used actions
3. **Batch Execution**: Batch multiple actions for efficiency
4. **Async Optimization**: Better async/await patterns for complex actions

The Action Factory provides a solid foundation for MIDIFlux's extensibility while maintaining performance and type safety throughout the system.

