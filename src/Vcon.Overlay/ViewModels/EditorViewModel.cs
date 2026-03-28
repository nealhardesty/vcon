using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;

namespace Vcon.Overlay.ViewModels;

/// <summary>
/// ViewModel for the layout editor: dragging, resizing, and saving control positions.
/// </summary>
public partial class EditorViewModel : ObservableObject
{
    private readonly IProfileManager _profileManager;
    private readonly ILogger<EditorViewModel> _logger;

    [ObservableProperty]
    private bool _isEditing;

    [ObservableProperty]
    private bool _showGrid;

    [ObservableProperty]
    private string? _selectedControlId;

    public EditorViewModel(IProfileManager profileManager, ILogger<EditorViewModel> logger)
    {
        _profileManager = profileManager;
        _logger = logger;
    }

    /// <summary>
    /// Enters layout edit mode.
    /// </summary>
    public void StartEditing()
    {
        IsEditing = true;
    }

    /// <summary>
    /// Exits layout edit mode and saves changes to the active profile.
    /// </summary>
    public async Task StopEditingAsync()
    {
        IsEditing = false;
        SelectedControlId = null;

        try
        {
            var profile = _profileManager.ActiveProfile;
            await _profileManager.SaveProfileAsync(profile);
            _logger.LogInformation("Saved profile '{ProfileName}' after editing", profile.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save profile after editing");
        }
    }

    /// <summary>
    /// Moves a control to a new normalized position within the active profile.
    /// </summary>
    public void MoveControl(string id, double normalizedX, double normalizedY)
    {
        var control = FindControl(id);
        if (control is null)
            return;

        control.Position.X = Math.Clamp(normalizedX, 0.0, 1.0);
        control.Position.Y = Math.Clamp(normalizedY, 0.0, 1.0);
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
    }

    private ControlDefinition? FindControl(string id)
    {
        var profile = _profileManager.ActiveProfile;
        return profile.Controls.Find(c => c.Id == id);
    }
}
