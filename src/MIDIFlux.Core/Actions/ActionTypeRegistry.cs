using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Registry for action types using reflection-based discovery.
/// Eliminates the need for massive switch statements by automatically discovering action types.
/// Thread-safe and optimized for performance with caching.
/// </summary>
public class ActionTypeRegistry
{
    private static readonly Lazy<ActionTypeRegistry> _instance = new(() => new ActionTypeRegistry());
    private readonly ConcurrentDictionary<string, Type> _actionTypes = new();
    private readonly ConcurrentDictionary<string, string> _displayNames = new();
    private readonly ConcurrentDictionary<string, ActionCategory> _categories = new();
    private readonly object _initializationLock = new();
    private bool _initialized = false;

    /// <summary>
    /// Gets the singleton instance of the ActionTypeRegistry
    /// </summary>
    public static ActionTypeRegistry Instance => _instance.Value;

    /// <summary>
    /// Private constructor for singleton pattern
    /// </summary>
    private ActionTypeRegistry()
    {
    }

    /// <summary>
    /// Initializes the registry by discovering all action types in the current assembly.
    /// This is called automatically on first use but can be called explicitly for eager initialization.
    /// </summary>
    public void Initialize()
    {
        if (_initialized)
            return;

        lock (_initializationLock)
        {
            if (_initialized)
                return;

            DiscoverActionTypes();
            _initialized = true;
        }
    }

    /// <summary>
    /// Gets the action type for the given type name.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="typeName">The action type name (e.g., "KeyPressReleaseAction")</param>
    /// <returns>The Type object for the action, or null if not found</returns>
    public Type? GetActionType(string typeName)
    {
        if (!_initialized)
        {
            Initialize();
        }

        _actionTypes.TryGetValue(typeName, out var actionType);
        return actionType;
    }

