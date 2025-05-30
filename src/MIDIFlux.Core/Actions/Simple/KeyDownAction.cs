using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.State;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for pressing a key down (and optionally auto-releasing it).
/// Consolidates KeyDownConfig into the action class using the parameter system.
/// </summary>
[ActionDisplayName("Key Down")]
public class KeyDownAction : ActionBase
{
    // Parameter names as constants for type safety
    private const string VirtualKeyCodeParam = "VirtualKeyCode";
    private const string AutoReleaseAfterMsParam = "AutoReleaseAfterMs";



    /// <summary>
    /// Initializes a new instance of KeyDownAction with default parameters
    /// </summary>
    public KeyDownAction() : base()
    {
        // Parameters are initialized in InitializeParameters()
    }

    /// <summary>
    /// Initializes a new instance of KeyDownAction with specified key
    /// </summary>
    /// <param name="key">The key to press down</param>
    /// <param name="autoReleaseAfterMs">Optional auto-release time in milliseconds</param>
    public KeyDownAction(Keys key, int? autoReleaseAfterMs = null) : base()
    {
        SetParameterValue(VirtualKeyCodeParam, key);
        SetParameterValue(AutoReleaseAfterMsParam, autoReleaseAfterMs);
    }

    /// <summary>
    /// Initializes the parameters for this action type
    /// </summary>
    protected override void InitializeParameters()
    {
        // Add VirtualKeyCode parameter as enum
        Parameters[VirtualKeyCodeParam] = new Parameter(
            ParameterType.Enum,
            Keys.A, // Default to 'A' key
            "Key")
        {
            EnumDefinition = EnumDefinition.FromEnum<Keys>(),
            ValidationHints = new Dictionary<string, object>
            {
                { "supportsKeyListening", true }
            }
        };

        // Add AutoReleaseAfterMs parameter (optional)
        Parameters[AutoReleaseAfterMsParam] = new Parameter(
            ParameterType.Integer,
            null, // Default to no auto-release
            "Auto Release After (ms)")
        {
            ValidationHints = new Dictionary<string, object>
            {
                { "min", 1 },
                { "max", 60000 }, // Max 1 minute
                { "optional", true }
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

        var autoReleaseAfterMs = GetParameterValue<int?>(AutoReleaseAfterMsParam);
        if (!ActionHelper.IsValidOptionalDelay(autoReleaseAfterMs))
        {
            AddValidationError("AutoReleaseAfterMs must be greater than 0 when specified");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic for the key down action
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    protected override async ValueTask ExecuteAsyncCore(int? midiValue)
    {
        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        var virtualKeyCode = (ushort)key;
        var autoReleaseAfterMs = GetParameterValue<int?>(AutoReleaseAfterMsParam);

        // Get ActionStateManager service if available for state tracking
        var actionStateManager = GetService<ActionStateManager>();
        var stateKey = $"*Key{virtualKeyCode}"; // Internal state key for this key

        // Check current state to avoid duplicate key presses
        if (actionStateManager != null)
        {
            var currentState = actionStateManager.GetState(stateKey);
            if (currentState == 1)
            {
                Logger.LogDebug("Key {VirtualKeyCode} is already pressed (state={State}), skipping key down", virtualKeyCode, currentState);
                return;
            }
        }

        // Create keyboard simulator - no service dependency needed for this simple action
        var keyboardSimulator = new KeyboardSimulator(Logger);

        // Press the key down
        if (!keyboardSimulator.SendKeyDown(virtualKeyCode))
        {
            var errorMsg = $"Failed to send key down for virtual key code {virtualKeyCode}";
            Logger.LogError(errorMsg);
            ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
            return;
        }

        // Update state if state manager is available
        if (actionStateManager != null)
        {
            actionStateManager.SetState(stateKey, 1); // 1 = pressed
        }

        Logger.LogTrace("Successfully pressed key down for VirtualKeyCode={VirtualKeyCode}", virtualKeyCode);

        // Handle auto-release if configured
        if (autoReleaseAfterMs.HasValue)
        {
            await Task.Delay(autoReleaseAfterMs.Value);

            // Release the key
            if (!keyboardSimulator.SendKeyUp(virtualKeyCode))
            {
                var errorMsg = $"Failed to auto-release key for virtual key code {virtualKeyCode}";
                Logger.LogError(errorMsg);
                ApplicationErrorHandler.ShowWarning(errorMsg, "MIDIFlux - Keyboard Action Error", Logger);
                return;
            }

            // Update state if state manager is available
            if (actionStateManager != null)
            {
                actionStateManager.SetState(stateKey, 0); // 0 = not pressed
            }

            Logger.LogTrace("Successfully auto-released key for VirtualKeyCode={VirtualKeyCode} after {Delay}ms",
                virtualKeyCode, autoReleaseAfterMs.Value);
        }
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    /// <returns>A default description string</returns>
    protected override string GetDefaultDescription()
    {
        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        var autoReleaseAfterMs = GetParameterValue<int?>(AutoReleaseAfterMsParam);
        var autoReleaseText = autoReleaseAfterMs.HasValue ? $" (auto-release: {autoReleaseAfterMs}ms)" : "";
        return $"Press Key Down ({key}){autoReleaseText}";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    /// <returns>An error message string</returns>
    protected override string GetErrorMessage()
    {
        var key = GetParameterValue<Keys>(VirtualKeyCodeParam);
        return $"Error executing KeyDownAction for key {key}";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// KeyDownAction is only compatible with trigger signals (discrete events).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public static InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.Trigger };
    }
}
