using FluentAssertions;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Tests.Infrastructure;
using MIDIFlux.Core.Tests.Utilities;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Tests for ActionMappingRegistry - the performance-critical lookup system
/// </summary>
public class ActionMappingRegistryTests : TestBase
{
    private readonly ActionMappingRegistry _registry;

    public ActionMappingRegistryTests()
    {
        _registry = new ActionMappingRegistry(CreateLogger<ActionMappingRegistry>());
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act & Assert
        _registry.Should().NotBeNull();
        var stats = _registry.GetStatistics();
        stats.TotalMappings.Should().Be(0);
        stats.EnabledMappings.Should().Be(0);
    }

    [Fact]
    public void LoadMappings_ShouldHandleEmptyList()
    {
        // Arrange
        var emptyMappings = new List<ActionMapping>();

        // Act
        _registry.LoadMappings(emptyMappings);

        // Assert
        var stats = _registry.GetStatistics();
        stats.TotalMappings.Should().Be(0);
        stats.EnabledMappings.Should().Be(0);
    }

    [Fact]
    public void LoadMappings_ShouldLoadSingleMapping()
    {
        // Arrange
        var action = new TestAction("Test Action");

        var mapping = new ActionMapping
        {
            Description = "Test Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = action
        };

        var mappings = new List<ActionMapping> { mapping };

        // Act
        _registry.LoadMappings(mappings);

        // Assert
        var stats = _registry.GetStatistics();
        stats.TotalMappings.Should().Be(1);
        stats.EnabledMappings.Should().Be(1);
    }

