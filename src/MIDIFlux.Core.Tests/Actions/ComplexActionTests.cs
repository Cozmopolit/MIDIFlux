using FluentAssertions;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Comprehensive tests for complex actions (workflow logic) in Phase 2.
/// Tests SequenceAction, ConditionalAction, and AlternatingAction with focus on:
/// - Workflow orchestration logic
/// - Parameter validation and error handling
/// - Action serialization/deserialization
/// - Sub-action coordination without external dependencies
/// </summary>
public class ComplexActionTests : ActionTestBase
{
    public ComplexActionTests()
    {
        // Ensure clean state for each test
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
        var stateManager = ActionBase.ServiceProvider?.GetService(typeof(ActionStateManager)) as ActionStateManager;
        if (stateManager == null)
        {
            throw new InvalidOperationException("ActionStateManager service is not available in test setup");
        }
    }

    #region SequenceAction Tests

    [Fact]
    public async Task SequenceAction_ShouldExecuteActionsInOrder()
    {
        // Arrange
        EnsureCleanTestState();

        var testAction1 = new TestAction("Action 1");
        var testAction2 = new TestAction("Action 2");
        var testAction3 = new TestAction("Action 3");

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { testAction1, testAction2, testAction3 } },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        // Ensure service provider is set
        ActionBase.ServiceProvider = ServiceProvider;

        // Act
        await sequenceAction.ExecuteAsync(100);

        // Assert
        testAction1.ExecutionCount.Should().Be(1);
        testAction2.ExecutionCount.Should().Be(1);
        testAction3.ExecutionCount.Should().Be(1);

