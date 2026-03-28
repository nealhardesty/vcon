namespace Vcon.Core.Models;

/// <summary>Keyboard key names for four cardinal directions (sticks and D-pads).</summary>
public sealed class DirectionalBinding
{
    /// <summary>Key name for the up direction.</summary>
    public string? Up { get; set; }

    /// <summary>Key name for the down direction.</summary>
    public string? Down { get; set; }

    /// <summary>Key name for the left direction.</summary>
    public string? Left { get; set; }

    /// <summary>Key name for the right direction.</summary>
    public string? Right { get; set; }
}
