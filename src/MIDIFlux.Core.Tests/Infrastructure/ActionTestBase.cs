using FluentAssertions;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;

namespace MIDIFlux.Core.Tests.Infrastructure;

/// <summary>
/// Base class for action-specific tests providing common action testing utilities
/// </summary>
public abstract class ActionTestBase : TestBase
{
    static ActionTestBase()
    {
        // Register test action types for JSON serialization
        // These are not discovered automatically since they're in the test assembly
        ActionTypeRegistry.Instance.RegisterActionType("TestAction", typeof(TestAction));
        ActionTypeRegistry.Instance.RegisterActionType("FailingTestAction", typeof(FailingTestAction));
        ActionTypeRegistry.Instance.RegisterActionType("InvalidTestAction", typeof(InvalidTestAction));
    }
    /// <summary>
    /// Creates an action instance of the specified type
    /// </summary>
    protected T CreateAction<T>() where T : ActionBase, new()
    {
        return new T();
    }

    /// <summary>
    /// Sets a parameter value on an action
    /// </summary>
    protected void SetParameter<T>(ActionBase action, string parameterName, T value)
    {
        var parameters = action.JsonParameters;
        parameters[parameterName] = value;
        action.JsonParameters = parameters;
    }

    /// <summary>
    /// Gets a parameter value from an action
    /// </summary>
    protected T? GetParameter<T>(ActionBase action, string parameterName)
    {
        if (action.JsonParameters.TryGetValue(parameterName, out var value))
        {
            if (value is T typedValue)
                return typedValue;

            // Try to convert if types don't match exactly
            try
            {
                return (T)Convert.ChangeType(value, typeof(T))!;
            }
            catch
            {
                return default;
            }
        }
        return default;
    }

    /// <summary>
    /// Validates that an action is properly configured
    /// </summary>
    protected void AssertActionIsValid(ActionBase action)
    {
        action.IsValid().Should().BeTrue($"Action {action.GetType().Name} should be valid");
        action.GetValidationErrors().Should().BeEmpty($"Action {action.GetType().Name} should have no validation errors");
    }

    /// <summary>
    /// Validates that an action is invalid with expected errors
    /// </summary>
    protected void AssertActionIsInvalid(ActionBase action, params string[] expectedErrorSubstrings)
    {
        action.IsValid().Should().BeFalse($"Action {action.GetType().Name} should be invalid");

        var errors = action.GetValidationErrors();
        errors.Should().NotBeEmpty($"Action {action.GetType().Name} should have validation errors");

        if (expectedErrorSubstrings.Length > 0)
        {
            foreach (var expectedSubstring in expectedErrorSubstrings)
            {
                errors.Should().Contain(error => error.Contains(expectedSubstring, StringComparison.OrdinalIgnoreCase),
                    $"Validation errors should contain '{expectedSubstring}'");
            }
        }
    }

    /// <summary>
    /// Executes an action and waits for completion
    /// </summary>
    protected async Task ExecuteActionAsync(ActionBase action, int? midiValue = null)
    {
        await action.ExecuteAsync(midiValue);
    }

    /// <summary>
    /// Executes an action synchronously (for testing simple actions)
    /// </summary>
    protected void ExecuteAction(ActionBase action, int? midiValue = null)
    {
        var task = action.ExecuteAsync(midiValue);
        if (!task.IsCompleted)
        {
            task.AsTask().Wait(TimeSpan.FromSeconds(5));
        }
    }

    /// <summary>
    /// Validates that an action has the expected parameter list
    /// </summary>
    protected void AssertParameterList(ActionBase action, params (string name, ParameterType type)[] expectedParameters)
    {
        var parameterList = action.GetParameterList();
        parameterList.Should().HaveCount(expectedParameters.Length,
            $"Action {action.GetType().Name} should have {expectedParameters.Length} parameters");

        for (int i = 0; i < expectedParameters.Length; i++)
        {
            var expected = expectedParameters[i];
            var actual = parameterList.FirstOrDefault(p => p.Name == expected.name);

            actual.Should().NotBeNull($"Parameter '{expected.name}' should exist");
            actual!.Type.Should().Be(expected.type, $"Parameter '{expected.name}' should have type {expected.type}");
        }
    }

