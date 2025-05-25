using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Configuration;
using MIDIFlux.Core.Helpers;
using MIDIFlux.Core.Midi;
using MIDIFlux.Core.Models;
using MIDIFlux.GUI.Helpers;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Base dialog class for creating and editing action mappings.
    /// Provides common functionality for MIDI input configuration and action selection.
    /// </summary>
    public partial class ActionMappingDialog : BaseDialog
    {
        protected readonly ILogger _logger;
        protected readonly ActionMapping _mapping;
        protected readonly MidiManager? _midiManager;
        protected bool _isNewMapping;
        protected bool _updatingUI = false;
        protected bool _isListening = false;

        /// <summary>
        /// Gets the edited action mapping
        /// </summary>
        public ActionMapping Mapping => _mapping;

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for creating a new mapping
        /// </summary>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        protected ActionMappingDialog(MidiManager? midiManager = null)
            : this(CreateDefaultMapping(), midiManager)
        {
            _isNewMapping = true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        public ActionMappingDialog(ActionMapping mapping, MidiManager? midiManager = null)
            : this(mapping, midiManager, false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ActionMappingDialog"/> class for editing an existing mapping
        /// </summary>
        /// <param name="mapping">The action mapping to edit</param>
        /// <param name="midiManager">Optional MidiManager for MIDI listening functionality</param>
        /// <param name="actionOnly">If true, only show action configuration (hide MIDI input configuration)</param>
        public ActionMappingDialog(ActionMapping mapping, MidiManager? midiManager, bool actionOnly)
        {
            // Create logger
            _logger = LoggingHelper.CreateLogger<ActionMappingDialog>();
            _logger.LogDebug("Initializing ActionMappingDialog");

            // Store the mapping and MIDI manager
            _mapping = mapping ?? throw new ArgumentNullException(nameof(mapping));
            _midiManager = midiManager;
            _isNewMapping = false;

            // Initialize components
            InitializeComponent();

            // Set the dialog title
            Text = _isNewMapping ? "Add Mapping" : "Edit Mapping";

            // Hide MIDI input configuration if actionOnly is true
            if (actionOnly)
            {
                // Hide MIDI input controls (this would need to be implemented based on the actual form layout)
                Text = "Configure Action";
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
            var config = new KeyPressReleaseConfig { VirtualKeyCode = 65 }; // 'A' key
            var logger = LoggingHelper.CreateLogger<ActionFactory>();
            var factory = new ActionFactory(logger);
            var action = factory.CreateAction(config);

            return new ActionMapping
            {
                Input = new ActionMidiInput
                {
                    InputType = ActionMidiInputType.NoteOn,
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
            actionTypeComboBox.SelectedIndexChanged += ActionTypeComboBox_SelectedIndexChanged;

            // Common button event handlers
            listenButton.Click += ListenButton_Click;
            testButton.Click += TestButton_Click;
            okButton.Click += OkButton_Click;
            cancelButton.Click += CancelButton_Click;

            // Description event handler
            descriptionTextBox.TextChanged += DescriptionTextBox_TextChanged;
            enabledCheckBox.CheckedChanged += EnabledCheckBox_CheckedChanged;
        }

        /// <summary>
        /// Loads the mapping data into the UI controls
        /// </summary>
        protected virtual void LoadMappingData()
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                try
                {
                    _updatingUI = true;

                    // Load MIDI input data
                    LoadMidiInputData();

                    // Load action data
                    LoadActionData();

                    // Load common properties
                    descriptionTextBox.Text = _mapping.Description ?? string.Empty;
                    enabledCheckBox.Checked = _mapping.IsEnabled;
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
            // Populate MIDI input type combo box
            midiInputTypeComboBox.Items.Clear();
            foreach (ActionMidiInputType inputType in Enum.GetValues<ActionMidiInputType>())
            {
                midiInputTypeComboBox.Items.Add(inputType);
            }
            midiInputTypeComboBox.SelectedItem = _mapping.Input.InputType;

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
                _midiManager,
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
            // Populate action type combo box
            PopulateActionTypeComboBox();

            // Select the current action type
            var actionTypeName = GetActionTypeName(_mapping.Action);
            if (actionTypeComboBox.Items.Contains(actionTypeName))
            {
                actionTypeComboBox.SelectedItem = actionTypeName;
            }
            else
            {
                actionTypeComboBox.SelectedIndex = 0;
            }

            // Load action-specific parameters
            LoadActionParameters();
        }

        /// <summary>
        /// Populates the action type combo box with available action types
        /// </summary>
        protected virtual void PopulateActionTypeComboBox()
        {
            actionTypeComboBox.Items.Clear();
            actionTypeComboBox.Items.AddRange(new object[]
            {
                "Key Press/Release",
                "Key Down",
                "Key Up",
                "Key Toggle",
                "Mouse Click",
                "Mouse Scroll",
                "Command Execution",
                "Delay",
                "Game Controller Button",
                "Game Controller Axis",
                "MIDI Output",
                "Sequence (Macro)",
                "Conditional (CC Range)",
                "Alternating (Toggle)"
            });
        }

        /// <summary>
        /// Gets the display name for an action type
        /// </summary>
        protected virtual string GetActionTypeName(IAction action)
        {
            return action.GetType().Name switch
            {
                "KeyPressReleaseAction" => "Key Press/Release",
                "KeyDownAction" => "Key Down",
                "KeyUpAction" => "Key Up",
                "KeyToggleAction" => "Key Toggle",
                "MouseClickAction" => "Mouse Click",
                "MouseScrollAction" => "Mouse Scroll",
                "CommandExecutionAction" => "Command Execution",
                "DelayAction" => "Delay",
                "GameControllerButtonAction" => "Game Controller Button",
                "GameControllerAxisAction" => "Game Controller Axis",
                "MidiOutputAction" => "MIDI Output",
                "SequenceAction" => "Sequence (Macro)",
                "ConditionalAction" => "Conditional (CC Range)",
                "AlternatingAction" => "Alternating (Toggle)",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// Loads action-specific parameters into the UI
        /// </summary>
        protected virtual void LoadActionParameters()
        {
            if (actionParametersPanel == null)
                return;

            // Clear existing controls
            actionParametersPanel.Controls.Clear();

            if (_mapping.Action == null)
                return;

            // Check if this is a complex action that needs a special dialog
            if (IsComplexAction(_mapping.Action))
            {
                CreateComplexActionControls();
            }
            else if (_mapping.Action is Core.Actions.Simple.MidiOutputAction)
            {
                CreateMidiOutputParameterControls();
            }
            else
            {
                // This will be implemented by derived classes for simple actions
                // For now, just show a placeholder
                var label = new Label
                {
                    Text = "Action parameters will be configured here.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10)
                };
                actionParametersPanel.Controls.Add(label);
            }
        }

        /// <summary>
        /// Checks if an action is a complex action that requires a special dialog
        /// </summary>
        protected virtual bool IsComplexAction(IAction action)
        {
            return action is Core.Actions.Complex.SequenceAction or Core.Actions.Complex.ConditionalAction or Core.Actions.Complex.AlternatingAction;
        }

        /// <summary>
        /// Creates controls for complex actions (sequence/conditional)
        /// </summary>
        protected virtual void CreateComplexActionControls()
        {
            var infoLabel = new Label
            {
                Text = GetComplexActionInfo(),
                AutoSize = false,
                Size = new System.Drawing.Size(400, 40),
                Location = new System.Drawing.Point(10, 10),
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };

            var configureButton = new Button
            {
                Text = "Configure...",
                Size = new System.Drawing.Size(100, 30),
                Location = new System.Drawing.Point(10, 60),
                UseVisualStyleBackColor = true
            };

            configureButton.Click += ConfigureComplexActionButton_Click;

            actionParametersPanel.Controls.Add(infoLabel);
            actionParametersPanel.Controls.Add(configureButton);
        }

        /// <summary>
        /// Gets information text for complex actions
        /// </summary>
        protected virtual string GetComplexActionInfo()
        {
            return _mapping.Action switch
            {
                Core.Actions.Complex.SequenceAction sequence =>
                    $"Sequence Action with {sequence.GetChildActions().Count} sub-actions.\nClick Configure to edit the sequence.",
                Core.Actions.Complex.ConditionalAction conditional =>
                    $"Conditional Action with {conditional.GetChildActions().Count} conditions.\nClick Configure to edit the conditions.",
                _ => "Complex action. Click Configure to edit."
            };
        }

        /// <summary>
        /// Creates controls for MIDI Output action parameters
        /// </summary>
        protected virtual void CreateMidiOutputParameterControls()
        {
            var config = ExtractMidiOutputConfig((Core.Actions.Simple.MidiOutputAction)_mapping.Action);

            // Output Device Label and ComboBox
            var deviceLabel = new Label
            {
                Text = "Output Device:",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };

            var deviceComboBox = new ComboBox
            {
                Name = "midiOutputDeviceComboBox",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Size = new System.Drawing.Size(300, 21),
                Location = new System.Drawing.Point(120, 8)
            };

            // Populate output device combo box
            PopulateMidiOutputDeviceComboBox(deviceComboBox, config.OutputDeviceName);

            // Commands Label and ListBox
            var commandsLabel = new Label
            {
                Text = "MIDI Commands:",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 40)
            };

            var commandsListBox = new ListBox
            {
                Name = "midiCommandsListBox",
                Size = new System.Drawing.Size(300, 100),
                Location = new System.Drawing.Point(120, 38)
            };

            // Populate commands list
            foreach (var command in config.Commands)
            {
                commandsListBox.Items.Add(command.ToString());
            }

            // Command management buttons
            var addCommandButton = new Button
            {
                Text = "Add",
                Size = new System.Drawing.Size(60, 23),
                Location = new System.Drawing.Point(430, 38)
            };

            var editCommandButton = new Button
            {
                Text = "Edit",
                Size = new System.Drawing.Size(60, 23),
                Location = new System.Drawing.Point(430, 65)
            };

            var removeCommandButton = new Button
            {
                Text = "Remove",
                Size = new System.Drawing.Size(60, 23),
                Location = new System.Drawing.Point(430, 92)
            };

            // Add event handlers
            addCommandButton.Click += (s, e) => AddMidiCommand(commandsListBox);
            editCommandButton.Click += (s, e) => EditMidiCommand(commandsListBox);
            removeCommandButton.Click += (s, e) => RemoveMidiCommand(commandsListBox);

            // Add controls to panel
            actionParametersPanel.Controls.AddRange(new Control[]
            {
                deviceLabel, deviceComboBox,
                commandsLabel, commandsListBox,
                addCommandButton, editCommandButton, removeCommandButton
            });
        }

        /// <summary>
        /// Handles the click event for the configure complex action button
        /// </summary>
        protected virtual void ConfigureComplexActionButton_Click(object? sender, EventArgs e)
        {
            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                switch (_mapping.Action)
                {
                    case Core.Actions.Complex.SequenceAction sequenceAction:
                        EditSequenceAction(sequenceAction);
                        break;
                    case Core.Actions.Complex.ConditionalAction conditionalAction:
                        EditConditionalAction(conditionalAction);
                        break;
                    case Core.Actions.Complex.AlternatingAction alternatingAction:
                        EditAlternatingAction(alternatingAction);
                        break;
                }
            }, _logger, "configuring complex action", this);
        }

        /// <summary>
        /// Saves the mapping data from the UI controls
        /// </summary>
        protected virtual bool SaveMappingData()
        {
            try
            {
                // Save MIDI input data
                if (!SaveMidiInputData())
                    return false;

                // Save action data
                if (!SaveActionData())
                    return false;

                // Save common properties
                _mapping.Description = descriptionTextBox.Text.Trim();
                _mapping.IsEnabled = enabledCheckBox.Checked;

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
                if (midiInputTypeComboBox.SelectedItem is ActionMidiInputType inputType)
                {
                    _mapping.Input.InputType = inputType;
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
            // This will be implemented by derived classes or through dynamic parameter panels
            // For now, just return true
            return true;
        }

        /// <summary>
        /// Validates the mapping data
        /// </summary>
        protected virtual bool ValidateMapping()
        {
            // Validate MIDI input using centralized helper
            if (!Helpers.MidiValidationHelper.ValidateMidiInput(
                _mapping.Input.InputNumber,
                _mapping.Input.Channel,
                _mapping.Input.InputType.ToString(),
                _logger,
                this))
            {
                return false;
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
                if (midiInputTypeComboBox.SelectedItem is ActionMidiInputType inputType)
                {
                    _mapping.Input.InputType = inputType;
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
        /// Handles the SelectedIndexChanged event of the ActionTypeComboBox
        /// </summary>
        protected virtual void ActionTypeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                // Create a new action based on the selected type
                var selectedType = actionTypeComboBox.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(selectedType))
                {
                    CreateActionFromType(selectedType);
                    LoadActionParameters();
                }
            }, _logger, "changing action type", this);
        }

        /// <summary>
        /// Creates a new action instance based on the selected type
        /// </summary>
        protected virtual void CreateActionFromType(string actionTypeName)
        {
            ActionConfig? config = actionTypeName switch
            {
                "Key Press/Release" => new KeyPressReleaseConfig { VirtualKeyCode = 65 }, // 'A' key
                "Key Down" => new KeyDownConfig { VirtualKeyCode = 65 },
                "Key Up" => new KeyUpConfig { VirtualKeyCode = 65 },
                "Key Toggle" => new KeyToggleConfig { VirtualKeyCode = 65 },
                "Mouse Click" => new MouseClickConfig { Button = MouseButton.Left },
                "Mouse Scroll" => new MouseScrollConfig { Direction = ScrollDirection.Up, Amount = 1 },
                "Command Execution" => new CommandExecutionConfig { Command = "echo test", ShellType = CommandShellType.PowerShell },
                "Delay" => new DelayConfig { Milliseconds = 100 },
                "Game Controller Button" => new GameControllerButtonConfig { Button = "A", ControllerIndex = 0 },
                "Game Controller Axis" => new GameControllerAxisConfig { AxisName = "LeftStickX", ControllerIndex = 0, AxisValue = 0.5f },
                "MIDI Output" => CreateMidiOutputAction(),
                "Sequence (Macro)" => CreateSequenceAction(),
                "Conditional (CC Range)" => CreateConditionalAction(),
                "Alternating (Toggle)" => CreateAlternatingAction(),
                _ => new KeyPressReleaseConfig { VirtualKeyCode = 65 }
            };

            // Create the action using the factory (if config was created)
            if (config != null)
            {
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(config);
            }
        }

        /// <summary>
        /// Creates a sequence action by launching the sequence configuration dialog
        /// </summary>
        /// <returns>SequenceConfig if user confirmed, null if cancelled</returns>
        protected virtual SequenceConfig? CreateSequenceAction()
        {
            var config = new SequenceConfig { SubActions = new List<ActionConfig>() };

            using var dialog = new SequenceActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                return dialog.SequenceConfig;
            }

            return null; // User cancelled
        }

        /// <summary>
        /// Creates a conditional action by launching the conditional configuration dialog
        /// </summary>
        /// <returns>ConditionalConfig if user confirmed, null if cancelled</returns>
        protected virtual ConditionalConfig? CreateConditionalAction()
        {
            var config = new ConditionalConfig { Conditions = new List<ValueConditionConfig>() };

            using var dialog = new ConditionalActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                return dialog.ConditionalConfig;
            }

            return null; // User cancelled
        }

        /// <summary>
        /// Creates an alternating action by launching the alternating configuration dialog
        /// </summary>
        /// <returns>AlternatingActionConfig if user confirmed, null if cancelled</returns>
        protected virtual AlternatingActionConfig? CreateAlternatingAction()
        {
            var config = new AlternatingActionConfig
            {
                PrimaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 65 }, // Default to 'A' key
                SecondaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 66 }, // Default to 'B' key
                StartWithPrimary = true,
                StateKey = "" // Auto-generated
            };

            using var dialog = new AlternatingActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                return dialog.AlternatingConfig;
            }

            return null; // User cancelled
        }

        /// <summary>
        /// Creates a MIDI output action with default configuration
        /// </summary>
        /// <returns>MidiOutputConfig with default settings</returns>
        protected virtual MidiOutputConfig CreateMidiOutputAction()
        {
            // Get the first available output device as default
            string defaultDeviceName = "Default Device";
            if (_midiManager != null)
            {
                var outputDevices = _midiManager.GetAvailableOutputDevices();
                if (outputDevices.Count > 0)
                {
                    defaultDeviceName = outputDevices[0].Name;
                }
            }

            return new MidiOutputConfig
            {
                OutputDeviceName = defaultDeviceName,
                Commands = new List<MidiOutputCommand>
                {
                    new MidiOutputCommand
                    {
                        MessageType = MidiMessageType.NoteOn,
                        Channel = 1,
                        Data1 = 60, // Middle C
                        Data2 = 127 // Full velocity
                    }
                }
            };
        }

        /// <summary>
        /// Edits an existing sequence action
        /// </summary>
        protected virtual void EditSequenceAction(Core.Actions.Complex.SequenceAction sequenceAction)
        {
            // Extract the current configuration from the sequence action
            var config = ExtractSequenceConfig(sequenceAction);

            using var dialog = new SequenceActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Create a new action with the updated configuration
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(dialog.SequenceConfig);

                // Refresh the action parameters display
                LoadActionParameters();
            }
        }

        /// <summary>
        /// Edits an existing conditional action
        /// </summary>
        protected virtual void EditConditionalAction(Core.Actions.Complex.ConditionalAction conditionalAction)
        {
            // Extract the current configuration from the conditional action
            var config = ExtractConditionalConfig(conditionalAction);

            using var dialog = new ConditionalActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Create a new action with the updated configuration
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(dialog.ConditionalConfig);

                // Refresh the action parameters display
                LoadActionParameters();
            }
        }

        /// <summary>
        /// Edits an existing alternating action
        /// </summary>
        protected virtual void EditAlternatingAction(Core.Actions.Complex.AlternatingAction alternatingAction)
        {
            // Extract the current configuration from the alternating action
            var config = ExtractAlternatingConfig(alternatingAction);

            using var dialog = new AlternatingActionDialog(config);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                // Create a new action with the updated configuration
                var factoryLogger = LoggingHelper.CreateLogger<ActionFactory>();
                var factory = new ActionFactory(factoryLogger);
                _mapping.Action = factory.CreateAction(dialog.AlternatingConfig);

                // Refresh the action parameters display
                LoadActionParameters();
            }
        }

        /// <summary>
        /// Extracts sequence configuration from a sequence action
        /// </summary>
        protected virtual SequenceConfig ExtractSequenceConfig(Core.Actions.Complex.SequenceAction sequenceAction)
        {
            var config = new SequenceConfig
            {
                Description = sequenceAction.Description,
                ErrorHandling = sequenceAction.ErrorHandling,
                SubActions = new List<ActionConfig>()
            };

            // Extract sub-action configurations
            foreach (var subAction in sequenceAction.GetChildActions())
            {
                var subConfig = ExtractActionConfig(subAction);
                if (subConfig != null)
                {
                    config.SubActions.Add(subConfig);
                }
            }

            return config;
        }

        /// <summary>
        /// Extracts conditional configuration from a conditional action
        /// </summary>
        protected virtual ConditionalConfig ExtractConditionalConfig(Core.Actions.Complex.ConditionalAction conditionalAction)
        {
            var config = new ConditionalConfig
            {
                Description = conditionalAction.Description,
                Conditions = new List<ValueConditionConfig>()
            };

            // Extract condition configurations
            var conditions = conditionalAction.GetConditions();
            var actions = conditionalAction.GetChildActions();

            for (int i = 0; i < conditions.Count && i < actions.Count; i++)
            {
                var actionConfig = ExtractActionConfig(actions[i]);
                if (actionConfig != null)
                {
                    var conditionConfig = new ValueConditionConfig
                    {
                        MinValue = conditions[i].MinValue,
                        MaxValue = conditions[i].MaxValue,
                        Description = conditions[i].Description,
                        Action = actionConfig
                    };
                    config.Conditions.Add(conditionConfig);
                }
            }

            return config;
        }

        /// <summary>
        /// Extracts alternating configuration from an alternating action
        /// </summary>
        protected virtual AlternatingActionConfig ExtractAlternatingConfig(Core.Actions.Complex.AlternatingAction alternatingAction)
        {
            // Note: This is a simplified extraction. In a real implementation, you might need
            // to access private fields through reflection or add public getters to the action class
            return new AlternatingActionConfig
            {
                PrimaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 65 }, // Default to 'A' key
                SecondaryAction = new KeyPressReleaseConfig { VirtualKeyCode = 66 }, // Default to 'B' key
                StartWithPrimary = true,
                StateKey = "", // Auto-generated
                Description = alternatingAction.Description
            };
        }

        /// <summary>
        /// Extracts MIDI output configuration from a MIDI output action
        /// </summary>
        protected virtual MidiOutputConfig ExtractMidiOutputConfig(Core.Actions.Simple.MidiOutputAction midiOutputAction)
        {
            // Note: This is a simplified extraction. In a real implementation, you might need
            // to access private fields through reflection or add public getters to the action class
            return new MidiOutputConfig
            {
                OutputDeviceName = "Unknown Device", // Would need getter in MidiOutputAction
                Commands = new List<MidiOutputCommand>(), // Would need getter in MidiOutputAction
                Description = midiOutputAction.Description
            };
        }

        /// <summary>
        /// Extracts action configuration from a action instance
        /// </summary>
        protected virtual ActionConfig? ExtractActionConfig(IAction action)
        {
            // This is a simplified approach - in a real implementation, you might want
            // to use reflection or a more sophisticated mapping system
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
                Core.Actions.Simple.DelayAction delayAction => new DelayConfig
                {
                    Milliseconds = delayAction.Milliseconds,
                    Description = delayAction.Description
                },
                Core.Actions.Simple.MouseClickAction mouseAction => new MouseClickConfig
                {
                    Button = mouseAction.Button,
                    Description = mouseAction.Description
                },
                Core.Actions.Simple.MouseScrollAction scrollAction => new MouseScrollConfig
                {
                    Direction = scrollAction.Direction,
                    Amount = scrollAction.Amount,
                    Description = scrollAction.Description
                },
                Core.Actions.Simple.CommandExecutionAction cmdAction => new CommandExecutionConfig
                {
                    Command = cmdAction.Command,
                    ShellType = cmdAction.ShellType,
                    Description = cmdAction.Description
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
                Core.Actions.Simple.MidiOutputAction midiOutputAction => ExtractMidiOutputConfig(midiOutputAction),
                Core.Actions.Complex.SequenceAction sequenceAction => ExtractSequenceConfig(sequenceAction),
                Core.Actions.Complex.ConditionalAction conditionalAction => ExtractConditionalConfig(conditionalAction),
                Core.Actions.Complex.AlternatingAction alternatingAction => ExtractAlternatingConfig(alternatingAction),
                // Add more action types as needed
                _ => null
            };
        }

        /// <summary>
        /// Handles the TextChanged event of the DescriptionTextBox
        /// </summary>
        protected virtual void DescriptionTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_updatingUI) return;

            ApplicationErrorHandler.RunWithUiErrorHandling(() =>
            {
                _mapping.Description = descriptionTextBox.Text.Trim();
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

                // Set the dialog result
                DialogResult = DialogResult.OK;
            }, _logger, "saving unified mapping", this);
        }

        /// <summary>
        /// Handles the Click event of the CancelButton
        /// </summary>
        protected virtual void CancelButton_Click(object? sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        #endregion

        #region MIDI Listening

        /// <summary>
        /// Starts listening for MIDI input
        /// </summary>
        protected virtual void StartMidiListening()
        {
            if (_midiManager == null)
            {
                _logger.LogError("Cannot start MIDI listening: MidiManager is null");
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
                _midiManager.MidiEventReceived += OnMidiMessageReceived;

                // Log available devices for debugging
                var devices = _midiManager.GetAvailableDevices();
                _logger.LogInformation("Starting MIDI listening. Available devices: {DeviceCount}", devices.Count);
                foreach (var device in devices)
                {
                    _logger.LogDebug("  - Device: {DeviceName} (ID: {DeviceId}, Connected: {IsConnected})",
                        device.Name, device.DeviceId, device.IsConnected);
                }

                // Check if any devices are actively listening
                var activeDevices = _midiManager.ActiveDeviceIds;
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
            if (_midiManager == null || !_isListening)
                return;

            try
            {
                _isListening = false;
                listenButton.Text = "Listen";
                listenButton.BackColor = System.Drawing.SystemColors.Control;

                // Unsubscribe from MIDI events
                _midiManager.MidiEventReceived -= OnMidiMessageReceived;

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
                _logger.LogInformation("Dialog received MIDI event: DeviceId={DeviceId}, EventType={EventType}, Channel={Channel}, Note={Note}, Velocity={Velocity}",
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

                // Set channel (Event.Channel is already 1-based from MidiEventConverter)
                midiChannelComboBox.SelectedIndex = e.Event.Channel; // Channel is already 1-based, UI expects 1-based

                // Set device name based on device ID
                SetDeviceNameFromDeviceId(e.DeviceId);

                // Set input type and number based on message type
                switch (e.Event.EventType)
                {
                    case MidiEventType.NoteOn:
                        midiInputTypeComboBox.SelectedItem = ActionMidiInputType.NoteOn;
                        if (e.Event.Note.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Note.Value;
                        }
                        break;
                    case MidiEventType.NoteOff:
                        midiInputTypeComboBox.SelectedItem = ActionMidiInputType.NoteOff;
                        if (e.Event.Note.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Note.Value;
                        }
                        break;
                    case MidiEventType.ControlChange:
                        midiInputTypeComboBox.SelectedItem = ActionMidiInputType.ControlChange;
                        if (e.Event.Controller.HasValue)
                        {
                            midiInputNumberNumericUpDown.Value = e.Event.Controller.Value;
                        }
                        break;
                }

                // Stop listening after first message
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
        /// <param name="deviceId">The MIDI device ID</param>
        protected virtual void SetDeviceNameFromDeviceId(int deviceId)
        {
            try
            {
                if (_midiManager == null)
                {
                    _logger.LogWarning("MidiManager is null, cannot resolve device name for device ID {DeviceId}", deviceId);
                    return;
                }

                // Get device info from the MIDI manager
                var deviceInfo = _midiManager.GetDeviceInfo(deviceId);
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

                if (_midiManager != null)
                {
                    var outputDevices = _midiManager.GetAvailableOutputDevices();
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
                    _logger.LogWarning("MidiManager is null, cannot populate output device combo box");
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
