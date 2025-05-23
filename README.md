# MIDIFlux

MIDIFlux is a Windows application that bridges MIDI input devices to keyboard events, allowing you to control your computer using MIDI controllers such as foot pedals, keyboards, or other MIDI devices.

## Overview

MIDIFlux captures MIDI events from connected devices and translates them into keyboard keypresses based on user-defined mappings. This enables musicians, presenters, and power users to create custom control setups using MIDI hardware.

### Key Features

- **MIDI Device Support**: Discover and connect to any MIDI input device
- **Device Hot-Plugging**: Reconnect to devices when they're plugged in
- **Customizable Mappings**: Map MIDI notes and controls to keyboard shortcuts
- **Multiple Profiles**: Create and switch between different mapping profiles
- **MIDI Channel Filtering**: Configure which MIDI channels to respond to
- **Test Mode**: Monitor MIDI inputs without triggering keyboard events
- **Detailed Logging**: Console and file-based logging for debugging
- **Relative Controls**: Support for jog wheels and other relative controls
- **Game Controller Integration**: Map MIDI controls to Xbox controller inputs (requires ViGEm)
- **System Volume Control**: Control system volume with MIDI faders
- **Mouse Wheel Control**: Control mouse scroll wheel with MIDI jog wheels

## Getting Started

### Prerequisites

- Windows operating system
- .NET 8.0 or later
- One or more MIDI input devices

### Installation

1. Clone or download this repository
2. Build the solution using Visual Studio or the .NET CLI:
   ```
   dotnet build
   ```

### Usage

#### Running the Application

To run MIDIFlux:

```
dotnet run --project src\MIDIFlux.App
```

The application will start in the system tray. Right-click the tray icon to access the menu.

#### Running with Configuration

To run the application with a specific configuration file:

```
dotnet run --project src\MIDIFlux.App --config config_examples\your-config.json
```

Example configuration files are available in the `config_examples` directory:
- `example-basic-keys.json`: Basic keyboard shortcuts
- `example-game-controller.json`: Game controller emulation
- `example-system-controls.json`: System volume control

#### Configuration

The application uses JSON configuration files. Example configurations are located in the `config_examples` directory. Configuration files can define:

- MIDI device to use
- MIDI channels to listen to
- Key mappings (MIDI notes to keyboard keys)
- Absolute control mappings (faders, knobs)
- Relative control mappings (jog wheels)
- Game controller mappings (for Xbox controller emulation, requires ViGEm)

For detailed documentation on configuration options, see the [Documentation](Documentation/README.md).

## Project Structure

- **src/MIDIFlux.Core**: Core library containing MIDI handling and keyboard simulation
- **src/MIDIFlux.App**: Windows Forms application for the user interface
- **src/MIDIFlux.GUI**: GUI components library for configuration
- **config**: Configuration files
- **Documentation**: Project documentation

## Development

MIDIFlux is developed in C# using .NET 8.0. It uses the NAudio library for MIDI device access and the Windows SendInput API for keyboard simulation.

### Development Status

MIDIFlux has implemented the following features:

1. ✅ **Phase 1**: MIDI Input Test Mode
2. ✅ **Phase 2**: Basic Keyboard Mapping
3. ✅ **Phase 3**: Advanced Features (profiles, channel filtering, hot-plugging)
4. ✅ **Phase 4**: Relative Controls (jog wheels)
5. ✅ **Phase 5**: Game Controller Integration (Xbox controller emulation)
6. ✅ **Phase 6**: Factory Pattern and Plugin Foundation
7. ✅ **Phase 7**: GUI for Configuration

Future development plans:

1. 🔄 **Phase 8**: Plugin System for Custom Handlers

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Dependencies

MIDIFlux relies on the following external libraries:

- [NAudio](https://github.com/naudio/NAudio) - For MIDI device access (included via NuGet)
- [ViGEm](https://github.com/ViGEm/ViGEmBus) - For Xbox controller emulation
  - [ViGEm.NET](https://github.com/ViGEm/ViGEm.NET) - .NET bindings for ViGEm
  - [ViGEmClient](https://github.com/ViGEm/ViGEmClient) - C/C++ SDK for ViGEm

The ViGEm dependencies are included via the NuGet package `Nefarius.ViGEm.Client`. The source code for these libraries is available at the links above.

### Game Controller Integration

To use the game controller integration features, you need to install the ViGEm Bus Driver:

1. Download and install the [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases)
2. Restart your computer
3. MIDIFlux will automatically detect the driver and enable game controller features

## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio) for MIDI device access
- [ViGEm](https://github.com/ViGEm/ViGEmBus) for Xbox controller emulation
