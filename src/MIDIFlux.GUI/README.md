# MIDIFlux.GUI

This project contains the Configuration GUI for the MIDIFlux application.

## Architecture

The Configuration GUI is designed as a tabbed interface that allows users to manage MIDI profiles, edit mappings, and configure application settings. It communicates with the main MIDIFlux application through the `MidiProcessingServiceProxy` class.

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

When creating new components in MIDIFlux.GUI, use the LoggingHelper class:

```csharp
// Import the LoggingHelper
using MIDIFlux.Core.Helpers;

// Create a logger for your component
var logger = LoggingHelper.CreateLogger<YourComponent>();
```

This approach ensures that your component will use the main application's logging infrastructure when available, with a fallback to console logging when running standalone.

### Benefits

- Centralized log management through the `LoggingHelper` class
- Consistent log levels across the application
- Single log file for easier troubleshooting
- Ability to control all logging through the main application's configuration
- Simplified logger creation with `LoggingHelper.CreateLogger<T>()`
- Consistent error handling with `ApplicationErrorHandler`
- Support for silent mode through `ApplicationErrorHandler.SilentMode`

## Form Naming Conventions

The project uses descriptive form names to clearly indicate their purpose:

- `ConfigurationForm`: The main form for the Configuration GUI that provides a tabbed interface
- `SystemTrayForm`: The main application form that manages the system tray icon and provides access to MIDIFlux functionality

## Project Structure

- `Controls/`: User controls for different tabs in the Configuration GUI
- `Forms/`: Forms used in the application
- `Helpers/`: Helper classes for common functionality
- `Interfaces/`: Interfaces used throughout the project
- `Models/`: Data models
- `Services/`: Service classes, including the `MidiProcessingServiceProxy`

