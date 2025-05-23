using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for selecting MIDI channels
    /// </summary>
    public partial class ChannelPickerDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly CheckBox[] _channelCheckBoxes = new CheckBox[16];
        private readonly CheckBox _allChannelsCheckBox;
        private bool _updatingCheckboxes = false;

        /// <summary>
        /// Gets the selected channels
        /// </summary>
        public List<int>? SelectedChannels { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelPickerDialog"/> class
        /// </summary>
        /// <param name="currentChannels">The currently selected channels</param>
        public ChannelPickerDialog(List<int>? currentChannels)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<ChannelPickerDialog>();
            _logger.LogDebug("Initializing ChannelPickerDialog");

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = "Select MIDI Channels";

            // Set the initial selected channels
            SelectedChannels = currentChannels?.ToList();

            // Create the "All Channels" checkbox
            _allChannelsCheckBox = new CheckBox
            {
                Text = "All Channels",
                AutoSize = true,
                Location = new Point(20, 20),
                Checked = currentChannels == null || currentChannels.Count == 0
            };
            _allChannelsCheckBox.CheckedChanged += AllChannelsCheckBox_CheckedChanged;
            Controls.Add(_allChannelsCheckBox);

            // Create the channel checkboxes
            const int checkboxWidth = 80;
            const int checkboxHeight = 24;
            const int startX = 20;
            const int startY = 50;
            const int columns = 4;

            for (int i = 0; i < 16; i++)
            {
                int channel = i + 1;
                int row = i / columns;
                int col = i % columns;

                _channelCheckBoxes[i] = new CheckBox
                {
                    Text = $"Channel {channel}",
                    AutoSize = true,
                    Location = new Point(startX + col * checkboxWidth, startY + row * checkboxHeight),
                    Checked = currentChannels != null && currentChannels.Contains(channel),
                    Enabled = !_allChannelsCheckBox.Checked
                };
                _channelCheckBoxes[i].CheckedChanged += ChannelCheckBox_CheckedChanged;
                Controls.Add(_channelCheckBoxes[i]);
            }

            // Create OK and Cancel buttons
            var okButton = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(startX + checkboxWidth * 2, startY + 5 * checkboxHeight),
                Size = new Size(80, 30)
            };
            Controls.Add(okButton);

            var cancelButton = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(startX + checkboxWidth * 3, startY + 5 * checkboxHeight),
                Size = new Size(80, 30)
            };
            Controls.Add(cancelButton);

            // Set the dialog size
            ClientSize = new Size(startX * 2 + checkboxWidth * columns, startY + 6 * checkboxHeight);

            // Set the accept button
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the AllChannelsCheckBox
        /// </summary>
        private void AllChannelsCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingCheckboxes)
                return;

            _updatingCheckboxes = true;
            try
            {
                _logger.LogDebug("All Channels checkbox changed: {Checked}", _allChannelsCheckBox.Checked);

                // Enable/disable the channel checkboxes
                foreach (var checkbox in _channelCheckBoxes)
                {
                    checkbox.Enabled = !_allChannelsCheckBox.Checked;
                    if (_allChannelsCheckBox.Checked)
                    {
                        checkbox.Checked = false;
                    }
                }

                // Update the selected channels
                UpdateSelectedChannels();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling All Channels checkbox change");
                ApplicationErrorHandler.ShowError("An error occurred while updating channel selection.", "Error", _logger, ex, this);
            }
            finally
            {
                _updatingCheckboxes = false;
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the ChannelCheckBox
        /// </summary>
        private void ChannelCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingCheckboxes)
                return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingCheckboxes = true;
                    _logger.LogDebug("Channel checkbox changed");

                    // If any channel is checked, uncheck the "All Channels" checkbox
                    if (_channelCheckBoxes.Any(cb => cb.Checked))
                    {
                        _allChannelsCheckBox.Checked = false;
                    }

                    // Update the selected channels
                    UpdateSelectedChannels();
                }
                finally
                {
                    _updatingCheckboxes = false;
                }
            }, _logger, "handling channel checkbox change", this);
        }

        /// <summary>
        /// Updates the SelectedChannels property based on the checkbox states
        /// </summary>
        private void UpdateSelectedChannels()
        {
            if (_allChannelsCheckBox.Checked)
            {
                SelectedChannels = null;
                _logger.LogDebug("Selected channels: All");
            }
            else
            {
                var selectedChannels = new List<int>();
                for (int i = 0; i < 16; i++)
                {
                    if (_channelCheckBoxes[i].Checked)
                    {
                        selectedChannels.Add(i + 1);
                    }
                }

                SelectedChannels = selectedChannels.Count > 0 ? selectedChannels : null;
                _logger.LogDebug("Selected channels: {Channels}", SelectedChannels == null ? "All" : string.Join(", ", SelectedChannels));
            }
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer? components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        }

        #endregion
    }
}