        testAction1.LastMidiValue.Should().Be(100);
        testAction2.LastMidiValue.Should().Be(100);
        testAction3.LastMidiValue.Should().Be(100);
    }

    [Fact]
    public async Task SequenceAction_ShouldContinueOnErrorWhenConfigured()
    {
        // Arrange
        EnsureCleanTestState();

        var testAction1 = new TestAction("Action 1");
        var failingAction = new FailingTestAction("Failing Action");
        var testAction3 = new TestAction("Action 3");

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { testAction1, failingAction, testAction3 } },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sequenceAction.ExecuteAsync(100));

        // Should execute action 1, fail on action 2, but continue to action 3
        testAction1.ExecutionCount.Should().Be(1);
        failingAction.ExecutionCount.Should().Be(1);
        testAction3.ExecutionCount.Should().Be(1);

        exception.Message.Should().Contain("Error in sequence step 2");
    }

    [Fact]
    public async Task SequenceAction_ShouldStopOnErrorWhenConfigured()
    {
        // Arrange
        EnsureCleanTestState();

        var testAction1 = new TestAction("Action 1");
        var failingAction = new FailingTestAction("Failing Action");
        var testAction3 = new TestAction("Action 3");

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { testAction1, failingAction, testAction3 } },
            { "ErrorHandling", SequenceErrorHandling.StopOnError }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await sequenceAction.ExecuteAsync(100));

        // Should execute action 1, fail on action 2, and NOT execute action 3
        testAction1.ExecutionCount.Should().Be(1);
        failingAction.ExecutionCount.Should().Be(1);
        testAction3.ExecutionCount.Should().Be(0); // Should not execute due to StopOnError

        exception.Message.Should().Contain("Error in sequence step 2");
    }

    [Fact]
    public void SequenceAction_ShouldValidateEmptySubActions()
    {
        // Arrange
        EnsureCleanTestState();

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase>() },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        // Act
        var isValid = sequenceAction.IsValid();

        // Assert
        isValid.Should().BeTrue(); // Empty sequence is valid (no-op)
        sequenceAction.GetValidationErrors().Should().BeEmpty();
    }

    [Fact]
    public void SequenceAction_ShouldValidateSubActionErrors()
    {
        // Arrange
        EnsureCleanTestState();

        var invalidAction = new InvalidTestAction("Invalid Action");

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { invalidAction } },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        // Act
        var isValid = sequenceAction.IsValid();

        // Assert
        isValid.Should().BeFalse();
        var errors = sequenceAction.GetValidationErrors();
        errors.Should().NotBeEmpty();
        errors.Should().Contain(error => error.Contains("Sub-action") && error.Contains("Invalid"));
    }

    #endregion

    #region ConditionalAction Tests

    [Fact]
    public async Task ConditionalAction_ShouldExecuteMatchingCondition()
    {
        // Arrange
        EnsureCleanTestState();

        var lowAction = new TestAction("Low Action");
        var midAction = new TestAction("Mid Action");
        var highAction = new TestAction("High Action");

        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 42, Action = lowAction, Description = "Low Range" },
            new() { MinValue = 43, MaxValue = 84, Action = midAction, Description = "Mid Range" },
            new() { MinValue = 85, MaxValue = 127, Action = highAction, Description = "High Range" }
        };

        var conditionalAction = new ConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Test low range
        await conditionalAction.ExecuteAsync(30);

        // Assert
        lowAction.ExecutionCount.Should().Be(1);
        midAction.ExecutionCount.Should().Be(0);
        highAction.ExecutionCount.Should().Be(0);
        lowAction.LastMidiValue.Should().Be(30);
    }

    [Fact]
    public async Task ConditionalAction_ShouldExecuteFirstMatchingCondition()
    {
        // Arrange
        EnsureCleanTestState();

        var action1 = new TestAction("Action 1");
        var action2 = new TestAction("Action 2");

        // Create overlapping conditions - first match should win
        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 100, Action = action1, Description = "First Range" },
            new() { MinValue = 50, MaxValue = 127, Action = action2, Description = "Second Range" }
        };

        var conditionalAction = new ConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Value 75 matches both conditions
        await conditionalAction.ExecuteAsync(75);

        // Assert - Only first matching condition should execute
        action1.ExecutionCount.Should().Be(1);
        action2.ExecutionCount.Should().Be(0);
        action1.LastMidiValue.Should().Be(75);
    }

    [Fact]
    public async Task ConditionalAction_ShouldHandleNoMatchingCondition()
    {
        // Arrange
        EnsureCleanTestState();

        var action = new TestAction("Test Action");

        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 50, Action = action, Description = "Low Range" }
        };

        var conditionalAction = new ConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Value outside any condition range
        await conditionalAction.ExecuteAsync(100);

        // Assert - No action should execute (not an error)
        action.ExecutionCount.Should().Be(0);
    }

    [Fact]
    public async Task ConditionalAction_ShouldHandleNullMidiValue()
    {
        // Arrange
        EnsureCleanTestState();

        var action = new TestAction("Test Action");

        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 127, Action = action, Description = "Full Range" }
        };

        var conditionalAction = new ConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Null MIDI value
        await conditionalAction.ExecuteAsync(null);

        // Assert - No action should execute
        action.ExecutionCount.Should().Be(0);
    }

    #endregion

    #region AlternatingAction Tests

    [Fact]
    public async Task AlternatingAction_ShouldAlternateBetweenActions()
    {
        // Arrange
        EnsureCleanTestState();

        var primaryAction = new TestAction("Primary Action");
        var secondaryAction = new TestAction("Secondary Action");

        var alternatingAction = new AlternatingAction();
        alternatingAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { primaryAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", true }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Execute multiple times
        await alternatingAction.ExecuteAsync(100);
        await alternatingAction.ExecuteAsync(101);
        await alternatingAction.ExecuteAsync(102);
        await alternatingAction.ExecuteAsync(103);

        // Assert - Should alternate: Primary, Secondary, Primary, Secondary
        primaryAction.ExecutionCount.Should().Be(2);
        secondaryAction.ExecutionCount.Should().Be(2);

        primaryAction.ExecutionHistory.Should().Equal(100, 102);
        secondaryAction.ExecutionHistory.Should().Equal(101, 103);
    }

    [Fact]
    public async Task AlternatingAction_ShouldStartWithSecondaryWhenConfigured()
    {
        // Arrange
        EnsureCleanTestState();

        var primaryAction = new TestAction("Primary Action");
        var secondaryAction = new TestAction("Secondary Action");

        var alternatingAction = new AlternatingAction();
        alternatingAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { primaryAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", false }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Execute twice
        await alternatingAction.ExecuteAsync(100);
        await alternatingAction.ExecuteAsync(101);

        // Assert - Should start with secondary: Secondary, Primary
        primaryAction.ExecutionCount.Should().Be(1);
        secondaryAction.ExecutionCount.Should().Be(1);

        secondaryAction.LastMidiValue.Should().Be(100); // First execution
        primaryAction.LastMidiValue.Should().Be(101);   // Second execution
    }

    [Fact]
    public void AlternatingAction_ShouldValidateEmptyActions()
    {
        // Arrange
        EnsureCleanTestState();

        var alternatingAction = new AlternatingAction();
        alternatingAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase>() },
            { "SecondaryAction", new List<ActionBase>() },
            { "StartWithPrimary", true }
        };

        // Act
        var isValid = alternatingAction.IsValid();

        // Assert
        isValid.Should().BeTrue(); // Empty actions are valid (no-op)
        alternatingAction.GetValidationErrors().Should().BeEmpty();
    }

    [Fact]
    public async Task AlternatingAction_ShouldHandleErrorInPrimaryAction()
    {
        // Arrange
        EnsureCleanTestState();

        var failingAction = new FailingTestAction("Failing Primary");
        var secondaryAction = new TestAction("Secondary Action");

        var alternatingAction = new AlternatingAction();
        alternatingAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { failingAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", true }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await alternatingAction.ExecuteAsync(100));

        // Should fail on primary action
        failingAction.ExecutionCount.Should().Be(1);
        secondaryAction.ExecutionCount.Should().Be(0);
        exception.Message.Should().Contain("Error executing Primary action");
    }

    #endregion

    #region ValueCondition Tests

    [Fact]
    public void ValueCondition_ShouldValidateCorrectly()
    {
        // Arrange
        var validCondition = new ValueCondition
        {
            MinValue = 0,
            MaxValue = 127,
            Action = new TestAction("Valid Action"),
            Description = "Valid Condition"
        };

        // Act
        var isValid = validCondition.IsValid();

        // Assert
        isValid.Should().BeTrue();
        validCondition.GetValidationErrors().Should().BeEmpty();
    }

    [Fact]
    public void ValueCondition_ShouldDetectInvalidRange()
    {
        // Arrange
        var invalidCondition = new ValueCondition
        {
            MinValue = 100,
            MaxValue = 50, // Invalid: min > max
            Action = new TestAction("Test Action"),
            Description = "Invalid Range"
        };

        // Act
        var isValid = invalidCondition.IsValid();

        // Assert
        isValid.Should().BeFalse();
        var errors = invalidCondition.GetValidationErrors();
        errors.Should().Contain("MinValue must be <= MaxValue");
    }

    [Fact]
    public void ValueCondition_ShouldDetectOutOfBoundsValues()
    {
        // Arrange
        var invalidCondition = new ValueCondition
        {
            MinValue = -10, // Invalid: < 0
            MaxValue = 200, // Invalid: > 127
            Action = new TestAction("Test Action")
        };

        // Act
        var isValid = invalidCondition.IsValid();

        // Assert
        isValid.Should().BeFalse();
        var errors = invalidCondition.GetValidationErrors();
        errors.Should().Contain("MinValue must be >= 0");
        errors.Should().Contain("MaxValue must be <= 127");
    }

    [Fact]
    public void ValueCondition_ShouldDetectInvalidAction()
    {
        // Arrange
        var invalidCondition = new ValueCondition
        {
            MinValue = 0,
            MaxValue = 127,
            Action = new InvalidTestAction("Invalid Action")
        };

        // Act
        var isValid = invalidCondition.IsValid();

        // Assert
        isValid.Should().BeFalse();
        var errors = invalidCondition.GetValidationErrors();
        errors.Should().Contain(error => error.Contains("Action:") && error.Contains("invalid"));
    }

    [Theory]
    [InlineData(0, 0, 0, true)]
    [InlineData(0, 127, 64, true)]
    [InlineData(50, 100, 75, true)]
    [InlineData(50, 100, 49, false)]
    [InlineData(50, 100, 101, false)]
    [InlineData(64, 64, 64, true)]
    public void ValueCondition_IsInRange_ShouldWorkCorrectly(int minValue, int maxValue, int testValue, bool expectedResult)
    {
        // Arrange
        var condition = new ValueCondition
        {
            MinValue = minValue,
            MaxValue = maxValue,
            Action = new TestAction("Test Action")
        };

        // Act
        var result = condition.IsInRange(testValue);

        // Assert
        result.Should().Be(expectedResult);
    }

    #endregion

    #region Parameter Validation Tests

    [Fact]
    public void ConditionalAction_ShouldValidateConditionList()
    {
        // Arrange
        EnsureCleanTestState();

        var validCondition = new ValueCondition
        {
            MinValue = 0,
            MaxValue = 127,
            Action = new TestAction("Valid Action")
        };

        var invalidCondition = new ValueCondition
        {
            MinValue = 100,
            MaxValue = 50, // Invalid range
            Action = new TestAction("Test Action")
        };

        var conditionalAction = new ConditionalAction();
        conditionalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", new List<ValueCondition> { validCondition, invalidCondition } }
        };

        // Act
        var isValid = conditionalAction.IsValid();

        // Assert
        isValid.Should().BeFalse();
        var errors = conditionalAction.GetValidationErrors();
        errors.Should().Contain(error => error.Contains("Condition 2") && error.Contains("MinValue must be <= MaxValue"));
    }

    [Fact]
    public void SequenceAction_ShouldValidateErrorHandlingParameter()
    {
        // Arrange
        EnsureCleanTestState();

        var sequenceAction = new SequenceAction();
        sequenceAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { new TestAction("Test") } },
            { "ErrorHandling", SequenceErrorHandling.StopOnError }
        };

        // Act
        var isValid = sequenceAction.IsValid();

        // Assert
        isValid.Should().BeTrue();
        sequenceAction.GetValidationErrors().Should().BeEmpty();
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task ComplexActionNesting_ShouldWorkCorrectly()
    {
        // Arrange
        EnsureCleanTestState();

        var innerAction1 = new TestAction("Inner Action 1");
        var innerAction2 = new TestAction("Inner Action 2");

        // Create a sequence inside a conditional
        var innerSequence = new SequenceAction();
        innerSequence.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { innerAction1, innerAction2 } },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        var condition = new ValueCondition
        {
            MinValue = 50,
            MaxValue = 100,
            Action = innerSequence,
            Description = "Mid Range Sequence"
        };

        var outerConditional = new ConditionalAction();
        outerConditional.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", new List<ValueCondition> { condition } }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act
        await outerConditional.ExecuteAsync(75);

        // Assert
        innerAction1.ExecutionCount.Should().Be(1);
        innerAction2.ExecutionCount.Should().Be(1);
        innerAction1.LastMidiValue.Should().Be(75);
        innerAction2.LastMidiValue.Should().Be(75);
    }

    [Fact]
    public async Task ComplexActionCombination_ShouldHandleMultipleExecutions()
    {
        // Arrange
        EnsureCleanTestState();

        var primaryAction = new TestAction("Primary");
        var secondaryAction = new TestAction("Secondary");

        var alternatingAction = new AlternatingAction();
        alternatingAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { primaryAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", true }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Execute multiple times with different MIDI values
        await alternatingAction.ExecuteAsync(10);
        await alternatingAction.ExecuteAsync(20);
        await alternatingAction.ExecuteAsync(30);
        await alternatingAction.ExecuteAsync(40);
        await alternatingAction.ExecuteAsync(50);

        // Assert - Should alternate correctly with different MIDI values
        primaryAction.ExecutionCount.Should().Be(3); // 1st, 3rd, 5th execution
        secondaryAction.ExecutionCount.Should().Be(2); // 2nd, 4th execution

        primaryAction.ExecutionHistory.Should().Equal(10, 30, 50);
        secondaryAction.ExecutionHistory.Should().Equal(20, 40);
    }

    #endregion

    public override void Dispose()
    {
        // Clear state before disposing
        EnsureCleanTestState();
        base.Dispose();
    }
}
