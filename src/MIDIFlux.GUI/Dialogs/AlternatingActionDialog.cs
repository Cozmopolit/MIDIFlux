using System;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for configuring alternating actions (toggle behaviors).
    /// Allows users to create and edit alternating actions with primary and secondary actions.
    /// </summary>
    public partial class AlternatingActionDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly AlternatingActionConfig _alternatingConfig;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the configured alternating action
        /// </summary>
        public AlternatingActionConfig AlternatingConfig => _alternatingConfig;

        /// <summary>
        /// Initializes a new instance of the AlternatingActionDialog
        /// </summary>
        /// <param name="config">The alternating action configuration to edit</param>
        public AlternatingActionDialog(AlternatingActionConfig config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            _alternatingConfig = config;
            _logger = LoggingHelper.CreateLogger<AlternatingActionDialog>();

            InitializeComponent();
            LoadAlternatingData();
        }

        /// <summary>
        /// Loads the alternating configuration data into the UI
        /// </summary>
        private void LoadAlternatingData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load basic properties
                    descriptionTextBox.Text = _alternatingConfig.Description ?? string.Empty;
                    stateKeyTextBox.Text = _alternatingConfig.StateKey ?? string.Empty;
                    startWithPrimaryCheckBox.Checked = _alternatingConfig.StartWithPrimary;

                    // Update action displays
                    UpdatePrimaryActionDisplay();
                    UpdateSecondaryActionDisplay();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading alternating action data", this);
        }

        /// <summary>
        /// Updates the primary action display
        /// </summary>
        private void UpdatePrimaryActionDisplay()
        {
            if (_alternatingConfig.PrimaryAction != null)
            {
                primaryActionLabel.Text = GetActionDisplayText(_alternatingConfig.PrimaryAction);
            }
            else
            {
                primaryActionLabel.Text = "No action configured";
            }
        }

        /// <summary>
        /// Updates the secondary action display
        /// </summary>
        private void UpdateSecondaryActionDisplay()
        {
            if (_alternatingConfig.SecondaryAction != null)
            {
                secondaryActionLabel.Text = GetActionDisplayText(_alternatingConfig.SecondaryAction);
            }
            else
            {
                secondaryActionLabel.Text = "No action configured";
            }
        }

        /// <summary>
        /// Gets a display text for an action configuration
        /// </summary>
        private string GetActionDisplayText(UnifiedActionConfig action)
        {
            return action.Type switch
            {
                UnifiedActionType.KeyPressRelease => "Key Press/Release",
                UnifiedActionType.KeyDown => "Key Down",
                UnifiedActionType.KeyUp => "Key Up",
                UnifiedActionType.KeyToggle => "Key Toggle",
                UnifiedActionType.MouseClick => "Mouse Click",
                UnifiedActionType.MouseScroll => "Mouse Scroll",
                UnifiedActionType.CommandExecution => "Command Execution",
                UnifiedActionType.Delay => "Delay",
                UnifiedActionType.GameControllerButton => "Game Controller Button",
                UnifiedActionType.GameControllerAxis => "Game Controller Axis",
                UnifiedActionType.MidiOutput => "MIDI Output",
                UnifiedActionType.SequenceAction => "Sequence (Macro)",
                UnifiedActionType.ConditionalAction => "Conditional (CC Range)",
                UnifiedActionType.AlternatingAction => "Alternating (Toggle)",
                _ => "Unknown Action"
            };
        }

        #region Event Handlers

        private void ConfigurePrimaryActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var updatedAction = ConfigureAction(_alternatingConfig.PrimaryAction, "Primary Action");
                if (updatedAction != null)
                {
                    _alternatingConfig.PrimaryAction = updatedAction;
                }
                UpdatePrimaryActionDisplay();
            }, _logger, "configuring primary action", this);
        }

        private void ConfigureSecondaryActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var updatedAction = ConfigureAction(_alternatingConfig.SecondaryAction, "Secondary Action");
                if (updatedAction != null)
                {
                    _alternatingConfig.SecondaryAction = updatedAction;
                }
                UpdateSecondaryActionDisplay();
            }, _logger, "configuring secondary action", this);
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _alternatingConfig.Description = descriptionTextBox.Text.Trim();
            }, _logger, "changing description", this);
        }

        private void StateKeyTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _alternatingConfig.StateKey = stateKeyTextBox.Text.Trim();
            }, _logger, "changing state key", this);
        }

        private void StartWithPrimaryCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _alternatingConfig.StartWithPrimary = startWithPrimaryCheckBox.Checked;
            }, _logger, "changing start with primary", this);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Validate the configuration
                if (!ValidateConfiguration())
                    return;

                DialogResult = DialogResult.OK;
            }, _logger, "saving alternating action", this);
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Configures an action using the unified action mapping dialog
        /// </summary>
        private UnifiedActionConfig? ConfigureAction(UnifiedActionConfig? actionConfig, string title)
        {
            // Create a temporary mapping for editing the action
            var tempMapping = new UnifiedActionMapping();

            if (actionConfig != null)
            {
                var factoryLogger = LoggingHelper.CreateLogger<UnifiedActionFactory>();
                var factory = new UnifiedActionFactory(factoryLogger);
                tempMapping.Action = factory.CreateAction(actionConfig);
            }

            using var dialog = new UnifiedActionMappingDialog(tempMapping, null, true);
            dialog.Text = $"Configure {title}";

            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Extract the updated action configuration
                var updatedConfig = ExtractActionConfig(dialog.Mapping.Action);
                return updatedConfig;
            }

            return null; // User cancelled
        }

        /// <summary>
        /// Validates the alternating action configuration
        /// </summary>
        private bool ValidateConfiguration()
        {
            if (_alternatingConfig.PrimaryAction == null)
            {
                ApplicationErrorHandler.ShowError("Primary action must be configured.", "Validation Error", _logger, null, this);
                return false;
            }

            if (_alternatingConfig.SecondaryAction == null)
            {
                ApplicationErrorHandler.ShowError("Secondary action must be configured.", "Validation Error", _logger, null, this);
                return false;
            }

            // Validate state key format if provided
            if (!string.IsNullOrEmpty(_alternatingConfig.StateKey))
            {
                if (!System.Text.RegularExpressions.Regex.IsMatch(_alternatingConfig.StateKey, @"^[a-zA-Z0-9]+$"))
                {
                    ApplicationErrorHandler.ShowError("State key must contain only alphanumeric characters.", "Validation Error", _logger, null, this);
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Extracts action configuration from a unified action instance
        /// </summary>
        private UnifiedActionConfig? ExtractActionConfig(IUnifiedAction action)
        {
            // This is a simplified approach - in a real implementation, you might want
            // to use reflection or a more sophisticated mapping system
            return action switch
            {
                Core.Actions.Simple.KeyPressReleaseAction keyAction => new KeyPressReleaseConfig
                {
                    VirtualKeyCode = keyAction.VirtualKeyCode,
                    Description = keyAction.Description
                },
                Core.Actions.Simple.KeyDownAction keyDownAction => new KeyDownConfig
                {
                    VirtualKeyCode = keyDownAction.VirtualKeyCode,
                    AutoReleaseAfterMs = keyDownAction.AutoReleaseAfterMs,
                    Description = keyDownAction.Description
                },
                Core.Actions.Simple.KeyUpAction keyUpAction => new KeyUpConfig
                {
                    VirtualKeyCode = keyUpAction.VirtualKeyCode,
                    Description = keyUpAction.Description
                },
                Core.Actions.Simple.KeyToggleAction keyToggleAction => new KeyToggleConfig
                {
                    VirtualKeyCode = keyToggleAction.VirtualKeyCode,
                    Description = keyToggleAction.Description
                },
                Core.Actions.Simple.DelayAction delayAction => new DelayConfig
                {
                    Milliseconds = delayAction.Milliseconds,
                    Description = delayAction.Description
                },
                Core.Actions.Simple.MouseClickAction mouseAction => new MouseClickConfig
                {
                    Button = mouseAction.Button,
                    Description = mouseAction.Description
                },
                Core.Actions.Simple.MouseScrollAction scrollAction => new MouseScrollConfig
                {
                    Direction = scrollAction.Direction,
                    Amount = scrollAction.Amount,
                    Description = scrollAction.Description
                },
                Core.Actions.Simple.CommandExecutionAction cmdAction => new CommandExecutionConfig
                {
                    Command = cmdAction.Command,
                    ShellType = cmdAction.ShellType,
                    Description = cmdAction.Description
                },
                Core.Actions.Simple.GameControllerButtonAction gameButtonAction => new GameControllerButtonConfig
                {
                    Button = gameButtonAction.Button,
                    ControllerIndex = gameButtonAction.ControllerIndex,
                    Description = gameButtonAction.Description
                },
                Core.Actions.Simple.GameControllerAxisAction gameAxisAction => new GameControllerAxisConfig
                {
                    AxisName = gameAxisAction.AxisName,
                    AxisValue = gameAxisAction.AxisValue,
                    ControllerIndex = gameAxisAction.ControllerIndex,
                    Description = gameAxisAction.Description
                },
                // Add more action types as needed
                _ => null
            };
        }

        #endregion
    }
}
