namespace MIDIFlux.GUI.Dialogs
{
    partial class MacroActionDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.actionTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.actionTypeComboBox = new System.Windows.Forms.ComboBox();
            this.keyboardPanel = new System.Windows.Forms.Panel();
            this.commandPanel = new System.Windows.Forms.Panel();
            this.delayPanel = new System.Windows.Forms.Panel();
            this.delayAfterGroupBox = new System.Windows.Forms.GroupBox();
            this.delayAfterNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();

            // Keyboard panel controls
            this.keyboardActionTypeComboBox = new System.Windows.Forms.ComboBox();
            this.virtualKeyComboBox = new System.Windows.Forms.ComboBox();
            this.shiftCheckBox = new System.Windows.Forms.CheckBox();
            this.ctrlCheckBox = new System.Windows.Forms.CheckBox();
            this.altCheckBox = new System.Windows.Forms.CheckBox();
            this.winCheckBox = new System.Windows.Forms.CheckBox();

            // Command panel controls
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.shellTypeComboBox = new System.Windows.Forms.ComboBox();
            this.runHiddenCheckBox = new System.Windows.Forms.CheckBox();
            this.waitForExitCheckBox = new System.Windows.Forms.CheckBox();

            // Delay panel controls
            this.millisecondsNumericUpDown = new System.Windows.Forms.NumericUpDown();





