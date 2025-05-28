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
        private readonly ActionFactory _actionFactory;

        /// <summary>
        /// Gets a value indicating whether the configuration has unsaved changes
        /// </summary>
        public bool IsDirty => HasUnsavedChanges;
        private readonly MidiProcessingServiceProxy _midiProcessingServiceProxy;
        private readonly ILogger _logger;

        private ProfileModel _profile;
        private MappingConfig _configuration = new();
        private BindingList<ActionMapping> _mappings;
        private BindingList<MappingDisplayModel> _displayMappings;



        // Column filtering state
        private readonly Dictionary<string, HashSet<string>> _columnFilters = new();
        private readonly Dictionary<string, string> _columnFilterValues = new();

        /// <summary>
        /// Gets the profile associated with this editor
        /// </summary>
        public ProfileModel Profile => _profile;

        /// <summary>
        /// Initializes a new instance of the ProfileEditorControl class
        /// </summary>
        /// <param name="profile">The profile to edit</param>
        /// <param name="midiProcessingServiceProxy">The MIDI processing service proxy</param>
        public ProfileEditorControl(ProfileModel profile, MidiProcessingServiceProxy? midiProcessingServiceProxy = null)
        {
            try
            {
                InitializeComponent();

                _profile = profile ?? throw new ArgumentNullException(nameof(profile));
                _logger = LoggingHelper.CreateLogger<ProfileEditorControl>();

                // Set the tab title
                TabTitle = $"Edit: {profile.Name}";

                // Create the unified configuration loader and factory for GUI context
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                _actionFactory = ActionFactory.CreateForGui(factoryLogger);
                var configurationService = new ConfigurationService(LoggingHelper.CreateLogger<ConfigurationService>());
                _configLoader = new ActionConfigurationLoader(_logger, _actionFactory, configurationService);

                // Use the provided MidiProcessingServiceProxy or create a new one
                _midiProcessingServiceProxy = midiProcessingServiceProxy ??
                    new MidiProcessingServiceProxy(LoggingHelper.CreateLogger<MidiProcessingServiceProxy>());

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
                // Show a context menu with mapping types
                var menu = new ContextMenuStrip();
                menu.Items.Add("Key Mapping", null, (s, e) => AddNewKeyMapping());
                menu.Items.Add("Mouse Mapping", null, (s, e) => AddNewMouseMapping());
                menu.Items.Add("Command Mapping", null, (s, e) => AddNewCommandMapping());
                menu.Items.Add("Game Controller Mapping", null, (s, e) => AddNewGameControllerMapping());
                menu.Items.Add("Sequence (Macro)", null, (s, e) => AddNewSequenceMapping());
                menu.Items.Add("Conditional (CC Range)", null, (s, e) => AddNewConditionalMapping());

                // Show the menu at the button's location
                var buttonBounds = addMappingButton.Bounds;
                var toolStrip = addMappingButton.Owner;
                if (toolStrip != null)
                {
                    var screenPoint = toolStrip.PointToScreen(new Point(buttonBounds.X, buttonBounds.Bottom));
                    menu.Show(screenPoint);
                }
                else
                {
                    menu.Show(this, new Point(10, 10)); // Fallback position
                }
            }, _logger, "showing add mapping menu", this);
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
        /// Handles column header clicks for filtering
        /// </summary>
        private void MappingsDataGridView_ColumnHeaderMouseClick(object? sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var column = mappingsDataGridView.Columns[e.ColumnIndex];
                ShowColumnFilterMenu(column);
            }
        }

        #endregion

        #region Helper Methods



        /// <summary>
        /// Sets up column filtering functionality
        /// </summary>
        private void SetupColumnFiltering()
        {
            // Enable sorting for all columns
            foreach (DataGridViewColumn column in mappingsDataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.Automatic;
            }
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
        }

        /// <summary>
        /// Clears the filter for a specific column
        /// </summary>
        private void ClearColumnFilter(string columnName)
        {
            _columnFilterValues.Remove(columnName);
            ApplyAllColumnFilters();
        }

        /// <summary>
        /// Applies all active column filters
        /// </summary>
        private void ApplyAllColumnFilters()
        {
            if (_columnFilterValues.Count == 0)
            {
                // No filters active, show all mappings
                mappingsDataGridView.DataSource = _displayMappings;
                return;
            }

            // Apply all active filters
            var filteredMappings = _displayMappings.Where(item =>
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
            }).ToList();

            mappingsDataGridView.DataSource = new BindingList<MappingDisplayModel>(filteredMappings);
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
                            MarkDirty();
                        }
                    }
                }
            }, _logger, "editing mapping", this);
        }

        /// <summary>
        /// Gets the MIDI manager from the parent form
        /// </summary>
        private MidiManager? GetMidiManager()
        {
            // Try to get the MIDI manager from the service proxy
            return _midiProcessingServiceProxy?.GetMidiManager();
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
        /// Adds a new key mapping
        /// </summary>
        private void AddNewKeyMapping()
        {
            AddNewMapping(new KeyPressReleaseConfig { VirtualKeyCode = 65 }); // Default to 'A' key
        }

        /// <summary>
        /// Adds a new mouse mapping
        /// </summary>
        private void AddNewMouseMapping()
        {
            AddNewMapping(new MouseClickConfig { Button = MouseButton.Left });
        }

        /// <summary>
        /// Adds a new command mapping
        /// </summary>
        private void AddNewCommandMapping()
        {
            AddNewMapping(new CommandExecutionConfig { Command = "echo Hello", ShellType = CommandShellType.PowerShell });
        }

        /// <summary>
        /// Adds a new game controller mapping
        /// </summary>
        private void AddNewGameControllerMapping()
        {
            AddNewMapping(new GameControllerButtonConfig { Button = "A", ControllerIndex = 0 });
        }

        /// <summary>
        /// Adds a new sequence (macro) mapping
        /// </summary>
        private void AddNewSequenceMapping()
        {
            var sequenceConfig = new SequenceConfig
            {
                ErrorHandling = SequenceErrorHandling.ContinueOnError,
                SubActions = new List<ActionConfig>
                {
                    new KeyPressReleaseConfig { VirtualKeyCode = 65 } // Default action
                }
            };
            AddNewMapping(sequenceConfig);
        }

        /// <summary>
        /// Adds a new conditional (CC range) mapping
        /// </summary>
        private void AddNewConditionalMapping()
        {
            var conditionalConfig = new ConditionalConfig
            {
                Conditions = new List<ValueConditionConfig>
                {
                    new ValueConditionConfig
                    {
                        MinValue = 0,
                        MaxValue = 63,
                        Action = new KeyPressReleaseConfig { VirtualKeyCode = 65 }
                    },
                    new ValueConditionConfig
                    {
                        MinValue = 64,
                        MaxValue = 127,
                        Action = new KeyPressReleaseConfig { VirtualKeyCode = 66 }
                    }
                }
            };
            AddNewMapping(conditionalConfig);
        }

        /// <summary>
        /// Adds a new mapping with the specified action configuration
        /// </summary>
        private void AddNewMapping(ActionConfig actionConfig)
        {
            try
            {
                // Create a new mapping with default MIDI input
                var newMapping = new ActionMapping
                {
                    Input = new ActionMidiInput
                    {
                        DeviceName = null, // Any device by default
                        InputType = ActionMidiInputType.NoteOn,
                        InputNumber = 60, // Middle C
                        Channel = 1
                    },
                    Action = _actionFactory.CreateAction(actionConfig),
                    Description = "New mapping",
                    IsEnabled = true
                };

                using var dialog = CreateAppropriateDialog(newMapping);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    // Add the mapping to both lists
                    _mappings.Add(dialog.Mapping);
                    _displayMappings.Add(new MappingDisplayModel(dialog.Mapping));
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







        #region Conversion Methods



        /// <summary>
        /// Creates the appropriate dialog for the given action mapping
        /// </summary>
        private ActionMappingDialog CreateAppropriateDialog(ActionMapping mapping)
        {
            // Determine if this is a key action that should use the specialized dialog
            if (IsKeyAction(mapping.Action))
            {
                return new KeyMappingDialog(mapping, GetMidiManager());
            }

            // For all other action types, use the base dialog
            return new ActionMappingDialog(mapping, GetMidiManager());
        }

        /// <summary>
        /// Determines if the given action is a key-related action
        /// </summary>
        private static bool IsKeyAction(IAction action)
        {
            return action.GetType().Name switch
            {
                "KeyPressReleaseAction" => true,
                "KeyDownAction" => true,
                "KeyUpAction" => true,
                "KeyToggleAction" => true,
                _ => false
            };
        }

        /// <summary>
        /// Converts an IAction back to its configuration representation
        /// </summary>
        private ActionConfig ConvertActionToConfig(IAction action)
        {
            // Extract actual configuration values from the action instance
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
                    AutoReleaseAfterMs = keyDownAction.AutoReleaseAfterMs,
                    Description = keyDownAction.Description
                },
                Core.Actions.Simple.KeyUpAction keyUpAction => new KeyUpConfig
                {
                    VirtualKeyCode = keyUpAction.VirtualKeyCode,
                    Description = keyUpAction.Description
                },
                Core.Actions.Simple.KeyToggleAction keyToggleAction => new KeyToggleConfig
                {
                    VirtualKeyCode = keyToggleAction.VirtualKeyCode,
                    Description = keyToggleAction.Description
                },
                Core.Actions.Simple.MouseClickAction mouseClickAction => new MouseClickConfig
                {
                    Button = mouseClickAction.Button,
                    Description = mouseClickAction.Description
                },
                Core.Actions.Simple.MouseScrollAction mouseScrollAction => new MouseScrollConfig
                {
                    Direction = mouseScrollAction.Direction,
                    Amount = mouseScrollAction.Amount,
                    Description = mouseScrollAction.Description
                },
                Core.Actions.Simple.CommandExecutionAction commandAction => new CommandExecutionConfig
                {
                    Command = commandAction.Command,
                    ShellType = commandAction.ShellType,
                    Description = commandAction.Description
                },
                Core.Actions.Simple.DelayAction delayAction => new DelayConfig
                {
                    Milliseconds = delayAction.Milliseconds,
                    Description = delayAction.Description
                },
                Core.Actions.Simple.GameControllerButtonAction gameButtonAction => new GameControllerButtonConfig
                {
                    Button = gameButtonAction.Button,
                    ControllerIndex = gameButtonAction.ControllerIndex,
                    Description = gameButtonAction.Description
                },
                Core.Actions.Simple.GameControllerAxisAction gameAxisAction => new GameControllerAxisConfig
                {
                    AxisName = gameAxisAction.AxisName,
                    AxisValue = gameAxisAction.AxisValue,
                    ControllerIndex = gameAxisAction.ControllerIndex,
                    Description = gameAxisAction.Description
                },
                // Complex actions - these need special handling but for now create basic configs
                _ when action.GetType().Name == "SequenceAction" => new SequenceConfig { ErrorHandling = SequenceErrorHandling.ContinueOnError, SubActions = new List<ActionConfig>() },
                _ when action.GetType().Name == "ConditionalAction" => new ConditionalConfig { Conditions = new List<ValueConditionConfig>() },
                _ => new KeyPressReleaseConfig { VirtualKeyCode = 65 } // Default fallback
            };
        }

        #endregion
    }


}
