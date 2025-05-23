namespace MIDIFlux.GUI.Dialogs
{
    partial class MacroMappingDialog
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
            this.noteNameLabel = new System.Windows.Forms.Label();
            this.noteNameLabelLabel = new System.Windows.Forms.Label();
            this.listenButton = new System.Windows.Forms.Button();
            this.midiNoteNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.midiNoteLabel = new System.Windows.Forms.Label();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.deleteActionButton = new System.Windows.Forms.Button();
            this.editActionButton = new System.Windows.Forms.Button();
            this.addActionButton = new System.Windows.Forms.Button();
            this.actionsListView = new System.Windows.Forms.ListView();
            this.orderColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.typeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.delayColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.optionsGroupBox = new System.Windows.Forms.GroupBox();
            this.ignoreNoteOffCheckBox = new System.Windows.Forms.CheckBox();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiTriggerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).BeginInit();
            this.actionsGroupBox.SuspendLayout();
            this.optionsGroupBox.SuspendLayout();
            this.descriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // midiTriggerGroupBox
            //
            this.midiTriggerGroupBox.Controls.Add(this.noteNameLabel);
            this.midiTriggerGroupBox.Controls.Add(this.noteNameLabelLabel);
            this.midiTriggerGroupBox.Controls.Add(this.listenButton);
            this.midiTriggerGroupBox.Controls.Add(this.midiNoteNumericUpDown);
            this.midiTriggerGroupBox.Controls.Add(this.midiNoteLabel);
            this.midiTriggerGroupBox.Location = new System.Drawing.Point(12, 12);
            this.midiTriggerGroupBox.Name = "midiTriggerGroupBox";
            this.midiTriggerGroupBox.Size = new System.Drawing.Size(360, 80);
            this.midiTriggerGroupBox.TabIndex = 0;
            this.midiTriggerGroupBox.TabStop = false;
            this.midiTriggerGroupBox.Text = "MIDI Trigger";
            //
            // noteNameLabel
            //
            this.noteNameLabel.AutoSize = true;
            this.noteNameLabel.Location = new System.Drawing.Point(124, 51);
            this.noteNameLabel.Name = "noteNameLabel";
            this.noteNameLabel.Size = new System.Drawing.Size(24, 15);
            this.noteNameLabel.TabIndex = 3;
            this.noteNameLabel.Text = "C4";
            //
            // noteNameLabelLabel
            //
            this.noteNameLabelLabel.AutoSize = true;
            this.noteNameLabelLabel.Location = new System.Drawing.Point(20, 51);
            this.noteNameLabelLabel.Name = "noteNameLabelLabel";
            this.noteNameLabelLabel.Size = new System.Drawing.Size(73, 15);
            this.noteNameLabelLabel.TabIndex = 2;
            this.noteNameLabelLabel.Text = "Note Name:";
            //
            // midiNoteNumericUpDown
            //
            this.midiNoteNumericUpDown.Location = new System.Drawing.Point(124, 22);
            this.midiNoteNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.midiNoteNumericUpDown.Name = "midiNoteNumericUpDown";
            this.midiNoteNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.midiNoteNumericUpDown.TabIndex = 1;
            this.midiNoteNumericUpDown.Value = new decimal(new int[] {
            60,
            0,
            0,
            0});
            //
            // listenButton
            //
            this.listenButton.Location = new System.Drawing.Point(190, 22);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(60, 23);
            this.listenButton.TabIndex = 4;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            //
            // midiNoteLabel
            //
            this.midiNoteLabel.AutoSize = true;
            this.midiNoteLabel.Location = new System.Drawing.Point(20, 24);
            this.midiNoteLabel.Name = "midiNoteLabel";
            this.midiNoteLabel.Size = new System.Drawing.Size(67, 15);
            this.midiNoteLabel.TabIndex = 0;
            this.midiNoteLabel.Text = "MIDI Note:";
            //
            // actionsGroupBox
            //
            this.actionsGroupBox.Controls.Add(this.moveDownButton);
            this.actionsGroupBox.Controls.Add(this.moveUpButton);
            this.actionsGroupBox.Controls.Add(this.deleteActionButton);
            this.actionsGroupBox.Controls.Add(this.editActionButton);
            this.actionsGroupBox.Controls.Add(this.addActionButton);
            this.actionsGroupBox.Controls.Add(this.actionsListView);
            this.actionsGroupBox.Location = new System.Drawing.Point(12, 158);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(560, 250);
            this.actionsGroupBox.TabIndex = 2;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions";
            //
            // moveDownButton
            //
            this.moveDownButton.Enabled = false;
            this.moveDownButton.Location = new System.Drawing.Point(479, 221);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(75, 23);
            this.moveDownButton.TabIndex = 5;
            this.moveDownButton.Text = "Move Down";
            this.moveDownButton.UseVisualStyleBackColor = true;
            //
            // moveUpButton
            //
            this.moveUpButton.Enabled = false;
            this.moveUpButton.Location = new System.Drawing.Point(398, 221);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(75, 23);
            this.moveUpButton.TabIndex = 4;
            this.moveUpButton.Text = "Move Up";
            this.moveUpButton.UseVisualStyleBackColor = true;
            //
            // deleteActionButton
            //
            this.deleteActionButton.Enabled = false;
            this.deleteActionButton.Location = new System.Drawing.Point(167, 221);
            this.deleteActionButton.Name = "deleteActionButton";
            this.deleteActionButton.Size = new System.Drawing.Size(75, 23);
            this.deleteActionButton.TabIndex = 3;
            this.deleteActionButton.Text = "Delete";
            this.deleteActionButton.UseVisualStyleBackColor = true;
            //
            // editActionButton
            //
            this.editActionButton.Enabled = false;
            this.editActionButton.Location = new System.Drawing.Point(86, 221);
            this.editActionButton.Name = "editActionButton";
            this.editActionButton.Size = new System.Drawing.Size(75, 23);
            this.editActionButton.TabIndex = 2;
            this.editActionButton.Text = "Edit";
            this.editActionButton.UseVisualStyleBackColor = true;
            //
            // addActionButton
            //
            this.addActionButton.Location = new System.Drawing.Point(5, 221);
            this.addActionButton.Name = "addActionButton";
            this.addActionButton.Size = new System.Drawing.Size(75, 23);
            this.addActionButton.TabIndex = 1;
            this.addActionButton.Text = "Add";
            this.addActionButton.UseVisualStyleBackColor = true;
            //
            // actionsListView
            //
            this.actionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.orderColumnHeader,
            this.typeColumnHeader,
            this.descriptionColumnHeader,
            this.delayColumnHeader});
            this.actionsListView.FullRowSelect = true;
            this.actionsListView.Location = new System.Drawing.Point(6, 22);
            this.actionsListView.MultiSelect = false;
            this.actionsListView.Name = "actionsListView";
            this.actionsListView.Size = new System.Drawing.Size(548, 193);
            this.actionsListView.TabIndex = 0;
            this.actionsListView.UseCompatibleStateImageBehavior = false;
            this.actionsListView.View = System.Windows.Forms.View.Details;
            //
            // orderColumnHeader
            //
            this.orderColumnHeader.Text = "#";
            this.orderColumnHeader.Width = 30;
            //
            // typeColumnHeader
            //
            this.typeColumnHeader.Text = "Type";
            this.typeColumnHeader.Width = 120;
            //
            // descriptionColumnHeader
            //
            this.descriptionColumnHeader.Text = "Description";
            this.descriptionColumnHeader.Width = 300;
            //
            // delayColumnHeader
            //
            this.delayColumnHeader.Text = "Delay";
            this.delayColumnHeader.Width = 80;
            //
            // optionsGroupBox
            //
            this.optionsGroupBox.Controls.Add(this.ignoreNoteOffCheckBox);
            this.optionsGroupBox.Location = new System.Drawing.Point(378, 12);
            this.optionsGroupBox.Name = "optionsGroupBox";
            this.optionsGroupBox.Size = new System.Drawing.Size(194, 80);
            this.optionsGroupBox.TabIndex = 1;
            this.optionsGroupBox.TabStop = false;
            this.optionsGroupBox.Text = "Options";
            //
            // ignoreNoteOffCheckBox
            //
            this.ignoreNoteOffCheckBox.AutoSize = true;
            this.ignoreNoteOffCheckBox.Checked = true;
            this.ignoreNoteOffCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.ignoreNoteOffCheckBox.Location = new System.Drawing.Point(20, 22);
            this.ignoreNoteOffCheckBox.Name = "ignoreNoteOffCheckBox";
            this.ignoreNoteOffCheckBox.Size = new System.Drawing.Size(107, 19);
            this.ignoreNoteOffCheckBox.TabIndex = 0;
            this.ignoreNoteOffCheckBox.Text = "Ignore Note Off";
            this.ignoreNoteOffCheckBox.UseVisualStyleBackColor = true;
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 98);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(560, 54);
            this.descriptionGroupBox.TabIndex = 3;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(6, 22);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(548, 23);
            this.descriptionTextBox.TabIndex = 0;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(497, 414);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(416, 414);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 414);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 4;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // MacroMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 449);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.optionsGroupBox);
            this.Controls.Add(this.actionsGroupBox);
            this.Controls.Add(this.midiTriggerGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MacroMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Macro Mapping";
            this.midiTriggerGroupBox.ResumeLayout(false);
            this.midiTriggerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).EndInit();
            this.actionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.ResumeLayout(false);
            this.optionsGroupBox.PerformLayout();
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox midiTriggerGroupBox;
        private System.Windows.Forms.Label noteNameLabel;
        private System.Windows.Forms.Label noteNameLabelLabel;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.NumericUpDown midiNoteNumericUpDown;
        private System.Windows.Forms.Label midiNoteLabel;
        private System.Windows.Forms.GroupBox actionsGroupBox;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button deleteActionButton;
        private System.Windows.Forms.Button editActionButton;
        private System.Windows.Forms.Button addActionButton;
        private System.Windows.Forms.ListView actionsListView;
        private System.Windows.Forms.ColumnHeader orderColumnHeader;
        private System.Windows.Forms.ColumnHeader typeColumnHeader;
        private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
        private System.Windows.Forms.ColumnHeader delayColumnHeader;
        private System.Windows.Forms.GroupBox optionsGroupBox;
        private System.Windows.Forms.CheckBox ignoreNoteOffCheckBox;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
    }
}
