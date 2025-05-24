namespace MIDIFlux.GUI.Dialogs
{
    partial class ConditionalActionDialog
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
            this.propertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.propertiesTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.descriptionLabel = new System.Windows.Forms.Label();
            this.descriptionTextBox = new System.Windows.Forms.TextBox();
            this.conditionsGroupBox = new System.Windows.Forms.GroupBox();
            this.conditionsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.conditionsListView = new System.Windows.Forms.ListView();
            this.rangeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.conditionsButtonPanel = new System.Windows.Forms.Panel();
            this.addConditionButton = new System.Windows.Forms.Button();
            this.editConditionButton = new System.Windows.Forms.Button();
            this.removeConditionButton = new System.Windows.Forms.Button();
            this.testGroupBox = new System.Windows.Forms.GroupBox();
            this.testTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.testValueLabel = new System.Windows.Forms.Label();
            this.testValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.testButton = new System.Windows.Forms.Button();
            this.testResultLabel = new System.Windows.Forms.Label();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.propertiesGroupBox.SuspendLayout();
            this.propertiesTableLayoutPanel.SuspendLayout();
            this.conditionsGroupBox.SuspendLayout();
            this.conditionsTableLayoutPanel.SuspendLayout();
            this.conditionsButtonPanel.SuspendLayout();
            this.testGroupBox.SuspendLayout();
            this.testTableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.testValueNumericUpDown)).BeginInit();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.propertiesGroupBox, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.conditionsGroupBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.testGroupBox, 0, 2);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 3);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 4;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 60F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.mainTableLayoutPanel.Size = new System.Drawing.Size(800, 600);
            this.mainTableLayoutPanel.TabIndex = 0;
            // 
            // propertiesGroupBox
            // 
            this.propertiesGroupBox.Controls.Add(this.propertiesTableLayoutPanel);
            this.propertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesGroupBox.Location = new System.Drawing.Point(3, 3);
            this.propertiesGroupBox.Name = "propertiesGroupBox";
            this.propertiesGroupBox.Size = new System.Drawing.Size(794, 54);
            this.propertiesGroupBox.TabIndex = 0;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Conditional Properties";
            // 
            // propertiesTableLayoutPanel
            // 
            this.propertiesTableLayoutPanel.ColumnCount = 2;
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionTextBox, 1, 0);
            this.propertiesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.propertiesTableLayoutPanel.Name = "propertiesTableLayoutPanel";
            this.propertiesTableLayoutPanel.RowCount = 1;
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.propertiesTableLayoutPanel.Size = new System.Drawing.Size(788, 35);
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
            this.descriptionTextBox.Location = new System.Drawing.Point(123, 7);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(662, 20);
            this.descriptionTextBox.TabIndex = 1;
            // 
            // conditionsGroupBox
            // 
            this.conditionsGroupBox.Controls.Add(this.conditionsTableLayoutPanel);
            this.conditionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionsGroupBox.Location = new System.Drawing.Point(3, 63);
            this.conditionsGroupBox.Name = "conditionsGroupBox";
            this.conditionsGroupBox.Size = new System.Drawing.Size(794, 404);
            this.conditionsGroupBox.TabIndex = 1;
            this.conditionsGroupBox.TabStop = false;
            this.conditionsGroupBox.Text = "Value Conditions";
            // 
            // conditionsTableLayoutPanel
            // 
            this.conditionsTableLayoutPanel.ColumnCount = 2;
            this.conditionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.conditionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.conditionsTableLayoutPanel.Controls.Add(this.conditionsListView, 0, 0);
            this.conditionsTableLayoutPanel.Controls.Add(this.conditionsButtonPanel, 1, 0);
            this.conditionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionsTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.conditionsTableLayoutPanel.Name = "conditionsTableLayoutPanel";
            this.conditionsTableLayoutPanel.RowCount = 1;
            this.conditionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.conditionsTableLayoutPanel.Size = new System.Drawing.Size(788, 385);
            this.conditionsTableLayoutPanel.TabIndex = 0;
            // 
            // conditionsListView
            // 
            this.conditionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.rangeColumnHeader,
            this.actionTypeColumnHeader,
            this.descriptionColumnHeader});
            this.conditionsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionsListView.FullRowSelect = true;
            this.conditionsListView.GridLines = true;
            this.conditionsListView.HideSelection = false;
            this.conditionsListView.Location = new System.Drawing.Point(3, 3);
            this.conditionsListView.MultiSelect = false;
            this.conditionsListView.Name = "conditionsListView";
            this.conditionsListView.Size = new System.Drawing.Size(662, 379);
            this.conditionsListView.TabIndex = 0;
            this.conditionsListView.UseCompatibleStateImageBehavior = false;
            this.conditionsListView.View = System.Windows.Forms.View.Details;
            // 
            // rangeColumnHeader
            // 
            this.rangeColumnHeader.Text = "Value Range";
            this.rangeColumnHeader.Width = 100;
            // 
            // actionTypeColumnHeader
            // 
            this.actionTypeColumnHeader.Text = "Action Type";
            this.actionTypeColumnHeader.Width = 150;
            // 
            // descriptionColumnHeader
            // 
            this.descriptionColumnHeader.Text = "Description";
            this.descriptionColumnHeader.Width = 350;
            // 
            // conditionsButtonPanel
            // 
            this.conditionsButtonPanel.Controls.Add(this.removeConditionButton);
            this.conditionsButtonPanel.Controls.Add(this.editConditionButton);
            this.conditionsButtonPanel.Controls.Add(this.addConditionButton);
            this.conditionsButtonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.conditionsButtonPanel.Location = new System.Drawing.Point(671, 3);
            this.conditionsButtonPanel.Name = "conditionsButtonPanel";
            this.conditionsButtonPanel.Size = new System.Drawing.Size(114, 379);
            this.conditionsButtonPanel.TabIndex = 1;
            // 
            // addConditionButton
            // 
            this.addConditionButton.Location = new System.Drawing.Point(3, 3);
            this.addConditionButton.Name = "addConditionButton";
            this.addConditionButton.Size = new System.Drawing.Size(108, 30);
            this.addConditionButton.TabIndex = 0;
            this.addConditionButton.Text = "Add Condition";
            this.addConditionButton.UseVisualStyleBackColor = true;
            // 
            // editConditionButton
            // 
            this.editConditionButton.Location = new System.Drawing.Point(3, 39);
            this.editConditionButton.Name = "editConditionButton";
            this.editConditionButton.Size = new System.Drawing.Size(108, 30);
            this.editConditionButton.TabIndex = 1;
            this.editConditionButton.Text = "Edit Condition";
            this.editConditionButton.UseVisualStyleBackColor = true;
            // 
            // removeConditionButton
            // 
            this.removeConditionButton.Location = new System.Drawing.Point(3, 75);
            this.removeConditionButton.Name = "removeConditionButton";
            this.removeConditionButton.Size = new System.Drawing.Size(108, 30);
            this.removeConditionButton.TabIndex = 2;
            this.removeConditionButton.Text = "Remove Condition";
            this.removeConditionButton.UseVisualStyleBackColor = true;
            // 
            // testGroupBox
            // 
            this.testGroupBox.Controls.Add(this.testTableLayoutPanel);
            this.testGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testGroupBox.Location = new System.Drawing.Point(3, 473);
            this.testGroupBox.Name = "testGroupBox";
            this.testGroupBox.Size = new System.Drawing.Size(794, 74);
            this.testGroupBox.TabIndex = 2;
            this.testGroupBox.TabStop = false;
            this.testGroupBox.Text = "Test Value";
            // 
            // testTableLayoutPanel
            // 
            this.testTableLayoutPanel.ColumnCount = 4;
            this.testTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.testTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.testTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 80F));
            this.testTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.testTableLayoutPanel.Controls.Add(this.testValueLabel, 0, 0);
            this.testTableLayoutPanel.Controls.Add(this.testValueNumericUpDown, 1, 0);
            this.testTableLayoutPanel.Controls.Add(this.testButton, 2, 0);
            this.testTableLayoutPanel.Controls.Add(this.testResultLabel, 3, 0);
            this.testTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.testTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.testTableLayoutPanel.Name = "testTableLayoutPanel";
            this.testTableLayoutPanel.RowCount = 1;
            this.testTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.testTableLayoutPanel.Size = new System.Drawing.Size(788, 55);
            this.testTableLayoutPanel.TabIndex = 0;
            // 
            // testValueLabel
            // 
            this.testValueLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.testValueLabel.AutoSize = true;
            this.testValueLabel.Location = new System.Drawing.Point(3, 21);
            this.testValueLabel.Name = "testValueLabel";
            this.testValueLabel.Size = new System.Drawing.Size(62, 13);
            this.testValueLabel.TabIndex = 0;
            this.testValueLabel.Text = "Test Value:";
            // 
            // testValueNumericUpDown
            // 
            this.testValueNumericUpDown.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.testValueNumericUpDown.Location = new System.Drawing.Point(83, 17);
            this.testValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.testValueNumericUpDown.Name = "testValueNumericUpDown";
            this.testValueNumericUpDown.Size = new System.Drawing.Size(94, 20);
            this.testValueNumericUpDown.TabIndex = 1;
            this.testValueNumericUpDown.Value = new decimal(new int[] {
            64,
            0,
            0,
            0});
            // 
            // testButton
            // 
            this.testButton.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.testButton.Location = new System.Drawing.Point(183, 16);
            this.testButton.Name = "testButton";
            this.testButton.Size = new System.Drawing.Size(74, 23);
            this.testButton.TabIndex = 2;
            this.testButton.Text = "Test";
            this.testButton.UseVisualStyleBackColor = true;
            // 
            // testResultLabel
            // 
            this.testResultLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.testResultLabel.AutoSize = true;
            this.testResultLabel.Location = new System.Drawing.Point(263, 21);
            this.testResultLabel.Name = "testResultLabel";
            this.testResultLabel.Size = new System.Drawing.Size(108, 13);
            this.testResultLabel.TabIndex = 3;
            this.testResultLabel.Text = "No matching condition";
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(3, 553);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(794, 44);
            this.buttonPanel.TabIndex = 3;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.Location = new System.Drawing.Point(632, 9);
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
            this.cancelButton.Location = new System.Drawing.Point(713, 9);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 1;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ConditionalActionDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "ConditionalActionDialog";
            this.Text = "Configure Conditional Action (Fader-to-Buttons)";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.propertiesGroupBox.ResumeLayout(false);
            this.propertiesTableLayoutPanel.ResumeLayout(false);
            this.propertiesTableLayoutPanel.PerformLayout();
            this.conditionsGroupBox.ResumeLayout(false);
            this.conditionsTableLayoutPanel.ResumeLayout(false);
            this.conditionsButtonPanel.ResumeLayout(false);
            this.testGroupBox.ResumeLayout(false);
            this.testTableLayoutPanel.ResumeLayout(false);
            this.testTableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.testValueNumericUpDown)).EndInit();
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.GroupBox propertiesGroupBox;
        private System.Windows.Forms.TableLayoutPanel propertiesTableLayoutPanel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.GroupBox conditionsGroupBox;
        private System.Windows.Forms.TableLayoutPanel conditionsTableLayoutPanel;
        private System.Windows.Forms.ListView conditionsListView;
        private System.Windows.Forms.ColumnHeader rangeColumnHeader;
        private System.Windows.Forms.ColumnHeader actionTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
        private System.Windows.Forms.Panel conditionsButtonPanel;
        private System.Windows.Forms.Button addConditionButton;
        private System.Windows.Forms.Button editConditionButton;
        private System.Windows.Forms.Button removeConditionButton;
        private System.Windows.Forms.GroupBox testGroupBox;
        private System.Windows.Forms.TableLayoutPanel testTableLayoutPanel;
        private System.Windows.Forms.Label testValueLabel;
        private System.Windows.Forms.NumericUpDown testValueNumericUpDown;
        private System.Windows.Forms.Button testButton;
        private System.Windows.Forms.Label testResultLabel;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
