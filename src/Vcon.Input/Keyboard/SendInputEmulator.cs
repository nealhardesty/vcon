using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;
using Vcon.Input.Native;
using static Vcon.Input.Native.NativeMethods;

namespace Vcon.Input.Keyboard;

/// <summary>
/// Emulates input by injecting keyboard and mouse events via Win32 <c>SendInput</c>.
/// Bindings are resolved from the <see cref="ControllerProfile"/> at construction time.
/// </summary>
public sealed class SendInputEmulator : IInputEmulator
{
    private readonly ILogger<SendInputEmulator> _logger;
    private readonly List<ButtonKeyBinding> _buttonBindings = [];
    private readonly List<AxisKeyBinding> _axisBindings = [];
    private readonly List<TriggerKeyBinding> _triggerBindings = [];
    private bool _connected;

    /// <summary>Initializes a new <see cref="SendInputEmulator"/> with bindings derived from <paramref name="profile"/>.</summary>
    public SendInputEmulator(ILogger<SendInputEmulator> logger, ControllerProfile profile)
    {
        _logger = logger;
        BuildBindings(profile);
    }

    /// <inheritdoc />
    public bool IsAvailable => true;

    /// <inheritdoc />
    public void Connect()
    {
        _connected = true;
        _logger.LogInformation("SendInput emulator connected");
    }

    /// <inheritdoc />
    public void Disconnect()
    {
        if (!_connected)
            return;

        ReleaseAllKeys();
        _connected = false;
        _logger.LogInformation("SendInput emulator disconnected");
    }

