using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using MIDIFlux.Core.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.GUI.Controls.ProfileManager;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Interfaces;
using MIDIFlux.GUI.Services;

namespace MIDIFlux.GUI.Forms
{
    /// <summary>
    /// Configuration form for the MIDIFlux application that provides a tabbed interface for managing profiles and settings
    /// </summary>
    public partial class ConfigurationForm : Form
    {
        private readonly Dictionary<TabPage, ITabControl> _tabControls = new();
        private readonly MidiProcessingServiceProxy _midiProcessingServiceProxy;
        private readonly ILogger<ConfigurationForm> _logger;
        private bool _isClosing = false;
        private MidiDeviceManager? _MidiDeviceManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationForm"/> class
        /// </summary>
        /// <param name="logger">The logger to use for this form</param>
        /// <param name="midiProcessingServiceProxy">The MIDI processing service proxy</param>
        public ConfigurationForm(ILogger<ConfigurationForm> logger, MidiProcessingServiceProxy midiProcessingServiceProxy)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _midiProcessingServiceProxy = midiProcessingServiceProxy ?? throw new ArgumentNullException(nameof(midiProcessingServiceProxy));

            InitializeComponent();

            // Initialize the UI synchronization context
            UISynchronizationHelper.Initialize(SynchronizationContext.Current);

            // Set up the notify icon
            notifyIcon.Icon = Icon;

            // Note: Global exception handling is already set up in Program.cs
            // No need to duplicate it here
        }





        /// <summary>
        /// Gets the MidiProcessingServiceProxy instance
        /// </summary>
        /// <returns>The MidiProcessingServiceProxy instance</returns>
        public MidiProcessingServiceProxy? GetMidiProcessingServiceProxy()
        {
            _logger.LogInformation("GetMidiProcessingServiceProxy called - returning: {Available}", _midiProcessingServiceProxy != null);
            return _midiProcessingServiceProxy;
        }

        /// <summary>
        /// Sets the MidiDeviceManager instance for MIDI event detection
        /// </summary>
        /// <param name="MidiDeviceManager">The MidiDeviceManager instance</param>
        public void SetMidiDeviceManager(MidiDeviceManager MidiDeviceManager)
        {
            _MidiDeviceManager = MidiDeviceManager;
            _logger.LogDebug("MidiDeviceManager set for ConfigurationForm");
        }

        /// <summary>
        /// Gets the MidiDeviceManager instance
        /// </summary>
        /// <returns>The MidiDeviceManager instance, or null if not set</returns>
        public MidiDeviceManager? GetMidiDeviceManager()
        {
            return _MidiDeviceManager;
        }

        /// <summary>
        /// Handles the Load event of the ConfigurationForm
        /// </summary>
        private void MainForm_Load(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _logger.LogDebug("ConfigurationForm is loading");

                // Set up the system tray icon and menu
                SetupSystemTray();

                // Verify that the MidiProcessingServiceProxy is properly initialized
                if (_midiProcessingServiceProxy == null)
                {
                    _logger.LogError("MidiProcessingServiceProxy is null during form load. This will cause initialization issues.");

                    // Show an error message
                    MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                        "Failed to initialize the Configuration GUI properly. Some features may not work correctly.",
                        "MIDIFlux - Initialization Error",
                        _logger,
                        null,
                        this);
                }
                else
                {
                    _logger.LogDebug("MidiProcessingServiceProxy is available: {Available}",
                        _midiProcessingServiceProxy.IsServiceAvailable() ? "Yes" : "No");
                }

                // Load the default profile manager tab
                LoadProfileManagerTab();

