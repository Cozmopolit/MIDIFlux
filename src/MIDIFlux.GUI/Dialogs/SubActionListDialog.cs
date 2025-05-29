using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Dialogs;
using MIDIFlux.GUI.Controls.ProfileEditor;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Dialogs;

/// <summary>
/// Dialog for editing SubActionList parameters - allows adding, editing, removing, and reordering nested actions
/// </summary>
public partial class SubActionListDialog : BaseDialog
{
    private readonly ILogger _logger;
    private readonly List<ActionBase> _actions;
    private readonly BindingList<ActionDisplayModel> _displayActions;
    private readonly string _parameterDisplayName;

    // UI Controls - initialized in InitializeComponent()
    private ListBox _actionsListBox = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _removeButton = null!;
    private Button _moveUpButton = null!;
    private Button _moveDownButton = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;

    /// <summary>
    /// Gets the edited list of actions
    /// </summary>
    public List<ActionBase> Actions => _actions;

    /// <summary>
    /// Initializes a new instance of SubActionListDialog
    /// </summary>
    /// <param name="actions">The list of actions to edit</param>
    /// <param name="parameterDisplayName">Display name for the parameter (e.g., "Sub Actions", "Sequence Steps")</param>
    public SubActionListDialog(List<ActionBase> actions, string parameterDisplayName)
    {
        _logger = LoggingHelper.CreateLogger<SubActionListDialog>();
        _actions = new List<ActionBase>(actions ?? new List<ActionBase>());
        _displayActions = new BindingList<ActionDisplayModel>();
        _parameterDisplayName = parameterDisplayName;

        InitializeComponent();
        SetupEventHandlers();
        LoadActions();
        UpdateButtonStates();
    }

    private void InitializeComponent()
    {
        Text = $"Configure {_parameterDisplayName}";
        Size = new System.Drawing.Size(600, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new System.Drawing.Size(500, 300);

        // Create main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };

        // Configure column styles
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        // Actions list box
        _actionsListBox = new ListBox
        {
            Dock = DockStyle.Fill,
            DisplayMember = nameof(ActionDisplayModel.DisplayText),
            SelectionMode = SelectionMode.One
        };

        // Button panel
        var buttonPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        _addButton = new Button
        {
            Text = "Add...",
            Size = new System.Drawing.Size(80, 30),
            Location = new System.Drawing.Point(5, 5),
            UseVisualStyleBackColor = true
        };

        _editButton = new Button
        {
            Text = "Edit...",
            Size = new System.Drawing.Size(80, 30),
            Location = new System.Drawing.Point(5, 40),
            UseVisualStyleBackColor = true
        };

        _removeButton = new Button
        {
            Text = "Remove",
            Size = new System.Drawing.Size(80, 30),
            Location = new System.Drawing.Point(5, 75),
            UseVisualStyleBackColor = true
        };

        _moveUpButton = new Button
        {
            Text = "Move Up",
            Size = new System.Drawing.Size(80, 30),
            Location = new System.Drawing.Point(5, 120),
            UseVisualStyleBackColor = true
        };

        _moveDownButton = new Button
        {
            Text = "Move Down",
            Size = new System.Drawing.Size(80, 30),
            Location = new System.Drawing.Point(5, 155),
            UseVisualStyleBackColor = true
        };

        buttonPanel.Controls.AddRange(new Control[]
        {
            _addButton, _editButton, _removeButton, _moveUpButton, _moveDownButton
        });

        // Dialog buttons panel
        var dialogButtonPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(5)
        };

        _okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new System.Drawing.Size(75, 30),
            Location = new System.Drawing.Point(5, 10),
            UseVisualStyleBackColor = true
        };

