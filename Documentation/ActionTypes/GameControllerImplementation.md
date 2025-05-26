# Game Controller Implementation in MIDIFlux

This document provides technical details about the implementation of Xbox controller emulation in MIDIFlux using the ViGEm framework and the unified action system.

## Overview

MIDIFlux emulates Xbox 360 controllers through the unified action system using ViGEm framework. This allows MIDI controllers to be used with games that support Xbox controllers. This feature is optional and only available when the ViGEm Bus Driver is installed.

## Architecture Overview

### Unified Action System Integration

Game controller functionality is implemented through the unified action system:

- **GameControllerButtonAction**: Handles button press/release events
- **GameControllerAxisAction**: Handles analog axis control
- **Strongly-typed Configuration**: Uses `GameControllerButtonConfig` and `GameControllerAxisConfig`
- **ViGEm Integration**: Seamless integration with ViGEm Bus Driver

### Dependencies

- **Nefarius.ViGEm.Client**: .NET wrapper for the ViGEm Bus Driver
- **ViGEm Bus Driver**: Required system driver for virtual controller emulation
- **MIDIFlux.Core.Actions**: Unified action system integration

## Action Implementation

### GameControllerButtonAction

Implements `IAction` interface for Xbox controller button simulation:

```csharp
public class GameControllerButtonAction : IAction
{
    public string Id { get; }
    public string Description { get; }

    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        // Button press/release logic with unified async execution
        // Returns ValueTask.CompletedTask for synchronous operations
        return ValueTask.CompletedTask;
    }
}
```

**Key Features**:
- Unified async execution model with minimal latency
- Automatic ViGEm availability checking
- Support for all Xbox 360 controller buttons
- Multiple controller support (0-3 controller indices)

### GameControllerAxisAction

Implements `IAction` interface for Xbox controller axis control:

```csharp
public class GameControllerAxisAction : IAction
{
    public string Id { get; }
    public string Description { get; }

    public ValueTask ExecuteAsync(int? midiValue = null)
    {
        // MIDI value to axis value conversion
        // Real-time axis updates with unified async execution
        return ValueTask.CompletedTask;
    }
}
```

**Key Features**:
- Real-time MIDI value to axis value conversion
- Support for all Xbox 360 controller axes and triggers
- Value range mapping (MIDI 0-127 to controller ranges)
- Multiple controller support

## Configuration System

### Current Unified Format

Game controller actions use the unified configuration format with `$type` discriminators:

```json
{
  "ProfileName": "Game Controller Profile",
  "MidiDevices": [
    {
      "DeviceName": "*",
      "Mappings": [
        {
          "Id": "button-a",
          "Description": "Xbox A button",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "A",
            "ControllerIndex": 0,
            "Description": "Press A button"
          }
        },
        {
          "Id": "left-stick-x",
          "Description": "Left thumbstick X-axis",
          "InputType": "ControlChange",
          "Channel": 1,
          "ControlNumber": 1,
          "Action": {
            "$type": "GameControllerAxisConfig",
            "Axis": "LeftThumbX",
            "ControllerIndex": 0,
            "Description": "Control left thumbstick horizontal"
          }
        }
      ]
    }
  ]
}
```

### Configuration Properties

#### GameControllerButtonConfig
- **Button**: Xbox controller button name (A, B, X, Y, etc.)
- **ControllerIndex**: Controller index (0-3) for multiple controllers
- **Description**: Optional human-readable description

#### GameControllerAxisConfig
- **Axis**: Xbox controller axis name (LeftThumbX, RightThumbY, etc.)
- **ControllerIndex**: Controller index (0-3) for multiple controllers
- **Description**: Optional human-readable description

### Multiple Controllers

MIDIFlux supports up to 4 virtual Xbox controllers simultaneously:

```json
{
  "ProfileName": "Multi-Controller Setup",
  "MidiDevices": [
    {
      "DeviceName": "Controller 1 Device",
      "Mappings": [
        {
          "Id": "p1-jump",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "A",
            "ControllerIndex": 0,
            "Description": "Player 1 jump"
          }
        }
      ]
    },
    {
      "DeviceName": "Controller 2 Device",
      "Mappings": [
        {
          "Id": "p2-jump",
          "InputType": "NoteOn",
          "Channel": 1,
          "Note": 36,
          "Action": {
            "$type": "GameControllerButtonConfig",
            "Button": "A",
            "ControllerIndex": 1,
            "Description": "Player 2 jump"
          }
        }
      ]
    }
  ]
}
```

## Technical Implementation

### ViGEm Integration

**Initialization Process**:
1. GameController actions attempt to initialize ViGEm client during construction
2. If ViGEm Bus Driver is not installed, initialization fails gracefully
3. Actions check ViGEm availability before attempting controller operations
4. Error handling provides clear feedback when ViGEm is unavailable

**Controller Management**:
- Virtual controllers are created per ControllerIndex
- Controllers are automatically connected when first used
- Proper cleanup and disconnection on application shutdown
- Thread-safe controller state management

### Value Conversion

**MIDI to Controller Value Mapping**:
- **Axes**: MIDI values (0-127) → Controller axes (-32768 to 32767)
- **Triggers**: MIDI values (0-127) → Trigger values (0-255)
- **Buttons**: MIDI Note On/Off → Button press/release

**Conversion Formula**:
```csharp
// For axes (centered at 0)
short axisValue = (short)((midiValue - 64) * 512);

// For triggers (0-based)
byte triggerValue = (byte)(midiValue * 2);
```

