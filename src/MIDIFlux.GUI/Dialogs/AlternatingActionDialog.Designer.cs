namespace MIDIFlux.GUI.Dialogs
{
    partial class AlternatingActionDialog
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
            this.mainTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionGroupBox = new System.Windows.Forms.GroupBox();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.configurationGroupBox = new System.Windows.Forms.GroupBox();
            this.configurationTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.stateKeyLabel = new System.Windows.Forms.Label();
            this.stateKeyTextBox = new System.Windows.Forms.TextBox();
            this.stateKeyHelpLabel = new System.Windows.Forms.Label();
            this.startWithPrimaryCheckBox = new System.Windows.Forms.CheckBox();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.actionsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.primaryActionGroupBox = new System.Windows.Forms.GroupBox();
            this.primaryActionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.primaryActionLabel = new System.Windows.Forms.Label();
            this.configurePrimaryActionButton = new System.Windows.Forms.Button();
            this.secondaryActionGroupBox = new System.Windows.Forms.GroupBox();
            this.secondaryActionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.secondaryActionLabel = new System.Windows.Forms.Label();
            this.configureSecondaryActionButton = new System.Windows.Forms.Button();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.descriptionGroupBox.SuspendLayout();
            this.configurationGroupBox.SuspendLayout();
            this.configurationTableLayoutPanel.SuspendLayout();
            this.actionsGroupBox.SuspendLayout();
            this.actionsTableLayoutPanel.SuspendLayout();
            this.primaryActionGroupBox.SuspendLayout();
            this.primaryActionTableLayoutPanel.SuspendLayout();
            this.secondaryActionGroupBox.SuspendLayout();
            this.secondaryActionTableLayoutPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.descriptionGroupBox, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.configurationGroupBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.actionsGroupBox, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 3);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(584, 461);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // descriptionGroupBox
            // 
            this.descriptionGroupBox.Controls.Add(this.descriptionTextBox);
            this.descriptionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionGroupBox.Location = new System.Drawing.Point(3, 3);
            this.descriptionGroupBox.Name = "descriptionGroupBox";
            this.descriptionGroupBox.Size = new System.Drawing.Size(578, 54);
            this.descriptionGroupBox.TabIndex = 0;
            this.descriptionGroupBox.TabStop = false;
            this.descriptionGroupBox.Text = "Description";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.descriptionTextBox.Location = new System.Drawing.Point(3, 19);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(572, 23);
            this.descriptionTextBox.TabIndex = 0;
            this.descriptionTextBox.TextChanged += new System.EventHandler(this.DescriptionTextBox_TextChanged);
            // 
            // configurationGroupBox
            // 
            this.configurationGroupBox.Controls.Add(this.configurationTableLayoutPanel);
            this.configurationGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationGroupBox.Location = new System.Drawing.Point(3, 63);
            this.configurationGroupBox.Name = "configurationGroupBox";
            this.configurationGroupBox.Size = new System.Drawing.Size(578, 94);
            this.configurationGroupBox.TabIndex = 1;
            this.configurationGroupBox.TabStop = false;
            this.configurationGroupBox.Text = "Configuration";
            // 
            // configurationTableLayoutPanel
            // 
            this.configurationTableLayoutPanel.ColumnCount = 3;
            this.configurationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.configurationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.configurationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.configurationTableLayoutPanel.Controls.Add(this.stateKeyLabel, 0, 0);
            this.configurationTableLayoutPanel.Controls.Add(this.stateKeyTextBox, 1, 0);
            this.configurationTableLayoutPanel.Controls.Add(this.stateKeyHelpLabel, 2, 0);
            this.configurationTableLayoutPanel.Controls.Add(this.startWithPrimaryCheckBox, 0, 1);
            this.configurationTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurationTableLayoutPanel.Location = new System.Drawing.Point(3, 19);
            this.configurationTableLayoutPanel.Name = "configurationTableLayoutPanel";
            this.configurationTableLayoutPanel.RowCount = 2;
            this.configurationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.configurationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.configurationTableLayoutPanel.Size = new System.Drawing.Size(572, 72);
            this.configurationTableLayoutPanel.TabIndex = 0;
            // 
            // stateKeyLabel
            // 
            this.stateKeyLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.stateKeyLabel.AutoSize = true;
            this.stateKeyLabel.Location = new System.Drawing.Point(3, 7);
            this.stateKeyLabel.Name = "stateKeyLabel";
            this.stateKeyLabel.Size = new System.Drawing.Size(61, 15);
            this.stateKeyLabel.TabIndex = 0;
            this.stateKeyLabel.Text = "State Key:";
            // 
            // stateKeyTextBox
            // 
            this.stateKeyTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.stateKeyTextBox.Location = new System.Drawing.Point(103, 3);
            this.stateKeyTextBox.Name = "stateKeyTextBox";
            this.stateKeyTextBox.Size = new System.Drawing.Size(230, 23);
            this.stateKeyTextBox.TabIndex = 1;
            this.stateKeyTextBox.TextChanged += new System.EventHandler(this.StateKeyTextBox_TextChanged);
            // 
            // stateKeyHelpLabel
            // 
            this.stateKeyHelpLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.stateKeyHelpLabel.AutoSize = true;
            this.stateKeyHelpLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.stateKeyHelpLabel.Location = new System.Drawing.Point(339, 7);
            this.stateKeyHelpLabel.Name = "stateKeyHelpLabel";
            this.stateKeyHelpLabel.Size = new System.Drawing.Size(154, 15);
            this.stateKeyHelpLabel.TabIndex = 2;
            this.stateKeyHelpLabel.Text = "(Leave empty for auto-gen)";
            // 
            // startWithPrimaryCheckBox
            // 
            this.startWithPrimaryCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.startWithPrimaryCheckBox.AutoSize = true;
            this.configurationTableLayoutPanel.SetColumnSpan(this.startWithPrimaryCheckBox, 3);
            this.startWithPrimaryCheckBox.Location = new System.Drawing.Point(3, 35);
            this.startWithPrimaryCheckBox.Name = "startWithPrimaryCheckBox";
            this.startWithPrimaryCheckBox.Size = new System.Drawing.Size(133, 19);
            this.startWithPrimaryCheckBox.TabIndex = 3;
            this.startWithPrimaryCheckBox.Text = "Start with Primary";
            this.startWithPrimaryCheckBox.UseVisualStyleBackColor = true;
            this.startWithPrimaryCheckBox.CheckedChanged += new System.EventHandler(this.StartWithPrimaryCheckBox_CheckedChanged);
            // 
            // actionsGroupBox
            // 
            this.actionsGroupBox.Controls.Add(this.actionsTableLayoutPanel);
            this.actionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsGroupBox.Location = new System.Drawing.Point(3, 163);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(578, 245);
            this.actionsGroupBox.TabIndex = 2;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions";
            // 
            // actionsTableLayoutPanel
            // 
            this.actionsTableLayoutPanel.ColumnCount = 2;
            this.actionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.actionsTableLayoutPanel.Controls.Add(this.primaryActionGroupBox, 0, 0);
            this.actionsTableLayoutPanel.Controls.Add(this.secondaryActionGroupBox, 1, 0);
            this.actionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsTableLayoutPanel.Location = new System.Drawing.Point(3, 19);
            this.actionsTableLayoutPanel.Name = "actionsTableLayoutPanel";
            this.actionsTableLayoutPanel.RowCount = 1;
            this.actionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionsTableLayoutPanel.Size = new System.Drawing.Size(572, 223);
            this.actionsTableLayoutPanel.TabIndex = 0;
            // 
            // primaryActionGroupBox
            // 
            this.primaryActionGroupBox.Controls.Add(this.primaryActionTableLayoutPanel);
            this.primaryActionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.primaryActionGroupBox.Location = new System.Drawing.Point(3, 3);
            this.primaryActionGroupBox.Name = "primaryActionGroupBox";
            this.primaryActionGroupBox.Size = new System.Drawing.Size(280, 217);
            this.primaryActionGroupBox.TabIndex = 0;
            this.primaryActionGroupBox.TabStop = false;
            this.primaryActionGroupBox.Text = "Primary Action";
            // 
            // primaryActionTableLayoutPanel
            // 
            this.primaryActionTableLayoutPanel.ColumnCount = 1;
            this.primaryActionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.primaryActionTableLayoutPanel.Controls.Add(this.primaryActionLabel, 0, 0);
            this.primaryActionTableLayoutPanel.Controls.Add(this.configurePrimaryActionButton, 0, 1);
            this.primaryActionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.primaryActionTableLayoutPanel.Location = new System.Drawing.Point(3, 19);
            this.primaryActionTableLayoutPanel.Name = "primaryActionTableLayoutPanel";
            this.primaryActionTableLayoutPanel.RowCount = 2;
            this.primaryActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.primaryActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.primaryActionTableLayoutPanel.Size = new System.Drawing.Size(274, 195);
            this.primaryActionTableLayoutPanel.TabIndex = 0;
            // 
            // primaryActionLabel
            // 
            this.primaryActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.primaryActionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.primaryActionLabel.Location = new System.Drawing.Point(3, 0);
            this.primaryActionLabel.Name = "primaryActionLabel";
            this.primaryActionLabel.Size = new System.Drawing.Size(268, 160);
            this.primaryActionLabel.TabIndex = 0;
            this.primaryActionLabel.Text = "No action configured";
            this.primaryActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // configurePrimaryActionButton
            // 
            this.configurePrimaryActionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configurePrimaryActionButton.Location = new System.Drawing.Point(3, 163);
            this.configurePrimaryActionButton.Name = "configurePrimaryActionButton";
            this.configurePrimaryActionButton.Size = new System.Drawing.Size(268, 29);
            this.configurePrimaryActionButton.TabIndex = 1;
            this.configurePrimaryActionButton.Text = "Configure...";
            this.configurePrimaryActionButton.UseVisualStyleBackColor = true;
            this.configurePrimaryActionButton.Click += new System.EventHandler(this.ConfigurePrimaryActionButton_Click);
            // 
            // secondaryActionGroupBox
            // 
            this.secondaryActionGroupBox.Controls.Add(this.secondaryActionTableLayoutPanel);
            this.secondaryActionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondaryActionGroupBox.Location = new System.Drawing.Point(289, 3);
            this.secondaryActionGroupBox.Name = "secondaryActionGroupBox";
            this.secondaryActionGroupBox.Size = new System.Drawing.Size(280, 217);
            this.secondaryActionGroupBox.TabIndex = 1;
            this.secondaryActionGroupBox.TabStop = false;
            this.secondaryActionGroupBox.Text = "Secondary Action";
            // 
            // secondaryActionTableLayoutPanel
            // 
            this.secondaryActionTableLayoutPanel.ColumnCount = 1;
            this.secondaryActionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.secondaryActionTableLayoutPanel.Controls.Add(this.secondaryActionLabel, 0, 0);
            this.secondaryActionTableLayoutPanel.Controls.Add(this.configureSecondaryActionButton, 0, 1);
            this.secondaryActionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.secondaryActionTableLayoutPanel.Location = new System.Drawing.Point(3, 19);
            this.secondaryActionTableLayoutPanel.Name = "secondaryActionTableLayoutPanel";
            this.secondaryActionTableLayoutPanel.RowCount = 2;
            this.secondaryActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.secondaryActionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.secondaryActionTableLayoutPanel.Size = new System.Drawing.Size(274, 195);
            this.secondaryActionTableLayoutPanel.TabIndex = 0;
            // 
            // secondaryActionLabel
            // 
            this.secondaryActionLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.secondaryActionLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.secondaryActionLabel.Location = new System.Drawing.Point(3, 0);
            this.secondaryActionLabel.Name = "secondaryActionLabel";
            this.secondaryActionLabel.Size = new System.Drawing.Size(268, 160);
            this.secondaryActionLabel.TabIndex = 0;
            this.secondaryActionLabel.Text = "No action configured";
            this.secondaryActionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // configureSecondaryActionButton
            // 
            this.configureSecondaryActionButton.Dock = System.Windows.Forms.DockStyle.Fill;
            this.configureSecondaryActionButton.Location = new System.Drawing.Point(3, 163);
            this.configureSecondaryActionButton.Name = "configureSecondaryActionButton";
            this.configureSecondaryActionButton.Size = new System.Drawing.Size(268, 29);
            this.configureSecondaryActionButton.TabIndex = 1;
            this.configureSecondaryActionButton.Text = "Configure...";
            this.configureSecondaryActionButton.UseVisualStyleBackColor = true;
            this.configureSecondaryActionButton.Click += new System.EventHandler(this.ConfigureSecondaryActionButton_Click);
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(3, 414);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(578, 44);
            this.buttonPanel.TabIndex = 3;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(419, 9);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 0;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(500, 9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // AlternatingActionDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(584, 461);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AlternatingActionDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Configure Alternating Action";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.descriptionGroupBox.ResumeLayout(false);
            this.descriptionGroupBox.PerformLayout();
            this.configurationGroupBox.ResumeLayout(false);
            this.configurationTableLayoutPanel.ResumeLayout(false);
            this.configurationTableLayoutPanel.PerformLayout();
            this.actionsGroupBox.ResumeLayout(false);
            this.actionsTableLayoutPanel.ResumeLayout(false);
            this.primaryActionGroupBox.ResumeLayout(false);
            this.primaryActionTableLayoutPanel.ResumeLayout(false);
            this.secondaryActionGroupBox.ResumeLayout(false);
            this.secondaryActionTableLayoutPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.GroupBox descriptionGroupBox;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.GroupBox configurationGroupBox;
        private System.Windows.Forms.TableLayoutPanel configurationTableLayoutPanel;
        private System.Windows.Forms.Label stateKeyLabel;
        private System.Windows.Forms.TextBox stateKeyTextBox;
        private System.Windows.Forms.Label stateKeyHelpLabel;
        private System.Windows.Forms.CheckBox startWithPrimaryCheckBox;
        private System.Windows.Forms.GroupBox actionsGroupBox;
        private System.Windows.Forms.TableLayoutPanel actionsTableLayoutPanel;
        private System.Windows.Forms.GroupBox primaryActionGroupBox;
        private System.Windows.Forms.TableLayoutPanel primaryActionTableLayoutPanel;
        private System.Windows.Forms.Label primaryActionLabel;
        private System.Windows.Forms.Button configurePrimaryActionButton;
        private System.Windows.Forms.GroupBox secondaryActionGroupBox;
        private System.Windows.Forms.TableLayoutPanel secondaryActionTableLayoutPanel;
        private System.Windows.Forms.Label secondaryActionLabel;
        private System.Windows.Forms.Button configureSecondaryActionButton;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
