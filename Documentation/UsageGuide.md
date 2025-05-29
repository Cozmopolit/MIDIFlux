# MIDIFlux Usage Guide

This guide provides a comprehensive overview of how to use MIDIFlux to map MIDI events to various actions including keyboard input, mouse actions, game controller emulation, system commands, and MIDI output.

## What is MIDIFlux?

MIDIFlux is a powerful MIDI-to-action mapping system that converts MIDI events into:

- **Keyboard Actions**: Key presses, key combinations, and complex sequences
- **Mouse Actions**: Mouse clicks and scroll wheel control
- **Game Controller Actions**: Xbox controller emulation via ViGEm
- **System Actions**: Command execution and delays
- **MIDI Output Actions**: Send MIDI messages to external devices
- **Complex Workflows**: Conditional logic, state management, and action sequences

## Core Concepts

### MIDI Input Types

MIDIFlux supports all standard MIDI message types:

- **Note On/Off**: Piano keys, drum pads, buttons
- **Control Change (CC)**: Knobs, faders, sliders
- **Program Change**: Preset selection
- **Pitch Bend**: Pitch wheels
- **Aftertouch**: Pressure-sensitive keys
- **SysEx**: System exclusive messages

### Action System

MIDIFlux supports a wide range of actions that can be triggered by MIDI events:

- **Simple Actions**: Direct execution for performance (KeyPress, MouseClick, etc.)
- **Complex Actions**: Orchestration and logic (Sequences, Conditionals, State management)
- **Strongly-typed Configuration**: All actions use `$type` discriminators and typed config classes

### State Management

MIDIFlux includes a state management system:

- **User-defined States**: Custom state variables for complex logic
- **Internal States**: Automatic tracking of keyboard keys, controller states
- **Profile-scoped**: States are initialized per profile and cleared on changes
- **Thread-safe**: Concurrent access supported for real-time MIDI processing

## Getting Started

### Installation

1. Download the latest MIDIFlux release from GitHub
2. Extract files to your preferred directory
3. Run `MIDIFlux.exe` or use command line with specific configurations

### First Run

When you first run MIDIFlux:

1. The application scans for available MIDI devices
2. MIDIFlux appears in the system tray
3. Right-click the system tray icon for options:
   - **Show MIDI Devices**: View connected MIDI hardware
   - **Load Configuration**: Select a profile to activate
   - **Configuration Editor**: Create and edit profiles (GUI)
   - **Exit**: Close the application

### Quick Start

1. **Connect your MIDI device** to your computer
2. **Load an example configuration** from the system tray menu
3. **Test the mappings** by pressing keys/controls on your MIDI device
4. **Create your own configuration** using the Configuration Editor or by editing JSON files

## Configuration System

MIDIFlux uses JSON configuration files called "profiles" that define mappings between MIDI events and actions.

### Profile Structure

All profiles use the same configuration format:

```json
{
  "ProfileName": "My Custom Profile",
  "Description": "Description of what this profile does",
  "InitialStates": {
    "UserState1": 0,
    "UserState2": 1
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Description": "Any MIDI device",
      "Mappings": [
        {
          "Id": "unique-mapping-id",
          "Description": "Human-readable description",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 60,
          "Action": {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 77,
            "Description": "Press M key"
          }
        }
      ]
    }
  ]
}
```

### Profile Properties

- **ProfileName**: Unique name for this profile
- **Description**: Optional description of the profile's purpose
- **InitialStates**: Optional user-defined state variables (alphanumeric keys only)
- **MidiDevices**: Array of MIDI device configurations

### MIDI Device Configuration

Each MIDI device entry defines:

- **DeviceName**: Specific device name or "*" for any device
- **Description**: Optional description of this device configuration
- **Mappings**: Array of MIDI event to action mappings

### Mapping Structure

Each mapping defines:

- **Id**: Unique identifier for this mapping
- **Description**: Human-readable description
- **InputType**: MIDI message type (NoteOn, NoteOff, ControlChange, etc.)
- **Channel**: MIDI channel (1-16) or null for any channel
- **Note/ControlNumber**: MIDI note or CC number (0-127)
- **Action**: The action configuration with `$type` discriminator

### Action Configuration

All actions use strongly-typed configuration with `$type` discriminators:

```json
{
  "$type": "KeyPressReleaseConfig",
  "VirtualKeyCode": 65,
  "Description": "Press A key"
}
```

Common action types:
- **KeyPressReleaseConfig**: Press and release a key
- **MouseClickConfig**: Click mouse buttons
- **SequenceConfig**: Execute multiple actions in sequence
- **ConditionalConfig**: Execute actions based on MIDI value
- **CommandExecutionConfig**: Execute shell commands

