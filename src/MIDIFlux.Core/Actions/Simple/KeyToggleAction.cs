using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Actions.Parameters;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for toggling the state of a key (like CapsLock, NumLock, etc.).
/// Consolidates KeyToggleConfig into the action class using the parameter system.
/// </summary>
[ActionDisplayName("Key Toggle")]
public class KeyToggleAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string VirtualKeyCodeParam = "VirtualKeyCode";



    /// <summary>
    /// Initializes a new instance of KeyToggleAction with default parameters
    /// </summary>
    public KeyToggleAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of KeyToggleAction with specified key
    /// </summary>
    /// <param name="key">The key to toggle</param>
    public KeyToggleAction(Keys key) : base()
    {
        SetParameterValue(VirtualKeyCodeParam, key);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add VirtualKeyCode parameter as enum
        Parameters[VirtualKeyCodeParam] = new Parameter(
            ParameterType.Enum,
            null, // No default - user must specify key
            "Key")
        {
            EnumDefinition = EnumDefinition.FromEnum<Keys>(),
            ValidationHints = new Dictionary<string, object>
            {
                { "supportsKeyListening", true }
            }
        };
    }

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        if (!ActionHelper.IsValidVirtualKeyCode((ushort)key))
        {
            AddValidationError("Key must be a valid virtual key code");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the key toggle action.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        var virtualKeyCode = (ushort)key;

        // Create keyboard simulator - no service dependency needed for this simple action
        var keyboardSimulator = new KeyboardSimulator(Logger);

        // For toggle keys, we need to press and release to toggle the state
        // This works for keys like CapsLock, NumLock, ScrollLock, etc.
        if (!keyboardSimulator.SendKeyDown(virtualKeyCode))
        {
            var errorMsg = $"Failed to send key down for toggle key virtual key code {virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        if (!keyboardSimulator.SendKeyUp(virtualKeyCode))
        {
            var errorMsg = $"Failed to send key up for toggle key virtual key code {virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return ValueTask.CompletedTask;
        }

        Logger.LogTrace("Successfully executed KeyToggleAction for VirtualKeyCode={VirtualKeyCode}", virtualKeyCode);

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        try
        {
            var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
            return $"Toggle Key ({key})";
        }
        catch
        {
            // During JSON deserialization, parameters may not be set yet
            return "Toggle Key";
        }
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        return $"Error executing KeyToggleAction for key {key}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// KeyToggleAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
