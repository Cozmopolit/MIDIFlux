using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Configuration;

namespace MIDIFlux.GUI.Forms
{
    /// <summary>
    /// Form for managing application settings
    /// </summary>
    public partial class SettingsForm : Form
    {
        private readonly ILogger<SettingsForm> _logger;
        private readonly ConfigurationService _configurationService;
        private bool _isDirty = false;

        // Controls
        private TabControl tabControl = null!;
        private Button okButton = null!;
        private Button cancelButton = null!;
        private Button applyButton = null!;



        // Logging tab controls
        private ComboBox logLevelComboBox = null!;
        private CheckBox silentModeCheckBox = null!;

        // Advanced tab controls
        private CheckBox autoReconnectCheckBox = null!;
        private NumericUpDown scanIntervalNumeric = null!;

        public SettingsForm(ConfigurationService configurationService)
        {
            _logger = LoggingHelper.CreateLogger<SettingsForm>();
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));

            try
            {
                _logger.LogDebug("Initializing SettingsForm");

                InitializeComponent();
                LoadSettings();

                _logger.LogInformation("SettingsForm initialized successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing SettingsForm: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    "An error occurred while initializing the Settings form.",
                    "MIDIFlux - Settings Error",
                    _logger,
                    ex);
            }
        }

        private void InitializeComponent()
        {
            Text = "MIDIFlux Settings";
            Size = new Size(500, 400);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;

            // Create tab control
            tabControl = new TabControl
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(10)
            };

            // Create tabs
            CreateLoggingTab();
            CreateAdvancedTab();

            // Create button panel
            var buttonPanel = new Panel
            {
                Height = 50,
                Dock = DockStyle.Bottom
            };

            okButton = new Button
            {
                Text = "OK",
                Size = new Size(75, 25),
                Location = new Point(260, 12),
                DialogResult = DialogResult.OK
            };
            okButton.Click += OkButton_Click;

            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(75, 25),
                Location = new Point(345, 12),
                DialogResult = DialogResult.Cancel
            };
            cancelButton.Click += CancelButton_Click;

            applyButton = new Button
            {
                Text = "Apply",
                Size = new Size(75, 25),
                Location = new Point(175, 12),
                Enabled = false
            };
            applyButton.Click += ApplyButton_Click;

            buttonPanel.Controls.AddRange(new Control[] { applyButton, okButton, cancelButton });

            Controls.Add(tabControl);
            Controls.Add(buttonPanel);

            AcceptButton = okButton;
            CancelButton = cancelButton;
        }



        private void CreateLoggingTab()
        {
            var loggingTab = new TabPage("Logging");

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Log level
            var logLevelLabel = new Label
            {
                Text = "Log Level:",
                Location = new Point(10, 20),
                Size = new Size(80, 20)
            };

            logLevelComboBox = new ComboBox
            {
                Location = new Point(100, 17),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            logLevelComboBox.Items.AddRange(new[] { "None", "Critical", "Error", "Warning", "Information", "Debug", "Trace" });
            logLevelComboBox.SelectedIndexChanged += Control_Changed;

            // Log level note
            var logLevelNote = new Label
            {
                Text = "Note: Logging level changes require application restart",
                Location = new Point(260, 20),
                Size = new Size(280, 20),
                ForeColor = Color.Gray,
                Font = new Font(SystemFonts.DefaultFont.FontFamily, 8.0f, FontStyle.Italic)
            };

            // Silent mode
            silentModeCheckBox = new CheckBox
            {
                Text = "Silent Mode (No popup error messages)",
                Location = new Point(10, 60),
                Size = new Size(300, 20)
            };
            silentModeCheckBox.CheckedChanged += Control_Changed;

            panel.Controls.AddRange(new Control[]
            {
                logLevelLabel, logLevelComboBox, logLevelNote,
                silentModeCheckBox
            });

            loggingTab.Controls.Add(panel);
            tabControl.TabPages.Add(loggingTab);
        }

        private void CreateAdvancedTab()
        {
            var advancedTab = new TabPage("Advanced");

            var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };

            // Auto reconnect
            autoReconnectCheckBox = new CheckBox
            {
                Text = "Automatically reconnect to MIDI devices",
                Location = new Point(10, 20),
                Size = new Size(300, 20)
            };
            autoReconnectCheckBox.CheckedChanged += Control_Changed;

            // Scan interval
            var scanIntervalLabel = new Label
            {
                Text = "Device Scan Interval (seconds):",
                Location = new Point(10, 60),
                Size = new Size(180, 20)
            };

            scanIntervalNumeric = new NumericUpDown
            {
                Location = new Point(200, 57),
                Size = new Size(80, 25),
                Minimum = 1,
                Maximum = 60,
                Value = 5
            };
            scanIntervalNumeric.ValueChanged += Control_Changed;

            panel.Controls.AddRange(new Control[]
            {
                autoReconnectCheckBox,
                scanIntervalLabel, scanIntervalNumeric
            });

            advancedTab.Controls.Add(panel);
            tabControl.TabPages.Add(advancedTab);
        }

        private void LoadSettings()
        {
            try
            {
                _logger.LogDebug("Loading settings into form controls");

                // Logging settings
                logLevelComboBox.SelectedItem = _configurationService.GetSetting("Logging.LogLevel", "Information");
                silentModeCheckBox.Checked = _configurationService.GetSetting("Application.SilentMode", false);

                // Advanced settings
                autoReconnectCheckBox.Checked = _configurationService.GetSetting("MIDI.AutoReconnect", true);
                scanIntervalNumeric.Value = _configurationService.GetSetting("MIDI.ScanIntervalSeconds", 5);

                _logger.LogDebug("Settings loaded successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    "An error occurred while loading settings.",
                    "MIDIFlux - Settings Error",
                    _logger,
                    ex);
            }
        }

        private void SaveSettings()
        {
            try
            {
                _logger.LogDebug("Saving settings from form controls");

                // Prepare all settings updates
                var updates = new Dictionary<string, object>
                {
                    // Logging settings
                    ["Logging.LogLevel"] = logLevelComboBox.SelectedItem?.ToString() ?? "Information",
                    ["Application.SilentMode"] = silentModeCheckBox.Checked,

                    // Advanced settings
                    ["MIDI.AutoReconnect"] = autoReconnectCheckBox.Checked,
                    ["MIDI.ScanIntervalSeconds"] = (int)scanIntervalNumeric.Value
                };

                // Save all settings atomically
                if (_configurationService.UpdateSettings(updates))
                {
                    _logger.LogInformation("Settings saved successfully");

                    // Update silent mode immediately
                    ApplicationErrorHandler.SilentMode = silentModeCheckBox.Checked;

                    _isDirty = false;
                    applyButton.Enabled = false;
                }
                else
                {
                    throw new InvalidOperationException("Failed to save settings to file");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    "An error occurred while saving settings.",
                    "MIDIFlux - Settings Error",
                    _logger,
                    ex);
            }
        }

        private void Control_Changed(object? sender, EventArgs e)
        {
            if (!_isDirty)
            {
                _isDirty = true;
                applyButton.Enabled = true;
                _logger.LogDebug("Settings marked as dirty");
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_isDirty)
                {
                    SaveSettings();
                }
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OK button click: {Message}", ex.Message);
                // Don't close the form if there was an error saving
                DialogResult = DialogResult.None;
            }
        }

        private void CancelButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (_isDirty)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Are you sure you want to cancel?",
                        "MIDIFlux - Unsaved Changes",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question);

                    if (result == DialogResult.No)
                    {
                        DialogResult = DialogResult.None;
                        return;
                    }
                }

                DialogResult = DialogResult.Cancel;
                Close();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Cancel button click: {Message}", ex.Message);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void ApplyButton_Click(object? sender, EventArgs e)
        {
            SaveSettings();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                if (_isDirty && DialogResult != DialogResult.OK && DialogResult != DialogResult.Cancel)
                {
                    var result = MessageBox.Show(
                        "You have unsaved changes. Do you want to save them before closing?",
                        "MIDIFlux - Unsaved Changes",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question);

                    switch (result)
                    {
                        case DialogResult.Yes:
                            SaveSettings();
                            break;
                        case DialogResult.Cancel:
                            e.Cancel = true;
                            return;
                    }
                }

                _logger.LogDebug("SettingsForm closing");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during form closing: {Message}", ex.Message);
            }

            base.OnFormClosing(e);
        }
    }
}
