namespace Vcon.Core.Models;

/// <summary>Control dimensions in device-independent pixels at 1920x1080 reference resolution.</summary>
public sealed class SizeInfo
{
    /// <summary>Width in device-independent pixels.</summary>
    public double Width { get; set; }

    /// <summary>Height in device-independent pixels.</summary>
    public double Height { get; set; }

    /// <summary>Radius for circular controls (analog sticks).</summary>
    public double Radius { get; set; }
}
