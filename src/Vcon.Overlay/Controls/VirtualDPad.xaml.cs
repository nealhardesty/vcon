using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Vcon.Overlay.Controls;

/// <summary>
/// Event arguments for D-Pad directional state changes.
/// </summary>
public sealed class DPadChangedEventArgs : EventArgs
{
    public string ControlId { get; }
    public bool Up { get; }
    public bool Down { get; }
    public bool Left { get; }
    public bool Right { get; }

    public DPadChangedEventArgs(string controlId, bool up, bool down, bool left, bool right)
    {
        ControlId = controlId;
        Up = up;
        Down = down;
        Left = left;
        Right = right;
    }
}

/// <summary>
/// Virtual D-Pad control supporting 8-directional input via touch/mouse.
/// </summary>
public partial class VirtualDPad : UserControl
{
    private const double CrossBarRatio = 0.33;

    private TouchDevice? _activeTouchDevice;
    private bool _isMouseTracking;

    /// <summary>Raised when the D-Pad directional state changes.</summary>
    public event EventHandler<DPadChangedEventArgs>? DPadChanged;

    /// <summary>Unique control identifier within the profile.</summary>
    public string ControlId { get; set; } = string.Empty;

    public VirtualDPad()
    {
        InitializeComponent();
        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        LayoutCross(e.NewSize.Width, e.NewSize.Height);
    }

    private void LayoutCross(double w, double h)
    {
        var barW = w * CrossBarRatio;
        var barH = h * CrossBarRatio;
        var centerX = (w - barW) / 2;
        var centerY = (h - barH) / 2;

        // Vertical bar: full height, centered width
        Canvas.SetLeft(VerticalBar, centerX);
        Canvas.SetTop(VerticalBar, 0);
        VerticalBar.Width = barW;
        VerticalBar.Height = h;

        // Horizontal bar: full width, centered height
        Canvas.SetLeft(HorizontalBar, 0);
        Canvas.SetTop(HorizontalBar, centerY);
        HorizontalBar.Width = w;
        HorizontalBar.Height = barH;

        // Up highlight: top section of vertical bar
        Canvas.SetLeft(UpHighlight, centerX);
        Canvas.SetTop(UpHighlight, 0);
        UpHighlight.Width = barW;
        UpHighlight.Height = centerY;

        // Down highlight: bottom section of vertical bar
        Canvas.SetLeft(DownHighlight, centerX);
        Canvas.SetTop(DownHighlight, centerY + barH);
        DownHighlight.Width = barW;
        DownHighlight.Height = centerY;

        // Left highlight: left section of horizontal bar
        Canvas.SetLeft(LeftHighlight, 0);
        Canvas.SetTop(LeftHighlight, centerY);
        LeftHighlight.Width = centerX;
        LeftHighlight.Height = barH;

        // Right highlight: right section of horizontal bar
        Canvas.SetLeft(RightHighlight, centerX + barW);
        Canvas.SetTop(RightHighlight, centerY);
        RightHighlight.Width = centerX;
        RightHighlight.Height = barH;
    }

    protected override void OnTouchDown(TouchEventArgs e)
    {
        if (_activeTouchDevice is not null)
            return;

        _activeTouchDevice = e.TouchDevice;
        CaptureTouch(e.TouchDevice);
        ProcessPointerPosition(e.GetTouchPoint(this).Position);
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
        ClearDirections();
        e.Handled = true;
    }

    protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
    {
        if (e.StylusDevice is not null || _activeTouchDevice is not null)
            return;

        _isMouseTracking = true;
        CaptureMouse();
        ProcessPointerPosition(e.GetPosition(this));
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
        ClearDirections();
        e.Handled = true;
    }

    private void ProcessPointerPosition(Point pos)
    {
        var centerX = ActualWidth / 2;
        var centerY = ActualHeight / 2;

        if (centerX <= 0 || centerY <= 0)
            return;

        var dx = pos.X - centerX;
        var dy = pos.Y - centerY;

        // Normalize to [-1, 1] range
        var nx = dx / centerX;
        var ny = dy / centerY;

        const double threshold = 0.2;

        bool up = ny < -threshold;
        bool down = ny > threshold;
        bool left = nx < -threshold;
        bool right = nx > threshold;

        ApplyHighlights(up, down, left, right);
        DPadChanged?.Invoke(this, new DPadChangedEventArgs(ControlId, up, down, left, right));
    }

    private void ClearDirections()
    {
        ApplyHighlights(false, false, false, false);
        DPadChanged?.Invoke(this, new DPadChangedEventArgs(ControlId, false, false, false, false));
    }

    private void ApplyHighlights(bool up, bool down, bool left, bool right)
    {
        UpHighlight.Opacity = up ? 1.0 : 0.0;
        DownHighlight.Opacity = down ? 1.0 : 0.0;
        LeftHighlight.Opacity = left ? 1.0 : 0.0;
        RightHighlight.Opacity = right ? 1.0 : 0.0;
    }
}
