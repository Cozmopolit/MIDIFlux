namespace MIDIFlux.GUI.Dialogs
{
    partial class AbsoluteControlMappingDialog
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
            this.handlerTypeGroupBox = new System.Windows.Forms.GroupBox();
            this.handlerDescriptionLabel = new System.Windows.Forms.Label();
            this.handlerTypeComboBox = new System.Windows.Forms.ComboBox();
            this.handlerTypeLabel = new System.Windows.Forms.Label();
            this.valueRangeGroupBox = new System.Windows.Forms.GroupBox();
            this.invertCheckBox = new System.Windows.Forms.CheckBox();
            this.maxValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxValueLabel = new System.Windows.Forms.Label();
            this.minValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.minValueLabel = new System.Windows.Forms.Label();
            this.parametersGroupBox = new System.Windows.Forms.GroupBox();
            this.parametersPanel = new System.Windows.Forms.Panel();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiControlGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).BeginInit();
            this.handlerTypeGroupBox.SuspendLayout();
            this.valueRangeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).BeginInit();
            this.parametersGroupBox.SuspendLayout();
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
            this.midiControlGroupBox.Size = new System.Drawing.Size(460, 80);
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
            // handlerTypeGroupBox
            //
            this.handlerTypeGroupBox.Controls.Add(this.handlerDescriptionLabel);
            this.handlerTypeGroupBox.Controls.Add(this.handlerTypeComboBox);
            this.handlerTypeGroupBox.Controls.Add(this.handlerTypeLabel);
            this.handlerTypeGroupBox.Location = new System.Drawing.Point(12, 98);
            this.handlerTypeGroupBox.Name = "handlerTypeGroupBox";
            this.handlerTypeGroupBox.Size = new System.Drawing.Size(460, 80);
            this.handlerTypeGroupBox.TabIndex = 1;
            this.handlerTypeGroupBox.TabStop = false;
            this.handlerTypeGroupBox.Text = "Handler Type";
            //
            // handlerDescriptionLabel
            //
            this.handlerDescriptionLabel.AutoSize = true;
            this.handlerDescriptionLabel.Location = new System.Drawing.Point(20, 51);
            this.handlerDescriptionLabel.Name = "handlerDescriptionLabel";
            this.handlerDescriptionLabel.Size = new System.Drawing.Size(127, 15);
            this.handlerDescriptionLabel.TabIndex = 2;
            this.handlerDescriptionLabel.Text = "Controls system volume";
            //
            // handlerTypeComboBox
            //
            this.handlerTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.handlerTypeComboBox.FormattingEnabled = true;
            this.handlerTypeComboBox.Items.AddRange(new object[] {
            "SystemVolume",
            "GameControllerAxis",
            "CCRange"});
            this.handlerTypeComboBox.Location = new System.Drawing.Point(120, 22);
            this.handlerTypeComboBox.Name = "handlerTypeComboBox";
            this.handlerTypeComboBox.Size = new System.Drawing.Size(325, 23);
            this.handlerTypeComboBox.TabIndex = 1;
            //
            // handlerTypeLabel
            //
            this.handlerTypeLabel.AutoSize = true;
            this.handlerTypeLabel.Location = new System.Drawing.Point(20, 25);
            this.handlerTypeLabel.Name = "handlerTypeLabel";
            this.handlerTypeLabel.Size = new System.Drawing.Size(80, 15);
            this.handlerTypeLabel.TabIndex = 0;
            this.handlerTypeLabel.Text = "Handler Type:";
            //
            // valueRangeGroupBox
            //
            this.valueRangeGroupBox.Controls.Add(this.invertCheckBox);
            this.valueRangeGroupBox.Controls.Add(this.maxValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.maxValueLabel);
            this.valueRangeGroupBox.Controls.Add(this.minValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.minValueLabel);
            this.valueRangeGroupBox.Location = new System.Drawing.Point(12, 184);
            this.valueRangeGroupBox.Name = "valueRangeGroupBox";
            this.valueRangeGroupBox.Size = new System.Drawing.Size(460, 80);
            this.valueRangeGroupBox.TabIndex = 2;
            this.valueRangeGroupBox.TabStop = false;
            this.valueRangeGroupBox.Text = "Value Range";
            //
            // invertCheckBox
            //
            this.invertCheckBox.AutoSize = true;
            this.invertCheckBox.Location = new System.Drawing.Point(264, 24);
            this.invertCheckBox.Name = "invertCheckBox";
            this.invertCheckBox.Size = new System.Drawing.Size(96, 19);
            this.invertCheckBox.TabIndex = 4;
            this.invertCheckBox.Text = "Invert Values";
            this.invertCheckBox.UseVisualStyleBackColor = true;
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
            // parametersGroupBox
            //
            this.parametersGroupBox.Controls.Add(this.parametersPanel);
            this.parametersGroupBox.Location = new System.Drawing.Point(12, 270);
            this.parametersGroupBox.Name = "parametersGroupBox";
            this.parametersGroupBox.Size = new System.Drawing.Size(460, 80);
            this.parametersGroupBox.TabIndex = 3;
            this.parametersGroupBox.TabStop = false;
            this.parametersGroupBox.Text = "Parameters";
            //
            // parametersPanel
            //
            this.parametersPanel.Location = new System.Drawing.Point(20, 22);
            this.parametersPanel.Name = "parametersPanel";
            this.parametersPanel.Size = new System.Drawing.Size(425, 52);
            this.parametersPanel.TabIndex = 0;
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 356);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(460, 80);
            this.descriptionGroupBox.TabIndex = 4;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(20, 22);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(425, 52);
            this.descriptionTextBox.TabIndex = 0;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 442);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 7;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(316, 442);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 6;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 442);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 5;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // AbsoluteControlMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 477);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.parametersGroupBox);
            this.Controls.Add(this.valueRangeGroupBox);
            this.Controls.Add(this.handlerTypeGroupBox);
            this.Controls.Add(this.midiControlGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AbsoluteControlMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Absolute Control Mapping";
            this.midiControlGroupBox.ResumeLayout(false);
            this.midiControlGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).EndInit();
            this.handlerTypeGroupBox.ResumeLayout(false);
            this.handlerTypeGroupBox.PerformLayout();
            this.valueRangeGroupBox.ResumeLayout(false);
            this.valueRangeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).EndInit();
            this.parametersGroupBox.ResumeLayout(false);
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
        private System.Windows.Forms.GroupBox handlerTypeGroupBox;
        private System.Windows.Forms.Label handlerDescriptionLabel;
        private System.Windows.Forms.ComboBox handlerTypeComboBox;
        private System.Windows.Forms.Label handlerTypeLabel;
        private System.Windows.Forms.GroupBox valueRangeGroupBox;
        private System.Windows.Forms.CheckBox invertCheckBox;
        private System.Windows.Forms.NumericUpDown maxValueNumericUpDown;
        private System.Windows.Forms.Label maxValueLabel;
        private System.Windows.Forms.NumericUpDown minValueNumericUpDown;
        private System.Windows.Forms.Label minValueLabel;
        private System.Windows.Forms.GroupBox parametersGroupBox;
        private System.Windows.Forms.Panel parametersPanel;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
    }
}
