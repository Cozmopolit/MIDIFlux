namespace MIDIFlux.GUI.Dialogs
{
    partial class MidiKey2KeyImportDialog
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
            this.filePathLabel = new System.Windows.Forms.Label();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.browseButton = new System.Windows.Forms.Button();
            this.profileNameLabel = new System.Windows.Forms.Label();
            this.profileNameTextBox = new System.Windows.Forms.TextBox();
            this.importButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.instructionsLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // filePathLabel
            // 
            this.filePathLabel.AutoSize = true;
            this.filePathLabel.Location = new System.Drawing.Point(12, 50);
            this.filePathLabel.Name = "filePathLabel";
            this.filePathLabel.Size = new System.Drawing.Size(129, 15);
            this.filePathLabel.TabIndex = 0;
            this.filePathLabel.Text = "MIDIKey2Key INI File:";
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.filePathTextBox.Location = new System.Drawing.Point(12, 68);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.ReadOnly = true;
            this.filePathTextBox.Size = new System.Drawing.Size(400, 23);
            this.filePathTextBox.TabIndex = 1;
            this.filePathTextBox.TextChanged += new System.EventHandler(this.FilePathTextBox_TextChanged);
            // 
            // browseButton
            // 
            this.browseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.browseButton.Location = new System.Drawing.Point(418, 68);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(75, 23);
            this.browseButton.TabIndex = 2;
            this.browseButton.Text = "Browse...";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // profileNameLabel
            // 
            this.profileNameLabel.AutoSize = true;
            this.profileNameLabel.Location = new System.Drawing.Point(12, 105);
            this.profileNameLabel.Name = "profileNameLabel";
            this.profileNameLabel.Size = new System.Drawing.Size(79, 15);
            this.profileNameLabel.TabIndex = 3;
            this.profileNameLabel.Text = "Profile Name:";
            // 
            // profileNameTextBox
            // 
            this.profileNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.profileNameTextBox.Location = new System.Drawing.Point(12, 123);
            this.profileNameTextBox.Name = "profileNameTextBox";
            this.profileNameTextBox.Size = new System.Drawing.Size(481, 23);
            this.profileNameTextBox.TabIndex = 4;
            this.profileNameTextBox.TextChanged += new System.EventHandler(this.ProfileNameTextBox_TextChanged);
            // 
            // importButton
            // 
            this.importButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.importButton.Enabled = false;
            this.importButton.Location = new System.Drawing.Point(337, 165);
            this.importButton.Name = "importButton";
            this.importButton.Size = new System.Drawing.Size(75, 23);
            this.importButton.TabIndex = 5;
            this.importButton.Text = "Import";
            this.importButton.UseVisualStyleBackColor = true;
            this.importButton.Click += new System.EventHandler(this.ImportButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(418, 165);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 6;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // instructionsLabel
            // 
            this.instructionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.instructionsLabel.Location = new System.Drawing.Point(12, 9);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new System.Drawing.Size(481, 32);
            this.instructionsLabel.TabIndex = 7;
            this.instructionsLabel.Text = "Select a MIDIKey2Key configuration file (.ini) to import into MIDIFlux. The conf" +
    "iguration will be converted to MIDIFlux format.";
            // 
            // MidiKey2KeyImportDialog
            // 
            this.AcceptButton = this.importButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(505, 200);
            this.Controls.Add(this.instructionsLabel);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.importButton);
            this.Controls.Add(this.profileNameTextBox);
            this.Controls.Add(this.profileNameLabel);
            this.Controls.Add(this.browseButton);
            this.Controls.Add(this.filePathTextBox);
            this.Controls.Add(this.filePathLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MidiKey2KeyImportDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Import MIDIKey2Key Configuration";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label filePathLabel;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.Label profileNameLabel;
        private System.Windows.Forms.TextBox profileNameTextBox;
        private System.Windows.Forms.Button importButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Label instructionsLabel;
    }
}
