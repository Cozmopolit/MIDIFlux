using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing a macro action
    /// </summary>
    public partial class MacroActionDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly MacroActionDefinition _action;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the edited macro action
        /// </summary>
        public MacroActionDefinition Action => _action;

        /// <summary>
        /// Initializes a new instance of the <see cref="MacroActionDialog"/> class
        /// </summary>
        /// <param name="action">The macro action to edit</param>
        public MacroActionDialog(MacroActionDefinition action)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<MacroActionDialog>();
            _logger.LogDebug("Initializing MacroActionDialog");

            // Store the action
            _action = action;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = "Edit Action";

            // Set up event handlers
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;
            keyboardActionTypeComboBox.SelectedIndexChanged += KeyboardActionTypeComboBox_SelectedIndexChanged;
            virtualKeyComboBox.SelectedIndexChanged += VirtualKeyComboBox_SelectedIndexChanged;
            shiftCheckBox.CheckedChanged += ModifierCheckBox_CheckedChanged;
            ctrlCheckBox.CheckedChanged += ModifierCheckBox_CheckedChanged;
            altCheckBox.CheckedChanged += ModifierCheckBox_CheckedChanged;
            winCheckBox.CheckedChanged += ModifierCheckBox_CheckedChanged;
            shellTypeComboBox.SelectedIndexChanged += ShellTypeComboBox_SelectedIndexChanged;
            runHiddenCheckBox.CheckedChanged += RunHiddenCheckBox_CheckedChanged;
            waitForExitCheckBox.CheckedChanged += WaitForExitCheckBox_CheckedChanged;
            commandTextBox.TextChanged += CommandTextBox_TextChanged;
            millisecondsNumericUpDown.ValueChanged += MillisecondsNumericUpDown_ValueChanged;


            delayAfterNumericUpDown.ValueChanged += DelayAfterNumericUpDown_ValueChanged;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;

            // Populate the action type combo box
            PopulateActionTypeComboBox();

            // Populate the keyboard action type combo box
            PopulateKeyboardActionTypeComboBox();

            // Populate the virtual key combo box
            PopulateVirtualKeyComboBox();

            // Populate the shell type combo box
            PopulateShellTypeComboBox();





            // Load the action data
            LoadActionData();
        }

        /// <summary>
        /// Populates the action type combo box
        /// </summary>
        private void PopulateActionTypeComboBox()
        {
            actionTypeComboBox.Items.Clear();
            actionTypeComboBox.Items.Add("Keyboard Action");
            actionTypeComboBox.Items.Add("Command Execution");
            actionTypeComboBox.Items.Add("Delay");
        }

        /// <summary>
        /// Populates the keyboard action type combo box
        /// </summary>
        private void PopulateKeyboardActionTypeComboBox()
        {
            keyboardActionTypeComboBox.Items.Clear();
            keyboardActionTypeComboBox.Items.Add("Press and Release");
            keyboardActionTypeComboBox.Items.Add("Press Down");
            keyboardActionTypeComboBox.Items.Add("Release Up");
            keyboardActionTypeComboBox.Items.Add("Toggle");
        }

        /// <summary>
        /// Populates the virtual key combo box
        /// </summary>
        private void PopulateVirtualKeyComboBox()
        {
            virtualKeyComboBox.Items.Clear();

            // Add common keys
            for (char c = 'A'; c <= 'Z'; c++)
            {
                virtualKeyComboBox.Items.Add(c.ToString());
            }

            for (int i = 0; i <= 9; i++)
            {
                virtualKeyComboBox.Items.Add(i.ToString());
            }

            // Add function keys
            for (int i = 1; i <= 12; i++)
            {
                virtualKeyComboBox.Items.Add($"F{i}");
            }

            // Add special keys
            virtualKeyComboBox.Items.Add("Enter");
            virtualKeyComboBox.Items.Add("Escape");
            virtualKeyComboBox.Items.Add("Tab");
            virtualKeyComboBox.Items.Add("Space");
            virtualKeyComboBox.Items.Add("Backspace");
            virtualKeyComboBox.Items.Add("Delete");
            virtualKeyComboBox.Items.Add("Insert");
            virtualKeyComboBox.Items.Add("Home");
            virtualKeyComboBox.Items.Add("End");
            virtualKeyComboBox.Items.Add("Page Up");
            virtualKeyComboBox.Items.Add("Page Down");
            virtualKeyComboBox.Items.Add("Left");
            virtualKeyComboBox.Items.Add("Right");
            virtualKeyComboBox.Items.Add("Up");
            virtualKeyComboBox.Items.Add("Down");
        }

        /// <summary>
        /// Populates the shell type combo box
        /// </summary>
        private void PopulateShellTypeComboBox()
        {
            shellTypeComboBox.Items.Clear();
            shellTypeComboBox.Items.Add("PowerShell");
            shellTypeComboBox.Items.Add("Command Prompt");
        }





        /// <summary>
        /// Loads the action data into the UI
        /// </summary>
        private void LoadActionData()
        {
            try
            {
                _updatingUI = true;

                // Set the action type
                switch (_action.Type)
                {
                    case ActionType.KeyPressRelease:
                    case ActionType.KeyDown:
                    case ActionType.KeyUp:
                    case ActionType.KeyToggle:
                        actionTypeComboBox.SelectedIndex = 0; // Keyboard Action
                        break;
                    case ActionType.CommandExecution:
                        actionTypeComboBox.SelectedIndex = 1; // Command Execution
                        break;
                    case ActionType.Delay:
                        actionTypeComboBox.SelectedIndex = 2; // Delay
                        break;

                    case ActionType.MouseMove:
                    case ActionType.MouseDown:
                    case ActionType.MouseUp:
                    case ActionType.MouseClick:
                    case ActionType.MouseScroll:
                        // Default to keyboard action for unsupported mouse actions
                        actionTypeComboBox.SelectedIndex = 0;
                        break;
                    default:
                        actionTypeComboBox.SelectedIndex = 0; // Default to Keyboard Action
                        break;
                }

                // Set the keyboard action type
                switch (_action.Type)
                {
                    case ActionType.KeyPressRelease:
                        keyboardActionTypeComboBox.SelectedIndex = 0; // Press and Release
                        break;
                    case ActionType.KeyDown:
                        keyboardActionTypeComboBox.SelectedIndex = 1; // Press Down
                        break;
                    case ActionType.KeyUp:
                        keyboardActionTypeComboBox.SelectedIndex = 2; // Release Up
                        break;
                    case ActionType.KeyToggle:
                        keyboardActionTypeComboBox.SelectedIndex = 3; // Toggle
                        break;
                    default:
                        keyboardActionTypeComboBox.SelectedIndex = 0; // Default to Press and Release
                        break;
                }

                // Set the virtual key
                if (_action.VirtualKeyCode.HasValue)
                {
                    string keyName = GetKeyName(_action.VirtualKeyCode.Value);
                    if (virtualKeyComboBox.Items.Contains(keyName))
                    {
                        virtualKeyComboBox.SelectedItem = keyName;
                    }
                }

                // Set the modifiers
                if (_action.Modifiers != null)
                {
                    foreach (var modifier in _action.Modifiers)
                    {
                        if (modifier == 16) // Shift
                        {
                            shiftCheckBox.Checked = true;
                        }
                        else if (modifier == 17) // Ctrl
                        {
                            ctrlCheckBox.Checked = true;
                        }
                        else if (modifier == 18) // Alt
                        {
                            altCheckBox.Checked = true;
                        }
                        else if (modifier == 91) // Win
                        {
                            winCheckBox.Checked = true;
                        }
                    }
                }

                // Set the command
                commandTextBox.Text = _action.Command ?? string.Empty;

                // Set the shell type
                if (_action.ShellType.HasValue)
                {
                    shellTypeComboBox.SelectedIndex = (int)_action.ShellType.Value;
                }
                else
                {
                    shellTypeComboBox.SelectedIndex = 0; // Default to PowerShell
                }

                // Set the run hidden flag
                runHiddenCheckBox.Checked = _action.RunHidden;

                // Set the wait for exit flag
                waitForExitCheckBox.Checked = _action.WaitForExit;

                // Set the milliseconds
                if (_action.Milliseconds.HasValue)
                {
                    millisecondsNumericUpDown.Value = _action.Milliseconds.Value;
                }
                else
                {
                    millisecondsNumericUpDown.Value = 1000; // Default to 1 second
                }



                // Mouse-related code removed as it's not supported

                // Set the delay after
                delayAfterNumericUpDown.Value = _action.DelayAfter;

                // Set the description
                descriptionTextBox.Text = _action.Description ?? string.Empty;

                // Update the UI based on the selected action type
                UpdateUIForActionType();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading macro action data");
                ApplicationErrorHandler.ShowError($"An error occurred while loading the action data: {ex.Message}", "MIDIFlux - Error", _logger, ex, this);
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Updates the UI based on the selected action type
        /// </summary>
        private void UpdateUIForActionType()
        {
            if (_updatingUI)
                return;

            try
            {
                _updatingUI = true;

                // Hide all panels
                keyboardPanel.Visible = false;
                commandPanel.Visible = false;
                delayPanel.Visible = false;

                // Show the appropriate panel based on the selected action type
                switch (actionTypeComboBox.SelectedIndex)
                {
                    case 0: // Keyboard Action
                        keyboardPanel.Visible = true;
                        break;
                    case 1: // Command Execution
                        commandPanel.Visible = true;
                        break;
                    case 2: // Delay
                        delayPanel.Visible = true;
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating UI for action type");
                ApplicationErrorHandler.ShowError($"An error occurred while updating the UI: {ex.Message}", "MIDIFlux - Error", _logger, ex, this);
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Saves the action data from the UI
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool SaveActionData()
        {
            try
            {
                // Set the action type based on the selected action type
                switch (actionTypeComboBox.SelectedIndex)
                {
                    case 0: // Keyboard Action
                        switch (keyboardActionTypeComboBox.SelectedIndex)
                        {
                            case 0: // Press and Release
                                _action.Type = ActionType.KeyPressRelease;
                                break;
                            case 1: // Press Down
                                _action.Type = ActionType.KeyDown;
                                break;
                            case 2: // Release Up
                                _action.Type = ActionType.KeyUp;
                                break;
                            case 3: // Toggle
                                _action.Type = ActionType.KeyToggle;
                                break;
                        }
                        break;
                    case 1: // Command Execution
                        _action.Type = ActionType.CommandExecution;
                        break;
                    case 2: // Delay
                        _action.Type = ActionType.Delay;
                        break;

                    // Mouse-related code removed as it's not supported
                }

                // Set the virtual key code
                if (actionTypeComboBox.SelectedIndex == 0 && virtualKeyComboBox.SelectedItem != null)
                {
                    string keyName = virtualKeyComboBox.SelectedItem.ToString() ?? string.Empty;
                    _action.VirtualKeyCode = (ushort?)GetVirtualKeyCode(keyName);
                }
                else
                {
                    _action.VirtualKeyCode = null;
                }

                // Set the modifiers
                if (actionTypeComboBox.SelectedIndex == 0)
                {
                    _action.Modifiers = new List<ushort>();
                    if (shiftCheckBox.Checked)
                    {
                        _action.Modifiers.Add(16); // Shift
                    }
                    if (ctrlCheckBox.Checked)
                    {
                        _action.Modifiers.Add(17); // Ctrl
                    }
                    if (altCheckBox.Checked)
                    {
                        _action.Modifiers.Add(18); // Alt
                    }
                    if (winCheckBox.Checked)
                    {
                        _action.Modifiers.Add(91); // Win
                    }
                }
                else
                {
                    _action.Modifiers = null;
                }

                // Set the command
                if (actionTypeComboBox.SelectedIndex == 1)
                {
                    _action.Command = commandTextBox.Text.Trim();
                    _action.ShellType = (CommandShellType)shellTypeComboBox.SelectedIndex;
                    _action.RunHidden = runHiddenCheckBox.Checked;
                    _action.WaitForExit = waitForExitCheckBox.Checked;
                }
                else
                {
                    _action.Command = null;
                    _action.ShellType = null;
                    _action.RunHidden = false;
                    _action.WaitForExit = false;
                }

                // Set the milliseconds
                if (actionTypeComboBox.SelectedIndex == 2)
                {
                    _action.Milliseconds = (int)millisecondsNumericUpDown.Value;
                }
                else
                {
                    _action.Milliseconds = null;
                }

                // Clear the macro ID as we don't support nested macros
                _action.MacroId = null;

                // Clear mouse-related properties as we don't support them in the UI
                _action.MouseX = null;
                _action.MouseY = null;
                _action.MouseButton = null;

                // Set the delay after
                _action.DelayAfter = (int)delayAfterNumericUpDown.Value;

                // Set the description
                _action.Description = descriptionTextBox.Text.Trim();
                if (string.IsNullOrEmpty(_action.Description))
                    _action.Description = null;

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving macro action data");
                ApplicationErrorHandler.ShowError($"An error occurred while saving the action data: {ex.Message}", "MIDIFlux - Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Validates the action data
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateActionData()
        {
            // Validate based on the selected action type
            switch (actionTypeComboBox.SelectedIndex)
            {
                case 0: // Keyboard Action
                    if (virtualKeyComboBox.SelectedItem == null)
                    {
                        ApplicationErrorHandler.ShowError("Please select a key.", "MIDIFlux - Validation Error", _logger, null, this);
                        return false;
                    }
                    break;
                case 1: // Command Execution
                    if (string.IsNullOrWhiteSpace(commandTextBox.Text))
                    {
                        ApplicationErrorHandler.ShowError("Please enter a command.", "MIDIFlux - Validation Error", _logger, null, this);
                        return false;
                    }
                    break;

            }

            return true;
        }

        /// <summary>
        /// Gets the name of a virtual key code
        /// </summary>
        /// <param name="virtualKeyCode">The virtual key code</param>
        /// <returns>The key name</returns>
        private string GetKeyName(int virtualKeyCode)
        {
            try
            {
                // Get the key name from the Windows API
                var keyName = System.Windows.Forms.Keys.GetName(typeof(System.Windows.Forms.Keys), virtualKeyCode);
                return keyName ?? $"Key {virtualKeyCode}";
            }
            catch
            {
                return $"Key {virtualKeyCode}";
            }
        }

        /// <summary>
        /// Gets the virtual key code for a key name
        /// </summary>
        /// <param name="keyName">The key name</param>
        /// <returns>The virtual key code</returns>
        private int GetVirtualKeyCode(string keyName)
        {
            try
            {
                // Handle special cases
                if (string.IsNullOrEmpty(keyName))
                {
                    return 0;
                }

                // Try to parse the key name as a Keys enum value
                if (Enum.TryParse<System.Windows.Forms.Keys>(keyName, true, out var key))
                {
                    return (int)key;
                }

                // Handle single letters and numbers
                if (keyName.Length == 1)
                {
                    char c = keyName[0];
                    if (c >= 'A' && c <= 'Z')
                    {
                        return (int)c;
                    }
                    if (c >= '0' && c <= '9')
                    {
                        return (int)c;
                    }
                }

                // Handle function keys
                if (keyName.StartsWith("F") && keyName.Length > 1 && int.TryParse(keyName.Substring(1), out int fKey) && fKey >= 1 && fKey <= 24)
                {
                    return 111 + fKey; // F1 is 112, F2 is 113, etc.
                }

                // Handle special keys
                return keyName.ToLower() switch
                {
                    "enter" => 13,
                    "escape" => 27,
                    "tab" => 9,
                    "space" => 32,
                    "backspace" => 8,
                    "delete" => 46,
                    "insert" => 45,
                    "home" => 36,
                    "end" => 35,
                    "page up" => 33,
                    "page down" => 34,
                    "left" => 37,
                    "right" => 39,
                    "up" => 38,
                    "down" => 40,
                    _ => 0
                };
            }
            catch
            {
                return 0;
            }
        }

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ActionTypeComboBox
        /// </summary>
        private void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIForActionType();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the KeyboardActionTypeComboBox
        /// </summary>
        private void KeyboardActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the VirtualKeyComboBox
        /// </summary>
        private void VirtualKeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the ModifierCheckBox
        /// </summary>
        private void ModifierCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ShellTypeComboBox
        /// </summary>
        private void ShellTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the RunHiddenCheckBox
        /// </summary>
        private void RunHiddenCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the CheckedChanged event of the WaitForExitCheckBox
        /// </summary>
        private void WaitForExitCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the TextChanged event of the CommandTextBox
        /// </summary>
        private void CommandTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the ValueChanged event of the MillisecondsNumericUpDown
        /// </summary>
        private void MillisecondsNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }





        /// <summary>
        /// Handles the ValueChanged event of the DelayAfterNumericUpDown
        /// </summary>
        private void DelayAfterNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the TextChanged event of the DescriptionTextBox
        /// </summary>
        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Save the action data
                if (!SaveActionData())
                    return;

                // Validate the action data
                if (!ValidateActionData())
                    return;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving macro action");
                ApplicationErrorHandler.ShowError($"An error occurred while saving the action: {ex.Message}", "MIDIFlux - Error", _logger, ex, this);
            }
        }

        #endregion
    }
}
