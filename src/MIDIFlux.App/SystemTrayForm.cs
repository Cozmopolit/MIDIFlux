using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using MIDIFlux.App.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MIDIFlux.GUI.Forms;
using MIDIFlux.GUI.Dialogs;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Services;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Configuration;

namespace MIDIFlux.App;

/// <summary>
/// Main application form that manages the system tray icon and provides access to MIDIFlux functionality
/// </summary>
public partial class SystemTrayForm : Form
{
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _contextMenu;
    private readonly MidiProcessingService _midiProcessingService;
    private readonly IHost _host;
    private readonly ILogger<SystemTrayForm> _logger;
    private readonly IConfiguration _configuration;
    private MIDIFlux.GUI.Dialogs.MidiInputDetectionDialog? _midiInputDetectionDialog;
    private bool _isExiting;

    public SystemTrayForm(IHost host)
    {
        InitializeComponent();
        _host = host;
        _logger = host.Services.GetRequiredService<ILogger<SystemTrayForm>>();
        _configuration = host.Services.GetRequiredService<IConfiguration>();
        _midiProcessingService = host.Services.GetRequiredService<MidiProcessingService>();

        // Create the notify icon
        _notifyIcon = new NotifyIcon
        {
            Icon = IconHelper.GetApplicationIconOrDefault(),
            Text = "MIDIFlux",
            Visible = true
        };

        // Create the context menu
        _contextMenu = new ContextMenuStrip();

        // Add event handler for context menu opening
        _contextMenu.Opening += ContextMenu_Opening;
        _notifyIcon.ContextMenuStrip = _contextMenu;

        // Add "Configure Mapping Profiles" menu item (most important - first)
        var configureMenuItem = new ToolStripMenuItem("Configure Mapping Profiles");
        configureMenuItem.Click += ConfigureMenuItem_Click;
        _contextMenu.Items.Add(configureMenuItem);

        // Add "Settings" menu item
        var settingsMenuItem = new ToolStripMenuItem("Settings");
        settingsMenuItem.Click += SettingsMenuItem_Click;
        _contextMenu.Items.Add(settingsMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Add "Configurations" menu item with hierarchical structure
        var configurationsMenuItem = new ToolStripMenuItem("Configurations");
        _contextMenu.Items.Add(configurationsMenuItem);

        // Get the AppData profiles directory
        string profilesDir = AppDataHelper.GetProfilesDirectory();

        // Ensure all directories exist
        AppDataHelper.EnsureDirectoriesExist(_host.Services.GetRequiredService<ILogger<SystemTrayForm>>());

        // Add default configuration
        var defaultConfigPath = Path.Combine(profilesDir, "default.json");
        if (File.Exists(defaultConfigPath))
        {
            var defaultConfigMenuItem = new ToolStripMenuItem("Default");
            defaultConfigMenuItem.Tag = defaultConfigPath;
            defaultConfigMenuItem.Click += ConfigMenuItem_Click;
            configurationsMenuItem.DropDownItems.Add(defaultConfigMenuItem);
        }

        // Recursively add configurations from the profiles directory and its subdirectories
        if (Directory.Exists(profilesDir))
        {
            // Add configurations from the root profiles directory first
            AddConfigurationsFromDirectory(profilesDir, configurationsMenuItem, rootDirectory: true);

            // Then add configurations from subdirectories
            foreach (var subDir in Directory.GetDirectories(profilesDir))
            {
                string dirName = Path.GetFileName(subDir);
                var subMenuItem = new ToolStripMenuItem(dirName);
                configurationsMenuItem.DropDownItems.Add(subMenuItem);

                AddConfigurationsFromDirectory(subDir, subMenuItem, rootDirectory: false);
            }
        }

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Add "MIDI Diagnostics" menu item
        var midiDiagnosticsMenuItem = new ToolStripMenuItem("MIDI Diagnostics");
        midiDiagnosticsMenuItem.Click += MidiDiagnosticsMenuItem_Click;
        _contextMenu.Items.Add(midiDiagnosticsMenuItem);

        // Add "MIDI Input Detection" menu item
        var midiInputDetectionMenuItem = new ToolStripMenuItem("MIDI Input Detection");
        midiInputDetectionMenuItem.Click += MidiInputDetectionMenuItem_Click;
        _contextMenu.Items.Add(midiInputDetectionMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Add "Logging" menu item with simplified options
        var loggingMenuItem = new ToolStripMenuItem("Logging");
        _contextMenu.Items.Add(loggingMenuItem);

        // Add "Open Log Viewer" option
        var openLogViewerMenuItem = new ToolStripMenuItem("Open Log Viewer");
        openLogViewerMenuItem.Click += OpenLogViewerMenuItem_Click;
        loggingMenuItem.DropDownItems.Add(openLogViewerMenuItem);

        // Add "Silent Mode" option
        var silentModeMenuItem = new ToolStripMenuItem("Silent Mode (No Popups)");
        silentModeMenuItem.CheckOnClick = true;
        silentModeMenuItem.Click += SilentModeMenuItem_Click;
        loggingMenuItem.DropDownItems.Add(silentModeMenuItem);

        // Set the initial checked state for silent mode
        UpdateSilentModeMenuState();

        _contextMenu.Items.Add(new ToolStripSeparator());

        var exitMenuItem = new ToolStripMenuItem("Exit");
        exitMenuItem.Click += ExitMenuItem_Click;
        _contextMenu.Items.Add(exitMenuItem);

        // Handle double-click on the notify icon
        _notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

        // Subscribe to service events
        _midiProcessingService.StatusChanged += MidiProcessingService_StatusChanged;
        _midiProcessingService.ConfigurationChanged += MidiProcessingService_ConfigurationChanged;

        // Initialize silent mode from configuration
        var configurationService = _host.Services.GetRequiredService<ConfigurationService>();
        ApplicationErrorHandler.SilentMode = configurationService.GetSetting("Application.SilentMode", false);

        // Check if there's already an active configuration and update the menu accordingly
        var activeConfigPath = _midiProcessingService.ActiveConfigurationPath;
        if (!string.IsNullOrEmpty(activeConfigPath))
        {
            _logger.LogDebug("Found active configuration on startup: {ConfigPath}", activeConfigPath);
            // Update the menu to reflect the active configuration
            // We need to do this after the menu is built, so we'll defer it
            Load += (sender, e) => {
                SystemTrayForm_Load(sender, e);
                // Update the menu state for the active configuration
                UpdateActiveConfigurationInMenu(activeConfigPath);
            };
        }
        else
        {
            // Hide the form when it's loaded
            Load += SystemTrayForm_Load;
        }
    }

    private void SystemTrayForm_Load(object? sender, EventArgs e)
    {
        Hide();
        ShowInTaskbar = false;
    }

    private void NotifyIcon_DoubleClick(object? sender, EventArgs e)
    {
        // Toggle MIDI processing
        if (_midiProcessingService.IsRunning)
        {
            _midiProcessingService.Stop();
        }
        else
        {
            _midiProcessingService.Start();
        }
    }

    private void MidiDiagnosticsMenuItem_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            _logger.LogInformation("Running MIDI diagnostic from system tray...");

            // Create a MidiProcessingServiceProxy to access the diagnostic functionality
            var proxyLogger = LoggingHelper.CreateLogger<MidiProcessingServiceProxy>();
            var serviceProxy = new MidiProcessingServiceProxy(proxyLogger);

            // Get the MidiDeviceManager for proxy setup
            var MidiDeviceManager = _host.Services.GetRequiredService<MidiDeviceManager>();

            // Set up the proxy with the current service instance
            if (_midiProcessingService != null && MidiDeviceManager != null)
            {
                _logger.LogDebug("Setting up MidiProcessingServiceProxy for diagnostic with service functions");

                serviceProxy.SetServiceFunctions(
                    _midiProcessingService!.LoadConfiguration,
                    () => _midiProcessingService!.ActiveConfigurationPath,
                    _midiProcessingService!.Start,
                    _midiProcessingService!.Stop,
                    MidiDeviceManager!.GetAvailableDevices,
                    () => MidiDeviceManager!,
                    _midiProcessingService!.GetProcessorStatistics);

                _logger.LogDebug("MidiProcessingServiceProxy setup completed for diagnostic");
            }
            else
            {
                _logger.LogError("Failed to set up MidiProcessingServiceProxy for diagnostic - missing required services: MidiService={MidiServiceAvailable}, MidiDeviceManager={MidiDeviceManagerAvailable}",
                    _midiProcessingService != null, MidiDeviceManager != null);
            }

            // Generate and display diagnostic report
            string report = MIDIFlux.GUI.Helpers.MidiDiagnosticHelper.GenerateDiagnosticReport(serviceProxy, _logger);

            // Show the report using centralized error handling
            ApplicationErrorHandler.ShowInformation(report, "MIDI Diagnostic Report", _logger);

            // Also log the report for debugging
            MIDIFlux.GUI.Helpers.MidiDiagnosticHelper.LogDiagnosticReport(serviceProxy, _logger);

            _logger.LogInformation("MIDI diagnostic completed from system tray");
        }, _logger, "running MIDI diagnostic from system tray");
    }

    private void MidiInputDetectionMenuItem_Click(object? sender, EventArgs e)
    {
        try
        {
            _logger.LogDebug("MIDI Input Detection menu item clicked");

            // Check if the dialog is already open
            if (_midiInputDetectionDialog != null && !_midiInputDetectionDialog.IsDisposed)
            {
                // If the dialog is already open, bring it to the front
                _midiInputDetectionDialog.WindowState = FormWindowState.Normal;
                _midiInputDetectionDialog.Activate();
                _logger.LogDebug("Activated existing MIDI Input Detection dialog");
                return;
            }

            // Get the required services
            var MidiDeviceManager = _host.Services.GetRequiredService<MidiDeviceManager>();
            var dialogLogger = LoggingHelper.CreateLogger<MIDIFlux.GUI.Dialogs.MidiInputDetectionDialog>();

            // Create the dialog
            _midiInputDetectionDialog = new MIDIFlux.GUI.Dialogs.MidiInputDetectionDialog(dialogLogger, MidiDeviceManager);

            // Handle dialog disposal when it's closed
            _midiInputDetectionDialog.FormClosed += (s, args) =>
            {
                _midiInputDetectionDialog?.Dispose();
                _midiInputDetectionDialog = null;
                _logger.LogDebug("MIDI Input Detection dialog closed and disposed");
            };

            // Show the dialog as non-modal
            _midiInputDetectionDialog.Show();
            _logger.LogDebug("Opened MIDI Input Detection dialog (non-modal)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error showing MIDI Input Detection dialog: {Message}", ex.Message);
            ApplicationErrorHandler.ShowError(
                "An error occurred while opening the MIDI Input Detection dialog.",
                "MIDIFlux - Error",
                _logger,
                ex);
        }
    }



    private void ConfigMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string configPath)
        {
            _midiProcessingService.LoadConfiguration(configPath);
        }
    }

    private void ExitMenuItem_Click(object? sender, EventArgs e)
    {
        // Set the exiting flag so OnFormClosing allows the close
        _isExiting = true;

        // Stop the host
        _host.StopAsync().Wait();

        // Clean up the notify icon
        _notifyIcon.Visible = false;

        // Close the application
        Application.Exit();
    }

    private void MidiProcessingService_StatusChanged(object? sender, bool isRunning)
    {
        // Update the notify icon text
        _notifyIcon.Text = $"MIDIFlux - {(isRunning ? "Running" : "Stopped")}";
    }

    private void MidiProcessingService_ConfigurationChanged(object? sender, string configPath)
    {
        // Update the Configurations menu items
        UpdateActiveConfigurationInMenu(configPath);
    }

    /// <summary>
    /// Updates the active configuration in the system tray menu
    /// </summary>
    /// <param name="configPath">The path of the active configuration</param>
    private void UpdateActiveConfigurationInMenu(string configPath)
    {
        try
        {
            // Find the Configurations menu item (should be at index 3)
            if (_contextMenu.Items.Count > 3 && _contextMenu.Items[3] is ToolStripMenuItem configurationsMenuItem)
            {
                UpdateMenuItemCheckedState(configurationsMenuItem, configPath);
                _logger.LogDebug("Updated system tray menu for active configuration: {ConfigPath}", configPath);
            }
            else
            {
                _logger.LogWarning("Could not find Configurations menu item to update");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating active configuration in menu: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Handles the click event of the Configure menu item
    /// </summary>
    private void ConfigureMenuItem_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() => {_logger.LogDebug("Configure menu item clicked");

            // Check if the configuration form is already open
            var existingForm = Application.OpenForms.OfType<ConfigurationForm>().FirstOrDefault();
            if (existingForm != null)
            {
                // If the form is already open, bring it to the front
                existingForm.WindowState = FormWindowState.Normal;
                existingForm.Activate();
                _logger.LogInformation("Activated existing Configuration GUI");
                return;
            }

            _logger.LogDebug("Creating new ConfigurationForm instance");

            // Create the MidiProcessingServiceProxy first
            var proxyLogger = LoggingHelper.CreateLogger<MidiProcessingServiceProxy>();
            var midiProcessingServiceProxy = new MidiProcessingServiceProxy(proxyLogger);

            // Create a new instance of the configuration form with required dependencies
            var configFormLogger = LoggingHelper.CreateLogger<ConfigurationForm>();
            var configForm = new ConfigurationForm(configFormLogger, midiProcessingServiceProxy);

            // Get the MidiDeviceManager for both proxy setup and MIDI event detection
            var MidiDeviceManager = _host.Services.GetRequiredService<MidiDeviceManager>();

            // Set up the proxy with the current service instance
            if (_midiProcessingService != null && MidiDeviceManager != null)
            {
                _logger.LogInformation("Setting up MidiProcessingServiceProxy with service functions");
                _logger.LogInformation("MidiProcessingService available: {Available}", _midiProcessingService != null);
                _logger.LogInformation("MidiDeviceManager available: {Available}", MidiDeviceManager != null);

                midiProcessingServiceProxy.SetServiceFunctions(
                    _midiProcessingService!.LoadConfiguration,
                    () => _midiProcessingService!.ActiveConfigurationPath,
                    _midiProcessingService!.Start,
                    _midiProcessingService!.Stop,
                    MidiDeviceManager!.GetAvailableDevices,
                    () => MidiDeviceManager!,
                    _midiProcessingService!.GetProcessorStatistics);

                _logger.LogInformation("MidiProcessingServiceProxy setup completed");
            }
            else
            {
                _logger.LogError("Failed to set up MidiProcessingServiceProxy - missing required services: MidiService={MidiServiceAvailable}, MidiDeviceManager={MidiDeviceManagerAvailable}",
                    _midiProcessingService != null, MidiDeviceManager != null);
            }

            // Set the MidiDeviceManager for MIDI event detection
            if (MidiDeviceManager != null)
            {
                configForm.SetMidiDeviceManager(MidiDeviceManager);
            }

            // Show the form
            configForm.Show();
            _logger.LogInformation("Opened Configuration GUI");
        }, _logger, "opening configuration GUI");
    }

    /// <summary>
    /// Handles the click event of the Settings menu item
    /// </summary>
    private void SettingsMenuItem_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            _logger.LogInformation("Opening Settings dialog");

            // Create and show the settings form
            var configurationService = _host.Services.GetRequiredService<ConfigurationService>();
            using var settingsForm = new MIDIFlux.GUI.Forms.SettingsForm(configurationService);
            var result = settingsForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                _logger.LogInformation("Settings dialog closed with OK result");
            }
            else
            {
                _logger.LogDebug("Settings dialog closed with {Result} result", result);
            }
        }, _logger, "opening settings dialog");
    }

    /// <summary>
    /// Recursively updates the checked state of menu items
    /// </summary>
    /// <param name="menuItem">The parent menu item</param>
    /// <param name="configPath">The path of the active configuration</param>
    private void UpdateMenuItemCheckedState(ToolStripMenuItem menuItem, string configPath)
    {
        // Check direct child items
        foreach (var item in menuItem.DropDownItems)
        {
            if (item is ToolStripMenuItem childMenuItem)
            {
                if (childMenuItem.Tag is string path && path == configPath)
                {
                    childMenuItem.Checked = true;
                }
                else
                {
                    childMenuItem.Checked = false;

                    // Recursively check submenus
                    if (childMenuItem.DropDownItems.Count > 0)
                    {
                        UpdateMenuItemCheckedState(childMenuItem, configPath);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Adds configuration files from a directory to a menu item
    /// </summary>
    /// <param name="directoryPath">The directory to scan for configuration files</param>
    /// <param name="parentMenuItem">The menu item to add configurations to</param>
    /// <param name="rootDirectory">Whether this is the root profiles directory</param>
    /// <returns>True if any configurations were added, false otherwise</returns>
    private bool AddConfigurationsFromDirectory(string directoryPath, ToolStripMenuItem parentMenuItem, bool rootDirectory)
    {
        bool result = false;
        bool success = ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            // Get all JSON files in the directory
            var configFiles = Directory.GetFiles(directoryPath, "*.json")
                .Where(file => !Path.GetFileName(file).Equals("current.json", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // If this is the root directory, exclude default.json as it's already added
            if (rootDirectory)
            {
                configFiles = configFiles
                    .Where(file => !Path.GetFileName(file).Equals("default.json", StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort them alphabetically
            configFiles.Sort();

            // Add them to the menu
            foreach (var configFile in configFiles)
            {
                var configName = Path.GetFileNameWithoutExtension(configFile);
                var configMenuItem = new ToolStripMenuItem(configName);
                configMenuItem.Tag = configFile;
                configMenuItem.Click += ConfigMenuItem_Click;
                parentMenuItem.DropDownItems.Add(configMenuItem);
            }

            // We don't recursively add subdirectories here anymore
            // This is now handled in the main code

            // Set the result - true if any configurations were added
            result = parentMenuItem.DropDownItems.Count > 0;
        }, _logger, "loading configurations");

        // Return false if the operation failed, otherwise return the actual result
        return success && result;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Allow closing when the user clicked Exit in the tray menu
        if (_isExiting)
        {
            base.OnFormClosing(e);
            return;
        }

        // Otherwise prevent the form from closing, just hide it (system tray behavior)
        if (!e.Cancel)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnFormClosing(e);
    }



    /// <summary>
    /// Updates the checked state of the silent mode menu item
    /// </summary>
    private void UpdateSilentModeMenuState()
    {
        try
        {
            // Get the Logging menu item
            var loggingMenuItem = _contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(i => i.Text == "Logging");

            if (loggingMenuItem == null)
            {
                _logger.LogWarning("Logging menu item not found");
                return;
            }

            // Update the checked state of the silent mode menu item
            var silentModeMenuItem = loggingMenuItem.DropDownItems
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(i => i.Text == "Silent Mode (No Popups)");

            if (silentModeMenuItem != null)
            {
                silentModeMenuItem.Checked = ApplicationErrorHandler.SilentMode;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating silent mode menu state: {Message}", ex.Message);
        }
    }



    /// <summary>
    /// Handles clicks on the Silent Mode menu item
    /// </summary>
    private void SilentModeMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem)
        {
            try
            {
                // Toggle silent mode
                ApplicationErrorHandler.SilentMode = menuItem.Checked;

                // Save to configuration
                var configurationService = _host.Services.GetRequiredService<ConfigurationService>();
                configurationService.UpdateSetting("Application.SilentMode", menuItem.Checked);

                _logger.LogInformation("Silent Mode set to {SilentMode}", menuItem.Checked);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing silent mode: {Message}", ex.Message);
            }
        }
    }

    /// <summary>
    /// Handles clicks on the Open Log Viewer menu item
    /// </summary>
    private void OpenLogViewerMenuItem_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            _logger.LogInformation("Opening log viewer");

            // Open the log viewer
            bool success = MIDIFlux.GUI.Helpers.LogViewerHelper.OpenLogViewer(_logger);

            if (!success)
            {
                _logger.LogWarning("Failed to open log viewer");

                // Use ApplicationErrorHandler to show the error (it will respect silent mode)
                ApplicationErrorHandler.ShowError(
                    "Failed to open log viewer. Please check the application logs for more information.",
                    "MIDIFlux - Error",
                    _logger);
            }
        }, _logger, "opening log viewer");
    }

    /// <summary>
    /// Refreshes the configurations menu with current profiles from the file system
    /// </summary>
    private void RefreshConfigurationsMenu()
    {
        try
        {
            // Find the Configurations menu item (should be at index 3)
            if (_contextMenu.Items.Count > 3 && _contextMenu.Items[3] is ToolStripMenuItem configurationsMenuItem)
            {
                // Clear existing configuration items
                configurationsMenuItem.DropDownItems.Clear();

                // Get the AppData profiles directory
                string profilesDir = AppDataHelper.GetProfilesDirectory();

                // Add default configuration
                var defaultConfigPath = Path.Combine(profilesDir, "default.json");
                if (File.Exists(defaultConfigPath))
                {
                    var defaultConfigMenuItem = new ToolStripMenuItem("Default");
                    defaultConfigMenuItem.Tag = defaultConfigPath;
                    defaultConfigMenuItem.Click += ConfigMenuItem_Click;
                    configurationsMenuItem.DropDownItems.Add(defaultConfigMenuItem);
                }

                // Recursively add configurations from the profiles directory and its subdirectories
                if (Directory.Exists(profilesDir))
                {
                    // Add configurations from the root profiles directory first
                    AddConfigurationsFromDirectory(profilesDir, configurationsMenuItem, rootDirectory: true);

                    // Then add configurations from subdirectories
                    foreach (var subDir in Directory.GetDirectories(profilesDir))
                    {
                        string dirName = Path.GetFileName(subDir);
                        var subMenuItem = new ToolStripMenuItem(dirName);
                        configurationsMenuItem.DropDownItems.Add(subMenuItem);

                        AddConfigurationsFromDirectory(subDir, subMenuItem, rootDirectory: false);
                    }
                }

                // Update the active configuration state
                var activeConfigPath = _midiProcessingService.ActiveConfigurationPath;
                if (!string.IsNullOrEmpty(activeConfigPath))
                {
                    UpdateActiveConfigurationInMenu(activeConfigPath);
                }

                _logger.LogDebug("Refreshed configurations menu with current profiles");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing configurations menu: {Message}", ex.Message);
        }
    }

    /// <summary>
    /// Handles the context menu opening event
    /// </summary>
    private void ContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        try
        {
            // Update the silent mode menu state
            UpdateSilentModeMenuState();

            // Refresh the configurations menu dynamically
            RefreshConfigurationsMenu();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling context menu opening: {Message}", ex.Message);
        }
    }

}
