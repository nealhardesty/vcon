using Vcon.Core.Models;

namespace Vcon.Core.Abstractions;

/// <summary>
/// Optional hook for observing controller state changes (diagnostics, logging, visualization).
/// </summary>
public interface IControllerStateObserver
{
    /// <summary>Called when the controller state has been updated.</summary>
    void OnStateChanged(ControllerState state);
}
