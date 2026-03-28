namespace Vcon.Core.Models;

/// <summary>Normalized screen position (0.0–1.0) relative to the display area.</summary>
public sealed class PositionInfo
{
    /// <summary>Horizontal position (0.0 = left edge, 1.0 = right edge).</summary>
    public double X { get; set; }

    /// <summary>Vertical position (0.0 = top edge, 1.0 = bottom edge).</summary>
    public double Y { get; set; }
}