    /// <summary>
    /// Gets all registered action types.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <returns>Dictionary of type name to Type mappings</returns>
    public IReadOnlyDictionary<string, Type> GetAllActionTypes()
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _actionTypes.ToImmutableDictionary();
    }

    /// <summary>
    /// Checks if an action type is registered.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="typeName">The action type name to check</param>
    /// <returns>True if the action type is registered, false otherwise</returns>
    public bool IsActionTypeRegistered(string typeName)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _actionTypes.ContainsKey(typeName);
    }

    /// <summary>
    /// Gets the display name for the given action type name.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="typeName">The action type name (e.g., "KeyPressReleaseAction")</param>
    /// <returns>The display name for the action, or the type name if no display name is found</returns>
    public string GetActionDisplayName(string typeName)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _displayNames.TryGetValue(typeName, out var displayName) ? displayName : typeName;
    }

    /// <summary>
    /// Gets the display name for the given action instance.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="action">The action instance</param>
    /// <returns>The display name for the action, or the type name if no display name is found</returns>
    public string GetActionDisplayName(ActionBase action)
    {
        return GetActionDisplayName(action.GetType().Name);
    }

    /// <summary>
    /// Creates a new instance of the specified action type.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="typeName">The action type name (e.g., "KeyPressReleaseAction")</param>
    /// <returns>A new instance of the action, or null if the type is not found</returns>
    public ActionBase? CreateActionInstance(string typeName)
    {
        var actionType = GetActionType(typeName);
        if (actionType == null)
        {
            return null;
        }

        var logger = LoggingHelper.CreateLogger<ActionTypeRegistry>();

        try
        {
            return SafeActivator.Create<ActionBase>(actionType, logger, typeName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to create action instance '{typeName}': {ex.Message}. This may indicate a missing dependency, initialization error, or configuration issue.", ex);
        }
    }

    /// <summary>
    /// Gets all action types with their display names for GUI population.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <returns>Dictionary of type name to display name mappings</returns>
    public IReadOnlyDictionary<string, string> GetAllActionDisplayNames()
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _displayNames.ToImmutableDictionary();
    }

    /// <summary>
    /// Determines if an action type contains sub-actions (making it a complex workflow action).
    /// Complex actions are those that orchestrate other actions through SubAction, SubActionList, or ValueConditionList parameters.
    /// </summary>
    /// <param name="typeName">The action type name (e.g., "SequenceAction")</param>
    /// <returns>True if the action contains sub-actions, false otherwise</returns>
    public bool HasSubActions(string typeName)
    {
        var actionType = GetActionType(typeName);
        if (actionType == null)
        {
            return false;
        }

        try
        {
            // Create a temporary instance to check its parameters
            var logger = LoggingHelper.CreateLogger<ActionTypeRegistry>();
            var instance = SafeActivator.Create<ActionBase>(actionType, logger, typeName);
            if (instance == null)
            {
                return false;
            }

            // Check if any parameter is a sub-action type
            var parameterList = instance.GetParameterList();
            return parameterList.Any(p =>
                p.Type == Parameters.ParameterType.SubAction ||
                p.Type == Parameters.ParameterType.SubActionList ||
                p.Type == Parameters.ParameterType.ValueConditionList);
        }
        catch
        {
            // If we can't create an instance or check parameters, assume it's not complex
            return false;
        }
    }

    /// <summary>
    /// Discovers all action types in the current assembly using reflection.
    /// Looks for classes that inherit from ActionBase.
    /// </summary>
    private void DiscoverActionTypes()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var actionBaseType = typeof(ActionBase);

            var actionTypes = assembly.GetTypes()
                .Where(type =>
                    type.IsClass &&
                    !type.IsAbstract &&
                    actionBaseType.IsAssignableFrom(type))
                .ToList();

            foreach (var actionType in actionTypes)
            {
                var typeName = actionType.Name;
                _actionTypes.TryAdd(typeName, actionType);

                // Extract display name from attribute or generate a default
                var displayName = GetDisplayNameFromAttribute(actionType) ?? GenerateDefaultDisplayName(typeName);
                _displayNames.TryAdd(typeName, displayName);

                // Extract category from attribute or default to Utility
                var category = GetCategoryFromAttribute(actionType) ?? ActionCategory.Utility;
                _categories.TryAdd(typeName, category);
            }

            // Log discovery results (only if logging is available)
            try
            {
                var logger = LoggingHelper.CreateLogger<ActionTypeRegistry>();
                logger.LogDebug("Discovered {Count} action types: {ActionTypes}",
                    _actionTypes.Count,
                    string.Join(", ", _actionTypes.Keys.OrderBy(k => k)));
            }
            catch
            {
                // Ignore logging errors during discovery
            }
        }
        catch (Exception ex)
        {
            // Log error if possible, but don't fail the application
            try
            {
                var logger = LoggingHelper.CreateLogger<ActionTypeRegistry>();
                logger.LogError(ex, "Failed to discover action types: {Message}", ex.Message);
            }
            catch
            {
                // If logging fails, we can't do much - just continue
            }
        }
    }

    /// <summary>
    /// Manually registers an action type.
    /// Useful for testing or plugin scenarios.
    /// </summary>
    /// <param name="typeName">The type name to register</param>
    /// <param name="actionType">The action type</param>
    /// <returns>True if registration succeeded, false if type name already exists</returns>
    public bool RegisterActionType(string typeName, Type actionType)
    {
        if (!typeof(ActionBase).IsAssignableFrom(actionType))
        {
            throw new ArgumentException($"Type {actionType.Name} must inherit from ActionBase", nameof(actionType));
        }

        return _actionTypes.TryAdd(typeName, actionType);
    }

    /// <summary>
    /// Extracts the display name from the ActionDisplayNameAttribute if present.
    /// </summary>
    /// <param name="actionType">The action type to check</param>
    /// <returns>The display name from the attribute, or null if not found</returns>
    private static string? GetDisplayNameFromAttribute(Type actionType)
    {
        var attribute = actionType.GetCustomAttribute<ActionDisplayNameAttribute>();
        return attribute?.DisplayName;
    }

    /// <summary>
    /// Extracts the category from the ActionCategoryAttribute if present.
    /// </summary>
    /// <param name="actionType">The action type to check</param>
    /// <returns>The category from the attribute, or null if not found</returns>
    private static ActionCategory? GetCategoryFromAttribute(Type actionType)
    {
        var attribute = actionType.GetCustomAttribute<ActionCategoryAttribute>();
        return attribute?.Category;
    }

    /// <summary>
    /// Generates a default display name from the action type name.
    /// Converts "KeyPressReleaseAction" to "Key Press Release".
    /// </summary>
    /// <param name="typeName">The action type name</param>
    /// <returns>A human-readable display name</returns>
    private static string GenerateDefaultDisplayName(string typeName)
    {
        // Remove "Action" suffix if present
        var name = typeName.EndsWith("Action") ? typeName[..^6] : typeName;

        // Insert spaces before capital letters (except the first one)
        var result = string.Empty;
        for (int i = 0; i < name.Length; i++)
        {
            if (i > 0 && char.IsUpper(name[i]) && !char.IsUpper(name[i - 1]))
            {
                result += " ";
            }
            result += name[i];
        }

        return result;
    }

    /// <summary>
    /// Gets the category for the given action type name.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="typeName">The action type name (e.g., "KeyPressReleaseAction")</param>
    /// <returns>The category for the action, or Utility if not found</returns>
    public ActionCategory GetActionCategory(string typeName)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _categories.TryGetValue(typeName, out var category) ? category : ActionCategory.Utility;
    }

    /// <summary>
    /// Gets all action types in the specified category.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <returns>Dictionary of type name to display name mappings for actions in the category</returns>
    public IReadOnlyDictionary<string, string> GetActionsByCategory(ActionCategory category)
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _categories
            .Where(kvp => kvp.Value == category)
            .ToDictionary(kvp => kvp.Key, kvp => _displayNames.TryGetValue(kvp.Key, out var name) ? name : kvp.Key)
            .ToImmutableDictionary();
    }

    /// <summary>
    /// Gets all available categories that have at least one action.
    /// Automatically initializes the registry on first use.
    /// </summary>
    /// <returns>List of categories with at least one action, in enum order</returns>
    public IReadOnlyList<ActionCategory> GetAllCategories()
    {
        if (!_initialized)
        {
            Initialize();
        }

        return _categories.Values
            .Distinct()
            .OrderBy(c => c)
            .ToList()
            .AsReadOnly();
    }

    /// <summary>
    /// Gets a human-readable display name for a category.
    /// </summary>
    /// <param name="category">The category</param>
    /// <returns>A human-readable display name</returns>
    public static string GetCategoryDisplayName(ActionCategory category)
    {
        return category switch
        {
            ActionCategory.Keyboard => "Keyboard",
            ActionCategory.Mouse => "Mouse",
            ActionCategory.GameController => "Game Controller",
            ActionCategory.MidiOutput => "MIDI Output",
            ActionCategory.FlowControl => "Flow Control",
            ActionCategory.State => "State Management",
            ActionCategory.Utility => "Utility",
            _ => category.ToString()
        };
    }

    /// <summary>
    /// Clears all registered action types.
    /// Primarily for testing purposes.
    /// </summary>
    internal void Clear()
    {
        _actionTypes.Clear();
        _displayNames.Clear();
        _categories.Clear();
        _initialized = false;
    }
}
