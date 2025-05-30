# MIDIFlux Configuration GUI Specification

**Version 3.0 – Updated: June 2024**

## Table of Contents

1. [Architecture & Foundation](#1--architecture--foundation)
2. [Main Application Window](#2--main-application-window)
3. [Profile Manager](#3--profile-manager)
4. [Profile Editor](#4--profile-editor)
5. [Mapping Editor](#5--mapping-editor)
6. [MIDI Input Detection](#6--midi-input-detection)
7. [Settings](#7--settings)
8. [Configuration Format](#8--configuration-format)
9. [State Management](#9--state-management)
10. [Acceptance Criteria](#10--acceptance-criteria)

---

## 1 · Architecture & Foundation

### 1.1 Configuration Storage

| Component | Description |
|-----------|-------------|
| **Base Directory** | All files are stored in **`%AppData%\MIDIFlux`** |
| **Settings File** | `settings.json` - Global application settings |
| **Profiles Directory** | `profiles\` - Contains subdirectories (e.g., `examples\`) with one or more JSON profile files |
| **Current Configuration** | `current.json` - Hidden file that stores a copy of the currently active configuration |

### 1.2 UI Technology

| Component | Description |
|-----------|-------------|
| **Framework** | Windows Forms (.NET 8) |
| **Main Window** | `MainForm` hosts a tab container (`TabControl` or *DockPanelSuite*) |
| **User Controls** | Each major function is implemented as a separate UserControl:<br>• `ProfileManagerControl`<br>• `ProfileEditorControl`<br>• `MappingEditorControl`<br>• `MidiDetectControl`<br>• `SettingsControl` |
| **UI Pattern** | Tabs are dynamically opened/closed; modal dialogs only for confirmations and pickers (e.g., Channel Picker, Unsaved Changes prompt) |
| **Architecture** | Controls contain no business logic to ensure testability and facilitate future migration to WPF/Avalonia |

### 1.3 Threading Model

| Component | Description |
|-----------|-------------|
| **MIDI Processing** | MIDI callbacks (from NAudio) run on worker threads |
| **UI Synchronization** | `MainForm` stores its `SynchronizationContext` during `Load` |
| **Thread Safety** | Helper method `void RunOnUI(Action)` posts to this context (`context.Post`) |
| **UI Updates** | All UI updates (status indicators, log entries, grid refreshes) are performed via `RunOnUI` |
| **Profile Switching** | `ReloadProfileAsync()` stops ports → loads JSON → starts ports within a `Task.Run`<br>Progress is reported back to the UI via `IProgress<double>` |

### 1.4 Accessibility

The entire GUI must be fully keyboard-navigable and screen-reader friendly (proper labels and automation properties). Target compliance level: **WCAG 2.1 AA**.

### 1.5 Packaging

The GUI editor is an integrated part of the main application. The build process produces a single portable executable (`MIDIFlux.exe`) via `PublishSingleFile=true`. There is no separate configuration editor executable.

---

## 2 · Main Application Window

### 2.1 Layout

The main window serves as a container for all functionality, with a tab-based interface for switching between different views:

- Profile Manager (default view)
- Profile Editor (opened when editing a profile)
- Mapping Editor (opened when editing a mapping)
- MIDI Input Detection (for testing and identifying MIDI inputs)
- Settings (for global application settings)

### 2.2 System Tray Integration

The main window integrates with the existing system tray functionality:

- Minimizing the window keeps the application running in the system tray
- Double-clicking the tray icon restores the window
- Right-clicking the tray icon shows the existing configuration menu plus an option to open the GUI

---

## 3 · Profile Manager

The Profile Manager is the main entry point for the configuration GUI, allowing users to manage MIDI profiles.

### 3.1 Features

- **Profile List**: Displays all available profiles in a tree view, organized by subdirectory
- **Profile Actions**:
  - **New Profile**: Creates a new empty profile
  - **Duplicate Profile**: Copies the currently selected profile with a new name (`Copy of <original>`)
  - **Delete Profile**: Removes the currently selected profile after confirmation
- **Profile Activation**: Allows activating a profile (loads it into the MIDIFlux runtime)
- **Unsaved Indicator**: Shows an asterisk (`*`) after the profile name in the title bar when there are unsaved changes
- **Global Settings**: Button to open the Settings view

### 3.2 UI Elements

- Tree view for profile navigation
- Action buttons for profile management
- Status indicator showing the currently active profile
- Search/filter box for finding profiles in large collections

---

## 4 · Profile Editor

The Profile Editor allows editing the details of a MIDI profile, including device configurations and mappings.

### 4.1 MIDI Device Configurations

- **Device List**: Shows all configured MIDI devices in the profile
- **Device Properties**:
  - **Input Profile Name**: Unique identifier for this input profile
  - **Device Name**: Name of the MIDI device (dropdown populated with available devices)
  - **MIDI Channels**: Channels to listen on (uses Channel Picker dialog)
- **Device Actions**:
  - **Add Device**: Adds a new device configuration
  - **Duplicate**: Creates a new device configuration based on the selected one
  - **Remove**: Deletes the selected device configuration

### 4.2 Channel Picker Dialog

- Replaces free text input with a dialog for selecting MIDI channels
- Checkbox matrix for channels 1-16 plus an "All" option
- Result displayed as a comma-separated list (e.g., `1,5,6,7`)

### 4.3 Mappings Grid

- Displays all mappings for the selected device configuration
- Columns:
  - Mapping Type (Note, CC, etc.)
  - Trigger (MIDI note/control number)
  - Action Type (Key, Macro, etc.)
  - Action Details (key code, description, etc.)
  - Description (user-defined)
- Features:
  - Sortable columns
  - Filter box for live filtering on trigger or action text
  - Multi-selection for bulk operations (delete, duplicate)
  - Validation to warn (but not prevent) when the same MIDI trigger is mapped multiple times

### 4.4 Live Preview (Test Mode)

- **Toggle Button**: Enables/disables Live Preview mode
- **Implementation**:
  - Saves current edited state to a temporary file (`preview_####.json`)
  - Loads this configuration via `MidiProcessingService.ReloadProfile(tempPath, preview:true)`
- **Preview Behavior**:
  - **Keyboard/Mouse**: Events are simulated but immediately released and displayed in the Status Pane
  - **Game Controller**: Virtual buttons/axes are temporarily set and automatically reset after 1 second
- **Status Pane**: Displays the last 10 preview events
- **Cleanup**: When deactivated or when the editor is closed, the original profile is reloaded and all preview key-downs are released

---

## 5 · Mapping Editor

The Mapping Editor provides a detailed interface for creating and editing different types of MIDI mappings.

### 5.1 Common Features

- **Mapping Type Selector**: Tabs or dropdown to select the mapping type:
  - Key Mapping (MIDI note to keyboard key)
  - Absolute Control Mapping (faders, knobs)
  - Relative Control Mapping (jog wheels)
  - Game Controller Mapping
  - CC Range Mapping
  - Macro Mapping
- **Anti-Recursion**: Prevents creating recursive mappings (e.g., macros that call other macros)
  - The Macro option is disabled in the Action Type dropdown when already in the Macro Editor

### 5.2 Key Mapping Editor

- **MIDI Note**: Input for the MIDI note number (0-127)
- **Virtual Key Code**: Dropdown or picker for the keyboard key
- **Modifiers**: Checkboxes for modifier keys (Shift, Ctrl, Alt, Win)
- **Description**: Text field for user description

### 5.3 Control Mapping Editors

- **Control Number**: Input for the MIDI control number (0-127)
- **Handler Type**: Dropdown for selecting the handler type
- **Parameters**: Dynamic form based on the selected handler type
- **Description**: Text field for user description

### 5.4 CC Range Mapping Editor

- **Control Number**: Input for the MIDI control number (0-127)
- **Value Ranges**: Grid for defining multiple value ranges:
  - Min Value (0-127)
  - Max Value (0-127)
  - Action Type
  - Action Parameters
- **Overlap Warning**: Yellow warning icon when ranges overlap
- **Range Priority**: The order in the grid determines evaluation priority

### 5.5 Game Controller Mapping Editor

- **Mapping Type**: Button or Axis
- **MIDI Trigger**: Note or Control Number
- **Controller Button/Axis**: Dropdown for selecting the target button/axis
- **Parameters**: Additional parameters based on the mapping type

---

## 6 · MIDI Input Detection

The MIDI Input Detection window helps users identify MIDI inputs for mapping.

### 6.1 Features

- **Device Selector**: Dropdown to select which MIDI device to monitor
- **MIDI Monitor**: Displays incoming MIDI messages in real-time
  - Note On/Off events
  - Control Change events
  - Other MIDI message types
- **Flood Control**: Groups rapid events (>10 events/second) with a message like "(muted 24 events)"
- **Listen Scope**: Checkbox to "Listen on all system MIDI devices (ignore current profile)"
- **Copy to Mapping**: Button to use the selected MIDI event as the basis for a new mapping

### 6.2 UI Elements

- Device selection dropdown
- Event display list/grid
- Filter options
- Status indicators
- Action buttons

---

## 7 · Settings

The Settings window provides access to global application settings.

### 7.1 General Settings

- **Startup Behavior**:
  - Start minimized option
  - Load last used profile option
- **UI Settings**:
  - Theme selection
  - Language selection

### 7.2 Logging Settings

- **Log Level**: Dropdown for selecting the logging detail level
- **Log Rotation**:
  - Max Log Size (MB): NumericUpDown control (default: 50)
  - Retain Logs (days): NumericUpDown control (default: 14)

### 7.3 Advanced Settings

- **MIDI Device Polling**: Interval for checking device connections
- **Performance Options**: Settings that affect application performance

---

## 8 · Configuration Format

### 8.1 JSON Schema

```json
{
  "midiDevices": [
    {
      "inputProfile": "Controller-Main",
      "deviceName": "MIDI Controller",
      "midiChannels": [1, 2, 3],
      "mappings": [
        {
          "midiNote": 60,
          "virtualKeyCode": 77,
          "modifiers": [],
          "description": "YouTube mute toggle (M key)"
        }
      ],
      "absoluteControlMappings": [
        {
          "controlNumber": 42,
          "handlerType": "SystemVolume",
          "parameters": {},
          "description": "System volume control"
        }
      ],
      "relativeControlMappings": [
        {
          "controlNumber": 30,
          "handlerType": "ScrollWheel",
          "sensitivity": 2,
          "parameters": {},
          "description": "Mouse scroll wheel"
        }
      ],
      "ccRangeMappings": [
        {
          "controlNumber": 7,
          "ranges": [
            {
              "minValue": 0,
              "maxValue": 63,
              "actionType": "KeyPress",
              "parameters": {
                "virtualKeyCode": 37
              },
              "description": "Left arrow for values 0-63"
            },
            {
              "minValue": 64,
              "maxValue": 127,
              "actionType": "KeyPress",
              "parameters": {
                "virtualKeyCode": 39
              },
              "description": "Right arrow for values 64-127"
            }
          ],
          "description": "Direction control"
        }
      ],
      "gameControllerMappings": {
        "buttons": [
          {
            "midiNote": 36,
            "buttonIndex": 0,
            "description": "A button"
          }
        ],
        "axes": [
          {
            "controlNumber": 1,
            "axisIndex": 0,
            "invert": false,
            "description": "Left thumbstick X"
          }
        ]
      }
    }
  ]
}
```

### 8.2 Design Principles

- **Type Separation**: Clear separation of mapping types for readability and parsing
- **Extensibility**: Extensions are made by adding arrays or fields at relevant locations
- **Backward Compatibility**: Existing files remain valid as the schema evolves

### 8.3 Implementation Strategy

The Configuration Editor uses a `MappingViewModel` with a `mappingCategory` field to display all mapping types in a single grid while directing edits to the appropriate editor tab.

This approach eliminates the need for structural migration scripts and keeps GUI changes separate from the file structure.

---

## 9 · State Management

### 9.1 Unsaved Changes Tracking

- An `IsDirty` flag is set on the first change event
- An asterisk (`*`) is displayed in the window title when changes are unsaved
- When closing the window or application, a dialog prompts to save changes
- The flag is reset and the asterisk removed after successful saving

### 9.2 Profile Activation

- The currently active profile is highlighted in the Profile Manager
- Activating a profile loads it into the MIDIFlux runtime via `MidiProcessingService.LoadConfiguration()`
- A copy of the active configuration is stored in `current.json` for automatic loading on application startup

### 9.3 Error Handling

- Configuration errors are displayed with clear messages
- Invalid configurations are not loaded into the runtime
- The user is guided on how to fix configuration issues

---

## 10 · Acceptance Criteria

1. **Profile Management**:
   - Create, duplicate, and delete profiles
   - Organize profiles in subdirectories

2. **Device Configuration**:
   - Add, duplicate, and remove device configurations
   - Select from available MIDI devices
   - Configure MIDI channels via the Channel Picker

3. **Mapping Management**:
   - Create and edit all mapping types
   - Duplicate Device Config → new entry with suggested name, editor focus on name field, no auto-save
   - Filter and sort mappings
   - Perform bulk operations on multiple mappings

4. **Live Preview**:
   - Activate Test Mode, press MIDI note 60, Status Pane shows "Note 60 → Key A"
   - Global input buffers contain no hanging key-downs after 500ms
   - Deactivating Test Mode restores the original profile

5. **MIDI Detection**:
   - Monitor MIDI inputs in real-time
   - Flood Control → Rotate jog wheel (>20 events in 1s) → Log shows compressed output
   - Create mappings directly from detected MIDI events

6. **Thread Safety**:
   - Automated test: MIDI callback triggers `OnNoteOn` 50 times per second
   - UI must not throw `InvalidOperationException`
   - All UI updates must be performed on the UI thread

7. **Settings**:
   - Configure logging options
   - Set startup behavior
   - Apply settings without application restart

8. **Accessibility**:
   - All functions accessible via keyboard
   - Screen readers can interpret all UI elements
   - Focus management follows logical flow

