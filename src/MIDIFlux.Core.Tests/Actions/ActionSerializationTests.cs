using FluentAssertions;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Complex;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions;

/// <summary>
/// Tests for JSON serialization and deserialization of complex actions.
/// Ensures that actions can be properly saved to and loaded from profile files.
/// </summary>
public class ActionSerializationTests : ActionTestBase
{
    public ActionSerializationTests()
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

    #region SequenceAction Serialization Tests

    [Fact]
    public void SequenceAction_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        EnsureCleanTestState();

        var testAction1 = new TestAction("Test Action 1");
        var testAction2 = new TestAction("Test Action 2");

        var originalAction = new SequenceAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { testAction1, testAction2 } },
            { "ErrorHandling", SequenceErrorHandling.StopOnError }
        };
        originalAction.Description = "Test Sequence";

        // Act - Serialize to JSON
        var json = JsonSerializer.Serialize(originalAction, new JsonSerializerOptions { WriteIndented = true });

        // Deserialize back to object
        var deserializedAction = JsonSerializer.Deserialize<SequenceAction>(json);

        // Assert
        deserializedAction.Should().NotBeNull();
        deserializedAction!.Description.Should().Be("Test Sequence");

        // Verify parameters were preserved
        var subActions = deserializedAction.JsonParameters["SubActions"] as List<ActionBase>;
        subActions.Should().NotBeNull();
        subActions!.Count.Should().Be(2);

        var errorHandling = deserializedAction.GetParameterValue<SequenceErrorHandling>("ErrorHandling");
        errorHandling.Should().Be(SequenceErrorHandling.StopOnError);
    }

    [Fact]
    public async Task SequenceAction_ShouldMaintainFunctionalityAfterSerialization()
    {
        // Arrange
        EnsureCleanTestState();

        var testAction1 = new TestAction("Action 1");
        var testAction2 = new TestAction("Action 2");

        var originalAction = new SequenceAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "SubActions", new List<ActionBase> { testAction1, testAction2 } },
            { "ErrorHandling", SequenceErrorHandling.ContinueOnError }
        };

        // Serialize and deserialize
        var json = JsonSerializer.Serialize(originalAction);
        var deserializedAction = JsonSerializer.Deserialize<SequenceAction>(json);

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Execute the deserialized action
        await deserializedAction!.ExecuteAsync(123);

        // Assert - Verify the deserialized action works correctly
        var subActions = deserializedAction.JsonParameters["SubActions"] as List<ActionBase>;
        var action1 = subActions![0] as TestAction;
        var action2 = subActions[1] as TestAction;

        action1!.ExecutionCount.Should().Be(1);
        action2!.ExecutionCount.Should().Be(1);
        action1.LastMidiValue.Should().Be(123);
        action2.LastMidiValue.Should().Be(123);
    }

    #endregion

    #region ConditionalAction Serialization Tests

    [Fact]
    public void ConditionalAction_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        EnsureCleanTestState();

        var lowAction = new TestAction("Low Action");
        var highAction = new TestAction("High Action");

        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 63, Action = lowAction, Description = "Low Range" },
            new() { MinValue = 64, MaxValue = 127, Action = highAction, Description = "High Range" }
        };

        var originalAction = new ConditionalAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };
        originalAction.Description = "Test Conditional";

        // Act - Serialize to JSON
        var json = JsonSerializer.Serialize(originalAction, new JsonSerializerOptions { WriteIndented = true });

        // Deserialize back to object
        var deserializedAction = JsonSerializer.Deserialize<ConditionalAction>(json);

        // Assert
        deserializedAction.Should().NotBeNull();
        deserializedAction!.Description.Should().Be("Test Conditional");

        // Verify conditions were preserved
        var deserializedConditions = deserializedAction.JsonParameters["Conditions"] as List<ValueCondition>;
        deserializedConditions.Should().NotBeNull();
        deserializedConditions!.Count.Should().Be(2);

        deserializedConditions[0].MinValue.Should().Be(0);
        deserializedConditions[0].MaxValue.Should().Be(63);
        deserializedConditions[0].Description.Should().Be("Low Range");

        deserializedConditions[1].MinValue.Should().Be(64);
        deserializedConditions[1].MaxValue.Should().Be(127);
        deserializedConditions[1].Description.Should().Be("High Range");
    }

    [Fact]
    public async Task ConditionalAction_ShouldMaintainFunctionalityAfterSerialization()
    {
        // Arrange
        EnsureCleanTestState();

        var lowAction = new TestAction("Low Action");
        var highAction = new TestAction("High Action");

        var conditions = new List<ValueCondition>
        {
            new() { MinValue = 0, MaxValue = 63, Action = lowAction },
            new() { MinValue = 64, MaxValue = 127, Action = highAction }
        };

        var originalAction = new ConditionalAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", conditions }
        };

        // Serialize and deserialize
        var json = JsonSerializer.Serialize(originalAction);
        var deserializedAction = JsonSerializer.Deserialize<ConditionalAction>(json);

        ActionBase.ServiceProvider = ServiceProvider;

        // Act - Execute the deserialized action with low value
        await deserializedAction!.ExecuteAsync(30);

        // Assert - Verify the correct condition was executed
        var deserializedConditions = deserializedAction.JsonParameters["Conditions"] as List<ValueCondition>;
        var lowActionDeserialized = deserializedConditions![0].Action as TestAction;
        var highActionDeserialized = deserializedConditions[1].Action as TestAction;

        lowActionDeserialized!.ExecutionCount.Should().Be(1);
        highActionDeserialized!.ExecutionCount.Should().Be(0);
        lowActionDeserialized.LastMidiValue.Should().Be(30);
    }

    #endregion

    #region AlternatingAction Serialization Tests

    [Fact]
    public void AlternatingAction_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        EnsureCleanTestState();

        var primaryAction = new TestAction("Primary Action");
        var secondaryAction = new TestAction("Secondary Action");

        var originalAction = new AlternatingAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { primaryAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", false }
        };
        originalAction.Description = "Test Alternating";

        // Act - Serialize to JSON
        var json = JsonSerializer.Serialize(originalAction, new JsonSerializerOptions { WriteIndented = true });

        // Deserialize back to object
        var deserializedAction = JsonSerializer.Deserialize<AlternatingAction>(json);

        // Assert
        deserializedAction.Should().NotBeNull();
        deserializedAction!.Description.Should().Be("Test Alternating");

        // Verify parameters were preserved
        var primaryActions = deserializedAction.JsonParameters["PrimaryAction"] as List<ActionBase>;
        var secondaryActions = deserializedAction.JsonParameters["SecondaryAction"] as List<ActionBase>;
        var startWithPrimary = deserializedAction.JsonParameters["StartWithPrimary"];

        primaryActions.Should().NotBeNull();
        primaryActions!.Count.Should().Be(1);
        secondaryActions.Should().NotBeNull();
        secondaryActions!.Count.Should().Be(1);
        startWithPrimary.Should().Be(false);
    }

    [Fact]
    public async Task AlternatingAction_ShouldMaintainStateAfterSerialization()
    {
        // Arrange
        EnsureCleanTestState();

        var primaryAction = new TestAction("Primary");
        var secondaryAction = new TestAction("Secondary");

        var originalAction = new AlternatingAction();
        originalAction.JsonParameters = new Dictionary<string, object?>
        {
            { "PrimaryAction", new List<ActionBase> { primaryAction } },
            { "SecondaryAction", new List<ActionBase> { secondaryAction } },
            { "StartWithPrimary", true }
        };

        ActionBase.ServiceProvider = ServiceProvider;

        // Execute once to change internal state
        await originalAction.ExecuteAsync(100);

        // Serialize and deserialize
        var json = JsonSerializer.Serialize(originalAction);
        var deserializedAction = JsonSerializer.Deserialize<AlternatingAction>(json);

        // Act - Execute the deserialized action
        await deserializedAction!.ExecuteAsync(200);

        // Assert - The deserialized action should start fresh (not maintain runtime state)
        var deserializedPrimaryActions = deserializedAction.JsonParameters["PrimaryAction"] as List<ActionBase>;
        var deserializedSecondaryActions = deserializedAction.JsonParameters["SecondaryAction"] as List<ActionBase>;

        var deserializedPrimary = deserializedPrimaryActions![0] as TestAction;
        var deserializedSecondary = deserializedSecondaryActions![0] as TestAction;

        // Since deserialization creates a fresh instance, it should start with primary again
        deserializedPrimary!.ExecutionCount.Should().Be(1);
        deserializedSecondary!.ExecutionCount.Should().Be(0);
        deserializedPrimary.LastMidiValue.Should().Be(200);
    }

    #endregion

    #region Complex Nested Serialization Tests

    [Fact]
    public void NestedComplexActions_ShouldSerializeAndDeserializeCorrectly()
    {
        // Arrange
        EnsureCleanTestState();

        var innerAction1 = new TestAction("Inner 1");
        var innerAction2 = new TestAction("Inner 2");

        // Create nested structure: ConditionalAction containing SequenceAction
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
            Description = "Sequence Condition"
        };

        var outerConditional = new ConditionalAction();
        outerConditional.JsonParameters = new Dictionary<string, object?>
        {
            { "Conditions", new List<ValueCondition> { condition } }
        };
        outerConditional.Description = "Nested Test";

        // Act - Serialize to JSON
        var json = JsonSerializer.Serialize(outerConditional, new JsonSerializerOptions { WriteIndented = true });

        // Deserialize back to object
        var deserializedAction = JsonSerializer.Deserialize<ConditionalAction>(json);

        // Assert
        deserializedAction.Should().NotBeNull();
        deserializedAction!.Description.Should().Be("Nested Test");

        // Verify nested structure was preserved
        var conditions = deserializedAction.JsonParameters["Conditions"] as List<ValueCondition>;
        conditions.Should().NotBeNull();
        conditions!.Count.Should().Be(1);

        var deserializedCondition = conditions[0];
        deserializedCondition.MinValue.Should().Be(50);
        deserializedCondition.MaxValue.Should().Be(100);
        deserializedCondition.Description.Should().Be("Sequence Condition");

        var nestedSequence = deserializedCondition.Action as SequenceAction;
        nestedSequence.Should().NotBeNull();

        var nestedSubActions = nestedSequence!.JsonParameters["SubActions"] as List<ActionBase>;
        nestedSubActions.Should().NotBeNull();
        nestedSubActions!.Count.Should().Be(2);
    }

    #endregion

    public override void Dispose()
    {
        // Clear state before disposing
        EnsureCleanTestState();
        base.Dispose();
    }
}
