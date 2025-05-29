namespace MIDIFlux.Core.Actions;

/// <summary>
/// Represents a mapping between MIDI input and a action.
/// This is the core data structure that connects MIDI events to actions.
/// Optimized for fast lookup performance in the mapping registry.
/// </summary>
public class ActionMapping
{
    /// <summary>
    /// MIDI input specification (WHEN to trigger)
    /// </summary>
    public MidiInput Input { get; set; } = new();

    /// <summary>
    /// Action to execute (WHAT to execute - simple or complex)
    /// </summary>
    public IAction Action { get; set; } = null!;

    /// <summary>
    /// Optional human-readable description of this mapping
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this mapping is currently enabled
    /// Optional flag that can be used to temporarily disable mappings
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Computed lookup key for O(1) registry performance.
    /// Delegates to the Input's GetLookupKey method for consistency.
    /// </summary>
    /// <returns>A string key optimized for dictionary lookup</returns>
    public string GetLookupKey()
    {
        return Input.GetLookupKey();
    }

    /// <summary>
    /// Creates a copy of this mapping with a new action
    /// </summary>
    /// <param name="newAction">The new action to use</param>
    /// <returns>A new mapping instance</returns>
    public ActionMapping WithAction(IAction newAction)
    {
        return new ActionMapping
        {
            Input = Input.Clone(),
            Action = newAction,
            Description = Description,
            IsEnabled = IsEnabled
        };
    }

    /// <summary>
    /// Returns a human-readable string representation of this mapping
    /// </summary>
    public override string ToString()
    {
        var status = IsEnabled ? "Enabled" : "Disabled";
        var description = string.IsNullOrEmpty(Description) ? Action.Description : Description;
        return $"[{status}] {Input} → {description}";
    }
}
