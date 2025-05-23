namespace MIDIFlux.GUI.Dialogs
{
    partial class GameControllerAxisMappingDialog
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
            this.listenButton = new System.Windows.Forms.Button();
            this.controlNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.controlNumberLabel = new System.Windows.Forms.Label();
            this.axisGroupBox = new System.Windows.Forms.GroupBox();
            this.controllerIndexNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.controllerIndexLabel = new System.Windows.Forms.Label();
            this.axisComboBox = new System.Windows.Forms.ComboBox();
            this.axisLabel = new System.Windows.Forms.Label();
            this.valueRangeGroupBox = new System.Windows.Forms.GroupBox();
            this.invertCheckBox = new System.Windows.Forms.CheckBox();
            this.maxValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxValueLabel = new System.Windows.Forms.Label();
            this.minValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.minValueLabel = new System.Windows.Forms.Label();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiControlGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).BeginInit();
            this.axisGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).BeginInit();
            this.valueRangeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).BeginInit();
            this.descriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            //
            // midiControlGroupBox
            //
            this.midiControlGroupBox.Controls.Add(this.listenButton);
            this.midiControlGroupBox.Controls.Add(this.controlNumberNumericUpDown);
            this.midiControlGroupBox.Controls.Add(this.controlNumberLabel);
            this.midiControlGroupBox.Location = new System.Drawing.Point(12, 12);
            this.midiControlGroupBox.Name = "midiControlGroupBox";
            this.midiControlGroupBox.Size = new System.Drawing.Size(360, 60);
            this.midiControlGroupBox.TabIndex = 0;
            this.midiControlGroupBox.TabStop = false;
            this.midiControlGroupBox.Text = "MIDI Control";
            //
            // controlNumberNumericUpDown
            //
            this.controlNumberNumericUpDown.Location = new System.Drawing.Point(124, 22);
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
            this.listenButton.Location = new System.Drawing.Point(190, 22);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(60, 23);
            this.listenButton.TabIndex = 2;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            //
            // controlNumberLabel
            //
            this.controlNumberLabel.AutoSize = true;
            this.controlNumberLabel.Location = new System.Drawing.Point(20, 24);
            this.controlNumberLabel.Name = "controlNumberLabel";
            this.controlNumberLabel.Size = new System.Drawing.Size(98, 15);
            this.controlNumberLabel.TabIndex = 0;
            this.controlNumberLabel.Text = "Control Number:";
            //
            // axisGroupBox
            //
            this.axisGroupBox.Controls.Add(this.controllerIndexNumericUpDown);
            this.axisGroupBox.Controls.Add(this.controllerIndexLabel);
            this.axisGroupBox.Controls.Add(this.axisComboBox);
            this.axisGroupBox.Controls.Add(this.axisLabel);
            this.axisGroupBox.Location = new System.Drawing.Point(12, 78);
            this.axisGroupBox.Name = "axisGroupBox";
            this.axisGroupBox.Size = new System.Drawing.Size(360, 90);
            this.axisGroupBox.TabIndex = 1;
            this.axisGroupBox.TabStop = false;
            this.axisGroupBox.Text = "Axis";
            //
            // controllerIndexNumericUpDown
            //
            this.controllerIndexNumericUpDown.Location = new System.Drawing.Point(124, 51);
            this.controllerIndexNumericUpDown.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.controllerIndexNumericUpDown.Name = "controllerIndexNumericUpDown";
            this.controllerIndexNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.controllerIndexNumericUpDown.TabIndex = 3;
            //
            // controllerIndexLabel
            //
            this.controllerIndexLabel.AutoSize = true;
            this.controllerIndexLabel.Location = new System.Drawing.Point(20, 53);
            this.controllerIndexLabel.Name = "controllerIndexLabel";
            this.controllerIndexLabel.Size = new System.Drawing.Size(98, 15);
            this.controllerIndexLabel.TabIndex = 2;
            this.controllerIndexLabel.Text = "Controller Index:";
            //
            // axisComboBox
            //
            this.axisComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.axisComboBox.FormattingEnabled = true;
            this.axisComboBox.Location = new System.Drawing.Point(124, 22);
            this.axisComboBox.Name = "axisComboBox";
            this.axisComboBox.Size = new System.Drawing.Size(200, 23);
            this.axisComboBox.TabIndex = 1;
            //
            // axisLabel
            //
            this.axisLabel.AutoSize = true;
            this.axisLabel.Location = new System.Drawing.Point(20, 25);
            this.axisLabel.Name = "axisLabel";
            this.axisLabel.Size = new System.Drawing.Size(32, 15);
            this.axisLabel.TabIndex = 0;
            this.axisLabel.Text = "Axis:";
            //
            // valueRangeGroupBox
            //
            this.valueRangeGroupBox.Controls.Add(this.invertCheckBox);
            this.valueRangeGroupBox.Controls.Add(this.maxValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.maxValueLabel);
            this.valueRangeGroupBox.Controls.Add(this.minValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.minValueLabel);
            this.valueRangeGroupBox.Location = new System.Drawing.Point(12, 174);
            this.valueRangeGroupBox.Name = "valueRangeGroupBox";
            this.valueRangeGroupBox.Size = new System.Drawing.Size(360, 90);
            this.valueRangeGroupBox.TabIndex = 2;
            this.valueRangeGroupBox.TabStop = false;
            this.valueRangeGroupBox.Text = "Value Range";
            //
            // invertCheckBox
            //
            this.invertCheckBox.AutoSize = true;
            this.invertCheckBox.Location = new System.Drawing.Point(124, 54);
            this.invertCheckBox.Name = "invertCheckBox";
            this.invertCheckBox.Size = new System.Drawing.Size(57, 19);
            this.invertCheckBox.TabIndex = 4;
            this.invertCheckBox.Text = "Invert";
            this.invertCheckBox.UseVisualStyleBackColor = true;
            //
            // maxValueNumericUpDown
            //
            this.maxValueNumericUpDown.Location = new System.Drawing.Point(264, 22);
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
            this.maxValueLabel.Location = new System.Drawing.Point(190, 24);
            this.maxValueLabel.Name = "maxValueLabel";
            this.maxValueLabel.Size = new System.Drawing.Size(68, 15);
            this.maxValueLabel.TabIndex = 2;
            this.maxValueLabel.Text = "Max Value:";
            //
            // minValueNumericUpDown
            //
            this.minValueNumericUpDown.Location = new System.Drawing.Point(124, 22);
            this.minValueNumericUpDown.Maximum = new decimal(new int[] {
            126,
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
            this.minValueLabel.Size = new System.Drawing.Size(66, 15);
            this.minValueLabel.TabIndex = 0;
            this.minValueLabel.Text = "Min Value:";
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 270);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(360, 80);
            this.descriptionGroupBox.TabIndex = 3;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Location = new System.Drawing.Point(6, 22);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(348, 52);
            this.descriptionTextBox.TabIndex = 0;
            //
            // cancelButton
            //
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(297, 356);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(216, 356);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 5;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 356);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 4;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // GameControllerAxisMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(384, 391);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.valueRangeGroupBox);
            this.Controls.Add(this.axisGroupBox);
            this.Controls.Add(this.midiControlGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameControllerAxisMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Axis Mapping";
            this.midiControlGroupBox.ResumeLayout(false);
            this.midiControlGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controlNumberNumericUpDown)).EndInit();
            this.axisGroupBox.ResumeLayout(false);
            this.axisGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).EndInit();
            this.valueRangeGroupBox.ResumeLayout(false);
            this.valueRangeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).EndInit();
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox midiControlGroupBox;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.NumericUpDown controlNumberNumericUpDown;
        private System.Windows.Forms.Label controlNumberLabel;
        private System.Windows.Forms.GroupBox axisGroupBox;
        private System.Windows.Forms.NumericUpDown controllerIndexNumericUpDown;
        private System.Windows.Forms.Label controllerIndexLabel;
        private System.Windows.Forms.ComboBox axisComboBox;
        private System.Windows.Forms.Label axisLabel;
        private System.Windows.Forms.GroupBox valueRangeGroupBox;
        private System.Windows.Forms.CheckBox invertCheckBox;
        private System.Windows.Forms.NumericUpDown maxValueNumericUpDown;
        private System.Windows.Forms.Label maxValueLabel;
        private System.Windows.Forms.NumericUpDown minValueNumericUpDown;
        private System.Windows.Forms.Label minValueLabel;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
    }
}
