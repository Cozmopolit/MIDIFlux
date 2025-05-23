using System;
using System.Windows.Forms;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Dialogs;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Helpers
{
    /// <summary>
    /// Helper class for MIDI input detection
    /// </summary>
    public static class MidiInputDetectionHelper
    {
        /// <summary>
        /// Shows the MIDI input detection dialog and returns the selected MIDI event
        /// </summary>
        /// <param name="logger">The logger to use</param>
        /// <param name="midiManager">The MIDI manager</param>
        /// <param name="parent">The parent form</param>
        /// <returns>The MIDI event args if a MIDI event was selected, null otherwise</returns>
        public static (MidiEvent? Event, int DeviceId, string DeviceName) DetectMidiInput(
            ILogger logger,
            MidiManager midiManager,
            IWin32Window? parent = null)
        {
            try
            {
                // Create the dialog
                using var dialog = new MidiInputDetectionDialog(logger, midiManager);

                // Show the dialog
                DialogResult result = parent != null
                    ? dialog.ShowDialog(parent)
                    : dialog.ShowDialog();

                // Check if a MIDI event was selected
                if (result == DialogResult.OK && dialog.SelectedMidiEvent != null)
                {
                    return (dialog.SelectedMidiEvent, dialog.SelectedDeviceId, dialog.SelectedDeviceName);
                }

                // No MIDI event was selected
                return (null, -1, string.Empty);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error showing MIDI input detection dialog");
                return (null, -1, string.Empty);
            }
        }
    }
}
