using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using MIDIFlux.Core.Tests.Infrastructure;
using MIDIFlux.Core.Tests.Mocks;
using MIDIFlux.Core.Tests.Utilities;
using Moq;
using Xunit;

namespace MIDIFlux.Core.Tests.Processing;

/// <summary>
/// Tests for MidiActionEngine - the core MIDI processing pipeline
/// </summary>
public class MidiActionEngineTests : TestBase
{
    private readonly ConfigurationService _configService;
    private readonly Mock<DeviceConfigurationManager> _mockDeviceConfigManager;
    private readonly ActionMappingRegistry _registry;
    private readonly MidiActionEngine _engine;

    public MidiActionEngineTests()
    {
        // Create real configuration service (can't be mocked as methods aren't virtual)
        _configService = new ConfigurationService(CreateLogger<ConfigurationService>());

        // Create mock device configuration manager
        _mockDeviceConfigManager = new Mock<DeviceConfigurationManager>(
            Mock.Of<ILogger<DeviceConfigurationManager>>(),
            _configService,
            Mock.Of<IServiceProvider>());

        // Create real registry for testing
        _registry = new ActionMappingRegistry(CreateLogger<ActionMappingRegistry>());

        // Create the engine under test
        _engine = new MidiActionEngine(
            Logger,
            _registry,
            _configService,
            _mockDeviceConfigManager.Object);
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act & Assert
        _engine.Should().NotBeNull();
        _engine.LatencyAnalyzer.Should().NotBeNull();
        // Note: LatencyAnalyzer settings depend on appsettings.json file existence
        // In test environment, it will use defaults (true, 1000) if file doesn't exist
        _engine.LatencyAnalyzer.IsEnabled.Should().BeTrue();
        _engine.LatencyAnalyzer.MaxMeasurements.Should().Be(1000);
    }

    [Fact]
    public void Constructor_ShouldThrowOnNullLogger()
    {
        // Arrange & Act & Assert
        var action = () => new MidiActionEngine(
            null!,
            _registry,
            _configService,
            _mockDeviceConfigManager.Object);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("logger");
    }

    [Fact]
    public void Constructor_ShouldThrowOnNullRegistry()
    {
        // Arrange & Act & Assert
        var action = () => new MidiActionEngine(
            Logger,
            null!,
            _configService,
            _mockDeviceConfigManager.Object);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("registry");
    }

    [Fact]
    public void Constructor_ShouldThrowOnNullDeviceConfigManager()
    {
        // Arrange & Act & Assert
        var action = () => new MidiActionEngine(
            Logger,
            _registry,
            _configService,
            null!);

        action.Should().Throw<ArgumentNullException>()
            .WithParameterName("deviceConfigManager");
    }

    [Fact]
    public async Task ProcessMidiEvent_ShouldReturnFalseForNullEvent()
    {
        // Arrange
        const string deviceId = "0";
        const string deviceName = "Test Device";

        // Act
        var result = await _engine.ProcessMidiEvent(deviceId, null!, deviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ProcessMidiEvent_ShouldReturnFalseWhenNoActionsFound()
    {
        // Arrange
        const string deviceId = "0";
        const string deviceName = "Test Device";
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act (registry is empty, so no actions should be found)
        var result = await _engine.ProcessMidiEvent(deviceId, midiEvent, deviceName);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HandleMidiEvent_ShouldNotThrowWithValidEvent()
    {
        // Arrange
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);
        var eventArgs = new MidiEventArgs("0", midiEvent);

        // Act & Assert
        var action = () => _engine.HandleMidiEvent(eventArgs);
        action.Should().NotThrow();
    }

    public override void Dispose()
    {
        // No specific cleanup needed for MidiActionEngine
        base.Dispose();
    }
}
