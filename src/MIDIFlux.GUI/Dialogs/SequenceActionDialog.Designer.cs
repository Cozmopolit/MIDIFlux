namespace MIDIFlux.GUI.Dialogs
{
    partial class SequenceActionDialog
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
            this.errorHandlingLabel = new System.Windows.Forms.Label();
            this.errorHandlingComboBox = new System.Windows.Forms.ComboBox();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.actionsTableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.actionsListView = new System.Windows.Forms.ListView();
            this.stepColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.descriptionColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionsButtonPanel = new System.Windows.Forms.Panel();
            this.addActionButton = new System.Windows.Forms.Button();
            this.editActionButton = new System.Windows.Forms.Button();
            this.removeActionButton = new System.Windows.Forms.Button();
            this.moveUpButton = new System.Windows.Forms.Button();
            this.moveDownButton = new System.Windows.Forms.Button();
            this.templatesButton = new System.Windows.Forms.Button();
            this.buttonPanel = new System.Windows.Forms.Panel();
            this.okButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.mainTableLayoutPanel.SuspendLayout();
            this.propertiesGroupBox.SuspendLayout();
            this.propertiesTableLayoutPanel.SuspendLayout();
            this.actionsGroupBox.SuspendLayout();
            this.actionsTableLayoutPanel.SuspendLayout();
            this.actionsButtonPanel.SuspendLayout();
            this.buttonPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainTableLayoutPanel
            // 
            this.mainTableLayoutPanel.ColumnCount = 1;
            this.mainTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.mainTableLayoutPanel.Controls.Add(this.propertiesGroupBox, 0, 0);
            this.mainTableLayoutPanel.Controls.Add(this.actionsGroupBox, 0, 1);
            this.mainTableLayoutPanel.Controls.Add(this.buttonPanel, 0, 2);
            this.mainTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainTableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.mainTableLayoutPanel.Name = "mainTableLayoutPanel";
            this.mainTableLayoutPanel.RowCount = 3;
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.mainTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
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
            this.propertiesGroupBox.Size = new System.Drawing.Size(794, 94);
            this.propertiesGroupBox.TabIndex = 0;
            this.propertiesGroupBox.TabStop = false;
            this.propertiesGroupBox.Text = "Sequence Properties";
            // 
            // propertiesTableLayoutPanel
            // 
            this.propertiesTableLayoutPanel.ColumnCount = 2;
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.propertiesTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionLabel, 0, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.descriptionTextBox, 1, 0);
            this.propertiesTableLayoutPanel.Controls.Add(this.errorHandlingLabel, 0, 1);
            this.propertiesTableLayoutPanel.Controls.Add(this.errorHandlingComboBox, 1, 1);
            this.propertiesTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertiesTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.propertiesTableLayoutPanel.Name = "propertiesTableLayoutPanel";
            this.propertiesTableLayoutPanel.RowCount = 2;
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.propertiesTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 30F));
            this.propertiesTableLayoutPanel.Size = new System.Drawing.Size(788, 75);
            this.propertiesTableLayoutPanel.TabIndex = 0;
            // 
            // descriptionLabel
            // 
            this.descriptionLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.descriptionLabel.AutoSize = true;
            this.descriptionLabel.Location = new System.Drawing.Point(3, 8);
            this.descriptionLabel.Name = "descriptionLabel";
            this.descriptionLabel.Size = new System.Drawing.Size(63, 13);
            this.descriptionLabel.TabIndex = 0;
            this.descriptionLabel.Text = "Description:";
            // 
            // descriptionTextBox
            // 
            this.descriptionTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.descriptionTextBox.Location = new System.Drawing.Point(123, 5);
            this.descriptionTextBox.Name = "descriptionTextBox";
            this.descriptionTextBox.Size = new System.Drawing.Size(662, 20);
            this.descriptionTextBox.TabIndex = 1;
            // 
            // errorHandlingLabel
            // 
            this.errorHandlingLabel.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.errorHandlingLabel.AutoSize = true;
            this.errorHandlingLabel.Location = new System.Drawing.Point(3, 38);
            this.errorHandlingLabel.Name = "errorHandlingLabel";
            this.errorHandlingLabel.Size = new System.Drawing.Size(78, 13);
            this.errorHandlingLabel.TabIndex = 2;
            this.errorHandlingLabel.Text = "Error Handling:";
            // 
            // errorHandlingComboBox
            // 
            this.errorHandlingComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.errorHandlingComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.errorHandlingComboBox.FormattingEnabled = true;
            this.errorHandlingComboBox.Location = new System.Drawing.Point(123, 34);
            this.errorHandlingComboBox.Name = "errorHandlingComboBox";
            this.errorHandlingComboBox.Size = new System.Drawing.Size(200, 21);
            this.errorHandlingComboBox.TabIndex = 3;
            // 
            // actionsGroupBox
            // 
            this.actionsGroupBox.Controls.Add(this.actionsTableLayoutPanel);
            this.actionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsGroupBox.Location = new System.Drawing.Point(3, 103);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(794, 444);
            this.actionsGroupBox.TabIndex = 1;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions in Sequence";
            // 
            // actionsTableLayoutPanel
            // 
            this.actionsTableLayoutPanel.ColumnCount = 2;
            this.actionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionsTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.actionsTableLayoutPanel.Controls.Add(this.actionsListView, 0, 0);
            this.actionsTableLayoutPanel.Controls.Add(this.actionsButtonPanel, 1, 0);
            this.actionsTableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsTableLayoutPanel.Location = new System.Drawing.Point(3, 16);
            this.actionsTableLayoutPanel.Name = "actionsTableLayoutPanel";
            this.actionsTableLayoutPanel.RowCount = 1;
            this.actionsTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.actionsTableLayoutPanel.Size = new System.Drawing.Size(788, 425);
            this.actionsTableLayoutPanel.TabIndex = 0;
            // 
            // actionsListView
            // 
            this.actionsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.stepColumnHeader,
            this.actionTypeColumnHeader,
            this.descriptionColumnHeader});
            this.actionsListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsListView.FullRowSelect = true;
            this.actionsListView.GridLines = true;
            this.actionsListView.HideSelection = false;
            this.actionsListView.Location = new System.Drawing.Point(3, 3);
            this.actionsListView.MultiSelect = false;
            this.actionsListView.Name = "actionsListView";
            this.actionsListView.Size = new System.Drawing.Size(662, 419);
            this.actionsListView.TabIndex = 0;
            this.actionsListView.UseCompatibleStateImageBehavior = false;
            this.actionsListView.View = System.Windows.Forms.View.Details;
            // 
            // stepColumnHeader
            // 
            this.stepColumnHeader.Text = "Step";
            this.stepColumnHeader.Width = 50;
            // 
            // actionTypeColumnHeader
            // 
            this.actionTypeColumnHeader.Text = "Action Type";
            this.actionTypeColumnHeader.Width = 150;
            // 
            // descriptionColumnHeader
            // 
            this.descriptionColumnHeader.Text = "Description";
            this.descriptionColumnHeader.Width = 400;
            // 
            // actionsButtonPanel
            // 
            this.actionsButtonPanel.Controls.Add(this.templatesButton);
            this.actionsButtonPanel.Controls.Add(this.moveDownButton);
            this.actionsButtonPanel.Controls.Add(this.moveUpButton);
            this.actionsButtonPanel.Controls.Add(this.removeActionButton);
            this.actionsButtonPanel.Controls.Add(this.editActionButton);
            this.actionsButtonPanel.Controls.Add(this.addActionButton);
            this.actionsButtonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.actionsButtonPanel.Location = new System.Drawing.Point(671, 3);
            this.actionsButtonPanel.Name = "actionsButtonPanel";
            this.actionsButtonPanel.Size = new System.Drawing.Size(114, 419);
            this.actionsButtonPanel.TabIndex = 1;
            // 
            // addActionButton
            // 
            this.addActionButton.Location = new System.Drawing.Point(3, 3);
            this.addActionButton.Name = "addActionButton";
            this.addActionButton.Size = new System.Drawing.Size(108, 30);
            this.addActionButton.TabIndex = 0;
            this.addActionButton.Text = "Add Action";
            this.addActionButton.UseVisualStyleBackColor = true;
            // 
            // editActionButton
            // 
            this.editActionButton.Location = new System.Drawing.Point(3, 39);
            this.editActionButton.Name = "editActionButton";
            this.editActionButton.Size = new System.Drawing.Size(108, 30);
            this.editActionButton.TabIndex = 1;
            this.editActionButton.Text = "Edit Action";
            this.editActionButton.UseVisualStyleBackColor = true;
            // 
            // removeActionButton
            // 
            this.removeActionButton.Location = new System.Drawing.Point(3, 75);
            this.removeActionButton.Name = "removeActionButton";
            this.removeActionButton.Size = new System.Drawing.Size(108, 30);
            this.removeActionButton.TabIndex = 2;
            this.removeActionButton.Text = "Remove Action";
            this.removeActionButton.UseVisualStyleBackColor = true;
            // 
            // moveUpButton
            // 
            this.moveUpButton.Location = new System.Drawing.Point(3, 120);
            this.moveUpButton.Name = "moveUpButton";
            this.moveUpButton.Size = new System.Drawing.Size(108, 30);
            this.moveUpButton.TabIndex = 3;
            this.moveUpButton.Text = "Move Up";
            this.moveUpButton.UseVisualStyleBackColor = true;
            // 
            // moveDownButton
            // 
            this.moveDownButton.Location = new System.Drawing.Point(3, 156);
            this.moveDownButton.Name = "moveDownButton";
            this.moveDownButton.Size = new System.Drawing.Size(108, 30);
            this.moveDownButton.TabIndex = 4;
            this.moveDownButton.Text = "Move Down";
            this.moveDownButton.UseVisualStyleBackColor = true;
            // 
            // templatesButton
            // 
            this.templatesButton.Location = new System.Drawing.Point(3, 201);
            this.templatesButton.Name = "templatesButton";
            this.templatesButton.Size = new System.Drawing.Size(108, 30);
            this.templatesButton.TabIndex = 5;
            this.templatesButton.Text = "Templates";
            this.templatesButton.UseVisualStyleBackColor = true;
            // 
            // buttonPanel
            // 
            this.buttonPanel.Controls.Add(this.cancelButton);
            this.buttonPanel.Controls.Add(this.okButton);
            this.buttonPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.buttonPanel.Location = new System.Drawing.Point(3, 553);
            this.buttonPanel.Name = "buttonPanel";
            this.buttonPanel.Size = new System.Drawing.Size(794, 44);
            this.buttonPanel.TabIndex = 2;
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
            // SequenceActionDialog
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.mainTableLayoutPanel);
            this.MinimumSize = new System.Drawing.Size(600, 400);
            this.Name = "SequenceActionDialog";
            this.Text = "Configure Sequence Action (Macro)";
            this.mainTableLayoutPanel.ResumeLayout(false);
            this.propertiesGroupBox.ResumeLayout(false);
            this.propertiesTableLayoutPanel.ResumeLayout(false);
            this.propertiesTableLayoutPanel.PerformLayout();
            this.actionsGroupBox.ResumeLayout(false);
            this.actionsTableLayoutPanel.ResumeLayout(false);
            this.actionsButtonPanel.ResumeLayout(false);
            this.buttonPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel mainTableLayoutPanel;
        private System.Windows.Forms.GroupBox propertiesGroupBox;
        private System.Windows.Forms.TableLayoutPanel propertiesTableLayoutPanel;
        private System.Windows.Forms.Label descriptionLabel;
        private System.Windows.Forms.TextBox descriptionTextBox;
        private System.Windows.Forms.Label errorHandlingLabel;
        private System.Windows.Forms.ComboBox errorHandlingComboBox;
        private System.Windows.Forms.GroupBox actionsGroupBox;
        private System.Windows.Forms.TableLayoutPanel actionsTableLayoutPanel;
        private System.Windows.Forms.ListView actionsListView;
        private System.Windows.Forms.ColumnHeader stepColumnHeader;
        private System.Windows.Forms.ColumnHeader actionTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader descriptionColumnHeader;
        private System.Windows.Forms.Panel actionsButtonPanel;
        private System.Windows.Forms.Button addActionButton;
        private System.Windows.Forms.Button editActionButton;
        private System.Windows.Forms.Button removeActionButton;
        private System.Windows.Forms.Button moveUpButton;
        private System.Windows.Forms.Button moveDownButton;
        private System.Windows.Forms.Button templatesButton;
        private System.Windows.Forms.Panel buttonPanel;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Button cancelButton;
    }
}
