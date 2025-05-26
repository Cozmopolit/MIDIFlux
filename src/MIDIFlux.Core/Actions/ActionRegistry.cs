using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Models;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Centralized registry of all available action types with their metadata.
/// Provides a single source of truth for action enumeration, eliminating
/// hardcoded lists and duplicate switch statements in the GUI layer.
/// </summary>
public static class ActionRegistry
{
    private static readonly Lazy<IReadOnlyList<ActionDescriptor>> _descriptors = new(CreateDescriptors);
    private static readonly Lazy<IReadOnlyDictionary<string, ActionDescriptor>> _classNameLookup = new(CreateClassNameLookup);

    /// <summary>
    /// Gets all available action descriptors
    /// </summary>
    public static IReadOnlyList<ActionDescriptor> All => _descriptors.Value;

    /// <summary>
    /// Gets action descriptors filtered by category
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <returns>Action descriptors in the specified category</returns>
    public static IEnumerable<ActionDescriptor> GetByCategory(ActionCategory category)
    {
        return All.Where(d => d.Category == category && d.IsAvailable);
    }

    /// <summary>
    /// Gets action descriptors filtered by multiple categories
    /// </summary>
    /// <param name="categories">The categories to include</param>
    /// <returns>Action descriptors in any of the specified categories</returns>
    public static IEnumerable<ActionDescriptor> GetByCategories(params ActionCategory[] categories)
    {
        var categorySet = categories.ToHashSet();
        return All.Where(d => categorySet.Contains(d.Category) && d.IsAvailable);
    }

    /// <summary>
    /// Gets an action descriptor by its action type
    /// </summary>
    /// <param name="actionType">The action type to find</param>
    /// <returns>The action descriptor, or null if not found</returns>
    public static ActionDescriptor? GetByActionType(ActionType actionType)
    {
        return All.FirstOrDefault(d => d.ActionType == actionType);
    }

    /// <summary>
    /// Gets the display name for an action type
    /// </summary>
    /// <param name="actionType">The action type</param>
    /// <returns>The display name, or "Unknown" if not found</returns>
    public static string GetDisplayName(ActionType actionType)
    {
        return GetByActionType(actionType)?.DisplayName ?? "Unknown";
    }

    /// <summary>
    /// Gets the display name for an action configuration
    /// </summary>
    /// <param name="actionConfig">The action configuration</param>
    /// <returns>The display name, or "Unknown" if not found</returns>
    public static string GetDisplayName(ActionConfig actionConfig)
    {
        return GetDisplayName(actionConfig.Type);
    }

    /// <summary>
    /// Gets the display name for an action instance
    /// </summary>
    /// <param name="action">The action instance</param>
    /// <returns>The display name, or "Unknown" if not found</returns>
    public static string GetDisplayName(IAction action)
    {
        // Try to get the ActionType from the action's configuration using reflection
        var configProperty = action.GetType().GetProperty("Config", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (configProperty?.GetValue(action) is ActionConfig config)
        {
            return GetDisplayName(config);
        }

        // Fallback: try to find by action class name using the lookup dictionary
        var actionTypeName = action.GetType().Name;
        if (_classNameLookup.Value.TryGetValue(actionTypeName, out var descriptor))
        {
            return descriptor.DisplayName;
        }

        return "Unknown";
    }

    /// <summary>
    /// Creates the complete list of action descriptors
    /// </summary>
    private static IReadOnlyList<ActionDescriptor> CreateDescriptors()
    {
        return new List<ActionDescriptor>
        {
            // Keyboard Actions
            new(ActionType.KeyPressRelease, "Key Press/Release", ActionCategory.Keyboard,
                () => new KeyPressReleaseConfig { VirtualKeyCode = 65 }), // 'A' key

            new(ActionType.KeyDown, "Key Down", ActionCategory.Keyboard,
                () => new KeyDownConfig { VirtualKeyCode = 65 }),

            new(ActionType.KeyUp, "Key Up", ActionCategory.Keyboard,
                () => new KeyUpConfig { VirtualKeyCode = 65 }),

            new(ActionType.KeyToggle, "Key Toggle", ActionCategory.Keyboard,
                () => new KeyToggleConfig { VirtualKeyCode = 65 }),

            // Mouse Actions
            new(ActionType.MouseClick, "Mouse Click", ActionCategory.Mouse,
                () => new MouseClickConfig { Button = MouseButton.Left }),

            new(ActionType.MouseScroll, "Mouse Scroll", ActionCategory.Mouse,
                () => new MouseScrollConfig { Direction = ScrollDirection.Up, Amount = 1 }),

            // System Actions
            new(ActionType.CommandExecution, "Command Execution", ActionCategory.System,
                () => new CommandExecutionConfig { Command = "echo test", ShellType = CommandShellType.PowerShell }),

            new(ActionType.Delay, "Delay", ActionCategory.System,
                () => new DelayConfig { Milliseconds = 100 }),

            // Game Controller Actions
            new(ActionType.GameControllerButton, "Game Controller Button", ActionCategory.GameController,
                () => new GameControllerButtonConfig { Button = "A", ControllerIndex = 0 }),

            new(ActionType.GameControllerAxis, "Game Controller Axis", ActionCategory.GameController,
                () => new GameControllerAxisConfig { AxisName = "LeftStickX", ControllerIndex = 0, AxisValue = 0.5f }),

            // MIDI Output Actions
            new(ActionType.MidiOutput, "MIDI Output", ActionCategory.MidiOutput,
                () => new MidiOutputConfig { Commands = new List<MidiOutputCommand>() }),

            // Complex Actions
            new(ActionType.SequenceAction, "Sequence (Macro)", ActionCategory.Complex,
                () => new SequenceConfig { SubActions = new List<ActionConfig>() }),

            new(ActionType.ConditionalAction, "Conditional (CC Range)", ActionCategory.Complex,
                () => new ConditionalConfig { Conditions = new List<ValueConditionConfig>() }),

            new(ActionType.AlternatingAction, "Alternating (Toggle)", ActionCategory.Complex,
                () => new AlternatingActionConfig { PrimaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 65 }, SecondaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 66 } }),

            // Future Actions (marked as unavailable)
            new(ActionType.ValueTransformAction, "Value Transform", ActionCategory.Complex,
                () => throw new NotImplementedException("Value Transform actions are not yet implemented"), false),

            new(ActionType.AudioControl, "Audio Control", ActionCategory.System,
                () => throw new NotImplementedException("Audio Control actions are not yet implemented"), false),

            new(ActionType.SystemIntegration, "System Integration", ActionCategory.System,
                () => throw new NotImplementedException("System Integration actions are not yet implemented"), false)
        }.AsReadOnly();
    }

    /// <summary>
    /// Creates a lookup dictionary for action class names to descriptors
    /// </summary>
    private static IReadOnlyDictionary<string, ActionDescriptor> CreateClassNameLookup()
    {
        var lookup = new Dictionary<string, ActionDescriptor>();

        foreach (var descriptor in All)
        {
            // Generate the expected class name from the ActionType enum
            // Most follow the pattern: ActionType + "Action" suffix
            var className = descriptor.ActionType.ToString() + "Action";

            // Handle special cases where the class name doesn't match the enum name exactly
            if (descriptor.ActionType == ActionType.KeyPressRelease)
                className = "KeyPressReleaseAction";

            lookup[className] = descriptor;
        }

        return lookup.AsReadOnly();
    }
}
