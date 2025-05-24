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
    /// Dialog for editing individual value conditions within conditional actions.
    /// Allows users to configure value ranges and associated actions.
    /// </summary>
    public partial class ValueConditionEditDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly ValueConditionConfig _condition;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the edited value condition
        /// </summary>
        public ValueConditionConfig Condition => _condition;

        /// <summary>
        /// Initializes a new instance of the ValueConditionEditDialog
        /// </summary>
        /// <param name="condition">The value condition to edit</param>
        public ValueConditionEditDialog(ValueConditionConfig condition)
        {
            _logger = LoggingHelper.CreateLogger<ValueConditionEditDialog>();
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));

            InitializeComponent();
            SetupEventHandlers();
            LoadConditionData();
        }

        /// <summary>
        /// Sets up event handlers for the dialog controls
        /// </summary>
        private void SetupEventHandlers()
        {
            minValueNumericUpDown.ValueChanged += MinValueNumericUpDown_ValueChanged;
            maxValueNumericUpDown.ValueChanged += MaxValueNumericUpDown_ValueChanged;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            configureActionButton.Click += ConfigureActionButton_Click;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        /// <summary>
        /// Loads the condition data into the UI
        /// </summary>
        private void LoadConditionData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load range values
                    minValueNumericUpDown.Value = _condition.MinValue;
                    maxValueNumericUpDown.Value = _condition.MaxValue;

                    // Load description
                    descriptionTextBox.Text = _condition.Description ?? string.Empty;

                    // Update action display
                    UpdateActionDisplay();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading condition data", this);
        }

        /// <summary>
        /// Updates the action display text
        /// </summary>
        private void UpdateActionDisplay()
        {
            var actionTypeName = GetActionTypeName(_condition.Action);
            var actionDescription = _condition.Action.Description ?? "No description";
            actionDisplayLabel.Text = $"{actionTypeName}: {actionDescription}";
        }

        /// <summary>
        /// Gets a user-friendly name for an action type
        /// </summary>
        private string GetActionTypeName(UnifiedActionConfig action)
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
                UnifiedActionType.SequenceAction => "Sequence (Macro)",
                UnifiedActionType.ConditionalAction => "Conditional (Nested)",
                _ => "Unknown"
            };
        }

        #region Event Handlers

        private void MinValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _condition.MinValue = (int)minValueNumericUpDown.Value;

                // Ensure max value is not less than min value
                if (maxValueNumericUpDown.Value < minValueNumericUpDown.Value)
                {
                    maxValueNumericUpDown.Value = minValueNumericUpDown.Value;
                }
            }
        }

        private void MaxValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _condition.MaxValue = (int)maxValueNumericUpDown.Value;

                // Ensure min value is not greater than max value
                if (minValueNumericUpDown.Value > maxValueNumericUpDown.Value)
                {
                    minValueNumericUpDown.Value = maxValueNumericUpDown.Value;
                }
            }
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _condition.Description = descriptionTextBox.Text;
            }
        }

        private void ConfigureActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a temporary mapping for editing the action
                var tempMapping = new UnifiedActionMapping();
                var factoryLogger = LoggingHelper.CreateLogger<UnifiedActionFactory>();
                var factory = new UnifiedActionFactory(factoryLogger);
                tempMapping.Action = factory.CreateAction(_condition.Action);

                using var dialog = new UnifiedActionMappingDialog(tempMapping, null, true);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Extract the updated action configuration
                    var updatedConfig = ExtractActionConfig(dialog.Mapping.Action);
                    if (updatedConfig != null)
                    {
                        _condition.Action = updatedConfig;
                        UpdateActionDisplay();
                    }
                }
            }, _logger, "configuring condition action", this);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (ValidateCondition())
                {
                    DialogResult = DialogResult.OK;
                }
            }, _logger, "validating and saving condition", this);
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion

        #region Helper Methods

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
                    Description = keyDownAction.Description
                },
                Core.Actions.Simple.KeyUpAction keyUpAction => new KeyUpConfig
                {
                    VirtualKeyCode = keyUpAction.VirtualKeyCode,
                    Description = keyUpAction.Description
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
                // Add more action types as needed
                _ => null
            };
        }

        /// <summary>
        /// Validates the condition configuration
        /// </summary>
        private bool ValidateCondition()
        {
            var errors = _condition.GetValidationErrors();
            if (errors.Count > 0)
            {
                var errorMessage = "The condition configuration has the following errors:\n\n" +
                                 string.Join("\n", errors);
                ShowError(errorMessage, "Validation Error");
                return false;
            }

            return true;
        }

        #endregion
    }
}
