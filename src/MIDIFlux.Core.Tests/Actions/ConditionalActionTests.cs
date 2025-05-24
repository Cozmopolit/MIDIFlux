using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;
using System.Diagnostics;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Unit tests for ConditionalAction to validate Step 3.2 implementation.
/// Tests conditional execution, value range checking, error handling, and logging.
/// Implements IDisposable to ensure proper cleanup of key states between tests.
/// </summary>
public class ConditionalActionTests : TestBase
{
    private readonly ILogger<UnifiedActionFactory> _logger;
    private readonly UnifiedActionFactory _factory;
    private readonly KeyboardSimulator _keyboardSimulator;
    private readonly KeyStateManager _keyStateManager;

    public ConditionalActionTests()
    {
        // Create logger for testing
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UnifiedActionFactory>();
        _factory = new UnifiedActionFactory(_logger);
        _keyboardSimulator = new KeyboardSimulator(_logger);

        // Create KeyStateManager for proper key state cleanup
        var keyStateLogger = loggerFactory.CreateLogger<KeyStateManager>();
        _keyStateManager = new KeyStateManager(_keyboardSimulator, keyStateLogger);
    }

    public override void Dispose()
    {
        // Ensure all keys are released after each test to prevent key state leakage
        // This is critical for tests that use KeyDown actions
        _keyStateManager.ReleaseAllKeys();
        base.Dispose();
    }

    [Fact]
    public void ConditionalAction_Constructor_WithValidConfig_CreatesSuccessfully()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 63,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 65, Description = "Press A" },
                    Description = "Low range -> A"
                },
                new ValueConditionConfig
                {
                    MinValue = 64,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 66, Description = "Press B" },
                    Description = "High range -> B"
                }
            },
            Description = "Fader to A/B keys"
        };

        // Act
        var action = new ConditionalAction(config, _factory);

        // Assert
        Assert.NotNull(action);
        Assert.Equal("Fader to A/B keys", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void ConditionalAction_Constructor_WithEmptyConditions_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions = { }, // Empty list
            Description = "Empty conditional"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConditionalAction(config, _factory));
        Assert.Contains("ConditionalConfig must contain at least one condition", exception.Message);
    }

    [Fact]
    public void ConditionalAction_Constructor_WithOverlappingRanges_ThrowsArgumentException()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 70,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                },
                new ValueConditionConfig
                {
                    MinValue = 60, // Overlaps with previous range
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 66 }
                }
            },
            Description = "Overlapping ranges"
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new ConditionalAction(config, _factory));
        Assert.Contains("Conditions have overlapping value ranges", exception.Message);
    }

    [Fact]
    public void ConditionalAction_Execute_WithLowValue_ExecutesFirstCondition()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 63,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 74, Description = "Press J" }, // J key - safe
                    Description = "Low range -> J"
                },
                new ValueConditionConfig
                {
                    MinValue = 64,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 75, Description = "Press K" }, // K key - safe
                    Description = "High range -> K"
                }
            },
            Description = "Fader to J/K keys"
        };
        var action = new ConditionalAction(config, _factory);

        // Act & Assert - Should execute without throwing
        var exception = Record.Exception(() => action.Execute(30)); // Low value
        Assert.Null(exception);
    }

    [Fact]
    public void ConditionalAction_Execute_WithHighValue_ExecutesSecondCondition()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 63,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 76, Description = "Press L" }, // L key - safe
                    Description = "Low range -> L"
                },
                new ValueConditionConfig
                {
                    MinValue = 64,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 77, Description = "Press M" }, // M key - safe
                    Description = "High range -> M"
                }
            },
            Description = "Fader to L/M keys"
        };
        var action = new ConditionalAction(config, _factory);

        // Act & Assert - Should execute without throwing
        var exception = Record.Exception(() => action.Execute(100)); // High value
        Assert.Null(exception);
    }

    [Fact]
    public void ConditionalAction_Execute_WithNoMatchingValue_DoesNothing()
    {
        // Arrange - Create conditions with gaps
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 30,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 78, Description = "Press N" }, // N key - safe
                    Description = "Low range -> N"
                },
                new ValueConditionConfig
                {
                    MinValue = 70,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 79, Description = "Press O" }, // O key - safe
                    Description = "High range -> O"
                }
            },
            Description = "Fader with gap"
        };
        var action = new ConditionalAction(config, _factory);

        // Act & Assert - Should execute without throwing (value 50 falls in gap)
        var exception = Record.Exception(() => action.Execute(50));
        Assert.Null(exception);
    }

    [Fact]
    public void ConditionalAction_Execute_WithoutMidiValue_DoesNothing()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 80, Description = "Press P" }, // P key - safe
                    Description = "Any value -> P"
                }
            },
            Description = "Any value conditional"
        };
        var action = new ConditionalAction(config, _factory);

        // Act & Assert - Should execute without throwing (no MIDI value provided)
        var exception = Record.Exception(() => action.Execute(null));
        Assert.Null(exception);
    }

    [Fact]
    public async Task ConditionalAction_ExecuteAsync_WithLowValue_ExecutesFirstConditionAsync()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 63,
                    Action = new DelayConfig { Milliseconds = 100, Description = "Delay 100ms" },
                    Description = "Low range -> Delay"
                },
                new ValueConditionConfig
                {
                    MinValue = 64,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 81, Description = "Press Q" }, // Q key - safe
                    Description = "High range -> Q"
                }
            },
            Description = "Async conditional test"
        };
        var action = new ConditionalAction(config, _factory);

        // Act - Time the async execution
        var stopwatch = Stopwatch.StartNew();
        await action.ExecuteAsync(30); // Low value - should trigger delay
        stopwatch.Stop();

        // Assert - Should take at least the delay time
        Assert.True(stopwatch.ElapsedMilliseconds >= 80,
            $"Async execution took {stopwatch.ElapsedMilliseconds}ms, expected >= 80ms");
    }

    [Fact]
    public void ConditionalAction_ToString_ReturnsDescription()
    {
        // Arrange
        var config = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 82, Description = "Press R" } // R key - safe
                }
            },
            Description = "Test conditional description"
        };
        var action = new ConditionalAction(config, _factory);

        // Act
        var result = action.ToString();

        // Assert
        Assert.Equal("Test conditional description", result);
    }
}
