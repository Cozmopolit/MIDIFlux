# Handler Factory and Plugin System

MIDIFlux uses a Factory Pattern for creating handlers, which provides a foundation for a future plugin system. This document explains how the Handler Factory works and how it will enable plugins.

## Handler Factory

The Handler Factory is responsible for creating different types of handlers based on configuration. It centralizes handler creation and registration, making it easier to add new handler types without modifying the core application.

### Handler Types

MIDIFlux supports three main types of handlers:

1. **Absolute Value Handlers**: Process absolute values from MIDI controls (0-127)
   - Example: System Volume Handler, vJoy Axis Handler

2. **Relative Value Handlers**: Process relative values from MIDI controls (increments/decrements)
   - Example: Scroll Wheel Handler

3. **Note Handlers**: Process MIDI note events (Note On/Off)
   - Example: vJoy Button Handler

### Built-in Handlers

The Handler Factory registers the following built-in handlers:

| Handler Type | Description | Platform |
|--------------|-------------|----------|
| SystemVolume | Controls system volume | Windows |
| GameControllerAxis | Maps MIDI controls to game controller axes | Windows |
| ScrollWheel | Maps MIDI controls to mouse scroll wheel | Windows |
| GameControllerButton | Maps MIDI notes to game controller buttons | Windows |
| CommandExecution | Executes shell commands | All |

Note: Macros are now handled through the dedicated MacroMapping system, not through the handler factory.

### Using the Handler Factory

The Handler Factory is used by the Event Dispatcher to create handlers based on configuration:

```csharp
// Create a factory
var handlerFactory = new HandlerFactory(logger);

// Create an absolute value handler
var absoluteHandler = handlerFactory.CreateAbsoluteHandler("SystemVolume", parameters);

// Create a relative value handler
var relativeHandler = handlerFactory.CreateRelativeHandler("ScrollWheel", parameters);

// Create a note handler
var noteHandler = handlerFactory.CreateNoteHandler("VJoyButton", parameters);
```

### Parameters

Handlers can be configured with parameters:

```json
{
  "handlerType": "ScrollWheel",
  "sensitivity": 2,
  "parameters": {
    "customParam1": "value1",
    "customParam2": 42
  }
}
```

The Handler Factory extracts these parameters and passes them to the handler constructor.

## Plugin System Foundation

The Handler Factory provides a foundation for a future plugin system. Here's how it will work:

### Plugin Registration

Plugins will register their handlers with the Handler Factory:

```csharp
// Register a custom handler
handlerFactory.RegisterHandler<MyCustomHandler>("MyCustomHandler", HandlerType.Absolute);
```

### Plugin Discovery

The application will discover plugins by scanning for assemblies in a plugins directory:

```csharp
// Load plugins from a directory
handlerFactory.LoadPlugins("plugins");
```

### Plugin Configuration

Plugins will provide their own configuration schema:

```json
{
  "handlerType": "MyCustomHandler",
  "parameters": {
    "customParam1": "value1",
    "customParam2": 42
  }
}
```

### Plugin Interfaces

Plugins will implement one or more of the following interfaces:

- `IAbsoluteValueHandler`: For handlers that process absolute values
- `IRelativeValueHandler`: For handlers that process relative values
- `INoteHandler`: For handlers that process note events

All of these interfaces inherit from `IMidiControlHandler`, which provides common properties like `ControlType` and `Description`.

## Game Controller Handler Hierarchy

The game controller handlers use a base class to share common functionality:

```
GameControllerBase (abstract)
├── GameControllerAxisHandler
└── GameControllerButtonHandler
```

The `GameControllerNoteHandler` uses composition to delegate to a `GameControllerButtonHandler`:

```
GameControllerNoteHandler
└── GameControllerButtonHandler
```

This design reduces code duplication and improves maintainability.

## Future Enhancements

The plugin system will be enhanced with the following features:

1. **Plugin Configuration UI**: Plugins will be able to provide their own configuration UI
2. **Plugin Versioning**: The application will check plugin compatibility
3. **Plugin Dependencies**: Plugins will be able to depend on other plugins
4. **Plugin Hot-Reloading**: Plugins will be reloadable without restarting the application

These enhancements will be implemented in Phase 8 of the development roadmap.

