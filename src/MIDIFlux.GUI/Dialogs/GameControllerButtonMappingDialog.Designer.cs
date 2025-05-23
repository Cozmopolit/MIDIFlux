namespace MIDIFlux.GUI.Dialogs
{
    partial class GameControllerButtonMappingDialog
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
            this.buttonGroupBox = new System.Windows.Forms.GroupBox();
            this.controllerIndexNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.controllerIndexLabel = new System.Windows.Forms.Label();
            this.buttonComboBox = new System.Windows.Forms.ComboBox();
            this.buttonLabel = new System.Windows.Forms.Label();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.testButton = new System.Windows.Forms.Button();
            this.midiTriggerGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).BeginInit();
            this.buttonGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).BeginInit();
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
            // buttonGroupBox
            //
            this.buttonGroupBox.Controls.Add(this.controllerIndexNumericUpDown);
            this.buttonGroupBox.Controls.Add(this.controllerIndexLabel);
            this.buttonGroupBox.Controls.Add(this.buttonComboBox);
            this.buttonGroupBox.Controls.Add(this.buttonLabel);
            this.buttonGroupBox.Location = new System.Drawing.Point(12, 98);
            this.buttonGroupBox.Name = "buttonGroupBox";
            this.buttonGroupBox.Size = new System.Drawing.Size(360, 90);
            this.buttonGroupBox.TabIndex = 1;
            this.buttonGroupBox.TabStop = false;
            this.buttonGroupBox.Text = "Button";
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
            // buttonComboBox
            //
            this.buttonComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.buttonComboBox.FormattingEnabled = true;
            this.buttonComboBox.Location = new System.Drawing.Point(124, 22);
            this.buttonComboBox.Name = "buttonComboBox";
            this.buttonComboBox.Size = new System.Drawing.Size(200, 23);
            this.buttonComboBox.TabIndex = 1;
            //
            // buttonLabel
            //
            this.buttonLabel.AutoSize = true;
            this.buttonLabel.Location = new System.Drawing.Point(20, 25);
            this.buttonLabel.Name = "buttonLabel";
            this.buttonLabel.Size = new System.Drawing.Size(46, 15);
            this.buttonLabel.TabIndex = 0;
            this.buttonLabel.Text = "Button:";
            //
            // descriptionGroupBox
            //
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Location = new System.Drawing.Point(12, 194);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(360, 80);
            this.descriptionGroupBox.TabIndex = 2;
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
            this.cancelButton.Location = new System.Drawing.Point(297, 280);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // okButton
            //
            this.okButton.Location = new System.Drawing.Point(216, 280);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            //
            // testButton
            //
            this.testButton.Location = new System.Drawing.Point(12, 280);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(75, 23);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // GameControllerButtonMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(384, 315);
            this.Controls.Add(this.testButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.buttonGroupBox);
            this.Controls.Add(this.midiTriggerGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameControllerButtonMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Button Mapping";
            this.midiTriggerGroupBox.ResumeLayout(false);
            this.midiTriggerGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiNoteNumericUpDown)).EndInit();
            this.buttonGroupBox.ResumeLayout(false);
            this.buttonGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).EndInit();
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
        private System.Windows.Forms.GroupBox buttonGroupBox;
        private System.Windows.Forms.NumericUpDown controllerIndexNumericUpDown;
        private System.Windows.Forms.Label controllerIndexLabel;
        private System.Windows.Forms.ComboBox buttonComboBox;
        private System.Windows.Forms.Label buttonLabel;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button testButton;
    }
}
