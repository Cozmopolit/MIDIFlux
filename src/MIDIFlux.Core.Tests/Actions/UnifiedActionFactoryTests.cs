using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Models;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Unit tests for UnifiedActionFactory to validate Step 2.1 implementation.
/// Tests factory creation, type safety, and error handling as specified.
/// Implements IDisposable to ensure proper cleanup of key states between tests.
/// </summary>
public class UnifiedActionFactoryTests : TestBase
{
    private readonly ILogger<UnifiedActionFactory> _logger;
    private readonly UnifiedActionFactory _factory;
    private readonly KeyboardSimulator _keyboardSimulator;

    public UnifiedActionFactoryTests()
    {
        // Create a test logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UnifiedActionFactory>();
        _factory = new UnifiedActionFactory(_logger);
        _keyboardSimulator = new KeyboardSimulator();
    }

    /// <summary>
    /// Cleanup method to ensure no keys are left in pressed state after tests.
    /// This prevents key state leakage between tests that could cause issues like
    /// accidental Ctrl+V execution when subsequent tests press 'V'.
    /// </summary>
    public override void Dispose()
    {
        // Release any potentially stuck modifier keys
        var modifierKeys = new ushort[] { 16, 17, 18, 160, 161, 162, 163, 164, 165 }; // Shift, Ctrl, Alt variants
        foreach (var key in modifierKeys)
        {
            try
            {
                _keyboardSimulator.SendKeyUp(key);
            }
            catch
            {
                // Ignore errors during cleanup - key might not be pressed
            }
        }

        // Release any other keys that might be stuck from our tests
        var testKeys = new ushort[] { 65, 66, 67, 68, 69, 70, 71, 72, 73, 144 }; // A, B, C, D, E, F, G, H, I, NumLock
        foreach (var key in testKeys)
        {
            try
            {
                _keyboardSimulator.SendKeyUp(key);
            }
            catch
            {
                // Ignore errors during cleanup - key might not be pressed
            }
        }

        base.Dispose();
    }