                _logger.LogInformation("ConfigurationForm loaded successfully");
            }, _logger, "loading Configuration GUI", this);
        }

        /// <summary>
        /// Sets up the system tray icon and context menu
        /// </summary>
        private void SetupSystemTray()
        {
            // Set the notify icon text
            notifyIcon.Text = "MIDIFlux Configuration";
            notifyIcon.ContextMenuStrip = trayContextMenu;
            notifyIcon.Visible = true;
        }

        /// <summary>
        /// Handles the Opening event of the tray context menu
        /// </summary>
        private void TrayContextMenu_Opening(object? sender, CancelEventArgs e)
        {
            // Clear existing items
            trayContextMenu.Items.Clear();

            // Add "Open Configuration" item
            var openConfigItem = new ToolStripMenuItem("Open Configuration");
            openConfigItem.Click += (s, args) => RestoreFromTray();
            trayContextMenu.Items.Add(openConfigItem);

            trayContextMenu.Items.Add(new ToolStripSeparator());

            // Configuration items from the main application will be added here when needed

            trayContextMenu.Items.Add(new ToolStripSeparator());

            // Add Exit item
            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, args) => Application.Exit();
            trayContextMenu.Items.Add(exitItem);
        }

        /// <summary>
        /// Handles the DoubleClick event of the notify icon
        /// </summary>
        private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            RestoreFromTray();
        }

        /// <summary>
        /// Restores the form from the system tray
        /// </summary>
        private void RestoreFromTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            Activate();
        }

        /// <summary>
        /// Handles the Resize event of the ConfigurationForm
        /// </summary>
        private void MainForm_Resize(object? sender, EventArgs e)
        {
            // Standard minimize behavior - no special handling needed
        }

        /// <summary>
        /// Handles the FormClosing event of the ConfigurationForm
        /// </summary>
        private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Check for unsaved changes
            if (!_isClosing)
            {
                _isClosing = true;

                // Check each tab for unsaved changes
                foreach (var tabControl in _tabControls.Values)
                {
                    if (tabControl.HasUnsavedChanges)
                    {
                        var message = $"Do you want to save changes to '{tabControl.TabTitle}'?";

                        // Use ApplicationErrorHandler to show the unsaved changes dialog and log it
                        var result = MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowUnsavedChangesConfirmation(
                            message,
                            "Unsaved Changes",
                            _logger,
                            this);

                        switch (result)
                        {
                            case DialogResult.Yes:
                                if (!tabControl.Save())
                                {
                                    e.Cancel = true;
                                    _isClosing = false;
                                    return;
                                }
                                break;
                            case DialogResult.Cancel:
                                e.Cancel = true;
                                _isClosing = false;
                                return;
                        }
                    }
                }
            }

            // Clean up the notify icon
            notifyIcon.Visible = false;
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the tab control
        /// </summary>
        private void TabControl_SelectedIndexChanged(object? sender, EventArgs e)
        {
            // Deactivate all tabs
            foreach (var tabControl in _tabControls.Values)
            {
                tabControl.Deactivate();
            }

            // Activate the selected tab
            if (tabControl.SelectedTab != null && _tabControls.TryGetValue(tabControl.SelectedTab, out var selectedTabControl))
            {
                selectedTabControl.Activate();
                UpdateStatusBar($"Current tab: {selectedTabControl.TabTitle}");
            }
        }

        /// <summary>
        /// Updates the status bar text
        /// </summary>
        /// <param name="text">The text to display</param>
        public void UpdateStatusBar(string text)
        {
            statusLabel.Text = text;
        }

        /// <summary>
        /// Adds a tab to the tab control
        /// </summary>
        /// <param name="tabControl">The tab control to add</param>
        public void AddTab(ITabControl tabControl)
        {
            if (tabControl is Control control)
            {
                // Create a new tab page
                var tabPage = new TabPage(tabControl.TabTitle);

                // Add the control to the tab page
                control.Dock = DockStyle.Fill;
                tabPage.Controls.Add(control);

                // Add the tab page to the tab control
                this.tabControl.TabPages.Add(tabPage);

                // Store the tab control
                _tabControls[tabPage] = tabControl;

                // Set up event handlers
                tabControl.TabTitleChanged += TabControl_TabTitleChanged;
                tabControl.UnsavedChangesChanged += TabControl_UnsavedChangesChanged;

                // Activate the tab
                this.tabControl.SelectedTab = tabPage;

                // Update the status bar
                UpdateStatusBar($"Added tab: {tabControl.TabTitle}");
            }
        }

        /// <summary>
        /// Adds a profile editor tab or activates existing one if it already exists
        /// </summary>
        /// <param name="profileEditorControl">The profile editor control to add</param>
        /// <returns>True if a new tab was created, false if an existing tab was activated</returns>
        public bool AddOrActivateProfileEditorTab(Controls.ProfileEditor.ProfileEditorControl profileEditorControl)
        {
            // Check if a tab for this profile already exists
            var existingTab = FindProfileEditorTab(profileEditorControl.Profile.FilePath);
            if (existingTab != null)
            {
                // Activate the existing tab
                this.tabControl.SelectedTab = existingTab;
                UpdateStatusBar($"Activated existing tab: {profileEditorControl.TabTitle}");
                return false;
            }

            // No existing tab found, add a new one
            AddTab(profileEditorControl);
            return true;
        }

        /// <summary>
        /// Finds an existing profile editor tab for the specified profile file path
        /// </summary>
        /// <param name="profileFilePath">The file path of the profile to find</param>
        /// <returns>The tab page if found, null otherwise</returns>
        private TabPage? FindProfileEditorTab(string profileFilePath)
        {
            foreach (var kvp in _tabControls)
            {
                if (kvp.Value is Controls.ProfileEditor.ProfileEditorControl profileEditor)
                {
                    if (string.Equals(profileEditor.Profile.FilePath, profileFilePath, StringComparison.OrdinalIgnoreCase))
                    {
                        return kvp.Key;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Removes a tab from the tab control
        /// </summary>
        /// <param name="tabControl">The tab control to remove</param>
        public void RemoveTab(ITabControl tabControl)
        {
            // Find the tab page for this tab control
            var tabPage = _tabControls.FirstOrDefault(x => x.Value == tabControl).Key;

            if (tabPage != null)
            {
                // Remove event handlers
                tabControl.TabTitleChanged -= TabControl_TabTitleChanged;
                tabControl.UnsavedChangesChanged -= TabControl_UnsavedChangesChanged;

                // Remove the tab page
                this.tabControl.TabPages.Remove(tabPage);

                // Remove from the dictionary
                _tabControls.Remove(tabPage);

                // Update the status bar
                UpdateStatusBar($"Removed tab: {tabControl.TabTitle}");
            }
        }

        /// <summary>
        /// Activates a tab in the tab control
        /// </summary>
        /// <param name="tabControl">The tab control to activate</param>
        public void ActivateTab(ITabControl tabControl)
        {
            // Find the tab page for this tab control
            var tabPage = _tabControls.FirstOrDefault(x => x.Value == tabControl).Key;

            if (tabPage != null)
            {
                // Select the tab
                this.tabControl.SelectedTab = tabPage;
            }
        }

        /// <summary>
        /// Handles the TabTitleChanged event of a tab control
        /// </summary>
        private void TabControl_TabTitleChanged(object? sender, EventArgs e)
        {
            if (sender is ITabControl tabControl)
            {
                // Find the tab page for this tab control
                var tabPage = _tabControls.FirstOrDefault(x => x.Value == tabControl).Key;

                if (tabPage != null)
                {
                    // Update the tab title
                    tabPage.Text = tabControl.TabTitle;
                }
            }
        }

        /// <summary>
        /// Handles the UnsavedChangesChanged event of a tab control
        /// </summary>
        private void TabControl_UnsavedChangesChanged(object? sender, EventArgs e)
        {
            if (sender is ITabControl tabControl)
            {
                // Find the tab page for this tab control
                var tabPage = _tabControls.FirstOrDefault(x => x.Value == tabControl).Key;

                if (tabPage != null)
                {
                    // Update the tab title to indicate unsaved changes
                    tabPage.Text = tabControl.HasUnsavedChanges ? $"{tabControl.TabTitle} *" : tabControl.TabTitle;
                }
            }
        }

        /// <summary>
        /// Loads the profile manager tab
        /// </summary>
        private void LoadProfileManagerTab()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _logger.LogDebug("Loading Profile Manager tab");

                // Create a new profile manager control
                _logger.LogDebug("Creating new ProfileManagerControl instance");
                var profileManagerLogger = LoggingHelper.CreateLogger<ProfileManagerControl>();
                var configurationService = new ConfigurationService(LoggingHelper.CreateLogger<ConfigurationService>());
                var actionConfigurationLoader = new ActionConfigurationLoader(LoggingHelper.CreateLogger<ActionConfigurationLoader>(), configurationService);
                var profileManagerControl = new ProfileManagerControl(profileManagerLogger, configurationService, actionConfigurationLoader);
                _logger.LogDebug("ProfileManagerControl instance created successfully");

                // Add it as a tab
                AddTab(profileManagerControl);

                _logger.LogDebug("Profile Manager tab loaded successfully");

                // Try to show a basic message in the status bar
                try
                {
                    UpdateStatusBar("Profile Manager tab loaded successfully.");
                }
                catch
                {
                    // Ignore any errors in the status bar update
                }
            }, _logger, "loading Profile Manager tab", this);
        }
    }
}

