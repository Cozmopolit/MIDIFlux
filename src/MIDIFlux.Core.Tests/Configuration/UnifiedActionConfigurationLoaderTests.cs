using System.Text.Json;
using Microsoft.Extensions.Logging;
using Xunit;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Tests.Configuration;

/// <summary>
/// Tests for UnifiedActionConfigurationLoader to validate JSON loading, type safety, and error handling
/// </summary>
public class UnifiedActionConfigurationLoaderTests : TestBase
{
    private readonly ILogger _logger;
    private readonly IUnifiedActionFactory _actionFactory;
    private readonly UnifiedActionConfigurationLoader _loader;

    public UnifiedActionConfigurationLoaderTests()
    {
        // Create a test logger
        var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        _logger = loggerFactory.CreateLogger<UnifiedActionConfigurationLoader>();
        _actionFactory = new UnifiedActionFactory(loggerFactory.CreateLogger<UnifiedActionFactory>());
        _loader = new UnifiedActionConfigurationLoader(_logger, _actionFactory);
    }

    [Fact]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnifiedActionConfigurationLoader(null!, _actionFactory));
    }

    [Fact]
    public void Constructor_WithNullActionFactory_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UnifiedActionConfigurationLoader(_logger, null!));
    }

    [Fact]
    public void LoadConfiguration_WithNonExistentFile_ReturnsNull()
    {
        // Arrange
        var nonExistentPath = "non_existent_file.json";

        // Act
        var result = _loader.LoadConfiguration(nonExistentPath);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void LoadConfiguration_WithValidJson_ReturnsConfiguration()
    {
        // Arrange
        var validJson = """
        {
          "ProfileName": "Test Profile",
          "Description": "Test description",
          "MidiDevices": [
            {
              "InputProfile": "Test-Device",
              "DeviceName": "Test Controller",
              "Mappings": [
                {
                  "Id": "test-mapping",
                  "Description": "Test key press",
                  "InputType": "NoteOn",
                  "Note": 60,
                  "Channel": 1,
                  "Action": {
                    "$type": "KeyPressReleaseConfig",
                    "VirtualKeyCode": 65,
                    "Description": "Press A key"
                  }
                }
              ]
            }
          ]
        }
        """;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, validJson);

            // Act
            var result = _loader.LoadConfiguration(tempFile);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Profile", result.ProfileName);
            Assert.Equal("Test description", result.Description);
            Assert.Single(result.MidiDevices);

            var device = result.MidiDevices[0];
            Assert.Equal("Test-Device", device.InputProfile);
            Assert.Equal("Test Controller", device.DeviceName);
            Assert.Single(device.Mappings);

            var mapping = device.Mappings[0];
            Assert.Equal("test-mapping", mapping.Id);
            Assert.Equal("Test key press", mapping.Description);
            Assert.Equal("NoteOn", mapping.InputType);
            Assert.Equal(60, mapping.Note);
            Assert.Equal(1, mapping.Channel);
            Assert.True(mapping.IsEnabled);

            Assert.IsType<KeyPressReleaseConfig>(mapping.Action);
            var action = (KeyPressReleaseConfig)mapping.Action;
            Assert.Equal((ushort)65, action.VirtualKeyCode);
            Assert.Equal("Press A key", action.Description);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadConfiguration_WithInvalidJson_ReturnsNull()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var tempFile = Path.GetTempFileName();

        try
        {
            File.WriteAllText(tempFile, invalidJson);

            // Act
            var result = _loader.LoadConfiguration(tempFile);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void LoadConfiguration_WithMissingProfileName_ReturnsNull()
    {
        // Arrange
        var invalidJson = """
        {
          "Description": "Test description",
          "MidiDevices": []
        }
        """;

        var tempFile = Path.GetTempFileName();
        try
        {
            File.WriteAllText(tempFile, invalidJson);

            // Act
            var result = _loader.LoadConfiguration(tempFile);

            // Assert
            Assert.Null(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ConvertToMappings_WithValidConfiguration_ReturnsCorrectMappings()
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
                            Description = "Test mapping",
                            InputType = "NoteOn",
                            Note = 60,
                            Channel = 1,
                            Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                        }
                    }
                }
            }
        };

        // The real factory will create the actual action

        // Act
        var result = _loader.ConvertToMappings(config);

        // Assert
        Assert.Single(result);
        var mapping = result[0];
        Assert.Equal("Test mapping", mapping.Description);
        Assert.Equal(UnifiedActionMidiInputType.NoteOn, mapping.Input.InputType);
        Assert.Equal(60, mapping.Input.InputNumber);
        Assert.Equal(1, mapping.Input.Channel);
        Assert.Equal("Test Controller", mapping.Input.DeviceName);
        Assert.NotNull(mapping.Action);
        Assert.IsType<MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction>(mapping.Action);
        Assert.True(mapping.IsEnabled);
    }

    [Fact]
    public void LoadMappingsIntoRegistry_WithValidConfiguration_ReturnsTrue()
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
                            Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                        }
                    }
                }
            }
        };

        var registryLogger = LoggerFactory.Create(builder => builder.AddConsole()).CreateLogger<UnifiedActionMappingRegistry>();
        var registry = new UnifiedActionMappingRegistry(registryLogger);

        // Act
        var result = _loader.LoadMappingsIntoRegistry(registry, config);

        // Assert
        Assert.True(result);

        // Verify that mappings were loaded into the registry
        var allMappings = registry.GetAllMappings().ToList();
        Assert.Single(allMappings);
        Assert.Equal(UnifiedActionMidiInputType.NoteOn, allMappings[0].Input.InputType);
    }

    [Fact]
    public void SaveConfiguration_WithValidConfiguration_ReturnsTrue()
    {
        // Arrange
        var config = new UnifiedMappingConfig
        {
            ProfileName = "Test Save Profile",
            Description = "Test save description",
            MidiDevices = new List<UnifiedDeviceConfig>
            {
                new UnifiedDeviceConfig
                {
                    DeviceName = "Test Controller",
                    Mappings = new List<UnifiedMappingConfigEntry>
                    {
                        new UnifiedMappingConfigEntry
                        {
                            Description = "Test save mapping",
                            InputType = "NoteOn",
                            Note = 60,
                            Channel = 1,
                            Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                        }
                    }
                }
            }
        };

        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var result = _loader.SaveConfiguration(config, tempFile);

            // Assert
            Assert.True(result);
            Assert.True(File.Exists(tempFile));

            // Verify the file content can be read back
            var savedContent = File.ReadAllText(tempFile);
            Assert.Contains("Test Save Profile", savedContent);
            Assert.Contains("Test save description", savedContent);
            Assert.Contains("KeyPressReleaseConfig", savedContent);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void SaveConfiguration_WithInvalidConfiguration_ReturnsFalse()
    {
        // Arrange
        var invalidConfig = new UnifiedMappingConfig
        {
            ProfileName = "", // Invalid - empty profile name
            MidiDevices = new List<UnifiedDeviceConfig>()
        };

        var tempFile = Path.GetTempFileName();
        try
        {
            // Act
            var result = _loader.SaveConfiguration(invalidConfig, tempFile);

            // Assert
            Assert.False(result);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void SaveLoadRoundTrip_WithComplexConfiguration_MaintainsDataIntegrity()
    {
        // Arrange
        var originalConfig = new UnifiedMappingConfig
        {
            ProfileName = "Round Trip Test",
            Description = "Testing round-trip serialization",
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
                            Id = "test-key-mapping",
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
                        },
                        new UnifiedMappingConfigEntry
                        {
                            Id = "test-cc-mapping",
                            Description = "Test CC mapping",
                            InputType = "ControlChange",
                            ControllerNumber = 1,
                            Channel = 2,
                            IsEnabled = false,
                            Action = new MouseClickConfig
                            {
                                Button = MouseButton.Left,
                                Description = "Left click"
                            }
                        }
                    }
                }
            }
        };

        var tempFile = Path.GetTempFileName();
        try
        {
            // Act - Save and then load
            var saveResult = _loader.SaveConfiguration(originalConfig, tempFile);
            var loadedConfig = _loader.LoadConfiguration(tempFile);

            // Assert
            Assert.True(saveResult);
            Assert.NotNull(loadedConfig);

            // Verify profile-level properties
            Assert.Equal(originalConfig.ProfileName, loadedConfig.ProfileName);
            Assert.Equal(originalConfig.Description, loadedConfig.Description);
            Assert.Equal(originalConfig.MidiDevices.Count, loadedConfig.MidiDevices.Count);

            // Verify device-level properties
            var originalDevice = originalConfig.MidiDevices[0];
            var loadedDevice = loadedConfig.MidiDevices[0];
            Assert.Equal(originalDevice.DeviceName, loadedDevice.DeviceName);
            Assert.Equal(originalDevice.InputProfile, loadedDevice.InputProfile);
            Assert.Equal(originalDevice.Mappings.Count, loadedDevice.Mappings.Count);

            // Verify first mapping
            var originalMapping1 = originalDevice.Mappings[0];
            var loadedMapping1 = loadedDevice.Mappings[0];
            Assert.Equal(originalMapping1.Id, loadedMapping1.Id);
            Assert.Equal(originalMapping1.Description, loadedMapping1.Description);
            Assert.Equal(originalMapping1.InputType, loadedMapping1.InputType);
            Assert.Equal(originalMapping1.Note, loadedMapping1.Note);
            Assert.Equal(originalMapping1.Channel, loadedMapping1.Channel);
            Assert.Equal(originalMapping1.IsEnabled, loadedMapping1.IsEnabled);

            // Verify action type preservation
            Assert.IsType<KeyPressReleaseConfig>(loadedMapping1.Action);
            var loadedAction1 = (KeyPressReleaseConfig)loadedMapping1.Action;
            var originalAction1 = (KeyPressReleaseConfig)originalMapping1.Action;
            Assert.Equal(originalAction1.VirtualKeyCode, loadedAction1.VirtualKeyCode);
            Assert.Equal(originalAction1.Description, loadedAction1.Description);

            // Verify second mapping
            var originalMapping2 = originalDevice.Mappings[1];
            var loadedMapping2 = loadedDevice.Mappings[1];
            Assert.Equal(originalMapping2.Id, loadedMapping2.Id);
            Assert.Equal(originalMapping2.InputType, loadedMapping2.InputType);
            Assert.Equal(originalMapping2.ControllerNumber, loadedMapping2.ControllerNumber);
            Assert.Equal(originalMapping2.IsEnabled, loadedMapping2.IsEnabled);

            Assert.IsType<MouseClickConfig>(loadedMapping2.Action);
            var loadedAction2 = (MouseClickConfig)loadedMapping2.Action;
            var originalAction2 = (MouseClickConfig)originalMapping2.Action;
            Assert.Equal(originalAction2.Button, loadedAction2.Button);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ConvertFromMappings_WithValidMappings_ReturnsCorrectConfiguration()
    {
        // Arrange
        var mappings = new List<UnifiedActionMapping>
        {
            new UnifiedActionMapping
            {
                Input = new UnifiedActionMidiInput
                {
                    InputType = UnifiedActionMidiInputType.NoteOn,
                    InputNumber = 60,
                    Channel = 1,
                    DeviceName = "Test Controller"
                },
                Action = new MIDIFlux.Core.Actions.Simple.KeyPressReleaseAction(new KeyPressReleaseConfig
                {
                    VirtualKeyCode = 65,
                    Description = "Press A key"
                }),
                Description = "Test mapping",
                IsEnabled = true
            }
        };

        // Act
        var config = _loader.ConvertFromMappings(mappings, "Test Profile", "Test description");

        // Assert
        Assert.Equal("Test Profile", config.ProfileName);
        Assert.Equal("Test description", config.Description);
        Assert.Single(config.MidiDevices);

        var device = config.MidiDevices[0];
        Assert.Equal("Test Controller", device.DeviceName);
        Assert.Single(device.Mappings);

        var mapping = device.Mappings[0];
        Assert.Equal("Test mapping", mapping.Description);
        Assert.Equal("NoteOn", mapping.InputType);
        Assert.Equal(60, mapping.Note);
        Assert.Equal(1, mapping.Channel);
        Assert.True(mapping.IsEnabled);
    }
}
