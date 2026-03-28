using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Vcon.Core.Models;

namespace Vcon.Overlay.Services;

/// <summary>
/// Registers and dispatches global hotkeys via Win32 RegisterHotKey/UnregisterHotKey.
/// </summary>
public sealed class GlobalHotkeyService : IDisposable
{
    private const int WM_HOTKEY = 0x0312;
    private const int HotkeyIdToggleOverlay = 1;
    private const int HotkeyIdToggleEditMode = 2;
    private const int HotkeyIdCycleProfile = 3;

    private nint _hwnd;
    private HwndSource? _hwndSource;
    private bool _disposed;

    /// <summary>Raised when the toggle-overlay hotkey is pressed.</summary>
    public event EventHandler? ToggleOverlayRequested;

    /// <summary>Raised when the toggle-edit-mode hotkey is pressed.</summary>
    public event EventHandler? ToggleEditModeRequested;

    /// <summary>Raised when the cycle-profile hotkey is pressed.</summary>
    public event EventHandler? CycleProfileRequested;

    /// <summary>
    /// Initializes the service with the window for message processing and registers hotkeys.
    /// </summary>
    public void Initialize(Window window, HotkeySettings settings)
    {
        var helper = new WindowInteropHelper(window);
        _hwnd = helper.Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        RegisterFromSetting(HotkeyIdToggleOverlay, settings.ToggleOverlay);
        RegisterFromSetting(HotkeyIdToggleEditMode, settings.ToggleEditMode);
        RegisterFromSetting(HotkeyIdCycleProfile, settings.CycleProfile);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_hwnd == 0)
            return;

        UnregisterHotKey(_hwnd, HotkeyIdToggleOverlay);
        UnregisterHotKey(_hwnd, HotkeyIdToggleEditMode);
        UnregisterHotKey(_hwnd, HotkeyIdCycleProfile);

        _hwndSource?.RemoveHook(WndProc);
    }

    private nint WndProc(nint hwnd, int msg, nint wParam, nint lParam, ref bool handled)
    {
        if (msg != WM_HOTKEY)
            return 0;

        var id = wParam.ToInt32();

        switch (id)
        {
            case HotkeyIdToggleOverlay:
                ToggleOverlayRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
                break;
            case HotkeyIdToggleEditMode:
                ToggleEditModeRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
                break;
            case HotkeyIdCycleProfile:
                CycleProfileRequested?.Invoke(this, EventArgs.Empty);
                handled = true;
                break;
        }

        return 0;
    }

    private void RegisterFromSetting(int id, string keyName)
    {
        if (!TryParseHotkey(keyName, out uint modifiers, out uint vk))
            return;

        RegisterHotKey(_hwnd, id, modifiers, vk);
    }

    private static bool TryParseHotkey(string keyName, out uint modifiers, out uint vk)
    {
        modifiers = 0;
        vk = 0;

        if (string.IsNullOrWhiteSpace(keyName))
            return false;

        if (Enum.TryParse<Key>(keyName, ignoreCase: true, out var key))
        {
            vk = (uint)KeyInterop.VirtualKeyFromKey(key);
            return vk != 0;
        }

        return false;
    }

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RegisterHotKey(nint hWnd, int id, uint fsModifiers, uint vk);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnregisterHotKey(nint hWnd, int id);
}