    /// <summary>
    /// Validates that an action supports the expected input categories
    /// </summary>
    protected void AssertCompatibleInputCategories(ActionBase action, params InputTypeCategory[] expectedCategories)
    {
        var actualCategories = action.GetCompatibleInputCategories();
        actualCategories.Should().BeEquivalentTo(expectedCategories,
            $"Action {action.GetType().Name} should support the expected input categories");
    }

    /// <summary>
    /// Creates a test action that tracks execution without side effects
    /// </summary>
    protected TestAction CreateTestAction(string description = "Test Action")
    {
        return new TestAction(description);
    }
}

/// <summary>
/// Test action implementation that tracks execution without side effects
/// </summary>
public class TestAction : ActionBase
{
    private readonly List<int?> _executionHistory = new();
    private readonly string _testDescription;

    public IReadOnlyList<int?> ExecutionHistory => _executionHistory.AsReadOnly();
    public int ExecutionCount => _executionHistory.Count;
    public int? LastMidiValue => _executionHistory.LastOrDefault();

    public TestAction(string description = "Test Action")
    {
        _testDescription = description;
        Description = description;
    }

    protected override void InitializeParameters()
    {
        // Test action has no parameters by default
    }

    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue, InputTypeCategory.RelativeValue };
    }

    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        _executionHistory.Add(midiValue);
        Logger.LogDebug("TestAction executed with MIDI value: {MidiValue}", midiValue);
        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Clears the execution history
    /// </summary>
    public void ClearHistory()
    {
        _executionHistory.Clear();
    }

    /// <summary>
    /// Gets the default description for this action
    /// </summary>
    protected override string GetDefaultDescription()
    {
        return _testDescription;
    }
}

/// <summary>
/// Test action that always fails during execution for testing error handling
/// </summary>
public class FailingTestAction : ActionBase
{
    private readonly List<int?> _executionHistory = new();
    private readonly string _testDescription;

    public IReadOnlyList<int?> ExecutionHistory => _executionHistory.AsReadOnly();
    public int ExecutionCount => _executionHistory.Count;
    public int? LastMidiValue => _executionHistory.LastOrDefault();

    public FailingTestAction(string description = "Failing Test Action")
    {
        _testDescription = description;
        Description = description;
    }

    protected override void InitializeParameters()
    {
        // Failing test action has no parameters
    }

    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger, InputTypeCategory.AbsoluteValue, InputTypeCategory.RelativeValue };
    }

    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        _executionHistory.Add(midiValue);
        Logger.LogDebug("FailingTestAction executed with MIDI value: {MidiValue} - throwing exception", midiValue);
        throw new InvalidOperationException($"FailingTestAction intentionally failed with MIDI value: {midiValue}");
    }

    protected override string GetDefaultDescription()
    {
        return _testDescription;
    }
}

/// <summary>
/// Test action that is always invalid for testing validation logic
/// </summary>
public class InvalidTestAction : ActionBase
{
    private readonly string _testDescription;

    public InvalidTestAction(string description = "Invalid Test Action")
    {
        _testDescription = description;
        Description = description;
    }

    protected override void InitializeParameters()
    {
        // Invalid test action has no parameters
    }

    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }

    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors
        AddValidationError("InvalidTestAction is always invalid by design");
        return false;
    }

    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        Logger.LogDebug("InvalidTestAction executed with MIDI value: {MidiValue}", midiValue);
        return ValueTask.CompletedTask;
    }

    protected override string GetDefaultDescription()
    {
        return _testDescription;
    }
}
