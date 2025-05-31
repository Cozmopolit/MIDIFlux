using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Stateful;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Infrastructure;
using Moq;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Tests for stateful actions - 100% internal testing without external side effects
/// </summary>
public class StatefulActionTests : ActionTestBase
{
    public StatefulActionTests()
    {
        // Use the inherited StateManager from TestBase
        // ActionBase.ServiceProvider is already set by TestBase

        // Ensure completely clean state for each test
        EnsureCleanTestState();
    }

    /// <summary>
    /// Ensures completely clean state before each test
    /// </summary>
    private void EnsureCleanTestState()
    {
        // Always ensure service provider is set first
        ActionBase.ServiceProvider = ServiceProvider;

        // Clear all states
        StateManager.ClearAllStates();

        // Verify ActionStateManager is available
        var stateManager = ActionBase.ServiceProvider?.GetService<ActionStateManager>();
        if (stateManager == null)
        {
            throw new InvalidOperationException("ActionStateManager service is not available in test setup");
        }
    }

    [Fact]
    public async Task StateSetAction_ShouldSetStateValue()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test

        var action = new StateSetAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "Value", 42 }
        };

        // Act
        await action.ExecuteAsync(127); // MIDI value should be ignored for StateSet

        // Assert
        var stateValue = StateManager.GetState("TestState");
        stateValue.Should().Be(42);
    }

    [Fact]
    public async Task StateIncreaseAction_ShouldIncreaseExistingState()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 10);

        var action = new StateIncreaseAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "Value", 5 }
        };

        // Ensure service provider is set for this specific action
        ActionBase.ServiceProvider = ServiceProvider;

        // Act
        await action.ExecuteAsync(127);

        // Assert
        var stateValue = StateManager.GetState("TestState");
        stateValue.Should().Be(15); // 10 + 5
    }

    [Fact]
    public async Task StateIncreaseAction_ShouldCreateStateIfNotExists()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test

        var action = new StateIncreaseAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "NewState" },
            { "Value", 7 }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        var stateValue = StateManager.GetState("NewState");
        stateValue.Should().Be(7); // 0 + 7 (non-existent state treated as 0)
    }

    [Fact]
    public async Task StateDecreaseAction_ShouldDecreaseExistingState()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 20);

        var action = new StateDecreaseAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "Value", 8 }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        var stateValue = StateManager.GetState("TestState");
        stateValue.Should().Be(12); // 20 - 8
    }

    [Fact]
    public async Task StateDecreaseAction_ShouldCreateNegativeStateIfNotExists()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test

        var action = new StateDecreaseAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "NewState" },
            { "Value", 3 }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        var stateValue = StateManager.GetState("NewState");
        stateValue.Should().Be(-3); // 0 - 3
    }

    [Fact]
    public async Task StateConditionalAction_ShouldExecuteWhenConditionMet()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 15);

        var testAction = new TestAction("Conditional Test Action");
        var action = new StateConditionalAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "ComparisonType", "GreaterThan" },
            { "ComparisonValue", 10 },
            { "TrueAction", testAction }
        };

        // Ensure service provider is set for this specific action
        ActionBase.ServiceProvider = ServiceProvider;

        // Act
        await action.ExecuteAsync(127);

        // Assert
        testAction.ExecutionCount.Should().Be(1);
        testAction.LastMidiValue.Should().Be(127);
    }

    [Fact]
    public async Task StateConditionalAction_ShouldNotExecuteWhenConditionNotMet()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 5);

        var testAction = new TestAction("Conditional Test Action");
        var action = new StateConditionalAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "ComparisonType", "GreaterThan" },
            { "ComparisonValue", 10 },
            { "TrueAction", testAction }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        testAction.ExecutionCount.Should().Be(0); // Condition not met (5 > 10 is false)
    }

    [Fact]
    public async Task StateConditionalAction_ShouldSupportEqualsOperator()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 42);

        var testAction = new TestAction("Conditional Test Action");
        var action = new StateConditionalAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "ComparisonType", "Equals" },
            { "ComparisonValue", 42 },
            { "TrueAction", testAction }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        testAction.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public async Task StateConditionalAction_ShouldSupportLessThanOperator()
    {
        // Arrange
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("TestState", 3);

        var testAction = new TestAction("Conditional Test Action");
        var action = new StateConditionalAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "TestState" },
            { "ComparisonType", "LessThan" },
            { "ComparisonValue", 5 },
            { "TrueAction", testAction }
        };

        // Act
        await action.ExecuteAsync(127);

        // Assert
        testAction.ExecutionCount.Should().Be(1);
    }

    [Fact]
    public void StatefulActions_ShouldValidateParameters()
    {
        // Arrange & Act & Assert
        var stateSetAction = new StateSetAction();
        // StateSetAction has default values, so it's valid by default
        stateSetAction.IsValid().Should().BeTrue();

        var stateIncreaseAction = new StateIncreaseAction();
        stateIncreaseAction.IsValid().Should().BeFalse(); // No Value parameter set

        var stateDecreaseAction = new StateDecreaseAction();
        stateDecreaseAction.IsValid().Should().BeFalse(); // No Value parameter set

        var stateConditionalAction = new StateConditionalAction();
        stateConditionalAction.IsValid().Should().BeFalse(); // No ComparisonType parameter set
    }

    [Fact]
    public void StatefulActions_ShouldHaveCorrectInputCategories()
    {
        // Arrange
        var stateSetAction = new StateSetAction();
        var stateIncreaseAction = new StateIncreaseAction();
        var stateDecreaseAction = new StateDecreaseAction();
        var stateConditionalAction = new StateConditionalAction();

        // Act & Assert - Check what input categories each action actually supports
        var stateSetCategories = stateSetAction.GetCompatibleInputCategories();
        var stateIncreaseCategories = stateIncreaseAction.GetCompatibleInputCategories();
        var stateDecreaseCategories = stateDecreaseAction.GetCompatibleInputCategories();
        var stateConditionalCategories = stateConditionalAction.GetCompatibleInputCategories();

        // All stateful actions should support at least trigger inputs
        stateSetCategories.Should().Contain(InputTypeCategory.Trigger);
        stateIncreaseCategories.Should().Contain(InputTypeCategory.Trigger);
        stateDecreaseCategories.Should().Contain(InputTypeCategory.Trigger);
        stateConditionalCategories.Should().Contain(InputTypeCategory.Trigger);
    }

    [Fact]
    public async Task StatefulActionCombination_ShouldWorkTogether()
    {
        // Arrange - Create a complex scenario with multiple stateful actions
        EnsureCleanTestState(); // Ensure clean state before test
        StateManager.SetState("Counter", 0);

        var increaseAction = new StateIncreaseAction();
        increaseAction.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "Counter" },
            { "Value", 1 }
        };

        var testAction = new TestAction("Conditional Action");
        var conditionalAction = new StateConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "StateKey", "Counter" },
            { "ComparisonType", "GreaterThan" },
            { "ComparisonValue", 2 },
            { "TrueAction", testAction }
        };

        // Ensure service provider is set for all actions
        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Simulate multiple MIDI triggers
        await increaseAction.ExecuteAsync(127); // Counter = 1
        await conditionalAction.ExecuteAsync(127); // Should not execute (1 > 2 is false)

        await increaseAction.ExecuteAsync(127); // Counter = 2
        await conditionalAction.ExecuteAsync(127); // Should not execute (2 > 2 is false)

        await increaseAction.ExecuteAsync(127); // Counter = 3
        await conditionalAction.ExecuteAsync(127); // Should execute (3 > 2 is true)

        // Assert
        StateManager.GetState("Counter").Should().Be(3);
        testAction.ExecutionCount.Should().Be(1); // Only executed once when condition was met
    }

    public override void Dispose()
    {
        // No specific cleanup needed
        base.Dispose();
    }
}
