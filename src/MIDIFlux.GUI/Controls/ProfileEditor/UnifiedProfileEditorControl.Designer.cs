namespace MIDIFlux.GUI.Controls.ProfileEditor
{
    partial class UnifiedProfileEditorControl
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

        #region Component Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer = new System.Windows.Forms.SplitContainer();
            this.devicePanel = new System.Windows.Forms.Panel();
            this.devicePropertiesGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceNameComboBox = new System.Windows.Forms.ComboBox();
            this.deviceNameLabel = new System.Windows.Forms.Label();
            this.inputProfileTextBox = new System.Windows.Forms.TextBox();
            this.inputProfileLabel = new System.Windows.Forms.Label();
            this.deviceListGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceListView = new System.Windows.Forms.ListView();
            this.inputProfileColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.deviceNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.mappingCountColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.deviceButtonsPanel = new System.Windows.Forms.Panel();
            this.removeDeviceButton = new System.Windows.Forms.Button();
            this.duplicateDeviceButton = new System.Windows.Forms.Button();
            this.addDeviceButton = new System.Windows.Forms.Button();
            this.mappingsPanel = new System.Windows.Forms.Panel();
            this.mappingsGroupBox = new System.Windows.Forms.GroupBox();
            this.mappingsDataGridView = new System.Windows.Forms.DataGridView();
            this.mappingTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.triggerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.channelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.deviceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionDetailsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.enabledColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.mappingsToolStrip = new System.Windows.Forms.ToolStrip();
            this.addMappingButton = new System.Windows.Forms.ToolStripButton();
            this.editMappingButton = new System.Windows.Forms.ToolStripButton();
            this.deleteMappingButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveButton = new System.Windows.Forms.ToolStripButton();

            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).BeginInit();
            this.splitContainer.Panel1.SuspendLayout();
            this.splitContainer.Panel2.SuspendLayout();
            this.splitContainer.SuspendLayout();
            this.devicePanel.SuspendLayout();
            this.devicePropertiesGroupBox.SuspendLayout();
            this.deviceListGroupBox.SuspendLayout();
            this.deviceButtonsPanel.SuspendLayout();
            this.mappingsPanel.SuspendLayout();
            this.mappingsGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingsDataGridView)).BeginInit();
            this.mappingsToolStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // splitContainer
            //
            this.splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer.Location = new System.Drawing.Point(0, 0);
            this.splitContainer.Name = "splitContainer";
            this.splitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // splitContainer.Panel1
            //
            this.splitContainer.Panel1.Controls.Add(this.devicePanel);
            this.splitContainer.Panel1Collapsed = true;
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.mappingsPanel);
            this.splitContainer.Size = new System.Drawing.Size(800, 600);
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.TabIndex = 0;
            //
            // devicePanel
            //
            this.devicePanel.Controls.Add(this.devicePropertiesGroupBox);
            this.devicePanel.Controls.Add(this.deviceListGroupBox);
            this.devicePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicePanel.Location = new System.Drawing.Point(0, 0);
            this.devicePanel.Name = "devicePanel";
            this.devicePanel.Padding = new System.Windows.Forms.Padding(10);
            this.devicePanel.Size = new System.Drawing.Size(800, 250);
            this.devicePanel.TabIndex = 0;
            //
            // devicePropertiesGroupBox
            //
            this.devicePropertiesGroupBox.Controls.Add(this.deviceNameComboBox);
            this.devicePropertiesGroupBox.Controls.Add(this.deviceNameLabel);
            this.devicePropertiesGroupBox.Controls.Add(this.inputProfileTextBox);
            this.devicePropertiesGroupBox.Controls.Add(this.inputProfileLabel);
            this.devicePropertiesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.devicePropertiesGroupBox.Location = new System.Drawing.Point(410, 10);
            this.devicePropertiesGroupBox.Name = "devicePropertiesGroupBox";
            this.devicePropertiesGroupBox.Size = new System.Drawing.Size(380, 230);
            this.devicePropertiesGroupBox.TabIndex = 1;
            this.devicePropertiesGroupBox.TabStop = false;
            this.devicePropertiesGroupBox.Text = "Device Properties";
            //
            // deviceNameComboBox
            //
            this.deviceNameComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceNameComboBox.FormattingEnabled = true;
            this.deviceNameComboBox.Location = new System.Drawing.Point(6, 85);
            this.deviceNameComboBox.Name = "deviceNameComboBox";
            this.deviceNameComboBox.Size = new System.Drawing.Size(368, 23);
            this.deviceNameComboBox.TabIndex = 3;
            //
            // deviceNameLabel
            //
            this.deviceNameLabel.AutoSize = true;
            this.deviceNameLabel.Location = new System.Drawing.Point(6, 67);
            this.deviceNameLabel.Name = "deviceNameLabel";
            this.deviceNameLabel.Size = new System.Drawing.Size(81, 15);
            this.deviceNameLabel.TabIndex = 2;
            this.deviceNameLabel.Text = "Device Name:";
            //
            // inputProfileTextBox
            //
            this.inputProfileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.inputProfileTextBox.Location = new System.Drawing.Point(6, 40);
            this.inputProfileTextBox.Name = "inputProfileTextBox";
            this.inputProfileTextBox.Size = new System.Drawing.Size(368, 23);
            this.inputProfileTextBox.TabIndex = 1;
            //
            // inputProfileLabel
            //
            this.inputProfileLabel.AutoSize = true;
            this.inputProfileLabel.Location = new System.Drawing.Point(6, 22);
            this.inputProfileLabel.Name = "inputProfileLabel";
            this.inputProfileLabel.Size = new System.Drawing.Size(77, 15);
            this.inputProfileLabel.TabIndex = 0;
            this.inputProfileLabel.Text = "Input Profile:";
            //
            // deviceListGroupBox
            //
            this.deviceListGroupBox.Controls.Add(this.deviceListView);
            this.deviceListGroupBox.Controls.Add(this.deviceButtonsPanel);
            this.deviceListGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
            this.deviceListGroupBox.Location = new System.Drawing.Point(10, 10);
            this.deviceListGroupBox.Name = "deviceListGroupBox";
            this.deviceListGroupBox.Size = new System.Drawing.Size(400, 230);
            this.deviceListGroupBox.TabIndex = 0;
            this.deviceListGroupBox.TabStop = false;
            this.deviceListGroupBox.Text = "Device List";
            //
            // deviceListView
            //
            this.deviceListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.inputProfileColumnHeader,
            this.deviceNameColumnHeader,
            this.mappingCountColumnHeader});
            this.deviceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.deviceListView.FullRowSelect = true;
            this.deviceListView.HideSelection = false;
            this.deviceListView.Location = new System.Drawing.Point(3, 19);
            this.deviceListView.MultiSelect = false;
            this.deviceListView.Name = "deviceListView";
            this.deviceListView.Size = new System.Drawing.Size(394, 173);
            this.deviceListView.TabIndex = 1;
            this.deviceListView.UseCompatibleStateImageBehavior = false;
            this.deviceListView.View = System.Windows.Forms.View.Details;
            //
            // inputProfileColumnHeader
            //
            this.inputProfileColumnHeader.Text = "Input Profile";
            this.inputProfileColumnHeader.Width = 150;
            //
            // deviceNameColumnHeader
            //
            this.deviceNameColumnHeader.Text = "Device Name";
            this.deviceNameColumnHeader.Width = 150;
            //
            // mappingCountColumnHeader
            //
            this.mappingCountColumnHeader.Text = "Mappings";
            this.mappingCountColumnHeader.Width = 80;
            //
            // deviceButtonsPanel
            //
            this.deviceButtonsPanel.Controls.Add(this.removeDeviceButton);
            this.deviceButtonsPanel.Controls.Add(this.duplicateDeviceButton);
            this.deviceButtonsPanel.Controls.Add(this.addDeviceButton);
            this.deviceButtonsPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.deviceButtonsPanel.Location = new System.Drawing.Point(3, 192);
            this.deviceButtonsPanel.Name = "deviceButtonsPanel";
            this.deviceButtonsPanel.Size = new System.Drawing.Size(394, 35);
            this.deviceButtonsPanel.TabIndex = 0;
            //
            // removeDeviceButton
            //
            this.removeDeviceButton.Location = new System.Drawing.Point(168, 6);
            this.removeDeviceButton.Name = "removeDeviceButton";
            this.removeDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.removeDeviceButton.TabIndex = 2;
            this.removeDeviceButton.Text = "Remove";
            this.removeDeviceButton.UseVisualStyleBackColor = true;
            //
            // duplicateDeviceButton
            //
            this.duplicateDeviceButton.Location = new System.Drawing.Point(87, 6);
            this.duplicateDeviceButton.Name = "duplicateDeviceButton";
            this.duplicateDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.duplicateDeviceButton.TabIndex = 1;
            this.duplicateDeviceButton.Text = "Duplicate";
            this.duplicateDeviceButton.UseVisualStyleBackColor = true;
            //
            // addDeviceButton
            //
            this.addDeviceButton.Location = new System.Drawing.Point(6, 6);
            this.addDeviceButton.Name = "addDeviceButton";
            this.addDeviceButton.Size = new System.Drawing.Size(75, 23);
            this.addDeviceButton.TabIndex = 0;
            this.addDeviceButton.Text = "Add";
            this.addDeviceButton.UseVisualStyleBackColor = true;
            //
            // mappingsPanel
            //
            this.mappingsPanel.Controls.Add(this.mappingsGroupBox);
            this.mappingsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mappingsPanel.Location = new System.Drawing.Point(0, 0);
            this.mappingsPanel.Name = "mappingsPanel";
            this.mappingsPanel.Padding = new System.Windows.Forms.Padding(10);
            this.mappingsPanel.Size = new System.Drawing.Size(800, 346);
            this.mappingsPanel.TabIndex = 0;
            //
            // mappingsGroupBox
            //
            this.mappingsGroupBox.Controls.Add(this.mappingsDataGridView);
            this.mappingsGroupBox.Controls.Add(this.mappingsToolStrip);
            this.mappingsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mappingsGroupBox.Location = new System.Drawing.Point(10, 10);
            this.mappingsGroupBox.Name = "mappingsGroupBox";
            this.mappingsGroupBox.Size = new System.Drawing.Size(780, 326);
            this.mappingsGroupBox.TabIndex = 0;
            this.mappingsGroupBox.TabStop = false;
            this.mappingsGroupBox.Text = "Mappings";
            //
            // mappingsDataGridView
            //
            this.mappingsDataGridView.AllowUserToAddRows = false;
            this.mappingsDataGridView.AllowUserToDeleteRows = false;
            this.mappingsDataGridView.AutoGenerateColumns = false;
            this.mappingsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mappingsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.mappingTypeColumn,
            this.triggerColumn,
            this.channelColumn,
            this.deviceColumn,
            this.actionTypeColumn,
            this.actionDetailsColumn,
            this.descriptionColumn,
            this.enabledColumn});
            this.mappingsDataGridView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mappingsDataGridView.Location = new System.Drawing.Point(3, 44);
            this.mappingsDataGridView.Name = "mappingsDataGridView";
            this.mappingsDataGridView.ReadOnly = true;
            this.mappingsDataGridView.RowTemplate.Height = 25;
            this.mappingsDataGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.mappingsDataGridView.Size = new System.Drawing.Size(774, 279);
            this.mappingsDataGridView.TabIndex = 1;
            //
            // mappingTypeColumn
            //
            this.mappingTypeColumn.DataPropertyName = "Type";
            this.mappingTypeColumn.HeaderText = "Type";
            this.mappingTypeColumn.Name = "mappingTypeColumn";
            this.mappingTypeColumn.ReadOnly = true;
            this.mappingTypeColumn.Width = 80;
            //
            // triggerColumn
            //
            this.triggerColumn.DataPropertyName = "Trigger";
            this.triggerColumn.HeaderText = "Trigger";
            this.triggerColumn.Name = "triggerColumn";
            this.triggerColumn.ReadOnly = true;
            this.triggerColumn.Width = 80;
            //
            // channelColumn
            //
            this.channelColumn.DataPropertyName = "Channel";
            this.channelColumn.HeaderText = "Channel";
            this.channelColumn.Name = "channelColumn";
            this.channelColumn.ReadOnly = true;
            this.channelColumn.Width = 60;
            //
            // deviceColumn
            //
            this.deviceColumn.DataPropertyName = "Device";
            this.deviceColumn.HeaderText = "Device";
            this.deviceColumn.Name = "deviceColumn";
            this.deviceColumn.ReadOnly = true;
            this.deviceColumn.Width = 100;
            //
            // actionTypeColumn
            //
            this.actionTypeColumn.DataPropertyName = "ActionType";
            this.actionTypeColumn.HeaderText = "Action Type";
            this.actionTypeColumn.Name = "actionTypeColumn";
            this.actionTypeColumn.ReadOnly = true;
            this.actionTypeColumn.Width = 120;
            //
            // actionDetailsColumn
            //
            this.actionDetailsColumn.DataPropertyName = "ActionDetails";
            this.actionDetailsColumn.HeaderText = "Action Details";
            this.actionDetailsColumn.Name = "actionDetailsColumn";
            this.actionDetailsColumn.ReadOnly = true;
            this.actionDetailsColumn.Width = 150;
            //
            // descriptionColumn
            //
            this.descriptionColumn.DataPropertyName = "Description";
            this.descriptionColumn.HeaderText = "Description";
            this.descriptionColumn.Name = "descriptionColumn";
            this.descriptionColumn.ReadOnly = true;
            this.descriptionColumn.Width = 200;
            //
            // enabledColumn
            //
            this.enabledColumn.DataPropertyName = "Enabled";
            this.enabledColumn.HeaderText = "Enabled";
            this.enabledColumn.Name = "enabledColumn";
            this.enabledColumn.ReadOnly = true;
            this.enabledColumn.Width = 60;
            //
            // mappingsToolStrip
            //
            this.mappingsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMappingButton,
            this.editMappingButton,
            this.deleteMappingButton,
            this.toolStripSeparator1,
            this.saveButton});
            this.mappingsToolStrip.Location = new System.Drawing.Point(3, 19);
            this.mappingsToolStrip.Name = "mappingsToolStrip";
            this.mappingsToolStrip.Size = new System.Drawing.Size(774, 25);
            this.mappingsToolStrip.TabIndex = 0;
            this.mappingsToolStrip.Text = "toolStrip1";
            //
            // addMappingButton
            //
            this.addMappingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.addMappingButton.Name = "addMappingButton";
            this.addMappingButton.Size = new System.Drawing.Size(33, 22);
            this.addMappingButton.Text = "Add";
            this.addMappingButton.ToolTipText = "Add a new mapping";
            //
            // editMappingButton
            //
            this.editMappingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.editMappingButton.Name = "editMappingButton";
            this.editMappingButton.Size = new System.Drawing.Size(31, 22);
            this.editMappingButton.Text = "Edit";
            this.editMappingButton.ToolTipText = "Edit the selected mapping";
            //
            // deleteMappingButton
            //
            this.deleteMappingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.deleteMappingButton.Name = "deleteMappingButton";
            this.deleteMappingButton.Size = new System.Drawing.Size(44, 22);
            this.deleteMappingButton.Text = "Delete";
            this.deleteMappingButton.ToolTipText = "Delete the selected mapping";
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            //
            // saveButton
            //
            this.saveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.saveButton.Name = "saveButton";
            this.saveButton.Size = new System.Drawing.Size(35, 22);
            this.saveButton.Text = "Save";
            this.saveButton.ToolTipText = "Save the configuration";

            //
            // UnifiedProfileEditorControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "UnifiedProfileEditorControl";
            this.Size = new System.Drawing.Size(800, 600);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            this.splitContainer.ResumeLayout(false);
            this.devicePanel.ResumeLayout(false);
            this.devicePropertiesGroupBox.ResumeLayout(false);
            this.devicePropertiesGroupBox.PerformLayout();
            this.deviceListGroupBox.ResumeLayout(false);
            this.deviceButtonsPanel.ResumeLayout(false);
            this.mappingsPanel.ResumeLayout(false);
            this.mappingsGroupBox.ResumeLayout(false);
            this.mappingsGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.mappingsDataGridView)).EndInit();
            this.mappingsToolStrip.ResumeLayout(false);
            this.mappingsToolStrip.PerformLayout();
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer;
        private System.Windows.Forms.Panel devicePanel;
        private System.Windows.Forms.GroupBox devicePropertiesGroupBox;
        private System.Windows.Forms.ComboBox deviceNameComboBox;
        private System.Windows.Forms.Label deviceNameLabel;
        private System.Windows.Forms.TextBox inputProfileTextBox;
        private System.Windows.Forms.Label inputProfileLabel;
        private System.Windows.Forms.GroupBox deviceListGroupBox;
        private System.Windows.Forms.ListView deviceListView;
        private System.Windows.Forms.ColumnHeader inputProfileColumnHeader;
        private System.Windows.Forms.ColumnHeader deviceNameColumnHeader;
        private System.Windows.Forms.ColumnHeader mappingCountColumnHeader;
        private System.Windows.Forms.Panel deviceButtonsPanel;
        private System.Windows.Forms.Button removeDeviceButton;
        private System.Windows.Forms.Button duplicateDeviceButton;
        private System.Windows.Forms.Button addDeviceButton;
        private System.Windows.Forms.Panel mappingsPanel;
        private System.Windows.Forms.GroupBox mappingsGroupBox;
        private System.Windows.Forms.DataGridView mappingsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn mappingTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn triggerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn channelColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn deviceColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionDetailsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
        private System.Windows.Forms.DataGridViewCheckBoxColumn enabledColumn;
        private System.Windows.Forms.ToolStrip mappingsToolStrip;
        private System.Windows.Forms.ToolStripButton addMappingButton;
        private System.Windows.Forms.ToolStripButton editMappingButton;
        private System.Windows.Forms.ToolStripButton deleteMappingButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton saveButton;
    }
}
