# Command Execution Actions

MIDIFlux supports executing shell commands (PowerShell or Command Prompt) through the CommandExecutionAction. This enables automation, application launching, and system integration triggered by MIDI events.

## CommandExecutionAction

Executes shell commands with configurable shell type, visibility, and execution behavior.

**Configuration Type**: `CommandExecutionConfig`

**Supported Shells**:
- **PowerShell**: Modern Windows shell with advanced scripting capabilities
- **CommandPrompt**: Traditional Windows command prompt

## Configuration Format

```json
{
  "$type": "CommandExecutionConfig",
  "Command": "Get-Date",
  "ShellType": "PowerShell",
  "RunHidden": false,
  "WaitForExit": true,
  "Description": "Display current date and time"
}
```

## Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `Command` | string | The command to execute |
| `ShellType` | string | `"PowerShell"` or `"CommandPrompt"` |
| `RunHidden` | bool | Whether to hide the console window |
| `WaitForExit` | bool | Whether to wait for command completion |
| `Description` | string | Optional description of the action |

### PowerShell

For PowerShell commands, set `ShellType` to `"PowerShell"`. Commands are executed using:

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "your command"
```

**PowerShell Configuration Example**:
```json
{
  "$type": "CommandExecutionConfig",
  "Command": "Get-Process | Sort-Object CPU -Descending | Select-Object -First 5",
  "ShellType": "PowerShell",
  "RunHidden": false,
  "WaitForExit": true,
  "Description": "Show top 5 CPU-consuming processes"
}
```

### Command Prompt

For Command Prompt commands, set `ShellType` to `"CommandPrompt"`. Commands are executed using:

```
cmd.exe /c your command
```

**Command Prompt Configuration Example**:
```json
{
  "$type": "CommandExecutionConfig",
  "Command": "dir /s",
  "ShellType": "CommandPrompt",
  "RunHidden": false,
  "WaitForExit": true,
  "Description": "List all files in current directory and subdirectories"
}
```

## Complete Mapping Examples

### Basic Command Execution

```json
{
  "Id": "show-date",
  "Description": "Display current date and time",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 36,
  "Action": {
    "$type": "CommandExecutionConfig",
    "Command": "Get-Date",
    "ShellType": "PowerShell",
    "RunHidden": false,
    "WaitForExit": true,
    "Description": "Show current date and time"
  }
}
```

### Hidden Command Execution

```json
{
  "Id": "launch-notepad",
  "Description": "Launch Notepad silently",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 37,
  "Action": {
    "$type": "CommandExecutionConfig",
    "Command": "Start-Process notepad",
    "ShellType": "PowerShell",
    "RunHidden": true,
    "WaitForExit": false,
    "Description": "Launch Notepad without showing console"
  }
}
```

### System Information Commands

```json
{
  "Id": "system-info",
  "Description": "Display system information",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 38,
  "Action": {
    "$type": "CommandExecutionConfig",
    "Command": "systeminfo | findstr /C:\"Total Physical Memory\"",
    "ShellType": "CommandPrompt",
    "RunHidden": false,
    "WaitForExit": true,
    "Description": "Show total physical memory"
  }
}
```

### File Operations

```json
{
  "Id": "backup-documents",
  "Description": "Backup documents folder",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 39,
  "Action": {
    "$type": "CommandExecutionConfig",
    "Command": "Copy-Item -Path \"$env:USERPROFILE\\Documents\" -Destination \"C:\\Backup\\Documents_$(Get-Date -Format 'yyyyMMdd')\" -Recurse",
    "ShellType": "PowerShell",
    "RunHidden": true,
    "WaitForExit": true,
    "Description": "Backup documents to dated folder"
  }
}
```

### Network Operations

```json
{
  "Id": "network-test",
  "Description": "Test network connectivity",
  "InputType": "NoteOn",
  "Channel": 1,
  "Note": 40,
  "Action": {
    "$type": "CommandExecutionConfig",
    "Command": "ping -n 4 8.8.8.8",
    "ShellType": "CommandPrompt",
    "RunHidden": false,
    "WaitForExit": true,
    "Description": "Ping Google DNS server"
  }
}
```

## Use Cases

### System Administration
- **Process Management**: Kill processes, check system status
- **File Operations**: Backup files, clean temporary directories
- **Network Diagnostics**: Test connectivity, check network status
- **Service Management**: Start/stop Windows services

### Application Integration
- **Launch Applications**: Start specific programs with parameters
- **Script Execution**: Run custom PowerShell or batch scripts
- **Automation**: Trigger automated workflows and tasks
- **Development Tools**: Build projects, run tests, deploy applications

### Media Production
- **File Processing**: Convert media files, batch operations
- **Backup Operations**: Automated project backups
- **System Monitoring**: Check disk space, memory usage
- **External Tool Integration**: Launch audio/video processing tools

### Gaming and Streaming
- **OBS Control**: Start/stop recording, switch scenes (via command line)
- **Game Launchers**: Start specific games with parameters
- **System Optimization**: Clear memory, close unnecessary processes
- **Streaming Tools**: Control streaming software via command line

## Execution Behavior

### Wait Behavior
- **WaitForExit = true**: Command executes and waits for completion before continuing
- **WaitForExit = false**: Command executes and returns immediately (fire-and-forget)

### Visibility Options
- **RunHidden = true**: No console window shown, silent execution
- **RunHidden = false**: Console window visible during execution

### Error Handling
- Command failures are logged with error details
- Failed commands don't stop other MIDI processing
- Exit codes are captured and logged for debugging

## Security Considerations

### Best Practices
1. **Validate Commands**: Ensure commands are safe and tested
2. **Limit Permissions**: Run MIDIFlux with minimal required permissions
3. **Avoid User Input**: Don't execute commands with untrusted input
4. **Test Thoroughly**: Verify commands work correctly before deployment

### Risk Mitigation
- Commands run with same permissions as MIDIFlux application
- No elevation of privileges without explicit user consent
- Command output is logged for audit purposes
- Failed commands are isolated and don't affect system stability

## Technical Notes

### Performance
- Command execution uses unified async execution model
- Long-running commands may impact MIDI responsiveness if WaitForExit=true
- Hidden commands generally execute faster than visible ones

### Compatibility
- PowerShell commands require PowerShell to be installed (default on Windows 10+)
- Command Prompt commands work on all Windows versions
- Some commands may require specific Windows features or roles

### Logging
- All command executions are logged with timestamps
- Command output and errors are captured in logs
- Exit codes are recorded for troubleshooting

## Related Actions

- **DelayAction**: Add delays before or after command execution in sequences
- **SequenceAction**: Combine command execution with other actions
- **ConditionalAction**: Execute commands based on MIDI values
- **StateConditionalAction**: Execute commands based on state values

## Best Practices

1. **Test Commands**: Always test commands manually before adding to configurations
2. **Use Descriptions**: Provide clear descriptions for all command actions
3. **Handle Errors**: Consider what happens if commands fail
4. **Performance Impact**: Be mindful of long-running commands in sequences
5. **Security First**: Never execute untrusted or user-provided commands

