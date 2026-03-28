namespace Vcon.Core.Models;

/// <summary>Layout, style, and binding definition for a single virtual control.</summary>
public sealed class ControlDefinition
{
    /// <summary>Unique identifier for this control instance (e.g. "a-button", "left-stick").</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Type of control (button, stick, trigger, D-pad).</summary>
    public ControlType Type { get; set; }

    /// <summary>Display label rendered on or near the control.</summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>Normalized screen position (0.0–1.0).</summary>
    public PositionInfo Position { get; set; } = new();

    /// <summary>Control size at 1920x1080 reference resolution.</summary>
    public SizeInfo Size { get; set; } = new();

    /// <summary>Visual style overrides (colors, borders).</summary>
    public StyleInfo? Style { get; set; }

    /// <summary>Input binding for XInput and keyboard/mouse modes.</summary>
    public InputBinding Binding { get; set; } = new();

    /// <summary>Dead zone radius for analog sticks (0.0–1.0). Ignored for non-stick controls.</summary>
    public float DeadZone { get; set; }
}
