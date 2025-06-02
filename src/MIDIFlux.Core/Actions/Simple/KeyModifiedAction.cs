using System.Windows.Forms;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Extensions;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for executing modified key combinations (e.g., Ctrl+C, Ctrl+Shift+A, Alt+Tab).
/// Automatically handles modifier key press/release sequences for user-friendly configuration.
/// Supports up to 4 modifiers for complex combinations.
/// </summary>
[ActionDisplayName("Key Modified")]
public class KeyModifiedAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string MainKeyParam = "MainKey";
    private const string Modifier1Param = "Modifier1";
    private const string Modifier2Param = "Modifier2";
    private const string Modifier3Param = "Modifier3";
    private const string Modifier4Param = "Modifier4";

    /// <summary>
    /// Gets or sets the description for this action.
    /// Dynamically generates description based on current parameter values.
    /// </summary>
    public new string Description
    {
        get => GetDefaultDescription();
        set { /* Ignore setter - description is always dynamic */ }
    }

    /// <summary>
    /// Initializes a new instance of KeyModifiedAction with default parameters
    /// </summary>
    public KeyModifiedAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of KeyModifiedAction with specified key combination
    /// </summary>
    /// <param name="mainKey">The main key to press</param>
    /// <param name="modifier1">First modifier key (optional)</param>
    /// <param name="modifier2">Second modifier key (optional)</param>
    /// <param name="modifier3">Third modifier key (optional)</param>
    /// <param name="modifier4">Fourth modifier key (optional)</param>
    public KeyModifiedAction(Keys mainKey, Keys? modifier1 = null, Keys? modifier2 = null, Keys? modifier3 = null, Keys? modifier4 = null) : base()
    {
        SetParameterValue(MainKeyParam, mainKey);
        SetParameterValue(Modifier1Param, modifier1);
        SetParameterValue(Modifier2Param, modifier2);
        SetParameterValue(Modifier3Param, modifier3);
        SetParameterValue(Modifier4Param, modifier4);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add MainKey parameter - only non-modifier keys allowed
        Parameters[MainKeyParam] = new Parameter(
            ParameterType.Enum,
            null, // No default - user must specify key
            "Main Key")
        {
            EnumDefinition = CreateMainKeyEnum(),
            ValidationHints = new Dictionary<string, object>
            {
                { "supportsKeyListening", true },
                { "description", "The primary key to press (non-modifier keys only)" }
            }
        };

        // Add Modifier parameters - only modifier keys allowed
        Parameters[Modifier1Param] = new Parameter(
            ParameterType.Enum,
            null, // Optional - no default
            "Modifier 1")
        {
            EnumDefinition = CreateModifierKeyEnum(),
            ValidationHints = new Dictionary<string, object>
            {
                { "optional", true },
                { "description", "First modifier key (Ctrl, Shift, Alt, Win)" }
            }
        };

        Parameters[Modifier2Param] = new Parameter(
            ParameterType.Enum,
            null, // Optional - no default
            "Modifier 2")
        {
            EnumDefinition = CreateModifierKeyEnum(),
            ValidationHints = new Dictionary<string, object>
            {
                { "optional", true },
                { "description", "Second modifier key (Ctrl, Shift, Alt, Win)" }
            }
        };

        Parameters[Modifier3Param] = new Parameter(
            ParameterType.Enum,
            null, // Optional - no default
            "Modifier 3")
        {
            EnumDefinition = CreateModifierKeyEnum(),
            ValidationHints = new Dictionary<string, object>
            {
                { "optional", true },
                { "description", "Third modifier key (Ctrl, Shift, Alt, Win)" }
            }
        };

        Parameters[Modifier4Param] = new Parameter(
            ParameterType.Enum,
            null, // Optional - no default
            "Modifier 4")
        {
            EnumDefinition = CreateModifierKeyEnum(),
            ValidationHints = new Dictionary<string, object>
            {
                { "optional", true },
                { "description", "Fourth modifier key (Ctrl, Shift, Alt, Win)" }
            }
        };
    }

    /// <summary>
    /// Creates an EnumDefinition containing only non-modifier keys for the main key parameter
    /// </summary>
    /// <returns>EnumDefinition with non-modifier keys</returns>
    private static EnumDefinition CreateMainKeyEnum()
    {
        var allKeys = Enum.GetValues<Keys>();
        var nonModifierKeys = allKeys.Where(k => !KeyboardStringParser.IsModifierKey((int)k)).ToArray();
        var names = nonModifierKeys.Select(k => k.ToString()).ToArray();
        return new EnumDefinition(names, nonModifierKeys.Cast<object>().ToArray());
    }

    /// <summary>
    /// Creates an EnumDefinition containing only modifier keys for modifier parameters
    /// </summary>
    /// <returns>EnumDefinition with modifier keys only</returns>
    private static EnumDefinition CreateModifierKeyEnum()
    {
        var allKeys = Enum.GetValues<Keys>();
        var modifierKeys = allKeys.Where(k => KeyboardStringParser.IsModifierKey((int)k)).ToArray();
        var names = modifierKeys.Select(k => k.ToString()).ToArray();
        return new EnumDefinition(names, modifierKeys.Cast<object>().ToArray());
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var mainKey = GetParameterValue<Keys?>(MainKeyParam);
        if (mainKey == null)
        {
            AddValidationError("Main Key must be specified");
        }
        else if (KeyboardStringParser.IsModifierKey((int)mainKey.Value))
        {
            AddValidationError("Main Key cannot be a modifier key");
        }

        // Collect all non-null modifiers for duplicate checking
        var modifiers = new List<Keys>();
        var modifier1 = GetParameterValue<Keys?>(Modifier1Param);
        var modifier2 = GetParameterValue<Keys?>(Modifier2Param);
        var modifier3 = GetParameterValue<Keys?>(Modifier3Param);
        var modifier4 = GetParameterValue<Keys?>(Modifier4Param);

        if (modifier1.HasValue) modifiers.Add(modifier1.Value);
        if (modifier2.HasValue) modifiers.Add(modifier2.Value);
        if (modifier3.HasValue) modifiers.Add(modifier3.Value);
        if (modifier4.HasValue) modifiers.Add(modifier4.Value);

        // Check for duplicate modifiers
        var duplicates = modifiers.GroupBy(m => m).Where(g => g.Count() > 1).Select(g => g.Key);
        foreach (var duplicate in duplicates)
        {
            AddValidationError($"Modifier key '{duplicate}' is specified multiple times");
        }

        // Validate that all specified modifiers are actually modifier keys
        foreach (var modifier in modifiers)
        {
            if (!KeyboardStringParser.IsModifierKey((int)modifier))
            {
                AddValidationError($"'{modifier}' is not a valid modifier key");
            }
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// KeyModifiedAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }

    /// <summary>
    /// Core execution logic for the key modified action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var mainKey = GetParameterValue<Keys?>(MainKeyParam);
        if (mainKey == null)
        {
            Logger.LogError("Cannot execute KeyModifiedAction: MainKey is not specified");
            return ValueTask.CompletedTask;
        }

        // Collect all non-null modifiers in order
        var modifiers = new List<ushort>();
        var modifier1 = GetParameterValue<Keys?>(Modifier1Param);
        var modifier2 = GetParameterValue<Keys?>(Modifier2Param);
        var modifier3 = GetParameterValue<Keys?>(Modifier3Param);
        var modifier4 = GetParameterValue<Keys?>(Modifier4Param);

        if (modifier1.HasValue) modifiers.Add((ushort)modifier1.Value);
        if (modifier2.HasValue) modifiers.Add((ushort)modifier2.Value);
        if (modifier3.HasValue) modifiers.Add((ushort)modifier3.Value);
        if (modifier4.HasValue) modifiers.Add((ushort)modifier4.Value);

        var mainKeyCode = (ushort)mainKey.Value;

        // Create keyboard simulator
        var keyboardSimulator = new KeyboardSimulator(Logger);

        // Execute the key combination using the extension method
        if (!keyboardSimulator.PressAndReleaseKey(mainKeyCode, modifiers))
        {
            var modifierText = modifiers.Count > 0 ? string.Join("+", modifiers.Select(m => ((Keys)m).ToString())) + "+" : "";
            var errorMsg = $"Failed to execute key combination: {modifierText}{mainKey}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        var combinationText = BuildCombinationText(modifiers, mainKey.Value);
        Logger.LogTrace("Successfully executed key combination: {Combination}", combinationText);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Builds a human-readable text representation of the key combination
    /// </summary>
    /// <param name="modifiers">List of modifier key codes</param>
    /// <param name="mainKey">The main key</param>
    /// <returns>Human-readable combination text (e.g., "Ctrl+Shift+C")</returns>
    private static string BuildCombinationText(List<ushort> modifiers, Keys mainKey)
    {
        var parts = new List<string>();
        parts.AddRange(modifiers.Select(m => ((Keys)m).ToString()));
        parts.Add(mainKey.ToString());
        return string.Join("+", parts);
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var mainKey = GetParameterValue<Keys?>(MainKeyParam);
        if (mainKey == null)
        {
            return "Key Modified";
        }

        // Build combination text for description
        var modifiers = new List<ushort>();
        var modifier1 = GetParameterValue<Keys?>(Modifier1Param);
        var modifier2 = GetParameterValue<Keys?>(Modifier2Param);
        var modifier3 = GetParameterValue<Keys?>(Modifier3Param);
        var modifier4 = GetParameterValue<Keys?>(Modifier4Param);

        if (modifier1.HasValue) modifiers.Add((ushort)modifier1.Value);
        if (modifier2.HasValue) modifiers.Add((ushort)modifier2.Value);
        if (modifier3.HasValue) modifiers.Add((ushort)modifier3.Value);
        if (modifier4.HasValue) modifiers.Add((ushort)modifier4.Value);

        var combinationText = BuildCombinationText(modifiers, mainKey.Value);
        return $"Key Modified ({combinationText})";
    }
}
