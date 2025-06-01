# Getting Started with MIDIFlux

MIDIFlux transforms MIDI input devices into versatile computer controllers. Connect any MIDI device and map it to keyboard shortcuts, mouse actions, game controller inputs, system commands, and more.

## Quick Start

1. **Download and Run**
   - Download the latest release from [GitHub Releases](https://github.com/Cozmopolit/MIDIFlux/releases/latest)
   - Extract and run the executable (no installation required!)
   - Find MIDIFlux in your system tray

2. **Connect Your MIDI Device**
   - Connect your MIDI device via USB
   - Right-click the system tray icon → "Show MIDI Devices" to verify detection

3. **Load an Example Profile**
   - Right-click the system tray icon → "Load Profile"
   - Choose from the example configurations (see below)
   - Test by pressing keys/controls on your MIDI device

## System Requirements

- **Operating System**: Windows 10/11 (x64)
- **Hardware**: One or more MIDI input devices
- **Dependencies**: None (all .NET dependencies included)
- **Optional**: ViGEm Bus Driver (for game controller emulation only)

## Example Configurations

MIDIFlux automatically installs example configurations to `%AppData%\MIDIFlux\profiles\examples\`:

### Start Here
- **basic-keyboard-shortcuts.json**: Copy/paste/cut shortcuts - perfect first example
- **system-controls.json**: Media controls (play/pause, track navigation)
- **all-action-types-demo.json**: Comprehensive showcase of every action type

### Advanced Examples
- **game-controller-demo.json**: Xbox controller emulation (requires ViGEm)
- **advanced-macros.json**: Complex action sequences and workflows
- **conditional-action-demo.json**: MIDI value-based conditional logic

### Specialized Use Cases
- **command-execution-examples.json**: Shell command execution patterns
- **midi-output-basic.json**: MIDI output to external devices
- **multi-channel-demo.json**: Multiple MIDI channel configurations

## Basic Configuration Concepts

### Profile Structure
- **Profiles**: JSON files that define MIDI device mappings
- **Devices**: Configure specific devices or use "*" for any device
- **Mappings**: Define which MIDI events trigger which actions
- **Actions**: What happens when a MIDI event occurs

### MIDI Input Types
- **NoteOn/NoteOff**: Piano keys, drum pads, buttons
- **ControlChange**: Knobs, faders, sliders
- **ProgramChange**: Preset selection
- **PitchBend**: Pitch wheels

### Action Types
- **Simple Actions**: KeyPress, MouseClick, CommandExecution
- **Complex Actions**: Sequences, Conditionals, State management

## Common Use Cases

### Productivity
- Map MIDI pads to Ctrl+C, Ctrl+V, Ctrl+Z shortcuts
- Use MIDI pedals for hands-free operations
- Create custom keyboard layouts with MIDI controllers

### Creative Applications
- Control DAW transport with MIDI buttons
- Map MIDI faders to timeline scrubbing
- Trigger scene changes in streaming software

### Gaming
- Use MIDI devices as Xbox controllers (via ViGEm)
- Create macro sequences for complex game actions
- Large MIDI pads for accessibility

### System Administration
- Execute system commands via MIDI triggers
- Automate backup workflows
- Quick access to diagnostic tools

## Configuration Workflow

1. **Start with Examples**: Load and test example configurations
2. **Understand the Pattern**: Examine how examples map MIDI events to actions
3. **Create Custom Profiles**: Use the Configuration Editor or edit JSON files
4. **Test Incrementally**: Add one mapping at a time and test
5. **Use Logging**: Enable debug logging to troubleshoot issues

## System Tray Menu

Right-click the MIDIFlux system tray icon for:
- **Show MIDI Devices**: View connected MIDI hardware
- **Load Profile**: Select and activate a configuration
- **Configuration Editor**: Create and edit profiles (GUI)
- **Exit**: Close the application

## File Locations

- **Configuration Files**: `%AppData%\MIDIFlux\profiles\`
- **Example Files**: `%AppData%\MIDIFlux\profiles\examples\`
- **Application Logs**: `%AppData%\MIDIFlux\Logs\`
- **Settings**: `%AppData%\MIDIFlux\appsettings.json`

## Next Steps

### Learn More
- **[Action Reference](ACTION_REFERENCE.md)**: Complete guide to all action types
- **[Developer Guide](DEVELOPER_GUIDE.md)**: Building from source and contributing
- **[Troubleshooting](Troubleshooting.md)**: Common issues and solutions

### Get Help
- **GitHub Issues**: Report bugs and request features
- **Example Files**: Study working configurations in the examples folder
- **Logs**: Check `%AppData%\MIDIFlux\Logs\` for detailed execution information

### Special Requirements

#### Game Controller Features
To use Xbox controller emulation:
1. Download and install [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases)
2. Restart your computer
3. Load `game-controller-demo.json` to test functionality

#### Advanced Features
- **State Management**: Use conditional logic based on custom state variables
- **Multi-Device Setups**: Configure multiple MIDI devices in a single profile
- **MIDI Output**: Send MIDI messages to external devices
- **Complex Sequences**: Chain multiple actions with timing and conditions

## Tips for Success

1. **Start Simple**: Begin with basic keyboard shortcuts before complex sequences
2. **Use Descriptive Names**: Clear mapping descriptions help with maintenance
3. **Test Incrementally**: Verify each mapping works before adding more
4. **Keep Backups**: Save working configurations before making changes
5. **Monitor Logs**: Use logging to understand what's happening during execution

---

**Ready to get started?** Load `basic-keyboard-shortcuts.json` from the system tray menu and press some keys on your MIDI device!
