using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Hardware;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Processing;
using MIDIFlux.Core.Tests.Infrastructure;
using MIDIFlux.Core.Tests.Mocks;
using MIDIFlux.Core.Tests.Utilities;
using Moq;
using Xunit;

namespace MIDIFlux.Core.Tests.Integration;

/// <summary>
/// Tests for IMidiHardwareAdapter abstraction layer.
/// These tests ensure the MIDI processing pipeline works correctly with different
/// device ID formats, preparing for Windows MIDI Services migration.
/// 
/// Key migration concerns tested:
/// - Device ID format agnosticism (numeric vs endpoint strings)
/// - Device connection/disconnection event handling
/// - MidiDeviceInfo model behavior with various ID formats
/// </summary>
public class HardwareAdapterAbstractionTests : TestBase
{
    private readonly ConfigurationService _configService;
    private readonly Mock<DeviceConfigurationManager> _mockDeviceConfigManager;
    private readonly ActionMappingRegistry _registry;
    private readonly MidiActionEngine _engine;

    /// <summary>
    /// Sample Windows MIDI Services-style endpoint device IDs for testing.
    /// These represent the format that will be used after migration.
    /// </summary>
    private static class WindowsMidiServicesDeviceIds
    {
        public const string InputDevice1 = @"\\?\SWD#MMDEVAPI#{0.0.1.00000000}.{a1b2c3d4-e5f6-7890-abcd-ef1234567890}#endpoint1";
        public const string InputDevice2 = @"\\?\SWD#MMDEVAPI#{0.0.1.00000000}.{b2c3d4e5-f6a7-8901-bcde-f12345678901}#endpoint2";
        public const string OutputDevice1 = @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{c3d4e5f6-a7b8-9012-cdef-123456789012}#output1";
        public const string OutputDevice2 = @"\\?\SWD#MMDEVAPI#{0.0.0.00000000}.{d4e5f6a7-b8c9-0123-defa-234567890123}#output2";
    }

    public HardwareAdapterAbstractionTests()
    {
        _configService = new ConfigurationService(CreateLogger<ConfigurationService>());
        _mockDeviceConfigManager = new Mock<DeviceConfigurationManager>(
            Mock.Of<ILogger<DeviceConfigurationManager>>(),
            _configService,
            Mock.Of<IServiceProvider>());

        _registry = new ActionMappingRegistry(CreateLogger<ActionMappingRegistry>());
        _engine = new MidiActionEngine(
            Logger,
            _registry,
            _configService,
            _mockDeviceConfigManager.Object);

        // Add Windows MIDI Services-style devices to the mock adapter
        SetupWindowsMidiServicesStyleDevices();
    }

    private void SetupWindowsMidiServicesStyleDevices()
    {
        // Add input devices with Windows MIDI Services-style IDs
        MockHardwareAdapter.AddInputDevice(new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice1,
            Name = "MIDI Controller (Windows MIDI Services)",
            IsConnected = true,
            SupportsInput = true,
            SupportsOutput = false,
            LastSeen = DateTime.Now
        });

