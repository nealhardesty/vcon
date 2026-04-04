using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Vcon.Core.Models;
using Vcon.Overlay.Controls;
using Vcon.Overlay.ViewModels;

namespace Vcon.Overlay.Windows;

/// <summary>
/// Transparent, topmost, no-activate overlay window hosting virtual controller controls.
/// </summary>
public partial class OverlayWindow : Window
{
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const int WS_EX_TOOLWINDOW = 0x00000080;

    private readonly OverlayViewModel _viewModel;

    public OverlayWindow(OverlayViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        SyncVisibilityFromViewModel();
        _viewModel.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(OverlayViewModel.IsVisible) || e.PropertyName is null)
                SyncVisibilityFromViewModel();
        };
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

    /// <summary>
    /// Clears the canvas and creates WPF controls for each <see cref="ControlDefinition"/> in the profile.
    /// </summary>
    public void LoadLayout(ControllerProfile profile)
    {
        ControlCanvas.Children.Clear();

        var screenWidth = SystemParameters.PrimaryScreenWidth;
        var screenHeight = SystemParameters.PrimaryScreenHeight;
        var scale = profile.Scale;

        foreach (var control in profile.Controls)
        {
            FrameworkElement? element = control.Type switch
            {
                ControlType.Button => CreateButton(control, scale),
                ControlType.Stick => CreateStick(control, scale),
                ControlType.Trigger => CreateTrigger(control, scale),
                ControlType.DPad => CreateDPad(control, scale),
                _ => null,
            };

            if (element is null)
                continue;

            System.Windows.Controls.Canvas.SetLeft(element, control.Position.X * screenWidth);
            System.Windows.Controls.Canvas.SetTop(element, control.Position.Y * screenHeight);
            element.Opacity = profile.Opacity;

            ControlCanvas.Children.Add(element);
        }
    }

    private VirtualButton CreateButton(ControlDefinition def, float scale)
    {
        var button = new VirtualButton
        {
            ControlId = def.Id,
            Label = def.Label,
            Width = def.Size.Width * scale,
            Height = def.Size.Height * scale,
        };

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

    [DllImport("user32.dll", EntryPoint = "GetWindowLongPtrW")]
    private static extern nint GetWindowLongPtr(nint hWnd, int nIndex);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtrW")]
    private static extern nint SetWindowLongPtr(nint hWnd, int nIndex, nint dwNewLong);
}
