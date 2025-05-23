namespace MIDIFlux.GUI.Dialogs
{
    partial class CCRangeEditDialog
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
            this.valueRangeGroupBox = new System.Windows.Forms.GroupBox();
            this.maxValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxValueLabel = new System.Windows.Forms.Label();
            this.minValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.minValueLabel = new System.Windows.Forms.Label();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.macroPanel = new System.Windows.Forms.Panel();
            this.macroNotImplementedLabel = new System.Windows.Forms.Label();
            this.commandPanel = new System.Windows.Forms.Panel();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.commandLabel = new System.Windows.Forms.Label();
            this.keyPanel = new System.Windows.Forms.Panel();
            this.selectKeyButton = new System.Windows.Forms.Button();
            this.keyTextBox = new System.Windows.Forms.TextBox();
            this.keyLabel = new System.Windows.Forms.Label();
            this.actionTypeComboBox = new System.Windows.Forms.ComboBox();
            this.actionTypeLabel = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.valueRangeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).BeginInit();
            this.actionGroupBox.SuspendLayout();
            this.macroPanel.SuspendLayout();
            this.commandPanel.SuspendLayout();
            this.keyPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // valueRangeGroupBox
            // 
            this.valueRangeGroupBox.Controls.Add(this.maxValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.maxValueLabel);
            this.valueRangeGroupBox.Controls.Add(this.minValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.minValueLabel);
            this.valueRangeGroupBox.Location = new System.Drawing.Point(12, 12);
            this.valueRangeGroupBox.Name = "valueRangeGroupBox";
            this.valueRangeGroupBox.Size = new System.Drawing.Size(360, 80);
            this.valueRangeGroupBox.TabIndex = 0;
            this.valueRangeGroupBox.TabStop = false;
            this.valueRangeGroupBox.Text = "Value Range";
            // 
            // maxValueNumericUpDown
            // 
            this.maxValueNumericUpDown.Location = new System.Drawing.Point(120, 51);
            this.maxValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.maxValueNumericUpDown.Name = "maxValueNumericUpDown";
            this.maxValueNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.maxValueNumericUpDown.TabIndex = 3;
            this.maxValueNumericUpDown.Value = new decimal(new int[] {
            127,
            0,
            0,
            0});
            // 
            // maxValueLabel
            // 
            this.maxValueLabel.AutoSize = true;
            this.maxValueLabel.Location = new System.Drawing.Point(20, 53);
            this.maxValueLabel.Name = "maxValueLabel";
            this.maxValueLabel.Size = new System.Drawing.Size(65, 15);
            this.maxValueLabel.TabIndex = 2;
            this.maxValueLabel.Text = "Max Value:";
            // 
            // minValueNumericUpDown
            // 
            this.minValueNumericUpDown.Location = new System.Drawing.Point(120, 22);
            this.minValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.minValueNumericUpDown.Name = "minValueNumericUpDown";
            this.minValueNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.minValueNumericUpDown.TabIndex = 1;
            // 
            // minValueLabel
            // 
            this.minValueLabel.AutoSize = true;
            this.minValueLabel.Location = new System.Drawing.Point(20, 24);
            this.minValueLabel.Name = "minValueLabel";
            this.minValueLabel.Size = new System.Drawing.Size(63, 15);
            this.minValueLabel.TabIndex = 0;
            this.minValueLabel.Text = "Min Value:";
            // 
            // actionGroupBox
            // 
            this.actionGroupBox.Controls.Add(this.macroPanel);
            this.actionGroupBox.Controls.Add(this.commandPanel);
            this.actionGroupBox.Controls.Add(this.keyPanel);
            this.actionGroupBox.Controls.Add(this.actionTypeComboBox);
            this.actionGroupBox.Controls.Add(this.actionTypeLabel);
            this.actionGroupBox.Location = new System.Drawing.Point(12, 98);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(360, 150);
            this.actionGroupBox.TabIndex = 1;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";
            // 
            // macroPanel
            // 
            this.macroPanel.Controls.Add(this.macroNotImplementedLabel);
            this.macroPanel.Location = new System.Drawing.Point(20, 80);
            this.macroPanel.Name = "macroPanel";
            this.macroPanel.Size = new System.Drawing.Size(334, 60);
            this.macroPanel.TabIndex = 4;
            this.macroPanel.Visible = false;
            // 
            // macroNotImplementedLabel
            // 
            this.macroNotImplementedLabel.AutoSize = true;
            this.macroNotImplementedLabel.Location = new System.Drawing.Point(3, 3);
            this.macroNotImplementedLabel.Name = "macroNotImplementedLabel";
            this.macroNotImplementedLabel.Size = new System.Drawing.Size(196, 15);
            this.macroNotImplementedLabel.TabIndex = 0;
            this.macroNotImplementedLabel.Text = "Macro editing is not yet implemented";
            // 
            // commandPanel
            // 
            this.commandPanel.Controls.Add(this.commandTextBox);
            this.commandPanel.Controls.Add(this.commandLabel);
            this.commandPanel.Location = new System.Drawing.Point(20, 80);
            this.commandPanel.Name = "commandPanel";
            this.commandPanel.Size = new System.Drawing.Size(334, 60);
            this.commandPanel.TabIndex = 3;
            this.commandPanel.Visible = false;
            // 
            // commandTextBox
            // 
            this.commandTextBox.Location = new System.Drawing.Point(100, 3);
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(231, 23);
            this.commandTextBox.TabIndex = 1;
            // 
            // commandLabel
            // 
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(3, 6);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(67, 15);
            this.commandLabel.TabIndex = 0;
            this.commandLabel.Text = "Command:";
            // 
            // keyPanel
            // 
            this.keyPanel.Controls.Add(this.selectKeyButton);
            this.keyPanel.Controls.Add(this.keyTextBox);
            this.keyPanel.Controls.Add(this.keyLabel);
            this.keyPanel.Location = new System.Drawing.Point(20, 80);
            this.keyPanel.Name = "keyPanel";
            this.keyPanel.Size = new System.Drawing.Size(334, 60);
            this.keyPanel.TabIndex = 2;
            // 
            // selectKeyButton
            // 
            this.selectKeyButton.Location = new System.Drawing.Point(256, 3);
            this.selectKeyButton.Name = "selectKeyButton";
            this.selectKeyButton.Size = new System.Drawing.Size(75, 23);
            this.selectKeyButton.TabIndex = 2;
            this.selectKeyButton.Text = "Select...";
            this.selectKeyButton.UseVisualStyleBackColor = true;
            // 
            // keyTextBox
            // 
            this.keyTextBox.Location = new System.Drawing.Point(100, 3);
            this.keyTextBox.Name = "keyTextBox";
            this.keyTextBox.Size = new System.Drawing.Size(150, 23);
            this.keyTextBox.TabIndex = 1;
            // 
            // keyLabel
            // 
            this.keyLabel.AutoSize = true;
            this.keyLabel.Location = new System.Drawing.Point(3, 6);
            this.keyLabel.Name = "keyLabel";
            this.keyLabel.Size = new System.Drawing.Size(29, 15);
            this.keyLabel.TabIndex = 0;
            this.keyLabel.Text = "Key:";
            // 
            // actionTypeComboBox
            // 
            this.actionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionTypeComboBox.FormattingEnabled = true;
            this.actionTypeComboBox.Items.AddRange(new object[] {
            "Key Press",
            "Command",
            "Macro"});
            this.actionTypeComboBox.Location = new System.Drawing.Point(120, 22);
            this.actionTypeComboBox.Name = "actionTypeComboBox";
            this.actionTypeComboBox.Size = new System.Drawing.Size(150, 23);
            this.actionTypeComboBox.TabIndex = 1;
            // 
            // actionTypeLabel
            // 
            this.actionTypeLabel.AutoSize = true;
            this.actionTypeLabel.Location = new System.Drawing.Point(20, 25);
            this.actionTypeLabel.Name = "actionTypeLabel";
            this.actionTypeLabel.Size = new System.Drawing.Size(73, 15);
            this.actionTypeLabel.TabIndex = 0;
            this.actionTypeLabel.Text = "Action Type:";
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(297, 254);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 3;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(216, 254);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 2;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // CCRangeEditDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(384, 289);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.actionGroupBox);
            this.Controls.Add(this.valueRangeGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CCRangeEditDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Edit CC Value Range";
            this.valueRangeGroupBox.ResumeLayout(false);
            this.valueRangeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).EndInit();
            this.actionGroupBox.ResumeLayout(false);
            this.actionGroupBox.PerformLayout();
            this.macroPanel.ResumeLayout(false);
            this.macroPanel.PerformLayout();
            this.commandPanel.ResumeLayout(false);
            this.commandPanel.PerformLayout();
            this.keyPanel.ResumeLayout(false);
            this.keyPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox valueRangeGroupBox;
        private System.Windows.Forms.NumericUpDown maxValueNumericUpDown;
        private System.Windows.Forms.Label maxValueLabel;
        private System.Windows.Forms.NumericUpDown minValueNumericUpDown;
        private System.Windows.Forms.Label minValueLabel;
        private System.Windows.Forms.GroupBox actionGroupBox;
        private System.Windows.Forms.ComboBox actionTypeComboBox;
        private System.Windows.Forms.Label actionTypeLabel;
        private System.Windows.Forms.Panel keyPanel;
        private System.Windows.Forms.Button selectKeyButton;
        private System.Windows.Forms.TextBox keyTextBox;
        private System.Windows.Forms.Label keyLabel;
        private System.Windows.Forms.Panel commandPanel;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.Panel macroPanel;
        private System.Windows.Forms.Label macroNotImplementedLabel;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}
