using System.Drawing;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Overlay.ViewModels;

namespace Vcon.Overlay.Services;

/// <summary>
/// System tray icon with context menu for overlay lifecycle management.
/// </summary>
public sealed class TrayIconService : IDisposable
{
    private readonly IProfileManager _profileManager;
    private readonly ILogger<TrayIconService> _logger;

    private NotifyIcon? _notifyIcon;
    private OverlayViewModel? _viewModel;
    private bool _disposed;

    public TrayIconService(IProfileManager profileManager, ILogger<TrayIconService> logger)
    {
        _profileManager = profileManager;
        _logger = logger;
    }

    /// <summary>
    /// Creates the tray icon and context menu wired to the given ViewModel.
    /// </summary>
    public void Initialize(OverlayViewModel viewModel)
    {
        _viewModel = viewModel;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "vcon — Virtual Controller Overlay",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };

        _notifyIcon.DoubleClick += (_, _) => _viewModel.ToggleVisibility();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;

        if (_notifyIcon is not null)
        {
            _notifyIcon.Visible = false;
            _notifyIcon.Dispose();
        }
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();

        var showHideItem = new ToolStripMenuItem("Show/Hide Overlay");
        showHideItem.Click += (_, _) => _viewModel?.ToggleVisibility();
        menu.Items.Add(showHideItem);

        var editModeItem = new ToolStripMenuItem("Edit Mode");
        editModeItem.Click += (_, _) => _viewModel?.ToggleEditMode();
        menu.Items.Add(editModeItem);

        var profilesItem = new ToolStripMenuItem("Profiles");
        menu.Items.Add(profilesItem);
        menu.Opening += async (_, _) => await RefreshProfilesSubmenu(profilesItem);

        menu.Items.Add(new ToolStripSeparator());

        var exitItem = new ToolStripMenuItem("Exit");
        exitItem.Click += (_, _) =>
        {
            _logger.LogInformation("Exit requested from tray menu");
            Application.Current?.Dispatcher.Invoke(() => Application.Current.Shutdown());
        };
        menu.Items.Add(exitItem);

        return menu;
    }

    private async Task RefreshProfilesSubmenu(ToolStripMenuItem profilesItem)
    {
        try
        {
            profilesItem.DropDownItems.Clear();
            var profiles = await _profileManager.GetAllProfilesAsync();
            var activeId = _profileManager.ActiveProfile.Id;

            foreach (var profile in profiles)
            {
                var item = new ToolStripMenuItem(profile.Name)
                {
                    Checked = profile.Id == activeId,
                    Tag = profile.Id,
                };

                item.Click += async (sender, _) =>
                {
                    if (sender is ToolStripMenuItem mi && mi.Tag is string id)
                        await _viewModel!.SwitchProfileAsync(id);
                };

                profilesItem.DropDownItems.Add(item);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh profiles submenu");
        }
    }
}
