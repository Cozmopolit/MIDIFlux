using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core;
using MIDIFlux.Core.Config;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Controls.Common;
using MIDIFlux.GUI.Dialogs;
using MIDIFlux.GUI.Forms;
using MIDIFlux.GUI.Helpers;
using MIDIFlux.GUI.Models;
using MIDIFlux.GUI.Services;

namespace MIDIFlux.GUI.Controls.ProfileEditor
{
    /// <summary>
    /// User control for editing a MIDI profile
    /// </summary>
    public partial class ProfileEditorControl : BaseTabUserControl
    {
        private readonly ConfigLoader _configLoader;
        private readonly MidiProcessingServiceProxy _midiProcessingServiceProxy;
        private ProfileModel _profile;
        private Configuration _configuration = new Configuration();
        private BindingList<MidiDeviceViewModel> _deviceViewModels;
        private BindingList<MappingViewModel> _mappingViewModels;
        private MidiDeviceViewModel? _selectedDevice;

        /// <summary>
        /// Gets the profile associated with this editor
        /// </summary>
        public ProfileModel Profile => _profile;

        // Preview mode fields
        private bool _previewModeEnabled = false;
        private string? _originalConfigPath = null;
        private string? _previewConfigPath = null;
        private readonly Queue<PreviewEventViewModel> _previewEvents = new Queue<PreviewEventViewModel>();
        private const int MaxPreviewEvents = 10;
        private MidiEventMonitor? _midiEventMonitor = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileEditorControl"/> class
        /// </summary>
        /// <param name="profile">The profile to edit</param>
        /// <param name="midiProcessingServiceProxy">The MIDI processing service proxy</param>
        public ProfileEditorControl(ProfileModel profile, MidiProcessingServiceProxy? midiProcessingServiceProxy = null)
        {
            try
            {
                // Initialize the preview-related fields to avoid null reference exceptions
                InitializePreviewControls();

                // Now initialize the component
                InitializeComponent();

                // Store the profile
                _profile = profile;

                // Set the tab title
                TabTitle = $"Edit: {profile.Name}";

                // Create the config loader
                _configLoader = new ConfigLoader(LoggingHelper.CreateLogger<ConfigLoader>());

                // Use the provided MidiProcessingServiceProxy or create a new one
                _midiProcessingServiceProxy = midiProcessingServiceProxy ?? new MidiProcessingServiceProxy(LoggingHelper.CreateLogger<MidiProcessingServiceProxy>());

                // Initialize binding lists
                _deviceViewModels = new BindingList<MidiDeviceViewModel>();
                _mappingViewModels = new BindingList<MappingViewModel>();

                // Set up event handlers
                deviceListView.SelectedIndexChanged += DeviceListView_SelectedIndexChanged;
                addDeviceButton.Click += AddDeviceButton_Click;
                duplicateDeviceButton.Click += DuplicateDeviceButton_Click;
                removeDeviceButton.Click += RemoveDeviceButton_Click;
                deviceNameComboBox.SelectedIndexChanged += DeviceProperty_Changed;
                inputProfileTextBox.TextChanged += DeviceProperty_Changed;
                midiChannelsTextBox.TextChanged += DeviceProperty_Changed;
                selectChannelsButton.Click += SelectChannelsButton_Click;
                mappingsDataGridView.SelectionChanged += MappingsDataGridView_SelectionChanged;
                mappingsDataGridView.CellDoubleClick += MappingsDataGridView_CellDoubleClick;
                addMappingButton.Click += AddMappingButton_Click;
                editMappingButton.Click += EditMappingButton_Click;
                deleteMappingButton.Click += DeleteMappingButton_Click;
                saveButton.Click += SaveButton_Click;
                filterTextBox.TextChanged += FilterTextBox_TextChanged;

                // Set up preview mode event handlers
                if (previewModeToggleButton != null)
                    previewModeToggleButton.Click += PreviewModeToggleButton_Click;
                if (clearEventsButton != null)
                    clearEventsButton.Click += ClearEventsButton_Click;

                // Initialize save button state
                UpdateSaveButtonState();
            }
            catch (Exception ex)
            {
                var logger = LoggingHelper.CreateLogger<ProfileEditorControl>();
                logger.LogError(ex, "Error initializing ProfileEditorControl: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Initializes the preview-related controls to avoid null reference exceptions
        /// </summary>
        private void InitializePreviewControls()
        {
            // Initialize all preview-related controls
            if (previewSplitContainer == null)
                previewSplitContainer = new SplitContainer
                {
                    Dock = DockStyle.Fill,
                    Orientation = Orientation.Horizontal,
                    Panel2MinSize = 100
                };

            if (previewGroupBox == null)
                previewGroupBox = new GroupBox
                {
                    Text = "Preview Events",
                    Dock = DockStyle.Fill
                };

            if (previewListView == null)
                previewListView = new ListView
                {
                    Dock = DockStyle.Fill,
                    FullRowSelect = true,
                    HideSelection = false,
                    View = View.Details
                };

            if (previewToolStrip == null)
                previewToolStrip = new ToolStrip();

            if (previewModeToggleButton == null)
                previewModeToggleButton = new ToolStripButton
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    Text = "Enable Preview Mode",
                    ToolTipText = "Toggle preview mode"
                };

            if (clearEventsButton == null)
                clearEventsButton = new ToolStripButton
                {
                    DisplayStyle = ToolStripItemDisplayStyle.Text,
                    Text = "Clear Events",
                    ToolTipText = "Clear all preview events"
                };

            if (timestampColumnHeader == null)
                timestampColumnHeader = new ColumnHeader { Text = "Time", Width = 80 };

            if (eventTypeColumnHeader == null)
                eventTypeColumnHeader = new ColumnHeader { Text = "Event Type", Width = 100 };

            if (triggerColumnHeader == null)
                triggerColumnHeader = new ColumnHeader { Text = "Trigger", Width = 150 };

            if (actionColumnHeader == null)
                actionColumnHeader = new ColumnHeader { Text = "Action", Width = 200 };

            if (deviceColumnHeader == null)
                deviceColumnHeader = new ColumnHeader { Text = "Device", Width = 150 };
        }

        /// <summary>
        /// Called when the control is loaded
        /// </summary>
        protected override void OnControlLoaded()
        {
            base.OnControlLoaded();

            try
            {
                // Make sure the preview list view has the column headers
                if (previewListView.Columns.Count == 0)
                {
                    previewListView.Columns.AddRange(new ColumnHeader[] {
                        timestampColumnHeader,
                        eventTypeColumnHeader,
                        triggerColumnHeader,
                        actionColumnHeader,
                        deviceColumnHeader
                    });
                }

                // Make sure the preview tool strip has the buttons
                if (previewToolStrip.Items.Count == 0)
                {
                    previewToolStrip.Items.AddRange(new ToolStripItem[] {
                        previewModeToggleButton,
                        clearEventsButton
                    });
                }

                // Make sure the preview group box has the controls
                if (previewGroupBox.Controls.Count == 0)
                {
                    previewGroupBox.Controls.Add(previewListView);
                    previewGroupBox.Controls.Add(previewToolStrip);
                }

                // Make sure the preview split container has the panels set up
                if (previewSplitContainer.Panel1.Controls.Count == 0 && mappingsGroupBox != null)
                {
                    previewSplitContainer.Panel1.Controls.Add(mappingsGroupBox);
                }

                if (previewSplitContainer.Panel2.Controls.Count == 0 && previewGroupBox != null)
                {
                    previewSplitContainer.Panel2.Controls.Add(previewGroupBox);
                }

                // Load the configuration
                LoadConfiguration();

                // Populate the device name dropdown
                PopulateDeviceNameDropdown();

                // Initialize button states
                editMappingButton.Enabled = false;
                deleteMappingButton.Enabled = false;

                // Initialize preview mode
                previewModeToggleButton.Text = "Enable Preview Mode";
                previewModeToggleButton.BackColor = SystemColors.Control;
                clearEventsButton.Enabled = true;

                // Add an initial event to the preview pane
                AddPreviewEvent(PreviewEventViewModel.Create(
                    "System",
                    "Information",
                    "Preview mode is disabled. Click 'Enable Preview Mode' to test your mappings.",
                    "System"));
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error in OnControlLoaded: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error loading profile editor: {ex.Message}",
                    "Error",
                    logger,
                    ex,
                    this);
            }
        }

        /// <summary>
        /// Loads the configuration from the profile
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                // Load the configuration
                _configuration = _configLoader.LoadConfiguration(_profile.FilePath) ?? new Configuration();

                // Create view models for the devices
                _deviceViewModels.Clear();
                foreach (var device in _configuration.MidiDevices)
                {
                    _deviceViewModels.Add(new MidiDeviceViewModel(device));
                }

                // Bind the device list view
                deviceListView.Items.Clear();
                foreach (var deviceViewModel in _deviceViewModels)
                {
                    var item = new ListViewItem(deviceViewModel.InputProfile);
                    item.SubItems.Add(deviceViewModel.DeviceName);
                    item.SubItems.Add(deviceViewModel.ChannelsDisplay);
                    item.Tag = deviceViewModel;
                    deviceListView.Items.Add(item);
                }

                // Select the first device if available
                if (deviceListView.Items.Count > 0)
                {
                    deviceListView.Items[0].Selected = true;
                }
                else
                {
                    // Clear the property editors
                    ClearPropertyEditors();
                    // Disable the device action buttons
                    UpdateDeviceActionButtonState();
                }

                // Mark as clean
                MarkClean();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error loading configuration from {FilePath}: {Message}", _profile.FilePath, ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error loading configuration: {ex.Message}",
                    "Error",
                    logger,
                    ex,
                    this);
            }
        }

        /// <summary>
        /// Populates the device name dropdown with available MIDI devices
        /// </summary>
        private void PopulateDeviceNameDropdown()
        {
            try
            {
                // Clear the dropdown
                deviceNameComboBox.Items.Clear();

                // Add a blank item
                deviceNameComboBox.Items.Add("");

                // Get the list of available devices
                if (_midiProcessingServiceProxy != null && _midiProcessingServiceProxy.IsServiceAvailable())
                {
                    try
                    {
                        var devices = _midiProcessingServiceProxy.GetAvailableMidiDevices();
                        if (devices != null)
                        {
                            foreach (var device in devices)
                            {
                                deviceNameComboBox.Items.Add(device.Name);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        var logger = GetLogger();
                        logger.LogError(ex, "Error getting available MIDI devices: {Message}", ex.Message);
                        // Continue without adding devices - the dropdown will just have the blank item
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError(
                    $"Error populating device dropdown: {ex.Message}",
                    "Error",
                    logger,
                    ex,
                    this);
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DeviceListView
        /// </summary>
        private void DeviceListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (deviceListView.SelectedItems.Count > 0)
            {
                // Get the selected device
                var item = deviceListView.SelectedItems[0];
                _selectedDevice = item.Tag as MidiDeviceViewModel;

                // Update the property editors
                UpdatePropertyEditors();

                // Update the mappings grid
                UpdateMappingsGrid();
            }
            else
            {
                // Clear the property editors
                ClearPropertyEditors();
                // Clear the mappings grid
                _mappingViewModels.Clear();
            }

            // Update the device action button state
            UpdateDeviceActionButtonState();
        }

        /// <summary>
        /// Updates the property editors with the selected device's properties
        /// </summary>
        private void UpdatePropertyEditors()
        {
            if (_selectedDevice != null)
            {
                // Update the property editors
                inputProfileTextBox.Text = _selectedDevice.InputProfile;
                deviceNameComboBox.Text = _selectedDevice.DeviceName;
                midiChannelsTextBox.Text = _selectedDevice.ChannelsDisplay;

                // Enable the property editors
                inputProfileTextBox.Enabled = true;
                deviceNameComboBox.Enabled = true;
                midiChannelsTextBox.Enabled = true;
                selectChannelsButton.Enabled = true;
            }
        }

        /// <summary>
        /// Clears the property editors
        /// </summary>
        private void ClearPropertyEditors()
        {
            // Clear the property editors
            inputProfileTextBox.Text = string.Empty;
            deviceNameComboBox.Text = string.Empty;
            midiChannelsTextBox.Text = string.Empty;

            // Disable the property editors
            inputProfileTextBox.Enabled = false;
            deviceNameComboBox.Enabled = false;
            midiChannelsTextBox.Enabled = false;
            selectChannelsButton.Enabled = false;

            // Clear the selected device
            _selectedDevice = null;
        }

        /// <summary>
        /// Updates the device action button state
        /// </summary>
        private void UpdateDeviceActionButtonState()
        {
            // Enable/disable the device action buttons
            duplicateDeviceButton.Enabled = _selectedDevice != null;
            removeDeviceButton.Enabled = _selectedDevice != null && _deviceViewModels.Count > 0;
        }

        /// <summary>
        /// Updates the mappings grid with the selected device's mappings
        /// </summary>
        private void UpdateMappingsGrid()
        {
            if (_selectedDevice != null)
            {
                // Clear the mappings view models
                _mappingViewModels.Clear();

                // Add the key mappings
                foreach (var mapping in _selectedDevice.Mappings)
                {
                    _mappingViewModels.Add(new MappingViewModel
                    {
                        MappingType = "Note",
                        Trigger = mapping.MidiNote.ToString(),
                        ActionType = "Key",
                        ActionDetails = $"Key: {(Keys)mapping.VirtualKeyCode}",
                        Description = mapping.Description ?? string.Empty,
                        SourceMapping = mapping
                    });
                }

                // Add the absolute control mappings
                foreach (var mapping in _selectedDevice.AbsoluteControlMappings)
                {
                    _mappingViewModels.Add(new MappingViewModel
                    {
                        MappingType = "Absolute Control",
                        Trigger = mapping.ControlNumber.ToString(),
                        ActionType = mapping.HandlerType,
                        ActionDetails = $"Min: {mapping.MinValue}, Max: {mapping.MaxValue}, Invert: {mapping.Invert}",
                        Description = mapping.Description ?? string.Empty,
                        SourceMapping = mapping
                    });
                }

                // Add the relative control mappings
                foreach (var mapping in _selectedDevice.RelativeControlMappings)
                {
                    _mappingViewModels.Add(new MappingViewModel
                    {
                        MappingType = "Relative Control",
                        Trigger = mapping.ControlNumber.ToString(),
                        ActionType = mapping.HandlerType,
                        ActionDetails = $"Sensitivity: {mapping.Sensitivity}, Invert: {mapping.Invert}",
                        Description = mapping.Description ?? string.Empty,
                        SourceMapping = mapping
                    });
                }

                // Add the CC range mappings
                foreach (var mapping in _selectedDevice.CCRangeMappings)
                {
                    _mappingViewModels.Add(new MappingViewModel
                    {
                        MappingType = "CC Range",
                        Trigger = mapping.ControlNumber.ToString(),
                        ActionType = mapping.HandlerType,
                        ActionDetails = $"{mapping.Ranges.Count} ranges",
                        Description = mapping.Description ?? string.Empty,
                        SourceMapping = mapping
                    });
                }



                // Bind the mappings to the grid
                mappingsDataGridView.DataSource = _mappingViewModels;

                // Apply the filter
                ApplyFilter();
            }
            else
            {
                // Clear the mappings
                _mappingViewModels.Clear();
                mappingsDataGridView.DataSource = null;
            }
        }

        /// <summary>
        /// Handles the Click event of the AddDeviceButton
        /// </summary>
        private void AddDeviceButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a new device configuration
                var deviceConfig = new MidiDeviceConfiguration
                {
                    InputProfile = "New Device",
                    DeviceName = string.Empty,
                    Mappings = new List<KeyMapping>()
                };

                // Add it to the configuration
                _configuration.MidiDevices.Add(deviceConfig);

                // Create a view model for the device
                var deviceViewModel = new MidiDeviceViewModel(deviceConfig);
                _deviceViewModels.Add(deviceViewModel);

                // Add it to the list view
                var item = new ListViewItem(deviceViewModel.InputProfile);
                item.SubItems.Add(deviceViewModel.DeviceName);
                item.SubItems.Add(deviceViewModel.ChannelsDisplay);
                item.Tag = deviceViewModel;
                deviceListView.Items.Add(item);

                // Select the new device
                item.Selected = true;
                deviceListView.Focus();

                // Mark as dirty
                MarkDirty();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error adding device: {ex.Message}", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the DuplicateDeviceButton
        /// </summary>
        private void DuplicateDeviceButton_Click(object? sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Create a deep copy of the device configuration
                var sourceConfig = _selectedDevice.DeviceConfiguration;
                var newConfig = new MidiDeviceConfiguration
                {
                    InputProfile = $"Copy of {sourceConfig.InputProfile}",
                    DeviceName = sourceConfig.DeviceName,
                    MidiChannels = sourceConfig.MidiChannels?.ToList(),
                    Mappings = sourceConfig.Mappings.Select(m => new KeyMapping
                    {
                        MidiNote = m.MidiNote,
                        VirtualKeyCode = m.VirtualKeyCode,
                        Modifiers = m.Modifiers.ToList(),
                        ActionType = m.ActionType,

                        Command = m.Command,
                        ShellType = m.ShellType,
                        RunHidden = m.RunHidden,
                        WaitForExit = m.WaitForExit,
                        Description = m.Description,
                        IgnoreNoteOff = m.IgnoreNoteOff,
                        AutoReleaseAfterMs = m.AutoReleaseAfterMs
                    }).ToList(),
                    AbsoluteControlMappings = sourceConfig.AbsoluteControlMappings.Select(m => new AbsoluteControlMapping
                    {
                        ControlNumber = m.ControlNumber,
                        Channel = m.Channel,
                        HandlerType = m.HandlerType,
                        MinValue = m.MinValue,
                        MaxValue = m.MaxValue,
                        Invert = m.Invert,
                        Parameters = new Dictionary<string, object>(m.Parameters)
                    }).ToList(),
                    RelativeControlMappings = sourceConfig.RelativeControlMappings.Select(m => new RelativeControlMapping
                    {
                        ControlNumber = m.ControlNumber,
                        Channel = m.Channel,
                        HandlerType = m.HandlerType,
                        Sensitivity = m.Sensitivity,
                        Invert = m.Invert,
                        Encoding = m.Encoding,
                        Parameters = new Dictionary<string, object>(m.Parameters)
                    }).ToList()
                };

                // Add it to the configuration
                _configuration.MidiDevices.Add(newConfig);

                // Create a view model for the device
                var deviceViewModel = new MidiDeviceViewModel(newConfig);
                _deviceViewModels.Add(deviceViewModel);

                // Add it to the list view
                var item = new ListViewItem(deviceViewModel.InputProfile);
                item.SubItems.Add(deviceViewModel.DeviceName);
                item.SubItems.Add(deviceViewModel.ChannelsDisplay);
                item.Tag = deviceViewModel;
                deviceListView.Items.Add(item);

                // Select the new device
                item.Selected = true;
                deviceListView.Focus();

                // Mark as dirty
                MarkDirty();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error duplicating device: {ex.Message}", "Error", logger, ex, this);
            }
        }
        /// <summary>
        /// Handles the Click event of the RemoveDeviceButton
        /// </summary>
        private void RemoveDeviceButton_Click(object? sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Confirm deletion
                var logger = GetLogger();
                var result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to remove the device '{_selectedDevice.InputProfile}'?",
                    "Remove Device",
                    logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the device from the configuration
                _configuration.MidiDevices.Remove(_selectedDevice.DeviceConfiguration);

                // Remove the device from the view models
                _deviceViewModels.Remove(_selectedDevice);

                // Remove the device from the list view
                foreach (ListViewItem item in deviceListView.Items)
                {
                    if (item.Tag == _selectedDevice)
                    {
                        deviceListView.Items.Remove(item);
                        break;
                    }
                }

                // Select the first device if available
                if (deviceListView.Items.Count > 0)
                {
                    deviceListView.Items[0].Selected = true;
                }
                else
                {
                    // Clear the property editors
                    ClearPropertyEditors();
                    // Clear the mappings grid
                    _mappingViewModels.Clear();
                    mappingsDataGridView.DataSource = null;
                }

                // Mark as dirty
                MarkDirty();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error removing device: {ex.Message}", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Changed event of the device properties
        /// </summary>
        private void DeviceProperty_Changed(object? sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Update the device configuration
                _selectedDevice.InputProfile = inputProfileTextBox.Text;
                _selectedDevice.DeviceName = deviceNameComboBox.Text;
                // Channels are updated when the select channels button is clicked

                // Update the list view item
                foreach (ListViewItem item in deviceListView.Items)
                {
                    if (item.Tag == _selectedDevice)
                    {
                        item.Text = _selectedDevice.InputProfile;
                        item.SubItems[1].Text = _selectedDevice.DeviceName;
                        item.SubItems[2].Text = _selectedDevice.ChannelsDisplay;
                        break;
                    }
                }

                // Mark as dirty
                MarkDirty();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error updating device properties: {ex.Message}", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the SelectChannelsButton
        /// </summary>
        private void SelectChannelsButton_Click(object? sender, EventArgs e)
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Open the channel picker dialog
                using (var channelPicker = new ChannelPickerDialog(_selectedDevice.MidiChannels))
                {
                    if (channelPicker.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the device with the selected channels
                        _selectedDevice.MidiChannels = channelPicker.SelectedChannels;

                        // Update the text box
                        midiChannelsTextBox.Text = _selectedDevice.ChannelsDisplay;

                        // Update the list view item
                        foreach (ListViewItem item in deviceListView.Items)
                        {
                            if (item.Tag == _selectedDevice)
                            {
                                item.SubItems[2].Text = _selectedDevice.ChannelsDisplay;
                                break;
                            }
                        }

                        // Mark as dirty
                        MarkDirty();
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error updating MIDI channels: {ex.Message}", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the SelectionChanged event of the MappingsDataGridView
        /// </summary>
        private void MappingsDataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            // Enable/disable the edit and delete buttons based on selection
            bool hasSelection = mappingsDataGridView.SelectedRows.Count > 0;
            editMappingButton.Enabled = hasSelection;
            deleteMappingButton.Enabled = hasSelection;
        }

        /// <summary>
        /// Handles the CellDoubleClick event of the MappingsDataGridView
        /// </summary>
        private void MappingsDataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
            // If a row is double-clicked, edit the mapping
            if (e.RowIndex >= 0)
            {
                EditSelectedMapping();
            }
        }

        /// <summary>
        /// Handles the Click event of the AddMappingButton
        /// </summary>
        private void AddMappingButton_Click(object? sender, EventArgs e)
        {
            AddNewMapping();
        }

        /// <summary>
        /// Handles the Click event of the EditMappingButton
        /// </summary>
        private void EditMappingButton_Click(object? sender, EventArgs e)
        {
            EditSelectedMapping();
        }

        /// <summary>
        /// Handles the Click event of the DeleteMappingButton
        /// </summary>
        private void DeleteMappingButton_Click(object? sender, EventArgs e)
        {
            DeleteSelectedMapping();
        }

        /// <summary>
        /// Adds a new mapping
        /// </summary>
        private void AddNewMapping()
        {
            if (_selectedDevice == null)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("Please select a device first.", "No Device Selected", logger, null, this);
                return;
            }

            try
            {
                // Create a menu with mapping types
                var menu = new ContextMenuStrip();
                menu.Items.Add("Key Mapping", null, (s, e) => AddNewKeyMapping());
                menu.Items.Add("Absolute Control Mapping", null, (s, e) => AddNewAbsoluteControlMapping());
                menu.Items.Add("Relative Control Mapping", null, (s, e) => AddNewRelativeControlMapping());
                menu.Items.Add("CC Range Mapping", null, (s, e) => AddNewCCRangeMapping());

                // Show the menu at the position of the button
                if (addMappingButton.Owner is Control owner)
                {
                    menu.Show(owner, new System.Drawing.Point(addMappingButton.Bounds.Left, addMappingButton.Bounds.Bottom));
                }
                else
                {
                    // Fallback to showing at cursor position if owner is not a Control
                    menu.Show(Cursor.Position);
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while showing the mapping type menu.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Adds a new key mapping
        /// </summary>
        private void AddNewKeyMapping()
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Get the MIDI manager for listening functionality
                var midiManager = GetMidiManagerFromParent();

                // Create a new key mapping dialog
                using (var dialog = new KeyMappingDialog(midiManager))
                {
                    // Show the dialog
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get the key mapping
                        var keyMapping = dialog.KeyMapping;

                        // Add it to the device
                        _selectedDevice.Mappings.Add(keyMapping);

                        // Update the mappings grid
                        UpdateMappingsGrid();

                        // Mark as dirty
                        MarkDirty();

                        // Log the action
                        var logger = GetLogger();
                        logger.LogInformation("Added new key mapping: MIDI Note {MidiNote} -> Key {VirtualKeyCode}", keyMapping.MidiNote, keyMapping.VirtualKeyCode);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while adding a new key mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Adds a new absolute control mapping
        /// </summary>
        private void AddNewAbsoluteControlMapping()
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Get the MIDI manager for listening functionality
                var midiManager = GetMidiManagerFromParent();

                // Create a new absolute control mapping dialog
                using (var dialog = new AbsoluteControlMappingDialog(midiManager))
                {
                    // Show the dialog
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get the absolute control mapping
                        var mapping = dialog.Mapping;

                        // Add it to the device
                        _selectedDevice.AbsoluteControlMappings.Add(mapping);

                        // Update the mappings grid
                        UpdateMappingsGrid();

                        // Mark as dirty
                        MarkDirty();

                        // Log the action
                        var logger = GetLogger();
                        logger.LogInformation("Added new absolute control mapping: Control {ControlNumber} -> {HandlerType}", mapping.ControlNumber, mapping.HandlerType);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while adding a new absolute control mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Adds a new relative control mapping
        /// </summary>
        private void AddNewRelativeControlMapping()
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Get the MIDI manager for listening functionality
                var midiManager = GetMidiManagerFromParent();

                // Create a new relative control mapping dialog
                using (var dialog = new RelativeControlMappingDialog(midiManager))
                {
                    // Show the dialog
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get the relative control mapping
                        var mapping = dialog.Mapping;

                        // Add it to the device
                        _selectedDevice.RelativeControlMappings.Add(mapping);

                        // Update the mappings grid
                        UpdateMappingsGrid();

                        // Mark as dirty
                        MarkDirty();

                        // Log the action
                        var logger = GetLogger();
                        logger.LogInformation("Added new relative control mapping: Control {ControlNumber} -> {HandlerType}", mapping.ControlNumber, mapping.HandlerType);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while adding a new relative control mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Adds a new CC range mapping
        /// </summary>
        private void AddNewCCRangeMapping()
        {
            if (_selectedDevice == null)
            {
                return;
            }

            try
            {
                // Get the MIDI manager for listening functionality
                var midiManager = GetMidiManagerFromParent();

                // Create a new CC range mapping dialog
                using (var dialog = new CCRangeMappingDialog(midiManager))
                {
                    // Show the dialog
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Get the CC range mapping
                        var mapping = dialog.Mapping;

                        // Add it to the device
                        _selectedDevice.CCRangeMappings.Add(mapping);

                        // Update the mappings grid
                        UpdateMappingsGrid();

                        // Mark as dirty
                        MarkDirty();

                        // Log the action
                        var logger = GetLogger();
                        logger.LogInformation("Added new CC range mapping: Control {ControlNumber} -> {HandlerType}", mapping.ControlNumber, mapping.HandlerType);
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while adding a new CC range mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Edits the selected mapping
        /// </summary>
        private void EditSelectedMapping()
        {
            if (_selectedDevice == null || mappingsDataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            try
            {
                // Get the selected mapping
                var selectedRow = mappingsDataGridView.SelectedRows[0];
                var mappingViewModel = _mappingViewModels[selectedRow.Index];

                // Check if it's a key mapping
                if (mappingViewModel.SourceMapping is KeyMapping keyMapping)
                {
                    // Get the MIDI manager for listening functionality
                    var midiManager = GetMidiManagerFromParent();

                    // Create a key mapping dialog
                    using (var dialog = new KeyMappingDialog(keyMapping, midiManager))
                    {
                        // Show the dialog
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the mappings grid
                            UpdateMappingsGrid();

                            // Mark as dirty
                            MarkDirty();

                            // Log the action
                            var logger = GetLogger();
                            logger.LogInformation("Edited key mapping: MIDI Note {MidiNote} -> Key {VirtualKeyCode}", keyMapping.MidiNote, keyMapping.VirtualKeyCode);
                        }
                    }
                }
                else if (mappingViewModel.SourceMapping is AbsoluteControlMapping absoluteControlMapping)
                {
                    // Get the MIDI manager for listening functionality
                    var midiManager = GetMidiManagerFromParent();

                    // Create an absolute control mapping dialog
                    using (var dialog = new AbsoluteControlMappingDialog(absoluteControlMapping, midiManager))
                    {
                        // Show the dialog
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the mappings grid
                            UpdateMappingsGrid();

                            // Mark as dirty
                            MarkDirty();

                            // Log the action
                            var logger = GetLogger();
                            logger.LogInformation("Edited absolute control mapping: Control {ControlNumber} -> {HandlerType}", absoluteControlMapping.ControlNumber, absoluteControlMapping.HandlerType);
                        }
                    }
                }
                else if (mappingViewModel.SourceMapping is RelativeControlMapping relativeControlMapping)
                {
                    // Get the MIDI manager for listening functionality
                    var midiManager = GetMidiManagerFromParent();

                    // Create a relative control mapping dialog
                    using (var dialog = new RelativeControlMappingDialog(relativeControlMapping, midiManager))
                    {
                        // Show the dialog
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the mappings grid
                            UpdateMappingsGrid();

                            // Mark as dirty
                            MarkDirty();

                            // Log the action
                            var logger = GetLogger();
                            logger.LogInformation("Edited relative control mapping: Control {ControlNumber} -> {HandlerType}", relativeControlMapping.ControlNumber, relativeControlMapping.HandlerType);
                        }
                    }
                }
                else if (mappingViewModel.SourceMapping is CCRangeMapping ccRangeMapping)
                {
                    // Get the MIDI manager for listening functionality
                    var midiManager = GetMidiManagerFromParent();

                    // Create a CC range mapping dialog
                    using (var dialog = new CCRangeMappingDialog(ccRangeMapping, midiManager))
                    {
                        // Show the dialog
                        if (dialog.ShowDialog(this) == DialogResult.OK)
                        {
                            // Update the mappings grid
                            UpdateMappingsGrid();

                            // Mark as dirty
                            MarkDirty();

                            // Log the action
                            var logger = GetLogger();
                            logger.LogInformation("Edited CC range mapping: Control {ControlNumber} -> {HandlerType}", ccRangeMapping.ControlNumber, ccRangeMapping.HandlerType);
                        }
                    }
                }
                else
                {
                    // Show an error message for unsupported mapping types
                    var logger = GetLogger();
                    ApplicationErrorHandler.ShowError($"Editing {mappingViewModel.MappingType} mappings is not yet supported.", "Unsupported Mapping Type", logger, null, this);
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while editing the mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Deletes the selected mapping
        /// </summary>
        private void DeleteSelectedMapping()
        {
            if (_selectedDevice == null || mappingsDataGridView.SelectedRows.Count == 0)
            {
                return;
            }

            try
            {
                // Get the selected mapping
                var selectedRow = mappingsDataGridView.SelectedRows[0];
                var mappingViewModel = _mappingViewModels[selectedRow.Index];

                // Confirm deletion
                var logger = GetLogger();
                DialogResult result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to delete this mapping?",
                    "Delete Mapping",
                    logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Delete the mapping based on its type
                if (mappingViewModel.SourceMapping is KeyMapping keyMapping)
                {
                    _selectedDevice.Mappings.Remove(keyMapping);
                    logger.LogInformation("Deleted key mapping: MIDI Note {MidiNote} -> Key {VirtualKeyCode}", keyMapping.MidiNote, keyMapping.VirtualKeyCode);
                }
                else if (mappingViewModel.SourceMapping is AbsoluteControlMapping absoluteControlMapping)
                {
                    _selectedDevice.AbsoluteControlMappings.Remove(absoluteControlMapping);
                    logger.LogInformation("Deleted absolute control mapping: Control {ControlNumber} -> {HandlerType}", absoluteControlMapping.ControlNumber, absoluteControlMapping.HandlerType);
                }
                else if (mappingViewModel.SourceMapping is RelativeControlMapping relativeControlMapping)
                {
                    _selectedDevice.RelativeControlMappings.Remove(relativeControlMapping);
                    logger.LogInformation("Deleted relative control mapping: Control {ControlNumber} -> {HandlerType}", relativeControlMapping.ControlNumber, relativeControlMapping.HandlerType);
                }
                else if (mappingViewModel.SourceMapping is CCRangeMapping ccRangeMapping)
                {
                    _selectedDevice.CCRangeMappings.Remove(ccRangeMapping);
                    logger.LogInformation("Deleted CC range mapping: Control {ControlNumber} -> {HandlerType}", ccRangeMapping.ControlNumber, ccRangeMapping.HandlerType);
                }
                else
                {
                    ApplicationErrorHandler.ShowError($"Deleting {mappingViewModel.MappingType} mappings is not yet supported.", "Unsupported Mapping Type", logger, null, this);
                    return;
                }

                // Update the mappings grid
                UpdateMappingsGrid();

                // Mark as dirty
                MarkDirty();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError("An error occurred while deleting the mapping.", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the TextChanged event of the FilterTextBox
        /// </summary>
        private void FilterTextBox_TextChanged(object? sender, EventArgs e)
        {
            ApplyFilter();
        }

        /// <summary>
        /// Handles the Click event of the SaveButton
        /// </summary>
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                Save();
            }, GetLogger(), "saving profile", this);
        }

        /// <summary>
        /// Processes command keys for keyboard shortcuts
        /// </summary>
        /// <param name="msg">The message to process</param>
        /// <param name="keyData">The key data</param>
        /// <returns>True if the key was processed, false otherwise</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Handle Ctrl+S for save
            if (keyData == (Keys.Control | Keys.S))
            {
                ApplicationErrorHandler.RunWithUiErrorHandling(() =>
                {
                    Save();
                }, GetLogger(), "saving profile via keyboard shortcut", this);
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        /// <summary>
        /// Updates the save button state based on the dirty state
        /// </summary>
        private void UpdateSaveButtonState()
        {
            try
            {
                if (saveButton != null)
                {
                    saveButton.Enabled = HasUnsavedChanges;
                    saveButton.Font = HasUnsavedChanges ?
                        new Font(saveButton.Font, FontStyle.Bold) :
                        new Font(saveButton.Font, FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error updating save button state");
            }
        }

        /// <summary>
        /// Called when the dirty state changes to update the save button state
        /// </summary>
        protected override void OnDirtyStateChanged()
        {
            base.OnDirtyStateChanged();
            UpdateSaveButtonState();
        }

        /// <summary>
        /// Applies the filter to the mappings grid
        /// </summary>
        private void ApplyFilter()
        {
            if (_mappingViewModels == null)
            {
                return;
            }

            try
            {
                var filterText = filterTextBox.Text.Trim().ToLower();
                if (string.IsNullOrEmpty(filterText))
                {
                    // Show all mappings
                    foreach (DataGridViewRow row in mappingsDataGridView.Rows)
                    {
                        row.Visible = true;
                    }
                }
                else
                {
                    // Filter the mappings
                    foreach (DataGridViewRow row in mappingsDataGridView.Rows)
                    {
                        var visible = false;
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            if (cell.Value != null && cell.Value.ToString()?.ToLower().Contains(filterText) == true)
                            {
                                visible = true;
                                break;
                            }
                        }
                        row.Visible = visible;
                    }
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error applying filter: {ex.Message}", "Error", logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the PreviewModeToggleButton
        /// </summary>
        private void PreviewModeToggleButton_Click(object? sender, EventArgs e)
        {
            if (_previewModeEnabled)
            {
                DisablePreviewMode();
            }
            else
            {
                EnablePreviewMode();
            }
        }

        /// <summary>
        /// Handles the Click event of the ClearEventsButton
        /// </summary>
        private void ClearEventsButton_Click(object? sender, EventArgs e)
        {
            ClearPreviewEvents();
        }

        /// <summary>
        /// Enables preview mode
        /// </summary>
        private void EnablePreviewMode()
        {
            try
            {
                var logger = GetLogger();
                logger.LogInformation("Enabling preview mode");

                // Check if we need to save changes first
                if (HasUnsavedChanges)
                {
                    var result = MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowConfirmation(
                        "You have unsaved changes. Preview mode requires saving the current configuration to a temporary file. Do you want to continue?",
                        "Unsaved Changes",
                        logger,
                        DialogResult.Yes,
                        this);

                    if (result != DialogResult.Yes)
                    {
                        logger.LogInformation("Preview mode cancelled due to unsaved changes");
                        return;
                    }
                }

                // Store the original configuration path
                _originalConfigPath = _midiProcessingServiceProxy.GetActiveConfigurationPath();
                logger.LogDebug("Original configuration path: {OriginalConfigPath}", _originalConfigPath);

                // Create a temporary file for the preview configuration
                string tempDir = Path.Combine(ConfigurationHelper.GetAppDataDirectory(), "temp");
                Directory.CreateDirectory(tempDir);
                _previewConfigPath = Path.Combine(tempDir, $"preview_{DateTime.Now:yyyyMMddHHmmss}.json");
                logger.LogDebug("Preview configuration path: {PreviewConfigPath}", _previewConfigPath);

                // Save the current configuration to the temporary file
                var configJson = System.Text.Json.JsonSerializer.Serialize(_configuration, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                File.WriteAllText(_previewConfigPath, configJson);
                logger.LogInformation("Saved preview configuration to {PreviewConfigPath}", _previewConfigPath);

                // Activate the preview configuration
                if (_midiProcessingServiceProxy.ActivateProfile(_previewConfigPath))
                {
                    // Start MIDI event monitoring
                    StartMidiEventMonitoring();

                    // Update the UI
                    _previewModeEnabled = true;
                    previewModeToggleButton.Text = "Disable Preview Mode";
                    previewModeToggleButton.BackColor = Color.LightGreen;

                    // Clear any existing preview events
                    ClearPreviewEvents();

                    // Add an initial event to show that preview mode is enabled
                    AddPreviewEvent(PreviewEventViewModel.Create(
                        "System",
                        "Preview Mode",
                        "Preview mode enabled - Play MIDI to see actions triggered",
                        "System"));

                    logger.LogInformation("Preview mode enabled successfully");
                }
                else
                {
                    logger.LogError("Failed to activate preview configuration");
                    MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                        "Failed to activate preview configuration. Preview mode could not be enabled.",
                        "Preview Mode Error",
                        logger,
                        null,
                        this);

                    // Clean up
                    CleanupPreviewMode();
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error enabling preview mode");
                MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                    $"An error occurred while enabling preview mode: {ex.Message}",
                    "Preview Mode Error",
                    logger,
                    ex,
                    this);

                // Clean up
                CleanupPreviewMode();
            }
        }

        /// <summary>
        /// Disables preview mode
        /// </summary>
        private void DisablePreviewMode()
        {
            try
            {
                var logger = GetLogger();
                logger.LogInformation("Disabling preview mode");

                // Stop MIDI event monitoring
                StopMidiEventMonitoring();

                // Restore the original configuration
                if (_originalConfigPath != null && File.Exists(_originalConfigPath))
                {
                    if (_midiProcessingServiceProxy.ActivateProfile(_originalConfigPath))
                    {
                        logger.LogInformation("Restored original configuration: {OriginalConfigPath}", _originalConfigPath);
                    }
                    else
                    {
                        logger.LogError("Failed to restore original configuration: {OriginalConfigPath}", _originalConfigPath);
                        MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                            "Failed to restore original configuration. You may need to manually reload your profile.",
                            "Preview Mode Error",
                            logger,
                            null,
                            this);
                    }
                }

                // Clean up
                CleanupPreviewMode();

                // Update the UI
                _previewModeEnabled = false;
                previewModeToggleButton.Text = "Enable Preview Mode";
                previewModeToggleButton.BackColor = SystemColors.Control;

                // Add an event to show that preview mode is disabled
                AddPreviewEvent(PreviewEventViewModel.Create(
                    "System",
                    "Preview Mode",
                    "Preview mode disabled",
                    "System"));

                logger.LogInformation("Preview mode disabled");
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error disabling preview mode");
                MIDIFlux.Core.Helpers.ApplicationErrorHandler.ShowError(
                    $"An error occurred while disabling preview mode: {ex.Message}",
                    "Preview Mode Error",
                    logger,
                    ex,
                    this);
            }
        }

        /// <summary>
        /// Cleans up preview mode resources
        /// </summary>
        private void CleanupPreviewMode()
        {
            try
            {
                // Delete the temporary preview configuration file
                if (_previewConfigPath != null && File.Exists(_previewConfigPath))
                {
                    File.Delete(_previewConfigPath);
                    var logger = GetLogger();
                    logger.LogDebug("Deleted preview configuration file: {PreviewConfigPath}", _previewConfigPath);
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error cleaning up preview mode");
            }
            finally
            {
                // Reset the preview mode fields
                _previewConfigPath = null;
                _originalConfigPath = null;
            }
        }

        /// <summary>
        /// Adds a preview event to the list
        /// </summary>
        /// <param name="previewEvent">The preview event to add</param>
        private void AddPreviewEvent(PreviewEventViewModel previewEvent)
        {
            try
            {
                // Add the event to the queue
                _previewEvents.Enqueue(previewEvent);

                // If we have more than the maximum number of events, remove the oldest one
                while (_previewEvents.Count > MaxPreviewEvents)
                {
                    _previewEvents.Dequeue();
                }

                // Update the list view
                UpdatePreviewListView();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error adding preview event");
            }
        }

        /// <summary>
        /// Clears all preview events
        /// </summary>
        private void ClearPreviewEvents()
        {
            try
            {
                // Clear the queue
                _previewEvents.Clear();

                // Update the list view
                UpdatePreviewListView();
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error clearing preview events");
            }
        }

        /// <summary>
        /// Updates the preview list view with the current events
        /// </summary>
        private void UpdatePreviewListView()
        {
            try
            {
                // Clear the list view
                previewListView.Items.Clear();

                // Add each event to the list view
                foreach (var previewEvent in _previewEvents.Reverse())
                {
                    var item = new ListViewItem(new string[]
                    {
                        previewEvent.Timestamp.ToString("HH:mm:ss"),
                        previewEvent.EventType,
                        previewEvent.Trigger,
                        previewEvent.Action,
                        previewEvent.DeviceName
                    });
                    item.Tag = previewEvent;
                    previewListView.Items.Add(item);
                }

                // Ensure the newest event is visible
                if (previewListView.Items.Count > 0)
                {
                    previewListView.Items[0].EnsureVisible();
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error updating preview list view");
            }
        }

        /// <summary>
        /// Deactivates the tab
        /// </summary>
        public override void Deactivate()
        {
            base.Deactivate();

            // If preview mode is enabled, disable it
            if (_previewModeEnabled)
            {
                DisablePreviewMode();
            }
        }

        /// <summary>
        /// Attempts to close the tab
        /// </summary>
        /// <returns>True if the tab can be closed, false otherwise</returns>
        public override bool TryClose()
        {
            // If preview mode is enabled, disable it
            if (_previewModeEnabled)
            {
                DisablePreviewMode();
            }

            return base.TryClose();
        }

        /// <summary>
        /// Saves any unsaved changes
        /// </summary>
        /// <returns>True if the save was successful, false otherwise</returns>
        public override bool Save()
        {
            try
            {

                // Save the configuration
                var configJson = System.Text.Json.JsonSerializer.Serialize(_configuration, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
                System.IO.File.WriteAllText(_profile.FilePath, configJson);

                // Mark as clean
                MarkClean();

                // Show success message
                var logger = GetLogger();
                ApplicationErrorHandler.ShowInformation($"Profile '{_profile.Name}' saved successfully", "Save Successful", logger, this);

                return true;
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                ApplicationErrorHandler.ShowError($"Error saving profile: {ex.Message}", "Error", logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Starts MIDI event monitoring for preview mode
        /// </summary>
        private void StartMidiEventMonitoring()
        {
            try
            {
                var logger = GetLogger();

                // Get the MIDI manager from the parent form
                var midiManager = GetMidiManagerFromParent();
                if (midiManager == null)
                {
                    logger.LogWarning("MIDI manager not available for preview monitoring");
                    return;
                }

                // Create and start the MIDI event monitor
                _midiEventMonitor = new MidiEventMonitor(logger, midiManager, MaxPreviewEvents);
                _midiEventMonitor.MidiEventReceived += OnPreviewMidiEventReceived;

                // Start listening to all devices
                if (_midiEventMonitor.StartListening(-1, true))
                {
                    logger.LogInformation("Started MIDI event monitoring for preview mode");
                }
                else
                {
                    logger.LogWarning("Failed to start MIDI event monitoring for preview mode");
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error starting MIDI event monitoring for preview mode");
            }
        }

        /// <summary>
        /// Stops MIDI event monitoring for preview mode
        /// </summary>
        private void StopMidiEventMonitoring()
        {
            try
            {
                if (_midiEventMonitor != null)
                {
                    _midiEventMonitor.MidiEventReceived -= OnPreviewMidiEventReceived;
                    _midiEventMonitor.StopListening();
                    _midiEventMonitor.Dispose();
                    _midiEventMonitor = null;

                    var logger = GetLogger();
                    logger.LogInformation("Stopped MIDI event monitoring for preview mode");
                }
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error stopping MIDI event monitoring for preview mode");
            }
        }

        /// <summary>
        /// Gets the MidiManager from the parent ConfigurationForm
        /// </summary>
        /// <returns>The MidiManager instance, or null if not available</returns>
        private MidiManager? GetMidiManagerFromParent()
        {
            try
            {
                // Walk up the control hierarchy to find the ConfigurationForm
                Control? current = this;
                while (current != null)
                {
                    if (current is ConfigurationForm configForm)
                    {
                        return configForm.GetMidiManager();
                    }
                    current = current.Parent;
                }

                var logger = GetLogger();
                logger.LogWarning("Could not find ConfigurationForm in parent hierarchy");
                return null;
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error getting MidiManager from parent");
                return null;
            }
        }

        /// <summary>
        /// Handles MIDI events received during preview mode
        /// </summary>
        private void OnPreviewMidiEventReceived(object? sender, MidiEventArgs e)
        {
            try
            {
                if (!_previewModeEnabled)
                    return;

                // Get device name
                var deviceName = "Unknown Device";
                var midiManager = GetMidiManagerFromParent();
                if (midiManager != null)
                {
                    var deviceInfo = midiManager.GetDeviceInfo(e.DeviceId);
                    if (deviceInfo != null)
                    {
                        deviceName = deviceInfo.Name;
                    }
                }

                // Create a preview event showing the MIDI input
                var midiEvent = e.Event;
                var trigger = GetMidiEventDescription(midiEvent);
                var action = "Checking for mapped actions...";

                // Add the MIDI event to the preview
                AddPreviewEvent(PreviewEventViewModel.Create(
                    "MIDI Input",
                    trigger,
                    action,
                    deviceName));

                // TODO: In a future enhancement, we could check if this MIDI event
                // actually triggered any actions and show those results as well
            }
            catch (Exception ex)
            {
                var logger = GetLogger();
                logger.LogError(ex, "Error handling preview MIDI event");
            }
        }

        /// <summary>
        /// Gets a description of a MIDI event for display
        /// </summary>
        private string GetMidiEventDescription(MidiEvent midiEvent)
        {
            return midiEvent.EventType switch
            {
                MidiEventType.NoteOn => $"Note {midiEvent.Note} On (Velocity {midiEvent.Velocity})",
                MidiEventType.NoteOff => $"Note {midiEvent.Note} Off",
                MidiEventType.ControlChange => $"CC {midiEvent.Controller} = {midiEvent.Value}" +
                                             (midiEvent.IsRelative ? " (Relative)" : ""),
                MidiEventType.Error => $"Error: {midiEvent.ErrorType}",
                MidiEventType.Other => "Other MIDI Event",
                _ => $"{midiEvent.EventType}"
            };
        }
    }
}

