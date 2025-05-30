using System;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Dialogs;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing individual ValueCondition objects
    /// </summary>
    public partial class ValueConditionEditDialog : BaseDialog
    {
        private ValueCondition _condition;



        /// <summary>
        /// Gets the edited condition
        /// </summary>
        public ValueCondition Condition => _condition;

        /// <summary>
        /// Initializes a new instance of ValueConditionEditDialog
        /// </summary>
        /// <param name="condition">The condition to edit</param>
        /// <param name="logger">The logger to use for this dialog</param>
        public ValueConditionEditDialog(ValueCondition condition, ILogger<ValueConditionEditDialog> logger) : base(logger)
        {
            _condition = new ValueCondition
            {
                MinValue = condition.MinValue,
                MaxValue = condition.MaxValue,
                Action = condition.Action
            };

            InitializeComponent();
            SetupEventHandlers();
            LoadConditionData();
        }



        private void SetupEventHandlers()
        {
            minValueNumericUpDown.ValueChanged += MinValueNumericUpDown_ValueChanged;
            maxValueNumericUpDown.ValueChanged += MaxValueNumericUpDown_ValueChanged;
            configureActionButton.Click += ConfigureActionButton_Click;
            okButton.Click += OkButton_Click;
        }

        private void LoadConditionData()
        {
            minValueNumericUpDown.Value = _condition.MinValue;
            maxValueNumericUpDown.Value = _condition.MaxValue;
            UpdateActionDescription();
        }

        private void UpdateActionDescription()
        {
            if (_condition.Action != null)
            {
                var actionType = _condition.Action.GetType().Name.Replace("Action", "");
                var description = _condition.Action.Description ?? "No description";
                actionDisplayLabel.Text = $"{actionType}: {description}";
                actionDisplayLabel.ForeColor = System.Drawing.SystemColors.ControlText;
            }
            else
            {
                actionDisplayLabel.Text = "No action configured";
                actionDisplayLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            }
        }

        #region Event Handlers

        private void MinValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            _condition.MinValue = (int)minValueNumericUpDown.Value;

            // Ensure max value is not less than min value
            if (maxValueNumericUpDown.Value < minValueNumericUpDown.Value)
            {
                maxValueNumericUpDown.Value = minValueNumericUpDown.Value;
            }
        }

        private void MaxValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            _condition.MaxValue = (int)maxValueNumericUpDown.Value;

            // Ensure min value is not greater than max value
            if (minValueNumericUpDown.Value > maxValueNumericUpDown.Value)
            {
                minValueNumericUpDown.Value = maxValueNumericUpDown.Value;
            }
        }

        private void ConfigureActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a default action if none exists
                if (_condition.Action == null)
                {
                    _condition.Action = new Core.Actions.Simple.KeyPressReleaseAction(Keys.A); // 'A' key
                }

                // Create a temporary mapping for the dialog
                var tempMapping = new ActionMapping
                {
                    Action = _condition.Action,
                    Description = _condition.Action.Description ?? "Condition Action",
                    IsEnabled = true,
                    Input = new MidiInput
                    {
                        InputType = MidiInputType.NoteOn,
                        InputNumber = 60,
                        Channel = 1
                    }
                };

                using var dialog = new ActionMappingDialog(tempMapping, null, true, LoggingHelper.CreateLogger<ActionMappingDialog>());
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _condition.Action = (ActionBase)dialog.Mapping.Action;
                    UpdateActionDescription();
                }
            }, _logger, "configuring condition action", this);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // Validate the condition
            if (_condition.Action == null)
            {
                MessageBox.Show("Please configure an action for this condition.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_condition.MinValue > _condition.MaxValue)
            {
                MessageBox.Show("Min value cannot be greater than max value.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Update final values
            _condition.MinValue = (int)minValueNumericUpDown.Value;
            _condition.MaxValue = (int)maxValueNumericUpDown.Value;
        }

        #endregion
    }
}
