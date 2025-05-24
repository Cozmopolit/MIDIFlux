using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using Xunit;

namespace MIDIFlux.Core.Tests.Processing;

/// <summary>
/// Unit tests for the UnifiedActionEventProcessor class.
/// Tests the high-performance MIDI event processing pipeline.
/// </summary>
public class UnifiedActionEventProcessorTests : TestBase
{
    private readonly ILogger _logger;
    private readonly UnifiedActionMappingRegistry _registry;
    private readonly UnifiedActionEventProcessor _processor;
    private readonly IUnifiedActionFactory _actionFactory;

    public UnifiedActionEventProcessorTests()
    {
        // Create logger
        _logger = LoggingHelper.CreateLogger<UnifiedActionEventProcessorTests>();

        // Create action factory
        _actionFactory = new UnifiedActionFactory(LoggingHelper.CreateLogger<UnifiedActionFactory>());

        // Create registry
        var registryLogger = LoggingHelper.CreateLogger<UnifiedActionMappingRegistry>();
        _registry = new UnifiedActionMappingRegistry(registryLogger);

        // Create processor
        var processorLogger = LoggingHelper.CreateLogger<UnifiedActionEventProcessor>();
        _processor = new UnifiedActionEventProcessor(processorLogger, _registry);
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesSuccessfully()
    {
        // Arrange & Act - Constructor called in setup

        // Assert
        Assert.NotNull(_processor);
        var stats = _processor.GetStatistics();
        Assert.NotNull(stats);
        Assert.NotNull(stats.RegistryStatistics);
    }

    [Fact]
    public void ProcessMidiEvent_WithNullEvent_ReturnsFalse()
    {
        // Arrange
        int deviceId = 1;
        MidiEvent? midiEvent = null;
        string deviceName = "Test Device";

        // Act
        bool result = _processor.ProcessMidiEvent(deviceId, midiEvent!, deviceName);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessMidiEvent_WithUnsupportedEventType_ReturnsFalse()
    {
        // Arrange
        var midiEvent = new MidiEvent
        {
            EventType = MidiEventType.Other,
            Channel = 1,
            Timestamp = DateTime.Now
        };

        // Act
        bool result = _processor.ProcessMidiEvent(1, midiEvent, "Test Device");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessMidiEvent_WithNoMatchingActions_ReturnsFalse()
    {
        // Arrange
        var midiEvent = new MidiEvent
        {
            EventType = MidiEventType.NoteOn,
            Channel = 1,
            Note = 60,
            Velocity = 127,
            Timestamp = DateTime.Now
        };

        // Act
        bool result = _processor.ProcessMidiEvent(1, midiEvent, "Test Device");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ProcessMidiEvent_WithMatchingAction_ExecutesActionAndReturnsTrue()
    {
        // Arrange
        // Create a test action
        var keyAction = new MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction(new KeyPressReleaseConfig
        {
            VirtualKeyCode = 65, // A key - safe for testing
            Description = "Test A key press"
        });

        // Create a mapping
        var mapping = new UnifiedActionMapping
        {
            Input = new UnifiedActionMidiInput
            {
                DeviceName = "Test Device",
                InputType = UnifiedActionMidiInputType.NoteOn,
                InputNumber = 60,
                Channel = 1
            },
            Action = keyAction,
            IsEnabled = true,
            Description = "Test mapping"
        };

        // Load mapping into registry
        _registry.LoadMappings(new[] { mapping });

        // Create MIDI event
        var midiEvent = new MidiEvent
        {
            EventType = MidiEventType.NoteOn,
            Channel = 1,
            Note = 60,
            Velocity = 127,
            Timestamp = DateTime.Now
        };

        // Act
        bool result = _processor.ProcessMidiEvent(1, midiEvent, "Test Device");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ProcessMidiEvent_WithMultipleMatchingActions_ExecutesAllActions()
    {
        // Arrange
        // Create multiple test actions
        var keyAction1 = new MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction(new KeyPressReleaseConfig
        {
            VirtualKeyCode = 65, // A key
            Description = "Test A key press"
        });

        var keyAction2 = new MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction(new KeyPressReleaseConfig
        {
            VirtualKeyCode = 66, // B key
            Description = "Test B key press"
        });

        // Create mappings for the same MIDI input
        var mappings = new[]
        {
            new UnifiedActionMapping
            {
                Input = new UnifiedActionMidiInput
                {
                    DeviceName = "Test Device",
                    InputType = UnifiedActionMidiInputType.NoteOn,
                    InputNumber = 60,
                    Channel = 1
                },
                Action = keyAction1,
                IsEnabled = true,
                Description = "Test mapping 1"
            },
            new UnifiedActionMapping
            {
                Input = new UnifiedActionMidiInput
                {
                    DeviceName = "Test Device",
                    InputType = UnifiedActionMidiInputType.NoteOn,
                    InputNumber = 60,
                    Channel = 1
                },
                Action = keyAction2,
                IsEnabled = true,
                Description = "Test mapping 2"
            }
        };

        // Load mappings into registry
        _registry.LoadMappings(mappings);

        // Create MIDI event
        var midiEvent = new MidiEvent
        {
            EventType = MidiEventType.NoteOn,
            Channel = 1,
            Note = 60,
            Velocity = 127,
            Timestamp = DateTime.Now
        };

        // Act
        bool result = _processor.ProcessMidiEvent(1, midiEvent, "Test Device");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ProcessMidiEvent_WithControlChangeEvent_ProcessesCorrectly()
    {
        // Arrange
        var delayAction = new MIDIFlux.Core.Actions.Simple.DelayAction(new DelayConfig
        {
            Milliseconds = 1, // Very short delay for testing
            Description = "Test delay"
        });

        var mapping = new UnifiedActionMapping
        {
            Input = new UnifiedActionMidiInput
            {
                DeviceName = "Test Device",
                InputType = UnifiedActionMidiInputType.ControlChange,
                InputNumber = 7, // Volume control
                Channel = 1
            },
            Action = delayAction,
            IsEnabled = true,
            Description = "Test CC mapping"
        };

        _registry.LoadMappings(new[] { mapping });

        var midiEvent = new MidiEvent
        {
            EventType = MidiEventType.ControlChange,
            Channel = 1,
            Controller = 7,
            Value = 100,
            Timestamp = DateTime.Now
        };

        // Act
        bool result = _processor.ProcessMidiEvent(1, midiEvent, "Test Device");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void GetStatistics_ReturnsValidStatistics()
    {
        // Act
        var stats = _processor.GetStatistics();

        // Assert
        Assert.NotNull(stats);
        Assert.NotNull(stats.RegistryStatistics);
        Assert.True(stats.LastProcessingTimeMs >= 0);
    }

    public override void Dispose()
    {
        // Clean up any resources if needed
        base.Dispose();
    }
}