        _cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Size = new System.Drawing.Size(75, 30),
            Location = new System.Drawing.Point(90, 10),
            UseVisualStyleBackColor = true
        };

        dialogButtonPanel.Controls.Add(_okButton);
        dialogButtonPanel.Controls.Add(_cancelButton);

        // Add controls to main layout
        mainPanel.Controls.Add(_actionsListBox, 0, 0);
        mainPanel.Controls.Add(buttonPanel, 1, 0);
        mainPanel.SetColumnSpan(dialogButtonPanel, 2);
        mainPanel.Controls.Add(dialogButtonPanel, 0, 1);

        Controls.Add(mainPanel);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void SetupEventHandlers()
    {
        _actionsListBox.SelectedIndexChanged += ActionsListBox_SelectedIndexChanged;
        _actionsListBox.DoubleClick += ActionsListBox_DoubleClick;
        _addButton.Click += AddButton_Click;
        _editButton.Click += EditButton_Click;
        _removeButton.Click += RemoveButton_Click;
        _moveUpButton.Click += MoveUpButton_Click;
        _moveDownButton.Click += MoveDownButton_Click;
    }

    private void LoadActions()
    {
        _displayActions.Clear();
        foreach (var action in _actions)
        {
            _displayActions.Add(new ActionDisplayModel(action));
        }
        _actionsListBox.DataSource = _displayActions;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _actionsListBox.SelectedIndex >= 0;
        var selectedIndex = _actionsListBox.SelectedIndex;
        var itemCount = _displayActions.Count;

        _editButton.Enabled = hasSelection;
        _removeButton.Enabled = hasSelection;
        _moveUpButton.Enabled = hasSelection && selectedIndex > 0;
        _moveDownButton.Enabled = hasSelection && selectedIndex < itemCount - 1;
    }

    #region Event Handlers

    private void ActionsListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private void ActionsListBox_DoubleClick(object? sender, EventArgs e)
    {
        if (_actionsListBox.SelectedIndex >= 0)
        {
            EditSelectedAction();
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            // Create a default action for the user to configure
            var defaultAction = new Core.Actions.Simple.KeyPressReleaseAction(); // 'A' key (default)

            // Create a temporary mapping for the dialog
            var tempMapping = new ActionMapping
            {
                Action = defaultAction,
                Description = "New Action",
                IsEnabled = true,
                Input = new MidiInput
                {
                    InputType = MidiInputType.NoteOn,
                    InputNumber = 60,
                    Channel = 1
                }
            };

            using var dialog = new ActionMappingDialog(tempMapping, null, actionOnly: true);
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _actions.Add((ActionBase)dialog.Mapping.Action);
                _displayActions.Add(new ActionDisplayModel((ActionBase)dialog.Mapping.Action));
                _actionsListBox.SelectedIndex = _displayActions.Count - 1;
                UpdateButtonStates();
            }
        }, _logger, "adding sub-action", this);
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        EditSelectedAction();
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            var selectedIndex = _actionsListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to remove this action?\n\n{_displayActions[selectedIndex].DisplayText}",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _actions.RemoveAt(selectedIndex);
                    _displayActions.RemoveAt(selectedIndex);

                    // Adjust selection
                    if (selectedIndex >= _displayActions.Count)
                        selectedIndex = _displayActions.Count - 1;
                    if (selectedIndex >= 0)
                        _actionsListBox.SelectedIndex = selectedIndex;

                    UpdateButtonStates();
                }
            }
        }, _logger, "removing sub-action", this);
    }

    private void MoveUpButton_Click(object? sender, EventArgs e)
    {
        MoveAction(-1);
    }

    private void MoveDownButton_Click(object? sender, EventArgs e)
    {
        MoveAction(1);
    }

    #endregion

    #region Helper Methods

    private void EditSelectedAction()
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            var selectedIndex = _actionsListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var action = _actions[selectedIndex];

                // Create a temporary mapping for the dialog
                var tempMapping = new ActionMapping
                {
                    Action = action,
                    Description = action.Description ?? "Action",
                    IsEnabled = true,
                    Input = new MidiInput
                    {
                        InputType = MidiInputType.NoteOn,
                        InputNumber = 60,
                        Channel = 1
                    }
                };

                using var dialog = new ActionMappingDialog(tempMapping, null, actionOnly: true);
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _actions[selectedIndex] = (ActionBase)dialog.Mapping.Action;
                    _displayActions[selectedIndex] = new ActionDisplayModel((ActionBase)dialog.Mapping.Action);
                    _actionsListBox.Refresh();
                }
            }
        }, _logger, "editing sub-action", this);
    }

    private void MoveAction(int direction)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            var selectedIndex = _actionsListBox.SelectedIndex;
            if (selectedIndex >= 0)
            {
                var newIndex = selectedIndex + direction;
                if (newIndex >= 0 && newIndex < _actions.Count)
                {
                    // Swap in both lists
                    var tempAction = _actions[selectedIndex];
                    _actions[selectedIndex] = _actions[newIndex];
                    _actions[newIndex] = tempAction;

                    var tempDisplay = _displayActions[selectedIndex];
                    _displayActions[selectedIndex] = _displayActions[newIndex];
                    _displayActions[newIndex] = tempDisplay;

                    // Update selection
                    _actionsListBox.SelectedIndex = newIndex;
                    _actionsListBox.Refresh();
                    UpdateButtonStates();
                }
            }
        }, _logger, "moving sub-action", this);
    }

    #endregion
}

/// <summary>
/// Display model for actions in the list
/// </summary>
public class ActionDisplayModel
{
    public ActionBase Action { get; }
    public string DisplayText { get; }

    public ActionDisplayModel(ActionBase action)
    {
        Action = action;
        DisplayText = $"{action.GetType().Name.Replace("Action", "")} - {action.Description ?? "No description"}";
    }
}
