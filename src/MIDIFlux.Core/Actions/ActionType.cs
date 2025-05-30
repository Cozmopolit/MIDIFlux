namespace MIDIFlux.Core.Actions;

/// <summary>
/// DEPRECATED: This enum is no longer used in the action system.
/// Action types are now discovered automatically using reflection via ActionTypeRegistry.
/// This file is kept temporarily for backward compatibility but will be removed in a future version.
/// </summary>
[Obsolete("Use ActionTypeRegistry for action type discovery instead of this enum")]
public enum ActionType
{
    // This enum is deprecated and should not be used
    // Use ActionTypeRegistry.Instance.GetAllActionTypes() instead
    [Obsolete("Use ActionTypeRegistry instead")]
    Deprecated = 0
}
