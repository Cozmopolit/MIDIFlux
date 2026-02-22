# Game Controller Quickstart

Turn your MIDI device into a virtual Xbox 360 controller.

## Prerequisites

1. Install the [ViGEm Bus Driver](https://github.com/ViGEm/ViGEmBus/releases) and restart your computer
2. Verify: MIDIFlux logs should show `ViGEm bus connected` on startup (no warnings about missing driver)

See [ViGEm Setup](ViGEm_Setup.md) for details and troubleshooting.

## Step 1: Test with the Demo Profile

1. Right-click the MIDIFlux tray icon → **Configurations** → **examples** → **game-controller-demo**
2. Press keys or move faders on your MIDI device
3. If the MIDI inputs happen to match the defaults (Notes 36-50, CC 1-6 on Channel 1), you'll see controller activity in a tool like [Gamepad Tester](https://hardwaretester.com/gamepad)

> **Tip:** Some games require the virtual controller to be present *before* the game launches. Activate your profile first, then start the game.

## Step 2: Remap to Your MIDI Device

The demo profile uses placeholder MIDI inputs. To make it work with your device:

1. Open **MIDI Input Detection** (tray menu) to learn which Notes/CCs your pads, keys, and faders send
2. Open **Configure Mapping Profiles** → select the demo profile → **Edit**
3. **Start with 2-3 mappings** to verify things work before remapping all 36:
   - Pick a button pair (e.g., "A Button Press" + "A Button Release") → **Edit** each → **Listen** → hit a pad → **OK**
   - Pick an axis (e.g., "Left Trigger") → **Edit** → **Listen** → move a fader → **OK**
4. **Save** and test with [Gamepad Tester](https://hardwaretester.com/gamepad) — you should see the A button and trigger respond
5. Once confirmed, remap the remaining mappings the same way

**Good to know:**
- Button pairs (Press + Release) must use the **same Note number** — the press mapping triggers on NoteOn, the release on NoteOff
- You only need to remap the controls you actually want to use — leave the rest as-is or disable them

## What's in the Demo Profile

The `game-controller-demo.json` maps a complete Xbox 360 controller:

| MIDI Input | Controller Output | Notes |
|---|---|---|
| Notes 36-39 | A, B, X, Y | Face buttons |
| Notes 40-41 | LB, RB | Shoulder buttons |
| Notes 42-43 | Back, Start | Menu buttons |
| Notes 44-45 | L3, R3 | Thumbstick clicks |
| Notes 46-49 | D-Pad Up/Down/Left/Right | D-pad |
| Note 50 | Guide | Xbox button |
| CC 1-4 | Left/Right stick X/Y | Thumbstick axes |
| CC 5-6 | Left/Right trigger | Triggers |

All buttons use **sustained** mappings (NoteOn → press, NoteOff → release), so holding a MIDI pad holds the controller button.

## Key Concepts

### Buttons: Instant vs. Sustained

- **GameControllerButtonAction** — single press-and-release on NoteOn (tap behavior)
- **GameControllerButtonDownAction** + **GameControllerButtonUpAction** — hold while MIDI key is held (the demo uses this)

### Axes and Triggers

**GameControllerAxisAction** with `UseMidiValue: true` maps the incoming CC value (0-127) to the axis range:
- **Thumbsticks**: 0-127 → -32768 to +32767 (center at CC 64)
- **Triggers**: 0-127 → 0 to 255

Set `Invert: true` to reverse the axis direction.

### Multiple Controllers

Set `ControllerIndex` to 0-3 to target different virtual controllers (up to 4 simultaneous).

## Advanced: Combo Sequences

See `game-controller-sustained-demo.json` for an example of combining buttons + axes in a timed sequence (e.g., jump + forward movement using SequenceAction with DelayAction).

## Further Reading

- [Action Reference](ACTION_REFERENCE.md) — all action types and parameters
- [ViGEm Setup](ViGEm_Setup.md) — driver details and compatibility
- [Troubleshooting](Troubleshooting.md) — common issues
