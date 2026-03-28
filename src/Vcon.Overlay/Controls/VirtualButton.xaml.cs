using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace Vcon.Overlay.Controls;

/// <summary>
/// Virtual button control for face buttons, bumpers, start/back/guide.
/// </summary>
public partial class VirtualButton : System.Windows.Controls.UserControl
{
    private bool _isPressed;

    /// <summary>Raised when the button is pressed.</summary>
    public event EventHandler<string>? ControlPressed;

    /// <summary>Raised when the button is released.</summary>
    public event EventHandler<string>? ControlReleased;

    /// <summary>Unique control identifier within the profile.</summary>
    public string ControlId { get; set; } = string.Empty;

    /// <summary>Display label rendered on the button.</summary>
    public string Label
    {
        get => LabelText.Text;
        set => LabelText.Text = value;
    }

    /// <summary>Whether the button is currently pressed.</summary>
    public bool IsPressed
    {
        get => _isPressed;
        private set
        {
            if (_isPressed == value)
                return;

            _isPressed = value;
            ApplyVisualState();
        }
    }

    public VirtualButton()
    {
        InitializeComponent();
    }

    /// <summary>Sets the fill brush from a hex color string.</summary>
    public void SetFillFromHex(string? hex)
    {
        if (hex is null)
            return;

        try
        {
            BackgroundEllipse.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        catch (FormatException)
        {
            // Invalid hex — keep default
        }
    }

    /// <summary>Sets the stroke brush from a hex color string.</summary>
    public void SetStrokeFromHex(string? hex)
    {
        if (hex is null)
            return;

        try
        {
            BackgroundEllipse.Stroke = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        }
        catch (FormatException)
        {
            // Invalid hex — keep default
        }
    }

    protected override void OnTouchDown(TouchEventArgs e)
    {
        CaptureTouch(e.TouchDevice);
        IsPressed = true;
        ControlPressed?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnTouchUp(TouchEventArgs e)
    {
        ReleaseTouchCapture(e.TouchDevice);
        IsPressed = false;
        ControlReleased?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null)
            return; // Touch already handled via OnTouchDown

        CaptureMouse();
        IsPressed = true;
        ControlPressed?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null)
            return;

        ReleaseMouseCapture();
        IsPressed = false;
        ControlReleased?.Invoke(this, ControlId);
        e.Handled = true;
    }

    private void ApplyVisualState()
    {
        PressTransform.ScaleX = _isPressed ? 0.9 : 1.0;
        PressTransform.ScaleY = _isPressed ? 0.9 : 1.0;
    }
}
