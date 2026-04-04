namespace Vcon.Core.Models;

/// <summary>
/// Normalized screen position (0.0–1.0) measured from the anchor edges.
/// Defaults to top-left for backward compatibility with older profiles.
/// </summary>
public sealed class PositionInfo
{
    /// <summary>Horizontal offset (0.0–1.0) from the <see cref="HorizontalAnchor"/> edge.</summary>
    public double X { get; set; }

    /// <summary>Vertical offset (0.0–1.0) from the <see cref="VerticalAnchor"/> edge.</summary>
    public double Y { get; set; }

    /// <summary>Which horizontal screen edge X is measured from (default: left).</summary>
    public HorizontalAnchor HAnchor { get; set; } = HorizontalAnchor.Left;

    /// <summary>Which vertical screen edge Y is measured from (default: top).</summary>
    public VerticalAnchor VAnchor { get; set; } = VerticalAnchor.Top;
}
