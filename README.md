# MIDIFlux

[![Release](https://img.shields.io/github/v/release/Cozmopolit/MIDIFlux?include_prereleases&label=release&color=blue)](https://github.com/Cozmopolit/MIDIFlux/releases)
[![Build Status](https://img.shields.io/github/actions/workflow/status/Cozmopolit/MIDIFlux/build.yml?branch=main)](https://github.com/Cozmopolit/MIDIFlux/actions)
[![License](https://img.shields.io/github/license/Cozmopolit/MIDIFlux)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/Cozmopolit/MIDIFlux/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **🚧 Beta Release** - MIDIFlux is currently in beta. Core functionality is stable, but some features may still change before v1.0.

MIDIFlux is a powerful Windows application that transforms MIDI input devices into versatile computer controllers. Connect any MIDI device—foot pedals, keyboards, control surfaces, or custom controllers—and map them to keyboard shortcuts, mouse actions, system controls, game controller inputs, and more.

## Quick Start

1. **Download the latest release**: [MIDIFlux Latest Release](https://github.com/Cozmopolit/MIDIFlux/releases/latest)
2. **Run the executable** - No installation required!
3. **Find MIDIFlux in your system tray** and load an example profile to get started

For detailed installation and usage instructions, see the [Installation & Usage](#installation--usage) section below.

### 🎯 What MIDIFlux Does

MIDIFlux captures MIDI events from connected devices and translates them into a wide range of computer actions:

- **⌨️ Keyboard Shortcuts**: Map MIDI notes/controls to any key combination
- **🖱️ Mouse Control**: Control mouse movement, clicks, and scroll wheel
- **🎮 Game Controllers**: Emulate Xbox controllers for gaming (via ViGEm)
- **🎵 Media Controls**: Play/pause, track navigation via media keys
- **🔊 Audio Playback**: Trigger sound effects and audio samples
- **💻 System Commands**: Execute shell commands and scripts
- **🔄 Advanced Actions**: Conditional logic, sequences, and complex macros

Perfect for musicians, streamers, presenters, gamers, and power users who want to create custom control setups using MIDI hardware.

## Attribution

This project was created using AI-assisted development, with full human orchestration but no manual code input.

**Primary Implementation:**
- 🤖 **Claude Sonnet 3.5/3.6** (Anthropic) – Primary coding agent, responsible for majority of implementation
- 🤖 **Claude Opus 4** (Anthropic) – Complex architectural decisions and advanced implementations

**Development Tools:**
- 🛠️ **Augment Code** – Agentic coding environment with Claude integration and custom MCP tools

**Code Review & Analysis:**
- 🤖 **Gemini 2.5 Pro** (Google) – Code review and implementation analysis
- 🤖 **Claude Opus 4.5** (Anthropic) – Architectural review and complex analysis
- 🤖 **GPT-4.1 / GPT-5.1** (OpenAI) – Code review and architectural discussion
- 🤖 **DeepSeek V3.2** (DeepSeek) – Code review and implementation analysis
- 🤖 **Qwen3 Coder** (Alibaba) – Code review and implementation analysis

**Human Contribution:**
- 🧑‍💻 **Human** – Project architecture, UX design, feature planning, orchestration, testing, and decision-making

> ⚠️ No code or documentation was manually typed. Every line was generated, reviewed, and refined through AI tools.

### Key Features

- **MIDI Device Support**: Discover and connect to any MIDI input device (Windows MIDI Services on Windows 11 24H2+, NAudio fallback for older systems)
- **Device Hot-Plugging**: Reconnect to devices when they're plugged in
- **Customizable Mappings**: Map MIDI notes and controls to keyboard shortcuts
- **Multiple Profiles**: Create and switch between different mapping profiles
- **MIDI Channel Filtering**: Configure which MIDI channels to respond to
- **Test Mode**: Monitor MIDI inputs without triggering keyboard events
- **Detailed Logging**: Console and file-based logging for debugging
- **Relative Controls**: Support for jog wheels and other relative controls
- **Game Controller Integration**: Map MIDI controls to Xbox controller inputs (requires ViGEm)
- **Mouse Wheel Control**: Control mouse scroll wheel with MIDI jog wheels

## System Requirements

- **Operating System**: Windows 10/11 (x64)
- **Hardware**: One or more MIDI input devices
- **Dependencies**: None! (All .NET dependencies are included in the executable)
- **Optional**: ViGEm Bus Driver (only required for game controller emulation features)

## Installation & Usage

### 🚀 Using the Release Executable (Recommended)

1. **Download** the latest release from the [Releases page](https://github.com/Cozmopolit/MIDIFlux/releases)
2. **Extract** the executable to any folder (it's portable!)
3. **Run** the downloaded executable (e.g., `MIDIFlux-v0.9.1-win-x64.exe`)
4. **Look for the MIDIFlux icon** in your system tray
5. **Right-click the tray icon** to access the menu

### 📁 Configuration Files

MIDIFlux automatically creates example configuration files on first run:

**Location**: `%AppData%\MIDIFlux\profiles\examples\`

**Available Examples**:
- `basic-keyboard-shortcuts.json`: Basic keyboard shortcuts
- `game-controller-demo.json`: Game controller emulation
- `system-controls.json`: Media controls (play/pause, track navigation via media keys)

### 🎮 Loading a Profile

1. **Right-click** the system tray icon
2. **Select "Load Profile"**
3. **Choose** an example profile to get started
4. **Connect your MIDI device** and start using it!

### 🔧 Configuration

The application uses JSON configuration files. Example configurations are located in `%AppData%\MIDIFlux\profiles\examples\`. Start with the provided examples and customize them for your needs.

For detailed documentation on configuration options, see the [Getting Started Guide](Documentation/GETTING_STARTED.md) and [Action Reference](Documentation/ACTION_REFERENCE.md).

---

## 🛠️ For Developers

### Building from Source

If you want to build MIDIFlux yourself or contribute to development:

#### Prerequisites
- Windows 10/11
- .NET 10.0 SDK
- Visual Studio 2022 or VS Code

#### Build Steps
```bash
# Clone the repository
git clone https://github.com/Cozmopolit/MIDIFlux.git
cd MIDIFlux

# Restore dependencies
dotnet restore

# Build the solution
dotnet build --configuration Release

# Run the application
dotnet run --project src\MIDIFlux.App
```

#### Running Tests
```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

### Project Structure

- **src/MIDIFlux.Core**: Core library containing MIDI handling and keyboard simulation
- **src/MIDIFlux.App**: Windows Forms application for the user interface
- **src/MIDIFlux.GUI**: GUI components library for configuration
- **src/MIDIFlux.Core.Tests**: Unit and integration tests
- **Documentation**: Project documentation

### Development

MIDIFlux is developed in C# using .NET 10.0 with a clean, modular architecture. For detailed technical information, see the [Developer Guide](Documentation/DEVELOPER_GUIDE.md).

### Development Status - Beta Release

**Current Status**: Beta release with stable core functionality

**Completed Features**:
1. ✅ **MIDI Input Processing**: Full MIDI device support with hot-plugging
2. ✅ **Windows MIDI Services**: Native Windows 11 24H2+ support with NAudio fallback
3. ✅ **Comprehensive Action System**: Keyboard, mouse, game controllers, system commands
4. ✅ **Audio Playback**: Low-latency WAV/MP3 sound triggering
5. ✅ **MIDI Output**: Send MIDI messages to external devices
6. ✅ **Advanced Logic**: Sequences, conditionals, state management, alternating actions
7. ✅ **Game Controller Integration**: Xbox controller emulation via ViGEm
8. ✅ **Configuration System**: JSON-based profiles with examples
9. ✅ **System Integration**: System tray, profile management, device hot-plugging
10. ✅ **MCP Server**: AI agent integration via Model Context Protocol

**Beta Status Notes**:
- 🚧 Core functionality is stable and ready for regular use
- 🚧 Some UI polish and minor features are still in progress
- 🚧 Feedback and bug reports are welcome to help improve the software

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Dependencies

MIDIFlux relies on the following external libraries:

- **MIDI Support**:
  - [Windows MIDI Services](https://github.com/microsoft/MIDI) - Native Windows 11 24H2+ MIDI API (preferred)
  - [NAudio](https://github.com/naudio/NAudio) - Fallback for older Windows versions (included via NuGet)
- **Game Controller Emulation**:
  - [ViGEm](https://github.com/ViGEm/ViGEmBus) - For Xbox controller emulation
  - [ViGEm.NET](https://github.com/ViGEm/ViGEm.NET) - .NET bindings for ViGEm
  - [ViGEmClient](https://github.com/ViGEm/ViGEmClient) - C/C++ SDK for ViGEm

The ViGEm dependencies are included via the NuGet package `Nefarius.ViGEm.Client`. The source code for these libraries is available at the links above.

### Game Controller Integration

To use the game controller integration features, you need to install the ViGEm Bus Driver:

1. Download and install the [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases)
2. Restart your computer
3. MIDIFlux will automatically detect the driver and enable game controller features

## Community & Support

- **Discord Server**: Join our community for support, feature requests, and discussions: [MIDIFlux Discord](https://discord.gg/bwNBtQCMKR)
- **GitHub Issues**: Report bugs and request features on [GitHub Issues](https://github.com/Cozmopolit/MIDIFlux/issues)
- **Documentation**: Comprehensive guides available in the [Documentation](Documentation/) folder

## Acknowledgments

- [NAudio](https://github.com/naudio/NAudio) for MIDI device access
- [ViGEm](https://github.com/ViGEm/ViGEmBus) for Xbox controller emulation
