using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Configuration;
using MIDIFlux.GUI.Controls.Common;
using MIDIFlux.GUI.Dialogs;
using MIDIFlux.GUI.Models;
using MIDIFlux.GUI.Services;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Models;
using ApplicationErrorHandler = MIDIFlux.Core.Helpers.ApplicationErrorHandler;
using MIDIFlux.Core.Midi;

namespace MIDIFlux.GUI.Controls.ProfileEditor
{
    /// <summary>
    /// User control for editing a MIDI profile using the action system
    /// </summary>
    public partial class ProfileEditorControl : BaseTabUserControl
    {
        private readonly ActionConfigurationLoader _configLoader;

        /// <summary>
        /// Gets a value indicating whether the configuration has unsaved changes
        /// </summary>
        public bool IsDirty => HasUnsavedChanges;
        private readonly MidiProcessingServiceProxy _midiProcessingServiceProxy;

        private ProfileModel _profile;
        private MappingConfig _configuration = new();
        private BindingList<ActionMapping> _mappings;
        private BindingList<MappingDisplayModel> _displayMappings;



        // Column filtering and sorting state
        private readonly Dictionary<string, HashSet<string>> _columnFilters = new();
        private readonly Dictionary<string, string> _columnFilterValues = new();
        private string? _sortColumn = null;
        private ListSortDirection _sortDirection = ListSortDirection.Ascending;

        /// <summary>
        /// Gets the profile associated with this editor
        /// </summary>
        public ProfileModel Profile => _profile;

        /// <summary>
        /// Initializes a new instance of the ProfileEditorControl class
        /// </summary>
        /// <param name="profile">The profile to edit</param>
        /// <param name="logger">The logger to use for this control</param>
        /// <param name="actionConfigurationLoader">The action configuration loader</param>
        /// <param name="midiProcessingServiceProxy">The MIDI processing service proxy</param>
        public ProfileEditorControl(ProfileModel profile, ILogger<ProfileEditorControl> logger, ActionConfigurationLoader actionConfigurationLoader, MidiProcessingServiceProxy midiProcessingServiceProxy) : base(logger)
        {
            try
            {
                InitializeComponent();

                _profile = profile ?? throw new ArgumentNullException(nameof(profile));
                _configLoader = actionConfigurationLoader ?? throw new ArgumentNullException(nameof(actionConfigurationLoader));
                _midiProcessingServiceProxy = midiProcessingServiceProxy ?? throw new ArgumentNullException(nameof(midiProcessingServiceProxy));

                // Set the tab title
                TabTitle = $"Edit: {profile.Name}";

                // Initialize binding lists for mappings
                _mappings = new BindingList<ActionMapping>();
                _displayMappings = new BindingList<MappingDisplayModel>();

                // Set up event handlers
                SetupEventHandlers();

                // Initialize button states
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error initializing ProfileEditorControl: {Message}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Sets up all event handlers
        /// </summary>
        private void SetupEventHandlers()
        {
            mappingsDataGridView.SelectionChanged += MappingsDataGridView_SelectionChanged;
            mappingsDataGridView.CellDoubleClick += MappingsDataGridView_CellDoubleClick;
            mappingsDataGridView.ColumnHeaderMouseClick += MappingsDataGridView_ColumnHeaderMouseClick;
            addMappingButton.Click += AddMappingButton_Click;
            editMappingButton.Click += EditMappingButton_Click;
            deleteMappingButton.Click += DeleteMappingButton_Click;
            saveButton.Click += SaveButton_Click;

            // Enable column sorting
            SetupColumnFiltering();
        }

        /// <summary>
        /// Called when the control is loaded
        /// </summary>
        protected override void OnControlLoaded()
        {
            base.OnControlLoaded();

            try
            {
                // Load the configuration
                LoadConfiguration();

                // Initialize button states
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in OnControlLoaded: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error loading profile editor: {ex.Message}",
                    "MIDIFlux - Error",
                    _logger,
                    ex,
                    this);
            }
        }

        /// <summary>
        /// Loads the unified configuration from the profile
        /// </summary>
        private void LoadConfiguration()
        {
            try
            {
                _logger.LogDebug("Loading configuration from profile: {ProfileName} at {FilePath}", _profile.Name, _profile.FilePath);

                // Load the unified configuration
                _configuration = _configLoader.LoadConfiguration(_profile.FilePath) ?? new MappingConfig
                {
                    ProfileName = _profile.Name,
                    Description = $"Profile for {_profile.Name}",
                    MidiDevices = new List<DeviceConfig>()
                };

                _logger.LogDebug("Loaded configuration with {DeviceCount} devices", _configuration.MidiDevices.Count);

                // Clear existing data
                _mappings.Clear();
                _displayMappings.Clear();

                // Convert configuration to runtime mappings using the existing ActionConfigurationLoader
                var runtimeMappings = _configLoader.ConvertToMappings(_configuration);

                foreach (var mapping in runtimeMappings)
                {
                    try
                    {
                        _mappings.Add(mapping);
                        _displayMappings.Add(new MappingDisplayModel(mapping));
                        _logger.LogTrace("Added mapping: {Description}", mapping.Description ?? "No description");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to add mapping '{Description}': {Message}",
                            mapping.Description ?? "Unknown", ex.Message);
                    }
                }

                _logger.LogDebug("Created {DisplayMappingCount} display mappings from configuration", _displayMappings.Count);

                _logger.LogDebug("Populated {DisplayMappingCount} display mappings", _displayMappings.Count);

                // Bind the display mappings to the grid
                mappingsDataGridView.DataSource = _displayMappings;

                _logger.LogDebug("Bound {RowCount} rows to DataGridView", mappingsDataGridView.Rows.Count);

                // Mark as clean
                MarkClean();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading configuration from {FilePath}: {Message}", _profile.FilePath, ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error loading configuration: {ex.Message}",
                    "MIDIFlux - Error",
                    _logger,
                    ex,
                    this);
            }
        }



