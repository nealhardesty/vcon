namespace Vcon.Core.Models;

/// <summary>Visual styling for a control (colors, borders).</summary>
public sealed class StyleInfo
{
    /// <summary>Fill color as a hex string (e.g. "#107C10").</summary>
    public string? Fill { get; set; }

    /// <summary>Stroke/border color as a hex string (e.g. "#0E6B0E").</summary>
    public string? Stroke { get; set; }
}