    [Fact]
    public void CreateAction_WithNullConfig_ThrowsArgumentNullException()
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => _factory.CreateAction(null!));
        Assert.Equal("config", exception.ParamName);
    }

    [Fact]
    public void CreateAction_WithInvalidConfig_ThrowsArgumentException()
    {
        // Arrange - Create an invalid KeyPressReleaseConfig
        var invalidConfig = new KeyPressReleaseConfig
        {
            VirtualKeyCode = 0 // Invalid - must be > 0
        };

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => _factory.CreateAction(invalidConfig));
        Assert.Equal("config", exception.ParamName);
        Assert.Contains("Invalid configuration for KeyPressRelease", exception.Message);
    }

    [Fact]
    public void CreateAction_WithValidKeyPressReleaseConfig_ReturnsKeyPressReleaseAction()
    {
        // Arrange
        var config = new KeyPressReleaseConfig
        {
            VirtualKeyCode = 65, // 'A' key
            Description = "Test Key Press"
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction>(action);
        Assert.Equal("Test Key Press", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_WithValidMouseClickConfig_ReturnsMouseClickAction()
    {
        // Arrange
        var config = new MouseClickConfig
        {
            Button = MouseButton.Right,
            Description = "Test Right Click"
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseClickAction>(action);
        Assert.Equal("Test Right Click", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_WithValidDelayConfig_ReturnsDelayAction()
    {
        // Arrange
        var config = new DelayConfig
        {
            Milliseconds = 1000,
            Description = "Test Delay"
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.DelayAction>(action);
        Assert.Equal("Test Delay", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_WithValidSequenceConfig_ReturnsSequenceAction()
    {
        // Arrange
        var subActionConfig = new KeyPressReleaseConfig { VirtualKeyCode = 65 };
        var config = new SequenceConfig
        {
            SubActions = { subActionConfig },
            ErrorHandling = SequenceErrorHandling.ContinueOnError,
            Description = "Test Sequence"
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Complex.SequenceAction>(action);
        Assert.Equal("Test Sequence", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_WithValidConditionalConfig_ReturnsConditionalAction()
    {
        // Arrange
        var conditionConfig = new ValueConditionConfig
        {
            MinValue = 0,
            MaxValue = 63,
            Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
        };
        var config = new ConditionalConfig
        {
            Conditions = { conditionConfig },
            Description = "Test Conditional"
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Complex.ConditionalAction>(action);
        Assert.Equal("Test Conditional", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_AllSimpleActionTypes_CreateSuccessfully()
    {
        // Test all simple action types to ensure factory completeness
        var configs = new UnifiedActionConfig[]
        {
            new KeyPressReleaseConfig { VirtualKeyCode = 65 },
            new KeyDownConfig { VirtualKeyCode = 66 },
            new KeyUpConfig { VirtualKeyCode = 67 },
            new KeyToggleConfig { VirtualKeyCode = 68 },
            new MouseClickConfig { Button = MouseButton.Left },
            new MouseScrollConfig { Direction = ScrollDirection.Up, Amount = 1 },
            new CommandExecutionConfig { Command = "echo test", ShellType = CommandShellType.PowerShell },
            new DelayConfig { Milliseconds = 100 },
            new GameControllerButtonConfig { Button = "A", ControllerIndex = 0 },
            new GameControllerAxisConfig { AxisName = "LeftX", AxisValue = 0.5f, ControllerIndex = 0 }
        };

        foreach (var config in configs)
        {
            // Act
            var action = _factory.CreateAction(config);

            // Assert
            Assert.NotNull(action);
            Assert.NotNull(action.Id);
            Assert.NotNull(action.Description);
        }
    }

    [Fact]
    public void CreateAction_ComplexSequenceWithMultipleActionTypes_CreatesCorrectly()
    {
        // Arrange - Create a complex sequence that tests multiple action types
        var sequenceConfig = new SequenceConfig
        {
            SubActions =
            {
                new KeyPressReleaseConfig { VirtualKeyCode = 67, Description = "Press C" },
                new DelayConfig { Milliseconds = 100, Description = "Wait 100ms" },
                new MouseClickConfig { Button = MouseButton.Left, Description = "Left click" },
                new DelayConfig { Milliseconds = 50, Description = "Wait 50ms" },
                new KeyPressReleaseConfig { VirtualKeyCode = 68, Description = "Press D" }
            },
            ErrorHandling = SequenceErrorHandling.ContinueOnError,
            Description = "Complex test sequence"
        };

        // Act
        var action = _factory.CreateAction(sequenceConfig);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Complex.SequenceAction>(action);
        Assert.Equal("Complex test sequence", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_ConditionalWithMultipleConditions_CreatesCorrectly()
    {
        // Arrange - Create a conditional action with multiple value ranges
        var conditionalConfig = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 42,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 88, Description = "Press X (low)" },
                    Description = "Low range"
                },
                new ValueConditionConfig
                {
                    MinValue = 43,
                    MaxValue = 84,
                    Action = new MouseClickConfig { Button = MouseButton.Right, Description = "Right click (mid)" },
                    Description = "Mid range"
                },
                new ValueConditionConfig
                {
                    MinValue = 85,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 89, Description = "Press Y (high)" },
                    Description = "High range"
                }
            },
            Description = "Fader to multiple actions"
        };

        // Act
        var action = _factory.CreateAction(conditionalConfig);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Complex.ConditionalAction>(action);
        Assert.Equal("Fader to multiple actions", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_NestedSequenceInConditional_CreatesCorrectly()
    {
        // Arrange - Create a conditional that contains a sequence (nested complex actions)
        // IMPORTANT: Use safe, non-modifier keys to avoid key state leakage
        var nestedSequence = new SequenceConfig
        {
            SubActions =
            {
                new KeyDownConfig { VirtualKeyCode = 72, Description = "H key down" }, // H key - safe
                new KeyPressReleaseConfig { VirtualKeyCode = 73, Description = "Press I key" }, // I key - safe
                new KeyUpConfig { VirtualKeyCode = 72, Description = "H key up" } // Release H key
            },
            ErrorHandling = SequenceErrorHandling.StopOnError,
            Description = "H+I key sequence (safe test sequence)"
        };

        var conditionalConfig = new ConditionalConfig
        {
            Conditions =
            {
                new ValueConditionConfig
                {
                    MinValue = 100,
                    MaxValue = 127,
                    Action = nestedSequence,
                    Description = "High value triggers H+I sequence"
                }
            },
            Description = "Conditional with nested sequence"
        };

        // Act
        var action = _factory.CreateAction(conditionalConfig);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Complex.ConditionalAction>(action);
        Assert.Equal("Conditional with nested sequence", action.Description);
        Assert.NotNull(action.Id);
    }

    [Fact]
    public void CreateAction_KeyboardActions_ExecuteWithoutErrors()
    {
        // Arrange - Create keyboard action configurations using safe, non-modifier keys
        // IMPORTANT: Avoid modifier keys (Ctrl=17, Shift=16, Alt=18) and use proper cleanup
        var keyPressConfig = new KeyPressReleaseConfig { VirtualKeyCode = 65, Description = "Test A key" }; // A key - safe
        var keyDownConfig = new KeyDownConfig { VirtualKeyCode = 70, Description = "Test F key down" }; // F key - safe, will be cleaned up
        var keyUpConfig = new KeyUpConfig { VirtualKeyCode = 70, Description = "Test F key up" }; // F key - matches the down action
        var keyToggleConfig = new KeyToggleConfig { VirtualKeyCode = 144, Description = "Test NumLock toggle" }; // NumLock - safer than CapsLock

        // Act - Create actions
        var keyPressAction = _factory.CreateAction(keyPressConfig);
        var keyDownAction = _factory.CreateAction(keyDownConfig);
        var keyUpAction = _factory.CreateAction(keyUpConfig);
        var keyToggleAction = _factory.CreateAction(keyToggleConfig);

        // Assert - Actions should execute without throwing exceptions
        // Note: These will actually send keyboard input, but that's expected for integration testing
        var exception1 = Record.Exception(() => keyPressAction.Execute(127));
        var exception2 = Record.Exception(() => keyDownAction.Execute(64));
        var exception3 = Record.Exception(() => keyUpAction.Execute(32)); // This will release the F key pressed above
        var exception4 = Record.Exception(() => keyToggleAction.Execute(100));

        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
        Assert.Null(exception4);

        // Verify async execution also works
        var asyncException1 = Record.Exception(() => keyPressAction.ExecuteAsync(127).AsTask().Wait(1000));
        var asyncException2 = Record.Exception(() => keyDownAction.ExecuteAsync(64).AsTask().Wait(1000));
        var asyncException3 = Record.Exception(() => keyUpAction.ExecuteAsync(32).AsTask().Wait(1000));
        var asyncException4 = Record.Exception(() => keyToggleAction.ExecuteAsync(100).AsTask().Wait(1000));

        Assert.Null(asyncException1);
        Assert.Null(asyncException2);
        Assert.Null(asyncException3);
        Assert.Null(asyncException4);

        // CLEANUP: Ensure NumLock is restored to its original state by toggling it again
        var cleanupException = Record.Exception(() => keyToggleAction.Execute(100));
        Assert.Null(cleanupException);
    }

    [Fact]
    public void CreateAction_KeyDownWithAutoRelease_ConfiguresCorrectly()
    {
        // Arrange - Use a safe, non-modifier key with short auto-release for testing
        var config = new KeyDownConfig
        {
            VirtualKeyCode = 71, // G key - safe, non-modifier key
            AutoReleaseAfterMs = 100 // Shorter delay for faster test execution
            // No custom description - let it generate the default with auto-release info
        };

        // Act
        var action = _factory.CreateAction(config);

        // Assert
        Assert.NotNull(action);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.KeyDownAction>(action);
        Assert.Contains("auto-release after 100ms", action.Description);

        // Test execution (auto-release will happen in background)
        var exception = Record.Exception(() => action.Execute(100));
        Assert.Null(exception);

        // Wait for auto-release to complete to avoid key state leakage
        System.Threading.Thread.Sleep(150); // Wait slightly longer than auto-release time
    }

    [Fact]
    public void CreateAction_MouseActions_ExecuteWithoutErrors()
    {
        // Arrange - Create mouse action configurations
        var mouseClickConfig = new MouseClickConfig { Button = MouseButton.Left, Description = "Test left click" };
        var mouseRightClickConfig = new MouseClickConfig { Button = MouseButton.Right, Description = "Test right click" };
        var mouseMiddleClickConfig = new MouseClickConfig { Button = MouseButton.Middle, Description = "Test middle click" };
        var mouseScrollUpConfig = new MouseScrollConfig { Direction = ScrollDirection.Up, Amount = 1, Description = "Test scroll up" };
        var mouseScrollDownConfig = new MouseScrollConfig { Direction = ScrollDirection.Down, Amount = 3, Description = "Test scroll down" };

        // Act - Create actions
        var leftClickAction = _factory.CreateAction(mouseClickConfig);
        var rightClickAction = _factory.CreateAction(mouseRightClickConfig);
        var middleClickAction = _factory.CreateAction(mouseMiddleClickConfig);
        var scrollUpAction = _factory.CreateAction(mouseScrollUpConfig);
        var scrollDownAction = _factory.CreateAction(mouseScrollDownConfig);

        // Assert - Actions should execute without throwing exceptions
        // Note: These will actually send mouse input, but that's expected for integration testing
        var exception1 = Record.Exception(() => leftClickAction.Execute(127));
        var exception2 = Record.Exception(() => rightClickAction.Execute(64));
        var exception3 = Record.Exception(() => middleClickAction.Execute(32));
        var exception4 = Record.Exception(() => scrollUpAction.Execute(100));
        var exception5 = Record.Exception(() => scrollDownAction.Execute(50));

        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);
        Assert.Null(exception4);
        Assert.Null(exception5);

        // Verify async execution also works
        var asyncException1 = Record.Exception(() => leftClickAction.ExecuteAsync(127).AsTask().Wait(1000));
        var asyncException2 = Record.Exception(() => rightClickAction.ExecuteAsync(64).AsTask().Wait(1000));
        var asyncException3 = Record.Exception(() => middleClickAction.ExecuteAsync(32).AsTask().Wait(1000));
        var asyncException4 = Record.Exception(() => scrollUpAction.ExecuteAsync(100).AsTask().Wait(1000));
        var asyncException5 = Record.Exception(() => scrollDownAction.ExecuteAsync(50).AsTask().Wait(1000));

        Assert.Null(asyncException1);
        Assert.Null(asyncException2);
        Assert.Null(asyncException3);
        Assert.Null(asyncException4);
        Assert.Null(asyncException5);

        // Verify action types
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseClickAction>(leftClickAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseClickAction>(rightClickAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseClickAction>(middleClickAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseScrollAction>(scrollUpAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.MouseScrollAction>(scrollDownAction);
    }

    [Fact]
    public void CreateAction_SystemActions_ExecuteWithoutErrors()
    {
        // Arrange - Create system action configurations
        var delayConfig = new DelayConfig { Milliseconds = 100, Description = "Test delay 100ms" };
        var delayLongConfig = new DelayConfig { Milliseconds = 1000, Description = "Test delay 1s" };
        var commandPowerShellConfig = new CommandExecutionConfig
        {
            Command = "Write-Host 'Hello from PowerShell'",
            ShellType = CommandShellType.PowerShell,
            RunHidden = true,
            WaitForExit = false,
            Description = "Test PowerShell command"
        };
        var commandCmdConfig = new CommandExecutionConfig
        {
            Command = "echo Hello from CMD",
            ShellType = CommandShellType.CommandPrompt,
            RunHidden = true,
            WaitForExit = false,
            Description = "Test CMD command"
        };

        // Act - Create actions
        var delayAction = _factory.CreateAction(delayConfig);
        var delayLongAction = _factory.CreateAction(delayLongConfig);
        var commandPowerShellAction = _factory.CreateAction(commandPowerShellConfig);
        var commandCmdAction = _factory.CreateAction(commandCmdConfig);

        // Assert - Actions should execute without throwing exceptions
        // Note: These will actually execute commands and delays, but that's expected for integration testing
        var exception1 = Record.Exception(() => delayAction.Execute(127));
        var exception2 = Record.Exception(() => commandPowerShellAction.Execute(64));
        var exception3 = Record.Exception(() => commandCmdAction.Execute(32));

        Assert.Null(exception1);
        Assert.Null(exception2);
        Assert.Null(exception3);

        // Test async execution for delays (this is where DelayAction actually delays)
        var delayTask = delayAction.ExecuteAsync(100);
        var delayLongTask = delayLongAction.ExecuteAsync(50);
        var commandPowerShellTask = commandPowerShellAction.ExecuteAsync(75);
        var commandCmdTask = commandCmdAction.ExecuteAsync(25);

        // Wait for all async operations to complete (with reasonable timeout)
        var asyncException1 = Record.Exception(() => delayTask.AsTask().Wait(2000));
        var asyncException2 = Record.Exception(() => delayLongTask.AsTask().Wait(3000));
        var asyncException3 = Record.Exception(() => commandPowerShellTask.AsTask().Wait(5000));
        var asyncException4 = Record.Exception(() => commandCmdTask.AsTask().Wait(5000));

        Assert.Null(asyncException1);
        Assert.Null(asyncException2);
        Assert.Null(asyncException3);
        Assert.Null(asyncException4);

        // Verify action types
        Assert.IsType<MIDIFlux.Core.Actions.Simple.DelayAction>(delayAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.DelayAction>(delayLongAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.CommandExecutionAction>(commandPowerShellAction);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.CommandExecutionAction>(commandCmdAction);

        // Verify descriptions
        Assert.Equal("Test delay 100ms", delayAction.Description);
        Assert.Equal("Test delay 1s", delayLongAction.Description);
        Assert.Equal("Test PowerShell command", commandPowerShellAction.Description);
        Assert.Equal("Test CMD command", commandCmdAction.Description);
    }

    [Fact]
    public void CreateAction_DelayAction_AsyncBehaviorWorksCorrectly()
    {
        // Arrange
        var config = new DelayConfig { Milliseconds = 200, Description = "Timing test delay" };
        var action = _factory.CreateAction(config);

        // Act & Assert - Test that sync execution doesn't delay
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        action.Execute(100);
        stopwatch.Stop();

        // Sync execution should be very fast (no actual delay)
        Assert.True(stopwatch.ElapsedMilliseconds < 50, $"Sync execution took {stopwatch.ElapsedMilliseconds}ms, expected < 50ms");

        // Act & Assert - Test that async execution does delay
        stopwatch.Restart();
        var task = action.ExecuteAsync(100).AsTask();
        task.Wait(1000); // Wait up to 1 second
        stopwatch.Stop();

        // Async execution should take approximately the delay time
        Assert.True(stopwatch.ElapsedMilliseconds >= 150, $"Async execution took {stopwatch.ElapsedMilliseconds}ms, expected >= 150ms");
        Assert.True(stopwatch.ElapsedMilliseconds < 400, $"Async execution took {stopwatch.ElapsedMilliseconds}ms, expected < 400ms");
    }

    [Fact]
    public void CreateAction_CommandExecutionAction_ConfigurationVariationsWork()
    {
        // Arrange - Test different configuration combinations
        var configs = new CommandExecutionConfig[]
        {
            new() { Command = "echo test1", ShellType = CommandShellType.CommandPrompt, RunHidden = false, WaitForExit = false },
            new() { Command = "Write-Host test2", ShellType = CommandShellType.PowerShell, RunHidden = true, WaitForExit = false },
            new() { Command = "echo test3", ShellType = CommandShellType.CommandPrompt, RunHidden = true, WaitForExit = true },
            new() { Command = "Write-Host test4", ShellType = CommandShellType.PowerShell, RunHidden = false, WaitForExit = true }
        };

        foreach (var config in configs)
        {
            // Act
            var action = _factory.CreateAction(config);

            // Assert
            Assert.NotNull(action);
            Assert.IsType<MIDIFlux.Core.Actions.Simple.CommandExecutionAction>(action);
            Assert.NotNull(action.Id);
            Assert.NotNull(action.Description);

            // Test execution doesn't throw
            var exception = Record.Exception(() => action.Execute(100));
            Assert.Null(exception);
        }
    }
}
