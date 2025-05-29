# MIDI Devices and Controllers

This directory contains documentation about MIDI devices, controllers, and how to map from them in MIDIFlux.

## Contents

- [Controller Mappings](ControllerMappings.md) - How to map MIDI controllers to various functions
- [Relative Controls](RelativeControls.md) - How to use jog wheels and other relative controls
- [MIDI Channel Handling](MIDI_Channel_Handling.md) - How MIDI channels are handled throughout MIDIFlux

## Overview

MIDIFlux supports a wide range of MIDI devices, from simple controllers to complex DJ equipment. This documentation explains how to configure and use different types of MIDI devices with MIDIFlux.

### Supported Controllers

MIDIFlux has been tested with the following MIDI controllers:

1. **Nektar Pacer Foot Controller**
   - Sends MIDI notes 52, 54, 55, 57, 59, 60 when the six lower pedals are pressed
   - Can be mapped to keyboard shortcuts or game controller buttons

2. **Traktor Kontrol S2 MK3**
   - Sends Control Change messages on channel 4
   - Controller number 42 for faders
   - Controller number 30 for the jog wheel (relative control)
   - Can be mapped to keyboard shortcuts, system volume, scroll wheel, or game controller axes

MIDIFlux supports using multiple MIDI devices simultaneously. You can map different MIDI devices to different functions, or even map the same MIDI notes from different devices to different actions.

