# Getting Started with MIDIFlux

MIDIFlux transforms MIDI input devices into versatile computer controllers. Connect any MIDI device and map it to keyboard shortcuts, mouse actions, game controller inputs, system commands, and more.

## Quick Start

1. **Download and Run**
   - Download the latest release from [GitHub Releases](https://github.com/Cozmopolit/MIDIFlux/releases/latest)
   - Extract and run the executable (no installation required!)
   - Find MIDIFlux in your system tray

2. **Verify Your MIDI Device**
   - Connect your MIDI device via USB
   - Right-click the tray icon → **MIDI Input Detection** → select your device → **Start Listening**
   - Press a key or turn a knob on your device — you should see events appear in the list
   - This confirms MIDIFlux can see your device. Close the dialog when done.

3. **Create Your First Profile — Fader/Knob → System Volume**

   - Right-click the tray icon → **Configure Mapping Profiles**
   - In the **Profile Manager** tab, click **New** and enter a name (e.g., `My Volume`)
   - Select your new profile and click **Edit** — the profile editor opens in a new tab
   - Click **Add Mapping** — the mapping dialog opens
   - Click **Listen**, then move a fader or turn a knob on your MIDI device — the device, input type, CC number, and channel are detected automatically
   - Under **Action Type**, select **SystemVolumeAction** → click **OK**
   - Click **Save** to save your profile
   - Switch back to the **Profile Manager** tab, select your profile and click **Activate**
   - Move your fader — the Windows volume slider should follow!

4. **Explore Example Profiles**
   - Right-click the tray icon → **Configurations** → **examples** → choose a profile
   - All examples use `"*"` (wildcard) as device name, so they work with any connected MIDI device
   - Example profiles map specific MIDI note/CC numbers — use **MIDI Input Detection** to find which controls on your device match

## System Requirements

- **Operating System**: Windows 10/11 (x64)
- **Hardware**: One or more MIDI input devices
- **Windows 11 24H2/25H2**: Install the [Windows MIDI Services Runtime](https://github.com/microsoft/MIDI/releases) — Microsoft is replacing the legacy MIDI driver via update KB5074105 (phased rollout, Jan–Mar 2026). MIDIFlux supports the new stack natively, but the runtime must be installed separately.
- **Optional**: [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases) (for game controller emulation only)

## Example Configurations

MIDIFlux automatically installs example configurations to `%AppData%\MIDIFlux\profiles\examples\`:

### Start Here
- **basic-keyboard-shortcuts.json**: Copy/paste/cut shortcuts - perfect first example
- **system-controls.json**: Media controls (play/pause, track navigation)
- **all-action-types-demo.json**: Comprehensive showcase of every action type

### Advanced Examples
- **game-controller-demo.json**: Complete Xbox controller mapping (requires [ViGEm](GameController_Quickstart.md))
- **advanced-macros.json**: Complex action sequences and workflows
- **conditional-action-demo.json**: MIDI value-based conditional logic

### Specialized Use Cases
- **command-execution-examples.json**: Shell command execution patterns
- **midi-output-basic.json**: MIDI output to external devices
- **multi-channel-demo.json**: Multiple MIDI channel configurations

## How Profiles Work

A profile is a JSON file that maps MIDI events to actions. Each mapping connects a specific MIDI input (identified by type, number, and channel) to an action:

**Key concepts:**
- **DeviceName**: A specific device name, or `"*"` to match any connected MIDI device
- **InputType**: What kind of MIDI message triggers the action — `NoteOn`, `ControlChangeAbsolute`, `ControlChangeRelative`, `PitchBend`, etc.
- **Note / ControlNumber**: Which specific key or knob triggers the mapping (use **MIDI Input Detection** to find these)
- **Action `$type`**: Which action to execute — see the [Action Reference](Action_Reference.md) for all available types

## Configuration Workflow

1. **Start with examples** — Load and test the provided example profiles
2. **Examine the pattern** — Open the JSON to see how MIDI events map to actions
3. **Create custom profiles** — Use the Configuration Editor (GUI) or edit JSON directly
4. **Test incrementally** — Add one mapping at a time and verify it works
5. **Use logging** — Check `%AppData%\MIDIFlux\Logs\` to troubleshoot issues

## System Tray Menu

Right-click the MIDIFlux system tray icon for:
- **Configure Mapping Profiles**: Open the profile editor (GUI) to create and edit profiles
- **Settings**: Application settings
- **Configurations**: Browse and activate profiles (organized by folder)
- **MIDI Diagnostics**: View performance statistics and device status
- **MIDI Input Detection**: Monitor live MIDI input from your devices (great for testing!)
- **Logging**: Open log viewer, toggle silent mode
- **Exit**: Close the application

## File Locations

- **Configuration Files**: `%AppData%\MIDIFlux\profiles\`
- **Example Files**: `%AppData%\MIDIFlux\profiles\examples\`
- **Application Logs**: `%AppData%\MIDIFlux\Logs\`
- **Settings**: `%AppData%\MIDIFlux\appsettings.json`

## Game Controller Emulation

Turn your MIDI device into a virtual Xbox 360 controller — see the **[Game Controller Quickstart](GameController_Quickstart.md)** for a complete walkthrough.

## Next Steps

- **[Game Controller Quickstart](GameController_Quickstart.md)** — MIDI to Xbox 360 controller
- **[Action Reference](ACTION_REFERENCE.md)** — Complete guide to all action types
- **[Troubleshooting](Troubleshooting.md)** — Common issues and solutions
- **[Developer Guide](Developer_Guide.md)** — Building from source
