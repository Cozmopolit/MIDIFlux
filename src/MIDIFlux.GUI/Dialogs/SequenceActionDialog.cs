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
    /// Dialog for configuring sequence actions (macros).
    /// Allows users to create and edit complex action sequences with sub-actions.
    /// </summary>
    public partial class SequenceActionDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly SequenceConfig _sequenceConfig;
        private bool _updatingUI = false;

        /// <summary>
        /// Gets the configured sequence action
        /// </summary>
        public SequenceConfig SequenceConfig => _sequenceConfig;

        /// <summary>
        /// Initializes a new instance of the SequenceActionDialog
        /// </summary>
        /// <param name="sequenceConfig">The sequence configuration to edit</param>
        public SequenceActionDialog(SequenceConfig sequenceConfig)
        {
            _logger = LoggingHelper.CreateLogger<SequenceActionDialog>();
            _sequenceConfig = sequenceConfig ?? throw new ArgumentNullException(nameof(sequenceConfig));

            InitializeComponent();
            SetupEventHandlers();
            LoadSequenceData();
        }

        /// <summary>
        /// Sets up event handlers for the dialog controls
        /// </summary>
        private void SetupEventHandlers()
        {
            addActionButton.Click += AddActionButton_Click;
            editActionButton.Click += EditActionButton_Click;
            removeActionButton.Click += RemoveActionButton_Click;
            moveUpButton.Click += MoveUpButton_Click;
            moveDownButton.Click += MoveDownButton_Click;
            templatesButton.Click += TemplatesButton_Click;
            actionsListView.SelectedIndexChanged += ActionsListView_SelectedIndexChanged;
            actionsListView.DoubleClick += ActionsListView_DoubleClick;
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            errorHandlingComboBox.SelectedIndexChanged += ErrorHandlingComboBox_SelectedIndexChanged;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;
        }

        /// <summary>
        /// Loads the sequence configuration data into the UI
        /// </summary>
        private void LoadSequenceData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load basic properties
                    descriptionTextBox.Text = _sequenceConfig.Description ?? string.Empty;

                    // Load error handling
                    errorHandlingComboBox.Items.Clear();
                    errorHandlingComboBox.Items.AddRange(new object[]
                    {
                        "Continue on Error",
                        "Stop on Error"
                    });
                    errorHandlingComboBox.SelectedIndex = _sequenceConfig.ErrorHandling == SequenceErrorHandling.ContinueOnError ? 0 : 1;

                    // Load sub-actions
                    RefreshActionsList();
                    UpdateButtonStates();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading sequence data", this);
        }

        /// <summary>
        /// Refreshes the actions list view with current sub-actions
        /// </summary>
        private void RefreshActionsList()
        {
            actionsListView.Items.Clear();

            for (int i = 0; i < _sequenceConfig.SubActions.Count; i++)
            {
                var action = _sequenceConfig.SubActions[i];
                var item = new ListViewItem(new[]
                {
                    (i + 1).ToString(),
                    GetActionTypeName(action),
                    action.Description ?? "No description"
                })
                {
                    Tag = action
                };
                actionsListView.Items.Add(item);
            }
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
                UnifiedActionType.SequenceAction => "Sequence (Nested)",
                UnifiedActionType.ConditionalAction => "Conditional",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Updates the enabled state of buttons based on current selection
        /// </summary>
        private void UpdateButtonStates()
        {
            var hasSelection = actionsListView.SelectedItems.Count > 0;
            var selectedIndex = hasSelection ? actionsListView.SelectedItems[0].Index : -1;

            editActionButton.Enabled = hasSelection;
            removeActionButton.Enabled = hasSelection;
            moveUpButton.Enabled = hasSelection && selectedIndex > 0;
            moveDownButton.Enabled = hasSelection && selectedIndex < actionsListView.Items.Count - 1;
        }

        #region Event Handlers

        private void AddActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                using var dialog = new UnifiedActionMappingDialog(new UnifiedActionMapping(), null, true);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Extract the action configuration from the mapping
                    var actionConfig = ExtractActionConfig(dialog.Mapping.Action);
                    if (actionConfig != null)
                    {
                        _sequenceConfig.SubActions.Add(actionConfig);
                        RefreshActionsList();

                        // Select the newly added item
                        if (actionsListView.Items.Count > 0)
                        {
                            var lastItem = actionsListView.Items[actionsListView.Items.Count - 1];
                            lastItem.Selected = true;
                            lastItem.EnsureVisible();
                        }

                        UpdateButtonStates();
                    }
                }
            }, _logger, "adding action to sequence", this);
        }

        private void EditActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = actionsListView.SelectedItems[0].Index;
                var actionConfig = _sequenceConfig.SubActions[selectedIndex];

                // Create a temporary mapping for editing
                var tempMapping = new UnifiedActionMapping();
                var factoryLogger = LoggingHelper.CreateLogger<UnifiedActionFactory>();
                var factory = new UnifiedActionFactory(factoryLogger);
                tempMapping.Action = factory.CreateAction(actionConfig);

                using var dialog = new UnifiedActionMappingDialog(tempMapping, null, true);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Update the action configuration
                    var updatedConfig = ExtractActionConfig(dialog.Mapping.Action);
                    if (updatedConfig != null)
                    {
                        _sequenceConfig.SubActions[selectedIndex] = updatedConfig;
                        RefreshActionsList();

                        // Restore selection
                        if (selectedIndex < actionsListView.Items.Count)
                        {
                            actionsListView.Items[selectedIndex].Selected = true;
                        }
                    }
                }
            }, _logger, "editing sequence action", this);
        }

        private void RemoveActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = actionsListView.SelectedItems[0].Index;
                var actionConfig = _sequenceConfig.SubActions[selectedIndex];
                var actionName = actionConfig.Description ?? GetActionTypeName(actionConfig);

                if (ShowConfirmation($"Are you sure you want to remove the action '{actionName}'?", "Remove Action"))
                {
                    _sequenceConfig.SubActions.RemoveAt(selectedIndex);
                    RefreshActionsList();
                    UpdateButtonStates();
                }
            }, _logger, "removing sequence action", this);
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = actionsListView.SelectedItems[0].Index;
                if (selectedIndex > 0)
                {
                    var action = _sequenceConfig.SubActions[selectedIndex];
                    _sequenceConfig.SubActions.RemoveAt(selectedIndex);
                    _sequenceConfig.SubActions.Insert(selectedIndex - 1, action);

                    RefreshActionsList();
                    actionsListView.Items[selectedIndex - 1].Selected = true;
                    UpdateButtonStates();
                }
            }, _logger, "moving sequence action up", this);
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (actionsListView.SelectedItems.Count == 0) return;

                var selectedIndex = actionsListView.SelectedItems[0].Index;
                if (selectedIndex < _sequenceConfig.SubActions.Count - 1)
                {
                    var action = _sequenceConfig.SubActions[selectedIndex];
                    _sequenceConfig.SubActions.RemoveAt(selectedIndex);
                    _sequenceConfig.SubActions.Insert(selectedIndex + 1, action);

                    RefreshActionsList();
                    actionsListView.Items[selectedIndex + 1].Selected = true;
                    UpdateButtonStates();
                }
            }, _logger, "moving sequence action down", this);
        }

        private void TemplatesButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                ShowTemplateMenu();
            }, _logger, "showing template menu", this);
        }

        private void ActionsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void ActionsListView_DoubleClick(object? sender, EventArgs e)
        {
            if (actionsListView.SelectedItems.Count > 0)
            {
                EditActionButton_Click(sender, e);
            }
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _sequenceConfig.Description = descriptionTextBox.Text;
            }
        }

        private void ErrorHandlingComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!_updatingUI)
            {
                _sequenceConfig.ErrorHandling = errorHandlingComboBox.SelectedIndex == 0
                    ? SequenceErrorHandling.ContinueOnError
                    : SequenceErrorHandling.StopOnError;
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (ValidateSequence())
                {
                    DialogResult = DialogResult.OK;
                }
            }, _logger, "validating and saving sequence", this);
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
                // Add more action types as needed
                _ => null
            };
        }

        /// <summary>
        /// Shows the template menu for common action sequences
        /// </summary>
        private void ShowTemplateMenu()
        {
            var menu = new ContextMenuStrip();

            // Add common templates
            menu.Items.Add("Ctrl+C (Copy)", null, (s, e) => AddTemplate_CtrlC());
            menu.Items.Add("Ctrl+V (Paste)", null, (s, e) => AddTemplate_CtrlV());
            menu.Items.Add("Ctrl+X (Cut)", null, (s, e) => AddTemplate_CtrlX());
            menu.Items.Add("Ctrl+Z (Undo)", null, (s, e) => AddTemplate_CtrlZ());
            menu.Items.Add("Ctrl+A (Select All)", null, (s, e) => AddTemplate_CtrlA());
            menu.Items.Add("-"); // Separator
            menu.Items.Add("Alt+Tab (Switch Window)", null, (s, e) => AddTemplate_AltTab());
            menu.Items.Add("Windows+D (Show Desktop)", null, (s, e) => AddTemplate_WinD());

            menu.Show(templatesButton, new System.Drawing.Point(0, templatesButton.Height));
        }

        /// <summary>
        /// Adds Ctrl+C template
        /// </summary>
        private void AddTemplate_CtrlC()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 17, Description = "Ctrl down" }, // VK_CONTROL
                new KeyPressReleaseConfig { VirtualKeyCode = 67, Description = "Press C" }, // 'C'
                new KeyUpConfig { VirtualKeyCode = 17, Description = "Ctrl up" }
            };

            AddTemplateActions(actions, "Ctrl+C (Copy)");
        }

        /// <summary>
        /// Adds Ctrl+V template
        /// </summary>
        private void AddTemplate_CtrlV()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 17, Description = "Ctrl down" },
                new KeyPressReleaseConfig { VirtualKeyCode = 86, Description = "Press V" }, // 'V'
                new KeyUpConfig { VirtualKeyCode = 17, Description = "Ctrl up" }
            };

            AddTemplateActions(actions, "Ctrl+V (Paste)");
        }

        /// <summary>
        /// Adds Ctrl+X template
        /// </summary>
        private void AddTemplate_CtrlX()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 17, Description = "Ctrl down" },
                new KeyPressReleaseConfig { VirtualKeyCode = 88, Description = "Press X" }, // 'X'
                new KeyUpConfig { VirtualKeyCode = 17, Description = "Ctrl up" }
            };

            AddTemplateActions(actions, "Ctrl+X (Cut)");
        }

        /// <summary>
        /// Adds Ctrl+Z template
        /// </summary>
        private void AddTemplate_CtrlZ()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 17, Description = "Ctrl down" },
                new KeyPressReleaseConfig { VirtualKeyCode = 90, Description = "Press Z" }, // 'Z'
                new KeyUpConfig { VirtualKeyCode = 17, Description = "Ctrl up" }
            };

            AddTemplateActions(actions, "Ctrl+Z (Undo)");
        }

        /// <summary>
        /// Adds Ctrl+A template
        /// </summary>
        private void AddTemplate_CtrlA()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 17, Description = "Ctrl down" },
                new KeyPressReleaseConfig { VirtualKeyCode = 65, Description = "Press A" }, // 'A'
                new KeyUpConfig { VirtualKeyCode = 17, Description = "Ctrl up" }
            };

            AddTemplateActions(actions, "Ctrl+A (Select All)");
        }

        /// <summary>
        /// Adds Alt+Tab template
        /// </summary>
        private void AddTemplate_AltTab()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 18, Description = "Alt down" }, // VK_MENU
                new KeyPressReleaseConfig { VirtualKeyCode = 9, Description = "Press Tab" }, // VK_TAB
                new KeyUpConfig { VirtualKeyCode = 18, Description = "Alt up" }
            };

            AddTemplateActions(actions, "Alt+Tab (Switch Window)");
        }

        /// <summary>
        /// Adds Windows+D template
        /// </summary>
        private void AddTemplate_WinD()
        {
            var actions = new List<UnifiedActionConfig>
            {
                new KeyDownConfig { VirtualKeyCode = 91, Description = "Windows key down" }, // VK_LWIN
                new KeyPressReleaseConfig { VirtualKeyCode = 68, Description = "Press D" }, // 'D'
                new KeyUpConfig { VirtualKeyCode = 91, Description = "Windows key up" }
            };

            AddTemplateActions(actions, "Windows+D (Show Desktop)");
        }

        /// <summary>
        /// Adds a list of template actions to the sequence
        /// </summary>
        private void AddTemplateActions(List<UnifiedActionConfig> actions, string templateName)
        {
            if (ShowConfirmation($"Add {templateName} template to the sequence?", "Add Template"))
            {
                foreach (var action in actions)
                {
                    _sequenceConfig.SubActions.Add(action);
                }

                RefreshActionsList();

                // Select the last added item
                if (actionsListView.Items.Count > 0)
                {
                    var lastItem = actionsListView.Items[actionsListView.Items.Count - 1];
                    lastItem.Selected = true;
                    lastItem.EnsureVisible();
                }

                UpdateButtonStates();
            }
        }

        /// <summary>
        /// Validates the sequence configuration
        /// </summary>
        private bool ValidateSequence()
        {
            var errors = _sequenceConfig.GetValidationErrors();
            if (errors.Count > 0)
            {
                var errorMessage = "The sequence configuration has the following errors:\n\n" +
                                 string.Join("\n", errors);
                ShowError(errorMessage, "Validation Error");
                return false;
            }

            if (_sequenceConfig.SubActions.Count == 0)
            {
                ShowError("The sequence must contain at least one action.", "Validation Error");
                return false;
            }

            return true;
        }

        #endregion
    }
}
