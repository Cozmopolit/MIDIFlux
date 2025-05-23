namespace MIDIFlux.GUI.Controls.ProfileEditor
{
    partial class ProfileEditorControl
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
            this.selectChannelsButton = new System.Windows.Forms.Button();
            this.midiChannelsTextBox = new System.Windows.Forms.TextBox();
            this.midiChannelsLabel = new System.Windows.Forms.Label();
            this.deviceNameComboBox = new System.Windows.Forms.ComboBox();
            this.deviceNameLabel = new System.Windows.Forms.Label();
            this.inputProfileTextBox = new System.Windows.Forms.TextBox();
            this.inputProfileLabel = new System.Windows.Forms.Label();
            this.deviceListGroupBox = new System.Windows.Forms.GroupBox();
            this.deviceListView = new System.Windows.Forms.ListView();
            this.inputProfileColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.deviceNameColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.channelsColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.deviceButtonsPanel = new System.Windows.Forms.Panel();
            this.removeDeviceButton = new System.Windows.Forms.Button();
            this.duplicateDeviceButton = new System.Windows.Forms.Button();
            this.addDeviceButton = new System.Windows.Forms.Button();
            this.mappingsPanel = new System.Windows.Forms.Panel();
            this.mappingsGroupBox = new System.Windows.Forms.GroupBox();
            this.mappingsDataGridView = new System.Windows.Forms.DataGridView();
            this.mappingTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.triggerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionTypeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.actionDetailsColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.descriptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.mappingsToolStrip = new System.Windows.Forms.ToolStrip();
            this.addMappingButton = new System.Windows.Forms.ToolStripButton();
            this.editMappingButton = new System.Windows.Forms.ToolStripButton();
            this.deleteMappingButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.filterLabel = new System.Windows.Forms.ToolStripLabel();
            this.filterTextBox = new System.Windows.Forms.ToolStripTextBox();
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
            //
            // splitContainer.Panel2
            //
            this.splitContainer.Panel2.Controls.Add(this.mappingsPanel);
            this.splitContainer.Size = new System.Drawing.Size(800, 600);
            this.splitContainer.SplitterDistance = 250;
            this.splitContainer.TabIndex = 0;
            //
            // previewSplitContainer
            //
            this.previewSplitContainer = new System.Windows.Forms.SplitContainer();
            this.previewSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.previewSplitContainer.Name = "previewSplitContainer";
            this.previewSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            //
            // previewSplitContainer.Panel1
            //
            this.previewSplitContainer.Panel1.Controls.Add(this.mappingsGroupBox);
            //
            // previewSplitContainer.Panel2
            //
            this.previewSplitContainer.Panel2.Controls.Add(this.previewGroupBox);
            this.previewSplitContainer.Panel2MinSize = 100;
            this.previewSplitContainer.Size = new System.Drawing.Size(780, 326);
            this.previewSplitContainer.SplitterDistance = 200;
            this.previewSplitContainer.TabIndex = 1;
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
            this.devicePropertiesGroupBox.Controls.Add(this.selectChannelsButton);
            this.devicePropertiesGroupBox.Controls.Add(this.midiChannelsTextBox);
            this.devicePropertiesGroupBox.Controls.Add(this.midiChannelsLabel);
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
            // selectChannelsButton
            //
            this.selectChannelsButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.selectChannelsButton.Location = new System.Drawing.Point(299, 130);
            this.selectChannelsButton.Name = "selectChannelsButton";
            this.selectChannelsButton.Size = new System.Drawing.Size(75, 23);
            this.selectChannelsButton.TabIndex = 6;
            this.selectChannelsButton.Text = "Select...";
            this.selectChannelsButton.UseVisualStyleBackColor = true;
            //
            // midiChannelsTextBox
            //
            this.midiChannelsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.midiChannelsTextBox.Location = new System.Drawing.Point(6, 130);
            this.midiChannelsTextBox.Name = "midiChannelsTextBox";
            this.midiChannelsTextBox.Size = new System.Drawing.Size(287, 23);
            this.midiChannelsTextBox.TabIndex = 5;
            //
            // midiChannelsLabel
            //
            this.midiChannelsLabel.AutoSize = true;
            this.midiChannelsLabel.Location = new System.Drawing.Point(6, 112);
            this.midiChannelsLabel.Name = "midiChannelsLabel";
            this.midiChannelsLabel.Size = new System.Drawing.Size(86, 15);
            this.midiChannelsLabel.TabIndex = 4;
            this.midiChannelsLabel.Text = "MIDI Channels:";
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
            this.channelsColumnHeader});
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
            // channelsColumnHeader
            //
            this.channelsColumnHeader.Text = "Channels";
            this.channelsColumnHeader.Width = 80;
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
            this.mappingsPanel.Controls.Add(this.previewSplitContainer);
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
            this.mappingsGroupBox.Location = new System.Drawing.Point(0, 0);
            this.mappingsGroupBox.Name = "mappingsGroupBox";
            this.mappingsGroupBox.Size = new System.Drawing.Size(780, 200);
            this.mappingsGroupBox.TabIndex = 0;
            this.mappingsGroupBox.TabStop = false;
            this.mappingsGroupBox.Text = "Mappings";
            //
            // previewGroupBox
            //
            this.previewGroupBox.Controls.Add(this.previewListView);
            this.previewGroupBox.Controls.Add(this.previewToolStrip);
            this.previewGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewGroupBox.Location = new System.Drawing.Point(0, 0);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.Size = new System.Drawing.Size(780, 122);
            this.previewGroupBox.TabIndex = 0;
            this.previewGroupBox.TabStop = false;
            this.previewGroupBox.Text = "Preview Events";
            //
            // previewListView
            //
            this.previewListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.timestampColumnHeader,
            this.eventTypeColumnHeader,
            this.triggerColumnHeader,
            this.actionColumnHeader,
            this.deviceColumnHeader});
            this.previewListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.previewListView.FullRowSelect = true;
            this.previewListView.HideSelection = false;
            this.previewListView.Location = new System.Drawing.Point(3, 44);
            this.previewListView.Name = "previewListView";
            this.previewListView.Size = new System.Drawing.Size(774, 75);
            this.previewListView.TabIndex = 1;
            this.previewListView.UseCompatibleStateImageBehavior = false;
            this.previewListView.View = System.Windows.Forms.View.Details;
            //
            // timestampColumnHeader
            //
            this.timestampColumnHeader.Text = "Time";
            this.timestampColumnHeader.Width = 80;
            //
            // eventTypeColumnHeader
            //
            this.eventTypeColumnHeader.Text = "Event Type";
            this.eventTypeColumnHeader.Width = 100;
            //
            // triggerColumnHeader
            //
            this.triggerColumnHeader.Text = "Trigger";
            this.triggerColumnHeader.Width = 150;
            //
            // actionColumnHeader
            //
            this.actionColumnHeader.Text = "Action";
            this.actionColumnHeader.Width = 200;
            //
            // deviceColumnHeader
            //
            this.deviceColumnHeader.Text = "Device";
            this.deviceColumnHeader.Width = 150;
            //
            // previewToolStrip
            //
            this.previewToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previewModeToggleButton,
            this.clearEventsButton});
            this.previewToolStrip.Location = new System.Drawing.Point(3, 19);
            this.previewToolStrip.Name = "previewToolStrip";
            this.previewToolStrip.Size = new System.Drawing.Size(774, 25);
            this.previewToolStrip.TabIndex = 0;
            this.previewToolStrip.Text = "toolStrip2";
            //
            // previewModeToggleButton
            //
            this.previewModeToggleButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.previewModeToggleButton.Name = "previewModeToggleButton";
            this.previewModeToggleButton.Size = new System.Drawing.Size(109, 22);
            this.previewModeToggleButton.Text = "Enable Preview Mode";
            this.previewModeToggleButton.ToolTipText = "Toggle preview mode";
            //
            // clearEventsButton
            //
            this.clearEventsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.clearEventsButton.Name = "clearEventsButton";
            this.clearEventsButton.Size = new System.Drawing.Size(75, 22);
            this.clearEventsButton.Text = "Clear Events";
            this.clearEventsButton.ToolTipText = "Clear all preview events";
            //
            // mappingsDataGridView
            //
            this.mappingsDataGridView.AllowUserToAddRows = false;
            this.mappingsDataGridView.AllowUserToDeleteRows = false;
            this.mappingsDataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.mappingsDataGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.mappingTypeColumn,
            this.triggerColumn,
            this.actionTypeColumn,
            this.actionDetailsColumn,
            this.descriptionColumn});
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
            this.mappingTypeColumn.DataPropertyName = "MappingType";
            this.mappingTypeColumn.HeaderText = "Type";
            this.mappingTypeColumn.Name = "mappingTypeColumn";
            this.mappingTypeColumn.ReadOnly = true;
            //
            // triggerColumn
            //
            this.triggerColumn.DataPropertyName = "Trigger";
            this.triggerColumn.HeaderText = "Trigger";
            this.triggerColumn.Name = "triggerColumn";
            this.triggerColumn.ReadOnly = true;
            //
            // actionTypeColumn
            //
            this.actionTypeColumn.DataPropertyName = "ActionType";
            this.actionTypeColumn.HeaderText = "Action Type";
            this.actionTypeColumn.Name = "actionTypeColumn";
            this.actionTypeColumn.ReadOnly = true;
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
            // mappingsToolStrip
            //
            this.mappingsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addMappingButton,
            this.editMappingButton,
            this.deleteMappingButton,
            this.toolStripSeparator1,
            this.saveButton,
            this.toolStripSeparator2,
            this.filterLabel,
            this.filterTextBox});
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
            this.saveButton.ToolTipText = "Save profile changes (Ctrl+S)";
            //
            // toolStripSeparator2
            //
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            //
            // filterLabel
            //
            this.filterLabel.Name = "filterLabel";
            this.filterLabel.Size = new System.Drawing.Size(36, 22);
            this.filterLabel.Text = "Filter:";
            //
            // filterTextBox
            //
            this.filterTextBox.Name = "filterTextBox";
            this.filterTextBox.Size = new System.Drawing.Size(200, 25);
            //
            // ProfileEditorControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer);
            this.Name = "ProfileEditorControl";
            this.Size = new System.Drawing.Size(800, 600);
            this.splitContainer.Panel1.ResumeLayout(false);
            this.splitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer)).EndInit();
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
        private System.Windows.Forms.Button selectChannelsButton;
        private System.Windows.Forms.TextBox midiChannelsTextBox;
        private System.Windows.Forms.Label midiChannelsLabel;
        private System.Windows.Forms.ComboBox deviceNameComboBox;
        private System.Windows.Forms.Label deviceNameLabel;
        private System.Windows.Forms.TextBox inputProfileTextBox;
        private System.Windows.Forms.Label inputProfileLabel;
        private System.Windows.Forms.GroupBox deviceListGroupBox;
        private System.Windows.Forms.ListView deviceListView;
        private System.Windows.Forms.ColumnHeader inputProfileColumnHeader;
        private System.Windows.Forms.ColumnHeader deviceNameColumnHeader;
        private System.Windows.Forms.ColumnHeader channelsColumnHeader;
        private System.Windows.Forms.Panel deviceButtonsPanel;
        private System.Windows.Forms.Button removeDeviceButton;
        private System.Windows.Forms.Button duplicateDeviceButton;
        private System.Windows.Forms.Button addDeviceButton;
        private System.Windows.Forms.Panel mappingsPanel;
        private System.Windows.Forms.GroupBox mappingsGroupBox;
        private System.Windows.Forms.DataGridView mappingsDataGridView;
        private System.Windows.Forms.DataGridViewTextBoxColumn mappingTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn triggerColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionTypeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn actionDetailsColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn descriptionColumn;
        private System.Windows.Forms.ToolStrip mappingsToolStrip;
        private System.Windows.Forms.ToolStripButton addMappingButton;
        private System.Windows.Forms.ToolStripButton editMappingButton;
        private System.Windows.Forms.ToolStripButton deleteMappingButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton saveButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel filterLabel;
        private System.Windows.Forms.ToolStripTextBox filterTextBox;
        private System.Windows.Forms.SplitContainer previewSplitContainer;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.ListView previewListView;
        private System.Windows.Forms.ColumnHeader timestampColumnHeader;
        private System.Windows.Forms.ColumnHeader eventTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader triggerColumnHeader;
        private System.Windows.Forms.ColumnHeader actionColumnHeader;
        private System.Windows.Forms.ColumnHeader deviceColumnHeader;
        private System.Windows.Forms.ToolStrip previewToolStrip;
        private System.Windows.Forms.ToolStripButton previewModeToggleButton;
        private System.Windows.Forms.ToolStripButton clearEventsButton;
    }
}

