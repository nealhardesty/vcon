using Vcon.Core.Models;

namespace Vcon.Core.Abstractions;

/// <summary>
/// Contract for input emulation backends (Xbox 360 via ViGEm, keyboard/mouse via SendInput).
/// </summary>
public interface IInputEmulator : IDisposable
{
    /// <summary>Whether the emulation backend is available on this system.</summary>
    bool IsAvailable { get; }

    /// <summary>Initialize and connect the virtual device.</summary>
    void Connect();

    /// <summary>Disconnect and release the virtual device.</summary>
    void Disconnect();

    /// <summary>Submit a full controller state snapshot to the emulation backend.</summary>
    void SubmitState(ControllerState state);
}
