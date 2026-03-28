using System.Windows.Input;

namespace Vcon.Overlay.Controls;

/// <summary>
/// Event arguments for trigger value changes.
/// </summary>
public sealed class TriggerChangedEventArgs : EventArgs
{
    public string ControlId { get; }
    public float Value { get; }

    public TriggerChangedEventArgs(string controlId, float value)
    {
        ControlId = controlId;
        Value = value;
    }
}

/// <summary>
/// Virtual trigger control (LT/RT) with binary press for v1.
/// </summary>
public partial class VirtualTrigger : System.Windows.Controls.UserControl
{
    private bool _isPressed;

    /// <summary>Raised when the trigger value changes.</summary>
    public event EventHandler<TriggerChangedEventArgs>? TriggerChanged;

    /// <summary>Raised when the trigger is pressed.</summary>
    public event EventHandler<string>? ControlPressed;

    /// <summary>Raised when the trigger is released.</summary>
    public event EventHandler<string>? ControlReleased;

    /// <summary>Unique control identifier within the profile.</summary>
    public string ControlId { get; set; } = string.Empty;

    /// <summary>Display label rendered on the trigger.</summary>
    public string Label
    {
        get => LabelText.Text;
        set => LabelText.Text = value;
    }

    public VirtualTrigger()
    {
        InitializeComponent();
    }

    protected override void OnTouchDown(TouchEventArgs e)
    {
        CaptureTouch(e.TouchDevice);
        SetPressed(true);
        e.Handled = true;
    }

    protected override void OnTouchUp(TouchEventArgs e)
    {
        ReleaseTouchCapture(e.TouchDevice);
        SetPressed(false);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null)
            return;

        CaptureMouse();
        SetPressed(true);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null)
            return;

        ReleaseMouseCapture();
        SetPressed(false);
        e.Handled = true;
    }

    private void SetPressed(bool pressed)
    {
        if (_isPressed == pressed)
            return;

        _isPressed = pressed;
        var value = pressed ? 1.0f : 0.0f;

        FillIndicator.Opacity = pressed ? 0.5 : 0.0;

        if (pressed)
            ControlPressed?.Invoke(this, ControlId);
        else
            ControlReleased?.Invoke(this, ControlId);

        TriggerChanged?.Invoke(this, new TriggerChangedEventArgs(ControlId, value));
    }
}
