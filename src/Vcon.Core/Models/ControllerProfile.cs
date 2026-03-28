namespace Vcon.Core.Models;

/// <summary>Serializable profile defining layout, bindings, and emulation mode for the overlay.</summary>
public sealed class ControllerProfile
{
    /// <summary>Unique profile identifier (used as filename stem).</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>Human-readable profile name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Input emulation mode (XInput or Keyboard).</summary>
    public InputMode Mode { get; set; }

    /// <summary>Global overlay opacity (0.1–1.0).</summary>
    public float Opacity { get; set; } = 0.7f;

    /// <summary>Layout scale factor relative to 1920x1080 reference resolution.</summary>
    public float Scale { get; set; } = 1.0f;

    /// <summary>All controls in this profile's layout.</summary>
    public List<ControlDefinition> Controls { get; set; } = [];
}
