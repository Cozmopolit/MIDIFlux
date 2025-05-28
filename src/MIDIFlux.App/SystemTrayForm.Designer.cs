namespace MIDIFlux.App;

partial class SystemTrayForm
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up the MIDI Input Detection dialog if it's open
            if (_midiInputDetectionDialog != null && !_midiInputDetectionDialog.IsDisposed)
            {
                _midiInputDetectionDialog.Close();
                _midiInputDetectionDialog.Dispose();
                _midiInputDetectionDialog = null;
            }

            // Clean up the notify icon
            _notifyIcon?.Dispose();

            // Clean up the context menu
            _contextMenu?.Dispose();

            // Clean up components
            if (components != null)
            {
                components.Dispose();
            }
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Text = "MIDIFlux";
    }

    #endregion
}
