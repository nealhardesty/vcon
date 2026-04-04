using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Configuration;
using Vcon.Core.Models;

namespace Vcon.Overlay.ViewModels;

/// <summary>
/// ViewModel for the layout editor: dragging, resizing, snapshot/restore, and profile locking.
/// </summary>
public partial class EditorViewModel : ObservableObject
{
    private const string LockedProfileId = "xbox-standard";

    private readonly IProfileManager _profileManager;
    private readonly ILogger<EditorViewModel> _logger;

    private string? _profileSnapshot;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _showGrid;

    [ObservableProperty]
    private string? _selectedControlId;

    /// <summary>Raised after a discard so the overlay can reload layout from the restored profile.</summary>
    public event EventHandler? LayoutReloadRequested;

    public EditorViewModel(IProfileManager profileManager, ILogger<EditorViewModel> logger)
    {
        _profileManager = profileManager;
        _logger = logger;
    }

    /// <summary>
    /// Enters layout edit mode. If the active profile is locked (default),
    /// it is automatically cloned and the clone becomes active before editing begins.
    /// </summary>
    public async Task StartEditingAsync()
    {
        if (IsEditing)
            return;

        try
        {
            var profile = _profileManager.ActiveProfile;

            if (IsProfileLocked(profile))
            {
                _logger.LogInformation("Profile '{Id}' is locked, cloning before edit", profile.Id);
                var clone = await _profileManager.CloneProfileAsync(profile.Id);
                await _profileManager.SwitchProfileAsync(clone.Id);
                profile = _profileManager.ActiveProfile;
            }

            _profileSnapshot = ProfileSerializer.SerializeProfile(profile);
            IsEditing = true;
            _logger.LogInformation("Entered edit mode for profile '{Id}'", profile.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enter edit mode");
        }
    }

    /// <summary>
    /// Saves the current profile to disk and exits edit mode.
    /// </summary>
    public async Task SaveAndStopAsync()
    {
        if (!IsEditing)
            return;

        try
        {
            var profile = _profileManager.ActiveProfile;
            await _profileManager.SaveProfileAsync(profile);
            _logger.LogInformation("Saved profile '{Name}' after editing", profile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile after editing");
        }
        finally
        {
            _profileSnapshot = null;
            SelectedControlId = null;
            IsEditing = false;
        }
    }

    /// <summary>
    /// Restores the profile from the pre-edit snapshot and exits edit mode.
    /// </summary>
    public async Task DiscardAndStopAsync()
    {
        if (!IsEditing)
            return;

        try
        {
            if (_profileSnapshot is not null)
            {
                var restored = ProfileSerializer.DeserializeProfile(_profileSnapshot);
                if (restored is not null)
                {
                    var active = _profileManager.ActiveProfile;
                    active.Controls = restored.Controls;
                    active.Opacity = restored.Opacity;
                    active.Scale = restored.Scale;

                    await _profileManager.SaveProfileAsync(active);
                    LayoutReloadRequested?.Invoke(this, EventArgs.Empty);
                    _logger.LogInformation("Discarded changes for profile '{Name}'", active.Name);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to discard editing changes");
        }
        finally
        {
            _profileSnapshot = null;
            SelectedControlId = null;
            IsEditing = false;
        }
    }

    /// <summary>
    /// Moves a control to a new position. Values are anchor-relative offsets
    /// (distance from the control's configured anchor edge, normalized 0.0-1.0).
    /// </summary>
    public void MoveControl(string id, double anchorRelativeX, double anchorRelativeY)
    {
        var control = FindControl(id);
        if (control is null)
            return;

        control.Position.X = Math.Clamp(anchorRelativeX, 0.0, 1.0);
        control.Position.Y = Math.Clamp(anchorRelativeY, 0.0, 1.0);
    }

    /// <summary>
    /// Resizes a control in the active profile (reference pixels at 1920x1080).
    /// </summary>
    public void ResizeControl(string id, double width, double height)
    {
        var control = FindControl(id);
        if (control is null)
            return;

        control.Size.Width = Math.Max(width, 20);
        control.Size.Height = Math.Max(height, 20);

        if (control.Type == ControlType.Stick)
            control.Size.Radius = Math.Min(control.Size.Width, control.Size.Height) / 2;
    }

    private static bool IsProfileLocked(ControllerProfile profile)
        => string.Equals(profile.Id, LockedProfileId, StringComparison.OrdinalIgnoreCase);

    private ControlDefinition? FindControl(string id)
    {
        var profile = _profileManager.ActiveProfile;
        return profile.Controls.Find(c => c.Id == id);
    }
}
