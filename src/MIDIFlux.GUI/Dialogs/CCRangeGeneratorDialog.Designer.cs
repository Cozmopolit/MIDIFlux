namespace MIDIFlux.GUI.Dialogs
{
    partial class CCRangeGeneratorDialog
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
            this.keySequenceGroupBox = new System.Windows.Forms.GroupBox();
            this.keySequenceTextBox = new System.Windows.Forms.TextBox();
            this.keySequenceLabel = new System.Windows.Forms.Label();
            this.valueRangeGroupBox = new System.Windows.Forms.GroupBox();
            this.maxValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.maxValueLabel = new System.Windows.Forms.Label();
            this.minValueNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.minValueLabel = new System.Windows.Forms.Label();
            this.previewGroupBox = new System.Windows.Forms.GroupBox();
            this.previewListView = new System.Windows.Forms.ListView();
            this.rangeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionTypeColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.actionDetailsColumnHeader = new System.Windows.Forms.ColumnHeader();
            this.cancelButton = new System.Windows.Forms.Button();
            this.generateButton = new System.Windows.Forms.Button();
            this.keySequenceGroupBox.SuspendLayout();
            this.valueRangeGroupBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).BeginInit();
            this.previewGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // keySequenceGroupBox
            // 
            this.keySequenceGroupBox.Controls.Add(this.keySequenceTextBox);
            this.keySequenceGroupBox.Controls.Add(this.keySequenceLabel);
            this.keySequenceGroupBox.Location = new System.Drawing.Point(12, 12);
            this.keySequenceGroupBox.Name = "keySequenceGroupBox";
            this.keySequenceGroupBox.Size = new System.Drawing.Size(460, 80);
            this.keySequenceGroupBox.TabIndex = 0;
            this.keySequenceGroupBox.TabStop = false;
            this.keySequenceGroupBox.Text = "Key Sequence";
            // 
            // keySequenceTextBox
            // 
            this.keySequenceTextBox.Location = new System.Drawing.Point(120, 22);
            this.keySequenceTextBox.Name = "keySequenceTextBox";
            this.keySequenceTextBox.Size = new System.Drawing.Size(325, 23);
            this.keySequenceTextBox.TabIndex = 1;
            this.keySequenceTextBox.Text = "1234567890";
            // 
            // keySequenceLabel
            // 
            this.keySequenceLabel.AutoSize = true;
            this.keySequenceLabel.Location = new System.Drawing.Point(20, 25);
            this.keySequenceLabel.Name = "keySequenceLabel";
            this.keySequenceLabel.Size = new System.Drawing.Size(84, 15);
            this.keySequenceLabel.TabIndex = 0;
            this.keySequenceLabel.Text = "Key Sequence:";
            // 
            // valueRangeGroupBox
            // 
            this.valueRangeGroupBox.Controls.Add(this.maxValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.maxValueLabel);
            this.valueRangeGroupBox.Controls.Add(this.minValueNumericUpDown);
            this.valueRangeGroupBox.Controls.Add(this.minValueLabel);
            this.valueRangeGroupBox.Location = new System.Drawing.Point(12, 98);
            this.valueRangeGroupBox.Name = "valueRangeGroupBox";
            this.valueRangeGroupBox.Size = new System.Drawing.Size(460, 80);
            this.valueRangeGroupBox.TabIndex = 1;
            this.valueRangeGroupBox.TabStop = false;
            this.valueRangeGroupBox.Text = "Value Range";
            // 
            // maxValueNumericUpDown
            // 
            this.maxValueNumericUpDown.Location = new System.Drawing.Point(120, 51);
            this.maxValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.maxValueNumericUpDown.Name = "maxValueNumericUpDown";
            this.maxValueNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.maxValueNumericUpDown.TabIndex = 3;
            this.maxValueNumericUpDown.Value = new decimal(new int[] {
            127,
            0,
            0,
            0});
            // 
            // maxValueLabel
            // 
            this.maxValueLabel.AutoSize = true;
            this.maxValueLabel.Location = new System.Drawing.Point(20, 53);
            this.maxValueLabel.Name = "maxValueLabel";
            this.maxValueLabel.Size = new System.Drawing.Size(65, 15);
            this.maxValueLabel.TabIndex = 2;
            this.maxValueLabel.Text = "Max Value:";
            // 
            // minValueNumericUpDown
            // 
            this.minValueNumericUpDown.Location = new System.Drawing.Point(120, 22);
            this.minValueNumericUpDown.Maximum = new decimal(new int[] {
            127,
            0,
            0,
            0});
            this.minValueNumericUpDown.Name = "minValueNumericUpDown";
            this.minValueNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.minValueNumericUpDown.TabIndex = 1;
            // 
            // minValueLabel
            // 
            this.minValueLabel.AutoSize = true;
            this.minValueLabel.Location = new System.Drawing.Point(20, 24);
            this.minValueLabel.Name = "minValueLabel";
            this.minValueLabel.Size = new System.Drawing.Size(63, 15);
            this.minValueLabel.TabIndex = 0;
            this.minValueLabel.Text = "Min Value:";
            // 
            // previewGroupBox
            // 
            this.previewGroupBox.Controls.Add(this.previewListView);
            this.previewGroupBox.Location = new System.Drawing.Point(12, 184);
            this.previewGroupBox.Name = "previewGroupBox";
            this.previewGroupBox.Size = new System.Drawing.Size(460, 200);
            this.previewGroupBox.TabIndex = 2;
            this.previewGroupBox.TabStop = false;
            this.previewGroupBox.Text = "Preview";
            // 
            // previewListView
            // 
            this.previewListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.rangeColumnHeader,
            this.actionTypeColumnHeader,
            this.actionDetailsColumnHeader});
            this.previewListView.FullRowSelect = true;
            this.previewListView.Location = new System.Drawing.Point(20, 22);
            this.previewListView.Name = "previewListView";
            this.previewListView.Size = new System.Drawing.Size(425, 172);
            this.previewListView.TabIndex = 0;
            this.previewListView.UseCompatibleStateImageBehavior = false;
            this.previewListView.View = System.Windows.Forms.View.Details;
            // 
            // rangeColumnHeader
            // 
            this.rangeColumnHeader.Text = "Range";
            this.rangeColumnHeader.Width = 100;
            // 
            // actionTypeColumnHeader
            // 
            this.actionTypeColumnHeader.Text = "Action Type";
            this.actionTypeColumnHeader.Width = 100;
            // 
            // actionDetailsColumnHeader
            // 
            this.actionDetailsColumnHeader.Text = "Action Details";
            this.actionDetailsColumnHeader.Width = 220;
            // 
            // cancelButton
            // 
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancelButton.Location = new System.Drawing.Point(397, 390);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(75, 23);
            this.cancelButton.TabIndex = 4;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // generateButton
            // 
            this.generateButton.Location = new System.Drawing.Point(316, 390);
            this.generateButton.Name = "generateButton";
            this.generateButton.Size = new System.Drawing.Size(75, 23);
            this.generateButton.TabIndex = 3;
            this.generateButton.Text = "Generate";
            this.generateButton.UseVisualStyleBackColor = true;
            // 
            // CCRangeGeneratorDialog
            // 
            this.AcceptButton = this.generateButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(484, 425);
            this.Controls.Add(this.generateButton);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.previewGroupBox);
            this.Controls.Add(this.valueRangeGroupBox);
            this.Controls.Add(this.keySequenceGroupBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CCRangeGeneratorDialog";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate CC Value Ranges";
            this.keySequenceGroupBox.ResumeLayout(false);
            this.keySequenceGroupBox.PerformLayout();
            this.valueRangeGroupBox.ResumeLayout(false);
            this.valueRangeGroupBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.maxValueNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.minValueNumericUpDown)).EndInit();
            this.previewGroupBox.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox keySequenceGroupBox;
        private System.Windows.Forms.TextBox keySequenceTextBox;
        private System.Windows.Forms.Label keySequenceLabel;
        private System.Windows.Forms.GroupBox valueRangeGroupBox;
        private System.Windows.Forms.NumericUpDown maxValueNumericUpDown;
        private System.Windows.Forms.Label maxValueLabel;
        private System.Windows.Forms.NumericUpDown minValueNumericUpDown;
        private System.Windows.Forms.Label minValueLabel;
        private System.Windows.Forms.GroupBox previewGroupBox;
        private System.Windows.Forms.ListView previewListView;
        private System.Windows.Forms.ColumnHeader rangeColumnHeader;
        private System.Windows.Forms.ColumnHeader actionTypeColumnHeader;
        private System.Windows.Forms.ColumnHeader actionDetailsColumnHeader;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button generateButton;
    }
}
