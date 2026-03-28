using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;
using Vcon.Input;

namespace Vcon.Overlay.ViewModels;

/// <summary>
/// Main ViewModel managing controller state, input dispatch, visibility, and edit mode.
/// </summary>
public partial class OverlayViewModel : ObservableObject
{
    private readonly IProfileManager _profileManager;
    private readonly InputEmulatorFactory _emulatorFactory;
    private readonly ILogger<OverlayViewModel> _logger;

    private IInputEmulator? _emulator;
    private readonly ControllerState _controllerState = new();

    private static readonly Dictionary<string, Action<ControllerState, bool>> ButtonSetters = new()
    {
        ["a-button"] = (s, v) => s.A = v,
        ["b-button"] = (s, v) => s.B = v,
        ["x-button"] = (s, v) => s.X = v,
        ["y-button"] = (s, v) => s.Y = v,
        ["left-bumper"] = (s, v) => s.LeftBumper = v,
        ["right-bumper"] = (s, v) => s.RightBumper = v,
        ["start"] = (s, v) => s.Start = v,
        ["back"] = (s, v) => s.Back = v,
        ["guide"] = (s, v) => s.Guide = v,
        ["left-stick-click"] = (s, v) => s.LeftStickClick = v,
        ["right-stick-click"] = (s, v) => s.RightStickClick = v,
    };

    private static readonly Dictionary<string, Action<ControllerState, float, float>> StickSetters = new()
    {
        ["left-stick"] = (s, x, y) => { s.LeftStickX = x; s.LeftStickY = y; },
        ["right-stick"] = (s, x, y) => { s.RightStickX = x; s.RightStickY = y; },
    };

    private static readonly Dictionary<string, Action<ControllerState, float>> TriggerSetters = new()
    {
        ["left-trigger"] = (s, v) => s.LeftTrigger = v,
        ["right-trigger"] = (s, v) => s.RightTrigger = v,
    };

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private ControllerProfile? _activeProfile;

    [ObservableProperty]
    private double _opacity = 0.7;

    public OverlayViewModel(
        IProfileManager profileManager,
        InputEmulatorFactory emulatorFactory,
        ILogger<OverlayViewModel> logger)
    {
        _profileManager = profileManager;
        _emulatorFactory = emulatorFactory;
        _logger = logger;

        _profileManager.ActiveProfileChanged += OnActiveProfileChanged;
    }

    /// <summary>
    /// Updates a binary button state by control ID.
    /// </summary>
    public void UpdateButton(string controlId, bool pressed)
    {
        if (ButtonSetters.TryGetValue(controlId, out var setter))
        {
            setter(_controllerState, pressed);
            SubmitStateIfActive();
        }
    }

    /// <summary>
    /// Updates an analog stick's X/Y axes by control ID.
    /// </summary>
    public void UpdateStick(string controlId, float x, float y)
    {
        if (StickSetters.TryGetValue(controlId, out var setter))
        {
            setter(_controllerState, x, y);
            SubmitStateIfActive();
        }
    }

    /// <summary>
    /// Updates a trigger's analog value by control ID.
    /// </summary>
    public void UpdateTrigger(string controlId, float value)
    {
        if (TriggerSetters.TryGetValue(controlId, out var setter))
        {
            setter(_controllerState, value);
            SubmitStateIfActive();
        }
    }

    /// <summary>
    /// Updates D-Pad directional state.
    /// </summary>
    public void UpdateDPad(bool up, bool down, bool left, bool right)
    {
        _controllerState.DPadUp = up;
        _controllerState.DPadDown = down;
        _controllerState.DPadLeft = left;
        _controllerState.DPadRight = right;
        SubmitStateIfActive();
    }

    /// <summary>
    /// Toggles overlay visibility.
    /// </summary>
    public void ToggleVisibility()
    {
        IsVisible = !IsVisible;
    }

    /// <summary>
    /// Toggles layout edit mode. When editing, input submission is suppressed.
    /// </summary>
    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;

        if (IsEditMode)
        {
            _controllerState.Reset();
        }
    }

    /// <summary>
    /// Cycles to the next available profile.
    /// </summary>
    public async Task CycleProfileAsync()
    {
        try
        {
            var profiles = await _profileManager.GetAllProfilesAsync();
            if (profiles.Count <= 1)
                return;

            var currentId = ActiveProfile?.Id ?? string.Empty;
            var currentIndex = -1;

            for (int i = 0; i < profiles.Count; i++)
            {
                if (profiles[i].Id == currentId)
                {
                    currentIndex = i;
                    break;
                }
            }

            var nextIndex = (currentIndex + 1) % profiles.Count;
            await _profileManager.SwitchProfileAsync(profiles[nextIndex].Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to cycle profile");
        }
    }

    /// <summary>
    /// Switches to a specific profile by ID.
    /// </summary>
    public async Task SwitchProfileAsync(string profileId)
    {
        try
        {
            await _profileManager.SwitchProfileAsync(profileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to switch to profile '{ProfileId}'", profileId);
        }
    }

    /// <summary>
    /// Connects the input emulator for the active profile's mode.
    /// </summary>
    public void ConnectEmulator()
    {
        DisconnectEmulator();

        if (ActiveProfile is null)
            return;

        try
        {
            _emulator = _emulatorFactory.CreateEmulator(ActiveProfile);

            if (!_emulator.IsAvailable)
            {
                _logger.LogWarning("Emulator for mode {Mode} is not available", ActiveProfile.Mode);
                _emulator = null;
                return;
            }

            _emulator.Connect();
            _logger.LogInformation("Connected emulator for mode {Mode}", ActiveProfile.Mode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect emulator for mode {Mode}", ActiveProfile?.Mode);
            _emulator = null;
        }
    }

    /// <summary>
    /// Disconnects and disposes the current input emulator.
    /// </summary>
    public void DisconnectEmulator()
    {
        if (_emulator is null)
            return;

        try
        {
            _emulator.Disconnect();
            _emulator.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting emulator");
        }

        _emulator = null;
    }

    private void SubmitStateIfActive()
    {
        if (IsEditMode || _emulator is null)
            return;

        try
        {
            _emulator.SubmitState(_controllerState);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit controller state");
        }
    }

    private void OnActiveProfileChanged(object? sender, ControllerProfile profile)
    {
        ActiveProfile = profile;
        Opacity = profile.Opacity;
        _controllerState.Reset();
        ConnectEmulator();
    }
}
