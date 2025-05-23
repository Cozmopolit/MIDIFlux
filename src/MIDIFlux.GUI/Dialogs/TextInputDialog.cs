using System;
using System.Windows.Forms;

namespace MIDIFlux.GUI.Dialogs
{
    /// <summary>
    /// Dialog for getting text input from the user
    /// </summary>
    public partial class TextInputDialog : Form
    {
        /// <summary>
        /// Gets or sets the input text
        /// </summary>
        public string InputText { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextInputDialog"/> class
        /// </summary>
        /// <param name="title">The dialog title</param>
        /// <param name="prompt">The prompt text</param>
        /// <param name="defaultText">The default text</param>
        public TextInputDialog(string title, string prompt, string defaultText = "")
        {
            InitializeComponent();

            // Set the dialog properties
            Text = title;
            promptLabel.Text = prompt;
            inputTextBox.Text = defaultText;
            InputText = defaultText;

            // Select all text in the text box
            inputTextBox.SelectAll();
        }

        /// <summary>
        /// Handles the Click event of the OK button
        /// </summary>
        private void okButton_Click(object? sender, EventArgs e)
        {
            // Get the input text
            InputText = inputTextBox.Text.Trim();

            // Validate the input
            if (string.IsNullOrWhiteSpace(InputText))
            {
                MessageBox.Show("Please enter a value", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                inputTextBox.Focus();
                return;
            }

            // Close the dialog with OK result
            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the Cancel button
        /// </summary>
        private void cancelButton_Click(object? sender, EventArgs e)
        {
            // Close the dialog with Cancel result
            DialogResult = DialogResult.Cancel;
            Close();
        }

        /// <summary>
        /// Handles the TextChanged event of the input text box
        /// </summary>
        private void inputTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Update the input text
            InputText = inputTextBox.Text.Trim();
        }
    }
}

