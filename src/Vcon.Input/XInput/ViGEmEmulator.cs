using Microsoft.Extensions.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;

namespace Vcon.Input.XInput;

/// <summary>Emulates a virtual Xbox 360 controller via the ViGEmBus driver.</summary>
public sealed class ViGEmEmulator : IInputEmulator
{
    private readonly ILogger<ViGEmEmulator> _logger;
    private ViGEmClient? _client;
    private IXbox360Controller? _controller;
    private bool _connected;

    /// <summary>Initializes a new <see cref="ViGEmEmulator"/>.</summary>
    public ViGEmEmulator(ILogger<ViGEmEmulator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public bool IsAvailable => ViGEmAvailability.IsDriverInstalled();

    /// <inheritdoc />
    public void Connect()
    {
        if (_connected)
            return;

        _client = new ViGEmClient();
        _controller = _client.CreateXbox360Controller();
        _controller.AutoSubmitReport = false;
        _controller.Connect();
        _connected = true;

        _logger.LogInformation("Virtual Xbox 360 controller connected");
    }

    /// <inheritdoc />
    public void Disconnect()
    {
        if (!_connected)
            return;

        try
        {
            _controller?.Disconnect();
            _logger.LogInformation("Virtual Xbox 360 controller disconnected");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error disconnecting virtual Xbox 360 controller");
        }
        finally
        {
            _connected = false;
        }
    }

    /// <inheritdoc />
    public void SubmitState(ControllerState state)
    {
        if (!_connected || _controller is null)
            return;

        try
        {
            _controller.ResetReport();
            MapButtons(state);
            MapAxes(state);
            MapTriggers(state);
            _controller.SubmitReport();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit Xbox 360 controller report");
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        Disconnect();
        (_controller as IDisposable)?.Dispose();
        _controller = null;
        _client?.Dispose();
        _client = null;
    }

    private void MapButtons(ControllerState state)
    {
        _controller!.SetButtonState(Xbox360Button.A, state.A);
        _controller.SetButtonState(Xbox360Button.B, state.B);
        _controller.SetButtonState(Xbox360Button.X, state.X);
        _controller.SetButtonState(Xbox360Button.Y, state.Y);
        _controller.SetButtonState(Xbox360Button.LeftShoulder, state.LeftBumper);
        _controller.SetButtonState(Xbox360Button.RightShoulder, state.RightBumper);
        _controller.SetButtonState(Xbox360Button.Start, state.Start);
        _controller.SetButtonState(Xbox360Button.Back, state.Back);
        _controller.SetButtonState(Xbox360Button.Guide, state.Guide);
        _controller.SetButtonState(Xbox360Button.Up, state.DPadUp);
        _controller.SetButtonState(Xbox360Button.Down, state.DPadDown);
        _controller.SetButtonState(Xbox360Button.Left, state.DPadLeft);
        _controller.SetButtonState(Xbox360Button.Right, state.DPadRight);
        _controller.SetButtonState(Xbox360Button.LeftThumb, state.LeftStickClick);
        _controller.SetButtonState(Xbox360Button.RightThumb, state.RightStickClick);
    }

    private void MapAxes(ControllerState state)
    {
        _controller!.SetAxisValue(Xbox360Axis.LeftThumbX, ClampAxis(state.LeftStickX));
        _controller.SetAxisValue(Xbox360Axis.LeftThumbY, ClampAxis(state.LeftStickY));
        _controller.SetAxisValue(Xbox360Axis.RightThumbX, ClampAxis(state.RightStickX));
        _controller.SetAxisValue(Xbox360Axis.RightThumbY, ClampAxis(state.RightStickY));
    }

    private void MapTriggers(ControllerState state)
    {
        _controller!.SetSliderValue(Xbox360Slider.LeftTrigger, ClampTrigger(state.LeftTrigger));
        _controller.SetSliderValue(Xbox360Slider.RightTrigger, ClampTrigger(state.RightTrigger));
    }

    private static short ClampAxis(float value) =>
        (short)Math.Clamp(value * 32767f, short.MinValue, short.MaxValue);

    private static byte ClampTrigger(float value) =>
        (byte)Math.Clamp(value * 255f, byte.MinValue, byte.MaxValue);
}
