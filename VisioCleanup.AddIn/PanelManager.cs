namespace VisioCleanup.AddIn;

using System;
using System.Collections.Generic;

using Microsoft.Office.Interop.Visio;

/// <summary>Manages the list of all installed panels</summary>
public class PanelManager : IDisposable
{
    private readonly Dictionary<int, PanelFrame> _panelFrames = new();

    public PanelManager(ThisAddIn thisAddIn) => this.ThisAddIn = thisAddIn;

    private ThisAddIn ThisAddIn { get; }

    public void Dispose()
    {
    }

    /// <summary>Returns true if panel is opened in the given Visio diagram window.</summary>
    /// <param name="window">Visio diagram window</param>
    /// <returns></returns>
    public bool IsPanelOpened(Window window) => this.FindWindowPanelFrame(window) != null;

    /// <summary>Shows or hides panel for the given Visio window.</summary>
    /// <param name="window">Target Visio diagram window where to show/hide the panel</param>
    public void TogglePanel(Window window)
    {
        if (window == null)
        {
            return;
        }

        var panelFrame = this.FindWindowPanelFrame(window);
        if (panelFrame == null)
        {
            panelFrame = new PanelFrame(new TheForm(window));
            panelFrame.CreateWindow(window);

            panelFrame.PanelFrameClosed += this.OnPanelFrameClosed;
            this._panelFrames.Add(window.ID, panelFrame);
        }
        else
        {
            panelFrame.DestroyWindow();
            this._panelFrames.Remove(window.ID);
        }
    }

    private PanelFrame FindWindowPanelFrame(Window window)
    {
        if (window == null)
        {
            return null;
        }

        return this._panelFrames.ContainsKey(window.ID) ? this._panelFrames[window.ID] : null;
    }

    private void OnPanelFrameClosed(Window window)
    {
        this._panelFrames.Remove(window.ID);
    }
}