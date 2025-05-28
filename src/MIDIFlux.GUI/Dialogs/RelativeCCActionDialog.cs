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
    /// Dialog for configuring relative CC actions.
    /// Allows users to configure increase/decrease actions and encoding method.
    /// </summary>
    public partial class RelativeCCActionDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly RelativeCCConfig _relativeCCConfig;
        private bool _updatingUI = false;

        // UI Controls
        private Button configureIncreaseButton = null!;
        private Button configureDecreaseButton = null!;
        private Label increaseActionLabel = null!;
        private Label decreaseActionLabel = null!;
        private TextBox descriptionTextBox = null!;

        /// <summary>
        /// Gets the configured relative CC action
        /// </summary>
        public RelativeCCConfig RelativeCCConfig => _relativeCCConfig;

        /// <summary>
        /// Initializes a new instance of RelativeCCActionDialog
        /// </summary>
        /// <param name="config">The relative CC configuration to edit</param>
        public RelativeCCActionDialog(RelativeCCConfig config)
        {
            _logger = LoggingHelper.CreateLogger<RelativeCCActionDialog>();
            _relativeCCConfig = config ?? throw new ArgumentNullException(nameof(config));

            InitializeComponent();
            LoadConfiguration();
        }

        /// <summary>
        /// Initializes the dialog components
        /// </summary>
        private void InitializeComponent()
        {
            Text = "Configure Relative CC Action";
            Size = new System.Drawing.Size(500, 280);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;

            var mainPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 4,
                Padding = new Padding(10)
            };

            // Set up column styles
            mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

            // Set up row styles
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Increase action
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F)); // Decrease action
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F)); // Description
            mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F)); // Button row

            // Increase action configuration
            var increaseGroupBox = new GroupBox
            {
                Text = "Increase Action (Positive Values)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var increaseInnerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            increaseInnerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            increaseInnerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            increaseActionLabel = new Label
            {
                Text = "Not configured",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            configureIncreaseButton = new Button
            {
                Text = "Configure...",
                Dock = DockStyle.Fill,
                Height = 30
            };
            configureIncreaseButton.Click += ConfigureIncreaseButton_Click;

            increaseInnerPanel.Controls.Add(increaseActionLabel, 0, 0);
            increaseInnerPanel.Controls.Add(configureIncreaseButton, 1, 0);
            increaseGroupBox.Controls.Add(increaseInnerPanel);
            mainPanel.Controls.Add(increaseGroupBox, 0, 0);

            // Decrease action configuration
            var decreaseGroupBox = new GroupBox
            {
                Text = "Decrease Action (Negative Values)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            var decreaseInnerPanel = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            decreaseInnerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
            decreaseInnerPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));

            decreaseActionLabel = new Label
            {
                Text = "Not configured",
                Dock = DockStyle.Fill,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                AutoEllipsis = true
            };

            configureDecreaseButton = new Button
            {
                Text = "Configure...",
                Dock = DockStyle.Fill,
                Height = 30
            };
            configureDecreaseButton.Click += ConfigureDecreaseButton_Click;

            decreaseInnerPanel.Controls.Add(decreaseActionLabel, 0, 0);
            decreaseInnerPanel.Controls.Add(configureDecreaseButton, 1, 0);
            decreaseGroupBox.Controls.Add(decreaseInnerPanel);
            mainPanel.Controls.Add(decreaseGroupBox, 0, 1);

            // Description
            var descriptionGroupBox = new GroupBox
            {
                Text = "Description (Optional)",
                Dock = DockStyle.Fill,
                Padding = new Padding(10)
            };

            descriptionTextBox = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true
            };
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            descriptionGroupBox.Controls.Add(descriptionTextBox);
            mainPanel.Controls.Add(descriptionGroupBox, 0, 2);

            // Buttons
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.RightToLeft,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Right
            };

            var cancelButton = new Button
            {
                Text = "Cancel",
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.Cancel
            };

            var okButton = new Button
            {
                Text = "OK",
                Size = new System.Drawing.Size(75, 30),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            buttonPanel.Controls.Add(cancelButton);
            buttonPanel.Controls.Add(okButton);
            mainPanel.Controls.Add(buttonPanel, 0, 3);

            Controls.Add(mainPanel);
            AcceptButton = okButton;
            CancelButton = cancelButton;
        }

        /// <summary>
        /// Loads the configuration into the UI
        /// </summary>
        private void LoadConfiguration()
        {
            _updatingUI = true;
            try
            {
                // Update action labels
                UpdateActionLabels();

                // Set description
                descriptionTextBox.Text = _relativeCCConfig.Description ?? string.Empty;
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Updates the action description labels
        /// </summary>
        private void UpdateActionLabels()
        {
            increaseActionLabel.Text = _relativeCCConfig.IncreaseAction?.ToString() ?? "Not configured";
            decreaseActionLabel.Text = _relativeCCConfig.DecreaseAction?.ToString() ?? "Not configured";
        }

        #region Event Handlers

        private void ConfigureIncreaseButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var newConfig = ConfigureAction(_relativeCCConfig.IncreaseAction, "Increase Action");
                if (newConfig != null)
                {
                    _relativeCCConfig.IncreaseAction = newConfig;
                    UpdateActionLabels();
                }
            }, _logger, "configuring increase action", this);
        }

        private void ConfigureDecreaseButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var newConfig = ConfigureAction(_relativeCCConfig.DecreaseAction, "Decrease Action");
                if (newConfig != null)
                {
                    _relativeCCConfig.DecreaseAction = newConfig;
                    UpdateActionLabels();
                }
            }, _logger, "configuring decrease action", this);
        }

        private void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _relativeCCConfig.Description = descriptionTextBox.Text.Trim();
            }, _logger, "changing description", this);
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Validate configuration
                if (!_relativeCCConfig.IsValid())
                {
                    var errors = string.Join("\n", _relativeCCConfig.GetValidationErrors());
                    MessageBox.Show($"Configuration is invalid:\n\n{errors}", "Validation Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult = DialogResult.OK;
            }, _logger, "saving relative CC configuration", this);
        }

        #endregion

        /// <summary>
        /// Configures an action using the action mapping dialog
        /// </summary>
        private ActionConfig? ConfigureAction(ActionConfig? actionConfig, string title)
        {
            // Create a temporary mapping for editing the action
            var tempMapping = new ActionMapping();

            if (actionConfig != null)
            {
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = ActionFactory.CreateForGui(factoryLogger);
                tempMapping.Action = factory.CreateAction(actionConfig);
            }

            using var dialog = new ActionMappingDialog(tempMapping, null, true);
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
        /// Extracts action configuration from an action instance
        /// </summary>
        private ActionConfig? ExtractActionConfig(IAction action)
        {
            // Use the same extraction logic as ActionMappingDialog
            // This is a simplified approach - could be refactored to a shared utility
            return action switch
            {
                Core.Actions.Simple.KeyPressReleaseAction keyAction => new KeyPressReleaseConfig
                {
                    VirtualKeyCode = keyAction.VirtualKeyCode,
                    Description = keyAction.Description
                },
                Core.Actions.Simple.MouseScrollAction scrollAction => new MouseScrollConfig
                {
                    Direction = scrollAction.Direction,
                    Amount = scrollAction.Amount,
                    Description = scrollAction.Description
                },
                Core.Actions.Simple.MouseClickAction clickAction => new MouseClickConfig
                {
                    Button = clickAction.Button,
                    Description = clickAction.Description
                },
                // Add more action types as needed
                _ => null
            };
        }
    }
}