## Supported Controller Elements

### Buttons
- **Face Buttons**: A, B, X, Y
- **Shoulder Buttons**: LeftShoulder, RightShoulder
- **System Buttons**: Back, Start, Guide
- **Thumbstick Buttons**: LeftThumb, RightThumb
- **D-Pad**: DPadUp, DPadDown, DPadLeft, DPadRight

### Axes
- **Left Thumbstick**: LeftThumbX, LeftThumbY
- **Right Thumbstick**: RightThumbX, RightThumbY
- **Triggers**: LeftTrigger, RightTrigger

## Performance Considerations

### Execution Model
- **Synchronous Execution**: Game controller actions execute synchronously for minimal latency
- **Real-time Updates**: Controller state is updated immediately on MIDI events
- **Efficient Value Conversion**: Optimized MIDI-to-controller value conversion
- **Memory Efficient**: Minimal memory allocation during execution

### Threading
- **Thread-safe Operations**: Controller state updates are thread-safe
- **Main Thread Execution**: Actions execute on main thread for timing accuracy
- **Background Cleanup**: Controller cleanup handled on background threads

## Error Handling

### Robust Error Management
- **Graceful Degradation**: Actions fail gracefully when ViGEm is unavailable
- **Comprehensive Logging**: All errors and warnings are logged with details
- **Validation**: Button and axis names are validated at configuration load time
- **Exception Handling**: Protected against invalid configurations and runtime errors

### Error Scenarios
- **ViGEm Not Available**: Clear error messages with installation instructions
- **Invalid Button Names**: Configuration validation with specific error details
- **Invalid Axis Names**: Validation with supported axis list
- **Controller Index Out of Range**: Validation for 0-3 range
- **MIDI Value Out of Range**: Automatic clamping to valid ranges

## Integration with Action System

### Action Factory Integration
Game controller actions are created through the unified ActionFactory:

```csharp
// Button action creation
var buttonAction = ActionFactory.CreateAction(new GameControllerButtonConfig
{
    Button = "A",
    ControllerIndex = 0,
    Description = "Press A button"
});

// Axis action creation
var axisAction = ActionFactory.CreateAction(new GameControllerAxisConfig
{
    Axis = "LeftThumbX",
    ControllerIndex = 0,
    Description = "Control left thumbstick X"
});
```

### Sequence Integration
Game controller actions can be used in sequences with other actions:

```json
{
  "Id": "complex-game-action",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 40,
  "Action": {
    "$type": "SequenceConfig",
    "SubActions": [
      {
        "$type": "GameControllerButtonConfig",
        "Button": "A",
        "ControllerIndex": 0,
        "Description": "Press A"
      },
      {
        "$type": "DelayConfig",
        "Milliseconds": 100,
        "Description": "Wait 100ms"
      },
      {
        "$type": "GameControllerButtonConfig",
        "Button": "B",
        "ControllerIndex": 0,
        "Description": "Press B"
      }
    ],
    "Description": "A-B combo sequence"
  }
}
```

## Testing and Validation

### Automated Testing
- **Unit Tests**: Individual action functionality
- **Integration Tests**: ViGEm integration testing
- **Configuration Tests**: Validation of configuration parsing
- **Error Condition Tests**: Testing graceful failure scenarios

### Manual Testing
- **Hardware Compatibility**: Tested with various MIDI controllers
- **Game Compatibility**: Tested with multiple Xbox controller-compatible games
- **Multi-Controller**: Tested with multiple simultaneous virtual controllers
- **Performance Testing**: Latency and throughput testing

## Troubleshooting

### Common Issues

**ViGEm Related**:
- **Controller Not Detected**: Install ViGEm Bus Driver from official GitHub
- **Driver Installation**: Restart computer after ViGEm installation
- **Permission Issues**: Run MIDIFlux as administrator if needed
- **Version Compatibility**: Ensure latest ViGEm Bus Driver version

**Configuration Issues**:
- **Invalid Button Names**: Check supported button list in documentation
- **Invalid Axis Names**: Verify axis names match Xbox controller specification
- **Controller Index**: Ensure ControllerIndex is 0-3
- **MIDI Mapping**: Verify MIDI note/CC numbers match device output

**Game Compatibility**:
- **Controller Not Recognized**: Verify controller appears in Windows Game Controllers
- **Input Lag**: Check system performance and MIDI buffer settings
- **Button Mapping**: Some games allow custom controller button mapping
- **Multiple Controllers**: Ensure games support multiple controllers

### Debugging Tools

**Windows Game Controllers**:
1. Open Control Panel → Game Controllers
2. Verify virtual Xbox controllers appear
3. Test button and axis functionality
4. Check controller properties and calibration

**MIDIFlux Logging**:
- Enable debug logging to see MIDI events
- Check controller action execution logs
- Monitor ViGEm initialization messages
- Review error messages for specific issues

## Future Enhancements

### Planned Features
1. **DualShock 4 Support**: PlayStation controller emulation
2. **Advanced Macros**: Complex button combination sequences
3. **Haptic Feedback**: Rumble/vibration support through MIDI output
4. **Configuration GUI**: Visual controller mapping interface
5. **Profile Templates**: Pre-configured templates for popular games

### Performance Improvements
1. **Optimized Value Conversion**: Further latency reduction
2. **Batch Updates**: Multiple controller updates in single operation
3. **Memory Optimization**: Reduced memory allocation during execution
4. **Threading Optimization**: Improved multi-threading for multiple controllers

