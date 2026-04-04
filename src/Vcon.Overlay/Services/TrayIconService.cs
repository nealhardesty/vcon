using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;
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
    private ToolStripMenuItem? _installDriverItem;
    private ToolStripMenuItem? _xInputModeItem;
    private ToolStripMenuItem? _keyboardModeItem;
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
            Text = "vcon - Virtual Controller Overlay",
            Visible = true,
            ContextMenuStrip = BuildContextMenu(),
        };

        _notifyIcon.DoubleClick += (_, _) => _viewModel.ToggleVisibility();

        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        UpdateInstallDriverVisibility(_viewModel.EmulatorStatus);
        UpdateInputModeChecks();
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

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(OverlayViewModel.EmulatorStatus))
        {
            var status = _viewModel!.EmulatorStatus;
            ShowEmulatorStatusBalloon(status);
            UpdateInstallDriverVisibility(status);
        }

        if (e.PropertyName is nameof(OverlayViewModel.ActiveProfile) or nameof(OverlayViewModel.EmulatorStatus))
            UpdateInputModeChecks();
    }

    private void UpdateInputModeChecks()
    {
        var mode = _viewModel?.ActiveProfile?.Mode;
        if (_xInputModeItem is not null)
            _xInputModeItem.Checked = mode == InputMode.XInput;
        if (_keyboardModeItem is not null)
            _keyboardModeItem.Checked = mode == InputMode.Keyboard;
    }

    private void UpdateInstallDriverVisibility(EmulatorConnectionStatus status)
    {
        if (_installDriverItem is not null)
            _installDriverItem.Visible = status == EmulatorConnectionStatus.DriverUnavailable;
    }

    private void ShowEmulatorStatusBalloon(EmulatorConnectionStatus status)
    {
        if (_notifyIcon is null)
            return;

        switch (status)
        {
            case EmulatorConnectionStatus.DriverUnavailable:
                _notifyIcon.ShowBalloonTip(
                    5000,
                    "vcon - Driver Not Found",
                    "ViGEmBus driver is not installed - Xbox 360 controller emulation disabled.\n"
                    + "Install from https://github.com/nefarius/ViGEmBus/releases",
                    ToolTipIcon.Warning);
                break;

            case EmulatorConnectionStatus.Failed:
                _notifyIcon.ShowBalloonTip(
                    5000,
                    "vcon - Controller Connection Failed",
                    "Failed to connect the virtual controller. Check logs for details.",
                    ToolTipIcon.Error);
                break;
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

        menu.Items.Add(new ToolStripSeparator());

        _xInputModeItem = new ToolStripMenuItem("Xbox Controller Mode");
        _xInputModeItem.Click += async (_, _) => await (_viewModel?.SwitchInputModeAsync(InputMode.XInput) ?? Task.CompletedTask);
        menu.Items.Add(_xInputModeItem);

        _keyboardModeItem = new ToolStripMenuItem("Keyboard Mode");
        _keyboardModeItem.Click += async (_, _) => await (_viewModel?.SwitchInputModeAsync(InputMode.Keyboard) ?? Task.CompletedTask);
        menu.Items.Add(_keyboardModeItem);

        menu.Items.Add(new ToolStripSeparator());

        var profilesItem = new ToolStripMenuItem("Profiles");
        menu.Items.Add(profilesItem);
        menu.Opening += async (_, _) =>
        {
            UpdateInputModeChecks();
            await RefreshProfilesSubmenu(profilesItem);
        };

        _installDriverItem = new ToolStripMenuItem("Install ViGEmBus Driver...");
        _installDriverItem.Click += async (_, _) => await DownloadAndInstallViGEmBusAsync();
        _installDriverItem.Visible = false;
        menu.Items.Add(_installDriverItem);

        menu.Items.Add(new ToolStripSeparator());

        var controllersItem = new ToolStripMenuItem("Game Controllers...");
        controllersItem.Click += (_, _) =>
        {
            try
            {
                Process.Start(new ProcessStartInfo("joy.cpl") { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open Game Controllers");
            }
        };
        menu.Items.Add(controllersItem);

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

    private async Task DownloadAndInstallViGEmBusAsync()
    {
        if (_installDriverItem is null || _notifyIcon is null)
            return;

        _installDriverItem.Enabled = false;

        string? installerPath = null;
        try
        {
            _logger.LogInformation("ViGEmBus install: checking latest release...");
            Balloon("Checking for latest ViGEmBus release...", ToolTipIcon.Info);

            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            http.DefaultRequestHeaders.UserAgent.ParseAdd("vcon");

            string json;
            try
            {
                json = await http.GetStringAsync(
                    "https://api.github.com/repos/nefarius/ViGEmBus/releases/latest");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to reach GitHub API");
                Balloon("Could not reach GitHub. Check your internet connection.", ToolTipIcon.Error);
                return;
            }
            catch (TaskCanceledException)
            {
                _logger.LogError("GitHub API request timed out");
                Balloon("Request to GitHub timed out. Try again later.", ToolTipIcon.Error);
                return;
            }

            using var doc = JsonDocument.Parse(json);
            string? downloadUrl = null;
            string? fileName = null;

            foreach (var asset in doc.RootElement.GetProperty("assets").EnumerateArray())
            {
                var name = asset.GetProperty("name").GetString() ?? "";
                _logger.LogDebug("ViGEmBus release asset: {Name}", name);

                if (name.Contains("ViGEmBus", StringComparison.OrdinalIgnoreCase)
                    && (name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)
                        || name.EndsWith(".msi", StringComparison.OrdinalIgnoreCase)))
                {
                    downloadUrl = asset.GetProperty("browser_download_url").GetString();
                    fileName = name;
                    break;
                }
            }

            if (downloadUrl is null || fileName is null)
            {
                _logger.LogError("No ViGEmBus installer asset found in latest release");
                Balloon("Could not find a ViGEmBus installer in the latest release.", ToolTipIcon.Error);
                return;
            }

            _logger.LogInformation("ViGEmBus install: downloading {FileName} from {Url}", fileName, downloadUrl);
            Balloon($"Downloading {fileName}...", ToolTipIcon.Info);
            installerPath = Path.Combine(Path.GetTempPath(), fileName);

            try
            {
                await using var stream = await http.GetStreamAsync(downloadUrl);
                await using var file = File.Create(installerPath);
                await stream.CopyToAsync(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to download ViGEmBus installer from {Url}", downloadUrl);
                Balloon("Download failed. Check your internet connection and try again.", ToolTipIcon.Error);
                return;
            }

            _logger.LogInformation("ViGEmBus install: download complete, launching installer");
            Balloon("Download complete. Launching installer (approve the UAC prompt)...", ToolTipIcon.Info);

            Process? process;
            try
            {
                process = Process.Start(new ProcessStartInfo
                {
                    FileName = installerPath,
                    Verb = "runas",
                    UseShellExecute = true,
                });
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == 1223)
            {
                _logger.LogInformation("User cancelled ViGEmBus UAC prompt");
                Balloon("Installation cancelled.", ToolTipIcon.Warning);
                return;
            }

            if (process is null)
            {
                _logger.LogError("Installer process did not start");
                Balloon("Could not start the installer.", ToolTipIcon.Error);
                return;
            }

            await process.WaitForExitAsync();
            _logger.LogInformation("ViGEmBus installer exited with code {Code}", process.ExitCode);

            if (process.ExitCode == 0)
            {
                _viewModel?.ConnectEmulator();

                if (_viewModel?.EmulatorStatus == EmulatorConnectionStatus.Connected)
                {
                    Balloon("ViGEmBus installed - virtual controller connected!", ToolTipIcon.Info);
                }
                else
                {
                    Balloon(
                        "ViGEmBus installed, but the controller could not connect yet. "
                        + "You may need to restart vcon or your PC.",
                        ToolTipIcon.Warning);
                }
            }
            else
            {
                Balloon(
                    $"Installer exited with code {process.ExitCode}. "
                    + "Try downloading manually from https://github.com/nefarius/ViGEmBus/releases",
                    ToolTipIcon.Error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download/install ViGEmBus");
            Balloon("Something went wrong. Check logs for details.", ToolTipIcon.Error);
        }
        finally
        {
            if (installerPath is not null)
            {
                try { File.Delete(installerPath); }
                catch { /* best-effort cleanup */ }
            }

            _installDriverItem.Enabled = true;
        }
    }

    private void Balloon(string text, ToolTipIcon icon)
    {
        _notifyIcon?.ShowBalloonTip(5000, "vcon", text, icon);
    }

    private async Task RefreshProfilesSubmenu(ToolStripMenuItem profilesItem)
    {
        try
        {
            profilesItem.DropDownItems.Clear();
            var profiles = await _profileManager.GetAllProfilesAsync();
            var activeId = _profileManager.ActiveProfile.Id;
            var isEditing = _viewModel?.Editor.IsEditing ?? false;

            // --- Profile list ---
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

            // --- Edit actions (visible only while editing) ---
            profilesItem.DropDownItems.Add(new ToolStripSeparator());

            var saveItem = new ToolStripMenuItem("Save Profile")
            {
                Visible = isEditing,
            };
            saveItem.Click += async (_, _) =>
            {
                if (_viewModel is not null)
                    await _viewModel.Editor.SaveAndStopAsync();
            };
            profilesItem.DropDownItems.Add(saveItem);

            var discardItem = new ToolStripMenuItem("Discard Changes")
            {
                Visible = isEditing,
            };
            discardItem.Click += async (_, _) =>
            {
                if (_viewModel is not null)
                    await _viewModel.Editor.DiscardAndStopAsync();
            };
            profilesItem.DropDownItems.Add(discardItem);

            // --- Profile management ---
            profilesItem.DropDownItems.Add(new ToolStripSeparator());

            var cloneItem = new ToolStripMenuItem("Clone Active Profile...");
            cloneItem.Click += async (_, _) =>
            {
                try
                {
                    var clone = await _profileManager.CloneProfileAsync(activeId);
                    await _viewModel!.SwitchProfileAsync(clone.Id);
                    Balloon($"Cloned to \"{clone.Name}\"", ToolTipIcon.Info);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to clone profile");
                    Balloon("Failed to clone profile.", ToolTipIcon.Error);
                }
            };
            profilesItem.DropDownItems.Add(cloneItem);

            var openDirItem = new ToolStripMenuItem("Open Profiles Directory");
            openDirItem.Click += (_, _) =>
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = _profileManager.ProfilesDirectory,
                        UseShellExecute = true,
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open profiles directory");
                }
            };
            profilesItem.DropDownItems.Add(openDirItem);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh profiles submenu");
        }
    }
}