    /// <inheritdoc />
    public void SubmitState(ControllerState state)
    {
        if (!_connected)
            return;

        ProcessButtons(state);
        ProcessAxes(state);
        ProcessTriggers(state);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_connected)
            Disconnect();
    }

    #region State processing

    private void ProcessButtons(ControllerState state)
    {
        foreach (var b in _buttonBindings)
        {
            bool current = b.GetPressed(state);
            if (current == b.WasPressed)
                continue;

            if (current)
                SendDown(b.VirtualKey, b.IsMouse);
            else
                SendUp(b.VirtualKey, b.IsMouse);

            b.WasPressed = current;
        }
    }

    private void ProcessAxes(ControllerState state)
    {
        foreach (var a in _axisBindings)
        {
            float value = a.GetValue(state);
            bool isPositive = value > a.Threshold;
            bool isNegative = value < -a.Threshold;

            if (isPositive != a.WasPositive)
            {
                if (isPositive) SendKeyDown(a.PositiveKey);
                else SendKeyUp(a.PositiveKey);
                a.WasPositive = isPositive;
            }

            if (isNegative != a.WasNegative)
            {
                if (isNegative) SendKeyDown(a.NegativeKey);
                else SendKeyUp(a.NegativeKey);
                a.WasNegative = isNegative;
            }
        }
    }

    private void ProcessTriggers(ControllerState state)
    {
        foreach (var t in _triggerBindings)
        {
            float value = t.GetValue(state);
            bool isPressed = value > t.Threshold;
            if (isPressed == t.WasPressed)
                continue;

            if (isPressed)
                SendDown(t.VirtualKey, t.IsMouse);
            else
                SendUp(t.VirtualKey, t.IsMouse);

            t.WasPressed = isPressed;
        }
    }

    #endregion

    #region Input injection helpers

    private static void SendDown(ushort vk, bool isMouse)
    {
        if (isMouse) SendMouseDown(vk);
        else SendKeyDown(vk);
    }

    private static void SendUp(ushort vk, bool isMouse)
    {
        if (isMouse) SendMouseUp(vk);
        else SendKeyUp(vk);
    }

    private static void SendKeyDown(ushort vk)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion { ki = new KEYBDINPUT { wVk = vk } }
        };
        NativeMethods.SendInput(1, [input], InputSize);
    }

    private static void SendKeyUp(ushort vk)
    {
        var input = new INPUT
        {
            type = INPUT_KEYBOARD,
            u = new InputUnion { ki = new KEYBDINPUT { wVk = vk, dwFlags = KEYEVENTF_KEYUP } }
        };
        NativeMethods.SendInput(1, [input], InputSize);
    }

    private static void SendMouseDown(ushort vk)
    {
        uint flag = vk switch
        {
            (ushort)VirtualKeyCode.VK_LBUTTON => MOUSEEVENTF_LEFTDOWN,
            (ushort)VirtualKeyCode.VK_RBUTTON => MOUSEEVENTF_RIGHTDOWN,
            (ushort)VirtualKeyCode.VK_MBUTTON => MOUSEEVENTF_MIDDLEDOWN,
            _ => 0
        };
        if (flag == 0) return;

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion { mi = new MOUSEINPUT { dwFlags = flag } }
        };
        NativeMethods.SendInput(1, [input], InputSize);
    }

    private static void SendMouseUp(ushort vk)
    {
        uint flag = vk switch
        {
            (ushort)VirtualKeyCode.VK_LBUTTON => MOUSEEVENTF_LEFTUP,
            (ushort)VirtualKeyCode.VK_RBUTTON => MOUSEEVENTF_RIGHTUP,
            (ushort)VirtualKeyCode.VK_MBUTTON => MOUSEEVENTF_MIDDLEUP,
            _ => 0
        };
        if (flag == 0) return;

        var input = new INPUT
        {
            type = INPUT_MOUSE,
            u = new InputUnion { mi = new MOUSEINPUT { dwFlags = flag } }
        };
        NativeMethods.SendInput(1, [input], InputSize);
    }

    private void ReleaseAllKeys()
    {
        foreach (var b in _buttonBindings.Where(b => b.WasPressed))
        {
            SendUp(b.VirtualKey, b.IsMouse);
            b.WasPressed = false;
        }

        foreach (var a in _axisBindings)
        {
            if (a.WasPositive) { SendKeyUp(a.PositiveKey); a.WasPositive = false; }
            if (a.WasNegative) { SendKeyUp(a.NegativeKey); a.WasNegative = false; }
        }

        foreach (var t in _triggerBindings.Where(t => t.WasPressed))
        {
            SendUp(t.VirtualKey, t.IsMouse);
            t.WasPressed = false;
        }
    }

    #endregion

    #region Binding construction

    private void BuildBindings(ControllerProfile profile)
    {
        foreach (var control in profile.Controls)
        {
            var binding = control.Binding;
            if (binding is null)
                continue;

            switch (control.Type)
            {
                case ControlType.Button:
                    TryAddButtonBinding(binding);
                    break;
                case ControlType.Trigger:
                    TryAddTriggerBinding(binding);
                    break;
                case ControlType.Stick:
                    TryAddStickBindings(binding);
                    break;
                case ControlType.DPad:
                    TryAddDPadBindings(binding);
                    break;
            }
        }

        _logger.LogDebug(
            "Built keyboard bindings: {Buttons} buttons, {Axes} axes, {Triggers} triggers",
            _buttonBindings.Count, _axisBindings.Count, _triggerBindings.Count);
    }

    private void TryAddButtonBinding(InputBinding binding)
    {
        if (binding.XInput is null || binding.Keyboard is null)
            return;

        var getter = GetButtonAccessor(binding.XInput);
        if (getter is null)
        {
            _logger.LogWarning("Unknown XInput button: {Name}", binding.XInput);
            return;
        }

        var vk = NativeMethods.ParseKeyName(binding.Keyboard);
        if (vk is null)
        {
            _logger.LogWarning("Unknown key name: {Name}", binding.Keyboard);
            return;
        }

        _buttonBindings.Add(new ButtonKeyBinding
        {
            GetPressed = getter,
            VirtualKey = (ushort)vk.Value,
            IsMouse = IsMouseButton(vk.Value)
        });
    }

    private void TryAddTriggerBinding(InputBinding binding)
    {
        if (binding.XInput is null || binding.Keyboard is null)
            return;

        var getter = GetTriggerAccessor(binding.XInput);
        if (getter is null)
        {
            _logger.LogWarning("Unknown XInput trigger: {Name}", binding.XInput);
            return;
        }

        var vk = NativeMethods.ParseKeyName(binding.Keyboard);
        if (vk is null)
        {
            _logger.LogWarning("Unknown key name: {Name}", binding.Keyboard);
            return;
        }

        _triggerBindings.Add(new TriggerKeyBinding
        {
            GetValue = getter,
            VirtualKey = (ushort)vk.Value,
            IsMouse = IsMouseButton(vk.Value)
        });
    }

    private void TryAddStickBindings(InputBinding binding)
    {
        if (binding.XInput is null || binding.KeyboardDirectional is null)
            return;

        var (getX, getY) = GetStickAccessors(binding.XInput);
        if (getX is null || getY is null)
        {
            _logger.LogWarning("Unknown XInput stick: {Name}", binding.XInput);
            return;
        }

        var dir = binding.KeyboardDirectional;

        var rightVk = dir.Right is not null ? NativeMethods.ParseKeyName(dir.Right) : null;
        var leftVk = dir.Left is not null ? NativeMethods.ParseKeyName(dir.Left) : null;
        if (rightVk is not null && leftVk is not null)
        {
            _axisBindings.Add(new AxisKeyBinding
            {
                GetValue = getX,
                PositiveKey = (ushort)rightVk.Value,
                NegativeKey = (ushort)leftVk.Value
            });
        }

        var upVk = dir.Up is not null ? NativeMethods.ParseKeyName(dir.Up) : null;
        var downVk = dir.Down is not null ? NativeMethods.ParseKeyName(dir.Down) : null;
        if (upVk is not null && downVk is not null)
        {
            _axisBindings.Add(new AxisKeyBinding
            {
                GetValue = getY,
                PositiveKey = (ushort)upVk.Value,
                NegativeKey = (ushort)downVk.Value
            });
        }
    }

    private void TryAddDPadBindings(InputBinding binding)
    {
        if (binding.KeyboardDirectional is null)
            return;

        var dir = binding.KeyboardDirectional;
        TryAddDirectionalButton(dir.Up, static s => s.DPadUp);
        TryAddDirectionalButton(dir.Down, static s => s.DPadDown);
        TryAddDirectionalButton(dir.Left, static s => s.DPadLeft);
        TryAddDirectionalButton(dir.Right, static s => s.DPadRight);
    }

    private void TryAddDirectionalButton(string? keyName, Func<ControllerState, bool> getter)
    {
        if (keyName is null)
            return;

        var vk = NativeMethods.ParseKeyName(keyName);
        if (vk is null)
        {
            _logger.LogWarning("Unknown key name: {Name}", keyName);
            return;
        }

        _buttonBindings.Add(new ButtonKeyBinding
        {
            GetPressed = getter,
            VirtualKey = (ushort)vk.Value,
            IsMouse = IsMouseButton(vk.Value)
        });
    }

    #endregion

    #region XInput name → ControllerState accessor maps

    private static Func<ControllerState, bool>? GetButtonAccessor(string xinputName) =>
        xinputName switch
        {
            "A" => static s => s.A,
            "B" => static s => s.B,
            "X" => static s => s.X,
            "Y" => static s => s.Y,
            "LeftShoulder" or "LB" => static s => s.LeftBumper,
            "RightShoulder" or "RB" => static s => s.RightBumper,
            "Start" => static s => s.Start,
            "Back" => static s => s.Back,
            "Guide" => static s => s.Guide,
            "LeftThumb" or "LS" => static s => s.LeftStickClick,
            "RightThumb" or "RS" => static s => s.RightStickClick,
            _ => null
        };

    private static Func<ControllerState, float>? GetTriggerAccessor(string xinputName) =>
        xinputName switch
        {
            "LeftTrigger" or "LT" => static s => s.LeftTrigger,
            "RightTrigger" or "RT" => static s => s.RightTrigger,
            _ => null
        };

    private static (Func<ControllerState, float>? getX, Func<ControllerState, float>? getY) GetStickAccessors(string xinputName) =>
        xinputName switch
        {
            "LeftThumb" or "LS" => (static s => s.LeftStickX, static s => s.LeftStickY),
            "RightThumb" or "RS" => (static s => s.RightStickX, static s => s.RightStickY),
            _ => (null, null)
        };

    private static bool IsMouseButton(VirtualKeyCode vk) =>
        vk is VirtualKeyCode.VK_LBUTTON or VirtualKeyCode.VK_RBUTTON or VirtualKeyCode.VK_MBUTTON;

    #endregion

    #region Binding types

    private sealed class ButtonKeyBinding
    {
        public required Func<ControllerState, bool> GetPressed { get; init; }
        public required ushort VirtualKey { get; init; }
        public required bool IsMouse { get; init; }
        public bool WasPressed { get; set; }
    }

    private sealed class AxisKeyBinding
    {
        public required Func<ControllerState, float> GetValue { get; init; }
        public required ushort PositiveKey { get; init; }
        public required ushort NegativeKey { get; init; }
        public float Threshold { get; init; } = 0.5f;
        public bool WasPositive { get; set; }
        public bool WasNegative { get; set; }
    }

    private sealed class TriggerKeyBinding
    {
        public required Func<ControllerState, float> GetValue { get; init; }
        public required ushort VirtualKey { get; init; }
        public required bool IsMouse { get; init; }
        public float Threshold { get; init; } = 0.5f;
        public bool WasPressed { get; set; }
    }

    #endregion
}
