using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for editing game controller mappings
    /// </summary>
    public partial class GameControllerMappingDialog : BaseDialog
    {
        private readonly ILogger _logger;
        private readonly GameControllerMappings _mappings;
        private bool _isNewMapping;
        private bool _updatingUI = false;
        private List<GameControllerButtonMapping> _buttonMappings = new List<GameControllerButtonMapping>();
        private List<GameControllerAxisMapping> _axisMappings = new List<GameControllerAxisMapping>();

        /// <summary>
        /// Gets the edited game controller mappings
        /// </summary>
        public GameControllerMappings Mappings => _mappings;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerMappingDialog"/> class for creating new mappings
        /// </summary>
        public GameControllerMappingDialog() : this(new GameControllerMappings())
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameControllerMappingDialog"/> class for editing existing mappings
        /// </summary>
        /// <param name="mappings">The game controller mappings to edit</param>
        public GameControllerMappingDialog(GameControllerMappings mappings)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<GameControllerMappingDialog>();
            _logger.LogDebug("Initializing GameControllerMappingDialog");

            // Store the mappings
            _mappings = mappings ?? new GameControllerMappings();
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Game Controller Mappings" : "Edit Game Controller Mappings";

            // Set up event handlers
            controllerIndexNumericUpDown.ValueChanged += ControllerIndexNumericUpDown_ValueChanged;

            // Button tab event handlers
            addButtonMappingButton.Click += AddButtonMappingButton_Click;
            editButtonMappingButton.Click += EditButtonMappingButton_Click;
            deleteButtonMappingButton.Click += DeleteButtonMappingButton_Click;
            buttonMappingsListView.SelectedIndexChanged += ButtonMappingsListView_SelectedIndexChanged;
            buttonMappingsListView.DoubleClick += ButtonMappingsListView_DoubleClick;

            // Axis tab event handlers
            addAxisMappingButton.Click += AddAxisMappingButton_Click;
            editAxisMappingButton.Click += EditAxisMappingButton_Click;
            deleteAxisMappingButton.Click += DeleteAxisMappingButton_Click;
            axisMappingsListView.SelectedIndexChanged += AxisMappingsListView_SelectedIndexChanged;
            axisMappingsListView.DoubleClick += AxisMappingsListView_DoubleClick;

            // Load the mappings data
            LoadMappingsData();
        }

        /// <summary>
        /// Loads the mappings data into the UI
        /// </summary>
        private void LoadMappingsData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Set the default controller index
                    controllerIndexNumericUpDown.Value = _mappings.DefaultControllerIndex;

                    // Load the button mappings
                    _buttonMappings = new List<GameControllerButtonMapping>(_mappings.Buttons);
                    UpdateButtonMappingsListView();

                    // Load the axis mappings
                    _axisMappings = new List<GameControllerAxisMapping>(_mappings.Axes);
                    UpdateAxisMappingsListView();

                    // Update the UI state
                    UpdateUIState();
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading game controller mappings data", this);
        }

        /// <summary>
        /// Updates the button mappings list view
        /// </summary>
        private void UpdateButtonMappingsListView()
        {
            buttonMappingsListView.Items.Clear();

            foreach (var mapping in _buttonMappings)
            {
                var item = new ListViewItem(new string[]
                {
                    mapping.MidiNote.ToString(),
                    mapping.Button,
                    mapping.ControllerIndex.ToString()
                });
                item.Tag = mapping;
                buttonMappingsListView.Items.Add(item);
            }

            // Update the UI state
            UpdateUIState();
        }

        /// <summary>
        /// Updates the axis mappings list view
        /// </summary>
        private void UpdateAxisMappingsListView()
        {
            axisMappingsListView.Items.Clear();

            foreach (var mapping in _axisMappings)
            {
                var item = new ListViewItem(new string[]
                {
                    mapping.ControlNumber.ToString(),
                    mapping.Axis,
                    $"{mapping.MinValue}-{mapping.MaxValue}",
                    mapping.Invert.ToString(),
                    mapping.ControllerIndex.ToString()
                });
                item.Tag = mapping;
                axisMappingsListView.Items.Add(item);
            }

            // Update the UI state
            UpdateUIState();
        }

        /// <summary>
        /// Updates the UI state based on the current selection
        /// </summary>
        private void UpdateUIState()
        {
            // Button tab
            bool hasButtonSelection = buttonMappingsListView.SelectedItems.Count > 0;
            editButtonMappingButton.Enabled = hasButtonSelection;
            deleteButtonMappingButton.Enabled = hasButtonSelection;

            // Axis tab
            bool hasAxisSelection = axisMappingsListView.SelectedItems.Count > 0;
            editAxisMappingButton.Enabled = hasAxisSelection;
            deleteAxisMappingButton.Enabled = hasAxisSelection;
        }

        /// <summary>
        /// Saves the mappings data from the UI
        /// </summary>
        /// <returns>True if successful, false otherwise</returns>
        private bool SaveMappingsData()
        {
            try
            {
                // Get the default controller index
                _mappings.DefaultControllerIndex = (int)controllerIndexNumericUpDown.Value;

                // Set the button mappings
                _mappings.Buttons = new List<GameControllerButtonMapping>(_buttonMappings);

                // Set the axis mappings
                _mappings.Axes = new List<GameControllerAxisMapping>(_axisMappings);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving game controller mappings data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the mappings data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Validates the mappings data
        /// </summary>
        /// <returns>True if valid, false otherwise</returns>
        private bool ValidateMappings()
        {
            // No specific validation needed for game controller mappings
            return true;
        }

        #region Button Mapping Event Handlers

        /// <summary>
        /// Handles the Click event of the AddButtonMappingButton
        /// </summary>
        private void AddButtonMappingButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a new button mapping
                var mapping = new GameControllerButtonMapping
                {
                    MidiNote = 60, // Default to middle C
                    Button = "A",
                    ControllerIndex = _mappings.DefaultControllerIndex
                };

                // Edit the mapping
                using (var dialog = new GameControllerButtonMappingDialog(mapping))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Add the mapping to the list
                        _buttonMappings.Add(mapping);

                        // Update the list view
                        UpdateButtonMappingsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding button mapping");
                ApplicationErrorHandler.ShowError("An error occurred while adding a button mapping.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditButtonMappingButton
        /// </summary>
        private void EditButtonMappingButton_Click(object? sender, EventArgs e)
        {
            EditSelectedButtonMapping();
        }

        /// <summary>
        /// Handles the Click event of the DeleteButtonMappingButton
        /// </summary>
        private void DeleteButtonMappingButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (buttonMappingsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected mapping
                var selectedItem = buttonMappingsListView.SelectedItems[0];
                var mapping = selectedItem.Tag as GameControllerButtonMapping;

                if (mapping == null)
                {
                    return;
                }

                // Confirm deletion
                var result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to delete the button mapping for MIDI note {mapping.MidiNote}?",
                    "Delete Button Mapping",
                    _logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the mapping
                _buttonMappings.Remove(mapping);

                // Update the list view
                UpdateButtonMappingsListView();
            }, _logger, "deleting button mapping", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ButtonMappingsListView
        /// </summary>
        private void ButtonMappingsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Handles the DoubleClick event of the ButtonMappingsListView
        /// </summary>
        private void ButtonMappingsListView_DoubleClick(object? sender, EventArgs e)
        {
            EditSelectedButtonMapping();
        }

        /// <summary>
        /// Edits the selected button mapping
        /// </summary>
        private void EditSelectedButtonMapping()
        {
            try
            {
                if (buttonMappingsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected mapping
                var selectedItem = buttonMappingsListView.SelectedItems[0];
                var mapping = selectedItem.Tag as GameControllerButtonMapping;

                if (mapping == null)
                {
                    return;
                }

                // Edit the mapping
                using (var dialog = new GameControllerButtonMappingDialog(mapping))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the list view
                        UpdateButtonMappingsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing button mapping");
                ApplicationErrorHandler.ShowError("An error occurred while editing the button mapping.", "Error", _logger, ex, this);
            }
        }

        #endregion

        #region Axis Mapping Event Handlers

        /// <summary>
        /// Handles the Click event of the AddAxisMappingButton
        /// </summary>
        private void AddAxisMappingButton_Click(object? sender, EventArgs e)
        {
            try
            {
                // Create a new axis mapping
                var mapping = new GameControllerAxisMapping
                {
                    ControlNumber = 7, // Default to CC 7 (Volume)
                    Axis = "LeftThumbX",
                    MinValue = 0,
                    MaxValue = 127,
                    Invert = false,
                    ControllerIndex = _mappings.DefaultControllerIndex
                };

                // Edit the mapping
                using (var dialog = new GameControllerAxisMappingDialog(mapping))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Add the mapping to the list
                        _axisMappings.Add(mapping);

                        // Update the list view
                        UpdateAxisMappingsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding axis mapping");
                ApplicationErrorHandler.ShowError("An error occurred while adding an axis mapping.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles the Click event of the EditAxisMappingButton
        /// </summary>
        private void EditAxisMappingButton_Click(object? sender, EventArgs e)
        {
            EditSelectedAxisMapping();
        }

        /// <summary>
        /// Handles the Click event of the DeleteAxisMappingButton
        /// </summary>
        private void DeleteAxisMappingButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (axisMappingsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected mapping
                var selectedItem = axisMappingsListView.SelectedItems[0];
                var mapping = selectedItem.Tag as GameControllerAxisMapping;

                if (mapping == null)
                {
                    return;
                }

                // Confirm deletion
                var result = ApplicationErrorHandler.ShowConfirmation(
                    $"Are you sure you want to delete the axis mapping for control {mapping.ControlNumber}?",
                    "Delete Axis Mapping",
                    _logger,
                    DialogResult.No,
                    this);

                if (result != DialogResult.Yes)
                {
                    return;
                }

                // Remove the mapping
                _axisMappings.Remove(mapping);

                // Update the list view
                UpdateAxisMappingsListView();
            }, _logger, "deleting axis mapping", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the AxisMappingsListView
        /// </summary>
        private void AxisMappingsListView_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateUIState();
        }

        /// <summary>
        /// Handles the DoubleClick event of the AxisMappingsListView
        /// </summary>
        private void AxisMappingsListView_DoubleClick(object? sender, EventArgs e)
        {
            EditSelectedAxisMapping();
        }

        /// <summary>
        /// Edits the selected axis mapping
        /// </summary>
        private void EditSelectedAxisMapping()
        {
            try
            {
                if (axisMappingsListView.SelectedItems.Count == 0)
                {
                    return;
                }

                // Get the selected mapping
                var selectedItem = axisMappingsListView.SelectedItems[0];
                var mapping = selectedItem.Tag as GameControllerAxisMapping;

                if (mapping == null)
                {
                    return;
                }

                // Edit the mapping
                using (var dialog = new GameControllerAxisMappingDialog(mapping))
                {
                    if (dialog.ShowDialog(this) == DialogResult.OK)
                    {
                        // Update the list view
                        UpdateAxisMappingsListView();
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error editing axis mapping");
                ApplicationErrorHandler.ShowError("An error occurred while editing the axis mapping.", "Error", _logger, ex, this);
            }
        }

        #endregion

        #region Other Event Handlers

        /// <summary>
        /// Handles the ValueChanged event of the ControllerIndexNumericUpDown
        /// </summary>
        private void ControllerIndexNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI)
                return;

            // Update the default controller index
            _mappings.DefaultControllerIndex = (int)controllerIndexNumericUpDown.Value;
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        private void okButton_Click(object sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the mappings data
                if (!SaveMappingsData())
                    return;

                // Validate the mappings data
                if (!ValidateMappings())
                    return;

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving game controller mappings", this);
        }

        #endregion
    }
}
