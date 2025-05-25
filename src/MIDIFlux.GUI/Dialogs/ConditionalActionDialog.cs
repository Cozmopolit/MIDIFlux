using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for configuring conditional actions (fader-to-buttons).
    /// Allows users to create and edit value-based conditional actions with multiple conditions.
    /// </summary>
    public partial class ConditionalActionDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly ConditionalConfig _conditionalConfig;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the configured conditional action
        /// </summary>
        public ConditionalConfig ConditionalConfig => _conditionalConfig;

        /// <summary>
        /// Initializes a new instance of the ConditionalActionDialog
        /// </summary>
        /// <param name="conditionalConfig">The conditional configuration to edit</param>
        public ConditionalActionDialog(ConditionalConfig conditionalConfig)
        {
            _logger = LoggingHelper.CreateLogger<ConditionalActionDialog>();
            _conditionalConfig = conditionalConfig ?? throw new ArgumentNullException(nameof(conditionalConfig));

            InitializeComponent();
            SetupEventHandlers();
            LoadConditionalData();
        }

        /// <summary>
        /// Sets up event handlers for the dialog controls
        /// </summary>
        private void SetupEventHandlers()
        {
            addConditionButton.Click += AddConditionButton_Click;
            editConditionButton.Click += EditConditionButton_Click;
            removeConditionButton.Click += RemoveConditionButton_Click;
            conditionsListView.SelectedIndexChanged += ConditionsListView_SelectedIndexChanged;
            conditionsListView.DoubleClick += ConditionsListView_DoubleClick;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
            testValueNumericUpDown.ValueChanged += TestValueNumericUpDown_ValueChanged;
            testButton.Click += TestButton_Click;
        }

        /// <summary>
        /// Loads the conditional configuration data into the UI
        /// </summary>
        private void LoadConditionalData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load basic properties
                    descriptionTextBox.Text = _conditionalConfig.Description ?? string.Empty;

                    // Load conditions
                    RefreshConditionsList();
                    UpdateButtonStates();
                    UpdateTestResult();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading conditional data", this);
        }

        /// <summary>
        /// Refreshes the conditions list view with current conditions
        /// </summary>
        private void RefreshConditionsList()
        {
            conditionsListView.Items.Clear();

            for (int i = 0; i < _conditionalConfig.Conditions.Count; i++)
            {
                var condition = _conditionalConfig.Conditions[i];
                var rangeText = condition.MinValue == condition.MaxValue
                    ? condition.MinValue.ToString()
                    : $"{condition.MinValue}-{condition.MaxValue}";

                var item = new ListViewItem(new[]
                {
                    rangeText,
                    GetActionTypeName(condition.Action),
                    condition.Description ?? condition.Action.Description ?? "No description"
                })
                {
                    Tag = condition
                };
                conditionsListView.Items.Add(item);
            }
        }

        /// <summary>
        /// Gets a user-friendly name for an action type
        /// </summary>
        private string GetActionTypeName(ActionConfig action)
        {
            return action.Type switch
            {
                ActionType.KeyPressRelease => "Key Press/Release",
                ActionType.KeyDown => "Key Down",
                ActionType.KeyUp => "Key Up",
                ActionType.KeyToggle => "Key Toggle",
                ActionType.MouseClick => "Mouse Click",
                ActionType.MouseScroll => "Mouse Scroll",
                ActionType.CommandExecution => "Command Execution",
                ActionType.Delay => "Delay",
                ActionType.GameControllerButton => "Game Controller Button",
                ActionType.GameControllerAxis => "Game Controller Axis",
                ActionType.SequenceAction => "Sequence (Macro)",
                ActionType.ConditionalAction => "Conditional (Nested)",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Updates the enabled state of buttons based on current selection
        /// </summary>
        private void UpdateButtonStates()
        {
            var hasSelection = conditionsListView.SelectedItems.Count > 0;

            editConditionButton.Enabled = hasSelection;
            removeConditionButton.Enabled = hasSelection;
        }

        /// <summary>
        /// Updates the test result based on the current test value
        /// </summary>
        private void UpdateTestResult()
        {
            var testValue = (int)testValueNumericUpDown.Value;
            var matchingCondition = _conditionalConfig.FindMatchingCondition(testValue);

            if (matchingCondition != null)
            {
                var rangeText = matchingCondition.MinValue == matchingCondition.MaxValue
                    ? matchingCondition.MinValue.ToString()
                    : $"{matchingCondition.MinValue}-{matchingCondition.MaxValue}";

                testResultLabel.Text = $"Matches: {rangeText} → {GetActionTypeName(matchingCondition.Action)}";
                testResultLabel.ForeColor = System.Drawing.Color.Green;
                testButton.Enabled = true;
            }
            else
            {
                testResultLabel.Text = "No matching condition";
                testResultLabel.ForeColor = System.Drawing.Color.Red;
                testButton.Enabled = false;
            }
        }

        #region Event Handlers

        private void AddConditionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a new condition with default values
                var newCondition = new ValueConditionConfig
                {
                    MinValue = 0,
                    MaxValue = 127,
                    Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }, // 'A' key
                    Description = "New condition"
                };

                using var dialog = new ValueConditionEditDialog(newCondition);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Check for overlapping ranges
                    if (HasOverlappingRange(dialog.Condition))
                    {
                        ShowError("The specified value range overlaps with an existing condition.", "Range Overlap");
                        return;
                    }

                    _conditionalConfig.Conditions.Add(dialog.Condition);
                    RefreshConditionsList();

                    // Select the newly added item
                    if (conditionsListView.Items.Count > 0)
                    {
                        var lastItem = conditionsListView.Items[conditionsListView.Items.Count - 1];
                        lastItem.Selected = true;
                        lastItem.EnsureVisible();
                    }

                    UpdateButtonStates();
                    UpdateTestResult();
                }
            }, _logger, "adding condition to conditional action", this);
        }

        private void EditConditionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (conditionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = conditionsListView.SelectedItems[0].Index;
                var condition = _conditionalConfig.Conditions[selectedIndex];

                using var dialog = new ValueConditionEditDialog(condition);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Check for overlapping ranges (excluding the current condition)
                    if (HasOverlappingRange(dialog.Condition, selectedIndex))
                    {
                        ShowError("The specified value range overlaps with an existing condition.", "Range Overlap");
                        return;
                    }

                    // Update the condition
                    _conditionalConfig.Conditions[selectedIndex] = dialog.Condition;
                    RefreshConditionsList();

                    // Restore selection
                    if (selectedIndex < conditionsListView.Items.Count)
                    {
                        conditionsListView.Items[selectedIndex].Selected = true;
                    }

                    UpdateTestResult();
                }
            }, _logger, "editing conditional condition", this);
        }

        private void RemoveConditionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (conditionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = conditionsListView.SelectedItems[0].Index;
                var condition = _conditionalConfig.Conditions[selectedIndex];
                var conditionName = condition.Description ?? $"Range {condition.MinValue}-{condition.MaxValue}";

                if (ShowConfirmation($"Are you sure you want to remove the condition '{conditionName}'?", "Remove Condition"))
                {
                    _conditionalConfig.Conditions.RemoveAt(selectedIndex);
                    RefreshConditionsList();
                    UpdateButtonStates();
                    UpdateTestResult();
                }
            }, _logger, "removing conditional condition", this);
        }

        private void ConditionsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void ConditionsListView_DoubleClick(object? sender, EventArgs e)
        {
            if (conditionsListView.SelectedItems.Count > 0)
            {
                EditConditionButton_Click(sender, e);
            }
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _conditionalConfig.Description = descriptionTextBox.Text;
            }
        }

        private void TestValueNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            UpdateTestResult();
        }

        private void TestButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var testValue = (int)testValueNumericUpDown.Value;
                var matchingCondition = _conditionalConfig.FindMatchingCondition(testValue);

                if (matchingCondition != null)
                {
                    ShowMessage($"Test value {testValue} would execute:\n\n" +
                              $"Range: {matchingCondition.MinValue}-{matchingCondition.MaxValue}\n" +
                              $"Action: {GetActionTypeName(matchingCondition.Action)}\n" +
                              $"Description: {matchingCondition.Description ?? matchingCondition.Action.Description ?? "No description"}",
                              "Test Result");
                }
            }, _logger, "testing conditional value", this);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            HandleOkButtonClick(ValidateConditional, "validating and saving conditional");
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            HandleCancelButtonClick();
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Checks if a condition has an overlapping range with existing conditions
        /// </summary>
        /// <param name="condition">The condition to check</param>
        /// <param name="excludeIndex">Index to exclude from the check (for editing)</param>
        /// <returns>True if there's an overlap, false otherwise</returns>
        private bool HasOverlappingRange(ValueConditionConfig condition, int excludeIndex = -1)
        {
            for (int i = 0; i < _conditionalConfig.Conditions.Count; i++)
            {
                if (i == excludeIndex) continue; // Skip the condition being edited

                var existingCondition = _conditionalConfig.Conditions[i];

                // Check for overlap: ranges overlap if one starts before the other ends
                if (condition.MinValue <= existingCondition.MaxValue &&
                    condition.MaxValue >= existingCondition.MinValue)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Validates the conditional configuration
        /// </summary>
        private bool ValidateConditional()
        {
            return ValidateConfigurationObject(_conditionalConfig, "conditional", () =>
            {
                if (_conditionalConfig.Conditions.Count == 0)
                {
                    ShowError("The conditional action must contain at least one condition.", "Validation Error");
                    return false;
                }
                return true;
            });
        }

        #endregion
    }
}
