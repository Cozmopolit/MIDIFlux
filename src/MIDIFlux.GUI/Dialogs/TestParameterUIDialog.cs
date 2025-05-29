using System;
using System.Windows.Forms;
using MIDIFlux.Core.Actions;
using MIDIFlux.Core.Actions.Simple;
using MIDIFlux.Core.Helpers;
using MIDIFlux.GUI.Controls.ProfileEditor;
using Microsoft.Extensions.Logging;

namespace MIDIFlux.GUI.Dialogs;

/// <summary>
/// Test dialog for verifying automatic parameter UI generation
/// </summary>
public partial class TestParameterUIDialog : Form
{
    private readonly ILogger _logger;
    private ActionBase _testAction = null!; // Initialized in CreateTestAction()
    private Panel _parameterPanel = null!; // Initialized in InitializeDialog()
    private Button _okButton = null!; // Initialized in InitializeDialog()
    private Button _cancelButton = null!; // Initialized in InitializeDialog()

    public TestParameterUIDialog()
    {
        _logger = LoggingHelper.CreateLogger<TestParameterUIDialog>();
        InitializeDialog();
        CreateTestAction();
        LoadParameterControls();
    }

    private void InitializeDialog()
    {
        Text = "Test Parameter UI Generation";
        Size = new System.Drawing.Size(500, 400);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;

        // Create main panel for parameters
        _parameterPanel = new Panel
        {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            Padding = new Padding(10)
        };

        // Create button panel
        var buttonPanel = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 50,
            Padding = new Padding(10)
        };

        _okButton = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Size = new System.Drawing.Size(75, 23),
            Location = new System.Drawing.Point(10, 10)
        };

        _cancelButton = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Size = new System.Drawing.Size(75, 23),
            Location = new System.Drawing.Point(95, 10)
        };

        buttonPanel.Controls.Add(_okButton);
        buttonPanel.Controls.Add(_cancelButton);

        Controls.Add(_parameterPanel);
        Controls.Add(buttonPanel);

        AcceptButton = _okButton;
        CancelButton = _cancelButton;
    }

    private void CreateTestAction()
    {
        // Create a test action with various parameter types
        _testAction = new KeyPressReleaseAction(Keys.A); // 'A' key
    }

    private void LoadParameterControls()
    {
        try
        {
            _parameterPanel.Controls.Clear();

            var parameterInfos = _testAction.GetParameterList();

            if (parameterInfos.Count == 0)
            {
                var noParamsLabel = new Label
                {
                    Text = "This action has no configurable parameters.",
                    AutoSize = true,
                    Location = new System.Drawing.Point(10, 10),
                    ForeColor = System.Drawing.SystemColors.GrayText
                };
                _parameterPanel.Controls.Add(noParamsLabel);
                return;
            }

            // Add title
            var titleLabel = new Label
            {
                Text = $"Parameters for {_testAction.GetType().Name}:",
                Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10)
            };
            _parameterPanel.Controls.Add(titleLabel);

            // Create controls for each parameter
            var yPosition = 40;
            foreach (var parameterInfo in parameterInfos)
            {
                var parameterPanel = ParameterControlFactory.CreateLabeledParameterControl(
                    parameterInfo, _testAction, _logger);

                parameterPanel.Location = new System.Drawing.Point(10, yPosition);
                parameterPanel.Width = _parameterPanel.Width - 40;
                parameterPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

                _parameterPanel.Controls.Add(parameterPanel);
                yPosition += parameterPanel.Height + 5;
            }

            // Add action info
            var infoLabel = new Label
            {
                Text = $"Action Type: {_testAction.GetType().Name}\nParameter Count: {parameterInfos.Count}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, yPosition + 10),
                ForeColor = System.Drawing.SystemColors.ControlDarkDark
            };
            _parameterPanel.Controls.Add(infoLabel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading parameter controls");

            var errorLabel = new Label
            {
                Text = $"Error loading parameters: {ex.Message}",
                AutoSize = true,
                Location = new System.Drawing.Point(10, 10),
                ForeColor = System.Drawing.Color.Red
            };
            _parameterPanel.Controls.Add(errorLabel);
        }
    }

    /// <summary>
    /// Changes the test action to a different type for testing
    /// </summary>
    public void SetTestAction(ActionBase action)
    {
        _testAction = action;
        LoadParameterControls();
    }
}

/// <summary>
/// Designer partial class for TestParameterUIDialog
/// </summary>
public partial class TestParameterUIDialog
{
    private System.ComponentModel.IContainer? components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }
}
