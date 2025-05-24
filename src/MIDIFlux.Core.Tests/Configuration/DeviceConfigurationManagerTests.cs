using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Config;
using Xunit;

namespace MIDIFlux.Core.Tests.Configuration;

/// <summary>
/// Tests for DeviceConfigurationManager to validate unified action system integration
/// </summary>
public class DeviceConfigurationManagerTests : TestBase
{
    private readonly ILogger _logger;
    private readonly IUnifiedActionFactory _actionFactory;
    private readonly DeviceConfigurationManager _manager;

    public DeviceConfigurationManagerTests()
    {
        // Create a test logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<DeviceConfigurationManager>();
        _actionFactory = new UnifiedActionFactory(loggerFactory.CreateLogger<UnifiedActionFactory>());
        _manager = new DeviceConfigurationManager(_logger, _actionFactory);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeviceConfigurationManager(null!, _actionFactory));
    }

    [Fact]
    public void Constructor_WithNullActionFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new DeviceConfigurationManager(_logger, null!));
    }

    [Fact]
    public void SetConfiguration_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _manager.SetConfiguration(null!));
    }

    [Fact]
    public void SetConfiguration_WithValidConfiguration_LoadsSuccessfully()
    {
        // Arrange
        var config = new UnifiedMappingConfig
        {
            ProfileName = "Test Profile",
            Description = "Test configuration",
            MidiDevices = new List<UnifiedDeviceConfig>
            {
                new UnifiedDeviceConfig
                {
                    DeviceName = "Test Controller",
                    InputProfile = "Test-Profile",
                    Mappings = new List<UnifiedMappingConfigEntry>
                    {
                        new UnifiedMappingConfigEntry
                        {
                            Id = "test-mapping",
                            Description = "Test key mapping",
                            InputType = "NoteOn",
                            Note = 60,
                            Channel = 1,
                            IsEnabled = true,
                            Action = new KeyPressReleaseConfig
                            {
                                VirtualKeyCode = 65,
                                Description = "Press A key"
                            }
                        }
                    }
                }
            }
        };

        // Act
        _manager.SetConfiguration(config);

        // Assert
        var loadedConfig = _manager.GetConfiguration();
        Assert.NotNull(loadedConfig);
        Assert.Equal("Test Profile", loadedConfig.ProfileName);
        Assert.Equal("Test configuration", loadedConfig.Description);
        Assert.Single(loadedConfig.MidiDevices);

        var stats = _manager.GetRegistryStatistics();
        Assert.Equal(1, stats.EnabledMappings);
        Assert.Equal(1, stats.TotalMappings);
    }

    [Fact]
    public void FindActions_WithValidInput_ReturnsMatchingActions()
    {
        // Arrange
        var config = new UnifiedMappingConfig
        {
            ProfileName = "Test Profile",
            MidiDevices = new List<UnifiedDeviceConfig>
            {
                new UnifiedDeviceConfig
                {
                    DeviceName = "Test Controller",
                    Mappings = new List<UnifiedMappingConfigEntry>
                    {
                        new UnifiedMappingConfigEntry
                        {
                            InputType = "NoteOn",
                            Note = 60,
                            Channel = 1,
                            Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                        }
                    }
                }
            }
        };

        _manager.SetConfiguration(config);

        var input = new UnifiedActionMidiInput
        {
            DeviceName = "Test Controller",
            InputType = UnifiedActionMidiInputType.NoteOn,
            InputNumber = 60,
            Channel = 1
        };

        // Act
        var actions = _manager.FindActions(input);

        // Assert
        Assert.Single(actions);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction>(actions[0]);
    }

    [Fact]
    public void FindDeviceConfigsForId_WithNoConfiguration_ReturnsEmptyList()
    {
        // Act
        var configs = _manager.FindDeviceConfigsForId(1);

        // Assert
        Assert.Empty(configs);
    }

    [Fact]
    public void GetActionRegistry_ReturnsValidRegistry()
    {
        // Act
        var registry = _manager.GetActionRegistry();

        // Assert
        Assert.NotNull(registry);
    }

    [Fact]
    public void GetRegistryStatistics_ReturnsValidStatistics()
    {
        // Act
        var stats = _manager.GetRegistryStatistics();

        // Assert
        Assert.NotNull(stats);
        Assert.Equal(0, stats.TotalMappings); // No configuration loaded yet
    }
}
