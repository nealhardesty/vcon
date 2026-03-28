using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Models;
using Vcon.Input.Keyboard;
using Vcon.Input.XInput;

namespace Vcon.Input;

/// <summary>Creates the correct <see cref="IInputEmulator"/> for a profile's input mode.</summary>
public sealed class InputEmulatorFactory
{
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>Initializes a new <see cref="InputEmulatorFactory"/>.</summary>
    public InputEmulatorFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    /// <summary>
    /// Creates an <see cref="IInputEmulator"/> matching <paramref name="profile"/>'s
    /// <see cref="ControllerProfile.Mode"/>.
    /// </summary>
    public IInputEmulator CreateEmulator(ControllerProfile profile) =>
        profile.Mode switch
        {
            InputMode.XInput => CreateXInputEmulator(),
            InputMode.Keyboard => CreateKeyboardEmulator(profile),
            _ => throw new ArgumentOutOfRangeException(
                nameof(profile),
                profile.Mode,
                $"Unsupported input mode: {profile.Mode}")
        };

    private IInputEmulator CreateXInputEmulator()
    {
        var logger = _loggerFactory.CreateLogger<ViGEmEmulator>();
        var emulator = new ViGEmEmulator(logger);

        if (!emulator.IsAvailable)
            logger.LogWarning(
                "ViGEmBus driver is not installed — Xbox 360 controller emulation will not function. " +
                "Install from https://github.com/nefarius/ViGEmBus/releases");

        return emulator;
    }

    private IInputEmulator CreateKeyboardEmulator(ControllerProfile profile)
    {
        var logger = _loggerFactory.CreateLogger<SendInputEmulator>();
        return new SendInputEmulator(logger, profile);
    }
}