    [Fact]
    public void FindActions_ShouldReturnEmptyForNoMatches()
    {
        // Arrange
        var input = new MidiInput
        {
            DeviceName = "Nonexistent Device",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert
        actions.Should().BeEmpty();
    }

    [Fact]
    public void FindActions_ShouldReturnExactMatch()
    {
        // Arrange
        var action = new TestAction("Test Action");

        var mapping = new ActionMapping
        {
            Description = "Test Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = action
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        var input = new MidiInput
        {
            DeviceName = "Test Device",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert
        actions.Should().HaveCount(1);
        actions[0].Should().Be(action);
    }

    [Fact]
    public void FindActions_ShouldIgnoreDisabledMappings()
    {
        // Arrange
        var action = new TestAction("Test Action");

        var mapping = new ActionMapping
        {
            Description = "Test Mapping",
            IsEnabled = false, // Disabled
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = action
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        var input = new MidiInput
        {
            DeviceName = "Test Device",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert
        actions.Should().BeEmpty();
    }

    [Fact]
    public void FindActions_ShouldSupportWildcardDevice()
    {
        // Arrange
        var action = new TestAction("Test Action");

        var mapping = new ActionMapping
        {
            Description = "Test Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "*", // Wildcard device
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = action
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        var input = new MidiInput
        {
            DeviceName = "Any Device",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert
        actions.Should().HaveCount(1);
        actions[0].Should().Be(action);
    }

    [Fact]
    public void FindActions_ShouldSupportWildcardChannel()
    {
        // Arrange
        var action = new TestAction("Test Action");

        var mapping = new ActionMapping
        {
            Description = "Test Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "Test Device",
                Channel = null, // Wildcard channel
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = action
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });

        var input = new MidiInput
        {
            DeviceName = "Test Device",
            Channel = 5, // Any channel
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert
        actions.Should().HaveCount(1);
        actions[0].Should().Be(action);
    }

    [Fact]
    public void LoadMappings_ShouldReplaceExistingMappings()
    {
        // Arrange
        var action1 = new TestAction("Test Action 1");
        var action2 = new TestAction("Test Action 2");

        var mapping1 = new ActionMapping
        {
            IsEnabled = true,
            Input = new MidiInput { DeviceName = "Device1", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
            Action = action1
        };

        var mapping2 = new ActionMapping
        {
            IsEnabled = true,
            Input = new MidiInput { DeviceName = "Device2", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 61 },
            Action = action2
        };

        // Act
        _registry.LoadMappings(new List<ActionMapping> { mapping1 });
        var statsAfterFirst = _registry.GetStatistics();

        _registry.LoadMappings(new List<ActionMapping> { mapping2 });
        var statsAfterSecond = _registry.GetStatistics();

        // Assert
        statsAfterFirst.TotalMappings.Should().Be(1);
        statsAfterSecond.TotalMappings.Should().Be(1); // Should replace, not add
    }

    [Fact]
    public void FourStepLookup_ShouldPrioritizeExactDeviceAndChannelMatch()
    {
        // Arrange - Set up mappings at different priority levels
        var exactAction = new TestAction("Exact Match");
        var deviceWildcardAction = new TestAction("Device Wildcard");
        var channelWildcardAction = new TestAction("Channel Wildcard");
        var bothWildcardAction = new TestAction("Both Wildcard");

        var mappings = new List<ActionMapping>
        {
            // Step 1: Exact device + channel (highest priority)
            new ActionMapping
            {
                Description = "Exact Match",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "TestDevice", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = exactAction
            },
            // Step 2: Exact device + wildcard channel
            new ActionMapping
            {
                Description = "Device Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "TestDevice", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = deviceWildcardAction
            },
            // Step 3: Wildcard device + exact channel
            new ActionMapping
            {
                Description = "Channel Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = channelWildcardAction
            },
            // Step 4: Wildcard device + wildcard channel (lowest priority)
            new ActionMapping
            {
                Description = "Both Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = bothWildcardAction
            }
        };

        _registry.LoadMappings(mappings);

        var input = new MidiInput
        {
            DeviceName = "TestDevice",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert - Should only return the exact match (Step 1), not the others
        actions.Should().HaveCount(1);
        actions[0].Should().Be(exactAction);
    }

    [Fact]
    public void FourStepLookup_ShouldFallbackToDeviceWildcardWhenNoExactMatch()
    {
        // Arrange - No exact match, but device wildcard exists
        var deviceWildcardAction = new TestAction("Device Wildcard");
        var channelWildcardAction = new TestAction("Channel Wildcard");
        var bothWildcardAction = new TestAction("Both Wildcard");

        var mappings = new List<ActionMapping>
        {
            // Step 2: Exact device + wildcard channel
            new ActionMapping
            {
                Description = "Device Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "TestDevice", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = deviceWildcardAction
            },
            // Step 3: Wildcard device + exact channel
            new ActionMapping
            {
                Description = "Channel Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = channelWildcardAction
            },
            // Step 4: Wildcard device + wildcard channel
            new ActionMapping
            {
                Description = "Both Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = bothWildcardAction
            }
        };

        _registry.LoadMappings(mappings);

        var input = new MidiInput
        {
            DeviceName = "TestDevice",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert - Should return Step 2 (device wildcard), not Step 3 or 4
        actions.Should().HaveCount(1);
        actions[0].Should().Be(deviceWildcardAction);
    }

    [Fact]
    public void FourStepLookup_ShouldFallbackToChannelWildcardWhenNoDeviceMatch()
    {
        // Arrange - No exact or device wildcard match, but channel wildcard exists
        var channelWildcardAction = new TestAction("Channel Wildcard");
        var bothWildcardAction = new TestAction("Both Wildcard");

        var mappings = new List<ActionMapping>
        {
            // Step 3: Wildcard device + exact channel
            new ActionMapping
            {
                Description = "Channel Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = channelWildcardAction
            },
            // Step 4: Wildcard device + wildcard channel
            new ActionMapping
            {
                Description = "Both Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = bothWildcardAction
            }
        };

        _registry.LoadMappings(mappings);

        var input = new MidiInput
        {
            DeviceName = "TestDevice",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert - Should return Step 3 (channel wildcard), not Step 4
        actions.Should().HaveCount(1);
        actions[0].Should().Be(channelWildcardAction);
    }

    [Fact]
    public void FourStepLookup_ShouldFallbackToBothWildcardAsLastResort()
    {
        // Arrange - Only both wildcard match exists
        var bothWildcardAction = new TestAction("Both Wildcard");

        var mappings = new List<ActionMapping>
        {
            // Step 4: Wildcard device + wildcard channel (last resort)
            new ActionMapping
            {
                Description = "Both Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = bothWildcardAction
            }
        };

        _registry.LoadMappings(mappings);

        var input = new MidiInput
        {
            DeviceName = "TestDevice",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert - Should return Step 4 (both wildcard)
        actions.Should().HaveCount(1);
        actions[0].Should().Be(bothWildcardAction);
    }

    [Fact]
    public void FourStepLookup_ShouldReturnMultipleActionsFromSamePriorityLevel()
    {
        // Arrange - Multiple exact matches at same priority level
        var exactAction1 = new TestAction("Exact Match 1");
        var exactAction2 = new TestAction("Exact Match 2");
        var wildcardAction = new TestAction("Wildcard");

        var mappings = new List<ActionMapping>
        {
            // Two exact matches (same priority level)
            new ActionMapping
            {
                Description = "Exact Match 1",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "TestDevice", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = exactAction1
            },
            new ActionMapping
            {
                Description = "Exact Match 2",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "TestDevice", Channel = 1, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = exactAction2
            },
            // Lower priority wildcard (should be ignored)
            new ActionMapping
            {
                Description = "Wildcard",
                IsEnabled = true,
                Input = new MidiInput { DeviceName = "*", Channel = null, InputType = MidiInputType.NoteOn, InputNumber = 60 },
                Action = wildcardAction
            }
        };

        _registry.LoadMappings(mappings);

        var input = new MidiInput
        {
            DeviceName = "TestDevice",
            Channel = 1,
            InputType = MidiInputType.NoteOn,
            InputNumber = 60
        };

        // Act
        var actions = _registry.FindActions(input);

        // Assert - Should return both exact matches, but not the wildcard
        actions.Should().HaveCount(2);
        actions.Should().Contain(exactAction1);
        actions.Should().Contain(exactAction2);
        actions.Should().NotContain(wildcardAction);
    }

    public override void Dispose()
    {
        // No specific cleanup needed for ActionMappingRegistry
        base.Dispose();
    }
}
