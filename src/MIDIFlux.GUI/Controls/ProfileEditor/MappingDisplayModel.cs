using MIDIFlux.Core.Actions;

namespace MIDIFlux.GUI.Controls.ProfileEditor;

/// <summary>
/// Display model for ActionMapping that provides flat properties for DataGridView binding.
/// This wrapper enables proper display of nested properties in Windows Forms DataGridView.
/// </summary>
public class MappingDisplayModel
{
    private readonly ActionMapping _mapping;

    public MappingDisplayModel(ActionMapping mapping)
    {
        _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
    }

    /// <summary>
    /// Gets the underlying mapping object
    /// </summary>
    public ActionMapping Mapping => _mapping;

    /// <summary>
    /// MIDI input type (NoteOn, NoteOff, ControlChange, etc.)
    /// </summary>
    public string Type => _mapping.Input.InputType.ToString();

    /// <summary>
    /// MIDI input number (note number, CC number, etc.)
    /// </summary>
    public int Trigger => _mapping.Input.InputNumber;

    /// <summary>
    /// MIDI channel (1-16, or null for any channel)
    /// </summary>
    public string Channel => _mapping.Input.Channel?.ToString() ?? "Any";

    /// <summary>
    /// MIDI device name (or "Any Device" for null/empty)
    /// </summary>
    public string Device => string.IsNullOrEmpty(_mapping.Input.DeviceName) ? "Any Device" : _mapping.Input.DeviceName;

    /// <summary>
    /// Action type description
    /// </summary>
    public string ActionType => _mapping.Action?.GetType().Name.Replace("Action", "") ?? "Unknown";

    /// <summary>
    /// Action details/description
    /// </summary>
    public string ActionDetails => _mapping.Action?.Description ?? "No description";

    /// <summary>
    /// Mapping description
    /// </summary>
    public string Description => _mapping.Description ?? string.Empty;

    /// <summary>
    /// Whether this mapping is enabled
    /// </summary>
    public bool Enabled => _mapping.IsEnabled;

    /// <summary>
    /// String representation for display
    /// </summary>
    public override string ToString()
    {
        return _mapping.ToString();
    }
}
