namespace VisioCleanup.AddIn;

using Microsoft.Office.Tools.Ribbon;

public partial class AddinRibbonComponent
{
    private void buttonCommand1_Click(object sender, RibbonControlEventArgs e)
    {
        Globals.ThisAddIn.Command1();
    }

    private void buttonToggle_Click(object sender, RibbonControlEventArgs e)
    {
        Globals.ThisAddIn.TogglePanel();
    }
}