            this.actionTypeGroupBox.SuspendLayout();
            this.delayAfterGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.delayAfterNumericUpDown)).BeginInit();
            this.descriptionGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.millisecondsNumericUpDown)).BeginInit();
            this.SuspendLayout();

            // Action Type GroupBox
            this.actionTypeGroupBox.Location = new System.Drawing.Point(12, 12);
            this.actionTypeGroupBox.Size = new System.Drawing.Size(460, 60);
            this.actionTypeGroupBox.Text = "Action Type";
            this.actionTypeGroupBox.Controls.Add(this.actionTypeComboBox);

            // Action Type ComboBox
            this.actionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionTypeComboBox.Location = new System.Drawing.Point(20, 22);
            this.actionTypeComboBox.Size = new System.Drawing.Size(200, 23);

            // Keyboard Panel
            this.keyboardPanel.Location = new System.Drawing.Point(12, 78);
            this.keyboardPanel.Size = new System.Drawing.Size(460, 120);
            this.keyboardPanel.Visible = false;
            this.keyboardPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Add controls to keyboard panel
            this.keyboardPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Action:", Location = new System.Drawing.Point(10, 10), AutoSize = true });
            this.keyboardPanel.Controls.Add(this.keyboardActionTypeComboBox);
            this.keyboardPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Key:", Location = new System.Drawing.Point(10, 40), AutoSize = true });
            this.keyboardPanel.Controls.Add(this.virtualKeyComboBox);
            this.keyboardPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Modifiers:", Location = new System.Drawing.Point(10, 70), AutoSize = true });
            this.keyboardPanel.Controls.Add(this.shiftCheckBox);
            this.keyboardPanel.Controls.Add(this.ctrlCheckBox);
            this.keyboardPanel.Controls.Add(this.altCheckBox);
            this.keyboardPanel.Controls.Add(this.winCheckBox);

            this.keyboardActionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.keyboardActionTypeComboBox.Location = new System.Drawing.Point(100, 7);
            this.keyboardActionTypeComboBox.Size = new System.Drawing.Size(200, 23);

            this.virtualKeyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.virtualKeyComboBox.Location = new System.Drawing.Point(100, 37);
            this.virtualKeyComboBox.Size = new System.Drawing.Size(200, 23);

            this.shiftCheckBox.Text = "Shift";
            this.shiftCheckBox.Location = new System.Drawing.Point(100, 70);
            this.shiftCheckBox.AutoSize = true;

            this.ctrlCheckBox.Text = "Ctrl";
            this.ctrlCheckBox.Location = new System.Drawing.Point(160, 70);
            this.ctrlCheckBox.AutoSize = true;

            this.altCheckBox.Text = "Alt";
            this.altCheckBox.Location = new System.Drawing.Point(220, 70);
            this.altCheckBox.AutoSize = true;

            this.winCheckBox.Text = "Win";
            this.winCheckBox.Location = new System.Drawing.Point(280, 70);
            this.winCheckBox.AutoSize = true;

            // Command Panel
            this.commandPanel.Location = new System.Drawing.Point(12, 78);
            this.commandPanel.Size = new System.Drawing.Size(460, 120);
            this.commandPanel.Visible = false;
            this.commandPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Add controls to command panel
            this.commandPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Command:", Location = new System.Drawing.Point(10, 10), AutoSize = true });
            this.commandPanel.Controls.Add(this.commandTextBox);
            this.commandPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Shell Type:", Location = new System.Drawing.Point(10, 40), AutoSize = true });
            this.commandPanel.Controls.Add(this.shellTypeComboBox);
            this.commandPanel.Controls.Add(this.runHiddenCheckBox);
            this.commandPanel.Controls.Add(this.waitForExitCheckBox);

            this.commandTextBox.Location = new System.Drawing.Point(100, 7);
            this.commandTextBox.Size = new System.Drawing.Size(350, 23);

            this.shellTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shellTypeComboBox.Location = new System.Drawing.Point(100, 37);
            this.shellTypeComboBox.Size = new System.Drawing.Size(200, 23);

            this.runHiddenCheckBox.Text = "Run Hidden";
            this.runHiddenCheckBox.Location = new System.Drawing.Point(100, 70);
            this.runHiddenCheckBox.AutoSize = true;

            this.waitForExitCheckBox.Text = "Wait for Exit";
            this.waitForExitCheckBox.Location = new System.Drawing.Point(220, 70);
            this.waitForExitCheckBox.AutoSize = true;

            // Delay Panel
            this.delayPanel.Location = new System.Drawing.Point(12, 78);
            this.delayPanel.Size = new System.Drawing.Size(460, 120);
            this.delayPanel.Visible = false;
            this.delayPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;

            // Add controls to delay panel
            this.delayPanel.Controls.Add(new System.Windows.Forms.Label { Text = "Milliseconds:", Location = new System.Drawing.Point(10, 10), AutoSize = true });
            this.delayPanel.Controls.Add(this.millisecondsNumericUpDown);

            this.millisecondsNumericUpDown.Location = new System.Drawing.Point(100, 7);
            this.millisecondsNumericUpDown.Size = new System.Drawing.Size(100, 23);
            this.millisecondsNumericUpDown.Maximum = 60000;
            this.millisecondsNumericUpDown.Minimum = 1;
            this.millisecondsNumericUpDown.Value = 1000;





            // Delay After GroupBox
            this.delayAfterGroupBox.Location = new System.Drawing.Point(12, 204);
            this.delayAfterGroupBox.Size = new System.Drawing.Size(460, 60);
            this.delayAfterGroupBox.Text = "Delay After Action (ms)";
            this.delayAfterGroupBox.Controls.Add(this.delayAfterNumericUpDown);

            this.delayAfterNumericUpDown.Location = new System.Drawing.Point(20, 22);
            this.delayAfterNumericUpDown.Size = new System.Drawing.Size(100, 23);
            this.delayAfterNumericUpDown.Maximum = 60000;
            this.delayAfterNumericUpDown.Minimum = 0;

            // Description GroupBox
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 270);
            this.descriptionGroupBox.Size = new System.Drawing.Size(460, 80);
            this.descriptionGroupBox.Text = "Description";
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);

            this.descriptionTextBox.Location = new System.Drawing.Point(6, 22);
            this.descriptionTextBox.Size = new System.Drawing.Size(448, 52);
            this.descriptionTextBox.Multiline = true;

            // Basic form setup
            this.ClientSize = new System.Drawing.Size(484, 361);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.delayAfterGroupBox);


            this.Controls.Add(this.delayPanel);
            this.Controls.Add(this.commandPanel);
            this.Controls.Add(this.keyboardPanel);
            this.Controls.Add(this.actionTypeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MacroActionDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit Action";

            // Set up OK and Cancel buttons
            this.okButton.Location = new System.Drawing.Point(316, 326);
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.Text = "OK";
            this.okButton.Click += new System.EventHandler(this.okButton_Click);

            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 326);
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.Text = "Cancel";

            this.actionTypeGroupBox.ResumeLayout(false);
            this.delayAfterGroupBox.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.delayAfterNumericUpDown)).EndInit();
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.millisecondsNumericUpDown)).EndInit();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.GroupBox actionTypeGroupBox;
        private System.Windows.Forms.ComboBox actionTypeComboBox;
        private System.Windows.Forms.Panel keyboardPanel;
        private System.Windows.Forms.Panel commandPanel;
        private System.Windows.Forms.Panel delayPanel;

        private System.Windows.Forms.GroupBox delayAfterGroupBox;
        private System.Windows.Forms.NumericUpDown delayAfterNumericUpDown;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;

        // Keyboard panel controls
        private System.Windows.Forms.ComboBox keyboardActionTypeComboBox;
        private System.Windows.Forms.ComboBox virtualKeyComboBox;
        private System.Windows.Forms.CheckBox shiftCheckBox;
        private System.Windows.Forms.CheckBox ctrlCheckBox;
        private System.Windows.Forms.CheckBox altCheckBox;
        private System.Windows.Forms.CheckBox winCheckBox;

        // Command panel controls
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.ComboBox shellTypeComboBox;
        private System.Windows.Forms.CheckBox runHiddenCheckBox;
        private System.Windows.Forms.CheckBox waitForExitCheckBox;

        // Delay panel controls
        private System.Windows.Forms.NumericUpDown millisecondsNumericUpDown;




    }
}
