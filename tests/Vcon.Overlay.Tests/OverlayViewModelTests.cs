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
    public void ToggleEditMode_FlipsIsEditMode()
    {
        var vm = CreateViewModel();

        Assert.False(vm.IsEditMode);

        vm.ToggleEditMode();
        Assert.True(vm.IsEditMode);

        vm.ToggleEditMode();
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
        var profileManager = Substitute.For<IProfileManager>();
        profileManager.ActiveProfile.Returns((ControllerProfile?)null);

        var emulatorFactory = new InputEmulatorFactory(NullLoggerFactory.Instance);
        var logger = NullLogger<OverlayViewModel>.Instance;

        return new OverlayViewModel(profileManager, emulatorFactory, logger);
    }
}
