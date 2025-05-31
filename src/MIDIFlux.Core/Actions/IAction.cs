using System.Threading.Tasks;
using MIDIFlux.Core.Actions.Parameters;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Interface for all actions in the MIDIFlux system.
/// Provides async-first execution for consistent behavior and maintainable architecture.
/// </summary>
public interface IAction
{
    /// <summary>
    /// Gets a human-readable description of this action for UI and logging
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// JSON-serializable parameters dictionary that handles both simple values and SubActionList
    /// </summary>
    Dictionary<string, object?> JsonParameters { get; set; }

    /// <summary>
    /// Executes the action asynchronously.
    /// All actions implement this execution model for consistent behavior.
    /// </summary>
    /// <param name="midiValue">Optional MIDI value (0-127) that triggered this action</param>
    /// <returns>A ValueTask that completes when the action is finished</returns>
    ValueTask ExecuteAsync(int? midiValue = null);

    /// <summary>
    /// Gets a list of parameter information for UI generation
    /// </summary>
    /// <returns>List of parameter metadata</returns>
    List<ParameterInfo> GetParameterList();

    /// <summary>
    /// Validates the action configuration and parameters
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    bool IsValid();

    /// <summary>
    /// Gets the validation errors from the last IsValid() call
    /// </summary>
    /// <returns>List of validation error messages</returns>
    List<string> GetValidationErrors();

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// Used by the GUI to filter available actions based on the selected input type.
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    InputTypeCategory[] GetCompatibleInputCategories();
}
