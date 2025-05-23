namespace MIDIFlux.GUI.Dialogs
{
    partial class MidiInputDetectionDialog
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
            this.deviceLabel = new System.Windows.Forms.Label();
            this.deviceComboBox = new System.Windows.Forms.ComboBox();
            this.listenButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.eventsListView = new System.Windows.Forms.ListView();
            this.timestampColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.eventTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.detailsColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.channelColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.rawDataColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.statusLabel = new System.Windows.Forms.Label();
            this.copyButton = new System.Windows.Forms.Button();
            this.closeButton = new System.Windows.Forms.Button();
            this.listenAllCheckBox = new System.Windows.Forms.CheckBox();
            this.clearButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // deviceLabel
            // 
            this.deviceLabel.AutoSize = true;
            this.deviceLabel.Location = new System.Drawing.Point(12, 15);
            this.deviceLabel.Name = "deviceLabel";
            this.deviceLabel.Size = new System.Drawing.Size(78, 15);
            this.deviceLabel.TabIndex = 0;
            this.deviceLabel.Text = "MIDI Device:";
            // 
            // deviceComboBox
            // 
            this.deviceComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.deviceComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.deviceComboBox.FormattingEnabled = true;
            this.deviceComboBox.Location = new System.Drawing.Point(96, 12);
            this.deviceComboBox.Name = "deviceComboBox";
            this.deviceComboBox.Size = new System.Drawing.Size(400, 23);
            this.deviceComboBox.TabIndex = 1;
            this.deviceComboBox.SelectedIndexChanged += new System.EventHandler(this.DeviceComboBox_SelectedIndexChanged);
            // 
            // listenButton
            // 
            this.listenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.listenButton.Location = new System.Drawing.Point(502, 11);
            this.listenButton.Name = "listenButton";
            this.listenButton.Size = new System.Drawing.Size(100, 23);
            this.listenButton.TabIndex = 2;
            this.listenButton.Text = "Start Listening";
            this.listenButton.UseVisualStyleBackColor = true;
            this.listenButton.Click += new System.EventHandler(this.ListenButton_Click);
            // 
            // refreshButton
            // 
            this.refreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.refreshButton.Location = new System.Drawing.Point(608, 11);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(80, 23);
            this.refreshButton.TabIndex = 3;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // eventsListView
            // 
            this.eventsListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.eventsListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.timestampColumnHeader,
            this.eventTypeColumnHeader,
            this.detailsColumnHeader,
            this.channelColumnHeader,
            this.rawDataColumnHeader});
            this.eventsListView.FullRowSelect = true;
            this.eventsListView.GridLines = true;
            this.eventsListView.Location = new System.Drawing.Point(12, 70);
            this.eventsListView.MultiSelect = false;
            this.eventsListView.Name = "eventsListView";
            this.eventsListView.Size = new System.Drawing.Size(676, 368);
            this.eventsListView.TabIndex = 5;
            this.eventsListView.UseCompatibleStateImageBehavior = false;
            this.eventsListView.View = System.Windows.Forms.View.Details;
            this.eventsListView.SelectedIndexChanged += new System.EventHandler(this.EventsListView_SelectedIndexChanged);
            // 
            // timestampColumnHeader
            // 
            this.timestampColumnHeader.Text = "Timestamp";
            this.timestampColumnHeader.Width = 100;
            // 
            // eventTypeColumnHeader
            // 
            this.eventTypeColumnHeader.Text = "Event Type";
            this.eventTypeColumnHeader.Width = 100;
            // 
            // detailsColumnHeader
            // 
            this.detailsColumnHeader.Text = "Details";
            this.detailsColumnHeader.Width = 200;
            // 
            // channelColumnHeader
            // 
            this.channelColumnHeader.Text = "Channel";
            this.channelColumnHeader.Width = 80;
            // 
            // rawDataColumnHeader
            // 
            this.rawDataColumnHeader.Text = "Raw Data";
            this.rawDataColumnHeader.Width = 180;
            // 
            // statusLabel
            // 
            this.statusLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusLabel.Location = new System.Drawing.Point(12, 447);
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(484, 23);
            this.statusLabel.TabIndex = 6;
            this.statusLabel.Text = "Not listening";
            this.statusLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // copyButton
            // 
            this.copyButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.copyButton.Enabled = false;
            this.copyButton.Location = new System.Drawing.Point(502, 447);
            this.copyButton.Name = "copyButton";
            this.copyButton.Size = new System.Drawing.Size(100, 23);
            this.copyButton.TabIndex = 7;
            this.copyButton.Text = "Copy to Mapping";
            this.copyButton.UseVisualStyleBackColor = true;
            this.copyButton.Click += new System.EventHandler(this.CopyButton_Click);
            // 
            // closeButton
            // 
            this.closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.closeButton.Location = new System.Drawing.Point(608, 447);
            this.closeButton.Name = "closeButton";
            this.closeButton.Size = new System.Drawing.Size(80, 23);
            this.closeButton.TabIndex = 8;
            this.closeButton.Text = "Close";
            this.closeButton.UseVisualStyleBackColor = true;
            this.closeButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // listenAllCheckBox
            // 
            this.listenAllCheckBox.AutoSize = true;
            this.listenAllCheckBox.Location = new System.Drawing.Point(12, 41);
            this.listenAllCheckBox.Name = "listenAllCheckBox";
            this.listenAllCheckBox.Size = new System.Drawing.Size(267, 19);
            this.listenAllCheckBox.TabIndex = 4;
            this.listenAllCheckBox.Text = "Listen on all system MIDI devices (ignore profile)";
            this.listenAllCheckBox.UseVisualStyleBackColor = true;
            this.listenAllCheckBox.CheckedChanged += new System.EventHandler(this.ListenAllCheckBox_CheckedChanged);
            // 
            // clearButton
            // 
            this.clearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.clearButton.Location = new System.Drawing.Point(608, 41);
            this.clearButton.Name = "clearButton";
            this.clearButton.Size = new System.Drawing.Size(80, 23);
            this.clearButton.TabIndex = 9;
            this.clearButton.Text = "Clear";
            this.clearButton.UseVisualStyleBackColor = true;
            this.clearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // MidiInputDetectionDialog
            // 
            this.AcceptButton = this.copyButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.closeButton;
            this.ClientSize = new System.Drawing.Size(700, 480);
            this.Controls.Add(this.clearButton);
            this.Controls.Add(this.listenAllCheckBox);
            this.Controls.Add(this.closeButton);
            this.Controls.Add(this.copyButton);
            this.Controls.Add(this.statusLabel);
            this.Controls.Add(this.eventsListView);
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.listenButton);
            this.Controls.Add(this.deviceComboBox);
            this.Controls.Add(this.deviceLabel);
            this.Name = "MidiInputDetectionDialog";
            this.Text = "MIDI Input Detection";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label deviceLabel;
        private System.Windows.Forms.ComboBox deviceComboBox;
        private System.Windows.Forms.Button listenButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ListView eventsListView;
        private System.Windows.Forms.ColumnHeader timestampColumnHeader;
        private System.Windows.Forms.ColumnHeader eventTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader detailsColumnHeader;
        private System.Windows.Forms.ColumnHeader channelColumnHeader;
        private System.Windows.Forms.ColumnHeader rawDataColumnHeader;
        private System.Windows.Forms.Label statusLabel;
        private System.Windows.Forms.Button copyButton;
        private System.Windows.Forms.Button closeButton;
        private System.Windows.Forms.CheckBox listenAllCheckBox;
        private System.Windows.Forms.Button clearButton;
    }
}
