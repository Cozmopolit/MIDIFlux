using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Dialogs;
using Microsoft.Extensions.Logging;
using ActionParameterInfo = MIDIFlux.Core.Actions.Parameters.ParameterInfo;

namespace MIDIFlux.GUI.Controls.ProfileEditor;

/// <summary>
/// Factory class for automatically generating UI controls from action parameter metadata.
/// Supports all parameter types in the unified action parameter system.
/// </summary>
public static class ParameterControlFactory
{
    /// <summary>
    /// Creates a UI control for the specified parameter
    /// </summary>
    /// <param name="parameterInfo">The parameter metadata</param>
    /// <param name="action">The action instance for parameter updates</param>
    /// <param name="logger">Logger for error handling</param>
    /// <returns>A control configured for the parameter type</returns>
    public static Control CreateParameterControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        logger.LogDebug("Creating parameter control for {ParameterName} of type {ParameterType} with value {ParameterValue}",
            parameterInfo.Name, parameterInfo.Type, parameterInfo.Value);

        return SafeActivator.Execute(() =>
        {
            var control = parameterInfo.Type switch
            {
                ParameterType.Integer => CreateIntegerControl(parameterInfo, action, logger),
                ParameterType.String => CreateStringControl(parameterInfo, action, logger),
                ParameterType.Boolean => CreateBooleanControl(parameterInfo, action, logger),
                ParameterType.Enum => CreateEnumControl(parameterInfo, action, logger),
                ParameterType.ByteArray => CreateByteArrayControl(parameterInfo, action, logger),
                ParameterType.SubAction => CreateSubActionControl(parameterInfo, action, logger),
                ParameterType.SubActionList => CreateSubActionListControl(parameterInfo, action, logger),
                ParameterType.ValueConditionList => CreateValueConditionListControl(parameterInfo, action, logger),
                _ => CreateUnsupportedControl(parameterInfo, logger)
            };

            logger.LogDebug("Successfully created {ControlType} control for parameter {ParameterName}",
                control.GetType().Name, parameterInfo.Name);
            return control;
        }, logger, $"creating parameter control for {parameterInfo.Name} of type {parameterInfo.Type}",
        () => CreateUnsupportedControl(parameterInfo, logger))!;
    }

    /// <summary>
    /// Creates a labeled control panel with the parameter control
    /// </summary>
    /// <param name="parameterInfo">The parameter metadata</param>
    /// <param name="action">The action instance</param>
    /// <param name="logger">Logger for error handling</param>
    /// <returns>A panel containing label and control</returns>
    public static Panel CreateLabeledParameterControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        return SafeActivator.Execute(() =>
        {
            logger.LogDebug("Creating labeled parameter control for {ParameterName} of type {ParameterType}",
                parameterInfo.Name, parameterInfo.Type);

            var panel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                Padding = new Padding(5)
            };

            var label = new Label
            {
                Text = parameterInfo.DisplayName + ":",
                AutoSize = true,
                Location = new Point(5, 8),
                Width = 120
            };

            var control = CreateParameterControl(parameterInfo, action, logger);
            control.Location = new Point(130, 5);
            control.Width = panel.Width - 140;
            control.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            panel.Controls.Add(label);
            panel.Controls.Add(control);

            logger.LogDebug("Successfully created labeled parameter control for {ParameterName}", parameterInfo.Name);
            return panel;
        }, logger, $"creating labeled parameter control for {parameterInfo.Name}", () =>
        {
            // Return an error panel
            var errorPanel = new Panel
            {
                Height = 30,
                Dock = DockStyle.Top,
                Padding = new Padding(5)
            };

            var errorLabel = new Label
            {
                Text = $"Error creating control for {parameterInfo.DisplayName}",
                AutoSize = true,
                Location = new Point(5, 8),
                ForeColor = Color.Red
            };

            errorPanel.Controls.Add(errorLabel);
            return errorPanel;
        })!;
    }

    #region Parameter Type Implementations

    /// <summary>
    /// Creates a NumericUpDown control for integer parameters
    /// </summary>
    private static Control CreateIntegerControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var numericUpDown = new NumericUpDown
        {
            Name = $"param_{parameterInfo.Name}",
            DecimalPlaces = 0
        };

        // Set min/max from validation hints
        if (parameterInfo.ValidationHints != null)
        {
            if (parameterInfo.ValidationHints.TryGetValue("min", out var minObj) && minObj is int min)
                numericUpDown.Minimum = min;
            if (parameterInfo.ValidationHints.TryGetValue("max", out var maxObj) && maxObj is int max)
                numericUpDown.Maximum = max;
        }

        // Set current value
        if (parameterInfo.Value is int intValue)
            numericUpDown.Value = Math.Max(numericUpDown.Minimum, Math.Min(numericUpDown.Maximum, intValue));

        // Handle value changes
        numericUpDown.ValueChanged += (sender, e) =>
        {
            try
            {
                action.SetParameterValue(parameterInfo.Name, (int)numericUpDown.Value);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting integer parameter {ParameterName}", parameterInfo.Name);
            }
        };

        return numericUpDown;
    }

    /// <summary>
    /// Creates a TextBox control for string parameters
    /// </summary>
    private static Control CreateStringControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var textBox = new TextBox
        {
            Name = $"param_{parameterInfo.Name}",
            Text = parameterInfo.Value?.ToString() ?? string.Empty
        };

        // Set max length from validation hints
        if (parameterInfo.ValidationHints != null)
        {
            if (parameterInfo.ValidationHints.TryGetValue("maxLength", out var maxLengthObj) && maxLengthObj is int maxLength)
                textBox.MaxLength = maxLength;
        }

        // Handle text changes
        textBox.TextChanged += (sender, e) =>
        {
            try
            {
                action.SetParameterValue(parameterInfo.Name, textBox.Text);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting string parameter {ParameterName}", parameterInfo.Name);
            }
        };

        return textBox;
    }

    /// <summary>
    /// Creates a CheckBox control for boolean parameters
    /// </summary>
    private static Control CreateBooleanControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var checkBox = new CheckBox
        {
            Name = $"param_{parameterInfo.Name}",
            Text = parameterInfo.DisplayName,
            Checked = parameterInfo.Value is bool boolValue && boolValue,
            AutoSize = true
        };

        // Handle checked changes
        checkBox.CheckedChanged += (sender, e) =>
        {
            try
            {
                action.SetParameterValue(parameterInfo.Name, checkBox.Checked);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting boolean parameter {ParameterName}", parameterInfo.Name);
            }
        };

        return checkBox;
    }

    /// <summary>
    /// Creates a ComboBox control for enum parameters
    /// </summary>
    private static Control CreateEnumControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        // Check if this enum parameter supports key listening
        bool supportsKeyListening = parameterInfo.GetValidationHint<bool>("supportsKeyListening");

        if (supportsKeyListening)
        {
            return CreateKeyListeningEnumControl(parameterInfo, action, logger);
        }

        var comboBox = new ComboBox
        {
            Name = $"param_{parameterInfo.Name}",
            DropDownStyle = ComboBoxStyle.DropDownList
        };

        // Populate enum options from EnumDefinition
        if (parameterInfo.EnumDefinition != null)
        {
            comboBox.Items.AddRange(parameterInfo.EnumDefinition.Options);

            // Set current selection using proper enum conversion
            if (parameterInfo.Value != null)
            {
                // Use the enum-aware GetValue method to get the properly typed enum value
                var enumValue = parameterInfo.GetEnumValue<System.Windows.Forms.Keys>();
                var index = Array.IndexOf(parameterInfo.EnumDefinition.Values, enumValue);
                if (index >= 0 && index < parameterInfo.EnumDefinition.Options.Length)
                {
                    comboBox.SelectedIndex = index;
                }
            }
        }

        // Handle selection changes
        comboBox.SelectedIndexChanged += (sender, e) =>
        {
            try
            {
                if (comboBox.SelectedIndex >= 0 && parameterInfo.EnumDefinition != null)
                {
                    var selectedValue = parameterInfo.EnumDefinition.Values[comboBox.SelectedIndex];
                    action.SetParameterValue(parameterInfo.Name, selectedValue);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting enum parameter {ParameterName}", parameterInfo.Name);
            }
        };

        return comboBox;
    }

    /// <summary>
    /// Creates a composite control with ComboBox and Listen button for key listening enum parameters
    /// </summary>
    private static Control CreateKeyListeningEnumControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        // Create a panel to hold both the ComboBox and Listen button
        var panel = new Panel
        {
            Name = $"param_{parameterInfo.Name}_panel",
            Height = 25,
            Dock = DockStyle.None
        };

        // Create the ComboBox
        var comboBox = new ComboBox
        {
            Name = $"param_{parameterInfo.Name}",
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(0, 0),
            Width = panel.Width - 80 // Leave space for the Listen button
        };

        // Create the Listen button
        var listenButton = new Button
        {
            Name = $"param_{parameterInfo.Name}_listen",
            Text = "Listen",
            Location = new Point(panel.Width - 75, 0),
            Width = 70,
            Height = 23,
            UseVisualStyleBackColor = true
        };

        // Populate enum options from EnumDefinition
        if (parameterInfo.EnumDefinition != null)
        {
            comboBox.Items.AddRange(parameterInfo.EnumDefinition.Options);

            // Set current selection using proper enum conversion
            if (parameterInfo.Value != null)
            {
                var enumValue = parameterInfo.GetEnumValue<System.Windows.Forms.Keys>();
                var index = Array.IndexOf(parameterInfo.EnumDefinition.Values, enumValue);
                if (index >= 0 && index < parameterInfo.EnumDefinition.Options.Length)
                {
                    comboBox.SelectedIndex = index;
                }
            }
        }

        // Handle ComboBox selection changes
        comboBox.SelectedIndexChanged += (sender, e) =>
        {
            try
            {
                if (comboBox.SelectedIndex >= 0 && parameterInfo.EnumDefinition != null)
                {
                    var selectedValue = parameterInfo.EnumDefinition.Values[comboBox.SelectedIndex];
                    action.SetParameterValue(parameterInfo.Name, selectedValue);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting enum parameter {ParameterName}", parameterInfo.Name);
            }
        };

        // Handle Listen button click - we'll need to find the parent dialog to handle this
        listenButton.Click += (sender, e) =>
        {
            try
            {
                // Find the parent ActionMappingDialog to handle key listening
                var parentDialog = FindParentDialog(panel);
                if (parentDialog is ActionMappingDialog actionDialog)
                {
                    // Call the key listening method directly
                    actionDialog.StartKeyListening(parameterInfo.Name, comboBox);
                }
                else
                {
                    logger.LogWarning("Could not find parent ActionMappingDialog for key listening");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error starting key listening for parameter {ParameterName}", parameterInfo.Name);
            }
        };

        // Handle panel resize to adjust ComboBox width
        panel.Resize += (sender, e) =>
        {
            comboBox.Width = panel.Width - 80;
            listenButton.Location = new Point(panel.Width - 75, 0);
        };

        // Add controls to panel
        panel.Controls.Add(comboBox);
        panel.Controls.Add(listenButton);

        return panel;
    }

    /// <summary>
    /// Finds the parent ActionMappingDialog for a control
    /// </summary>
    private static Form? FindParentDialog(Control control)
    {
        Control? current = control;
        while (current != null)
        {
            if (current is Form form && form.GetType().Name.Contains("ActionMappingDialog"))
            {
                return form;
            }
            current = current.Parent;
        }
        return null;
    }

    /// <summary>
    /// Creates a specialized control for byte array parameters (used by SysEx)
    /// </summary>
    private static Control CreateByteArrayControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var panel = new Panel
        {
            Name = $"param_{parameterInfo.Name}",
            Height = 60,
            BorderStyle = BorderStyle.FixedSingle
        };

        var textBox = new TextBox
        {
            Multiline = true,
            Dock = DockStyle.Fill,
            PlaceholderText = "Enter hex bytes separated by spaces (e.g., F0 43 12 00 F7)"
        };

        // Set current value using HexByteConverter
        if (parameterInfo.Value is byte[] byteArray)
        {
            textBox.Text = HexByteConverter.FormatByteArray(byteArray);
        }

        // Handle text changes with validation
        textBox.TextChanged += (sender, e) =>
        {
            try
            {
                var hexString = textBox.Text.Trim();
                if (string.IsNullOrEmpty(hexString))
                {
                    action.SetParameterValue(parameterInfo.Name, Array.Empty<byte>());
                    textBox.BackColor = SystemColors.Window; // Clear error indication
                    return;
                }

                // Use HexByteConverter for consistent hex parsing
                if (HexByteConverter.TryParseHexString(hexString, out var bytes))
                {
                    action.SetParameterValue(parameterInfo.Name, bytes);
                    textBox.BackColor = SystemColors.Window; // Clear error indication
                }
                else
                {
                    // Invalid hex format - show validation error
                    textBox.BackColor = Color.LightPink; // Indicate error
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error setting byte array parameter {ParameterName}", parameterInfo.Name);
                textBox.BackColor = Color.LightPink; // Indicate error
            }
        };

        panel.Controls.Add(textBox);
        return panel;
    }

    /// <summary>
    /// Creates a button control for SubAction parameters that opens a single action dialog
    /// </summary>
    private static Control CreateSubActionControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        // Create a panel to hold both the button and description label
        var panel = new Panel
        {
            Name = $"param_{parameterInfo.Name}_panel",
            Height = 25
        };

        var button = new Button
        {
            Name = $"param_{parameterInfo.Name}",
            Text = "Configure...",
            UseVisualStyleBackColor = true,
            Width = 100,
            Height = 23,
            Location = new Point(0, 1)
        };

        var descriptionLabel = new Label
        {
            Name = $"param_{parameterInfo.Name}_desc",
            AutoSize = false,
            Height = 23,
            Location = new Point(105, 1),
            TextAlign = ContentAlignment.MiddleLeft,
            ForeColor = SystemColors.GrayText,
            Text = GetSubActionDescription(parameterInfo.Value as ActionBase)
        };

        // Update description when panel resizes
        panel.Resize += (sender, e) =>
        {
            descriptionLabel.Width = panel.Width - 110;
        };

        // Handle button click to open single action configuration dialog
        button.Click += (sender, e) =>
        {
            SafeActivator.Execute(() =>
            {
                var currentAction = parameterInfo.Value as ActionBase ?? new KeyPressReleaseAction();

                // Create a temporary mapping for the dialog
                var tempMapping = new ActionMapping
                {
                    Action = currentAction,
                    Description = currentAction.Description ?? "Action",
                    IsEnabled = true,
                    Input = new MidiInput
                    {
                        InputType = MidiInputType.NoteOn,
                        InputNumber = 60,
                        Channel = 1
                    }
                };

                using var dialog = new ActionMappingDialog(tempMapping, null, true, LoggingHelper.CreateLogger<ActionMappingDialog>());
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the parameter value with the edited action
                    action.SetParameterValue(parameterInfo.Name, dialog.Mapping.Action);

                    // Update the description label
                    descriptionLabel.Text = GetSubActionDescription(dialog.Mapping.Action as ActionBase);
                }
            }, logger, $"opening sub-action dialog for parameter {parameterInfo.Name}");
        };

        panel.Controls.Add(button);
        panel.Controls.Add(descriptionLabel);
        return panel;
    }

    /// <summary>
    /// Gets a formatted description for a sub-action
    /// </summary>
    private static string GetSubActionDescription(ActionBase? action)
    {
        if (action == null)
            return "No action configured";

        var actionType = action.GetType().Name.Replace("Action", "");
        var description = action.Description ?? "No description";
        return $"{actionType}: {description}";
    }

    /// <summary>
    /// Creates a button control for SubActionList parameters that opens a dialog
    /// </summary>
    private static Control CreateSubActionListControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var button = new Button
        {
            Name = $"param_{parameterInfo.Name}",
            Text = "Configure Actions...",
            UseVisualStyleBackColor = true
        };

        // Handle button click to open sub-action configuration dialog
        button.Click += (sender, e) =>
        {
            SafeActivator.Execute(() =>
            {
                var subActions = parameterInfo.Value as List<ActionBase> ?? new List<ActionBase>();

                using var dialog = new SubActionListDialog(subActions, parameterInfo.DisplayName, LoggingHelper.CreateLogger<SubActionListDialog>());
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the parameter value with the edited actions
                    action.SetParameterValue(parameterInfo.Name, dialog.Actions);
                }
            }, logger, $"opening sub-action list dialog for parameter {parameterInfo.Name}");
        };

        return button;
    }

    /// <summary>
    /// Creates a button control for ValueConditionList parameters that opens a dialog
    /// </summary>
    private static Control CreateValueConditionListControl(ActionParameterInfo parameterInfo, ActionBase action, ILogger logger)
    {
        var button = new Button
        {
            Name = $"param_{parameterInfo.Name}",
            Text = "Configure Conditions...",
            UseVisualStyleBackColor = true
        };

        // Handle button click to open condition configuration dialog
        button.Click += (sender, e) =>
        {
            SafeActivator.Execute(() =>
            {
                var conditions = parameterInfo.Value as List<ValueCondition> ?? new List<ValueCondition>();

                using var dialog = new ValueConditionListDialog(conditions, parameterInfo.DisplayName, LoggingHelper.CreateLogger<ValueConditionListDialog>());
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    // Update the parameter value with the edited conditions
                    action.SetParameterValue(parameterInfo.Name, dialog.Conditions);
                }
            }, logger, $"opening condition dialog for parameter {parameterInfo.Name}");
        };

        return button;
    }

    /// <summary>
    /// Creates a placeholder control for unsupported parameter types
    /// </summary>
    private static Control CreateUnsupportedControl(ActionParameterInfo parameterInfo, ILogger logger)
    {
        logger.LogWarning("Unsupported parameter type {ParameterType} for parameter {ParameterName}",
            parameterInfo.Type, parameterInfo.Name);

        return new Label
        {
            Text = $"Unsupported parameter type: {parameterInfo.Type}",
            ForeColor = Color.Red,
            AutoSize = true
        };
    }

    #endregion
}
