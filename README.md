# MIDIFlux

[![Release](https://img.shields.io/github/v/release/Cozmopolit/MIDIFlux?include_prereleases&label=release&color=blue)](https://github.com/Cozmopolit/MIDIFlux/releases)
[![Build Status](https://img.shields.io/github/actions/workflow/status/Cozmopolit/MIDIFlux/build.yml?branch=main)](https://github.com/Cozmopolit/MIDIFlux/actions)
[![License](https://img.shields.io/github/license/Cozmopolit/MIDIFlux)](LICENSE)
[![Platform](https://img.shields.io/badge/platform-Windows-blue)](https://github.com/Cozmopolit/MIDIFlux/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/download/dotnet/10.0)

> **� Pre-Release Candidate** – MIDIFlux is stabilizing toward v1.0. Core functionality is production-ready; feedback welcome!

> ⚠️ **Windows 11 Users**: Microsoft is rolling out a new MIDI driver (KB5074105, Jan–Mar 2026) that breaks legacy MIDI applications. MIDIFlux supports the new stack natively — install the [Windows MIDI Services Runtime](https://github.com/microsoft/MIDI/releases) to ensure your MIDI devices work.

MIDIFlux is a Windows application that transforms any MIDI device—foot pedals, keyboards, control surfaces, or custom controllers—into a versatile computer controller. Map MIDI events to keyboard shortcuts, mouse actions, game controller inputs, audio playback, system commands, and more.

## Quick Start

1. **Download** the [latest release](https://github.com/Cozmopolit/MIDIFlux/releases/latest) – no installation required
2. **Run** the executable and find MIDIFlux in your **system tray**
3. **Load an example profile** to get started

See [Installation & Usage](#installation--usage) for details.

## Features

- **🎹 Full MIDI Support** – Any MIDI input device, with hot-plugging and channel filtering. Native [Windows MIDI Services](https://github.com/microsoft/MIDI) on Windows 11 24H2+, [NAudio](https://github.com/naudio/NAudio) fallback for older systems.
- **⌨️ Keyboard & Mouse** – Map notes/controls to key combinations, mouse clicks, scroll wheel, and media keys
- **🎮 Game Controllers** – Emulate Xbox controllers via [ViGEm](https://github.com/ViGEm/ViGEmBus) (buttons, axes, triggers)
- **🔊 Audio Playback** – Trigger WAV/MP3 sound effects with low latency
- **🎵 MIDI Output** – Send MIDI messages to external devices
- **💻 System Commands** – Execute shell commands and scripts
- **🔄 Advanced Logic** – Sequences, conditionals, state management, alternating actions, and complex macros
- **🤖 MCP Server** – Built-in [Model Context Protocol](https://modelcontextprotocol.io/) server for AI-assisted configuration
- **📋 Multiple Profiles** – Create, switch, and manage mapping profiles from the system tray
- **🔧 Relative Controls** – Support for jog wheels and other relative MIDI controls

## System Requirements

- **OS**: Windows 10/11 (x64)
- **Hardware**: One or more MIDI input devices
- **Windows 11 24H2/25H2**: Install the [Windows MIDI Services Runtime](https://github.com/microsoft/MIDI/releases) — Microsoft is replacing the legacy MIDI driver via update KB5074105 (phased rollout, Jan–Mar 2026). MIDIFlux supports the new stack natively, but the runtime must be installed separately.
- **Optional**: [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases) for game controller emulation

## Installation & Usage

### Getting Started

1. **Download** the latest release from the [Releases page](https://github.com/Cozmopolit/MIDIFlux/releases)
2. **Extract** the executable to any folder (portable)
3. **Run** the executable (e.g., `MIDIFlux-v0.9.2-win-x64.exe`)

> ⚠️ **Windows SmartScreen Warning**: On first launch, Windows may show a "Windows protected your PC" warning. This is expected – MIDIFlux is not code-signed (code signing certificates are expensive for open-source projects). Click **"More info"** → **"Run anyway"** to proceed. The full source code is available in this repository for review.

4. **Right-click the tray icon** → Load Profile → Choose an example profile
5. **Connect your MIDI device** and start using it!

MIDIFlux creates example profiles on first run in `%AppData%\MIDIFlux\profiles\examples\`.

### Configuration

MIDIFlux uses JSON configuration files. Start with the provided examples and customize them for your needs.

- [Getting Started Guide](Documentation/User/Getting_Started.md) – Installation, first profile, device setup
- [Action Reference](Documentation/User/Action_Reference.md) – Complete reference for all action types

## Community & Support

- **Discord**: [MIDIFlux Discord Server](https://discord.gg/J5ksw53rAg) – support, feature requests, discussions
- **Issues**: [GitHub Issues](https://github.com/Cozmopolit/MIDIFlux/issues) – bug reports and feature requests
- **Docs**: [Documentation](Documentation/) folder – guides, action reference, and [developer documentation](Documentation/Developer/Developer_Guide.md)

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

## License

This project is licensed under the MIT License – see the [LICENSE](LICENSE) file for details.
