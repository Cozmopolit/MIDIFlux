using FluentAssertions;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Infrastructure;
using Moq;
using Xunit;

namespace MIDIFlux.Core.Tests.State;

/// <summary>
/// Tests for ActionStateManager - thread-safe state management for stateful actions
/// </summary>
public class ActionStateManagerTests : TestBase
{
    private readonly ActionStateManager _stateManager;
    private readonly Mock<KeyboardSimulator> _mockKeyboardSimulator;

    public ActionStateManagerTests()
    {
        _mockKeyboardSimulator = new Mock<KeyboardSimulator>();
        _stateManager = new ActionStateManager(_mockKeyboardSimulator.Object, CreateLogger<ActionStateManager>());
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act & Assert
        _stateManager.Should().NotBeNull();
        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(0);
        stats.UserDefinedStates.Should().Be(0);
        stats.InternalStates.Should().Be(0);
    }

    [Fact]
    public void SetState_ShouldCreateNewState()
    {
        // Arrange
        const string key = "TestState";
        const int value = 42;

        // Act
        _stateManager.SetState(key, value);

        // Assert
        var retrievedValue = _stateManager.GetState(key);
        retrievedValue.Should().Be(value);

        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(1);
        stats.UserDefinedStates.Should().Be(1);
        stats.InternalStates.Should().Be(0);
    }

    [Fact]
    public void SetState_ShouldUpdateExistingState()
    {
        // Arrange
        const string key = "TestState";
        const int initialValue = 42;
        const int updatedValue = 84;

        // Act
        _stateManager.SetState(key, initialValue);
        _stateManager.SetState(key, updatedValue);

        // Assert
        var retrievedValue = _stateManager.GetState(key);
        retrievedValue.Should().Be(updatedValue);

        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(1); // Should still be 1, not 2
    }

    [Fact]
    public void GetState_ShouldReturnMinusOneForNonexistentState()
    {
        // Arrange
        const string nonexistentKey = "NonexistentState";

        // Act
        var value = _stateManager.GetState(nonexistentKey);

        // Assert
        value.Should().Be(-1); // ActionStateManager returns -1 for non-existent states
    }

    [Fact]
    public void ClearState_ShouldRemoveExistingState()
    {
        // Arrange
        const string key = "TestState";
        const int value = 42;

        _stateManager.SetState(key, value);
        _stateManager.GetState(key).Should().Be(value); // Verify it exists

        // Act
        _stateManager.ClearState(key);

        // Assert
        _stateManager.GetState(key).Should().Be(-1); // Should return -1 for non-existent state
        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(0);
    }

    [Fact]
    public void ClearState_ShouldHandleNonexistentState()
    {
        // Arrange
        const string nonexistentKey = "NonexistentState";

        // Act & Assert - Should not throw
        var action = () => _stateManager.ClearState(nonexistentKey);
        action.Should().NotThrow();
    }

    [Fact]
    public void ClearAllStates_ShouldRemoveAllStates()
    {
        // Arrange
        _stateManager.SetState("State1", 10);
        _stateManager.SetState("State2", 20);
        _stateManager.SetState("*Key65", 1); // Internal state

        var statsBeforeClear = _stateManager.GetStatistics();
        statsBeforeClear.ActiveStates.Should().Be(3);

        // Act
        _stateManager.ClearAllStates();

        // Assert
        var statsAfterClear = _stateManager.GetStatistics();
        statsAfterClear.ActiveStates.Should().Be(0);
        statsAfterClear.UserDefinedStates.Should().Be(0);
        statsAfterClear.InternalStates.Should().Be(0);

        // Note: KeyboardSimulator.SendKeyUp is not virtual, so we can't verify it was called
        // The test verifies that ClearAllStates works correctly by checking the statistics
    }

    [Fact]
    public void InternalStates_ShouldBeTrackedSeparately()
    {
        // Arrange
        const string userKey = "UserState";
        const string internalKey = "*Key65"; // Internal keys must follow *Key{digits} format

        // Act
        _stateManager.SetState(userKey, 10);
        _stateManager.SetState(internalKey, 20);

        // Assert
        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(2);
        stats.UserDefinedStates.Should().Be(1);
        stats.InternalStates.Should().Be(1);
    }

    [Fact]
    public void InitializeStates_ShouldSetMultipleStates()
    {
        // Arrange
        var initialStates = new Dictionary<string, int>
        {
            { "State1", 10 },
            { "State2", 20 },
            { "State3", 30 }
        };

        // Act
        _stateManager.InitializeStates(initialStates);

        // Assert
        _stateManager.GetState("State1").Should().Be(10);
        _stateManager.GetState("State2").Should().Be(20);
        _stateManager.GetState("State3").Should().Be(30);

        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(3);
        stats.UserDefinedStates.Should().Be(3);
    }

    [Fact]
    public void InitializeStates_ShouldHandleEmptyDictionary()
    {
        // Arrange
        var emptyStates = new Dictionary<string, int>();

        // Act
        _stateManager.InitializeStates(emptyStates);

        // Assert
        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(0);
    }

    [Fact]
    public void InitializeStates_ShouldThrowOnNullDictionary()
    {
        // Arrange & Act & Assert
        var action = () => _stateManager.InitializeStates(null!);
        action.Should().Throw<NullReferenceException>(); // ActionStateManager doesn't handle null gracefully
    }

    [Fact]
    public async Task ThreadSafety_ShouldHandleConcurrentOperations()
    {
        // Arrange
        const string key = "ConcurrentState";
        const int iterations = 100;
        const int threadCount = 10;

        // Act - Multiple threads setting the same state
        var tasks = new Task[threadCount];
        for (int i = 0; i < threadCount; i++)
        {
            int threadId = i;
            tasks[i] = Task.Run(() =>
            {
                for (int j = 0; j < iterations; j++)
                {
                    // Each thread sets the state to a unique value
                    _stateManager.SetState(key, threadId * iterations + j);
                }
            });
        }

        await Task.WhenAll(tasks);

        // Assert - The final value should be one of the values set by the threads
        var finalValue = _stateManager.GetState(key);
        finalValue.Should().BeGreaterOrEqualTo(0); // Should have some valid value
        finalValue.Should().BeLessThan(threadCount * iterations); // Should be within expected range

        var stats = _stateManager.GetStatistics();
        stats.ActiveStates.Should().Be(1); // Should still be just one state
    }

    [Fact]
    public void StateKeys_ShouldBeValidated()
    {
        // Arrange & Act & Assert
        // Valid alphanumeric keys should work
        var action1 = () => _stateManager.SetState("ValidKey123", 10);
        action1.Should().NotThrow();

        // Valid internal keys should work
        var action2 = () => _stateManager.SetState("*Key65", 20);
        action2.Should().NotThrow();

        // Invalid user-defined keys with special characters should throw
        var action3 = () => _stateManager.SetState("Key-With-Dashes", 30);
        action3.Should().Throw<ArgumentException>(); // User keys must be alphanumeric only

        // Invalid internal keys should throw
        var action4 = () => _stateManager.SetState("*InvalidKey", 40);
        action4.Should().Throw<ArgumentException>(); // Internal keys must follow *Key{digits} format

        // Empty/null keys should throw
        var action5 = () => _stateManager.SetState("", 50);
        action5.Should().Throw<ArgumentException>();

        var action6 = () => _stateManager.SetState(null!, 60);
        action6.Should().Throw<ArgumentException>();
    }

    public override void Dispose()
    {
        // No specific cleanup needed for ActionStateManager
        base.Dispose();
    }
}
