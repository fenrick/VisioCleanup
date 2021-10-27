namespace VisioCleanup.AddIn;

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Microsoft.Office.Interop.Visio;

/// <summary>
///     Integrates a winform in Visio. Creates an anchor window for the given diagram window, and installs the
///     specified form as a child in that panel.
/// </summary>
public sealed class PanelFrame : IVisEventProc
{
    private const string AddonWindowMergeId = "bdfa4a55-e530-4b6d-83d8-7f41c4ec1109";

    private const int GW_CHILD = 5;

    private const int GW_HWNDNEXT = 2;

    private const int GWL_EXSTYLE = -20;

    private const int GWL_STYLE = -16;

    private const int SWP_NOCOPYBITS = 0x100;

    private const int SWP_NOMOVE = 0x2;

    private const int SWP_NOZORDER = 0x4;

    private const int WS_CHILD = 0x40000000;

    private const int WS_EX_COMPOSITED = 0x02000000;

    private const int WS_OVERLAPPED = 0x00000000;

    private Form _form;

    private Window _visioWindow;

    /// <summary>Constructs a new panel frame.</summary>
    /// <param name="form">The form to install</param>
    public PanelFrame(Form form) => this._form = form;

    /// <summary>The event is triggered when user closes the panel using "x" button</summary>
    /// <param name="window">The parent diagram window for which the panel was closed.</param>
    public delegate void PanelFrameClosedEventHandler(Window window);

    public event PanelFrameClosedEventHandler PanelFrameClosed;

    /// <summary>Install the panel into given window (actually creates the form and shows it)</summary>
    /// <param name="visioParentWindow">The parent Visio window where the panel should be installed to.</param>
    /// <returns></returns>
    public Window CreateWindow(Window visioParentWindow)
    {
        Window retVal = null;

        try
        {
            if (visioParentWindow == null)
            {
                return null;
            }

            if (this._form != null)
            {
                this._visioWindow = visioParentWindow.Windows.Add(
                    this._form.Text,
                    (int) VisWindowStates.visWSDockedRight | (int) VisWindowStates.visWSAnchorMerged
                                                           | (int) VisWindowStates.visWSVisible,
                    VisWinTypes.visAnchorBarAddon,
                    0,
                    0,
                    300,
                    300,
                    AddonWindowMergeId,
                    string.Empty,
                    0);

                this._visioWindow.BeforeWindowClosed += this.OnBeforeWindowClosed;

                var parentWindowHandle = (IntPtr) this._visioWindow.WindowHandle32;

                SetWindowLong(this._form.Handle, GWL_STYLE, WS_CHILD);
                SetWindowLong(this._form.Handle, GWL_EXSTYLE, WS_EX_COMPOSITED);
                SetParent(this._form.Handle, parentWindowHandle);

                this._form.Show();

                JiggleWindow(parentWindowHandle);

                this._visioWindow.Activate();

                retVal = this._visioWindow;
            }
        }
        catch (Exception ex)
        {
            Debug.Write(ex.Message);
        }

        return retVal;
    }

    /// <summary>Destroys the panel frame along with the form.</summary>
    public void DestroyWindow()
    {
        try
        {
            if ((this._visioWindow != null) && (this._form != null))
            {
                this._form.Hide();

                SetWindowLong(this._form.Handle, GWL_STYLE, WS_OVERLAPPED);
                SetParent(this._form.Handle, (IntPtr) 0);

                this._visioWindow.Close();
                this._visioWindow = null;
            }

            if (this._form != null)
            {
                this._form.Close();
                this._form.Dispose();
                this._form = null;
            }
        }

        // ReSharper disable once EmptyGeneralCatchClause : ignore all errors on exit
        catch
        {
        }
    }

    object IVisEventProc.VisEventProc(
        short nEventCode,
        object pSourceObj,
        int nEventId,
        int nEventSeqNum,
        object pSubjectObj,
        object vMoreInfo)
    {
        object returnValue = false;

        try
        {
            var subjectWindow = pSubjectObj as Window;
            switch (nEventCode)
            {
                case (short) VisEventCodes.visEvtDel + (short) VisEventCodes.visEvtWindow:
                    {
                        this.OnBeforeWindowClosed(subjectWindow);
                        break;
                    }
            }
        }
        catch (Exception ex)
        {
            Debug.Write(ex.Message);
        }

        return returnValue;
    }

    [DllImport(
        "user32.dll",
        EntryPoint = "GetWindowRect",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.Winapi)]
    private static extern int GetWindowRect(IntPtr hWnd, ref RECT lpRect);

    private static void JiggleWindow(IntPtr handle)
    {
        var lpRect = new RECT();
        GetWindowRect(handle, ref lpRect);

        var l = lpRect.left;
        var T = lpRect.top;
        var w = lpRect.right - lpRect.left;
        var h = lpRect.bottom - lpRect.top;

        const int flags = SWP_NOCOPYBITS | SWP_NOMOVE | SWP_NOZORDER;
        SetWindowPos(handle, new IntPtr(0), l, T, w, h + 1, flags);
        SetWindowPos(handle, new IntPtr(0), l, T, w, h, flags);
    }

    [DllImport(
        "user32.dll",
        EntryPoint = "SetParent",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.Winapi)]
    private static extern int SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport(
        "user32.dll",
        EntryPoint = "SetWindowLong",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.Winapi)]
    private static extern int SetWindowLong(IntPtr hWnd, int index, int newLong);

    [DllImport(
        "user32.dll",
        EntryPoint = "SetWindowPos",
        CharSet = CharSet.Auto,
        CallingConvention = CallingConvention.Winapi)]
    private static extern int SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        int wFlags);

    private void OnBeforeWindowClosed(Window visioWindow)
    {
        if (this.PanelFrameClosed != null)
        {
            this.PanelFrameClosed(this._visioWindow.ParentWindow);
        }

        this.DestroyWindow();
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct RECT
    {
        public readonly int left;

        public readonly int top;

        public readonly int right;

        public readonly int bottom;
    }
}