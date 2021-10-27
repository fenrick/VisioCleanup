namespace VisioCleanup.AddIn;

using System;
using System.Windows.Forms;

public partial class ThisAddIn
{
    private PanelManager _panelManager;

    /// <summary>A simple command</summary>
    public void Command1()
    {
        MessageBox.Show("Hello from command 1!", "VisioCleanup.AddIn");
    }

    public void TogglePanel()
    {
        this._panelManager.TogglePanel(this.Application.ActiveWindow);
    }

    /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
    private void InternalStartup()
    {
        this.Startup += this.ThisAddIn_Startup;
        this.Shutdown += this.ThisAddIn_Shutdown;
    }

    private void ThisAddIn_Shutdown(object sender, EventArgs e)
    {
        this._panelManager.Dispose();
    }

    private void ThisAddIn_Startup(object sender, EventArgs e)
    {
        this._panelManager = new PanelManager(this);
    }
}