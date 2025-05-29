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
    /// Clears all registered action types.
    /// Primarily for testing purposes.
    /// </summary>
    internal void Clear()
    {
        _actionTypes.Clear();
        _initialized = false;
    }
}
