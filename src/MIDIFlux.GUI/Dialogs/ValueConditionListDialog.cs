using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Parameters;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Dialogs;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Dialogs;

/// <summary>
/// Dialog for editing ValueConditionList parameters - allows adding, editing, removing value conditions
/// </summary>
public partial class ValueConditionListDialog : BaseDialog
{
    private readonly List<ValueCondition> _conditions;
    private readonly BindingList<ValueConditionDisplayModel> _displayConditions;
    private readonly string _parameterDisplayName;

    // UI Controls - initialized in InitializeComponent()
    private DataGridView _conditionsDataGridView = null!;
    private Button _addButton = null!;
    private Button _editButton = null!;
    private Button _removeButton = null!;
    private Button _okButton = null!;
    private Button _cancelButton = null!;

    /// <summary>
    /// Gets the edited list of value conditions
    /// </summary>
    public List<ValueCondition> Conditions => _conditions;

    /// <summary>
    /// Initializes a new instance of ValueConditionListDialog
    /// </summary>
    /// <param name="conditions">The list of conditions to edit</param>
    /// <param name="parameterDisplayName">Display name for the parameter (e.g., "Conditions", "Value Ranges")</param>
    /// <param name="logger">The logger to use for this dialog</param>
    public ValueConditionListDialog(List<ValueCondition> conditions, string parameterDisplayName, ILogger<ValueConditionListDialog> logger) : base(logger)
    {
        _conditions = new List<ValueCondition>(conditions ?? new List<ValueCondition>());
        _displayConditions = new BindingList<ValueConditionDisplayModel>();
        _parameterDisplayName = parameterDisplayName;

        InitializeComponent();
        SetupEventHandlers();
        LoadConditions();
        UpdateButtonStates();
    }

    private void InitializeComponent()
    {
        Text = $"Configure {_parameterDisplayName}";
        Size = new System.Drawing.Size(700, 450);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new System.Drawing.Size(600, 350);

        // Create main layout
        var mainPanel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(10)
        };

        // Configure column styles
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 75F));
        mainPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        mainPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));

        // Conditions data grid view
        _conditionsDataGridView = new DataGridView
        {
            Dock = DockStyle.Fill,
            AutoGenerateColumns = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            ReadOnly = true
        };

        // Configure columns
        _conditionsDataGridView.Columns.AddRange(new DataGridViewColumn[]
        {
            new DataGridViewTextBoxColumn
            {
                Name = "MinValue",
                HeaderText = "Min Value",
                DataPropertyName = nameof(ValueConditionDisplayModel.MinValue),
                Width = 80
            },
            new DataGridViewTextBoxColumn
            {
                Name = "MaxValue",
                HeaderText = "Max Value",
                DataPropertyName = nameof(ValueConditionDisplayModel.MaxValue),
                Width = 80
            },
            new DataGridViewTextBoxColumn
            {
                Name = "ActionType",
                HeaderText = "Action Type",
                DataPropertyName = nameof(ValueConditionDisplayModel.ActionType),
                Width = 120
            },
            new DataGridViewTextBoxColumn
            {
                Name = "ActionDescription",
                HeaderText = "Action Description",
                DataPropertyName = nameof(ValueConditionDisplayModel.ActionDescription),
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            }
        });

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

        buttonPanel.Controls.AddRange(new Control[] { _addButton, _editButton, _removeButton });

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
        mainPanel.Controls.Add(_conditionsDataGridView, 0, 0);
        mainPanel.Controls.Add(buttonPanel, 1, 0);
        mainPanel.SetColumnSpan(dialogButtonPanel, 2);
        mainPanel.Controls.Add(dialogButtonPanel, 0, 1);

        Controls.Add(mainPanel);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void SetupEventHandlers()
    {
        _conditionsDataGridView.SelectionChanged += ConditionsDataGridView_SelectionChanged;
        _conditionsDataGridView.CellDoubleClick += ConditionsDataGridView_CellDoubleClick;
        _addButton.Click += AddButton_Click;
        _editButton.Click += EditButton_Click;
        _removeButton.Click += RemoveButton_Click;
    }

    private void LoadConditions()
    {
        _displayConditions.Clear();
        foreach (var condition in _conditions)
        {
            _displayConditions.Add(new ValueConditionDisplayModel(condition));
        }
        _conditionsDataGridView.DataSource = _displayConditions;
    }

    private void UpdateButtonStates()
    {
        var hasSelection = _conditionsDataGridView.SelectedRows.Count > 0;
        _editButton.Enabled = hasSelection;
        _removeButton.Enabled = hasSelection;
    }

    #region Event Handlers

    private void ConditionsDataGridView_SelectionChanged(object? sender, EventArgs e)
    {
        UpdateButtonStates();
    }

    private void ConditionsDataGridView_CellDoubleClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            EditSelectedCondition();
        }
    }

    private void AddButton_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            // Create a default condition
            var defaultCondition = new ValueCondition
            {
                MinValue = 0,
                MaxValue = 127,
                Action = new Core.Actions.Simple.KeyPressReleaseAction(Keys.A) // 'A' key
            };

            using var dialog = new ValueConditionEditDialog(defaultCondition, LoggingHelper.CreateLogger<ValueConditionEditDialog>());
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _conditions.Add(dialog.Condition);
                _displayConditions.Add(new ValueConditionDisplayModel(dialog.Condition));
                UpdateButtonStates();
            }
        }, _logger, "adding value condition", this);
    }

    private void EditButton_Click(object? sender, EventArgs e)
    {
        EditSelectedCondition();
    }

    private void RemoveButton_Click(object? sender, EventArgs e)
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            if (_conditionsDataGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = _conditionsDataGridView.SelectedRows[0].Index;
                var condition = _displayConditions[selectedIndex];

                var result = MessageBox.Show(
                    $"Are you sure you want to remove this condition?\n\nRange: {condition.MinValue}-{condition.MaxValue}\nAction: {condition.ActionDescription}",
                    "Confirm Removal",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _conditions.RemoveAt(selectedIndex);
                    _displayConditions.RemoveAt(selectedIndex);
                    UpdateButtonStates();
                }
            }
        }, _logger, "removing value condition", this);
    }

    #endregion

    #region Helper Methods

    private void EditSelectedCondition()
    {
        ApplicationErrorHandler.RunWithUiErrorHandling(() =>
        {
            if (_conditionsDataGridView.SelectedRows.Count > 0)
            {
                var selectedIndex = _conditionsDataGridView.SelectedRows[0].Index;
                var condition = _conditions[selectedIndex];

                using var dialog = new ValueConditionEditDialog(condition, LoggingHelper.CreateLogger<ValueConditionEditDialog>());
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    _conditions[selectedIndex] = dialog.Condition;
                    _displayConditions[selectedIndex] = new ValueConditionDisplayModel(dialog.Condition);
                    _conditionsDataGridView.Refresh();
                }
            }
        }, _logger, "editing value condition", this);
    }

    #endregion
}

/// <summary>
/// Display model for value conditions in the grid
/// </summary>
public class ValueConditionDisplayModel
{
    public ValueCondition Condition { get; }
    public int MinValue { get; }
    public int MaxValue { get; }
    public string ActionType { get; }
    public string ActionDescription { get; }

    public ValueConditionDisplayModel(ValueCondition condition)
    {
        Condition = condition;
        MinValue = condition.MinValue;
        MaxValue = condition.MaxValue;
        ActionType = condition.Action?.GetType().Name.Replace("Action", "") ?? "None";
        ActionDescription = condition.Action?.Description ?? "No description";
    }
}
