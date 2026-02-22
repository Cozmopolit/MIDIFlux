using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Keyboard;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.Core.Mouse;
using MIDIFlux.GUI.Controls.ProfileEditor;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Helper class for MIDI input type combo box items with display names.
    /// </summary>
    public class InputTypeComboBoxItem
    {
        public MidiInputType InputType { get; }
        public string DisplayName { get; }

        public InputTypeComboBoxItem(MidiInputType inputType, string displayName)
        {
            InputType = inputType;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Helper class for action category combo box items with display names.
    /// </summary>
    public class CategoryComboBoxItem
    {
        public ActionCategory Category { get; }
        public string DisplayName { get; }

        public CategoryComboBoxItem(ActionCategory category, string displayName)
        {
            Category = category;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Helper class for action type combo box items with type name and display name.
    /// </summary>
    public class ActionTypeComboBoxItem
    {
        public string TypeName { get; }
        public string DisplayName { get; }

        public ActionTypeComboBoxItem(string typeName, string displayName)
        {
            TypeName = typeName;
            DisplayName = displayName;
        }

        public override string ToString() => DisplayName;
    }

    /// <summary>
    /// Base dialog class for creating and editing action mappings.
    /// Provides common functionality for MIDI input configuration and action selection.
    /// </summary>
    public partial class ActionMappingDialog : BaseDialog
    {
        protected readonly ActionMapping _mapping;
        protected readonly MidiDeviceManager? _MidiDeviceManager;
        protected bool _isNewMapping;
        protected bool _updatingUI = false;
        protected bool _isListening = false;
        protected bool _actionOnly = false;

        // Keyboard listening fields
        protected KeyboardListener? _keyboardListener;
        protected bool _isKeyListening = false;
        protected string? _keyListeningParameterName;
        protected ComboBox? _keyListeningComboBox;

        // Change tracking for unsaved changes warning
        protected bool _hasUnsavedChanges = false;

        /// <summary>
        /// Gets the edited action mapping
        /// </summary>
        public ActionMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="MidiDeviceManager">Optional MidiDeviceManager for MIDI listening functionality</param>
        protected ActionMappingDialog(MidiDeviceManager? MidiDeviceManager = null)
            : this(CreateDefaultMapping(), MidiDeviceManager)
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="MidiDeviceManager">Optional MidiDeviceManager for MIDI listening functionality</param>
        /// <param name="logger">The logger to use for this dialog</param>
        public ActionMappingDialog(ActionMapping mapping, MidiDeviceManager? MidiDeviceManager, ILogger<ActionMappingDialog> logger)
            : this(mapping, MidiDeviceManager, false, logger)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for editing an existing mapping (with LoggingHelper fallback)
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="MidiDeviceManager">Optional MidiDeviceManager for MIDI listening functionality</param>
        public ActionMappingDialog(ActionMapping mapping, MidiDeviceManager? MidiDeviceManager)
            : this(mapping, MidiDeviceManager, false, LoggingHelper.CreateLogger<ActionMappingDialog>())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="MidiDeviceManager">Optional MidiDeviceManager for MIDI listening functionality</param>
        /// <param name="actionOnly">If true, only show action configuration (hide MIDI input configuration)</param>
        /// <param name="logger">The logger to use for this dialog</param>
        public ActionMappingDialog(ActionMapping mapping, MidiDeviceManager? MidiDeviceManager, bool actionOnly, ILogger<ActionMappingDialog> logger) : base(logger)
        {
            _logger.LogDebug("Initializing ActionMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            _MidiDeviceManager = MidiDeviceManager;
            _isNewMapping = false;
            _actionOnly = actionOnly;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Mapping" : "Edit Mapping";

            // Hide MIDI input configuration if actionOnly is true
            if (actionOnly)
            {
                Text = "Configure Action";
                HideMidiInputControls();
            }

            // Set up common event handlers
            SetupEventHandlers();

            // Load the mapping data
            LoadMappingData();
        }

        /// <summary>
        /// Creates a default mapping for new mappings
        /// </summary>
        private static ActionMapping CreateDefaultMapping()
        {
            // Create a simple default action
            var action = new Core.Actions.Simple.KeyPressReleaseAction(); // 'A' key (default)

            return new ActionMapping
            {
                Input = new MidiInput
                {
                    InputType = MidiInputType.NoteOn,
                    InputNumber = 60, // Middle C
                    Channel = null, // Any channel
                    DeviceName = null // Any device
                },
                Action = action,
                Description = "New Mapping",
                IsEnabled = true
            };
        }

        /// <summary>
        /// Sets up common event handlers for all unified dialogs
        /// </summary>
        protected virtual void SetupEventHandlers()
        {
            // MIDI input event handlers
            midiInputTypeComboBox.SelectedIndexChanged += MidiInputTypeComboBox_SelectedIndexChanged;
            midiInputNumberNumericUpDown.ValueChanged += MidiInputNumberNumericUpDown_ValueChanged;
            midiChannelComboBox.SelectedIndexChanged += MidiChannelComboBox_SelectedIndexChanged;
            deviceNameComboBox.SelectedIndexChanged += DeviceNameComboBox_SelectedIndexChanged;

            // Action configuration event handlers
            categoryComboBox.SelectedIndexChanged += CategoryComboBox_SelectedIndexChanged;
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;

            // Common button event handlers
            listenButton.Click += ListenButton_Click;
            testButton.Click += TestButton_Click;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;

            // Description event handler
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            enabledCheckBox.CheckedChanged += EnabledCheckBox_CheckedChanged;

            // Form closing event handler for unsaved changes warning
            FormClosing += ActionMappingDialog_FormClosing;
        }

        /// <summary>
        /// Loads the mapping data into the UI controls
        /// </summary>
        protected virtual void LoadMappingData()
        {
            _logger.LogDebug("LoadMappingData called - ActionOnly={ActionOnly}, ActionType={ActionType}",
                _actionOnly, _mapping.Action?.GetType().Name ?? "null");

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load MIDI input data (skip if actionOnly mode)
                    if (!_actionOnly)
                    {
                        _logger.LogDebug("Loading MIDI input data");
                        LoadMidiInputData();
                    }

                    // Load action data
                    _logger.LogDebug("Loading action data");
                    LoadActionData();

                    // Load common properties
                    // Always load description (needed for sub-actions), but skip enabled checkbox in actionOnly mode
                    descriptionTextBox.Text = _mapping.Description ?? string.Empty;
                    if (!_actionOnly)
                    {
                        enabledCheckBox.Checked = _mapping.IsEnabled;
                    }

                    _logger.LogInformation("Mapping data loaded successfully");
                }
                finally
                {
                    _updatingUI = false;
                }
            }, _logger, "loading unified mapping data", this);
        }

        /// <summary>
        /// Loads MIDI input configuration into the UI
        /// </summary>
        protected virtual void LoadMidiInputData()
        {
            // Populate MIDI input type combo box with user-friendly names
            midiInputTypeComboBox.Items.Clear();
            var inputTypeDisplayNames = GetInputTypeDisplayNames();
            foreach (MidiInputType inputType in Enum.GetValues<MidiInputType>())
            {
                var displayName = inputTypeDisplayNames.GetValueOrDefault(inputType, inputType.ToString());
                midiInputTypeComboBox.Items.Add(new InputTypeComboBoxItem(inputType, displayName));
            }

            // Select the current input type
            for (int i = 0; i < midiInputTypeComboBox.Items.Count; i++)
            {
                if (midiInputTypeComboBox.Items[i] is InputTypeComboBoxItem item &&
                    item.InputType == _mapping.Input.InputType)
                {
                    midiInputTypeComboBox.SelectedIndex = i;
                    break;
                }
            }

            // Set input number
            midiInputNumberNumericUpDown.Value = _mapping.Input.InputNumber;

            // Populate channel combo box
            midiChannelComboBox.Items.Clear();
            midiChannelComboBox.Items.Add("Any Channel");
            for (int i = 1; i <= 16; i++)
            {
                midiChannelComboBox.Items.Add($"Channel {i}");
            }
            midiChannelComboBox.SelectedIndex = _mapping.Input.Channel ?? 0;

            // Populate device name combo box using centralized helper
            Helpers.MidiDeviceComboBoxHelper.PopulateDeviceComboBox(
                deviceNameComboBox,
                _MidiDeviceManager,
                _logger,
                includeAnyDevice: true,
                selectedDeviceName: _mapping.Input.DeviceName);

            // Device selection is now handled by the helper method above
        }

        /// <summary>
        /// Loads action configuration into the UI
        /// </summary>
        protected virtual void LoadActionData()
        {
            // Prevent event handlers from firing during initial load
            _updatingUI = true;
            try
            {
                // Populate category combo box first
                PopulateCategoryComboBox();

                // Get the current action's category and select it
                var actionTypeName = GetActionTypeName(_mapping.Action);
                var actionCategory = ActionTypeRegistry.Instance.GetActionCategory(actionTypeName);

                // Find and select the matching category
                for (int i = 0; i < categoryComboBox.Items.Count; i++)
                {
                    if (categoryComboBox.Items[i] is CategoryComboBoxItem item && item.Category == actionCategory)
                    {
                        categoryComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // Populate action types for the selected category
                PopulateActionTypeComboBox();

                // Select the current action type
                var displayName = ActionTypeRegistry.Instance.GetActionDisplayName(actionTypeName);
                for (int i = 0; i < actionTypeComboBox.Items.Count; i++)
                {
                    if (actionTypeComboBox.Items[i] is ActionTypeComboBoxItem actionItem && actionItem.DisplayName == displayName)
                    {
                        actionTypeComboBox.SelectedIndex = i;
                        break;
                    }
                }

                // Load action-specific parameters
                LoadActionParameters();
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Populates the category combo box with available action categories.
        /// </summary>
        protected virtual void PopulateCategoryComboBox()
        {
            categoryComboBox.Items.Clear();

            // Get all categories that have at least one action
            var categories = ActionTypeRegistry.Instance.GetAllCategories();

            foreach (var category in categories)
            {
                var displayName = ActionTypeRegistry.GetCategoryDisplayName(category);
                categoryComboBox.Items.Add(new CategoryComboBoxItem(category, displayName));
            }

            // Select first category by default
            if (categoryComboBox.Items.Count > 0)
            {
                categoryComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Populates the action type combo box with actions from the selected category.
        /// Filters actions based on the selected MIDI input type's compatibility.
        /// </summary>
        protected virtual void PopulateActionTypeComboBox()
        {
            actionTypeComboBox.Items.Clear();

            // Get the selected category
            if (categoryComboBox.SelectedItem is not CategoryComboBoxItem selectedCategoryItem)
            {
                return;
            }

            var selectedActionCategory = selectedCategoryItem.Category;

            // Get the selected input type category for filtering
            InputTypeCategory? selectedInputCategory = null;
            if (midiInputTypeComboBox.SelectedItem is InputTypeComboBoxItem selectedItem)
            {
                selectedInputCategory = selectedItem.InputType.GetCategory();
            }

            // Get all actions in the selected category
            var actionsInCategory = ActionTypeRegistry.Instance.GetActionsByCategory(selectedActionCategory);

            // Filter and sort actions
            var filteredActions = new List<(string TypeName, string DisplayName)>();

            foreach (var kvp in actionsInCategory)
            {
                var actionTypeName = kvp.Key;
                var displayName = kvp.Value;

                // If input category selected, check compatibility
                if (selectedInputCategory != null && !IsActionCompatibleWithCategory(actionTypeName, selectedInputCategory.Value))
                {
                    continue; // Skip incompatible actions
                }

                filteredActions.Add((actionTypeName, displayName));
            }

            // Sort alphabetically by display name
            filteredActions.Sort((a, b) => string.Compare(a.DisplayName, b.DisplayName, StringComparison.OrdinalIgnoreCase));

            // Add actions to combo box
            foreach (var (typeName, displayName) in filteredActions)
            {
                actionTypeComboBox.Items.Add(new ActionTypeComboBoxItem(typeName, displayName));
            }

            // Select first item by default
            if (actionTypeComboBox.Items.Count > 0)
            {
                actionTypeComboBox.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Gets a dictionary mapping MIDI input types to their user-friendly display names.
        /// </summary>
        /// <returns>Dictionary of input type to display name</returns>
        protected virtual Dictionary<MidiInputType, string> GetInputTypeDisplayNames()
        {
            return new Dictionary<MidiInputType, string>
            {
                { MidiInputType.NoteOn, "Note On" },
                { MidiInputType.NoteOff, "Note Off" },
                { MidiInputType.ControlChangeAbsolute, "Control Change (Absolute)" },
                { MidiInputType.ControlChangeRelative, "Control Change (Relative)" },
                { MidiInputType.PitchBend, "Pitch Bend" },
                { MidiInputType.PolyphonicKeyPressure, "Polyphonic Key Pressure" },
                { MidiInputType.ChannelPressure, "Channel Pressure" },
                { MidiInputType.SysEx, "SysEx" },
                { MidiInputType.ProgramChange, "Program Change" }
            };
        }

        /// <summary>
        /// Gets a dictionary mapping action type names to their display names using the registry.
        /// This eliminates hardcoded action type dependencies.
        /// </summary>
        /// <returns>Dictionary of action type name to display name</returns>
        protected virtual Dictionary<string, string> GetActionDisplayNames()
        {
            return ActionTypeRegistry.Instance.GetAllActionDisplayNames().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }



        /// <summary>
        /// Checks if an action type is compatible with the specified input type category.
        /// Creates an instance of the action and calls GetCompatibleInputCategories() method.
        /// </summary>
        /// <param name="actionTypeName">The action type name (e.g., "KeyPressReleaseAction")</param>
        /// <param name="category">The input type category to check compatibility with</param>
        /// <returns>True if the action is compatible with the category, false otherwise</returns>
        protected virtual bool IsActionCompatibleWithCategory(string actionTypeName, InputTypeCategory category)
        {
            try
            {
                // Create an instance using the registry
                var actionInstance = ActionTypeRegistry.Instance.CreateActionInstance(actionTypeName);
                if (actionInstance == null)
                {
                    _logger.LogWarning("Could not create instance of action type: {ActionTypeName}", actionTypeName);
                    return false;
                }

                // Check if the action supports this input category
                var compatibleCategories = actionInstance.GetCompatibleInputCategories();
                return compatibleCategories.Contains(category);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking action compatibility for {ActionTypeName} with category {Category}",
                    actionTypeName, category);
                return false;
            }
        }

        /// <summary>
        /// Selects the specified input type in the combo box.
        /// </summary>
        /// <param name="inputType">The input type to select</param>
        protected virtual void SelectInputTypeInComboBox(MidiInputType inputType)
        {
            for (int i = 0; i < midiInputTypeComboBox.Items.Count; i++)
            {
                if (midiInputTypeComboBox.Items[i] is InputTypeComboBoxItem item &&
                    item.InputType == inputType)
                {
                    midiInputTypeComboBox.SelectedIndex = i;
                    break;
                }
            }
        }



        /// <summary>
        /// Gets the type name for an action (e.g., "SequenceAction", "KeyPressReleaseAction").
        /// </summary>
        protected virtual string GetActionTypeName(IAction action)
        {
            return action.GetType().Name;
        }

        /// <summary>
        /// Loads action-specific parameters into the UI using automatic parameter UI generation
        /// </summary>
        protected virtual void LoadActionParameters()
        {
            if (actionParametersPanel == null)
            {
                _logger.LogError("actionParametersPanel is null!");
                return;
            }

            // Debug panel information
            _logger.LogDebug("actionParametersPanel - Visible: {Visible}, Size: {Width}x{Height}, Location: {X},{Y}",
                actionParametersPanel.Visible, actionParametersPanel.Width, actionParametersPanel.Height,
                actionParametersPanel.Location.X, actionParametersPanel.Location.Y);

            // Suspend layout during control changes
            actionParametersPanel.SuspendLayout();

            try
            {
                // Clear existing controls
                actionParametersPanel.Controls.Clear();

                if (_mapping.Action == null)
                    return;

                // Debug logging to help diagnose the issue
                _logger.LogDebug("Loading parameters for action type {ActionType}", _mapping.Action.GetType().Name);

                // Use automatic parameter UI generation for all action types
                var parameterInfos = ((ActionBase)_mapping.Action).GetParameterList();

                _logger.LogDebug("Found {ParameterCount} parameters for action {ActionType}",
                    parameterInfos.Count, _mapping.Action.GetType().Name);

                if (parameterInfos.Count == 0)
                {
                    // No parameters to configure
                    var label = new Label
                    {
                        Text = "This action has no configurable parameters.",
                        AutoSize = true,
                        Location = new System.Drawing.Point(10, 10),
                        ForeColor = SystemColors.GrayText
                    };
                    actionParametersPanel.Controls.Add(label);
                    return;
                }

                // Create controls for each parameter using a vertical stack approach
                var yPosition = 5;
                foreach (var parameterInfo in parameterInfos)
                {
                    _logger.LogDebug("Creating control for parameter {ParameterName} of type {ParameterType}",
                        parameterInfo.Name, parameterInfo.Type);

                    var parameterPanel = ParameterControlFactory.CreateLabeledParameterControl(
                        parameterInfo, (ActionBase)_mapping.Action, _logger);

                    // Force layout to get accurate auto-sized height
                    parameterPanel.PerformLayout();

                    parameterPanel.Location = new System.Drawing.Point(5, yPosition);
                    parameterPanel.Width = actionParametersPanel.Width - 25; // Account for scrollbar
                    parameterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                    // Use the actual height after layout, with minimum of 32
                    var controlHeight = Math.Max(parameterPanel.PreferredSize.Height, 32);
                    parameterPanel.Height = controlHeight;

                    _logger.LogDebug("Adding parameter panel at position {X},{Y} with size {Width}x{Height}",
                        parameterPanel.Location.X, parameterPanel.Location.Y, parameterPanel.Width, parameterPanel.Height);

                    actionParametersPanel.Controls.Add(parameterPanel);
                    yPosition += controlHeight + 3;
                }

                _logger.LogDebug("Successfully loaded {ControlCount} parameter controls, final panel size: {Width}x{Height}",
                    actionParametersPanel.Controls.Count, actionParametersPanel.Width, actionParametersPanel.Height);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading action parameters for action type {ActionType}",
                    _mapping.Action.GetType().Name);

                // Show error message
                var errorLabel = new Label
                {
                    Text = $"Error loading parameters: {ex.Message}",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                    ForeColor = Color.Red
                };
                actionParametersPanel.Controls.Add(errorLabel);
            }
            finally
            {
                actionParametersPanel.ResumeLayout(true);
            }
        }







        /// <summary>
        /// Saves the mapping data from the UI controls
        /// </summary>
        protected virtual bool SaveMappingData()
        {
            _logger.LogDebug("SaveMappingData called - ActionOnly={ActionOnly}, ActionType={ActionType}",
                _actionOnly, _mapping.Action?.GetType().Name ?? "null");

            try
            {
                // Save MIDI input data (skip if actionOnly mode)
                if (!_actionOnly)
                {
                    _logger.LogDebug("Saving MIDI input data");
                    if (!SaveMidiInputData())
                    {
                        _logger.LogWarning("SaveMidiInputData returned false");
                        return false;
                    }
                }

                // Save action data
                _logger.LogDebug("Saving action data");
                if (!SaveActionData())
                {
                    _logger.LogWarning("SaveActionData returned false");
                    return false;
                }

                // Save common properties (skip if actionOnly mode)
                if (!_actionOnly)
                {
                    _mapping.Description = descriptionTextBox.Text.Trim();
                    _mapping.IsEnabled = enabledCheckBox.Checked;
                    _logger.LogDebug("Saved common properties - Description={Description}, Enabled={Enabled}",
                        _mapping.Description, _mapping.IsEnabled);
                }

                _logger.LogInformation("Mapping data saved successfully");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving unified mapping data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the mapping data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Saves MIDI input configuration from the UI
        /// </summary>
        protected virtual bool SaveMidiInputData()
        {
            try
            {
                // Save input type
                if (midiInputTypeComboBox.SelectedItem is InputTypeComboBoxItem item)
                {
                    _mapping.Input.InputType = item.InputType;
                }

                // Save input number
                _mapping.Input.InputNumber = (int)midiInputNumberNumericUpDown.Value;

                // Save channel
                _mapping.Input.Channel = midiChannelComboBox.SelectedIndex == 0 ? null : midiChannelComboBox.SelectedIndex;

                // Save device name using centralized helper
                _mapping.Input.DeviceName = Helpers.MidiDeviceComboBoxHelper.GetSelectedDeviceName(deviceNameComboBox);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving MIDI input data");
                ApplicationErrorHandler.ShowError("An error occurred while saving the MIDI input data.", "Error", _logger, ex, this);
                return false;
            }
        }

        /// <summary>
        /// Saves action configuration from the UI
        /// </summary>
        protected virtual bool SaveActionData()
        {
            // Action parameters are already saved by event handlers in ParameterControlFactory
            // when ComboBox/TextBox/etc. values change. The event handlers call action.SetParameterValue()
            // directly, so the Action object already contains the current parameter values.
            // This method is intentionally empty - no additional save logic is needed.

            _logger.LogDebug("SaveActionData called - Action parameters already saved by event handlers");
            return true;
        }

        /// <summary>
        /// Validates the mapping data
        /// </summary>
        protected virtual bool ValidateMapping()
        {
            // Validate MIDI input using centralized helper (skip if actionOnly mode)
            if (!_actionOnly)
            {
                if (!Helpers.MidiValidationHelper.ValidateMidiInput(
                    _mapping.Input.InputNumber,
                    _mapping.Input.Channel,
                    _mapping.Input.InputType.ToString(),
                    _logger,
                    this))
                {
                    return false;
                }
            }

            // Validate action
            if (_mapping.Action == null)
            {
                ApplicationErrorHandler.ShowError("An action must be selected.", "Validation Error", _logger, null, this);
                return false;
            }

            return true;
        }

        #region Event Handlers

        /// <summary>
        /// Handles the SelectedIndexChanged event of the MidiInputTypeComboBox
        /// </summary>
        protected virtual void MidiInputTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Update the input type in the mapping
                if (midiInputTypeComboBox.SelectedItem is InputTypeComboBoxItem item)
                {
                    _mapping.Input.InputType = item.InputType;

                    // Refresh action type dropdown to show only compatible actions
                    PopulateActionTypeComboBox();
                }
            }, _logger, "changing MIDI input type", this);
        }

        /// <summary>
        /// Handles the ValueChanged event of the MidiInputNumberNumericUpDown
        /// </summary>
        protected virtual void MidiInputNumberNumericUpDown_ValueChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _mapping.Input.InputNumber = (int)midiInputNumberNumericUpDown.Value;
            }, _logger, "changing MIDI input number", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the MidiChannelComboBox
        /// </summary>
        protected virtual void MidiChannelComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _mapping.Input.Channel = midiChannelComboBox.SelectedIndex == 0 ? null : midiChannelComboBox.SelectedIndex;
            }, _logger, "changing MIDI channel", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the DeviceNameComboBox
        /// </summary>
        protected virtual void DeviceNameComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _mapping.Input.DeviceName = Helpers.MidiDeviceComboBoxHelper.GetSelectedDeviceName(deviceNameComboBox);
            }, _logger, "changing device name", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the CategoryComboBox
        /// </summary>
        protected virtual void CategoryComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Repopulate action types for the newly selected category
                PopulateActionTypeComboBox();
            }, _logger, "changing action category", this);
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ActionTypeComboBox
        /// </summary>
        protected virtual void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a new action based on the selected type
                if (actionTypeComboBox.SelectedItem is ActionTypeComboBoxItem selectedItem)
                {
                    CreateActionFromTypeName(selectedItem.TypeName);
                    LoadActionParameters();
                    _hasUnsavedChanges = true;
                }
            }, _logger, "changing action type", this);
        }

        /// <summary>
        /// Creates a new action instance based on the action type name using the registry.
        /// </summary>
        protected virtual void CreateActionFromTypeName(string actionTypeName)
        {
            if (string.IsNullOrEmpty(actionTypeName))
            {
                _logger.LogWarning("Empty action type name provided, using default");
                _mapping.Action = new Core.Actions.Simple.KeyPressReleaseAction();
                return;
            }

            // Create the action instance using the registry
            var actionInstance = ActionTypeRegistry.Instance.CreateActionInstance(actionTypeName);
            if (actionInstance == null)
            {
                _logger.LogWarning("Could not create action instance for type '{ActionTypeName}', using default", actionTypeName);
                _mapping.Action = new Core.Actions.Simple.KeyPressReleaseAction(); // Default fallback
                return;
            }

            _mapping.Action = actionInstance;
        }









        /// <summary>
        /// Handles the TextChanged event of the DescriptionTextBox
        /// </summary>
        protected virtual void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var description = descriptionTextBox.Text.Trim();
                _mapping.Description = description;

                // In actionOnly mode, also update the action's description directly
                if (_actionOnly && _mapping.Action is ActionBase actionBase)
                {
                    actionBase.Description = description;
                }

                // Mark that changes have been made
                _hasUnsavedChanges = true;
            }, _logger, "changing description", this);
        }

        /// <summary>
        /// Handles the CheckedChanged event of the EnabledCheckBox
        /// </summary>
        protected virtual void EnabledCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _mapping.IsEnabled = enabledCheckBox.Checked;
                _hasUnsavedChanges = true;
            }, _logger, "changing enabled state", this);
        }

        /// <summary>
        /// Handles the Click event of the ListenButton
        /// </summary>
        protected virtual void ListenButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (_isListening)
                {
                    StopMidiListening();
                }
                else
                {
                    StartMidiListening();
                }
            }, _logger, "toggling MIDI listening", this);
        }

        /// <summary>
        /// Handles the Click event of the TestButton
        /// </summary>
        protected virtual void TestButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save current data first
                if (!SaveMappingData())
                    return;

                // Test the action
                TestAction();
            }, _logger, "testing action", this);
        }

        /// <summary>
        /// Handles the Click event of the OkButton
        /// </summary>
        protected virtual void OkButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Save the mapping data
                if (!SaveMappingData())
                    return;

                // Validate the mapping data
                if (!ValidateMapping())
                    return;

                // Clear the unsaved changes flag since we're saving
                _hasUnsavedChanges = false;

                _logger.LogDebug("Mapping saved successfully, closing dialog with OK result");

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving unified mapping", this);
        }

        /// <summary>
        /// Handles the Click event of the CancelButton
        /// </summary>
        protected virtual void CancelButton_Click(object? sender, EventArgs e)
        {
            _logger.LogDebug("Cancel button clicked, closing dialog with Cancel result");
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Handles the FormClosing event to warn about unsaved changes
        /// </summary>
        protected virtual void ActionMappingDialog_FormClosing(object? sender, FormClosingEventArgs e)
        {
            // Only show warning if closing without OK and there are unsaved changes
            if (DialogResult != DialogResult.OK && _hasUnsavedChanges)
            {
                _logger.LogDebug("Dialog closing with unsaved changes, showing confirmation");

                var result = MessageBox.Show(
                    "You have unsaved changes. Are you sure you want to close without saving?",
                    "Unsaved Changes",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.No)
                {
                    _logger.LogDebug("User chose to continue editing");
                    e.Cancel = true; // Cancel the close operation
                }
                else
                {
                    _logger.LogDebug("User chose to discard changes and close");
                }
            }
            else
            {
                _logger.LogDebug("Dialog closing with DialogResult={DialogResult}, HasUnsavedChanges={HasUnsavedChanges}",
                    DialogResult, _hasUnsavedChanges);
            }
        }

        #endregion

        #region MIDI Listening

        /// <summary>
        /// Starts listening for MIDI input
        /// </summary>
        protected virtual void StartMidiListening()
        {
            if (_MidiDeviceManager == null)
            {
                _logger.LogError("Cannot start MIDI listening: MidiDeviceManager is null");
                _logger.LogError("This usually means the Configuration GUI is not connected to the main MIDIFlux application");
                _logger.LogError("Please ensure MIDIFlux.exe is running and open this dialog from the system tray menu");
                ApplicationErrorHandler.ShowError(
                    "MIDI Manager is not available.\n\n" +
                    "This usually means the Configuration GUI is not connected to the main MIDIFlux application.\n" +
                    "Please ensure MIDIFlux.exe is running and open this dialog from the system tray menu.",
                    "MIDI Manager Not Available", _logger, null, this);
                return;
            }

            if (_isListening)
            {
                _logger.LogWarning("MIDI listening is already active");
                return;
            }

            try
            {
                _isListening = true;
                listenButton.Text = "Stop Listening";
                listenButton.BackColor = System.Drawing.Color.LightCoral;

                // Subscribe to MIDI events
                _MidiDeviceManager.MidiEventReceived += OnMidiMessageReceived;

                // Log available devices for debugging
                var devices = _MidiDeviceManager.GetAvailableDevices();
                _logger.LogInformation("Starting MIDI listening. Available devices: {DeviceCount}", devices.Count);
                foreach (var device in devices)
                {
                    _logger.LogDebug("  - Device: {DeviceName} (ID: {DeviceId}, Connected: {IsConnected})",
                        device.Name, device.DeviceId, device.IsConnected);
                }

                // Check if any devices are actively listening
                var activeDevices = _MidiDeviceManager.ActiveDeviceIds;
                _logger.LogInformation("Active MIDI devices: [{ActiveDevices}]", string.Join(", ", activeDevices));

                _logger.LogInformation("Started MIDI listening for dialog input detection");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting MIDI listening");
                ApplicationErrorHandler.ShowError("Failed to start MIDI listening.", "Error", _logger, ex, this);
                _isListening = false;
                listenButton.Text = "Listen";
                listenButton.BackColor = System.Drawing.SystemColors.Control;
            }
        }

        /// <summary>
        /// Stops listening for MIDI input
        /// </summary>
        protected virtual void StopMidiListening()
        {
            if (_MidiDeviceManager == null || !_isListening)
                return;

            try
            {
                _isListening = false;
                listenButton.Text = "Listen";
                listenButton.BackColor = System.Drawing.SystemColors.Control;

                // Unsubscribe from MIDI events
                _MidiDeviceManager.MidiEventReceived -= OnMidiMessageReceived;

                _logger.LogDebug("Stopped MIDI listening");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping MIDI listening");
                ApplicationErrorHandler.ShowError("Failed to stop MIDI listening.", "Error", _logger, ex, this);
            }
        }

        /// <summary>
        /// Handles received MIDI messages during listening
        /// </summary>
        protected virtual void OnMidiMessageReceived(object? sender, MidiEventArgs e)
        {
            if (!_isListening)
            {
                _logger.LogTrace("Received MIDI message but not listening, ignoring");
                return;
            }

            try
            {
                _logger.LogDebug("Dialog received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
                    e.DeviceId, e.Event.EventType, e.Event.Channel, e.Event.Note, e.Event.Velocity);

                // Update UI on the main thread
                if (InvokeRequired)
                {
                    Invoke(new Action(() => OnMidiMessageReceived(sender, e)));
                    return;
                }

                // Auto-fill MIDI input based on received message
                AutoFillMidiInput(e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling MIDI message during listening");
            }
        }

        /// <summary>
        /// Auto-fills MIDI input fields based on received MIDI message
        /// </summary>
        protected virtual void AutoFillMidiInput(MidiEventArgs e)
        {
            try
            {
                _updatingUI = true;

                // Set input type and number based on message type
                switch (e.Event.EventType)
                {
                    case MidiEventType.NoteOff:
                        // Skip NoteOff during Listen mode - users almost always want NoteOn
                        _logger.LogDebug("Ignoring NoteOff event during Listen mode (Note={Note}, Channel={Channel}) - waiting for NoteOn or other event type",
                            e.Event.Note, e.Event.Channel);
                        return; // Don't stop listening, wait for next event

                    case MidiEventType.PitchBend:
                        // Skip PitchBend at center position (8192) - many MIDI keyboards send this
                        // as a "reset" message before every NoteOn, making it noise in Listen mode
                        if (e.Event.PitchBendValue == 8192)
                        {
                            _logger.LogDebug("Ignoring PitchBend center (8192) during Listen mode (Channel={Channel}) - likely noise from keyboard",
                                e.Event.Channel);
                            return; // Don't stop listening, wait for next event
                        }
                        SelectInputTypeInComboBox(MidiInputType.PitchBend);
                        midiInputNumberNumericUpDown.Value = 0; // PitchBend has no input number
                        _logger.LogInformation("Captured PitchBend event during Listen: Value={Value}, Channel={Channel}",
                            e.Event.PitchBendValue, e.Event.Channel);
                        break;

                    case MidiEventType.NoteOn:
                        SelectInputTypeInComboBox(MidiInputType.NoteOn);
                        if (e.Event.Note.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Note.Value;
                        }
                        _logger.LogInformation("Captured NoteOn event during Listen: Note={Note}, Channel={Channel}",
                            e.Event.Note, e.Event.Channel);
                        break;

                    case MidiEventType.ControlChange:
                        // Default to absolute for now - user can change if needed
                        SelectInputTypeInComboBox(MidiInputType.ControlChangeAbsolute);
                        if (e.Event.Controller.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Controller.Value;
                        }
                        _logger.LogInformation("Captured ControlChange event during Listen: Controller={Controller}, Channel={Channel}",
                            e.Event.Controller, e.Event.Channel);
                        break;

                    case MidiEventType.ProgramChange:
                        SelectInputTypeInComboBox(MidiInputType.ProgramChange);
                        if (e.Event.ProgramNumber.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.ProgramNumber.Value;
                        }
                        _logger.LogInformation("Captured ProgramChange event during Listen: Program={Program}, Channel={Channel}",
                            e.Event.ProgramNumber, e.Event.Channel);
                        break;

                    case MidiEventType.ChannelPressure:
                        SelectInputTypeInComboBox(MidiInputType.ChannelPressure);
                        midiInputNumberNumericUpDown.Value = 0; // ChannelPressure has no input number
                        _logger.LogInformation("Captured ChannelPressure event during Listen: Pressure={Pressure}, Channel={Channel}",
                            e.Event.Pressure, e.Event.Channel);
                        break;

                    case MidiEventType.PolyphonicKeyPressure:
                        SelectInputTypeInComboBox(MidiInputType.PolyphonicKeyPressure);
                        if (e.Event.Note.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Note.Value;
                        }
                        _logger.LogInformation("Captured PolyphonicKeyPressure event during Listen: Note={Note}, Channel={Channel}",
                            e.Event.Note, e.Event.Channel);
                        break;

                    default:
                        // For unhandled event types (SysEx, Error, Other), skip them
                        // SysEx requires manual pattern configuration and can't be auto-filled
                        _logger.LogDebug("Ignoring {EventType} event during Listen mode - not auto-fillable",
                            e.Event.EventType);
                        return; // Don't stop listening, wait for next event
                }

                // Set channel (Event.Channel is already 1-based from MidiEventConverter)
                midiChannelComboBox.SelectedIndex = e.Event.Channel; // Channel is already 1-based, UI expects 1-based

                // Set device name based on device ID
                SetDeviceNameFromDeviceId(e.DeviceId);

                // Stop listening after capturing a valid event
                StopMidiListening();
            }
            finally
            {
                _updatingUI = false;
            }
        }

        /// <summary>
        /// Sets the device name in the combo box based on the device ID
        /// </summary>
        /// <param name="deviceId">The MIDI device ID (format depends on adapter implementation)</param>
        protected virtual void SetDeviceNameFromDeviceId(string deviceId)
        {
            try
            {
                if (_MidiDeviceManager == null)
                {
                    _logger.LogWarning("MidiDeviceManager is null, cannot resolve device name for device ID {DeviceId}", deviceId);
                    return;
                }

                // Get device info from the MIDI manager
                var deviceInfo = _MidiDeviceManager.GetDeviceInfo(deviceId);
                if (deviceInfo != null)
                {
                    var deviceName = deviceInfo.Name;
                    _logger.LogDebug("Resolved device ID {DeviceId} to device name '{DeviceName}'", deviceId, deviceName);

                    // Check if the device name is already in the combo box
                    bool deviceFound = false;
                    for (int i = 0; i < deviceNameComboBox.Items.Count; i++)
                    {
                        var item = deviceNameComboBox.Items[i]?.ToString();
                        if (item == deviceName)
                        {
                            deviceNameComboBox.SelectedIndex = i;
                            deviceFound = true;
                            break;
                        }
                    }

                    // If device not found in combo box, add it and select it
                    if (!deviceFound)
                    {
                        deviceNameComboBox.Items.Add(deviceName);
                        deviceNameComboBox.SelectedItem = deviceName;
                        _logger.LogDebug("Added device '{DeviceName}' to combo box and selected it", deviceName);
                    }

                    // Update the mapping's device name
                    _mapping.Input.DeviceName = deviceName;
                }
                else
                {
                    _logger.LogWarning("Could not get device info for device ID {DeviceId}", deviceId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error setting device name from device ID {DeviceId}", deviceId);
            }
        }

        #endregion

        #region MIDI Output Helpers

        /// <summary>
        /// Populates the MIDI output device combo box
        /// </summary>
        protected virtual void PopulateMidiOutputDeviceComboBox(ComboBox comboBox, string? selectedDeviceName = null)
        {
            try
            {
                comboBox.Items.Clear();

                if (_MidiDeviceManager != null)
                {
                    var outputDevices = _MidiDeviceManager.GetAvailableOutputDevices();
                    foreach (var device in outputDevices)
                    {
                        comboBox.Items.Add(device.Name);
                    }

                    _logger.LogDebug("Populated output device combo box with {DeviceCount} devices", outputDevices.Count);

                    // Select the specified device if provided
                    if (!string.IsNullOrEmpty(selectedDeviceName) && comboBox.Items.Contains(selectedDeviceName))
                    {
                        comboBox.SelectedItem = selectedDeviceName;
                    }
                    else if (comboBox.Items.Count > 0)
                    {
                        comboBox.SelectedIndex = 0; // Select first device as default
                    }
                }
                else
                {
                    _logger.LogWarning("MidiDeviceManager is null, cannot populate output device combo box");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error populating MIDI output device combo box");
            }
        }

        /// <summary>
        /// Adds a new MIDI command to the list
        /// </summary>
        protected virtual void AddMidiCommand(ListBox commandsListBox)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                var command = new MidiOutputCommand
                {
                    MessageType = MidiMessageType.NoteOn,
                    Channel = 1,
                    Data1 = 60, // Middle C
                    Data2 = 127 // Full velocity
                };

                if (EditMidiCommandDialog(command))
                {
                    commandsListBox.Items.Add(command.ToString());
                    commandsListBox.SelectedIndex = commandsListBox.Items.Count - 1;
                }
            }, _logger, "adding MIDI command", this);
        }

        /// <summary>
        /// Edits the selected MIDI command in the list
        /// </summary>
        protected virtual void EditMidiCommand(ListBox commandsListBox)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (commandsListBox.SelectedIndex < 0)
                {
                    ApplicationErrorHandler.ShowError("Please select a command to edit.", "No Selection", _logger, null, this);
                    return;
                }

                // For now, create a new command with default values
                // In a real implementation, you'd extract the existing command data
                var command = new MidiOutputCommand
                {
                    MessageType = MidiMessageType.NoteOn,
                    Channel = 1,
                    Data1 = 60,
                    Data2 = 127
                };

                if (EditMidiCommandDialog(command))
                {
                    commandsListBox.Items[commandsListBox.SelectedIndex] = command.ToString();
                }
            }, _logger, "editing MIDI command", this);
        }

        /// <summary>
        /// Removes the selected MIDI command from the list
        /// </summary>
        protected virtual void RemoveMidiCommand(ListBox commandsListBox)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (commandsListBox.SelectedIndex < 0)
                {
                    ApplicationErrorHandler.ShowError("Please select a command to remove.", "No Selection", _logger, null, this);
                    return;
                }

                commandsListBox.Items.RemoveAt(commandsListBox.SelectedIndex);
            }, _logger, "removing MIDI command", this);
        }

        /// <summary>
        /// Shows a dialog to edit a MIDI command
        /// </summary>
        protected virtual bool EditMidiCommandDialog(MidiOutputCommand command)
        {
            // For now, show a simple input dialog
            // In a real implementation, you'd create a proper MIDI command editing dialog
            var result = MessageBox.Show(
                $"Current command: {command}\n\nThis is a placeholder for MIDI command editing.\nClick OK to keep the default command.",
                "Edit MIDI Command",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Information);

            return result == DialogResult.OK;
        }

        #endregion

        #region Mouse Action Parameter Controls



        #endregion

        #region Action Testing

        /// <summary>
        /// Tests the current action
        /// </summary>
        protected virtual void TestAction()
        {
            try
            {
                if (_mapping.Action == null)
                {
                    ApplicationErrorHandler.ShowError("No action to test.", "Test Error", _logger, null, this);
                    return;
                }

                _logger.LogDebug("Testing action: {ActionType}", _mapping.Action.GetType().Name);

                // Execute the action with a test MIDI value
                var testValue = 127; // Full velocity/value
                var task = _mapping.Action.ExecuteAsync(testValue);
                if (!task.IsCompleted)
                {
                    task.AsTask().Wait(TimeSpan.FromSeconds(5));
                }

                _logger.LogDebug("Action test completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing action");
                ApplicationErrorHandler.ShowError("Failed to test the action.", "Test Error", _logger, ex, this);
            }
        }

        #endregion

        #region Action Only Mode Support

        /// <summary>
        /// Hides MIDI input controls when in action-only mode
        /// </summary>
        private void HideMidiInputControls()
        {
            _logger.LogDebug("HideMidiInputControls: actionParametersPanel visible before: {Visible}", actionParametersPanel?.Visible);

            // Hide the entire MIDI Input group box if it exists
            var midiInputGroupBox = this.Controls.Find("midiInputGroupBox", true).FirstOrDefault();
            if (midiInputGroupBox != null)
            {
                _logger.LogDebug("Hiding midiInputGroupBox");
                midiInputGroupBox.Visible = false;
            }

            // Hide individual MIDI input controls if they exists
            // Note: We keep descriptionTextBox visible for sub-actions, but hide enabledCheckBox
            var controlsToHide = new[]
            {
                "midiInputTypeComboBox", "midiInputNumberNumericUpDown", "midiChannelComboBox",
                "deviceNameComboBox", "listenButton", "enabledCheckBox"
            };

            foreach (var controlName in controlsToHide)
            {
                var control = this.Controls.Find(controlName, true).FirstOrDefault();
                if (control != null)
                {
                    _logger.LogDebug("Hiding control: {ControlName}", controlName);
                    control.Visible = false;
                    // Also hide associated labels
                    var label = this.Controls.Find(controlName.Replace("ComboBox", "Label").Replace("NumericUpDown", "Label").Replace("TextBox", "Label").Replace("CheckBox", "Label").Replace("Button", "Label"), true).FirstOrDefault();
                    if (label != null)
                    {
                        label.Visible = false;
                    }
                }
            }

            // Ensure actionParametersPanel stays visible and has adequate height
            if (actionParametersPanel != null)
            {
                _logger.LogDebug("Ensuring actionParametersPanel stays visible");
                actionParametersPanel.Visible = true;

                // In action-only mode, ensure the parameters panel has a minimum height
                if (actionParametersPanel.Height < 150)
                {
                    _logger.LogDebug("Setting minimum height for actionParametersPanel from {OldHeight} to 150", actionParametersPanel.Height);
                    actionParametersPanel.Height = 150;
                }
            }

            // Adjust dialog size for action-only mode, but ensure it's large enough for parameters
            var minHeight = 350; // Minimum height to show parameters properly
            this.Height = Math.Max(minHeight, this.Height - 100); // Reduce less aggressively

            _logger.LogDebug("HideMidiInputControls: actionParametersPanel visible after: {Visible}, height: {Height}",
                actionParametersPanel?.Visible, actionParametersPanel?.Height);
        }

        #endregion

        #region Keyboard Listening

        /// <summary>
        /// Starts listening for keyboard input for a specific parameter
        /// </summary>
        /// <param name="parameterName">The name of the parameter to update</param>
        /// <param name="comboBox">The ComboBox to update when a key is detected</param>
        public void StartKeyListening(string parameterName, ComboBox comboBox)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                if (_isKeyListening)
                {
                    StopKeyListening();
                }

                try
                {
                    _isKeyListening = true;
                    _keyListeningParameterName = parameterName;
                    _keyListeningComboBox = comboBox;

                    // Initialize keyboard listener if needed
                    if (_keyboardListener == null)
                    {
                        _keyboardListener = new KeyboardListener(_logger);
                        _keyboardListener.KeyboardEvent += OnKeyboardEvent;
                    }

                    // Start listening
                    if (_keyboardListener.StartListening())
                    {
                        // Update the Listen button appearance
                        var listenButton = FindListenButtonForComboBox(comboBox);
                        if (listenButton != null)
                        {
                            listenButton.Text = "Stop";
                            listenButton.BackColor = System.Drawing.Color.LightCoral;
                        }

                        _logger.LogInformation("Started keyboard listening for parameter {ParameterName}", parameterName);
                    }
                    else
                    {
                        _logger.LogError("Failed to start keyboard listening");
                        ApplicationErrorHandler.ShowError("Failed to start keyboard listening.", "Error", _logger, null, this);
                        _isKeyListening = false;
                        _keyListeningParameterName = null;
                        _keyListeningComboBox = null;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error starting keyboard listening");
                    ApplicationErrorHandler.ShowError("Failed to start keyboard listening.", "Error", _logger, ex, this);
                    _isKeyListening = false;
                    _keyListeningParameterName = null;
                    _keyListeningComboBox = null;
                }
            }, _logger, "starting keyboard listening", this);
        }

        /// <summary>
        /// Stops listening for keyboard input
        /// </summary>
        private void StopKeyListening()
        {
            if (!_isKeyListening || _keyboardListener == null)
                return;

            try
            {
                _keyboardListener.StopListening();
                _isKeyListening = false;

                // Update the Listen button appearance
                if (_keyListeningComboBox != null)
                {
                    var listenButton = FindListenButtonForComboBox(_keyListeningComboBox);
                    if (listenButton != null)
                    {
                        listenButton.Text = "Listen";
                        listenButton.BackColor = System.Drawing.SystemColors.Control;
                    }
                }

                _keyListeningParameterName = null;
                _keyListeningComboBox = null;

                _logger.LogDebug("Stopped keyboard listening");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping keyboard listening");
            }
        }

        /// <summary>
        /// Handles keyboard events during listening
        /// </summary>
        private void OnKeyboardEvent(object? sender, KeyboardEventArgs e)
        {
            if (!_isKeyListening || !e.IsKeyDown) // Only respond to key down events
                return;

            try
            {
                _logger.LogDebug("Keyboard event received: Key={Key}", e.Key);

                // Update UI on the main thread
                if (InvokeRequired)
                {
                    Invoke(new Action(() => OnKeyboardEvent(sender, e)));
                    return;
                }

                // Update the ComboBox with the detected key
                if (_keyListeningComboBox != null && _keyListeningParameterName != null && _mapping.Action != null)
                {
                    // Find the matching enum value in the ComboBox
                    var actionBase = (ActionBase)_mapping.Action;
                    var parameterInfo = actionBase.GetParameterList().FirstOrDefault(p => p.Name == _keyListeningParameterName);

                    if (parameterInfo?.EnumDefinition != null)
                    {
                        var index = Array.IndexOf(parameterInfo.EnumDefinition.Values, e.Key);
                        if (index >= 0 && index < parameterInfo.EnumDefinition.Options.Length)
                        {
                            _keyListeningComboBox.SelectedIndex = index;
                            _logger.LogInformation("Set key parameter {ParameterName} to {Key}", _keyListeningParameterName, e.Key);

                            // Provide visual feedback that the key was captured
                            FlashComboBoxSuccess(_keyListeningComboBox);

                            // Mark that changes have been made
                            _hasUnsavedChanges = true;
                        }
                        else
                        {
                            _logger.LogWarning("Key {Key} not found in enum definition for parameter {ParameterName}", e.Key, _keyListeningParameterName);
                        }
                    }
                }

                // Stop listening after first key
                StopKeyListening();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error handling keyboard event");
            }
        }

        /// <summary>
        /// Finds the Listen button associated with a ComboBox
        /// </summary>
        private Button? FindListenButtonForComboBox(ComboBox comboBox)
        {
            var parent = comboBox.Parent;
            if (parent != null)
            {
                var listenButtonName = comboBox.Name + "_listen";
                return parent.Controls.Find(listenButtonName, false).FirstOrDefault() as Button;
            }
            return null;
        }

        /// <summary>
        /// Provides visual feedback when a key is captured by briefly flashing the ComboBox background
        /// </summary>
        private async void FlashComboBoxSuccess(ComboBox comboBox)
        {
            if (comboBox == null || comboBox.IsDisposed)
                return;

            try
            {
                var originalBackColor = comboBox.BackColor;
                var successColor = Color.LightGreen;

                // Flash to success color
                comboBox.BackColor = successColor;
                await Task.Delay(300);

                // Fade back to original color
                if (!comboBox.IsDisposed)
                {
                    comboBox.BackColor = originalBackColor;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error flashing ComboBox success feedback");
            }
        }

        #endregion

        #region Cleanup

        /// <summary>
        /// Clean up any resources being used
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop MIDI listening if active
                if (_isListening)
                {
                    StopMidiListening();
                }

                // Stop keyboard listening if active
                if (_isKeyListening)
                {
                    StopKeyListening();
                }

                // Dispose keyboard listener
                if (_keyboardListener != null)
                {
                    _keyboardListener.Dispose();
                    _keyboardListener = null;
                }

                // Dispose components if they exist
                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion
    }
}
