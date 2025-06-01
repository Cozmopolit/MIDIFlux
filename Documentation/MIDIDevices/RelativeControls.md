# Relative Controls in MIDIFlux

MIDIFlux supports relative controls like jog wheels and endless encoders. This document explains how to configure and use relative controls.

## What are Relative Controls?

Relative controls send incremental values rather than absolute positions. For example, a jog wheel might send a value of 65 when turned clockwise and 63 when turned counterclockwise, rather than an absolute position like a fader would.

## Relative Value Encodings

MIDI devices use different encoding methods for relative values. MIDIFlux supports the following encodings:

### Sign-Magnitude (Signed Bit)

- Values 0-63 represent positive increments (1 to 64)
- Values 65-127 represent negative increments (-1 to -63)
- Value 64 represents no change

This is the most common encoding method for relative controls.

### Two's Complement

- Values 0-64 represent positive increments (0 to 64)
- Values 65-127 represent negative increments (-63 to -1)

### Binary Offset

- Values 0-63 represent negative increments (-64 to -1)
- Values 65-127 represent positive increments (1 to 63)
- Value 64 represents no change

The Traktor Kontrol S2 MK3 jog wheel uses this encoding method.

## Configuring Relative Controls

To configure a relative control, create a mapping with `ControlChangeRelative` as the InputType:

```json
{
  "Id": "jog-wheel-scroll",
  "Description": "Jog wheel to mouse scroll",
  "InputType": "ControlChangeRelative",
  "Channel": 4,
  "ControlNumber": 30,
  "Action": {
    "$type": "RelativeCCAction",
    "Parameters": {
      "PositiveAction": {
        "$type": "MouseScrollAction",
        "Parameters": {
          "Direction": "Up",
          "Amount": 2
        },
        "Description": "Scroll up"
      },
      "NegativeAction": {
        "$type": "MouseScrollAction",
        "Parameters": {
          "Direction": "Down",
          "Amount": 2
        },
        "Description": "Scroll down"
      }
    },
    "Description": "Relative scroll control"
  }
}
```

### Configuration Parameters

- **InputType**: Must be `"ControlChangeRelative"`
- **Channel**: MIDI channel (1-16, or `null` for any channel)
- **ControlNumber**: The MIDI control number (0-127)
- **Action**: Usually a `RelativeCCAction` with positive and negative actions

## RelativeCCAction

The `RelativeCCAction` is specifically designed for relative controls:

- **PositiveAction**: Action to execute for positive (clockwise) movement
- **NegativeAction**: Action to execute for negative (counter-clockwise) movement
- **AccelerationStateKey**: Optional state key for acceleration tracking
- **AccelerationThreshold**: Number of rapid movements before acceleration
- **AccelerationMultiplier**: Multiplier for accelerated actions

## Example: Traktor Kontrol S2 MK3 Jog Wheel

The Traktor Kontrol S2 MK3 jog wheel sends Control Change messages on channel 4 with controller number 30. It uses Binary Offset encoding.

### Configuration Example

```json
{
  "ProfileName": "Multi-Device Relative Controls",
  "MidiDevices": [
    {
      "DeviceName": "Traktor Kontrol S2 MK3 MIDI",
      "Mappings": [
        {
          "Id": "traktor-jog-wheel",
          "Description": "Traktor jog wheel to scroll",
          "InputType": "ControlChangeRelative",
          "Channel": 4,
          "ControlNumber": 30,
          "Action": {
            "$type": "RelativeCCAction",
            "Parameters": {
              "PositiveAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Up",
                  "Amount": 2
                },
                "Description": "Scroll up (high sensitivity)"
              },
              "NegativeAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Down",
                  "Amount": 2
                },
                "Description": "Scroll down (high sensitivity)"
              }
            },
            "Description": "Traktor jog wheel scroll control"
          }
        }
      ]
    },
    {
      "DeviceName": "Another MIDI Device",
      "Mappings": [
        {
          "Id": "other-jog-wheel",
          "Description": "Other device jog wheel",
          "InputType": "ControlChangeRelative",
          "Channel": 1,
          "ControlNumber": 30,
          "Action": {
            "$type": "RelativeCCAction",
            "Parameters": {
              "PositiveAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Up",
                  "Amount": 1
                },
                "Description": "Scroll up (low sensitivity)"
              },
              "NegativeAction": {
                "$type": "MouseScrollAction",
                "Parameters": {
                  "Direction": "Down",
                  "Amount": 1
                },
                "Description": "Scroll down (low sensitivity)"
              }
            },
            "Description": "Other device scroll control"
          }
        }
      ]
    }
  ]
}
```

This configuration maps jog wheels from two different devices to mouse scroll with different sensitivities.

## Detecting Relative Controls

MIDIFlux automatically interprets relative control values using the standard Sign-Magnitude encoding:
- Values 1-63: Positive movement (clockwise)
- Values 65-127: Negative movement (counter-clockwise)
- Value 64: No movement (ignored)

Most MIDI controllers use this standard encoding, so no special configuration is needed.

## Troubleshooting

If your relative control is not working correctly:

1. **Check Input Type**: Ensure you're using `"ControlChangeRelative"` as the InputType
2. **Verify Control Numbers**: Check the log files to see the raw MIDI values and control numbers
3. **Test Direction**: If the direction is reversed, swap the PositiveAction and NegativeAction
4. **Adjust Sensitivity**: Change the `Amount` parameter in MouseScrollAction for different sensitivity
5. **Check Channel**: Ensure the MIDI channel matches (use `null` for any channel)
6. **Device Name**: Ensure the `DeviceName` matches what MIDIFlux detects (use `"*"` for any device)

### Common Issues

- **Wrong InputType**: Using `ControlChangeAbsolute` instead of `ControlChangeRelative`
- **Reversed Direction**: Positive and negative actions are swapped
- **Too Sensitive**: Reduce the `Amount` parameter in scroll actions
- **Not Sensitive Enough**: Increase the `Amount` parameter or add acceleration
- **Channel Mismatch**: Controller sending on different channel than configured

### Multi-Device Setup

When using multiple MIDI devices with relative controls:

1. Make sure each device is properly connected and recognized by Windows
2. Check that the device names in your configuration match the actual device names
3. Use unique mapping IDs for each device to avoid conflicts
4. Test each device separately to isolate issues
5. Consider using `"*"` as DeviceName if you want the same control to work on any device

