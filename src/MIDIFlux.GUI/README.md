# MIDIFlux.GUI

This project contains GUI infrastructure and basic forms for the MIDIFlux application.

## Current Status

The MIDIFlux.GUI project provides basic GUI infrastructure and forms, but does not implement a comprehensive configuration interface. The main MIDIFlux application works primarily through:

- **JSON configuration files** in `%AppData%\MIDIFlux\profiles\`
- **System tray interface** for basic operations
- **Direct profile editing** using any text editor

## What's Included

### Forms
- **`ConfigurationForm`**: Basic configuration form with minimal functionality
- **`SettingsForm`**: Application settings dialog
- **`SystemTrayForm`**: System tray integration (part of MIDIFlux.App)

### Infrastructure
- **`MidiProcessingServiceProxy`**: Communication bridge with the main application
- **Base controls**: Common base classes for GUI components
- **Error handling**: Integration with centralized error handling
- **Logging integration**: Centralized logging through the main application

## Logging

### Important: Centralized Logging

MIDIFlux.GUI uses the main application's logging infrastructure to ensure consistent logging across the entire application. This approach provides several benefits:

1. All logs are written to the same log file
2. Log levels are controlled centrally through the main application's configuration
3. Log rotation and management are handled consistently

### How Logging Works

1. The main application (`SystemTrayForm`) passes its `ILoggerFactory` to the Configuration GUI when launching it
2. The `MidiProcessingServiceProxy` receives this logger factory via `SetLoggerFactory()` and sets it in the centralized `LoggingHelper` class
3. All components in MIDIFlux.GUI should obtain their loggers through the `LoggingHelper` class

### Implementing Logging in New Components

When creating new components in MIDIFlux.GUI, use the LoggingHelper class with the standardized `ILogger<T>` pattern:

```csharp
// Import the LoggingHelper
using MIDIFlux.Core.Helpers;

// Create a type-safe logger for your component
var logger = LoggingHelper.CreateLogger<YourComponent>();
```

**Important**: Always use the generic `CreateLogger<T>()` method for type-safe logging. This ensures:
- Consistent logger category naming based on the component type
- Better IntelliSense support and compile-time type checking
- Standardized logging patterns across the entire codebase

### Benefits

- Centralized log management through the `LoggingHelper` class
- Consistent log levels across the application
- Single log file for easier troubleshooting
- Ability to control all logging through the main application's configuration
- **Type-safe logging** with `LoggingHelper.CreateLogger<T>()` - standardized across all components
- Consistent error handling with `ApplicationErrorHandler`
- Support for silent mode through `ApplicationErrorHandler.SilentMode`
- **Compile-time type checking** for logger categories
- **Better debugging experience** with strongly-typed logger names

## Project Structure

- `Controls/`: Basic user controls and infrastructure
- `Forms/`: Application forms (minimal implementation)
- `Helpers/`: Helper classes for common functionality
- `Interfaces/`: Interfaces used throughout the project
- `Models/`: Data models for GUI operations
- `Services/`: Service classes, including the `MidiProcessingServiceProxy`

## Configuration Management

MIDIFlux uses JSON configuration files for profile management. Users can:

1. **Edit profiles directly** using any text editor
2. **Use example profiles** from `%AppData%\MIDIFlux\profiles\examples\`
3. **Create new profiles** by copying and modifying existing ones
4. **Activate profiles** through the system tray menu

For detailed information about the configuration format and action system, see the main project documentation.

## Future Development

The GUI project provides a foundation for future GUI development but currently focuses on:
- Infrastructure and logging integration
- Basic forms and dialogs
- Service communication patterns
- Error handling integration

Any future GUI expansion would build upon this existing infrastructure.
