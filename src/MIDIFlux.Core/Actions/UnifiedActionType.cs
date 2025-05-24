namespace MIDIFlux.Core.Actions;

/// <summary>
/// Defines all action types supported by the unified action system.
/// Includes both simple actions (hot path) and complex actions (orchestration).
/// </summary>
public enum UnifiedActionType
{
    // Simple Actions (Hot Path) - Direct execution for performance
    
    /// <summary>
    /// Press and release a key
    /// </summary>
    KeyPressRelease,
    
    /// <summary>
    /// Press and hold a key
    /// </summary>
    KeyDown,
    
    /// <summary>
    /// Release a key
    /// </summary>
    KeyUp,
    
    /// <summary>
    /// Toggle key state (like CapsLock)
    /// </summary>
    KeyToggle,
    
    /// <summary>
    /// Click mouse button (Left/Right/Middle)
    /// </summary>
    MouseClick,
    
    /// <summary>
    /// Scroll wheel (Up/Down/Left/Right)
    /// </summary>
    MouseScroll,
    
    /// <summary>
    /// Execute shell command
    /// </summary>
    CommandExecution,
    
    /// <summary>
    /// Wait for specified time
    /// </summary>
    Delay,
    
    /// <summary>
    /// Press game controller button
    /// </summary>
    GameControllerButton,
    
    /// <summary>
    /// Set game controller axis value
    /// </summary>
    GameControllerAxis,

    // Complex Actions (Orchestration) - Handle logic and sequencing
    
    /// <summary>
    /// Execute actions sequentially (macros)
    /// </summary>
    SequenceAction,
    
    /// <summary>
    /// Execute actions based on MIDI value (fader-to-buttons)
    /// </summary>
    ConditionalAction,

    // Future extensibility (POST-V1.0)
    
    /// <summary>
    /// Transform MIDI value then execute action (POST-V1.0)
    /// </summary>
    ValueTransformAction,
    
    /// <summary>
    /// Send MIDI output (future)
    /// </summary>
    MidiOutput,
    
    /// <summary>
    /// Control audio settings (future)
    /// </summary>
    AudioControl,
    
    /// <summary>
    /// System integration actions (future)
    /// </summary>
    SystemIntegration

    // REMOVED: MouseMove (too complex for V1.0)
    // REMOVED: MouseDown/MouseUp (use MouseClick instead)
}
