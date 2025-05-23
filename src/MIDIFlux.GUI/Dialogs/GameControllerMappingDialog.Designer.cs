namespace MIDIFlux.GUI.Dialogs
{
    partial class GameControllerMappingDialog
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
            this.controllerSettingsGroupBox = new System.Windows.Forms.GroupBox();
            this.controllerIndexNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.controllerIndexLabel = new System.Windows.Forms.Label();
            this.mappingsTabControl = new System.Windows.Forms.TabControl();
            this.buttonMappingsTabPage = new System.Windows.Forms.TabPage();
            this.deleteButtonMappingButton = new System.Windows.Forms.Button();
            this.editButtonMappingButton = new System.Windows.Forms.Button();
            this.addButtonMappingButton = new System.Windows.Forms.Button();
            this.buttonMappingsListView = new System.Windows.Forms.ListView();
            this.midiNoteColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.buttonColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.buttonControllerIndexColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.axisMappingsTabPage = new System.Windows.Forms.TabPage();
            this.deleteAxisMappingButton = new System.Windows.Forms.Button();
            this.editAxisMappingButton = new System.Windows.Forms.Button();
            this.addAxisMappingButton = new System.Windows.Forms.Button();
            this.axisMappingsListView = new System.Windows.Forms.ListView();
            this.controlNumberColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.axisColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.valueRangeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.invertColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.axisControllerIndexColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.controllerSettingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).BeginInit();
            this.mappingsTabControl.SuspendLayout();
            this.buttonMappingsTabPage.SuspendLayout();
            this.axisMappingsTabPage.SuspendLayout();
            this.descriptionGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // controllerSettingsGroupBox
            // 
            this.controllerSettingsGroupBox.Controls.Add(this.controllerIndexNumericUpDown);
            this.controllerSettingsGroupBox.Controls.Add(this.controllerIndexLabel);
            this.controllerSettingsGroupBox.Location = new System.Drawing.Point(12, 12);
            this.controllerSettingsGroupBox.Name = "controllerSettingsGroupBox";
            this.controllerSettingsGroupBox.Size = new System.Drawing.Size(560, 60);
            this.controllerSettingsGroupBox.TabIndex = 0;
            this.controllerSettingsGroupBox.TabStop = false;
            this.controllerSettingsGroupBox.Text = "Controller Settings";
            // 
            // controllerIndexNumericUpDown
            // 
            this.controllerIndexNumericUpDown.Location = new System.Drawing.Point(124, 22);
            this.controllerIndexNumericUpDown.Maximum = new decimal(new int[] {
            3,
            0,
            0,
            0});
            this.controllerIndexNumericUpDown.Name = "controllerIndexNumericUpDown";
            this.controllerIndexNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.controllerIndexNumericUpDown.TabIndex = 1;
            // 
            // controllerIndexLabel
            // 
            this.controllerIndexLabel.AutoSize = true;
            this.controllerIndexLabel.Location = new System.Drawing.Point(20, 24);
            this.controllerIndexLabel.Name = "controllerIndexLabel";
            this.controllerIndexLabel.Size = new System.Drawing.Size(98, 15);
            this.controllerIndexLabel.TabIndex = 0;
            this.controllerIndexLabel.Text = "Controller Index:";
            // 
            // mappingsTabControl
            // 
            this.mappingsTabControl.Controls.Add(this.buttonMappingsTabPage);
            this.mappingsTabControl.Controls.Add(this.axisMappingsTabPage);
            this.mappingsTabControl.Location = new System.Drawing.Point(12, 78);
            this.mappingsTabControl.Name = "mappingsTabControl";
            this.mappingsTabControl.SelectedIndex = 0;
            this.mappingsTabControl.Size = new System.Drawing.Size(560, 270);
            this.mappingsTabControl.TabIndex = 1;
            // 
            // buttonMappingsTabPage
            // 
            this.buttonMappingsTabPage.Controls.Add(this.deleteButtonMappingButton);
            this.buttonMappingsTabPage.Controls.Add(this.editButtonMappingButton);
            this.buttonMappingsTabPage.Controls.Add(this.addButtonMappingButton);
            this.buttonMappingsTabPage.Controls.Add(this.buttonMappingsListView);
            this.buttonMappingsTabPage.Location = new System.Drawing.Point(4, 24);
            this.buttonMappingsTabPage.Name = "buttonMappingsTabPage";
            this.buttonMappingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.buttonMappingsTabPage.Size = new System.Drawing.Size(552, 242);
            this.buttonMappingsTabPage.TabIndex = 0;
            this.buttonMappingsTabPage.Text = "Buttons";
            this.buttonMappingsTabPage.UseVisualStyleBackColor = true;
            // 
            // deleteButtonMappingButton
            // 
            this.deleteButtonMappingButton.Enabled = false;
            this.deleteButtonMappingButton.Location = new System.Drawing.Point(167, 213);
            this.deleteButtonMappingButton.Name = "deleteButtonMappingButton";
            this.deleteButtonMappingButton.Size = new System.Drawing.Size(75, 23);
            this.deleteButtonMappingButton.TabIndex = 3;
            this.deleteButtonMappingButton.Text = "Delete";
            this.deleteButtonMappingButton.UseVisualStyleBackColor = true;
            // 
            // editButtonMappingButton
            // 
            this.editButtonMappingButton.Enabled = false;
            this.editButtonMappingButton.Location = new System.Drawing.Point(86, 213);
            this.editButtonMappingButton.Name = "editButtonMappingButton";
            this.editButtonMappingButton.Size = new System.Drawing.Size(75, 23);
            this.editButtonMappingButton.TabIndex = 2;
            this.editButtonMappingButton.Text = "Edit";
            this.editButtonMappingButton.UseVisualStyleBackColor = true;
            // 
            // addButtonMappingButton
            // 
            this.addButtonMappingButton.Location = new System.Drawing.Point(5, 213);
            this.addButtonMappingButton.Name = "addButtonMappingButton";
            this.addButtonMappingButton.Size = new System.Drawing.Size(75, 23);
            this.addButtonMappingButton.TabIndex = 1;
            this.addButtonMappingButton.Text = "Add";
            this.addButtonMappingButton.UseVisualStyleBackColor = true;
            // 
            // buttonMappingsListView
            // 
            this.buttonMappingsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.midiNoteColumnHeader,
            this.buttonColumnHeader,
            this.buttonControllerIndexColumnHeader});
            this.buttonMappingsListView.FullRowSelect = true;
            this.buttonMappingsListView.Location = new System.Drawing.Point(5, 6);
            this.buttonMappingsListView.MultiSelect = false;
            this.buttonMappingsListView.Name = "buttonMappingsListView";
            this.buttonMappingsListView.Size = new System.Drawing.Size(541, 201);
            this.buttonMappingsListView.TabIndex = 0;
            this.buttonMappingsListView.UseCompatibleStateImageBehavior = false;
            this.buttonMappingsListView.View = System.Windows.Forms.View.Details;
            // 
            // midiNoteColumnHeader
            // 
            this.midiNoteColumnHeader.Text = "MIDI Note";
            this.midiNoteColumnHeader.Width = 100;
            // 
            // buttonColumnHeader
            // 
            this.buttonColumnHeader.Text = "Button";
            this.buttonColumnHeader.Width = 150;
            // 
            // buttonControllerIndexColumnHeader
            // 
            this.buttonControllerIndexColumnHeader.Text = "Controller Index";
            this.buttonControllerIndexColumnHeader.Width = 100;
            // 
            // axisMappingsTabPage
            // 
            this.axisMappingsTabPage.Controls.Add(this.deleteAxisMappingButton);
            this.axisMappingsTabPage.Controls.Add(this.editAxisMappingButton);
            this.axisMappingsTabPage.Controls.Add(this.addAxisMappingButton);
            this.axisMappingsTabPage.Controls.Add(this.axisMappingsListView);
            this.axisMappingsTabPage.Location = new System.Drawing.Point(4, 24);
            this.axisMappingsTabPage.Name = "axisMappingsTabPage";
            this.axisMappingsTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.axisMappingsTabPage.Size = new System.Drawing.Size(552, 242);
            this.axisMappingsTabPage.TabIndex = 1;
            this.axisMappingsTabPage.Text = "Axes";
            this.axisMappingsTabPage.UseVisualStyleBackColor = true;
            // 
            // deleteAxisMappingButton
            // 
            this.deleteAxisMappingButton.Enabled = false;
            this.deleteAxisMappingButton.Location = new System.Drawing.Point(167, 213);
            this.deleteAxisMappingButton.Name = "deleteAxisMappingButton";
            this.deleteAxisMappingButton.Size = new System.Drawing.Size(75, 23);
            this.deleteAxisMappingButton.TabIndex = 3;
            this.deleteAxisMappingButton.Text = "Delete";
            this.deleteAxisMappingButton.UseVisualStyleBackColor = true;
            // 
            // editAxisMappingButton
            // 
            this.editAxisMappingButton.Enabled = false;
            this.editAxisMappingButton.Location = new System.Drawing.Point(86, 213);
            this.editAxisMappingButton.Name = "editAxisMappingButton";
            this.editAxisMappingButton.Size = new System.Drawing.Size(75, 23);
            this.editAxisMappingButton.TabIndex = 2;
            this.editAxisMappingButton.Text = "Edit";
            this.editAxisMappingButton.UseVisualStyleBackColor = true;
            // 
            // addAxisMappingButton
            // 
            this.addAxisMappingButton.Location = new System.Drawing.Point(5, 213);
            this.addAxisMappingButton.Name = "addAxisMappingButton";
            this.addAxisMappingButton.Size = new System.Drawing.Size(75, 23);
            this.addAxisMappingButton.TabIndex = 1;
            this.addAxisMappingButton.Text = "Add";
            this.addAxisMappingButton.UseVisualStyleBackColor = true;
            // 
            // axisMappingsListView
            // 
            this.axisMappingsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.controlNumberColumnHeader,
            this.axisColumnHeader,
            this.valueRangeColumnHeader,
            this.invertColumnHeader,
            this.axisControllerIndexColumnHeader});
            this.axisMappingsListView.FullRowSelect = true;
            this.axisMappingsListView.Location = new System.Drawing.Point(5, 6);
            this.axisMappingsListView.MultiSelect = false;
            this.axisMappingsListView.Name = "axisMappingsListView";
            this.axisMappingsListView.Size = new System.Drawing.Size(541, 201);
            this.axisMappingsListView.TabIndex = 0;
            this.axisMappingsListView.UseCompatibleStateImageBehavior = false;
            this.axisMappingsListView.View = System.Windows.Forms.View.Details;
            // 
            // controlNumberColumnHeader
            // 
            this.controlNumberColumnHeader.Text = "Control Number";
            this.controlNumberColumnHeader.Width = 100;
            // 
            // axisColumnHeader
            // 
            this.axisColumnHeader.Text = "Axis";
            this.axisColumnHeader.Width = 100;
            // 
            // valueRangeColumnHeader
            // 
            this.valueRangeColumnHeader.Text = "Value Range";
            this.valueRangeColumnHeader.Width = 100;
            // 
            // invertColumnHeader
            // 
            this.invertColumnHeader.Text = "Invert";
            this.invertColumnHeader.Width = 50;
            // 
            // axisControllerIndexColumnHeader
            // 
            this.axisControllerIndexColumnHeader.Text = "Controller Index";
            this.axisControllerIndexColumnHeader.Width = 100;
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
            this.descriptionTextBox.Location = new System.Drawing.Point(6, 22);
            this.descriptionTextBox.Multiline = true;
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(548, 52);
            this.descriptionTextBox.TabIndex = 0;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(497, 440);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(416, 440);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 3;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // GameControllerMappingDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 475);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.descriptionGroupBox);
            this.Controls.Add(this.mappingsTabControl);
            this.Controls.Add(this.controllerSettingsGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GameControllerMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Game Controller Mapping";
            this.controllerSettingsGroupBox.ResumeLayout(false);
            this.controllerSettingsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.controllerIndexNumericUpDown)).EndInit();
            this.mappingsTabControl.ResumeLayout(false);
            this.buttonMappingsTabPage.ResumeLayout(false);
            this.axisMappingsTabPage.ResumeLayout(false);
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox controllerSettingsGroupBox;
        private System.Windows.Forms.NumericUpDown controllerIndexNumericUpDown;
        private System.Windows.Forms.Label controllerIndexLabel;
        private System.Windows.Forms.TabControl mappingsTabControl;
        private System.Windows.Forms.TabPage buttonMappingsTabPage;
        private System.Windows.Forms.Button deleteButtonMappingButton;
        private System.Windows.Forms.Button editButtonMappingButton;
        private System.Windows.Forms.Button addButtonMappingButton;
        private System.Windows.Forms.ListView buttonMappingsListView;
        private System.Windows.Forms.ColumnHeader midiNoteColumnHeader;
        private System.Windows.Forms.ColumnHeader buttonColumnHeader;
        private System.Windows.Forms.ColumnHeader buttonControllerIndexColumnHeader;
        private System.Windows.Forms.TabPage axisMappingsTabPage;
        private System.Windows.Forms.Button deleteAxisMappingButton;
        private System.Windows.Forms.Button editAxisMappingButton;
        private System.Windows.Forms.Button addAxisMappingButton;
        private System.Windows.Forms.ListView axisMappingsListView;
        private System.Windows.Forms.ColumnHeader controlNumberColumnHeader;
        private System.Windows.Forms.ColumnHeader axisColumnHeader;
        private System.Windows.Forms.ColumnHeader valueRangeColumnHeader;
        private System.Windows.Forms.ColumnHeader invertColumnHeader;
        private System.Windows.Forms.ColumnHeader axisControllerIndexColumnHeader;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
    }
}
