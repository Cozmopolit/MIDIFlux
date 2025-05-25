namespace MIDIFlux.Core.Actions;

/// <summary>
/// Factory interface for creating actions from strongly-typed configuration.
/// Provides type-safe action creation with compile-time validation.
/// </summary>
public interface IActionFactory
{
    /// <summary>
    /// Creates a action from strongly-typed configuration.
    /// Uses pattern matching on config types for type-safe creation.
    /// </summary>
    /// <param name="config">The strongly-typed configuration for the action</param>
    /// <returns>The created action instance</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null</exception>
    /// <exception cref="NotSupportedException">Thrown when the config type is not supported</exception>
    /// <exception cref="ArgumentException">Thrown when the config is invalid</exception>
    IAction CreateAction(Configuration.ActionConfig config);
}