        /// <summary>
        /// Updates all button states based on current selection
        /// </summary>
        private void UpdateButtonStates()
        {
            var hasSelectedMapping = mappingsDataGridView.SelectedRows.Count > 0;

            addMappingButton.Enabled = true; // Always allow adding mappings
            editMappingButton.Enabled = hasSelectedMapping;
            deleteMappingButton.Enabled = hasSelectedMapping;
        }

        /// <summary>
        /// Updates the save button state based on dirty flag
        /// </summary>
        private void UpdateSaveButtonState()
        {
            saveButton.Enabled = IsDirty;
        }

        /// <summary>
        /// Marks the editor as dirty (has unsaved changes)
        /// </summary>
        protected override void MarkDirty()
        {
            base.MarkDirty();
            UpdateSaveButtonState();
        }

        /// <summary>
        /// Marks the editor as clean (no unsaved changes)
        /// </summary>
        protected override void MarkClean()
        {
            base.MarkClean();
            UpdateSaveButtonState();
        }

        #region Event Handlers





        /// <summary>
        /// Handles the SelectionChanged event of the MappingsDataGridView
        /// </summary>
        private void MappingsDataGridView_SelectionChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        /// <summary>
        /// Handles the CellDoubleClick event of the MappingsDataGridView
        /// </summary>
        private void MappingsDataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
        {
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
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a new mapping with sensible defaults - user can configure everything in the dialog
                AddNewMapping();
            }, _logger, "adding new mapping", this);
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
            if (mappingsDataGridView.SelectedRows.Count == 0) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var selectedRow = mappingsDataGridView.SelectedRows[0];
                var displayModel = selectedRow.DataBoundItem as MappingDisplayModel;

                if (displayModel?.Mapping != null)
                {
                    var result = MessageBox.Show(
                        $"Are you sure you want to delete this mapping?\n\n{displayModel.Mapping}",
                        "Confirm Mapping Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result == DialogResult.Yes)
                    {
                        // Remove from both lists
                        _mappings.Remove(displayModel.Mapping);
                        _displayMappings.Remove(displayModel);

                        // Refresh the filtered view to remove the deleted mapping
                        ApplyAllColumnFilters();
                        UpdateColumnHeaders();

                        MarkDirty();
                    }
                }
            }, _logger, "deleting mapping", this);
        }

        /// <summary>
        /// Handles the Click event of the SaveButton
        /// </summary>
        private void SaveButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                SaveConfiguration();
            }, _logger, "saving configuration", this);
        }

        /// <summary>
        /// Handles column header clicks for sorting and filtering
        /// </summary>
        private void MappingsDataGridView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            var column = mappingsDataGridView.Columns[e.ColumnIndex];

            if (e.Button == MouseButtons.Left)
            {
                // Handle sorting
                HandleColumnSort(column);
            }
            else if (e.Button == MouseButtons.Right)
            {
                // Handle filtering
                ShowColumnFilterMenu(column);
            }
        }

        #endregion

        #region Helper Methods



        /// <summary>
        /// Sets up column filtering and sorting functionality
        /// </summary>
        private void SetupColumnFiltering()
        {
            // Disable automatic sorting since we'll handle it manually
            foreach (DataGridViewColumn column in mappingsDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Programmatic;
            }

            // Add tooltip to help users discover functionality
            var toolTip = new ToolTip();
            toolTip.SetToolTip(mappingsDataGridView,
                "Left-click column headers to sort • Right-click column headers to filter");

            // Update column headers to show filter/sort indicators
            UpdateColumnHeaders();
        }

        /// <summary>
        /// Handles sorting when a column header is clicked
        /// </summary>
        private void HandleColumnSort(DataGridViewColumn column)
        {
            var propertyName = column.DataPropertyName;

            // Toggle sort direction if clicking the same column
            if (_sortColumn == propertyName)
            {
                _sortDirection = _sortDirection == ListSortDirection.Ascending
                    ? ListSortDirection.Descending
                    : ListSortDirection.Ascending;
            }
            else
            {
                _sortColumn = propertyName;
                _sortDirection = ListSortDirection.Ascending;
            }

            // Apply sorting and filtering
            ApplyAllColumnFilters();
            UpdateColumnHeaders();
        }

        /// <summary>
        /// Updates column headers to show sort and filter indicators
        /// </summary>
        private void UpdateColumnHeaders()
        {
            foreach (DataGridViewColumn column in mappingsDataGridView.Columns)
            {
                var originalHeaderText = GetOriginalHeaderText(column.Name);
                var headerText = originalHeaderText;

                // Add sort indicator
                if (_sortColumn == column.DataPropertyName)
                {
                    headerText += _sortDirection == ListSortDirection.Ascending ? " ▲" : " ▼";
                }

                // Add filter indicator
                if (_columnFilterValues.ContainsKey(column.DataPropertyName))
                {
                    headerText += " 🔍";
                }

                column.HeaderText = headerText;
            }
        }

        /// <summary>
        /// Gets the original header text for a column (without indicators)
        /// </summary>
        private string GetOriginalHeaderText(string columnName)
        {
            return columnName switch
            {
                "mappingTypeColumn" => "Type",
                "triggerColumn" => "Trigger",
                "channelColumn" => "Channel",
                "deviceColumn" => "Device",
                "actionTypeColumn" => "Action Type",
                "actionDetailsColumn" => "Action Details",
                "descriptionColumn" => "Description",
                "enabledColumn" => "Enabled",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Shows the filter menu for a specific column
        /// </summary>
        private void ShowColumnFilterMenu(DataGridViewColumn column)
        {
            var menu = new ContextMenuStrip();

            // Get unique values for this column
            var uniqueValues = GetUniqueValuesForColumn(column.DataPropertyName);
            var currentFilter = _columnFilterValues.GetValueOrDefault(column.DataPropertyName, string.Empty);

            // Add info header
            var infoItem = new ToolStripMenuItem("Filter by value:")
            {
                Enabled = false,
                Font = new Font(SystemFonts.MenuFont ?? SystemFonts.DefaultFont, FontStyle.Bold)
            };
            menu.Items.Add(infoItem);
            menu.Items.Add(new ToolStripSeparator());

            // Add "Clear Filter" option
            var clearItem = new ToolStripMenuItem("Clear Filter");
            clearItem.Click += (s, e) => ClearColumnFilter(column.DataPropertyName);
            clearItem.Enabled = !string.IsNullOrEmpty(currentFilter);
            menu.Items.Add(clearItem);

            menu.Items.Add(new ToolStripSeparator());

            // Add filter options for each unique value
            foreach (var value in uniqueValues.Take(20)) // Limit to 20 items for performance
            {
                var item = new ToolStripMenuItem(value);
                item.Checked = currentFilter == value;
                item.Click += (s, e) => ApplyColumnFilter(column.DataPropertyName, value);
                menu.Items.Add(item);
            }

            // Show the menu at the current mouse position
            menu.Show(Cursor.Position);
        }

        /// <summary>
        /// Gets unique values for a specific column
        /// </summary>
        private List<string> GetUniqueValuesForColumn(string propertyName)
        {
            var values = new HashSet<string>();

            foreach (var item in _displayMappings)
            {
                var value = GetPropertyValue(item, propertyName)?.ToString() ?? string.Empty;
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }

            return values.OrderBy(v => v).ToList();
        }

        /// <summary>
        /// Gets the value of a property from an object using reflection
        /// </summary>
        private object? GetPropertyValue(object obj, string propertyName)
        {
            return obj.GetType().GetProperty(propertyName)?.GetValue(obj);
        }

        /// <summary>
        /// Applies a filter to a specific column
        /// </summary>
        private void ApplyColumnFilter(string columnName, string filterValue)
        {
            _columnFilterValues[columnName] = filterValue;
            ApplyAllColumnFilters();
            UpdateColumnHeaders();
        }

        /// <summary>
        /// Clears the filter for a specific column
        /// </summary>
        private void ClearColumnFilter(string columnName)
        {
            _columnFilterValues.Remove(columnName);
            ApplyAllColumnFilters();
            UpdateColumnHeaders();
        }

        /// <summary>
        /// Applies all active column filters and sorting
        /// </summary>
        private void ApplyAllColumnFilters()
        {
            IEnumerable<MappingDisplayModel> workingSet = _displayMappings;

            // Apply filters if any are active
            if (_columnFilterValues.Count > 0)
            {
                workingSet = workingSet.Where(item =>
                {
                    foreach (var filter in _columnFilterValues)
                    {
                        var value = GetPropertyValue(item, filter.Key)?.ToString() ?? string.Empty;
                        if (value != filter.Value)
                        {
                            return false;
                        }
                    }
                    return true;
                });
            }

            // Apply sorting if a sort column is set
            if (!string.IsNullOrEmpty(_sortColumn))
            {
                workingSet = _sortDirection == ListSortDirection.Ascending
                    ? workingSet.OrderBy(item => GetPropertyValue(item, _sortColumn))
                    : workingSet.OrderByDescending(item => GetPropertyValue(item, _sortColumn));
            }

            // Update the data source
            var resultList = workingSet.ToList();
            mappingsDataGridView.DataSource = new BindingList<MappingDisplayModel>(resultList);
        }



        /// <summary>
        /// Saves the current configuration to file
        /// </summary>
        private void SaveConfiguration()
        {
            try
            {
                // Convert current mappings back to configuration using the existing method
                var newConfig = _configLoader.ConvertFromMappings(_mappings.ToList(), _profile.Name, _configuration.Description);

                // First, validate the configuration and get detailed error messages
                if (!newConfig.IsValid())
                {
                    var validationErrors = newConfig.GetValidationErrors();
                    var errorMessage = string.Join("\n• ", validationErrors);
                    MessageBox.Show($"Configuration validation failed:\n\n• {errorMessage}",
                        "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var success = _configLoader.SaveConfiguration(newConfig, _profile.FilePath);
                if (success)
                {
                    // Update our internal configuration reference
                    _configuration = newConfig;
                    MarkClean();
                    MessageBox.Show("Configuration saved successfully.", "Save Complete",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to save configuration. Please check the logs for details.",
                        "Save Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error saving configuration: {ex.Message}",
                    "MIDIFlux - Save Error",
                    _logger,
                    ex,
                    this);
            }
        }



        /// <summary>
        /// Edits the currently selected mapping
        /// </summary>
        private void EditSelectedMapping()
        {
            if (mappingsDataGridView.SelectedRows.Count == 0) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var selectedRow = mappingsDataGridView.SelectedRows[0];
                var displayModel = selectedRow.DataBoundItem as MappingDisplayModel;

                if (displayModel?.Mapping != null)
                {
                    using var dialog = CreateAppropriateDialog(displayModel.Mapping);
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the mapping in both lists
                        var index = _mappings.IndexOf(displayModel.Mapping);
                        if (index >= 0)
                        {
                            _mappings[index] = dialog.Mapping;
                            _displayMappings[index] = new MappingDisplayModel(dialog.Mapping);

                            // Refresh the filtered view to show the updated mapping if it matches current filters
                            ApplyAllColumnFilters();
                            UpdateColumnHeaders();

                            MarkDirty();
                        }
                    }
                }
            }, _logger, "editing mapping", this);
        }

        /// <summary>
        /// Gets the MIDI manager from the parent form
        /// </summary>
        private MidiDeviceManager? GetMidiDeviceManager()
        {
            // Try to get the MIDI manager from the service proxy
            return _midiProcessingServiceProxy?.GetMidiDeviceManager();
        }

        /// <summary>
        /// Saves the current configuration (required by BaseTabUserControl)
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        public override bool Save()
        {
            try
            {
                SaveConfiguration();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving configuration: {Message}", ex.Message);
                return false;
            }
        }

        #endregion

        #region Add Mapping Methods

        /// <summary>
        /// Adds a new mapping with sensible defaults
        /// </summary>
        private void AddNewMapping()
        {
            try
            {
                // Create a new mapping with sensible defaults - user can configure everything in the dialog
                var newMapping = new ActionMapping
                {
                    Input = new MidiInput
                    {
                        DeviceName = null, // Any device by default
                        InputType = MidiInputType.NoteOn,
                        InputNumber = 60, // Middle C
                        Channel = 1
                    },
                    Action = new Core.Actions.Simple.KeyPressReleaseAction(Keys.A), // Most common action type
                    Description = "New mapping",
                    IsEnabled = true
                };

                using var dialog = CreateAppropriateDialog(newMapping);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Add the mapping to both lists
                    _mappings.Add(dialog.Mapping);
                    _displayMappings.Add(new MappingDisplayModel(dialog.Mapping));

                    // Refresh the filtered view to show the new mapping if it matches current filters
                    ApplyAllColumnFilters();
                    UpdateColumnHeaders();

                    MarkDirty();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding new mapping: {Message}", ex.Message);
                ApplicationErrorHandler.ShowError(
                    $"Error adding new mapping: {ex.Message}",
                    "MIDIFlux - Error",
                    _logger,
                    ex,
                    this);
            }
        }

        #endregion







        #region Dialog Creation

        /// <summary>
        /// Creates the appropriate dialog for the given action mapping
        /// </summary>
        private ActionMappingDialog CreateAppropriateDialog(ActionMapping mapping)
        {
            // With the unified parameter system, we can use the base dialog for all action types
            // The automatic parameter UI generation will handle action-specific controls
            return new ActionMappingDialog(mapping, GetMidiDeviceManager());
        }

        #endregion
    }


}