        MockHardwareAdapter.AddInputDevice(new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice2,
            Name = "USB MIDI Keyboard (Windows MIDI Services)",
            IsConnected = true,
            SupportsInput = true,
            SupportsOutput = false,
            LastSeen = DateTime.Now
        });

        // Add output devices with Windows MIDI Services-style IDs
        MockHardwareAdapter.AddOutputDevice(new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.OutputDevice1,
            Name = "MIDI Synth (Windows MIDI Services)",
            IsConnected = true,
            SupportsInput = false,
            SupportsOutput = true,
            LastSeen = DateTime.Now
        });

        MockHardwareAdapter.AddOutputDevice(new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.OutputDevice2,
            Name = "MIDI Interface (Windows MIDI Services)",
            IsConnected = true,
            SupportsInput = false,
            SupportsOutput = true,
            LastSeen = DateTime.Now
        });
    }

    #region Device ID Format Agnostic Tests

    [Fact]
    public void GetInputDevices_ShouldReturnDevicesWithEndpointStyleIds()
    {
        // Act
        var devices = MockHardwareAdapter.GetInputDevices().ToList();

        // Assert - Should include both numeric and endpoint-style IDs
        devices.Should().Contain(d => d.DeviceId == WindowsMidiServicesDeviceIds.InputDevice1);
        devices.Should().Contain(d => d.DeviceId == WindowsMidiServicesDeviceIds.InputDevice2);
    }

    [Fact]
    public void GetOutputDevices_ShouldReturnDevicesWithEndpointStyleIds()
    {
        // Act
        var devices = MockHardwareAdapter.GetOutputDevices().ToList();

        // Assert
        devices.Should().Contain(d => d.DeviceId == WindowsMidiServicesDeviceIds.OutputDevice1);
        devices.Should().Contain(d => d.DeviceId == WindowsMidiServicesDeviceIds.OutputDevice2);
    }

    [Fact]
    public void StartInputDevice_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Act
        var result = MockHardwareAdapter.StartInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);

        // Assert
        result.Should().BeTrue();
        MockHardwareAdapter.ActiveInputDevices.Should().Contain(WindowsMidiServicesDeviceIds.InputDevice1);
    }

    [Fact]
    public void StartOutputDevice_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Act
        var result = MockHardwareAdapter.StartOutputDevice(WindowsMidiServicesDeviceIds.OutputDevice1);

        // Assert
        result.Should().BeTrue();
        MockHardwareAdapter.ActiveOutputDevices.Should().Contain(WindowsMidiServicesDeviceIds.OutputDevice1);
    }

    [Fact]
    public void StopInputDevice_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        MockHardwareAdapter.StartInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);

        // Act
        var result = MockHardwareAdapter.StopInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);

        // Assert
        result.Should().BeTrue();
        MockHardwareAdapter.ActiveInputDevices.Should().NotContain(WindowsMidiServicesDeviceIds.InputDevice1);
    }

    [Fact]
    public void StopOutputDevice_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        MockHardwareAdapter.StartOutputDevice(WindowsMidiServicesDeviceIds.OutputDevice1);

        // Act
        var result = MockHardwareAdapter.StopOutputDevice(WindowsMidiServicesDeviceIds.OutputDevice1);

        // Assert
        result.Should().BeTrue();
        MockHardwareAdapter.ActiveOutputDevices.Should().NotContain(WindowsMidiServicesDeviceIds.OutputDevice1);
    }

    [Fact]
    public void IsDeviceActive_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        MockHardwareAdapter.StartInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);

        // Act & Assert
        MockHardwareAdapter.IsDeviceActive(WindowsMidiServicesDeviceIds.InputDevice1).Should().BeTrue();
        MockHardwareAdapter.IsDeviceActive(WindowsMidiServicesDeviceIds.InputDevice2).Should().BeFalse();
    }

    [Fact]
    public void GetActiveDeviceIds_ShouldReturnEndpointStyleDeviceIds()
    {
        // Arrange
        MockHardwareAdapter.StartInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);
        MockHardwareAdapter.StartOutputDevice(WindowsMidiServicesDeviceIds.OutputDevice1);

        // Act
        var activeIds = MockHardwareAdapter.GetActiveDeviceIds();

        // Assert
        activeIds.Should().Contain(WindowsMidiServicesDeviceIds.InputDevice1);
        activeIds.Should().Contain(WindowsMidiServicesDeviceIds.OutputDevice1);
    }

    [Fact]
    public void SendMidiMessage_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        MockHardwareAdapter.StartOutputDevice(WindowsMidiServicesDeviceIds.OutputDevice1);
        var command = new MidiOutputCommand
        {
            MessageType = MidiMessageType.NoteOn,
            Channel = 1,
            Data1 = 60,
            Data2 = 127
        };

        // Act
        var result = MockHardwareAdapter.SendMidiMessage(WindowsMidiServicesDeviceIds.OutputDevice1, command);

        // Assert
        result.Should().BeTrue();
        MockHardwareAdapter.SentCommands.Should().ContainSingle();
        MockHardwareAdapter.SentCommands[0].Should().BeEquivalentTo(command);
    }

    [Fact]
    public void SimulateMidiEvent_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        MockHardwareAdapter.StartInputDevice(WindowsMidiServicesDeviceIds.InputDevice1);
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);
        MidiEventArgs? receivedArgs = null;
        MockHardwareAdapter.MidiEventReceived += (sender, args) => receivedArgs = args;

        // Act
        MockHardwareAdapter.SimulateMidiEvent(WindowsMidiServicesDeviceIds.InputDevice1, midiEvent);

        // Assert
        receivedArgs.Should().NotBeNull();
        receivedArgs!.DeviceId.Should().Be(WindowsMidiServicesDeviceIds.InputDevice1);
        receivedArgs.Event.Should().BeEquivalentTo(midiEvent);
    }

    #endregion

    #region MIDI Processing Pipeline with Endpoint-Style Device IDs

    [Fact]
    public async Task ProcessMidiEvent_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        var testAction = new TestAction("Endpoint Device Test");
        var mapping = new ActionMapping
        {
            Description = "Test Mapping for Endpoint Device",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "MIDI Controller (Windows MIDI Services)",
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process with endpoint-style device ID
        var result = await _engine.ProcessMidiEvent(
            WindowsMidiServicesDeviceIds.InputDevice1,
            midiEvent,
            "MIDI Controller (Windows MIDI Services)");

        // Assert
        result.Should().BeTrue();
        testAction.ExecutionCount.Should().Be(1);
        testAction.LastMidiValue.Should().Be(127);
    }

    [Fact]
    public async Task ProcessMidiEvent_WildcardDevice_ShouldWorkWithEndpointStyleDeviceId()
    {
        // Arrange
        var testAction = new TestAction("Wildcard Device Test");
        var mapping = new ActionMapping
        {
            Description = "Wildcard Device Mapping",
            IsEnabled = true,
            Input = new MidiInput
            {
                DeviceName = "*", // Wildcard - should match any device
                Channel = 1,
                InputType = MidiInputType.NoteOn,
                InputNumber = 60
            },
            Action = testAction
        };

        _registry.LoadMappings(new List<ActionMapping> { mapping });
        var midiEvent = MidiEventBuilder.NoteOn(1, 60, 127);

        // Act - Process with endpoint-style device ID, wildcard device name
        var result = await _engine.ProcessMidiEvent(
            WindowsMidiServicesDeviceIds.InputDevice1,
            midiEvent,
            "Any Device Name Should Match");

        // Assert
        result.Should().BeTrue();
        testAction.ExecutionCount.Should().Be(1);
    }

    #endregion

    #region Device Connection/Disconnection Event Tests

    [Fact]
    public void DeviceConnected_ShouldRaiseEventWithEndpointStyleDeviceId()
    {
        // Arrange
        MidiDeviceInfo? receivedDeviceInfo = null;
        MockHardwareAdapter.DeviceConnected += (sender, info) => receivedDeviceInfo = info;

        var newDevice = new MidiDeviceInfo
        {
            DeviceId = @"\\?\SWD#MMDEVAPI#{0.0.1.00000000}.{new-device-guid}#endpoint-new",
            Name = "Newly Connected Device",
            IsConnected = true,
            SupportsInput = true,
            SupportsOutput = false,
            LastSeen = DateTime.Now
        };

        // Act
        MockHardwareAdapter.SimulateDeviceConnected(newDevice);

        // Assert
        receivedDeviceInfo.Should().NotBeNull();
        receivedDeviceInfo!.DeviceId.Should().Be(newDevice.DeviceId);
        receivedDeviceInfo.Name.Should().Be(newDevice.Name);
        receivedDeviceInfo.IsConnected.Should().BeTrue();
    }

    [Fact]
    public void DeviceDisconnected_ShouldRaiseEventWithEndpointStyleDeviceId()
    {
        // Arrange
        MidiDeviceInfo? receivedDeviceInfo = null;
        MockHardwareAdapter.DeviceDisconnected += (sender, info) => receivedDeviceInfo = info;

        var disconnectedDevice = new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice1,
            Name = "MIDI Controller (Windows MIDI Services)",
            IsConnected = false,
            SupportsInput = true,
            SupportsOutput = false,
            LastSeen = DateTime.Now
        };

        // Act
        MockHardwareAdapter.SimulateDeviceDisconnected(disconnectedDevice);

        // Assert
        receivedDeviceInfo.Should().NotBeNull();
        receivedDeviceInfo!.DeviceId.Should().Be(WindowsMidiServicesDeviceIds.InputDevice1);
        receivedDeviceInfo.IsConnected.Should().BeFalse();
    }

    [Fact]
    public void MultipleConnectDisconnectCycles_ShouldWorkWithEndpointStyleDeviceIds()
    {
        // Arrange
        var connectEvents = new List<MidiDeviceInfo>();
        var disconnectEvents = new List<MidiDeviceInfo>();
        MockHardwareAdapter.DeviceConnected += (sender, info) => connectEvents.Add(info);
        MockHardwareAdapter.DeviceDisconnected += (sender, info) => disconnectEvents.Add(info);

        var device = new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice1,
            Name = "MIDI Controller",
            SupportsInput = true,
            SupportsOutput = false
        };

        // Act - Simulate multiple connect/disconnect cycles
        for (int i = 0; i < 3; i++)
        {
            device.IsConnected = true;
            MockHardwareAdapter.SimulateDeviceConnected(device);

            device.IsConnected = false;
            MockHardwareAdapter.SimulateDeviceDisconnected(device);
        }

        // Assert
        connectEvents.Should().HaveCount(3);
        disconnectEvents.Should().HaveCount(3);
        connectEvents.Should().OnlyContain(d => d.DeviceId == WindowsMidiServicesDeviceIds.InputDevice1);
        disconnectEvents.Should().OnlyContain(d => d.DeviceId == WindowsMidiServicesDeviceIds.InputDevice1);
    }

    [Fact]
    public void DeviceEvents_ShouldWorkWithBothNumericAndEndpointStyleIds()
    {
        // Arrange
        var receivedDeviceIds = new List<string>();
        MockHardwareAdapter.DeviceConnected += (sender, info) => receivedDeviceIds.Add(info.DeviceId);

        var numericDevice = new MidiDeviceInfo { DeviceId = "0", Name = "NAudio Style Device" };
        var endpointDevice = new MidiDeviceInfo { DeviceId = WindowsMidiServicesDeviceIds.InputDevice1, Name = "Windows MIDI Services Style Device" };

        // Act
        MockHardwareAdapter.SimulateDeviceConnected(numericDevice);
        MockHardwareAdapter.SimulateDeviceConnected(endpointDevice);

        // Assert - Both formats should work
        receivedDeviceIds.Should().HaveCount(2);
        receivedDeviceIds.Should().Contain("0");
        receivedDeviceIds.Should().Contain(WindowsMidiServicesDeviceIds.InputDevice1);
    }

    #endregion

    #region MidiDeviceInfo Model Tests with Various ID Formats

    [Fact]
    public void MidiDeviceInfo_ToString_ShouldWorkWithLongEndpointDeviceId()
    {
        // Arrange
        var device = new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice1,
            Name = "MIDI Controller",
            IsConnected = true,
            IsActive = true,
            SupportsInput = true,
            SupportsOutput = false
        };

        // Act
        var result = device.ToString();

        // Assert - Should not truncate or corrupt the device ID
        result.Should().Contain(WindowsMidiServicesDeviceIds.InputDevice1);
        result.Should().Contain("MIDI Controller");
        result.Should().Contain("Input");
        result.Should().Contain("Active");
    }

    [Fact]
    public void MidiDeviceInfo_ToDetailedString_ShouldWorkWithLongEndpointDeviceId()
    {
        // Arrange
        var device = new MidiDeviceInfo
        {
            DeviceId = WindowsMidiServicesDeviceIds.InputDevice1,
            Name = "USB MIDI Controller",
            Manufacturer = "Test Manufacturer",
            DriverVersion = "1.0.0",
            IsConnected = true,
            SupportsInput = true,
            SupportsOutput = false,
            LastSeen = DateTime.Now
        };

        // Act
        var result = device.ToDetailedString();

        // Assert
        result.Should().Contain(WindowsMidiServicesDeviceIds.InputDevice1);
        result.Should().Contain("USB MIDI Controller");
        result.Should().Contain("Test Manufacturer");
        result.Should().Contain("Connected");
    }

    [Fact]
    public void MidiDeviceInfo_ShouldHandleSpecialCharactersInEndpointId()
    {
        // Arrange - Device ID with backslashes, braces, hashes, etc.
        var specialCharId = @"\\?\SWD#MMDEVAPI#{0.0.1.00000000}.{a1b2c3d4-e5f6-7890-abcd-ef1234567890}#endpoint\subpath";
        var device = new MidiDeviceInfo
        {
            DeviceId = specialCharId,
            Name = "Test Device",
            IsConnected = true
        };

        // Act
        var toString = device.ToString();
        var detailedString = device.ToDetailedString();

        // Assert - Special characters should be preserved
        toString.Should().Contain(specialCharId);
        detailedString.Should().Contain(specialCharId);
        device.DeviceId.Should().Be(specialCharId);
    }

    #endregion
}
