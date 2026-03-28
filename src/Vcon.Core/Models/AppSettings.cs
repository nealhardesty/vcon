namespace Vcon.Core.Models;

/// <summary>
/// Global application settings persisted to settings.json.
/// </summary>
public sealed class AppSettings
{
    /// <summary>ID of the currently active controller profile.</summary>
    public string ActiveProfileId { get; set; } = "xbox-standard";

    /// <summary>Global hotkey bindings.</summary>
    public HotkeySettings Hotkeys { get; set; } = new();

    /// <summary>Whether the app starts minimized to the system tray.</summary>
    public bool StartMinimized { get; set; }

    /// <summary>Whether the app launches at Windows startup.</summary>
    public bool StartWithWindows { get; set; }

    /// <summary>Minimum log level (e.g. "Debug", "Information", "Warning", "Error").</summary>
    public string LogLevel { get; set; } = "Warning";
}

/// <summary>
/// Configurable global hotkey bindings.
/// </summary>
public sealed class HotkeySettings
{
    /// <summary>Hotkey to show/hide the overlay.</summary>
    public string ToggleOverlay { get; set; } = "F9";

    /// <summary>Hotkey to toggle layout edit mode.</summary>
    public string ToggleEditMode { get; set; } = "F10";

    /// <summary>Hotkey to cycle to the next profile.</summary>
    public string CycleProfile { get; set; } = "F11";
}