### Example Configurations

MIDIFlux includes several example configurations in `%AppData%\MIDIFlux\profiles\examples\`:

- **basic-keyboard-shortcuts.json**: Basic keyboard shortcuts
- **game-controller-demo.json**: Game controller emulation
- **command-execution-examples.json**: Shell command examples
- **midi-output-basic.json**: MIDI output examples
- **conditional-action-demo.json**: State-based conditional actions

## Advanced Features

### Multiple MIDI Devices

MIDIFlux supports multiple MIDI devices in a single profile:

```json
{
  "ProfileName": "Multi-Device Setup",
  "MidiDevices": [
    {
      "DeviceName": "Launchpad Pro",
      "Description": "Main controller for keyboard shortcuts",
      "Mappings": [
        {
          "Id": "copy-shortcut",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
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
            "Description": "Copy (Ctrl+C)"
          }
        }
      ]
    },
    {
      "DeviceName": "Traktor Kontrol S2 MK3",
      "Description": "Secondary controller for media control",
      "Mappings": [
        {
          "Id": "media-play-pause",
          "InputType": "NoteOn",
          "Channel": 4,
          "Note": 20,
          "Action": {
            "$type": "KeyPressReleaseConfig",
            "VirtualKeyCode": 32,
            "Description": "Press Space (Play/Pause)"
          }
        }
      ]
    }
  ]
}
```

### Complex Action Sequences

Create sophisticated workflows with SequenceAction:

```json
{
  "Id": "complex-workflow",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 40,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "CommandExecutionConfig",
        "Command": "echo Starting workflow...",
        "ShellType": "CMD",
        "Description": "Log start"
      },
      {
        "$type": "DelayConfig",
        "Milliseconds": 500,
        "Description": "Wait 500ms"
      },
      {
        "$type": "KeyPressReleaseConfig",
        "VirtualKeyCode": 13,
        "Description": "Press Enter"
      },
      {
        "$type": "MidiOutputConfig",
        "OutputDeviceName": "Launchpad Pro",
        "Commands": [
          {
            "MessageType": "NoteOn",
            "Channel": 1,
            "Data1": 60,
            "Data2": 127
          }
        ],
        "Description": "Light up feedback LED"
      }
    ],
    "Description": "Complex multi-action workflow"
  }
}
```

### State-Based Logic

Use stateful actions for complex conditional behaviors:

```json
{
  "ProfileName": "Stateful Control",
  "InitialStates": {
    "Mode": 0,
    "BankNumber": 1
  },
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "mode-switch",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "StateConditionalConfig",
            "Conditions": [
              {
                "StateKey": "Mode",
                "ComparisonType": "Equals",
                "ComparisonValue": 0
              }
            ],
            "LogicType": "Single",
            "ActionIfTrue": {
              "$type": "SetStateConfig",
              "StateKey": "Mode",
              "StateValue": 1,
              "Description": "Switch to Mode 1"
            },
            "ActionIfFalse": {
              "$type": "SetStateConfig",
              "StateKey": "Mode",
              "StateValue": 0,
              "Description": "Switch to Mode 0"
            },
            "Description": "Toggle between modes"
          }
        }
      ]
    }
  ]
}
```

### MIDI Channel Filtering

Configure specific MIDI channels for precise control:

- **Channel**: 1-16 for specific channel
- **Channel**: null for any channel
- Use different channels for different control zones

### Device Hot-Plugging

MIDIFlux supports hot-plugging:

- Connect/disconnect MIDI devices without restarting
- Automatic device detection and reconnection
- Graceful handling of device disconnections

### Logging and Debugging

MIDIFlux provides comprehensive logging in the `Logs` directory:

- **Device Detection**: MIDI devices found and connected
- **Configuration Loading**: Profile validation and loading status
- **MIDI Events**: Real-time MIDI event logging (Debug level)
- **Action Execution**: Action execution results and errors
- **Error Details**: Detailed error messages with stack traces

Log files are automatically rotated to prevent excessive disk usage.

## Troubleshooting

### MIDI Device Issues

**Device Not Detected**:
1. Verify device is properly connected via USB
2. Check if device requires specific drivers
3. Try different USB ports
4. Use "Show MIDI Devices" in system tray to verify detection
5. Restart MIDIFlux if device was connected after startup

**Device Name Mismatch**:
1. Check exact device name in "Show MIDI Devices"
2. Use "*" for any device if specific name doesn't work
3. Device names are case-sensitive
4. Some devices may have different names in different modes

### Configuration Issues

**Profile Not Loading**:
1. Check JSON syntax for errors (use JSON validator)
2. Verify all required properties are present
3. Check log files for specific validation errors
4. Ensure `$type` discriminators are correct

**Actions Not Executing**:
1. Enable debug logging to see MIDI events
2. Verify MIDI channel settings (1-16 or null)
3. Check MIDI note/CC numbers match your device
4. Ensure target applications have focus for keyboard actions
5. Verify virtual key codes are correct

**State-Related Issues**:
1. Check that state keys are alphanumeric only
2. Verify initial states are defined in profile
3. Use debug logging to track state changes
4. Remember states are cleared on profile changes

### Performance Issues

**High Latency**:
1. Avoid long-running commands in sequences
2. Use `WaitForExit: false` for background commands
3. Minimize complex nested sequences
4. Check system performance during MIDI processing

**Memory Usage**:
1. Log files are automatically rotated
2. States are cleaned up on profile changes
3. Restart application if memory usage grows excessively

### Game Controller Issues

**ViGEm Not Working**:
1. Install ViGEm Bus Driver from official GitHub
2. Restart computer after ViGEm installation
3. Run MIDIFlux as administrator if needed
4. Check Windows Game Controllers in Control Panel

**Controller Not Recognized in Games**:
1. Verify controller appears in Windows Game Controllers
2. Some games require specific controller types
3. Try different ControllerIndex values (0-3)
4. Close other controller emulation software

### MIDI Output Issues

**Output Device Not Found**:
1. Use exact device name (case-sensitive)
2. Verify device is connected and recognized by Windows
3. Check device name in Windows MIDI settings
4. No wildcards supported for output devices

**Messages Not Sent**:
1. Verify MIDI channel is 1-16 (not 0-15)
2. Check Data1 and Data2 values are 0-127
3. Ensure output device supports the message type
4. Use debug logging to verify message sending

## Common Use Cases

### Productivity
- **Keyboard Shortcuts**: Map MIDI pads to Ctrl+C, Ctrl+V, Ctrl+Z
- **Application Switching**: Map MIDI buttons to Alt+Tab
- **Media Control**: Map MIDI faders to volume, play/pause
- **Window Management**: Map MIDI controls to window positioning

### Creative Applications
- **DAW Control**: Map MIDI controllers to transport controls
- **Video Editing**: Map MIDI faders to timeline scrubbing
- **Live Performance**: Map MIDI pads to scene triggers
- **Streaming**: Map MIDI buttons to OBS scene switching

### Gaming
- **Controller Emulation**: Use MIDI devices as Xbox controllers
- **Macro Execution**: Complex game action sequences
- **Communication**: Quick chat messages and voice activation
- **Accessibility**: Large MIDI pads for easier game control

### System Administration
- **Command Execution**: Run system commands via MIDI triggers
- **Process Management**: Start/stop applications
- **Network Operations**: Trigger network diagnostics
- **Backup Operations**: Automated backup workflows

## Best Practices

### Configuration Design
1. **Use Descriptive IDs**: Clear, unique identifiers for all mappings
2. **Document Everything**: Add descriptions to all actions and mappings
3. **Start Simple**: Begin with basic mappings before complex sequences
4. **Test Incrementally**: Test each mapping individually before combining

### Performance Optimization
1. **Minimize Delays**: Use delays sparingly in sequences
2. **Optimize Hot Paths**: Use simple actions for frequently triggered events
3. **Batch Operations**: Group related actions in sequences
4. **Monitor Resources**: Check CPU and memory usage during operation

### Maintenance
1. **Version Control**: Keep configuration files in version control
2. **Backup Configurations**: Save working configurations before changes
3. **Regular Testing**: Verify configurations work after system updates
4. **Documentation**: Maintain documentation for complex setups

### Security
1. **Validate Commands**: Never execute untrusted shell commands
2. **Limit Permissions**: Run MIDIFlux with minimal required permissions
3. **Review Configurations**: Audit configurations from external sources
4. **Monitor Logs**: Check logs for unexpected behavior

## Getting Help

### Resources
- **Documentation**: Complete action type documentation in `Documentation/ActionTypes/`
- **Examples**: Working examples in `%AppData%\MIDIFlux\profiles\examples\`
- **Logs**: Detailed execution logs in `%AppData%\MIDIFlux\Logs\`
- **GitHub Issues**: Report bugs and request features

### Community Support
- **GitHub Discussions**: Ask questions and share configurations
- **Example Sharing**: Contribute working configurations for others
- **Bug Reports**: Report issues with detailed reproduction steps
- **Feature Requests**: Suggest improvements and new functionality

