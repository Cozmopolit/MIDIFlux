# MIDIFlux Event Handling

This document describes the event handling architecture in MIDIFlux.

## Overview

MIDIFlux uses a direct call approach for handling MIDI events. The flow is as follows:

1. `MidiManager` receives MIDI events from the MIDI device
2. `MidiManager` directly calls `EventDispatcher.HandleMidiEvent()` with the event
3. `EventDispatcher` processes the event and executes the appropriate actions

## Components

### MidiManager

The `MidiManager` class is responsible for:

- Managing MIDI device connections
- Receiving MIDI events from the device
- Converting NAudio MIDI events to our internal `MidiEvent` format
- Directly dispatching events to the `EventDispatcher`

### EventDispatcher

The `EventDispatcher` class is responsible for:

- Maintaining the mapping configuration
- Processing MIDI events
- Executing keyboard actions based on the configuration

### MidiProcessingService

The `MidiProcessingService` class is responsible for:

- Connecting the `MidiManager` and `EventDispatcher`
- Managing the application lifecycle
- Loading and managing configurations

## Event Flow

1. NAudio raises a `MessageReceived` event when a MIDI message is received
2. `MidiManager.MidiIn_MessageReceived` handles this event
3. `MidiManager` converts the NAudio event to our internal `MidiEvent` format
4. `MidiManager` directly calls `EventDispatcher.HandleMidiEvent()`
5. `EventDispatcher` processes the event based on the current configuration
6. `EventDispatcher` executes the appropriate keyboard actions

## Benefits of Direct Call Approach

- Simplified code flow
- Reduced complexity
- Lower latency
- Easier to debug and maintain
- More predictable behavior

## Initialization

The components are connected during initialization:

```csharp
// In MidiProcessingService constructor
_midiManager.SetEventDispatcher(_eventDispatcher);
```

This establishes the direct call relationship between the components.

