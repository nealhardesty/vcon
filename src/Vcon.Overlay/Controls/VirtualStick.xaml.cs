using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vcon.Overlay.Controls;

/// <summary>
/// Event arguments for analog stick movement.
/// </summary>
public sealed class StickMovedEventArgs : EventArgs
{
    public string ControlId { get; }
    public float X { get; }
    public float Y { get; }

    public StickMovedEventArgs(string controlId, float x, float y)
    {
        ControlId = controlId;
        X = x;
        Y = y;
    }
}

/// <summary>
/// Virtual analog stick control with circular touch zone and draggable thumb indicator.
/// </summary>
public partial class VirtualStick : UserControl
{
    private const double ThumbSizeRatio = 0.4;

    private TouchDevice? _activeTouchDevice;
    private bool _isMouseTracking;

    /// <summary>Raised when the thumb position changes.</summary>
    public event EventHandler<StickMovedEventArgs>? StickMoved;

    /// <summary>Raised when the stick is pressed (click).</summary>
    public event EventHandler<string>? ControlPressed;

    /// <summary>Raised when the stick press is released.</summary>
    public event EventHandler<string>? ControlReleased;

    /// <summary>Unique control identifier within the profile.</summary>
    public string ControlId { get; set; } = string.Empty;

    /// <summary>Dead zone threshold below which stick output is zero (0.0–1.0).</summary>
    public float DeadZone { get; set; } = 0.15f;

    /// <summary>Current thumb X output (−1.0 to +1.0).</summary>
    public float ThumbX { get; private set; }

    /// <summary>Current thumb Y output (−1.0 to +1.0).</summary>
    public float ThumbY { get; private set; }

    public VirtualStick()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        var thumbSize = Math.Min(e.NewSize.Width, e.NewSize.Height) * ThumbSizeRatio;
        ThumbEllipse.Width = thumbSize;
        ThumbEllipse.Height = thumbSize;
        CenterThumb();
    }

    protected override void OnTouchDown(TouchEventArgs e)
    {
        if (_activeTouchDevice is not null)
            return;

        _activeTouchDevice = e.TouchDevice;
        CaptureTouch(e.TouchDevice);
        ProcessPointerPosition(e.GetTouchPoint(this).Position);
        ControlPressed?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnTouchMove(TouchEventArgs e)
    {
        if (e.TouchDevice != _activeTouchDevice)
            return;

        ProcessPointerPosition(e.GetTouchPoint(this).Position);
        e.Handled = true;
    }

    protected override void OnTouchUp(TouchEventArgs e)
    {
        if (e.TouchDevice != _activeTouchDevice)
            return;

        ReleaseTouchCapture(e.TouchDevice);
        _activeTouchDevice = null;
        ResetThumb();
        ControlReleased?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null || _activeTouchDevice is not null)
            return;

        _isMouseTracking = true;
        CaptureMouse();
        ProcessPointerPosition(e.GetPosition(this));
        ControlPressed?.Invoke(this, ControlId);
        e.Handled = true;
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        if (!_isMouseTracking)
            return;

        ProcessPointerPosition(e.GetPosition(this));
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        if (!_isMouseTracking)
            return;

        _isMouseTracking = false;
        ReleaseMouseCapture();
        ResetThumb();
        ControlReleased?.Invoke(this, ControlId);
        e.Handled = true;
    }

    private void ProcessPointerPosition(Point pos)
    {
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;
        var radius = Math.Min(centerX, centerY);

        if (radius <= 0)
            return;

        var dx = pos.X - centerX;
        var dy = pos.Y - centerY;
        var magnitude = Math.Sqrt(dx * dx + dy * dy);

        if (magnitude > radius)
        {
            dx = dx / magnitude * radius;
            dy = dy / magnitude * radius;
            magnitude = radius;
        }

        var normalizedMag = (float)(magnitude / radius);
        if (normalizedMag < DeadZone)
        {
            ThumbX = 0f;
            ThumbY = 0f;
        }
        else
        {
            ThumbX = (float)(dx / radius);
            ThumbY = (float)(-dy / radius); // WPF Y is inverted vs controller Y
        }

        var thumbSize = ThumbEllipse.Width;
        Canvas.SetLeft(ThumbEllipse, centerX + dx - thumbSize / 2);
        Canvas.SetTop(ThumbEllipse, centerY + dy - thumbSize / 2);

        StickMoved?.Invoke(this, new StickMovedEventArgs(ControlId, ThumbX, ThumbY));
    }

    private void ResetThumb()
    {
        ThumbX = 0f;
        ThumbY = 0f;
        CenterThumb();
        StickMoved?.Invoke(this, new StickMovedEventArgs(ControlId, 0f, 0f));
    }

    private void CenterThumb()
    {
        var thumbSize = ThumbEllipse.Width;
        Canvas.SetLeft(ThumbEllipse, ActualWidth / 2 - thumbSize / 2);
        Canvas.SetTop(ThumbEllipse, ActualHeight / 2 - thumbSize / 2);
    }
}
