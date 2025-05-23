namespace MIDIFlux.GUI.Dialogs
{
    partial class CCRangeMappingDialog
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
            this.midiControlGroupBox = new System.Windows.Forms.GroupBox();
            this.selectChannelButton = new System.Windows.Forms.Button();
            this.channelTextBox = new System.Windows.Forms.TextBox();
            this.channelLabel = new System.Windows.Forms.Label();
            this.listenButton = new System.Windows.Forms.Button();
            this.controlNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.controlNumberLabel = new System.Windows.Forms.Label();
            this.rangesGroupBox = new System.Windows.Forms.GroupBox();
            this.generateButton = new System.Windows.Forms.Button();
            this.deleteRangeButton = new System.Windows.Forms.Button();
            this.editRangeButton = new System.Windows.Forms.Button();
            this.addRangeButton = new System.Windows.Forms.Button();
            this.rangesListView = new System.Windows.Forms.ListView();
            this.rangeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionDetailsColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiControlGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).BeginInit();
            this.rangesGroupBox.SuspendLayout();
            this.descriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // midiControlGroupBox
            //
            this.midiControlGroupBox.Controls.Add(this.selectChannelButton);
            this.midiControlGroupBox.Controls.Add(this.channelTextBox);
            this.midiControlGroupBox.Controls.Add(this.channelLabel);
            this.midiControlGroupBox.Controls.Add(this.listenButton);
            this.midiControlGroupBox.Controls.Add(this.controlNumberNumericUpDown);
            this.midiControlGroupBox.Controls.Add(this.controlNumberLabel);
            this.midiControlGroupBox.Location = new System.Drawing.Point(12, 12);
            this.midiControlGroupBox.Name = "midiControlGroupBox";
            this.midiControlGroupBox.Size = new System.Drawing.Size(560, 80);
            this.midiControlGroupBox.TabIndex = 0;
            this.midiControlGroupBox.TabStop = false;
            this.midiControlGroupBox.Text = "MIDI Control";
            //
            // selectChannelButton
            //
            this.selectChannelButton.Location = new System.Drawing.Point(370, 22);
            this.selectChannelButton.Name = "selectChannelButton";
            this.selectChannelButton.Size = new System.Drawing.Size(75, 23);
            this.selectChannelButton.TabIndex = 4;
            this.selectChannelButton.Text = "Select...";
            this.selectChannelButton.UseVisualStyleBackColor = true;
            //
            // channelTextBox
            //
            this.channelTextBox.Location = new System.Drawing.Point(264, 22);
            this.channelTextBox.Name = "channelTextBox";
            this.channelTextBox.ReadOnly = true;
            this.channelTextBox.Size = new System.Drawing.Size(100, 23);
            this.channelTextBox.TabIndex = 3;
            this.channelTextBox.Text = "All Channels";
            //
            // channelLabel
            //
            this.channelLabel.AutoSize = true;
            this.channelLabel.Location = new System.Drawing.Point(210, 25);
            this.channelLabel.Name = "channelLabel";
            this.channelLabel.Size = new System.Drawing.Size(54, 15);
            this.channelLabel.TabIndex = 2;
            this.channelLabel.Text = "Channel:";
            //
            // controlNumberNumericUpDown
            //
            this.controlNumberNumericUpDown.Location = new System.Drawing.Point(120, 23);
            this.controlNumberNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.controlNumberNumericUpDown.Name = "controlNumberNumericUpDown";
            this.controlNumberNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.controlNumberNumericUpDown.TabIndex = 1;
            this.controlNumberNumericUpDown.Value = new decimal(new int[] {
            7,
            0,
            0,
            0});
            //
            // listenButton
            //
            this.listenButton.Location = new System.Drawing.Point(186, 23);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(60, 23);
            this.listenButton.TabIndex = 2;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            //
            // controlNumberLabel
            //
            this.controlNumberLabel.AutoSize = true;
            this.controlNumberLabel.Location = new System.Drawing.Point(20, 25);
            this.controlNumberLabel.Name = "controlNumberLabel";
            this.controlNumberLabel.Size = new System.Drawing.Size(98, 15);
            this.controlNumberLabel.TabIndex = 0;
            this.controlNumberLabel.Text = "Control Number:";
            //
            // rangesGroupBox
            //
            this.rangesGroupBox.Controls.Add(this.generateButton);
            this.rangesGroupBox.Controls.Add(this.deleteRangeButton);
            this.rangesGroupBox.Controls.Add(this.editRangeButton);
            this.rangesGroupBox.Controls.Add(this.addRangeButton);
            this.rangesGroupBox.Controls.Add(this.rangesListView);
            this.rangesGroupBox.Location = new System.Drawing.Point(12, 98);
            this.rangesGroupBox.Name = "rangesGroupBox";
            this.rangesGroupBox.Size = new System.Drawing.Size(560, 250);
            this.rangesGroupBox.TabIndex = 1;
            this.rangesGroupBox.TabStop = false;
            this.rangesGroupBox.Text = "Value Ranges";
            //
            // generateButton
            //
            this.generateButton.Location = new System.Drawing.Point(20, 215);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(120, 23);
            this.generateButton.TabIndex = 4;
            this.generateButton.Text = "Generate Ranges...";
            this.generateButton.UseVisualStyleBackColor = true;
            //
            // deleteRangeButton
            //
            this.deleteRangeButton.Enabled = false;
            this.deleteRangeButton.Location = new System.Drawing.Point(479, 215);
            this.deleteRangeButton.Name = "deleteRangeButton";
            this.deleteRangeButton.Size = new System.Drawing.Size(75, 23);
            this.deleteRangeButton.TabIndex = 3;
            this.deleteRangeButton.Text = "Delete";
            this.deleteRangeButton.UseVisualStyleBackColor = true;
            //
            // editRangeButton
            //
            this.editRangeButton.Enabled = false;
            this.editRangeButton.Location = new System.Drawing.Point(398, 215);
            this.editRangeButton.Name = "editRangeButton";
            this.editRangeButton.Size = new System.Drawing.Size(75, 23);
            this.editRangeButton.TabIndex = 2;
            this.editRangeButton.Text = "Edit";
            this.editRangeButton.UseVisualStyleBackColor = true;
            //
            // addRangeButton
            //
            this.addRangeButton.Location = new System.Drawing.Point(317, 215);
            this.addRangeButton.Name = "addRangeButton";
            this.addRangeButton.Size = new System.Drawing.Size(75, 23);
            this.addRangeButton.TabIndex = 1;
            this.addRangeButton.Text = "Add";
            this.addRangeButton.UseVisualStyleBackColor = true;
            //
            // rangesListView
            //
            this.rangesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.rangeColumnHeader,
            this.actionTypeColumnHeader,
            this.actionDetailsColumnHeader});
            this.rangesListView.FullRowSelect = true;
            this.rangesListView.Location = new System.Drawing.Point(20, 22);
            this.rangesListView.MultiSelect = false;
            this.rangesListView.Name = "rangesListView";
            this.rangesListView.Size = new System.Drawing.Size(534, 187);
            this.rangesListView.TabIndex = 0;
            this.rangesListView.UseCompatibleStateImageBehavior = false;
            this.rangesListView.View = System.Windows.Forms.View.Details;
            //
            // rangeColumnHeader
            //
            this.rangeColumnHeader.Text = "Range";
            this.rangeColumnHeader.Width = 100;
            //
            // actionTypeColumnHeader
            //
            this.actionTypeColumnHeader.Text = "Action Type";
            this.actionTypeColumnHeader.Width = 100;
            //
            // actionDetailsColumnHeader
            //
            this.actionDetailsColumnHeader.Text = "Action Details";
            this.actionDetailsColumnHeader.Width = 330;
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 354);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(560, 80);
            this.descriptionGroupBox.TabIndex = 2;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(20, 22);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(534, 52);
            this.descriptionTextBox.TabIndex = 0;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(497, 440);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(416, 440);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 440);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // CCRangeMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 475);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.rangesGroupBox);
            this.Controls.Add(this.midiControlGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CCRangeMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "CC Range Mapping";
            this.midiControlGroupBox.ResumeLayout(false);
            this.midiControlGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).EndInit();
            this.rangesGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox midiControlGroupBox;
        private System.Windows.Forms.Button selectChannelButton;
        private System.Windows.Forms.TextBox channelTextBox;
        private System.Windows.Forms.Label channelLabel;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.NumericUpDown controlNumberNumericUpDown;
        private System.Windows.Forms.Label controlNumberLabel;
        private System.Windows.Forms.GroupBox rangesGroupBox;
        private System.Windows.Forms.Button deleteRangeButton;
        private System.Windows.Forms.Button editRangeButton;
        private System.Windows.Forms.Button addRangeButton;
        private System.Windows.Forms.ListView rangesListView;
        private System.Windows.Forms.ColumnHeader rangeColumnHeader;
        private System.Windows.Forms.ColumnHeader actionTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader actionDetailsColumnHeader;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Button generateButton;
    }
}
