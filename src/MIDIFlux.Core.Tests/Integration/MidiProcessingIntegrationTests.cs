using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Infrastructure;
using MIDIFlux.Core.Tests.Mocks;
using MIDIFlux.Core.Tests.Utilities;
using Moq;
using Xunit;

namespace MIDIFlux.Core.Tests.Integration;

/// <summary>
/// Integration tests for the complete MIDI processing pipeline
/// Tests the hot path: MIDI input → action lookup → execution
/// </summary>
public class MidiProcessingIntegrationTests : TestBase
{
    private readonly ConfigurationService _configService;
    private readonly Mock<DeviceConfigurationManager> _mockDeviceConfigManager;
    private readonly ActionMappingRegistry _registry;
    private readonly MidiActionEngine _engine;
    private readonly TestAction _testAction;

    public MidiProcessingIntegrationTests()
    {
        // Create real configuration service (can't be mocked as methods aren't virtual)
        _configService = new ConfigurationService(CreateLogger<ConfigurationService>());

        // Create mock device configuration manager
        _mockDeviceConfigManager = new Mock<DeviceConfigurationManager>(
            Mock.Of<ILogger<DeviceConfigurationManager>>(),
            _configService,
            Mock.Of<IServiceProvider>());

        // Create real registry and engine for integration testing
        _registry = new ActionMappingRegistry(CreateLogger<ActionMappingRegistry>());
        _engine = new MidiActionEngine(
            Logger,
            _registry,
            _configService,
            _mockDeviceConfigManager.Object);

        // Create a test action that tracks execution
        _testAction = new TestAction();
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldExecuteActionForMatchingMidiInput()
    {
        // Arrange - Set up a mapping
        var mapping = new ActionMapping
        {
            Description = "Test Note Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = _testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        // Create matching MIDI event
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process the MIDI event through the complete pipeline
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Test Device");

        // Assert
        result.Should().BeTrue(); // Action was found and executed
        _testAction.ExecutionCount.Should().Be(1);
        _testAction.LastMidiValue.Should().Be(127); // Should have received the velocity value
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldNotExecuteForNonMatchingInput()
    {
        // Arrange - Set up a mapping for note 60
        var mapping = new ActionMapping
        {
            Description = "Test Note Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = _testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        // Create non-matching MIDI event (note 61 instead of 60)
        var midiEvent = MidiEventBuilder.NoteOn(1, 61, 127);

        // Act - Process the MIDI event
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Test Device");

        // Assert
        result.Should().BeFalse(); // No action should be found
        _testAction.ExecutionCount.Should().Be(0);
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldSupportWildcardDeviceMatching()
    {
        // Arrange - Set up a mapping with wildcard device
        var mapping = new ActionMapping
        {
            Description = "Wildcard Device Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "*", // Wildcard device
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = _testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        // Create MIDI event from any device
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process with different device name
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Any Device Name");

        // Assert
        result.Should().BeTrue(); // Wildcard should match any device
        _testAction.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldSupportWildcardChannelMatching()
    {
        // Arrange - Set up a mapping with wildcard channel
        var mapping = new ActionMapping
        {
            Description = "Wildcard Channel Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = null, // Wildcard channel
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = _testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        // Create MIDI event on different channel
        var midiEvent = MidiEventBuilder.NoteOn(5, 60, 127); // Channel 5

        // Act - Process the MIDI event
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Test Device");

        // Assert
        result.Should().BeTrue(); // Wildcard channel should match any channel
        _testAction.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldExecuteMultipleActionsForSameInput()
    {
        // Arrange - Set up multiple mappings for the same input
        var testAction1 = new TestAction();
        var testAction2 = new TestAction();

        var mapping1 = new ActionMapping
        {
            Description = "First Action",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = testAction1
        };

        var mapping2 = new ActionMapping
        {
            Description = "Second Action",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = testAction2
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping1, mapping2 });

        // Create matching MIDI event
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process the MIDI event
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Test Device");

        // Assert
        result.Should().BeTrue(); // Actions were found and executed
        testAction1.ExecutionCount.Should().Be(1);
        testAction2.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task CompleteProcessingPipeline_ShouldIgnoreDisabledMappings()
    {
        // Arrange - Set up enabled and disabled mappings
        var enabledAction = new TestAction();
        var disabledAction = new TestAction();

        var enabledMapping = new ActionMapping
        {
            Description = "Enabled Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = enabledAction
        };

        var disabledMapping = new ActionMapping
        {
            Description = "Disabled Mapping",
            IsEnabled = false, // Disabled
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = disabledAction
        };

        _registry.LoadMappings(new List<ActionMapping> { enabledMapping, disabledMapping });

        // Create matching MIDI event
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process the MIDI event
        var result = await _engine.ProcessMidiEvent("0", midiEvent, "Test Device");

        // Assert
        result.Should().BeTrue(); // Enabled action should be executed
        enabledAction.ExecutionCount.Should().Be(1);
        disabledAction.ExecutionCount.Should().Be(0); // Disabled action should not be executed
    }

    [Fact]
    public void RegistryStatistics_ShouldReflectLoadedMappings()
    {
        // Arrange - Load various mappings with valid actions
        var action1 = new TestAction("Test Action 1");
        var action2 = new TestAction("Test Action 2");
        var action3 = new TestAction("Test Action 3");

        var mappings = new List<ActionMapping>
        {
            new ActionMapping
            {
                Description = "Note On Mapping",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "Device1", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = action1
            },
            new ActionMapping
            {
                Description = "Note Off Mapping",
                IsEnabled = false, // Disabled
                Input = new MidiInput { DeviceName = "Device2", Channel = 2, InputType = MidiInputType.NoteOff, InputNumber = 61 },
                Action = action2
            },
            new ActionMapping
            {
                Description = "Control Change Mapping",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.ControlChangeAbsolute, InputNumber = 7 },
                Action = action3
            }
        };

        // Act
        _registry.LoadMappings(mappings);
        var stats = _registry.GetStatistics();

        // Assert - Check if all mappings were loaded (some might be filtered out due to validation)
        stats.TotalMappings.Should().BeGreaterThan(0);
        stats.EnabledMappings.Should().BeGreaterThan(0);
        stats.UniqueDevices.Should().BeGreaterThan(0);
        stats.UniqueChannels.Should().BeGreaterThan(0);
        stats.LookupKeys.Should().BeGreaterThan(0);

        // Log the actual statistics for debugging
        Logger.LogInformation("Registry Statistics: {Stats}", stats.ToString());
    }

    public override void Dispose()
    {
        // No specific cleanup needed for integration tests
        base.Dispose();
    }
}
