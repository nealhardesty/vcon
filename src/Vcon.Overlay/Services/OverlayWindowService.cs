using System.Runtime.InteropServices;

namespace Vcon.Overlay.Services;

/// <summary>
/// Win32 interop service for applying and maintaining overlay window styles.
/// </summary>
public sealed class OverlayWindowService
{
    private const int GWL_EXSTYLE = -20;

    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_TOPMOST = 0x00000008;

    private static readonly nint HWND_TOPMOST = -1;

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    /// <summary>
    /// Applies WS_EX_NOACTIVATE, WS_EX_TOOLWINDOW, and WS_EX_TOPMOST extended styles to the window.
    /// </summary>
    public void ApplyOverlayStyles(nint hwnd)
    {
        var current = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        var desired = current | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, desired);
    }

    /// <summary>
    /// Re-asserts topmost Z-order if another window has covered the overlay.
    /// </summary>
    public void EnsureTopmost(nint hwnd)
    {
        SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(nint hWnd, nint hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);
}
