using System.Runtime.Versioning;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using Microsoft.Extensions.Logging;
using NAudio.CoreAudioApi;

namespace MIDIFlux.Core.Actions.Simple;

/// <summary>
/// Action for setting the Windows system master volume from a MIDI value.
/// Maps MIDI CC values (0-127) directly to volume level (0%-100%).
/// Uses NAudio CoreAudioApi for direct volume control.
/// </summary>
[SupportedOSPlatform("windows")]
[ActionDisplayName("System Volume")]
[ActionCategory(ActionCategory.Utility)]
public class SystemVolumeAction : ActionBase
{
    // No parameters needed - the MIDI value IS the volume level

    /// <summary>
    /// Initializes a new instance of SystemVolumeAction
    /// </summary>
    public SystemVolumeAction() : base()
    {
    }

    /// <summary>
    /// Initializes the parameters for this action type.
    /// SystemVolumeAction has no configurable parameters - it uses the incoming MIDI value directly.
    /// </summary>
    protected override void InitializeParameters()
    {
        // No parameters - MIDI value (0-127) is mapped directly to volume (0.0-1.0)
    }

    /// <summary>
    /// Validates the action configuration
    /// </summary>
    /// <returns>True if valid</returns>
    public override bool IsValid()
    {
        base.IsValid();
        // No parameters to validate - always valid
        return GetValidationErrors().Count == 0;
    }

    /// <summary>
    /// Core execution logic: sets Windows system master volume from MIDI value.
    /// </summary>
    /// <param name="midiValue">MIDI value (0-127) mapped to volume level (0%-100%)</param>
    /// <returns>A ValueTask that completes when the volume is set</returns>
    protected override ValueTask ExecuteAsyncCore(int? midiValue)
    {
        if (!midiValue.HasValue)
        {
            Logger.LogWarning("SystemVolumeAction requires a MIDI value (0-127) but none was provided");
            return ValueTask.CompletedTask;
        }

        // Clamp to valid MIDI range and convert to 0.0-1.0
        var clampedValue = Math.Clamp(midiValue.Value, 0, 127);
        float volumeLevel = clampedValue / 127.0f;

        try
        {
            using var enumerator = new MMDeviceEnumerator();
            using var device = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia);
            device.AudioEndpointVolume.MasterVolumeLevelScalar = volumeLevel;

            Logger.LogDebug("Set system volume to {VolumePercent}% (MIDI value: {MidiValue})",
                (int)(volumeLevel * 100), clampedValue);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to set system volume: {ErrorMessage}", ex.Message);
            ApplicationErrorHandler.ShowWarning(
                $"Failed to set system volume: {ex.Message}",
                "MIDIFlux - System Volume Error", Logger);
        }

        return ValueTask.CompletedTask;
    }

    /// <summary>
    /// Gets the default description for this action type
    /// </summary>
    protected override string GetDefaultDescription()
    {
        return "Set System Volume (from MIDI value)";
    }

    /// <summary>
    /// Gets the error message for this action type
    /// </summary>
    protected override string GetErrorMessage()
    {
        return "Error executing SystemVolumeAction";
    }

    /// <summary>
    /// Gets the input type categories that are compatible with this action.
    /// SystemVolumeAction requires absolute values from faders/knobs (CC 0-127).
    /// </summary>
    /// <returns>Array of compatible input type categories</returns>
    public override InputTypeCategory[] GetCompatibleInputCategories()
    {
        return new[] { InputTypeCategory.AbsoluteValue };
    }
}

