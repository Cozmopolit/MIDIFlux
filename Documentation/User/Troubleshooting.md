# Troubleshooting Guide

Common issues and solutions for MIDIFlux. Check logs at `%AppData%\MIDIFlux\Logs\` for detailed error information.

## MIDI Device Issues

### Device Not Detected
**Symptoms**: Device doesn't appear in "Show MIDI Devices" menu

**Solutions**:
1. Verify device is properly connected via USB
2. Check if device requires specific drivers
3. Try different USB ports
4. Restart MIDIFlux if device was connected after startup
5. Check Windows Device Manager for MIDI device recognition

### Device Name Mismatch
**Symptoms**: Configuration loads but no response from device

**Solutions**:
1. Check exact device name in "Show MIDI Devices" menu
2. Use "*" for any device if specific name doesn't work
3. Device names are case-sensitive
4. Some devices may have different names in different modes

### MIDI Channel Issues
**Symptoms**: Some controls work, others don't

**Solutions**:
1. Check that device is sending on expected channel (1-16)
2. Verify channel number in configuration matches device
3. Use null for any channel if unsure
4. Enable debug logging to see actual channel numbers
5. Remember MIDIFlux uses 1-based channels (1-16), not 0-based (0-15)

## Configuration Issues

### Profile Not Loading
**Symptoms**: Error message when loading profile, or profile loads but no actions work

**Solutions**:
1. Check JSON syntax for errors (use JSON validator)
2. Verify all required properties are present
3. Check log files for specific validation errors
4. Ensure `$type` discriminators are correct and match action types
5. Validate that all action parameters are properly formatted

### Actions Not Executing
**Symptoms**: MIDI events received but no keyboard/mouse/controller actions occur

**Solutions**:
1. Enable debug logging to see MIDI events
2. Verify MIDI channel settings (1-16 or null)
3. Check MIDI note/CC numbers match your device
4. Ensure target applications have focus for keyboard actions
5. Verify virtual key codes are correct (case-sensitive)
6. Check that action parameters are valid

### State-Related Issues
**Symptoms**: Conditional actions not working, state values not changing

**Solutions**:
1. Check that state keys are alphanumeric only
2. Verify initial states are defined in profile `InitialStates`
3. Use debug logging to track state changes
4. Remember states are cleared on profile changes
5. Ensure state keys don't conflict with internal state keys (avoid asterisk prefix)

## Performance Issues

### High Latency
**Symptoms**: Noticeable delay between MIDI input and action execution

**Solutions**:
1. Avoid long-running commands in sequences
2. Use `WaitForExit: false` for background commands
3. Minimize complex nested sequences
4. Check system performance during MIDI processing
5. Reduce number of simultaneous MIDI events

### Memory Usage
**Symptoms**: Application memory usage grows over time

**Solutions**:
1. Log files are automatically rotated
2. States are cleaned up on profile changes
3. Restart application if memory usage grows excessively
4. Check for memory leaks in custom command executions

## Game Controller Issues

### ViGEm Not Working
**Symptoms**: Game controller actions configured but no controller appears in games

**Solutions**:
1. Install ViGEm Bus Driver from [official GitHub](https://github.com/ViGEm/ViGEmBus/releases)
2. Restart computer after ViGEm installation
3. Run MIDIFlux as administrator if needed
4. Check Windows Game Controllers in Control Panel
5. Verify ViGEm service is running in Windows Services

### Controller Not Recognized in Games
**Symptoms**: Controller appears in Windows but games don't recognize it

**Solutions**:
1. Verify controller appears in Windows Game Controllers
2. Some games require specific controller types
3. Try different ControllerIndex values (0-3)
4. Close other controller emulation software
5. Test with different games to isolate issue

## MIDI Output Issues

### Output Device Not Found
**Symptoms**: Error messages about MIDI output device not available

**Solutions**:
1. Use exact device name (case-sensitive)
2. Verify device is connected and recognized by Windows
3. Check device name in Windows MIDI settings
4. No wildcards supported for output devices
5. Ensure device supports MIDI input (not just output)

### Messages Not Sent
**Symptoms**: No response from external MIDI device

**Solutions**:
1. Verify MIDI channel is 1-16 (not 0-15)
2. Check Data1 and Data2 values are 0-127
3. Ensure output device supports the message type
4. Use debug logging to verify message sending
5. Test with MIDI monitor software to confirm messages

## Audio Issues

### Sound Files Not Playing
**Symptoms**: PlaySound actions configured but no audio output

**Solutions**:
1. Verify file path is correct (absolute or relative to `%AppData%\MIDIFlux\sounds\`)
2. Check file format is supported (WAV, MP3)
3. Ensure audio file is not corrupted
4. Check Windows audio settings and default playback device
5. Verify file permissions allow reading

### Audio Latency
**Symptoms**: Noticeable delay between MIDI trigger and sound playback

**Solutions**:
1. Use WAV files instead of MP3 for lower latency
2. Ensure audio files are pre-loaded (happens automatically)
3. Check system audio buffer settings
4. Close other audio applications that might interfere

## Keyboard/Mouse Issues

### Keys Not Working
**Symptoms**: Keyboard actions configured but no key presses occur

**Solutions**:
1. Verify virtual key code is correct and case-sensitive
2. Check that target application has focus
3. Ensure MIDI events are being received (check logs)
4. Test with simple applications like Notepad
5. Check for conflicting keyboard software

### Mouse Actions Not Working
**Symptoms**: Mouse click/scroll actions don't work

**Solutions**:
1. Verify mouse button names are correct ("Left", "Right", "Middle")
2. Check scroll direction values ("Up", "Down", "Left", "Right")
3. Ensure target application accepts mouse input
4. Test with different applications to isolate issue

## Command Execution Issues

### Commands Not Running
**Symptoms**: CommandExecution actions don't execute shell commands

**Solutions**:
1. Test commands manually in PowerShell/Command Prompt first
2. Check command syntax and parameters
3. Verify shell type is correct ("PowerShell" or "CommandPrompt")
4. Use absolute paths for executables
5. Check Windows execution policies for PowerShell

### Command Errors
**Symptoms**: Commands run but produce errors

**Solutions**:
1. Check log files for detailed error messages
2. Verify command has necessary permissions
3. Use `RunHidden: false` to see command output
4. Test with simple commands first (e.g., "echo test")

## Debugging Steps

### Enable Debug Logging
1. Edit `%AppData%\MIDIFlux\appsettings.json`
2. Set logging level to "Debug"
3. Restart MIDIFlux
4. Check logs for detailed event information

### Check Log Files
- **Location**: `%AppData%\MIDIFlux\Logs\`
- **Current Log**: Most recent `.log` file
- **Error Details**: Look for ERROR and WARN level messages
- **MIDI Events**: Debug level shows all MIDI events received

### Test Incrementally
1. Start with simple configurations
2. Test one mapping at a time
3. Add complexity gradually
4. Verify each step works before proceeding

### Validate Configuration
1. Use JSON validator to check syntax
2. Compare with working example files
3. Check all required properties are present
4. Verify `$type` discriminators match action types

## Getting Help

### Before Reporting Issues
1. Check this troubleshooting guide
2. Review example configurations for working patterns
3. Enable debug logging and check log files
4. Test with minimal configuration to isolate issue

### Reporting Bugs
Include in your report:
- MIDIFlux version
- Windows version
- MIDI device model
- Configuration file (if relevant)
- Log files showing the issue
- Steps to reproduce

### Resources
- **Example Files**: `%AppData%\MIDIFlux\profiles\examples\`
- **Log Files**: `%AppData%\MIDIFlux\Logs\`
- **GitHub Issues**: [Report bugs and request features](https://github.com/Cozmopolit/MIDIFlux/issues)
- **Documentation**: [Getting Started](Getting_Started.md), [Action Reference](Action_Reference.md)
