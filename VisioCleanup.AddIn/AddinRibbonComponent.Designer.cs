namespace VisioCleanup.AddIn;

using System.ComponentModel;

using Microsoft.Office.Core;
using Microsoft.Office.Tools.Ribbon;

using VisioCleanup.AddIn.Properties;

partial class AddinRibbonComponent : RibbonBase
{
    internal RibbonButton Command1;

    internal RibbonGroup Group1;

    internal RibbonTab Tab1;

    internal RibbonButton TogglePanel;

    /// <summary>Required designer variable.</summary>
    private readonly IContainer components = null;

    public AddinRibbonComponent()
        : base(Globals.Factory.GetRibbonFactory())
    {
        this.InitializeComponent();
    }

    /// <summary>Clean up any resources being used.</summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (this.components != null))
        {
            this.components.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Required method for Designer support - do not modify the contents of this method with the code editor.</summary>
    private void InitializeComponent()
    {
        this.Tab1 = this.Factory.CreateRibbonTab();
        this.Group1 = this.Factory.CreateRibbonGroup();
        this.Command1 = this.Factory.CreateRibbonButton();
        this.TogglePanel = this.Factory.CreateRibbonButton();
        this.Tab1.SuspendLayout();
        this.Group1.SuspendLayout();

        // Tab1
        this.Tab1.ControlId.ControlIdType = RibbonControlIdType.Office;
        this.Tab1.ControlId.OfficeId = "TabHome";
        this.Tab1.Groups.Add(this.Group1);
        this.Tab1.Label = "TabHome";
        this.Tab1.Name = "Tab1";

        // Group1
        this.Group1.Items.Add(this.Command1);
        this.Group1.Items.Add(this.TogglePanel);
        this.Group1.Label = "VisioCleanup.AddIn";
        this.Group1.Name = "Group1";

        // Command1
        this.Command1.ControlSize = RibbonControlSize.RibbonControlSizeLarge;
        this.Command1.Image = Resources.Command1;
        this.Command1.Label = "Command 1";
        this.Command1.Name = "Command1";
        this.Command1.ShowImage = true;
        this.Command1.Click += new RibbonControlEventHandler(this.buttonCommand1_Click);

        // TogglePanel
        this.TogglePanel.ControlSize = RibbonControlSize.RibbonControlSizeLarge;
        this.TogglePanel.Image = Resources.TogglePanel;
        this.TogglePanel.Label = "Toggle Panel";
        this.TogglePanel.Name = "TogglePanel";
        this.TogglePanel.ShowImage = true;
        this.TogglePanel.Click += new RibbonControlEventHandler(this.buttonToggle_Click);

        // AddinRibbon
        this.Name = "AddinRibbon";
        this.RibbonType = "Microsoft.Visio.Drawing";
        this.Tabs.Add(this.Tab1);
        this.Tab1.ResumeLayout(false);
        this.Tab1.PerformLayout();
        this.Group1.ResumeLayout(false);
        this.Group1.PerformLayout();
    }
}

internal partial class ThisRibbonCollection
{
    internal AddinRibbonComponent Ribbon => this.GetRibbon<AddinRibbonComponent>();
}