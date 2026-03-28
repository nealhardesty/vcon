using Microsoft.Extensions.Logging.Abstractions;
using Vcon.Core.Models;
using Vcon.Input.Keyboard;
using Xunit;

namespace Vcon.Input.Tests;

public sealed class SendInputEmulatorTests
{
    [Fact]
    public void Constructor_BuildsBindingsFromProfile()
    {
        var profile = new ControllerProfile
        {
            Id = "kb",
            Name = "Keyboard",
            Mode = InputMode.Keyboard,
            Controls =
            [
                new ControlDefinition
                {
                    Id = "a-button",
                    Type = ControlType.Button,
                    Binding = new InputBinding
                    {
                        XInput = "A",
                        Keyboard = "Space",
                    },
                },
            ],
        };

        var emulator = new SendInputEmulator(NullLogger<SendInputEmulator>.Instance, profile);

        Assert.NotNull(emulator);
    }

    [Fact]
    public void IsAvailable_AlwaysTrue()
    {
        var profile = new ControllerProfile { Mode = InputMode.Keyboard };
        var emulator = new SendInputEmulator(NullLogger<SendInputEmulator>.Instance, profile);

        Assert.True(emulator.IsAvailable);
    }

    [Fact]
    public void Connect_Disconnect_DoNotThrow()
    {
        var profile = new ControllerProfile { Mode = InputMode.Keyboard };
        using var emulator = new SendInputEmulator(NullLogger<SendInputEmulator>.Instance, profile);

        emulator.Connect();
        emulator.Disconnect();
    }
}
