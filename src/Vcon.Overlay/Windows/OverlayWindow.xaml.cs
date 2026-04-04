using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Vcon.Core.Models;
using Vcon.Overlay.Controls;
using Vcon.Overlay.ViewModels;

namespace Vcon.Overlay.Windows;

/// <summary>
/// Transparent, topmost, no-activate overlay window hosting virtual controller controls.
/// Supports edit mode with drag-to-move and resize handles.
/// </summary>
public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private static readonly SolidColorBrush EditBorderBrush =
        new(Color.FromArgb(180, 0, 200, 255));
    private static readonly SolidColorBrush EditGripBrush =
        new(Color.FromArgb(200, 0, 200, 255));

    private readonly OverlayViewModel _viewModel;

    private readonly Dictionary<string, (FrameworkElement Element, ControlDefinition Definition)> _controlMap = new();
    private readonly List<UIElement> _editOverlays = new();

    private double _screenWidth;
    private double _screenHeight;
    private Thickness _taskbarInsets;
    private float _scale;

    // Drag / resize state
    private bool _isDragging;
    private bool _isResizing;
    private FrameworkElement? _dragElement;
    private FrameworkElement? _dragOverlay;
    private ControlDefinition? _dragDef;
    private Point _dragStartMouse;
    private double _dragStartH;
    private double _dragStartV;
    private double _resizeStartWidth;
    private double _resizeStartHeight;

    public OverlayWindow(OverlayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        SyncVisibilityFromViewModel();

        _viewModel.PropertyChanged += (_, e) =>
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.InvokeAsync(() => HandlePropertyChanged(e.PropertyName));
                return;
            }
            HandlePropertyChanged(e.PropertyName);
        };

        _viewModel.Editor.LayoutReloadRequested += (_, _) =>
            Dispatcher.InvokeAsync(() =>
            {
                if (_viewModel.ActiveProfile is not null)
                    LoadLayout(_viewModel.ActiveProfile);
            });
    }

    private void HandlePropertyChanged(string? propertyName)
    {
        switch (propertyName)
        {
            case nameof(OverlayViewModel.IsVisible):
                SyncVisibilityFromViewModel();
                break;
            case nameof(OverlayViewModel.IsEditMode):
                if (_viewModel.IsEditMode)
                    EnterEditMode();
                else
                    ExitEditMode();
                break;
            case nameof(OverlayViewModel.ActiveProfile):
                if (_viewModel.ActiveProfile is not null)
                    LoadLayout(_viewModel.ActiveProfile);
                break;
            case null:
                SyncVisibilityFromViewModel();
                break;
        }
    }

    private void SyncVisibilityFromViewModel()
        => Visibility = _viewModel.IsVisible ? Visibility.Visible : Visibility.Collapsed;

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);

        var hwnd = new WindowInteropHelper(this).Handle;
        var extStyle = GetWindowLongPtr(hwnd, GWL_EXSTYLE);
        SetWindowLongPtr(hwnd, GWL_EXSTYLE, extStyle | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW);
    }

    // ──────────────────────────────────────────────────────────────
    //  Layout
    // ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Clears the canvas and creates WPF controls for each <see cref="ControlDefinition"/> in the profile.
    /// </summary>
    public void LoadLayout(ControllerProfile profile)
    {
        ControlCanvas.Children.Clear();
        _controlMap.Clear();
        _editOverlays.Clear();
        ResetDragState();

        _screenWidth = SystemParameters.PrimaryScreenWidth;
        _screenHeight = SystemParameters.PrimaryScreenHeight;
        var workArea = SystemParameters.WorkArea;
        _taskbarInsets = new Thickness(
            left: workArea.Left,
            top: workArea.Top,
            right: _screenWidth - workArea.Right,
            bottom: _screenHeight - workArea.Bottom);
        _scale = profile.Scale;

        foreach (var control in profile.Controls)
        {
            FrameworkElement? element = control.Type switch
            {
                ControlType.Button => CreateButton(control, _scale),
                ControlType.Stick => CreateStick(control, _scale),
                ControlType.Trigger => CreateTrigger(control, _scale),
                ControlType.DPad => CreateDPad(control, _scale),
                _ => null,
            };

            if (element is null)
                continue;

            PositionControl(element, control.Position, _screenWidth, _screenHeight, _taskbarInsets);
            element.Opacity = profile.Opacity;

            ControlCanvas.Children.Add(element);
            _controlMap[control.Id] = (element, control);
        }
    }

    private static void PositionControl(
        FrameworkElement element, PositionInfo pos,
        double screenWidth, double screenHeight, Thickness taskbarInsets)
    {
        if (pos.HAnchor == HorizontalAnchor.Right)
            Canvas.SetRight(element, pos.X * screenWidth + taskbarInsets.Right);
        else
            Canvas.SetLeft(element, pos.X * screenWidth + taskbarInsets.Left);

        if (pos.VAnchor == VerticalAnchor.Bottom)
            Canvas.SetBottom(element, pos.Y * screenHeight + taskbarInsets.Bottom);
        else
            Canvas.SetTop(element, pos.Y * screenHeight + taskbarInsets.Top);
    }

    // ──────────────────────────────────────────────────────────────
    //  Edit mode
    // ──────────────────────────────────────────────────────────────

    private void EnterEditMode()
    {
        foreach (var (_, (element, def)) in _controlMap)
        {
            var overlay = CreateEditOverlay(element, def);
            ControlCanvas.Children.Add(overlay);
            _editOverlays.Add(overlay);
        }
    }

    private void ExitEditMode()
    {
        ResetDragState();

        foreach (var overlay in _editOverlays)
            ControlCanvas.Children.Remove(overlay);
        _editOverlays.Clear();

        if (_viewModel.ActiveProfile is not null)
            LoadLayout(_viewModel.ActiveProfile);
    }

    private Grid CreateEditOverlay(FrameworkElement element, ControlDefinition def)
    {
        var overlay = new Grid
        {
            Width = element.Width,
            Height = element.Height,
            Background = System.Windows.Media.Brushes.Transparent,
            Cursor = System.Windows.Input.Cursors.SizeAll,
        };

        var border = new Border
        {
            BorderBrush = EditBorderBrush,
            BorderThickness = new Thickness(1.5),
            IsHitTestVisible = false,
        };
        overlay.Children.Add(border);

        var grip = new Border
        {
            Width = 12,
            Height = 12,
            Background = EditGripBrush,
            HorizontalAlignment = System.Windows.HorizontalAlignment.Right,
            VerticalAlignment = System.Windows.VerticalAlignment.Bottom,
            Cursor = System.Windows.Input.Cursors.SizeNWSE,
        };
        overlay.Children.Add(grip);

        CopyCanvasPosition(element, overlay, def.Position);

        // --- Drag (on overlay body, excluding grip) ---
        overlay.MouseLeftButtonDown += (_, e) =>
        {
            if (_isResizing) return;
            BeginDrag(element, overlay, def, e.GetPosition(ControlCanvas));
            overlay.CaptureMouse();
            e.Handled = true;
        };
        overlay.MouseMove += (_, e) =>
        {
            if (!_isDragging || _dragElement != element) return;
            UpdateDrag(e.GetPosition(ControlCanvas));
            e.Handled = true;
        };
        overlay.MouseLeftButtonUp += (_, e) =>
        {
            if (!_isDragging || _dragElement != element) return;
            EndDrag();
            overlay.ReleaseMouseCapture();
            e.Handled = true;
        };

        // --- Resize (on grip only) ---
        grip.MouseLeftButtonDown += (_, e) =>
        {
            BeginResize(element, overlay, def, e.GetPosition(ControlCanvas));
            grip.CaptureMouse();
            e.Handled = true;
        };
        grip.MouseMove += (_, e) =>
        {
            if (!_isResizing || _dragElement != element) return;
            UpdateResize(e.GetPosition(ControlCanvas));
            e.Handled = true;
        };
        grip.MouseLeftButtonUp += (_, e) =>
        {
            if (!_isResizing || _dragElement != element) return;
            EndResize();
            grip.ReleaseMouseCapture();
            e.Handled = true;
        };

        return overlay;
    }

    // ──────────────────────────────────────────────────────────────
    //  Drag
    // ──────────────────────────────────────────────────────────────

    private void BeginDrag(FrameworkElement element, FrameworkElement overlay, ControlDefinition def, Point mousePos)
    {
        _isDragging = true;
        _dragElement = element;
        _dragOverlay = overlay;
        _dragDef = def;
        _dragStartMouse = mousePos;

        _dragStartH = def.Position.HAnchor == HorizontalAnchor.Right
            ? Canvas.GetRight(element) : Canvas.GetLeft(element);
        _dragStartV = def.Position.VAnchor == VerticalAnchor.Bottom
            ? Canvas.GetBottom(element) : Canvas.GetTop(element);
    }

    private void UpdateDrag(Point mousePos)
    {
        if (_dragElement is null || _dragOverlay is null || _dragDef is null) return;

        var dx = mousePos.X - _dragStartMouse.X;
        var dy = mousePos.Y - _dragStartMouse.Y;

        if (_dragDef.Position.HAnchor == HorizontalAnchor.Right)
        {
            var v = _dragStartH - dx;
            Canvas.SetRight(_dragElement, v);
            Canvas.SetRight(_dragOverlay, v);
        }
        else
        {
            var v = _dragStartH + dx;
            Canvas.SetLeft(_dragElement, v);
            Canvas.SetLeft(_dragOverlay, v);
        }

        if (_dragDef.Position.VAnchor == VerticalAnchor.Bottom)
        {
            var v = _dragStartV - dy;
            Canvas.SetBottom(_dragElement, v);
            Canvas.SetBottom(_dragOverlay, v);
        }
        else
        {
            var v = _dragStartV + dy;
            Canvas.SetTop(_dragElement, v);
            Canvas.SetTop(_dragOverlay, v);
        }
    }

    private void EndDrag()
    {
        if (_dragElement is null || _dragDef is null) return;

        double normX = _dragDef.Position.HAnchor == HorizontalAnchor.Right
            ? (Canvas.GetRight(_dragElement) - _taskbarInsets.Right) / _screenWidth
            : (Canvas.GetLeft(_dragElement) - _taskbarInsets.Left) / _screenWidth;

        double normY = _dragDef.Position.VAnchor == VerticalAnchor.Bottom
            ? (Canvas.GetBottom(_dragElement) - _taskbarInsets.Bottom) / _screenHeight
            : (Canvas.GetTop(_dragElement) - _taskbarInsets.Top) / _screenHeight;

        _viewModel.Editor.MoveControl(_dragDef.Id, normX, normY);
        ResetDragState();
    }

    // ──────────────────────────────────────────────────────────────
    //  Resize
    // ──────────────────────────────────────────────────────────────

    private void BeginResize(FrameworkElement element, FrameworkElement overlay, ControlDefinition def, Point mousePos)
    {
        _isResizing = true;
        _dragElement = element;
        _dragOverlay = overlay;
        _dragDef = def;
        _dragStartMouse = mousePos;
        _resizeStartWidth = element.Width;
        _resizeStartHeight = element.Height;
    }

    private void UpdateResize(Point mousePos)
    {
        if (_dragElement is null || _dragOverlay is null) return;

        var newW = Math.Max(_resizeStartWidth + (mousePos.X - _dragStartMouse.X), 20);
        var newH = Math.Max(_resizeStartHeight + (mousePos.Y - _dragStartMouse.Y), 20);

        _dragElement.Width = newW;
        _dragElement.Height = newH;
        _dragOverlay.Width = newW;
        _dragOverlay.Height = newH;
    }

    private void EndResize()
    {
        if (_dragElement is null || _dragDef is null) return;

        _viewModel.Editor.ResizeControl(
            _dragDef.Id,
            _dragElement.Width / _scale,
            _dragElement.Height / _scale);
        ResetDragState();
    }

    private void ResetDragState()
    {
        _isDragging = false;
        _isResizing = false;
        _dragElement = null;
        _dragOverlay = null;
        _dragDef = null;
    }

    // ──────────────────────────────────────────────────────────────
    //  Helpers
    // ──────────────────────────────────────────────────────────────

    private static void CopyCanvasPosition(FrameworkElement source, FrameworkElement target, PositionInfo pos)
    {
        if (pos.HAnchor == HorizontalAnchor.Right)
            Canvas.SetRight(target, Canvas.GetRight(source));
        else
            Canvas.SetLeft(target, Canvas.GetLeft(source));

        if (pos.VAnchor == VerticalAnchor.Bottom)
            Canvas.SetBottom(target, Canvas.GetBottom(source));
        else
            Canvas.SetTop(target, Canvas.GetTop(source));
    }

    // ──────────────────────────────────────────────────────────────
    //  Control factories
    // ──────────────────────────────────────────────────────────────

    private VirtualButton CreateButton(ControlDefinition def, float scale)
    {
        var w = def.Size.Width * scale;
        var h = def.Size.Height * scale;
        var button = new VirtualButton
        {
            ControlId = def.Id,
            Label = def.Label,
            Width = w,
            Height = h,
        };

        var cornerRadius = def.Style?.CornerRadius is { } cr ? cr * scale : Math.Min(w, h) / 2;
        button.SetCornerRadius(cornerRadius);

        if (def.Style is not null)
        {
            button.SetFillFromHex(def.Style.Fill);
            button.SetStrokeFromHex(def.Style.Stroke);
        }

        button.ControlPressed += (_, id) => _viewModel.UpdateButton(id, true);
        button.ControlReleased += (_, id) => _viewModel.UpdateButton(id, false);
        return button;
    }

    private VirtualStick CreateStick(ControlDefinition def, float scale)
    {
        var diameter = def.Size.Radius * 2 * scale;
        var stick = new VirtualStick
        {
            ControlId = def.Id,
            DeadZone = def.DeadZone,
            Width = diameter,
            Height = diameter,
        };

        stick.StickMoved += (_, args) => _viewModel.UpdateStick(args.ControlId, args.X, args.Y);
        stick.ControlPressed += (_, id) => _viewModel.UpdateButton(id + "-click", true);
        stick.ControlReleased += (_, id) => _viewModel.UpdateButton(id + "-click", false);
        return stick;
    }

    private VirtualTrigger CreateTrigger(ControlDefinition def, float scale)
    {
        var trigger = new VirtualTrigger
        {
            ControlId = def.Id,
            Label = def.Label,
            Width = def.Size.Width * scale,
            Height = def.Size.Height * scale,
        };

        trigger.TriggerChanged += (_, args) => _viewModel.UpdateTrigger(args.ControlId, args.Value);
        return trigger;
    }

    private VirtualDPad CreateDPad(ControlDefinition def, float scale)
    {
        var dpad = new VirtualDPad
        {
            ControlId = def.Id,
            Width = def.Size.Width * scale,
            Height = def.Size.Height * scale,
        };

        dpad.DPadChanged += (_, args) =>
            _viewModel.UpdateDPad(args.Up, args.Down, args.Left, args.Right);
        return dpad;
    }

    // ──────────────────────────────────────────────────────────────
    //  P/Invoke
    // ──────────────────────────────────────────────────────────────

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
}
