namespace MIDIFlux.GUI.Dialogs
{
    partial class ValueConditionEditDialog
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
            this.rangeGroupBox = new System.Windows.Forms.GroupBox();
            this.rangeTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.minValueLabel = new System.Windows.Forms.Label();
            this.minValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxValueLabel = new System.Windows.Forms.Label();
            this.maxValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.rangeInfoLabel = new System.Windows.Forms.Label();
            this.actionGroupBox = new System.Windows.Forms.GroupBox();
            this.actionTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.actionLabel = new System.Windows.Forms.Label();
            this.actionDisplayLabel = new System.Windows.Forms.Label();
            this.configureActionButton = new System.Windows.Forms.Button();
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.propertiesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.rangeGroupBox.SuspendLayout();
            this.rangeTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).BeginInit();
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
            this.mainTableLayoutPanel.Controls.Add(this.rangeGroupBox, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.actionGroupBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.propertiesGroupBox, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 3);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(500, 290);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // rangeGroupBox
            // 
            this.rangeGroupBox.Controls.Add(this.rangeTableLayoutPanel);
            this.rangeGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rangeGroupBox.Location = new System.Drawing.Point(3, 3);
            this.rangeGroupBox.Name = "rangeGroupBox";
            this.rangeGroupBox.Size = new System.Drawing.Size(494, 94);
            this.rangeGroupBox.TabIndex = 0;
            this.rangeGroupBox.TabStop = false;
            this.rangeGroupBox.Text = "Value Range";
            // 
            // rangeTableLayoutPanel
            // 
            this.rangeTableLayoutPanel.ColumnCount = 4;
            this.rangeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.rangeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.rangeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.rangeTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.rangeTableLayoutPanel.Controls.Add(this.minValueLabel, 0, 0);
            this.rangeTableLayoutPanel.Controls.Add(this.minValueNumericUpDown, 1, 0);
            this.rangeTableLayoutPanel.Controls.Add(this.maxValueLabel, 2, 0);
            this.rangeTableLayoutPanel.Controls.Add(this.maxValueNumericUpDown, 3, 0);
            this.rangeTableLayoutPanel.Controls.Add(this.rangeInfoLabel, 0, 1);
            this.rangeTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rangeTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.rangeTableLayoutPanel.Name = "rangeTableLayoutPanel";
            this.rangeTableLayoutPanel.RowCount = 2;
            this.rangeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.rangeTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.rangeTableLayoutPanel.Size = new System.Drawing.Size(488, 75);
            this.rangeTableLayoutPanel.TabIndex = 0;
            // 
            // minValueLabel
            // 
            this.minValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.minValueLabel.AutoSize = true;
            this.minValueLabel.Location = new System.Drawing.Point(3, 8);
            this.minValueLabel.Name = "minValueLabel";
            this.minValueLabel.Size = new System.Drawing.Size(57, 13);
            this.minValueLabel.TabIndex = 0;
            this.minValueLabel.Text = "Min Value:";
            // 
            // minValueNumericUpDown
            // 
            this.minValueNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.minValueNumericUpDown.Location = new System.Drawing.Point(83, 5);
            this.minValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.minValueNumericUpDown.Name = "minValueNumericUpDown";
            this.minValueNumericUpDown.Size = new System.Drawing.Size(94, 20);
            this.minValueNumericUpDown.TabIndex = 1;
            // 
            // maxValueLabel
            // 
            this.maxValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.maxValueLabel.AutoSize = true;
            this.maxValueLabel.Location = new System.Drawing.Point(183, 8);
            this.maxValueLabel.Name = "maxValueLabel";
            this.maxValueLabel.Size = new System.Drawing.Size(60, 13);
            this.maxValueLabel.TabIndex = 2;
            this.maxValueLabel.Text = "Max Value:";
            // 
            // maxValueNumericUpDown
            // 
            this.maxValueNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.maxValueNumericUpDown.Location = new System.Drawing.Point(263, 5);
            this.maxValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.maxValueNumericUpDown.Name = "maxValueNumericUpDown";
            this.maxValueNumericUpDown.Size = new System.Drawing.Size(94, 20);
            this.maxValueNumericUpDown.TabIndex = 3;
            this.maxValueNumericUpDown.Value = new decimal(new int[] {
            127,
            0,
            0,
            0});
            // 
            // rangeInfoLabel
            // 
            this.rangeInfoLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.rangeInfoLabel.AutoSize = true;
            this.rangeInfoLabel.ForeColor = System.Drawing.SystemColors.GrayText;
            this.rangeInfoLabel.Location = new System.Drawing.Point(3, 46);
            this.rangeInfoLabel.Name = "rangeInfoLabel";
            this.rangeInfoLabel.Size = new System.Drawing.Size(282, 13);
            this.rangeInfoLabel.TabIndex = 4;
            this.rangeInfoLabel.Text = "MIDI values range from 0 to 127. Use same value for exact match.";
            // 
            // actionGroupBox
            // 
            this.actionGroupBox.Controls.Add(this.actionTableLayoutPanel);
            this.actionGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionGroupBox.Location = new System.Drawing.Point(3, 103);
            this.actionGroupBox.Name = "actionGroupBox";
            this.actionGroupBox.Size = new System.Drawing.Size(494, 74);
            this.actionGroupBox.TabIndex = 1;
            this.actionGroupBox.TabStop = false;
            this.actionGroupBox.Text = "Action to Execute";
            // 
            // actionTableLayoutPanel
            // 
            this.actionTableLayoutPanel.ColumnCount = 3;
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.actionTableLayoutPanel.Controls.Add(this.actionLabel, 0, 0);
            this.actionTableLayoutPanel.Controls.Add(this.actionDisplayLabel, 1, 0);
            this.actionTableLayoutPanel.Controls.Add(this.configureActionButton, 2, 0);
            this.actionTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.actionTableLayoutPanel.Name = "actionTableLayoutPanel";
            this.actionTableLayoutPanel.RowCount = 1;
            this.actionTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionTableLayoutPanel.Size = new System.Drawing.Size(488, 55);
            this.actionTableLayoutPanel.TabIndex = 0;
            // 
            // actionLabel
            // 
            this.actionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.actionLabel.AutoSize = true;
            this.actionLabel.Location = new System.Drawing.Point(3, 21);
            this.actionLabel.Name = "actionLabel";
            this.actionLabel.Size = new System.Drawing.Size(40, 13);
            this.actionLabel.TabIndex = 0;
            this.actionLabel.Text = "Action:";
            // 
            // actionDisplayLabel
            // 
            this.actionDisplayLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.actionDisplayLabel.AutoSize = true;
            this.actionDisplayLabel.Location = new System.Drawing.Point(53, 21);
            this.actionDisplayLabel.Name = "actionDisplayLabel";
            this.actionDisplayLabel.Size = new System.Drawing.Size(108, 13);
            this.actionDisplayLabel.TabIndex = 1;
            this.actionDisplayLabel.Text = "Key Press/Release: A";
            // 
            // configureActionButton
            // 
            this.configureActionButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.configureActionButton.Location = new System.Drawing.Point(410, 16);
            this.configureActionButton.Name = "configureActionButton";
            this.configureActionButton.Size = new System.Drawing.Size(75, 23);
            this.configureActionButton.TabIndex = 2;
            this.configureActionButton.Text = "Configure...";
            this.configureActionButton.UseVisualStyleBackColor = true;
            // 
            // propertiesGroupBox
            // 
            this.propertiesGroupBox.Controls.Add(this.propertiesTableLayoutPanel);
            this.propertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesGroupBox.Location = new System.Drawing.Point(3, 183);
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.Size = new System.Drawing.Size(494, 54);
            this.propertiesGroupBox.TabIndex = 2;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Properties";
            // 
            // propertiesTableLayoutPanel
            // 
            this.propertiesTableLayoutPanel.ColumnCount = 2;
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionTextBox, 1, 0);
            this.propertiesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.propertiesTableLayoutPanel.Name = "propertiesTableLayoutPanel";
            this.propertiesTableLayoutPanel.RowCount = 1;
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesTableLayoutPanel.Size = new System.Drawing.Size(488, 35);
            this.propertiesTableLayoutPanel.TabIndex = 0;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 11);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Description:";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.Location = new System.Drawing.Point(83, 7);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(402, 20);
            this.descriptionTextBox.TabIndex = 1;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(3, 243);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(494, 44);
            this.buttonPanel.TabIndex = 3;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(332, 9);
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
            this.cancelButton.Location = new System.Drawing.Point(413, 9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ValueConditionEditDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(500, 290);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ValueConditionEditDialog";
            this.Text = "Edit Value Condition";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.rangeGroupBox.ResumeLayout(false);
            this.rangeTableLayoutPanel.ResumeLayout(false);
            this.rangeTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).EndInit();
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

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.GroupBox rangeGroupBox;
        private System.Windows.Forms.TableLayoutPanel rangeTableLayoutPanel;
        private System.Windows.Forms.Label minValueLabel;
        private System.Windows.Forms.NumericUpDown minValueNumericUpDown;
        private System.Windows.Forms.Label maxValueLabel;
        private System.Windows.Forms.NumericUpDown maxValueNumericUpDown;
        private System.Windows.Forms.Label rangeInfoLabel;
        private System.Windows.Forms.GroupBox actionGroupBox;
        private System.Windows.Forms.TableLayoutPanel actionTableLayoutPanel;
        private System.Windows.Forms.Label actionLabel;
        private System.Windows.Forms.Label actionDisplayLabel;
        private System.Windows.Forms.Button configureActionButton;
        private System.Windows.Forms.GroupBox propertiesGroupBox;
        private System.Windows.Forms.TableLayoutPanel propertiesTableLayoutPanel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
