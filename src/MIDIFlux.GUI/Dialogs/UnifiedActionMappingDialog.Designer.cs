namespace MIDIFlux.GUI.Dialogs
{
    partial class UnifiedActionMappingDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;



        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.midiInputGroupBox = new System.Windows.Forms.GroupBox();
            this.midiInputTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.midiInputTypeLabel = new System.Windows.Forms.Label();
            this.midiInputTypeComboBox = new System.Windows.Forms.ComboBox();
            this.midiInputNumberLabel = new System.Windows.Forms.Label();
            this.midiInputNumberNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.midiChannelLabel = new System.Windows.Forms.Label();
            this.midiChannelComboBox = new System.Windows.Forms.ComboBox();
            this.deviceNameLabel = new System.Windows.Forms.Label();
            this.deviceNameComboBox = new System.Windows.Forms.ComboBox();
            this.listenButton = new System.Windows.Forms.Button();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.actionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.actionTypeLabel = new System.Windows.Forms.Label();
            this.actionTypeComboBox = new System.Windows.Forms.ComboBox();
            this.actionParametersPanel = new System.Windows.Forms.Panel();
            this.testButton = new System.Windows.Forms.Button();
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.propertiesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.enabledCheckBox = new System.Windows.Forms.CheckBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.midiInputGroupBox.SuspendLayout();
            this.midiInputTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiInputNumberNumericUpDown)).BeginInit();
            this.actionGroupBox.SuspendLayout();
            this.actionTableLayoutPanel.SuspendLayout();
            this.propertiesGroupBox.SuspendLayout();
            this.propertiesTableLayoutPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            //
            // mainTableLayoutPanel
            //
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.midiInputGroupBox, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.actionGroupBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.propertiesGroupBox, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 3);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(484, 461);
            this.mainTableLayoutPanel.TabIndex = 0;
            //
            // midiInputGroupBox
            //
            this.midiInputGroupBox.Controls.Add(this.midiInputTableLayoutPanel);
            this.midiInputGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midiInputGroupBox.Location = new System.Drawing.Point(3, 3);
            this.midiInputGroupBox.Name = "midiInputGroupBox";
            this.midiInputGroupBox.Size = new System.Drawing.Size(478, 114);
            this.midiInputGroupBox.TabIndex = 0;
            this.midiInputGroupBox.TabStop = false;
            this.midiInputGroupBox.Text = "MIDI Input";
            //
            // midiInputTableLayoutPanel
            //
            this.midiInputTableLayoutPanel.ColumnCount = 3;
            this.midiInputTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.midiInputTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.midiInputTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.midiInputTableLayoutPanel.Controls.Add(this.midiInputTypeLabel, 0, 0);
            this.midiInputTableLayoutPanel.Controls.Add(this.midiInputTypeComboBox, 1, 0);
            this.midiInputTableLayoutPanel.Controls.Add(this.midiInputNumberLabel, 0, 1);
            this.midiInputTableLayoutPanel.Controls.Add(this.midiInputNumberNumericUpDown, 1, 1);
            this.midiInputTableLayoutPanel.Controls.Add(this.midiChannelLabel, 0, 2);
            this.midiInputTableLayoutPanel.Controls.Add(this.midiChannelComboBox, 1, 2);
            this.midiInputTableLayoutPanel.Controls.Add(this.deviceNameLabel, 0, 3);
            this.midiInputTableLayoutPanel.Controls.Add(this.deviceNameComboBox, 1, 3);
            this.midiInputTableLayoutPanel.Controls.Add(this.listenButton, 2, 0);
            this.midiInputTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.midiInputTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.midiInputTableLayoutPanel.Name = "midiInputTableLayoutPanel";
            this.midiInputTableLayoutPanel.RowCount = 4;
            this.midiInputTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.midiInputTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.midiInputTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.midiInputTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.midiInputTableLayoutPanel.Size = new System.Drawing.Size(472, 95);
            this.midiInputTableLayoutPanel.TabIndex = 0;
            //
            // midiInputTypeLabel
            //
            this.midiInputTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.midiInputTypeLabel.AutoSize = true;
            this.midiInputTypeLabel.Location = new System.Drawing.Point(3, 5);
            this.midiInputTypeLabel.Name = "midiInputTypeLabel";
            this.midiInputTypeLabel.Size = new System.Drawing.Size(65, 13);
            this.midiInputTypeLabel.TabIndex = 0;
            this.midiInputTypeLabel.Text = "Input Type:";
            //
            // midiInputTypeComboBox
            //
            this.midiInputTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.midiInputTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.midiInputTypeComboBox.FormattingEnabled = true;
            this.midiInputTypeComboBox.Location = new System.Drawing.Point(103, 1);
            this.midiInputTypeComboBox.Name = "midiInputTypeComboBox";
            this.midiInputTypeComboBox.Size = new System.Drawing.Size(286, 21);
            this.midiInputTypeComboBox.TabIndex = 1;
            //
            // midiInputNumberLabel
            //
            this.midiInputNumberLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.midiInputNumberLabel.AutoSize = true;
            this.midiInputNumberLabel.Location = new System.Drawing.Point(3, 28);
            this.midiInputNumberLabel.Name = "midiInputNumberLabel";
            this.midiInputNumberLabel.Size = new System.Drawing.Size(78, 13);
            this.midiInputNumberLabel.TabIndex = 2;
            this.midiInputNumberLabel.Text = "Input Number:";
            //
            // midiInputNumberNumericUpDown
            //
            this.midiInputNumberNumericUpDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.midiInputNumberNumericUpDown.Location = new System.Drawing.Point(103, 24);
            this.midiInputNumberNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.midiInputNumberNumericUpDown.Name = "midiInputNumberNumericUpDown";
            this.midiInputNumberNumericUpDown.Size = new System.Drawing.Size(286, 20);
            this.midiInputNumberNumericUpDown.TabIndex = 3;
            //
            // midiChannelLabel
            //
            this.midiChannelLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.midiChannelLabel.AutoSize = true;
            this.midiChannelLabel.Location = new System.Drawing.Point(3, 51);
            this.midiChannelLabel.Name = "midiChannelLabel";
            this.midiChannelLabel.Size = new System.Drawing.Size(49, 13);
            this.midiChannelLabel.TabIndex = 4;
            this.midiChannelLabel.Text = "Channel:";
            //
            // midiChannelComboBox
            //
            this.midiChannelComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.midiChannelComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.midiChannelComboBox.FormattingEnabled = true;
            this.midiChannelComboBox.Location = new System.Drawing.Point(103, 47);
            this.midiChannelComboBox.Name = "midiChannelComboBox";
            this.midiChannelComboBox.Size = new System.Drawing.Size(286, 21);
            this.midiChannelComboBox.TabIndex = 5;
            //
            // deviceNameLabel
            //
            this.deviceNameLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.deviceNameLabel.AutoSize = true;
            this.deviceNameLabel.Location = new System.Drawing.Point(3, 74);
            this.deviceNameLabel.Name = "deviceNameLabel";
            this.deviceNameLabel.Size = new System.Drawing.Size(44, 13);
            this.deviceNameLabel.TabIndex = 6;
            this.deviceNameLabel.Text = "Device:";
            //
            // deviceNameComboBox
            //
            this.deviceNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceNameComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceNameComboBox.FormattingEnabled = true;
            this.deviceNameComboBox.Location = new System.Drawing.Point(103, 70);
            this.deviceNameComboBox.Name = "deviceNameComboBox";
            this.deviceNameComboBox.Size = new System.Drawing.Size(286, 21);
            this.deviceNameComboBox.TabIndex = 7;
            //
            // listenButton
            //
            this.listenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.listenButton.Location = new System.Drawing.Point(395, 0);
            this.listenButton.Name = "listenButton";
            this.midiInputTableLayoutPanel.SetRowSpan(this.listenButton, 2);
            this.listenButton.Size = new System.Drawing.Size(74, 46);
            this.listenButton.TabIndex = 8;
            this.listenButton.Text = "Listen";
            this.listenButton.UseVisualStyleBackColor = true;
            //
            // actionGroupBox
            //
            this.actionGroupBox.Controls.Add(this.actionTableLayoutPanel);
            this.actionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionGroupBox.Location = new System.Drawing.Point(3, 123);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(478, 215);
            this.actionGroupBox.TabIndex = 1;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action";
            //
            // actionTableLayoutPanel
            //
            this.actionTableLayoutPanel.ColumnCount = 3;
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.actionTableLayoutPanel.Controls.Add(this.actionTypeLabel, 0, 0);
            this.actionTableLayoutPanel.Controls.Add(this.actionTypeComboBox, 1, 0);
            this.actionTableLayoutPanel.Controls.Add(this.actionParametersPanel, 0, 1);
            this.actionTableLayoutPanel.Controls.Add(this.testButton, 2, 0);
            this.actionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.actionTableLayoutPanel.Name = "actionTableLayoutPanel";
            this.actionTableLayoutPanel.RowCount = 2;
            this.actionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.actionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionTableLayoutPanel.Size = new System.Drawing.Size(472, 196);
            this.actionTableLayoutPanel.TabIndex = 0;
            //
            // actionTypeLabel
            //
            this.actionTypeLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.actionTypeLabel.AutoSize = true;
            this.actionTypeLabel.Location = new System.Drawing.Point(3, 5);
            this.actionTypeLabel.Name = "actionTypeLabel";
            this.actionTypeLabel.Size = new System.Drawing.Size(71, 13);
            this.actionTypeLabel.TabIndex = 0;
            this.actionTypeLabel.Text = "Action Type:";
            //
            // actionTypeComboBox
            //
            this.actionTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.actionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.actionTypeComboBox.FormattingEnabled = true;
            this.actionTypeComboBox.Location = new System.Drawing.Point(103, 1);
            this.actionTypeComboBox.Name = "actionTypeComboBox";
            this.actionTypeComboBox.Size = new System.Drawing.Size(286, 21);
            this.actionTypeComboBox.TabIndex = 1;
            //
            // actionParametersPanel
            //
            this.actionParametersPanel.AutoScroll = true;
            this.actionParametersPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.actionTableLayoutPanel.SetColumnSpan(this.actionParametersPanel, 3);
            this.actionParametersPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionParametersPanel.Location = new System.Drawing.Point(3, 26);
            this.actionParametersPanel.Name = "actionParametersPanel";
            this.actionParametersPanel.Size = new System.Drawing.Size(466, 167);
            this.actionParametersPanel.TabIndex = 2;
            //
            // testButton
            //
            this.testButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.testButton.Location = new System.Drawing.Point(395, 0);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(74, 23);
            this.testButton.TabIndex = 3;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            //
            // propertiesGroupBox
            //
            this.propertiesGroupBox.Controls.Add(this.propertiesTableLayoutPanel);
            this.propertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesGroupBox.Location = new System.Drawing.Point(3, 344);
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.Size = new System.Drawing.Size(478, 74);
            this.propertiesGroupBox.TabIndex = 2;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Properties";
            //
            // propertiesTableLayoutPanel
            //
            this.propertiesTableLayoutPanel.ColumnCount = 2;
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionTextBox, 1, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.enabledCheckBox, 0, 1);
            this.propertiesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.propertiesTableLayoutPanel.Name = "propertiesTableLayoutPanel";
            this.propertiesTableLayoutPanel.RowCount = 2;
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 23F));
            this.propertiesTableLayoutPanel.Size = new System.Drawing.Size(472, 55);
            this.propertiesTableLayoutPanel.TabIndex = 0;
            //
            // descriptionLabel
            //
            this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 5);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Description:";
            //
            // descriptionTextBox
            //
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.Location = new System.Drawing.Point(103, 1);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(366, 20);
            this.descriptionTextBox.TabIndex = 1;
            //
            // enabledCheckBox
            //
            this.enabledCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.enabledCheckBox.AutoSize = true;
            this.enabledCheckBox.Checked = true;
            this.enabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.propertiesTableLayoutPanel.SetColumnSpan(this.enabledCheckBox, 2);
            this.enabledCheckBox.Location = new System.Drawing.Point(3, 26);
            this.enabledCheckBox.Name = "enabledCheckBox";
            this.enabledCheckBox.Size = new System.Drawing.Size(65, 17);
            this.enabledCheckBox.TabIndex = 2;
            this.enabledCheckBox.Text = "Enabled";
            this.enabledCheckBox.UseVisualStyleBackColor = true;
            //
            // buttonPanel
            //
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(3, 424);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(478, 34);
            this.buttonPanel.TabIndex = 3;
            //
            // okButton
            //
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(319, 8);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            //
            // cancelButton
            //
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(400, 8);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            //
            // UnifiedActionMappingDialog
            //
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 461);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UnifiedActionMappingDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Unified Action Mapping";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.midiInputGroupBox.ResumeLayout(false);
            this.midiInputTableLayoutPanel.ResumeLayout(false);
            this.midiInputTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.midiInputNumberNumericUpDown)).EndInit();
            this.actionGroupBox.ResumeLayout(false);
            this.actionTableLayoutPanel.ResumeLayout(false);
            this.actionTableLayoutPanel.PerformLayout();
            this.propertiesGroupBox.ResumeLayout(false);
            this.propertiesTableLayoutPanel.ResumeLayout(false);
            this.propertiesTableLayoutPanel.PerformLayout();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        protected System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        protected System.Windows.Forms.GroupBox midiInputGroupBox;
        protected System.Windows.Forms.TableLayoutPanel midiInputTableLayoutPanel;
        protected System.Windows.Forms.Label midiInputTypeLabel;
        protected System.Windows.Forms.ComboBox midiInputTypeComboBox;
        protected System.Windows.Forms.Label midiInputNumberLabel;
        protected System.Windows.Forms.NumericUpDown midiInputNumberNumericUpDown;
        protected System.Windows.Forms.Label midiChannelLabel;
        protected System.Windows.Forms.ComboBox midiChannelComboBox;
        protected System.Windows.Forms.Label deviceNameLabel;
        protected System.Windows.Forms.ComboBox deviceNameComboBox;
        protected System.Windows.Forms.Button listenButton;
        protected System.Windows.Forms.GroupBox actionGroupBox;
        protected System.Windows.Forms.TableLayoutPanel actionTableLayoutPanel;
        protected System.Windows.Forms.Label actionTypeLabel;
        protected System.Windows.Forms.ComboBox actionTypeComboBox;
        protected System.Windows.Forms.Panel actionParametersPanel;
        protected System.Windows.Forms.Button testButton;
        protected System.Windows.Forms.GroupBox propertiesGroupBox;
        protected System.Windows.Forms.TableLayoutPanel propertiesTableLayoutPanel;
        protected System.Windows.Forms.Label descriptionLabel;
        protected System.Windows.Forms.TextBox descriptionTextBox;
        protected System.Windows.Forms.CheckBox enabledCheckBox;
        protected System.Windows.Forms.Panel buttonPanel;
        protected System.Windows.Forms.Button okButton;
        protected System.Windows.Forms.Button cancelButton;
    }
}
