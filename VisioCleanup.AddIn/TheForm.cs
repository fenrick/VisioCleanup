namespace VisioCleanup.AddIn;

using System;
using System.Windows.Forms;

using Microsoft.Office.Interop.Visio;

public partial class TheForm : Form
{
    private readonly Window _window;

    /// <summary>Form constructor, receives parent Visio diagram window</summary>
    /// <param name="window">Parent Visio diagram window</param>
    public TheForm(Window window)
    {
        this._window = window;
        this.InitializeComponent();
    }

    /// <summary>Sample method. We just show a Message Box. Do something meaningful here instead.</summary>
    private void button1_Click(object sender, EventArgs e)
    {
        MessageBox.Show(this._window.Document.Name);
    }
}