namespace Vcon.Core.Models;

/// <summary>Visual styling for a control (colors, borders, shape).</summary>
public sealed class StyleInfo
{
    /// <summary>Fill color as a hex string (e.g. "#107C10").</summary>
    public string? Fill { get; set; }

    /// <summary>Stroke/border color as a hex string (e.g. "#0E6B0E").</summary>
    public string? Stroke { get; set; }

    /// <summary>
    /// Corner radius in device-independent pixels. When null or omitted, defaults to
    /// half the smaller dimension (producing a circle for square controls, pill for rectangular).
    /// Set to a small value (e.g. 10) for a rounded rectangle.
    /// </summary>
    public double? CornerRadius { get; set; }
}
