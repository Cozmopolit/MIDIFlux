using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.State;
using MIDIFlux.Core.Tests.Mocks;
using Xunit;

namespace MIDIFlux.Core.Tests;

/// <summary>
/// Basic tests to verify the test infrastructure works correctly
/// </summary>
public class BasicTests : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BasicTests> _logger;
    private readonly MockMidiHardwareAdapter _mockAdapter;
    private readonly ActionStateManager _stateManager;

    public BasicTests()
    {
        var services = new ServiceCollection();

        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Debug));

        // Add mock hardware adapter
        services.AddSingleton<IMidiHardwareAdapter, MockMidiHardwareAdapter>();

        // Add core services
        services.AddSingleton<MIDIFlux.Core.Keyboard.KeyboardSimulator>();
        services.AddSingleton<ActionStateManager>();

        _serviceProvider = services.BuildServiceProvider();
        _logger = _serviceProvider.GetRequiredService<ILogger<BasicTests>>();
        _mockAdapter = (MockMidiHardwareAdapter)_serviceProvider.GetRequiredService<IMidiHardwareAdapter>();
        _stateManager = _serviceProvider.GetRequiredService<ActionStateManager>();

        // Set global service provider for actions
        ActionBase.ServiceProvider = _serviceProvider;
    }

    [Fact]
    public void ServiceProvider_ShouldBeConfiguredCorrectly()
    {
        // Arrange & Act & Assert
        _serviceProvider.Should().NotBeNull();
        _logger.Should().NotBeNull();
        _mockAdapter.Should().NotBeNull();
        _stateManager.Should().NotBeNull();
    }

    [Fact]
    public void MockMidiHardwareAdapter_ShouldImplementInterface()
    {
        // Arrange & Act & Assert
        _mockAdapter.Should().BeAssignableTo<IMidiHardwareAdapter>();
        _mockAdapter.GetInputDevices().Should().NotBeEmpty();
        _mockAdapter.GetOutputDevices().Should().NotBeEmpty();
    }

    [Fact]
    public void MockMidiHardwareAdapter_ShouldStartAndStopDevices()
    {
        // Arrange
        const int deviceId = 0;

        // Act
        var startResult = _mockAdapter.StartInputDevice(deviceId);
        var isActive = _mockAdapter.IsDeviceActive(deviceId);
        var stopResult = _mockAdapter.StopInputDevice(deviceId);
        var isActiveAfterStop = _mockAdapter.IsDeviceActive(deviceId);

        // Assert
        startResult.Should().BeTrue();
        isActive.Should().BeTrue();
        stopResult.Should().BeTrue();
        isActiveAfterStop.Should().BeFalse();
    }

    [Fact]
    public void MockMidiHardwareAdapter_ShouldSimulateMidiEvents()
    {
        // Arrange
        const int deviceId = 0;
        var eventReceived = false;
        MidiEventArgs? receivedEventArgs = null;

        _mockAdapter.MidiEventReceived += (sender, args) =>
        {
            eventReceived = true;
            receivedEventArgs = args;
        };

        _mockAdapter.StartInputDevice(deviceId);

        var testEvent = new MidiEvent
        {
            EventType = MidiEventType.NoteOn,
            Channel = 1,
            Note = 60,
            Velocity = 127
        };

        // Act
        _mockAdapter.SimulateMidiEvent(deviceId, testEvent);

        // Assert
        eventReceived.Should().BeTrue();
        receivedEventArgs.Should().NotBeNull();
        receivedEventArgs!.DeviceId.Should().Be(deviceId);
        receivedEventArgs.Event.Should().BeEquivalentTo(testEvent);
    }

    [Fact]
    public void ActionStateManager_ShouldManageStates()
    {
        // Arrange
        const string stateKey = "TestState";
        const int stateValue = 42;

        // Act
        var initialValue = _stateManager.GetState(stateKey);
        _stateManager.SetState(stateKey, stateValue);
        var setValue = _stateManager.GetState(stateKey);

        // Assert
        initialValue.Should().Be(-1); // Non-existent state
        setValue.Should().Be(stateValue);
    }

    [Fact]
    public void ActionStateManager_ShouldClearStates()
    {
        // Arrange
        const string stateKey = "TestState";
        const int stateValue = 42;

        _stateManager.SetState(stateKey, stateValue);
        var valueBeforeClear = _stateManager.GetState(stateKey);

        // Act
        _stateManager.ClearAllStates();
        var valueAfterClear = _stateManager.GetState(stateKey);

        // Assert
        valueBeforeClear.Should().Be(stateValue);
        valueAfterClear.Should().Be(-1);
    }

    [Fact]
    public void KeyPressReleaseAction_ShouldHaveCorrectParameters()
    {
        // Arrange & Act
        var action = new KeyPressReleaseAction();
        var parameters = action.GetParameterList();

        // Assert
        action.Should().NotBeNull();
        parameters.Should().NotBeEmpty();
        parameters.Should().Contain(p => p.Name == "VirtualKeyCode");
    }

    [Fact]
    public void KeyPressReleaseAction_ShouldBeInvalidWithoutParameters()
    {
        // Arrange
        var action = new KeyPressReleaseAction();

        // Act & Assert
        // The action should throw an exception when trying to validate without required parameters
        var validationAction = () => action.IsValid();
        validationAction.Should().Throw<InvalidOperationException>()
            .WithMessage("*VirtualKeyCode*");
    }

    [Fact]
    public void KeyPressReleaseAction_ShouldBeValidWithCorrectParameters()
    {
        // Arrange
        var action = new KeyPressReleaseAction();
        action.JsonParameters = new Dictionary<string, object?>
        {
            { "VirtualKeyCode", Keys.A }
        };

        // Act
        var isValid = action.IsValid();
        var errors = action.GetValidationErrors();

        // Assert
        isValid.Should().BeTrue();
        errors.Should().BeEmpty();
    }

    [Fact]
    public void ActionTypeRegistry_ShouldDiscoverActions()
    {
        // Arrange & Act
        var registry = ActionTypeRegistry.Instance;
        var actionTypes = registry.GetAllActionTypes();

        // Assert
        registry.Should().NotBeNull();
        actionTypes.Should().NotBeEmpty();
        actionTypes.Should().ContainKey("KeyPressReleaseAction");
    }

    [Fact]
    public void ActionTypeRegistry_ShouldCreateActionInstances()
    {
        // Arrange
        var registry = ActionTypeRegistry.Instance;

        // Act
        var action = registry.CreateActionInstance("KeyPressReleaseAction");

        // Assert
        action.Should().NotBeNull();
        action.Should().BeOfType<KeyPressReleaseAction>();
        action!.Description.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Parameter_ShouldStoreAndRetrieveValues()
    {
        // Arrange
        var parameter = new Parameter(ParameterType.String, "test value", "Test Parameter");

        // Act
        var value = parameter.GetValue<string>();

        // Assert
        parameter.Type.Should().Be(ParameterType.String);
        parameter.DisplayName.Should().Be("Test Parameter");
        value.Should().Be("test value");
    }

    [Fact]
    public void ParameterInfo_ShouldProvideReadOnlyAccess()
    {
        // Arrange
        var parameter = new Parameter(ParameterType.Integer, 42, "Test Integer");
        var parameterInfo = new ParameterInfo("TestParam", parameter);

        // Act & Assert
        parameterInfo.Name.Should().Be("TestParam");
        parameterInfo.Type.Should().Be(ParameterType.Integer);
        parameterInfo.DisplayName.Should().Be("Test Integer");
        parameterInfo.Value.Should().Be(42);
    }

    [Fact]
    public void MidiInputType_ShouldHaveCorrectCategories()
    {
        // Arrange & Act & Assert
        MidiInputType.NoteOn.GetCategory().Should().Be(InputTypeCategory.Trigger);
        MidiInputType.ControlChangeAbsolute.GetCategory().Should().Be(InputTypeCategory.AbsoluteValue);
        MidiInputType.ControlChangeRelative.GetCategory().Should().Be(InputTypeCategory.RelativeValue);
    }

    public void Dispose()
    {
        ActionBase.ServiceProvider = null;
        if (_serviceProvider is IDisposable disposableProvider)
        {
            disposableProvider.Dispose();
        }
        GC.SuppressFinalize(this);
    }
}
