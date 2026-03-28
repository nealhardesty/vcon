using Nefarius.ViGEm.Client;

namespace Vcon.Input.XInput;

/// <summary>Runtime detection of the ViGEmBus driver.</summary>
internal static class ViGEmAvailability
{
    private static readonly Lazy<bool> Cached = new(Probe, LazyThreadSafetyMode.ExecutionAndPublication);

    /// <summary>Returns <c>true</c> if the ViGEmBus driver is installed and reachable.</summary>
    internal static bool IsDriverInstalled() => Cached.Value;

    private static bool Probe()
    {
        try
        {
            using var client = new ViGEmClient();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
