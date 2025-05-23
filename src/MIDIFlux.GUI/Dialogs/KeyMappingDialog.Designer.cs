namespace MIDIFlux.GUI.Dialogs
{
    partial class KeyMappingDialog
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
            this.midiTriggerGroupBox = new System.Windows.Forms.GroupBox();
            this.listenButton = new System.Windows.Forms.Button();
            this.noteNameLabel = new System.Windows.Forms.Label();
            this.noteNameLabelLabel = new System.Windows.Forms.Label();
            this.midiNoteNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.midiNoteLabel = new System.Windows.Forms.Label();
            this.actionTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.actionTypeComboBox = new System.Windows.Forms.ComboBox();
            this.keySelectionPanel = new System.Windows.Forms.Panel();
            this.keySelectionGroupBox = new System.Windows.Forms.GroupBox();
            this.modifiersGroupBox = new System.Windows.Forms.GroupBox();
            this.winCheckBox = new System.Windows.Forms.CheckBox();
            this.altCheckBox = new System.Windows.Forms.CheckBox();
            this.ctrlCheckBox = new System.Windows.Forms.CheckBox();
            this.shiftCheckBox = new System.Windows.Forms.CheckBox();
            this.virtualKeyComboBox = new System.Windows.Forms.ComboBox();
            this.virtualKeyLabel = new System.Windows.Forms.Label();
            this.commandExecutionPanel = new System.Windows.Forms.Panel();
            this.commandExecutionGroupBox = new System.Windows.Forms.GroupBox();
            this.waitForExitCheckBox = new System.Windows.Forms.CheckBox();
            this.runHiddenCheckBox = new System.Windows.Forms.CheckBox();
            this.shellTypeComboBox = new System.Windows.Forms.ComboBox();
            this.shellTypeLabel = new System.Windows.Forms.Label();
            this.commandTextBox = new System.Windows.Forms.TextBox();
            this.commandLabel = new System.Windows.Forms.Label();
            this.noteOnOnlyGroupBox = new System.Windows.Forms.GroupBox();
            this.autoReleaseNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.autoReleaseCheckBox = new System.Windows.Forms.CheckBox();
            this.ignoreNoteOffCheckBox = new System.Windows.Forms.CheckBox();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiTriggerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).BeginInit();
            this.actionTypeGroupBox.SuspendLayout();
            this.keySelectionPanel.SuspendLayout();
            this.keySelectionGroupBox.SuspendLayout();
            this.modifiersGroupBox.SuspendLayout();
            this.commandExecutionPanel.SuspendLayout();
            this.commandExecutionGroupBox.SuspendLayout();
            this.noteOnOnlyGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoReleaseNumericUpDown)).BeginInit();
            this.descriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // midiTriggerGroupBox
            //
            this.midiTriggerGroupBox.Controls.Add(this.listenButton);
            this.midiTriggerGroupBox.Controls.Add(this.noteNameLabel);
            this.midiTriggerGroupBox.Controls.Add(this.noteNameLabelLabel);
            this.midiTriggerGroupBox.Controls.Add(this.midiNoteNumericUpDown);
            this.midiTriggerGroupBox.Controls.Add(this.midiNoteLabel);
            this.midiTriggerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.midiTriggerGroupBox.Name = "midiTriggerGroupBox";
            this.midiTriggerGroupBox.Size = new System.Drawing.Size(460, 60);
            this.midiTriggerGroupBox.TabIndex = 0;
            this.midiTriggerGroupBox.TabStop = false;
            this.midiTriggerGroupBox.Text = "MIDI Trigger";
            //
            // listenButton
            //
            this.listenButton.Location = new System.Drawing.Point(350, 23);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(90, 23);
            this.listenButton.TabIndex = 4;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.listenButton_Click);
            //
            // noteNameLabel
            //
            this.noteNameLabel.AutoSize = true;
            this.noteNameLabel.Location = new System.Drawing.Point(300, 25);
            this.noteNameLabel.Name = "noteNameLabel";
            this.noteNameLabel.Size = new System.Drawing.Size(21, 15);
            this.noteNameLabel.TabIndex = 3;
            this.noteNameLabel.Text = "C4";
            //
            // noteNameLabelLabel
            //
            this.noteNameLabelLabel.AutoSize = true;
            this.noteNameLabelLabel.Location = new System.Drawing.Point(230, 25);
            this.noteNameLabelLabel.Name = "noteNameLabelLabel";
            this.noteNameLabelLabel.Size = new System.Drawing.Size(73, 15);
            this.noteNameLabelLabel.TabIndex = 2;
            this.noteNameLabelLabel.Text = "Note Name:";
            //
            // midiNoteNumericUpDown
            //
            this.midiNoteNumericUpDown.Location = new System.Drawing.Point(100, 23);
            this.midiNoteNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.midiNoteNumericUpDown.Name = "midiNoteNumericUpDown";
            this.midiNoteNumericUpDown.Size = new System.Drawing.Size(80, 23);
            this.midiNoteNumericUpDown.TabIndex = 1;
            this.midiNoteNumericUpDown.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            this.midiNoteNumericUpDown.ValueChanged += new System.EventHandler(this.midiNoteNumericUpDown_ValueChanged);
            //
            // midiNoteLabel
            //
            this.midiNoteLabel.AutoSize = true;
            this.midiNoteLabel.Location = new System.Drawing.Point(20, 25);
            this.midiNoteLabel.Name = "midiNoteLabel";
            this.midiNoteLabel.Size = new System.Drawing.Size(65, 15);
            this.midiNoteLabel.TabIndex = 0;
            this.midiNoteLabel.Text = "MIDI Note:";
            //
            // actionTypeGroupBox
            //
            this.actionTypeGroupBox.Controls.Add(this.actionTypeComboBox);
            this.actionTypeGroupBox.Location = new System.Drawing.Point(12, 78);
            this.actionTypeGroupBox.Name = "actionTypeGroupBox";
            this.actionTypeGroupBox.Size = new System.Drawing.Size(460, 60);
            this.actionTypeGroupBox.TabIndex = 1;
            this.actionTypeGroupBox.TabStop = false;
            this.actionTypeGroupBox.Text = "Action Type";
            //
            // actionTypeComboBox
            //
            this.actionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionTypeComboBox.FormattingEnabled = true;
            this.actionTypeComboBox.Location = new System.Drawing.Point(20, 22);
            this.actionTypeComboBox.Name = "actionTypeComboBox";
            this.actionTypeComboBox.Size = new System.Drawing.Size(420, 23);
            this.actionTypeComboBox.TabIndex = 0;
            //
            // keySelectionPanel
            //
            this.keySelectionPanel.Controls.Add(this.keySelectionGroupBox);
            this.keySelectionPanel.Location = new System.Drawing.Point(12, 144);
            this.keySelectionPanel.Name = "keySelectionPanel";
            this.keySelectionPanel.Size = new System.Drawing.Size(460, 140);
            this.keySelectionPanel.TabIndex = 2;
            //
            // keySelectionGroupBox
            //
            this.keySelectionGroupBox.Controls.Add(this.modifiersGroupBox);
            this.keySelectionGroupBox.Controls.Add(this.virtualKeyComboBox);
            this.keySelectionGroupBox.Controls.Add(this.virtualKeyLabel);
            this.keySelectionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.keySelectionGroupBox.Location = new System.Drawing.Point(0, 0);
            this.keySelectionGroupBox.Name = "keySelectionGroupBox";
            this.keySelectionGroupBox.Size = new System.Drawing.Size(460, 140);
            this.keySelectionGroupBox.TabIndex = 0;
            this.keySelectionGroupBox.TabStop = false;
            this.keySelectionGroupBox.Text = "Key Selection";
            //
            // modifiersGroupBox
            //
            this.modifiersGroupBox.Controls.Add(this.winCheckBox);
            this.modifiersGroupBox.Controls.Add(this.altCheckBox);
            this.modifiersGroupBox.Controls.Add(this.ctrlCheckBox);
            this.modifiersGroupBox.Controls.Add(this.shiftCheckBox);
            this.modifiersGroupBox.Location = new System.Drawing.Point(20, 60);
            this.modifiersGroupBox.Name = "modifiersGroupBox";
            this.modifiersGroupBox.Size = new System.Drawing.Size(420, 60);
            this.modifiersGroupBox.TabIndex = 2;
            this.modifiersGroupBox.TabStop = false;
            this.modifiersGroupBox.Text = "Modifier Keys";
            //
            // winCheckBox
            //
            this.winCheckBox.AutoSize = true;
            this.winCheckBox.Location = new System.Drawing.Point(300, 25);
            this.winCheckBox.Name = "winCheckBox";
            this.winCheckBox.Size = new System.Drawing.Size(47, 19);
            this.winCheckBox.TabIndex = 3;
            this.winCheckBox.Text = "Win";
            this.winCheckBox.UseVisualStyleBackColor = true;
            //
            // altCheckBox
            //
            this.altCheckBox.AutoSize = true;
            this.altCheckBox.Location = new System.Drawing.Point(200, 25);
            this.altCheckBox.Name = "altCheckBox";
            this.altCheckBox.Size = new System.Drawing.Size(41, 19);
            this.altCheckBox.TabIndex = 2;
            this.altCheckBox.Text = "Alt";
            this.altCheckBox.UseVisualStyleBackColor = true;
            //
            // ctrlCheckBox
            //
            this.ctrlCheckBox.AutoSize = true;
            this.ctrlCheckBox.Location = new System.Drawing.Point(100, 25);
            this.ctrlCheckBox.Name = "ctrlCheckBox";
            this.ctrlCheckBox.Size = new System.Drawing.Size(45, 19);
            this.ctrlCheckBox.TabIndex = 1;
            this.ctrlCheckBox.Text = "Ctrl";
            this.ctrlCheckBox.UseVisualStyleBackColor = true;
            //
            // shiftCheckBox
            //
            this.shiftCheckBox.AutoSize = true;
            this.shiftCheckBox.Location = new System.Drawing.Point(20, 25);
            this.shiftCheckBox.Name = "shiftCheckBox";
            this.shiftCheckBox.Size = new System.Drawing.Size(51, 19);
            this.shiftCheckBox.TabIndex = 0;
            this.shiftCheckBox.Text = "Shift";
            this.shiftCheckBox.UseVisualStyleBackColor = true;
            //
            // virtualKeyComboBox
            //
            this.virtualKeyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.virtualKeyComboBox.FormattingEnabled = true;
            this.virtualKeyComboBox.Location = new System.Drawing.Point(100, 25);
            this.virtualKeyComboBox.Name = "virtualKeyComboBox";
            this.virtualKeyComboBox.Size = new System.Drawing.Size(340, 23);
            this.virtualKeyComboBox.TabIndex = 1;
            //
            // virtualKeyLabel
            //
            this.virtualKeyLabel.AutoSize = true;
            this.virtualKeyLabel.Location = new System.Drawing.Point(20, 28);
            this.virtualKeyLabel.Name = "virtualKeyLabel";
            this.virtualKeyLabel.Size = new System.Drawing.Size(67, 15);
            this.virtualKeyLabel.TabIndex = 0;
            this.virtualKeyLabel.Text = "Virtual Key:";
            //
            // commandExecutionPanel
            //
            this.commandExecutionPanel.Controls.Add(this.commandExecutionGroupBox);
            this.commandExecutionPanel.Location = new System.Drawing.Point(12, 144);
            this.commandExecutionPanel.Name = "commandExecutionPanel";
            this.commandExecutionPanel.Size = new System.Drawing.Size(460, 140);
            this.commandExecutionPanel.TabIndex = 3;
            this.commandExecutionPanel.Visible = false;
            //
            // commandExecutionGroupBox
            //
            this.commandExecutionGroupBox.Controls.Add(this.waitForExitCheckBox);
            this.commandExecutionGroupBox.Controls.Add(this.runHiddenCheckBox);
            this.commandExecutionGroupBox.Controls.Add(this.shellTypeComboBox);
            this.commandExecutionGroupBox.Controls.Add(this.shellTypeLabel);
            this.commandExecutionGroupBox.Controls.Add(this.commandTextBox);
            this.commandExecutionGroupBox.Controls.Add(this.commandLabel);
            this.commandExecutionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.commandExecutionGroupBox.Location = new System.Drawing.Point(0, 0);
            this.commandExecutionGroupBox.Name = "commandExecutionGroupBox";
            this.commandExecutionGroupBox.Size = new System.Drawing.Size(460, 140);
            this.commandExecutionGroupBox.TabIndex = 0;
            this.commandExecutionGroupBox.TabStop = false;
            this.commandExecutionGroupBox.Text = "Command Execution";
            //
            // waitForExitCheckBox
            //
            this.waitForExitCheckBox.AutoSize = true;
            this.waitForExitCheckBox.Location = new System.Drawing.Point(200, 110);
            this.waitForExitCheckBox.Name = "waitForExitCheckBox";
            this.waitForExitCheckBox.Size = new System.Drawing.Size(91, 19);
            this.waitForExitCheckBox.TabIndex = 5;
            this.waitForExitCheckBox.Text = "Wait for Exit";
            this.waitForExitCheckBox.UseVisualStyleBackColor = true;
            //
            // runHiddenCheckBox
            //
            this.runHiddenCheckBox.AutoSize = true;
            this.runHiddenCheckBox.Location = new System.Drawing.Point(100, 110);
            this.runHiddenCheckBox.Name = "runHiddenCheckBox";
            this.runHiddenCheckBox.Size = new System.Drawing.Size(90, 19);
            this.runHiddenCheckBox.TabIndex = 4;
            this.runHiddenCheckBox.Text = "Run Hidden";
            this.runHiddenCheckBox.UseVisualStyleBackColor = true;
            //
            // shellTypeComboBox
            //
            this.shellTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.shellTypeComboBox.FormattingEnabled = true;
            this.shellTypeComboBox.Location = new System.Drawing.Point(100, 80);
            this.shellTypeComboBox.Name = "shellTypeComboBox";
            this.shellTypeComboBox.Size = new System.Drawing.Size(340, 23);
            this.shellTypeComboBox.TabIndex = 3;
            //
            // shellTypeLabel
            //
            this.shellTypeLabel.AutoSize = true;
            this.shellTypeLabel.Location = new System.Drawing.Point(20, 83);
            this.shellTypeLabel.Name = "shellTypeLabel";
            this.shellTypeLabel.Size = new System.Drawing.Size(64, 15);
            this.shellTypeLabel.TabIndex = 2;
            this.shellTypeLabel.Text = "Shell Type:";
            //
            // commandTextBox
            //
            this.commandTextBox.Location = new System.Drawing.Point(100, 25);
            this.commandTextBox.Multiline = true;
            this.commandTextBox.Name = "commandTextBox";
            this.commandTextBox.Size = new System.Drawing.Size(340, 50);
            this.commandTextBox.TabIndex = 1;
            //
            // commandLabel
            //
            this.commandLabel.AutoSize = true;
            this.commandLabel.Location = new System.Drawing.Point(20, 28);
            this.commandLabel.Name = "commandLabel";
            this.commandLabel.Size = new System.Drawing.Size(67, 15);
            this.commandLabel.TabIndex = 0;
            this.commandLabel.Text = "Command:";
            //
            // noteOnOnlyGroupBox
            //
            this.noteOnOnlyGroupBox.Controls.Add(this.autoReleaseNumericUpDown);
            this.noteOnOnlyGroupBox.Controls.Add(this.autoReleaseCheckBox);
            this.noteOnOnlyGroupBox.Controls.Add(this.ignoreNoteOffCheckBox);
            this.noteOnOnlyGroupBox.Location = new System.Drawing.Point(12, 290);
            this.noteOnOnlyGroupBox.Name = "noteOnOnlyGroupBox";
            this.noteOnOnlyGroupBox.Size = new System.Drawing.Size(460, 60);
            this.noteOnOnlyGroupBox.TabIndex = 4;
            this.noteOnOnlyGroupBox.TabStop = false;
            this.noteOnOnlyGroupBox.Text = "Note-On Only Mode";
            //
            // autoReleaseNumericUpDown
            //
            this.autoReleaseNumericUpDown.Enabled = false;
            this.autoReleaseNumericUpDown.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.autoReleaseNumericUpDown.Location = new System.Drawing.Point(350, 24);
            this.autoReleaseNumericUpDown.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.autoReleaseNumericUpDown.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.autoReleaseNumericUpDown.Name = "autoReleaseNumericUpDown";
            this.autoReleaseNumericUpDown.Size = new System.Drawing.Size(90, 23);
            this.autoReleaseNumericUpDown.TabIndex = 2;
            this.autoReleaseNumericUpDown.Value = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            //
            // autoReleaseCheckBox
            //
            this.autoReleaseCheckBox.AutoSize = true;
            this.autoReleaseCheckBox.Enabled = false;
            this.autoReleaseCheckBox.Location = new System.Drawing.Point(200, 25);
            this.autoReleaseCheckBox.Name = "autoReleaseCheckBox";
            this.autoReleaseCheckBox.Size = new System.Drawing.Size(144, 19);
            this.autoReleaseCheckBox.TabIndex = 1;
            this.autoReleaseCheckBox.Text = "Auto-Release After ms";
            this.autoReleaseCheckBox.UseVisualStyleBackColor = true;
            //
            // ignoreNoteOffCheckBox
            //
            this.ignoreNoteOffCheckBox.AutoSize = true;
            this.ignoreNoteOffCheckBox.Location = new System.Drawing.Point(20, 25);
            this.ignoreNoteOffCheckBox.Name = "ignoreNoteOffCheckBox";
            this.ignoreNoteOffCheckBox.Size = new System.Drawing.Size(124, 19);
            this.ignoreNoteOffCheckBox.TabIndex = 0;
            this.ignoreNoteOffCheckBox.Text = "Ignore Note-Off";
            this.ignoreNoteOffCheckBox.UseVisualStyleBackColor = true;
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 356);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(460, 60);
            this.descriptionGroupBox.TabIndex = 5;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(20, 22);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(420, 23);
            this.descriptionTextBox.TabIndex = 0;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 422);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 30);
            this.cancelButton.TabIndex = 8;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(316, 422);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 30);
            this.okButton.TabIndex = 7;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 422);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 30);
            this.testButton.TabIndex = 6;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // KeyMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.noteOnOnlyGroupBox);
            this.Controls.Add(this.commandExecutionPanel);
            this.Controls.Add(this.keySelectionPanel);
            this.Controls.Add(this.actionTypeGroupBox);
            this.Controls.Add(this.midiTriggerGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "KeyMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Key Mapping";
            this.midiTriggerGroupBox.ResumeLayout(false);
            this.midiTriggerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).EndInit();
            this.actionTypeGroupBox.ResumeLayout(false);
            this.keySelectionPanel.ResumeLayout(false);
            this.keySelectionGroupBox.ResumeLayout(false);
            this.keySelectionGroupBox.PerformLayout();
            this.modifiersGroupBox.ResumeLayout(false);
            this.modifiersGroupBox.PerformLayout();
            this.commandExecutionPanel.ResumeLayout(false);
            this.commandExecutionGroupBox.ResumeLayout(false);
            this.commandExecutionGroupBox.PerformLayout();
            this.noteOnOnlyGroupBox.ResumeLayout(false);
            this.noteOnOnlyGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.autoReleaseNumericUpDown)).EndInit();
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox midiTriggerGroupBox;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.Label noteNameLabel;
        private System.Windows.Forms.Label noteNameLabelLabel;
        private System.Windows.Forms.NumericUpDown midiNoteNumericUpDown;
        private System.Windows.Forms.Label midiNoteLabel;
        private System.Windows.Forms.GroupBox actionTypeGroupBox;
        private System.Windows.Forms.ComboBox actionTypeComboBox;
        private System.Windows.Forms.Panel keySelectionPanel;
        private System.Windows.Forms.GroupBox keySelectionGroupBox;
        private System.Windows.Forms.GroupBox modifiersGroupBox;
        private System.Windows.Forms.CheckBox winCheckBox;
        private System.Windows.Forms.CheckBox altCheckBox;
        private System.Windows.Forms.CheckBox ctrlCheckBox;
        private System.Windows.Forms.CheckBox shiftCheckBox;
        private System.Windows.Forms.ComboBox virtualKeyComboBox;
        private System.Windows.Forms.Label virtualKeyLabel;
        private System.Windows.Forms.Panel commandExecutionPanel;
        private System.Windows.Forms.GroupBox commandExecutionGroupBox;
        private System.Windows.Forms.CheckBox waitForExitCheckBox;
        private System.Windows.Forms.CheckBox runHiddenCheckBox;
        private System.Windows.Forms.ComboBox shellTypeComboBox;
        private System.Windows.Forms.Label shellTypeLabel;
        private System.Windows.Forms.TextBox commandTextBox;
        private System.Windows.Forms.Label commandLabel;
        private System.Windows.Forms.GroupBox noteOnOnlyGroupBox;
        private System.Windows.Forms.NumericUpDown autoReleaseNumericUpDown;
        private System.Windows.Forms.CheckBox autoReleaseCheckBox;
        private System.Windows.Forms.CheckBox ignoreNoteOffCheckBox;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
    }
}

