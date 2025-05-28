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
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath),
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

        // Add "Show MIDI Devices" menu item
        var showDevicesMenuItem = new ToolStripMenuItem("Show MIDI Devices");
        showDevicesMenuItem.Click += ShowDevicesMenuItem_Click;
        _contextMenu.Items.Add(showDevicesMenuItem);

        // Add "MIDI Input Detection" menu item
        var midiInputDetectionMenuItem = new ToolStripMenuItem("MIDI Input Detection");
        midiInputDetectionMenuItem.Click += MidiInputDetectionMenuItem_Click;
        _contextMenu.Items.Add(midiInputDetectionMenuItem);

        _contextMenu.Items.Add(new ToolStripSeparator());

        // Add "Logging" menu item with options
        var loggingMenuItem = new ToolStripMenuItem("Logging");
        _contextMenu.Items.Add(loggingMenuItem);

        // Add logging level options
        var loggingLevelMenuItem = new ToolStripMenuItem("Logging Level");
        loggingMenuItem.DropDownItems.Add(loggingLevelMenuItem);

        var noneMenuItem = new ToolStripMenuItem("None (Off)");
        noneMenuItem.Tag = "None";
        noneMenuItem.Click += LoggingLevelMenuItem_Click;
        loggingLevelMenuItem.DropDownItems.Add(noneMenuItem);

        var debugMenuItem = new ToolStripMenuItem("Debug");
        debugMenuItem.Tag = "Debug";
        debugMenuItem.Click += LoggingLevelMenuItem_Click;
        loggingLevelMenuItem.DropDownItems.Add(debugMenuItem);

        var infoMenuItem = new ToolStripMenuItem("Information");
        infoMenuItem.Tag = "Information";
        infoMenuItem.Click += LoggingLevelMenuItem_Click;
        loggingLevelMenuItem.DropDownItems.Add(infoMenuItem);

        var warningMenuItem = new ToolStripMenuItem("Warning");
        warningMenuItem.Tag = "Warning";
        warningMenuItem.Click += LoggingLevelMenuItem_Click;
        loggingLevelMenuItem.DropDownItems.Add(warningMenuItem);

        var errorMenuItem = new ToolStripMenuItem("Error");
        errorMenuItem.Tag = "Error";
        errorMenuItem.Click += LoggingLevelMenuItem_Click;
        loggingLevelMenuItem.DropDownItems.Add(errorMenuItem);

        // Add separator
        loggingMenuItem.DropDownItems.Add(new ToolStripSeparator());

        // Add "Open Log Viewer" option
        var openLogViewerMenuItem = new ToolStripMenuItem("Open Log Viewer");
        openLogViewerMenuItem.Click += OpenLogViewerMenuItem_Click;
        loggingMenuItem.DropDownItems.Add(openLogViewerMenuItem);

        // Add "Silent Mode" option
        var silentModeMenuItem = new ToolStripMenuItem("Silent Mode (No Popups)");
        silentModeMenuItem.CheckOnClick = true;
        silentModeMenuItem.Click += SilentModeMenuItem_Click;
        loggingMenuItem.DropDownItems.Add(silentModeMenuItem);

        // Set the initial checked state based on current configuration
        UpdateLoggingMenuCheckedState();

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

    private void ShowDevicesMenuItem_Click(object? sender, EventArgs e)
    {
        // Get the MIDI manager from the service provider
        var midiManager = _host.Services.GetRequiredService<MidiManager>();
        var devices = midiManager.GetAvailableDevices();

        // Build the message
        var message = new System.Text.StringBuilder();
        message.AppendLine("Available MIDI Input Devices:");

        if (devices.Count > 0)
        {
            foreach (var device in devices)
            {
                message.AppendLine($" - {device}");
            }
        }
        else
        {
            message.AppendLine(" - No MIDI devices found");
        }

        // Show the message box using ApplicationErrorHandler
        ApplicationErrorHandler.ShowInformation(
            message.ToString(),
            "MIDIFlux - Device List",
            _logger);
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
            var midiManager = _host.Services.GetRequiredService<MidiManager>();
            var dialogLogger = _host.Services.GetRequiredService<ILogger<MIDIFlux.GUI.Dialogs.MidiInputDetectionDialog>>();

            // Create the dialog
            _midiInputDetectionDialog = new MIDIFlux.GUI.Dialogs.MidiInputDetectionDialog(dialogLogger, midiManager);

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

            // Create a new instance of the configuration form
            var configForm = new ConfigurationForm();

            // Get the MidiProcessingServiceProxy from the form
            var midiProcessingServiceProxy = configForm.GetMidiProcessingServiceProxy();

            // Get the MidiManager for both proxy setup and MIDI event detection
            var midiManager = _host.Services.GetRequiredService<MidiManager>();

            // Set up the proxy with the current service instance
            if (midiProcessingServiceProxy != null && _midiProcessingService != null && midiManager != null)
            {
                _logger.LogInformation("Setting up MidiProcessingServiceProxy with service functions");
                _logger.LogInformation("MidiProcessingService available: {Available}", _midiProcessingService != null);
                _logger.LogInformation("MidiManager available: {Available}", midiManager != null);

                midiProcessingServiceProxy.SetServiceFunctions(
                    _midiProcessingService!.LoadConfiguration,
                    () => _midiProcessingService!.ActiveConfigurationPath,
                    _midiProcessingService!.Start,
                    _midiProcessingService!.Stop,
                    midiManager!.GetAvailableDevices,
                    () => midiManager!);

                _logger.LogInformation("MidiProcessingServiceProxy setup completed");
            }
            else
            {
                _logger.LogError("Failed to set up MidiProcessingServiceProxy - missing required services: Proxy={ProxyAvailable}, MidiService={MidiServiceAvailable}, MidiManager={MidiManagerAvailable}",
                    midiProcessingServiceProxy != null, _midiProcessingService != null, midiManager != null);
            }

            // Set the MidiManager for MIDI event detection
            if (midiManager != null)
            {
                configForm.SetMidiManager(midiManager);
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

    protected override void OnClosing(CancelEventArgs e)
    {
        // Prevent the form from closing, just hide it
        if (!e.Cancel)
        {
            e.Cancel = true;
            Hide();
        }

        base.OnClosing(e);
    }

    /// <summary>
    /// Handles clicks on logging level menu items
    /// </summary>
    private void LoggingLevelMenuItem_Click(object? sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem && menuItem.Tag is string logLevel)
        {
            try
            {
                _logger.LogInformation("Changing logging level to {LogLevel}", logLevel);

                // Use the unified configuration service
                var configurationService = _host.Services.GetRequiredService<ConfigurationService>();
                var success = configurationService.UpdateSetting("Logging.LogLevel", logLevel);

                if (!success)
                {
                    _logger.LogError("Failed to update logging level in application settings");
                    return;
                }

                // Update the checked state of the menu items
                UpdateLoggingMenuCheckedState();

                // Show a notification
                _notifyIcon.BalloonTipTitle = "MIDIFlux - Logging Level Changed";
                _notifyIcon.BalloonTipText = $"Logging level has been set to {logLevel}";
                _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(3000);

                // Note: The logging level change will take effect the next time the application is started
                // We could reload the configuration, but that would require restarting all services
                _logger.LogInformation("Logging level changed to {LogLevel}. Changes will take effect on next application start.", logLevel);
            }
            catch (Exception ex)
            {
                // Use ApplicationErrorHandler to show the error and log it
                ApplicationErrorHandler.ShowError(
                    "Error changing logging level: " + ex.Message,
                    "MIDIFlux - Error",
                    _logger,
                    ex);
            }
        }
    }

    /// <summary>
    /// Updates the checked state of the logging level menu items based on the current configuration
    /// </summary>
    private void UpdateLoggingMenuCheckedState()
    {
        try
        {
            // Get the current logging level from the configuration
            string currentLogLevel = _configuration.GetValue<string>("Logging:LogLevel:Default") ?? "None";

            // Get the Logging menu item
            var loggingMenuItem = _contextMenu.Items
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(i => i.Text == "Logging");

            if (loggingMenuItem == null)
            {
                _logger.LogWarning("Logging menu item not found");
                return;
            }

            // Get the Logging Level submenu
            var loggingLevelMenuItem = loggingMenuItem.DropDownItems
                .OfType<ToolStripMenuItem>()
                .FirstOrDefault(i => i.Text == "Logging Level");

            if (loggingLevelMenuItem == null)
            {
                _logger.LogWarning("Logging Level submenu not found");
                return;
            }

            // Update the checked state of each logging level menu item
            foreach (ToolStripMenuItem item in loggingLevelMenuItem.DropDownItems.OfType<ToolStripMenuItem>())
            {
                if (item.Tag is string logLevel)
                {
                    item.Checked = string.Equals(logLevel, currentLogLevel, StringComparison.OrdinalIgnoreCase);
                }
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
            _logger.LogError(ex, "Error updating logging menu checked state: {Message}", ex.Message);
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

                // Show a notification
                _notifyIcon.BalloonTipTitle = "MIDIFlux - Silent Mode Changed";
                _notifyIcon.BalloonTipText = menuItem.Checked
                    ? "Silent Mode enabled. No popup messages will be shown."
                    : "Silent Mode disabled. Popup messages will be shown.";
                _notifyIcon.BalloonTipIcon = ToolTipIcon.Info;
                _notifyIcon.ShowBalloonTip(3000);
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
    /// Handles the context menu opening event
    /// </summary>
    private void ContextMenu_Opening(object? sender, CancelEventArgs e)
    {
        try
        {
            // Update the logging level menu items
            UpdateLoggingMenuCheckedState();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling context menu opening: {Message}", ex.Message);
        }
    }

}
