namespace MIDIFlux.GUI.Forms;

partial class ConfigurationForm
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
        if (disposing && (components != null))
        {
            components.Dispose();
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
        this.tabControl = new System.Windows.Forms.TabControl();
        this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
        this.trayContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
        this.statusStrip = new System.Windows.Forms.StatusStrip();
        this.statusLabel = new System.Windows.Forms.ToolStripStatusLabel();
        this.statusStrip.SuspendLayout();
        this.SuspendLayout();
        //
        // tabControl
        //
        this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabControl.Location = new System.Drawing.Point(0, 0);
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new System.Drawing.Size(1024, 746);
        this.tabControl.TabIndex = 0;
        this.tabControl.SelectedIndexChanged += new System.EventHandler(this.TabControl_SelectedIndexChanged);
        //
        // notifyIcon
        //
        this.notifyIcon.Text = "MIDIFlux";
        this.notifyIcon.Visible = true;
        this.notifyIcon.DoubleClick += new System.EventHandler(this.NotifyIcon_DoubleClick);
        //
        // trayContextMenu
        //
        this.trayContextMenu.Name = "trayContextMenu";
        this.trayContextMenu.Size = new System.Drawing.Size(61, 4);
        this.trayContextMenu.Opening += new System.ComponentModel.CancelEventHandler(this.TrayContextMenu_Opening);
        //
        // statusStrip
        //
        this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
        this.statusLabel});
        this.statusStrip.Location = new System.Drawing.Point(0, 746);
        this.statusStrip.Name = "statusStrip";
        this.statusStrip.Size = new System.Drawing.Size(1024, 22);
        this.statusStrip.TabIndex = 1;
        this.statusStrip.Text = "statusStrip1";
        //
        // statusLabel
        //
        this.statusLabel.Name = "statusLabel";
        this.statusLabel.Size = new System.Drawing.Size(39, 17);
        this.statusLabel.Text = "Ready";
        //
        // ConfigurationForm
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1024, 768);
        this.Controls.Add(this.tabControl);
        this.Controls.Add(this.statusStrip);
        this.MinimumSize = new System.Drawing.Size(800, 600);
        this.Name = "ConfigurationForm";
        this.Text = "MIDIFlux Configuration";
        this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
        this.Load += new System.EventHandler(this.MainForm_Load);
        this.Resize += new System.EventHandler(this.MainForm_Resize);
        this.statusStrip.ResumeLayout(false);
        this.statusStrip.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
    }

    #endregion

    private System.Windows.Forms.TabControl tabControl;
    private System.Windows.Forms.NotifyIcon notifyIcon;
    private System.Windows.Forms.ContextMenuStrip trayContextMenu;
    private System.Windows.Forms.StatusStrip statusStrip;
    private System.Windows.Forms.ToolStripStatusLabel statusLabel;
}

