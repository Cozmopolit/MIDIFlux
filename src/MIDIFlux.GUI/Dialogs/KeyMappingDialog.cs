using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.GUI.Helpers;
using Simple = MIDIFlux.Core.Actions.Simple;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for creating and editing unified key action mappings.
    /// Replaces the legacy KeyMappingDialog with action system support.
    /// </summary>
    public partial class KeyMappingDialog : ActionMappingDialog
    {
        // Key action parameter controls
        private ComboBox? _keyComboBox;
        private NumericUpDown? _autoReleaseNumericUpDown;
        private CheckBox? _autoReleaseCheckBox;

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMappingDialog"/> class for creating a new key mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public KeyMappingDialog(MidiManager? midiManager = null) : base(midiManager)
        {
            Text = "Add Key Mapping";
            SetupKeyActionDefaults();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="KeyMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public KeyMappingDialog(ActionMapping mapping, MidiManager? midiManager = null)
            : base(mapping, midiManager)
        {
            Text = "Edit Key Mapping";
        }

        /// <summary>
        /// Sets up default key action for new mappings
        /// </summary>
        private void SetupKeyActionDefaults()
        {
            // Ensure we start with a key action
            if (_mapping.Action is not Simple.KeyPressReleaseAction and not Simple.KeyDownAction and not Simple.KeyUpAction and not Simple.KeyToggleAction)
            {
                var config = new KeyPressReleaseConfig { VirtualKeyCode = 65 }; // 'A' key
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(config);
            }
        }

        /// <summary>
        /// Populates the action type combo box with key-specific action types from the registry
        /// </summary>
        protected override void PopulateActionTypeComboBox()
        {
            actionTypeComboBox.Items.Clear();

            // Get only keyboard action descriptors from the registry
            var keyboardDescriptors = ActionRegistry.GetByCategory(ActionCategory.Keyboard).ToArray();
            actionTypeComboBox.Items.AddRange(keyboardDescriptors.Cast<object>().ToArray());

            // Set display member to show the display name
            actionTypeComboBox.DisplayMember = nameof(ActionDescriptor.DisplayName);
        }

        /// <summary>
        /// Loads action-specific parameters into the UI
        /// </summary>
        protected override void LoadActionParameters()
        {
            if (actionParametersPanel == null)
                return;

            // Clear existing controls
            actionParametersPanel.Controls.Clear();

            // Create parameter controls based on action type
            CreateKeyParameterControls();

            // Load current values
            LoadKeyParameterValues();
        }

        /// <summary>
        /// Creates the parameter controls for key actions
        /// </summary>
        private void CreateKeyParameterControls()
        {
            if (actionParametersPanel == null)
                return;

            var tableLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true
            };

            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
            tableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));
            tableLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 25F));

            // Key selection
            var keyLabel = new Label
            {
                Text = "Key:",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            tableLayout.Controls.Add(keyLabel, 0, 0);

            _keyComboBox = new ComboBox
            {
                Dock = DockStyle.Fill,
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            PopulateKeyComboBox();
            _keyComboBox.SelectedIndexChanged += KeyComboBox_SelectedIndexChanged;
            tableLayout.Controls.Add(_keyComboBox, 1, 0);

            // Auto-release checkbox (for KeyDown actions)
            _autoReleaseCheckBox = new CheckBox
            {
                Text = "Auto-release after (ms):",
                Anchor = AnchorStyles.Left,
                AutoSize = true
            };
            _autoReleaseCheckBox.CheckedChanged += AutoReleaseCheckBox_CheckedChanged;
            tableLayout.Controls.Add(_autoReleaseCheckBox, 0, 1);

            // Auto-release time
            _autoReleaseNumericUpDown = new NumericUpDown
            {
                Dock = DockStyle.Fill,
                Minimum = 1,
                Maximum = 10000,
                Value = 100,
                Enabled = false
            };
            _autoReleaseNumericUpDown.ValueChanged += AutoReleaseNumericUpDown_ValueChanged;
            tableLayout.Controls.Add(_autoReleaseNumericUpDown, 1, 1);

            actionParametersPanel.Controls.Add(tableLayout);
        }

        /// <summary>
        /// Populates the key combo box with available virtual key codes
        /// </summary>
        private void PopulateKeyComboBox()
        {
            if (_keyComboBox == null)
                return;

            _keyComboBox.Items.Clear();

            // Add common keys with their display names
            var keys = new Dictionary<string, ushort>
            {
                { "A", 65 }, { "B", 66 }, { "C", 67 }, { "D", 68 }, { "E", 69 }, { "F", 70 },
                { "G", 71 }, { "H", 72 }, { "I", 73 }, { "J", 74 }, { "K", 75 }, { "L", 76 },
                { "M", 77 }, { "N", 78 }, { "O", 79 }, { "P", 80 }, { "Q", 81 }, { "R", 82 },
                { "S", 83 }, { "T", 84 }, { "U", 85 }, { "V", 86 }, { "W", 87 }, { "X", 88 },
                { "Y", 89 }, { "Z", 90 },
                { "0", 48 }, { "1", 49 }, { "2", 50 }, { "3", 51 }, { "4", 52 },
                { "5", 53 }, { "6", 54 }, { "7", 55 }, { "8", 56 }, { "9", 57 },
                { "Space", 32 }, { "Enter", 13 }, { "Tab", 9 }, { "Escape", 27 },
                { "Shift", 16 }, { "Ctrl", 17 }, { "Alt", 18 },
                { "F1", 112 }, { "F2", 113 }, { "F3", 114 }, { "F4", 115 },
                { "F5", 116 }, { "F6", 117 }, { "F7", 118 }, { "F8", 119 },
                { "F9", 120 }, { "F10", 121 }, { "F11", 122 }, { "F12", 123 },
                { "Left Arrow", 37 }, { "Up Arrow", 38 }, { "Right Arrow", 39 }, { "Down Arrow", 40 },
                { "Page Up", 33 }, { "Page Down", 34 }, { "Home", 36 }, { "End", 35 },
                { "Insert", 45 }, { "Delete", 46 }, { "Backspace", 8 }
            };

            foreach (var key in keys)
            {
                _keyComboBox.Items.Add(new KeyItem(key.Key, key.Value));
            }
        }

        /// <summary>
        /// Loads current key parameter values into the controls
        /// </summary>
        private void LoadKeyParameterValues()
        {
            if (_keyComboBox == null || _autoReleaseCheckBox == null || _autoReleaseNumericUpDown == null)
                return;

            try
            {
                _updatingUI = true;

                // Load key value
                ushort keyCode = _mapping.Action switch
                {
                    Simple.KeyPressReleaseAction kpra => GetKeyCodeFromAction(kpra),
                    Simple.KeyDownAction kda => GetKeyCodeFromAction(kda),
                    Simple.KeyUpAction kua => GetKeyCodeFromAction(kua),
                    Simple.KeyToggleAction kta => GetKeyCodeFromAction(kta),
                    _ => 65 // Default to 'A'
                };

                // Select the key in the combo box
                foreach (KeyItem item in _keyComboBox.Items)
                {
                    if (item.VirtualKeyCode == keyCode)
                    {
                        _keyComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Load auto-release settings (only for KeyDown actions)
                if (_mapping.Action is Simple.KeyDownAction keyDownAction)
                {
                    var autoReleaseMs = GetAutoReleaseFromAction(keyDownAction);
                    _autoReleaseCheckBox.Checked = autoReleaseMs.HasValue;
                    _autoReleaseNumericUpDown.Enabled = autoReleaseMs.HasValue;
                    if (autoReleaseMs.HasValue)
                    {
                        _autoReleaseNumericUpDown.Value = Math.Max(1, Math.Min(10000, autoReleaseMs.Value));
                    }
                }
                else
                {
                    _autoReleaseCheckBox.Checked = false;
                    _autoReleaseNumericUpDown.Enabled = false;
                }

                // Show/hide auto-release controls based on action type
                var showAutoRelease = _mapping.Action is Simple.KeyDownAction;
                _autoReleaseCheckBox.Visible = showAutoRelease;
                _autoReleaseNumericUpDown.Visible = showAutoRelease;
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Gets the virtual key code from a key action using reflection
        /// </summary>
        private ushort GetKeyCodeFromAction(IAction action)
        {
            try
            {
                var property = action.GetType().GetProperty("VirtualKeyCode");
                if (property != null && property.GetValue(action) is ushort keyCode)
                {
                    return keyCode;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get key code from action");
            }
            return 65; // Default to 'A'
        }

        /// <summary>
        /// Gets the auto-release time from a KeyDown action using reflection
        /// </summary>
        private int? GetAutoReleaseFromAction(Simple.KeyDownAction action)
        {
            try
            {
                var property = action.GetType().GetProperty("AutoReleaseAfterMs");
                if (property != null)
                {
                    var value = property.GetValue(action);
                    if (value is int autoRelease)
                    {
                        return autoRelease;
                    }
                    return value as int?;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get auto-release from action");
            }
            return null;
        }

        /// <summary>
        /// Saves action configuration from the UI
        /// </summary>
        protected override bool SaveActionData()
        {
            try
            {
                if (_keyComboBox?.SelectedItem is not KeyItem selectedKey)
                {
                    ApplicationErrorHandler.ShowError("Please select a key.", "Validation Error", _logger, null, this);
                    return false;
                }

                // Create the appropriate configuration based on current action type
                ActionConfig config = _mapping.Action switch
                {
                    Simple.KeyPressReleaseAction => new KeyPressReleaseConfig { VirtualKeyCode = selectedKey.VirtualKeyCode },
                    Simple.KeyDownAction => new KeyDownConfig
                    {
                        VirtualKeyCode = selectedKey.VirtualKeyCode,
                        AutoReleaseAfterMs = _autoReleaseCheckBox?.Checked == true ? (int?)_autoReleaseNumericUpDown?.Value : null
                    },
                    Simple.KeyUpAction => new KeyUpConfig { VirtualKeyCode = selectedKey.VirtualKeyCode },
                    Simple.KeyToggleAction => new KeyToggleConfig { VirtualKeyCode = selectedKey.VirtualKeyCode },
                    _ => new KeyPressReleaseConfig { VirtualKeyCode = selectedKey.VirtualKeyCode }
                };

                // Create new action with updated configuration
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(config);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving key action data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the key action data.", "Error", _logger, ex, this);
                return false;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the KeyComboBox
        /// </summary>
        private void KeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // The actual saving will happen when the dialog is saved
                // This just provides immediate feedback
            }, _logger, "changing key selection", this);
        }

        /// <summary>
        /// Handles the CheckedChanged event of the AutoReleaseCheckBox
        /// </summary>
        private void AutoReleaseCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (_autoReleaseNumericUpDown != null)
                {
                    _autoReleaseNumericUpDown.Enabled = _autoReleaseCheckBox?.Checked == true;
                }
            }, _logger, "changing auto-release setting", this);
        }

        /// <summary>
        /// Handles the ValueChanged event of the AutoReleaseNumericUpDown
        /// </summary>
        private void AutoReleaseNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // The actual saving will happen when the dialog is saved
                // This just provides immediate feedback
            }, _logger, "changing auto-release time", this);
        }

        /// <summary>
        /// Handles action type changes to update parameter controls
        /// </summary>
        protected override void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a new action based on the selected descriptor
                if (actionTypeComboBox.SelectedItem is ActionDescriptor descriptor)
                {
                    CreateKeyActionFromDescriptor(descriptor);
                    LoadActionParameters();
                }
            }, _logger, "changing key action type", this);
        }

        /// <summary>
        /// Creates a new key action instance based on the selected descriptor
        /// </summary>
        private void CreateKeyActionFromDescriptor(ActionDescriptor descriptor)
        {
            // Get current key code if available
            ushort currentKeyCode = 65; // Default to 'A'
            if (_keyComboBox?.SelectedItem is KeyItem currentKey)
            {
                currentKeyCode = currentKey.VirtualKeyCode;
            }

            // Create the default config from the descriptor and update the key code
            var config = descriptor.CreateDefaultConfig();

            // Update the virtual key code for all keyboard action configs
            switch (config)
            {
                case KeyPressReleaseConfig kprc:
                    kprc.VirtualKeyCode = currentKeyCode;
                    break;
                case KeyDownConfig kdc:
                    kdc.VirtualKeyCode = currentKeyCode;
                    break;
                case KeyUpConfig kuc:
                    kuc.VirtualKeyCode = currentKeyCode;
                    break;
                case KeyToggleConfig ktc:
                    ktc.VirtualKeyCode = currentKeyCode;
                    break;
            }

            // Create the action using the factory
            var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
            var factory = new ActionFactory(factoryLogger);
            _mapping.Action = factory.CreateAction(config);
        }

        #endregion
    }

    /// <summary>
    /// Helper class to represent a key item in the combo box
    /// </summary>
    public class KeyItem
    {
        public string DisplayName { get; }
        public ushort VirtualKeyCode { get; }

        public KeyItem(string displayName, ushort virtualKeyCode)
        {
            DisplayName = displayName;
            VirtualKeyCode = virtualKeyCode;
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
