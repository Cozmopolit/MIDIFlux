namespace MIDIFlux.Core.Actions.Configuration;

/// <summary>
/// Configuration for GameControllerButton actions.
/// Represents pressing a game controller button.
/// </summary>
public class GameControllerButtonConfig : ActionConfig
{
    /// <summary>
    /// The button name (A, B, X, Y, etc.)
    /// </summary>
    public string Button { get; set; } = "";

    /// <summary>
    /// The controller index (0-3)
    /// </summary>
    public int ControllerIndex { get; set; } = 0;

    /// <summary>
    /// Initializes a new instance of GameControllerButtonConfig
    /// </summary>
    public GameControllerButtonConfig()
    {
        Type = ActionType.GameControllerButton;
    }

    /// <summary>
    /// Validates the configuration parameters
    /// </summary>
    public override bool IsValid()
    {
        base.IsValid(); // Clear previous errors

        if (string.IsNullOrWhiteSpace(Button))
        {
            AddValidationError("Button name cannot be empty or whitespace");
        }

        if (ControllerIndex < 0 || ControllerIndex > 3)
        {
            AddValidationError("ControllerIndex must be between 0 and 3");
        }

        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Returns a human-readable string representation
    /// </summary>
    public override string ToString()
    {
        if (!string.IsNullOrEmpty(Description))
            return Description;

        return $"Controller {ControllerIndex + 1} Button {Button}";
    }
}
