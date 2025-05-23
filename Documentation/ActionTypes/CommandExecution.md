# Command Execution Feature

MIDIFlux can execute shell commands (PowerShell or Command Prompt) in response to MIDI note events. This document explains how to use this feature.

## Overview

The Command Execution feature allows you to map MIDI notes to execute commands in either PowerShell or Command Prompt. This can be useful for:

- Launching applications
- Running scripts
- Performing system operations
- Automating tasks

## Configuration

Command execution mappings are defined in your configuration file using the `CommandExecution` action type (value: 4).

### Basic Configuration

Here's a basic example of a command execution mapping:

```json
{
  "midiNote": 60,
  "actionType": 4,
  "command": "Get-Date",
  "shellType": "powershell",
  "runHidden": false,
  "waitForExit": true,
  "description": "Display current date and time"
}
```

### Configuration Properties

| Property | Type | Description |
|----------|------|-------------|
| `midiNote` | int | The MIDI note number to map |
| `actionType` | int | Must be `4` for command execution |
| `command` | string | The command to execute |
| `shellType` | string | Either `"powershell"` or `"cmd"` |
| `runHidden` | bool | Whether to run the command without showing a console window |
| `waitForExit` | bool | Whether to wait for the command to complete before continuing |
| `description` | string | Optional description of the mapping |

## Shell Types

MIDIFlux supports two shell types:

### PowerShell

For PowerShell commands, set `shellType` to `"powershell"`. The command will be executed using:

```
powershell.exe -NoProfile -ExecutionPolicy Bypass -Command "your command"
```

Example:
```json
{
  "midiNote": 60,
  "actionType": 4,
  "command": "Get-Process | Sort-Object CPU -Descending | Select-Object -First 5",
  "shellType": "powershell",
  "description": "Show top 5 CPU-consuming processes"
}
```

### Command Prompt

For Command Prompt commands, set `shellType` to `"cmd"`. The command will be executed using:

```
cmd.exe /c your command
```

Example:
```json
{
  "midiNote": 61,
  "actionType": 4,
  "command": "dir /s",
  "shellType": "cmd",
  "description": "List all files in current directory and subdirectories"
}
```

## Additional Options

### Running Hidden

Set `runHidden` to `true` to execute the command without showing a console window. This is useful for commands that don't require user interaction or visual output.

Example:
```json
{
  "midiNote": 62,
  "actionType": 4,
  "command": "Start-Process notepad",
  "shellType": "powershell",
  "runHidden": true,
  "description": "Launch Notepad without showing PowerShell window"
}
```

### Waiting for Completion

Set `waitForExit` to `true` to wait for the command to complete before continuing. This is useful for commands that need to finish before other actions are taken.

Example:
```json
{
  "midiNote": 63,
  "actionType": 4,
  "command": "ping -n 5 127.0.0.1",
  "shellType": "cmd",
  "waitForExit": true,
  "description": "Ping localhost 5 times and wait for completion"
}
```

## Example Configuration

A complete example configuration file is available at `config/example-command-execution.json`.

## Important Notes

1. Be careful with commands that might be destructive or resource-intensive.
2. Commands run with the same permissions as the MIDIFlux application.
3. For security reasons, avoid running commands that accept user input from untrusted sources.
4. Command output is logged but not displayed to the user (unless the command itself opens a window).

