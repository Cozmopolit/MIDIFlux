namespace MIDIFlux.GUI.Controls.ProfileManager
{
    partial class ProfileManagerControl
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
            this.profileTreeView = new System.Windows.Forms.TreeView();
            this.searchTextBox = new System.Windows.Forms.TextBox();
            this.searchLabel = new System.Windows.Forms.Label();
            this.actionsGroupBox = new System.Windows.Forms.GroupBox();
            this.importMidiKey2KeyButton = new System.Windows.Forms.Button();
            this.openFolderButton = new System.Windows.Forms.Button();
            this.refreshButton = new System.Windows.Forms.Button();
            this.activateButton = new System.Windows.Forms.Button();
            this.editButton = new System.Windows.Forms.Button();
            this.deleteButton = new System.Windows.Forms.Button();
            this.duplicateButton = new System.Windows.Forms.Button();
            this.newButton = new System.Windows.Forms.Button();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.actionsGroupBox.SuspendLayout();
            this.statusStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // profileTreeView
            //
            this.profileTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileTreeView.Location = new System.Drawing.Point(16, 45);
            this.profileTreeView.Name = "profileTreeView";
            this.profileTreeView.Size = new System.Drawing.Size(450, 400);
            this.profileTreeView.TabIndex = 0;
            //
            // searchTextBox
            //
            this.searchTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.searchTextBox.Location = new System.Drawing.Point(70, 16);
            this.searchTextBox.Name = "searchTextBox";
            this.searchTextBox.Size = new System.Drawing.Size(396, 23);
            this.searchTextBox.TabIndex = 1;
            //
            // searchLabel
            //
            this.searchLabel.AutoSize = true;
            this.searchLabel.Location = new System.Drawing.Point(16, 19);
            this.searchLabel.Name = "searchLabel";
            this.searchLabel.Size = new System.Drawing.Size(48, 15);
            this.searchLabel.TabIndex = 2;
            this.searchLabel.Text = "Search:";
            //
            // actionsGroupBox
            //
            this.actionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.actionsGroupBox.Controls.Add(this.importMidiKey2KeyButton);
            this.actionsGroupBox.Controls.Add(this.openFolderButton);
            this.actionsGroupBox.Controls.Add(this.refreshButton);
            this.actionsGroupBox.Controls.Add(this.activateButton);
            this.actionsGroupBox.Controls.Add(this.editButton);
            this.actionsGroupBox.Controls.Add(this.deleteButton);
            this.actionsGroupBox.Controls.Add(this.duplicateButton);
            this.actionsGroupBox.Controls.Add(this.newButton);
            this.actionsGroupBox.Location = new System.Drawing.Point(472, 45);
            this.actionsGroupBox.Name = "actionsGroupBox";
            this.actionsGroupBox.Size = new System.Drawing.Size(150, 290);
            this.actionsGroupBox.TabIndex = 3;
            this.actionsGroupBox.TabStop = false;
            this.actionsGroupBox.Text = "Actions";
            //
            // openFolderButton
            //
            this.openFolderButton.Location = new System.Drawing.Point(6, 166);
            this.openFolderButton.Name = "openFolderButton";
            this.openFolderButton.Size = new System.Drawing.Size(138, 23);
            this.openFolderButton.TabIndex = 4;
            this.openFolderButton.Text = "Open Profiles Folder";
            this.openFolderButton.UseVisualStyleBackColor = true;
            //
            // refreshButton
            //
            this.refreshButton.Location = new System.Drawing.Point(6, 224);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(138, 23);
            this.refreshButton.TabIndex = 7;
            this.refreshButton.Text = "Refresh";
            this.refreshButton.UseVisualStyleBackColor = true;
            //
            // importMidiKey2KeyButton
            //
            this.importMidiKey2KeyButton.Location = new System.Drawing.Point(6, 195);
            this.importMidiKey2KeyButton.Name = "importMidiKey2KeyButton";
            this.importMidiKey2KeyButton.Size = new System.Drawing.Size(138, 23);
            this.importMidiKey2KeyButton.TabIndex = 6;
            this.importMidiKey2KeyButton.Text = "Import MIDIKey2Key";
            this.importMidiKey2KeyButton.UseVisualStyleBackColor = true;
            //
            // activateButton
            //
            this.activateButton.Location = new System.Drawing.Point(6, 137);
            this.activateButton.Name = "activateButton";
            this.activateButton.Size = new System.Drawing.Size(138, 23);
            this.activateButton.TabIndex = 3;
            this.activateButton.Text = "Activate";
            this.activateButton.UseVisualStyleBackColor = true;
            //
            // editButton
            //
            this.editButton.Location = new System.Drawing.Point(6, 108);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(138, 23);
            this.editButton.TabIndex = 5;
            this.editButton.Text = "Edit";
            this.editButton.UseVisualStyleBackColor = true;
            //
            // deleteButton
            //
            this.deleteButton.Location = new System.Drawing.Point(6, 80);
            this.deleteButton.Name = "deleteButton";
            this.deleteButton.Size = new System.Drawing.Size(138, 23);
            this.deleteButton.TabIndex = 2;
            this.deleteButton.Text = "Delete";
            this.deleteButton.UseVisualStyleBackColor = true;
            //
            // duplicateButton
            //
            this.duplicateButton.Location = new System.Drawing.Point(6, 51);
            this.duplicateButton.Name = "duplicateButton";
            this.duplicateButton.Size = new System.Drawing.Size(138, 23);
            this.duplicateButton.TabIndex = 1;
            this.duplicateButton.Text = "Duplicate";
            this.duplicateButton.UseVisualStyleBackColor = true;
            //
            // newButton
            //
            this.newButton.Location = new System.Drawing.Point(6, 22);
            this.newButton.Name = "newButton";
            this.newButton.Size = new System.Drawing.Size(138, 23);
            this.newButton.TabIndex = 0;
            this.newButton.Text = "New";
            this.newButton.UseVisualStyleBackColor = true;

            //
            // statusStrip
            //
            this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statusLabel});
            this.statusStrip.Location = new System.Drawing.Point(0, 453);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(638, 22);
            this.statusStrip.TabIndex = 4;
            this.statusStrip.Text = "statusStrip1";
            //
            // statusLabel
            //
            this.statusLabel.Name = "statusLabel";
            this.statusLabel.Size = new System.Drawing.Size(95, 17);
            this.statusLabel.Text = "No active profile";
            //
            // ProfileManagerControl
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.actionsGroupBox);
            this.Controls.Add(this.searchLabel);
            this.Controls.Add(this.searchTextBox);
            this.Controls.Add(this.profileTreeView);
            this.Name = "ProfileManagerControl";
            this.Size = new System.Drawing.Size(638, 475);
            this.actionsGroupBox.ResumeLayout(false);
            this.statusStrip.ResumeLayout(false);
            this.statusStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView profileTreeView;
        private System.Windows.Forms.TextBox searchTextBox;
        private System.Windows.Forms.Label searchLabel;
        private System.Windows.Forms.GroupBox actionsGroupBox;
        private System.Windows.Forms.Button activateButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button duplicateButton;
        private System.Windows.Forms.Button newButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripStatusLabel statusLabel;
        private System.Windows.Forms.Button openFolderButton;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button importMidiKey2KeyButton;
    }
}

