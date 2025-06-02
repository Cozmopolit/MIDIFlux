using System.Windows.Forms;
using FluentAssertions;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Tests.Infrastructure;
using Xunit;

namespace MIDIFlux.Core.Tests.Actions.Simple;

/// <summary>
/// Tests for KeyModifiedAction - modified key combination execution
/// </summary>
public class KeyModifiedActionTests : ActionTestBase
{
    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var action = new KeyModifiedAction();

        // Assert
        action.Should().NotBeNull();
        action.Description.Should().Be("Key Modified");
        AssertCompatibleInputCategories(action, InputTypeCategory.Trigger);
    }

    [Fact]
    public void Constructor_WithParameters_ShouldSetValues()
    {
        // Arrange & Act
        var action = new KeyModifiedAction(Keys.C, Keys.ControlKey, Keys.ShiftKey);

        // Assert
        action.GetParameterValue<Keys?>("MainKey").Should().Be(Keys.C);
        action.GetParameterValue<Keys?>("Modifier1").Should().Be(Keys.ControlKey);
        action.GetParameterValue<Keys?>("Modifier2").Should().Be(Keys.ShiftKey);
        action.GetParameterValue<Keys?>("Modifier3").Should().BeNull();
        action.GetParameterValue<Keys?>("Modifier4").Should().BeNull();
    }

    [Fact]
    public void IsValid_WithValidConfiguration_ShouldReturnTrue()
    {
        // Arrange
        var action = new KeyModifiedAction(Keys.C, Keys.ControlKey);

        // Act & Assert
        action.IsValid().Should().BeTrue();
        action.GetValidationErrors().Should().BeEmpty();
    }

    [Fact]
    public void IsValid_WithoutMainKey_ShouldReturnFalse()
    {
        // Arrange
        var action = new KeyModifiedAction();

        // Act & Assert
        action.IsValid().Should().BeFalse();
        action.GetValidationErrors().Should().Contain("Main Key must be specified");
    }

    [Fact]
    public void IsValid_WithModifierAsMainKey_ShouldReturnFalse()
    {
        // Arrange
        var action = new KeyModifiedAction();
        action.SetParameterValue("MainKey", Keys.ControlKey);

        // Act & Assert
        action.IsValid().Should().BeFalse();
        action.GetValidationErrors().Should().Contain("Main Key cannot be a modifier key");
    }

    [Fact]
    public void IsValid_WithDuplicateModifiers_ShouldReturnFalse()
    {
        // Arrange
        var action = new KeyModifiedAction(Keys.C, Keys.ControlKey, Keys.ControlKey);

        // Act & Assert
        action.IsValid().Should().BeFalse();
        action.GetValidationErrors().Should().Contain("Modifier key 'ControlKey' is specified multiple times");
    }

    [Fact]
    public void IsValid_WithNonModifierAsModifier_ShouldReturnFalse()
    {
        // Arrange
        var action = new KeyModifiedAction();
        action.SetParameterValue("MainKey", Keys.C);
        action.SetParameterValue("Modifier1", Keys.A); // A is not a modifier key

        // Act & Assert
        action.IsValid().Should().BeFalse();
        action.GetValidationErrors().Should().Contain("'A' is not a valid modifier key");
    }

    [Fact]
    public void GetDefaultDescription_WithValidConfiguration_ShouldReturnCombination()
    {
        // Arrange
        var action = new KeyModifiedAction(Keys.C, Keys.ControlKey, Keys.ShiftKey);

        // Act
        var description = action.Description;

        // Assert
        description.Should().Be("Key Modified (ControlKey+ShiftKey+C)");
    }

    [Fact]
    public void GetDefaultDescription_WithoutConfiguration_ShouldReturnDefault()
    {
        // Arrange
        var action = new KeyModifiedAction();

        // Act
        var description = action.Description;

        // Assert
        description.Should().Be("Key Modified");
    }

    [Theory]
    [InlineData(Keys.C, Keys.ControlKey, null, null, null, "Key Modified (ControlKey+C)")]
    [InlineData(Keys.A, Keys.ControlKey, Keys.ShiftKey, null, null, "Key Modified (ControlKey+ShiftKey+A)")]
    [InlineData(Keys.F, Keys.ControlKey, Keys.ShiftKey, Keys.Menu, null, "Key Modified (ControlKey+ShiftKey+Menu+F)")]
    [InlineData(Keys.Tab, Keys.Menu, null, null, null, "Key Modified (Menu+Tab)")]
    public void GetDefaultDescription_WithVariousCombinations_ShouldReturnCorrectText(
        Keys mainKey, Keys? mod1, Keys? mod2, Keys? mod3, Keys? mod4, string expected)
    {
        // Arrange
        var action = new KeyModifiedAction(mainKey, mod1, mod2, mod3, mod4);

        // Act
        var description = action.Description;

        // Assert
        description.Should().Be(expected);
    }

    [Fact]
    public async Task ExecuteAsync_WithValidConfiguration_ShouldComplete()
    {
        // Arrange
        var action = new KeyModifiedAction(Keys.C, Keys.ControlKey);

        // Act & Assert - Should not throw
        await action.ExecuteAsync(64);
    }

    [Fact]
    public async Task ExecuteAsync_WithoutMainKey_ShouldComplete()
    {
        // Arrange
        var action = new KeyModifiedAction();

        // Act & Assert - Should not throw (logs error but completes)
        await action.ExecuteAsync(64);
    }

    [Fact]
    public void ParameterEnums_ShouldBeCorrectlyFiltered()
    {
        // Arrange
        var action = new KeyModifiedAction();
        var parameterList = action.GetParameterList();

        // Act
        var mainKeyParam = parameterList.First(p => p.Name == "MainKey");
        var modifier1Param = parameterList.First(p => p.Name == "Modifier1");

        // Assert
        mainKeyParam.EnumDefinition.Should().NotBeNull();
        modifier1Param.EnumDefinition.Should().NotBeNull();

        // MainKey should not contain modifier keys
        var mainKeyOptions = mainKeyParam.EnumDefinition!.Values.Cast<Keys>().ToArray();
        mainKeyOptions.Should().NotContain(Keys.ControlKey);
        mainKeyOptions.Should().NotContain(Keys.ShiftKey);
        mainKeyOptions.Should().NotContain(Keys.Menu);
        mainKeyOptions.Should().Contain(Keys.A);
        mainKeyOptions.Should().Contain(Keys.C);

        // Modifier should only contain modifier keys
        var modifierOptions = modifier1Param.EnumDefinition!.Values.Cast<Keys>().ToArray();
        modifierOptions.Should().Contain(Keys.ControlKey);
        modifierOptions.Should().Contain(Keys.ShiftKey);
        modifierOptions.Should().Contain(Keys.Menu);
        modifierOptions.Should().NotContain(Keys.A);
        modifierOptions.Should().NotContain(Keys.C);
    }

    [Fact]
    public void ValidationHints_ShouldBeSetCorrectly()
    {
        // Arrange
        var action = new KeyModifiedAction();
        var parameterList = action.GetParameterList();

        // Act
        var mainKeyParam = parameterList.First(p => p.Name == "MainKey");
        var modifier1Param = parameterList.First(p => p.Name == "Modifier1");

        // Assert
        mainKeyParam.ValidationHints.Should().ContainKey("supportsKeyListening");
        mainKeyParam.ValidationHints!["supportsKeyListening"].Should().Be(true);

        modifier1Param.ValidationHints.Should().ContainKey("optional");
        modifier1Param.ValidationHints!["optional"].Should().Be(true);
    }
}
