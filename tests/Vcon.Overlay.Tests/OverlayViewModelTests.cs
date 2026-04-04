using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;
using Vcon.Input;
using Vcon.Overlay.ViewModels;
using Xunit;

namespace Vcon.Overlay.Tests;

public sealed class OverlayViewModelTests
{
    [Fact]
    public void ToggleVisibility_FlipsIsVisible()
    {
        var vm = CreateViewModel();

        Assert.True(vm.IsVisible);

        vm.ToggleVisibility();
        Assert.False(vm.IsVisible);

        vm.ToggleVisibility();
        Assert.True(vm.IsVisible);
    }

    [Fact]
    public async Task StartEditing_SetsIsEditMode()
    {
        var (vm, _) = CreateViewModelWithEditor();

        Assert.False(vm.IsEditMode);

        await vm.Editor.StartEditingAsync();

        Assert.True(vm.IsEditMode);
    }

    [Fact]
    public async Task SaveAndStop_ClearsIsEditMode()
    {
        var (vm, _) = CreateViewModelWithEditor();

        await vm.Editor.StartEditingAsync();
        Assert.True(vm.IsEditMode);

        await vm.Editor.SaveAndStopAsync();
        Assert.False(vm.IsEditMode);
    }

    [Fact]
    public async Task DiscardAndStop_ClearsIsEditMode()
    {
        var (vm, profileManager) = CreateViewModelWithEditor();

        await vm.Editor.StartEditingAsync();
        Assert.True(vm.IsEditMode);

        await vm.Editor.DiscardAndStopAsync();
        Assert.False(vm.IsEditMode);
    }

    [Fact]
    public void UpdateButton_WithUnknownId_DoesNotThrow()
    {
        var vm = CreateViewModel();

        var ex = Record.Exception(() => vm.UpdateButton("no-such-control", true));

        Assert.Null(ex);
    }

    private static OverlayViewModel CreateViewModel()
    {
        var (vm, _) = CreateViewModelWithEditor();
        return vm;
    }

    private static (OverlayViewModel ViewModel, IProfileManager ProfileManager) CreateViewModelWithEditor()
    {
        var profileManager = Substitute.For<IProfileManager>();
        var profile = new ControllerProfile
        {
            Id = "test-profile",
            Name = "Test Profile",
            Mode = InputMode.XInput,
            Controls = [],
        };
        profileManager.ActiveProfile.Returns(profile);

        var emulatorFactory = new InputEmulatorFactory(NullLoggerFactory.Instance);
        var editorVm = new EditorViewModel(profileManager, NullLogger<EditorViewModel>.Instance);
        var logger = NullLogger<OverlayViewModel>.Instance;

        var vm = new OverlayViewModel(profileManager, emulatorFactory, editorVm, logger);
        return (vm, profileManager);
    }
}
