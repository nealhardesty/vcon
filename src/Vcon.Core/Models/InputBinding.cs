namespace Vcon.Core.Models;

/// <summary>Maps a virtual control to its XInput and keyboard/mouse identifiers.</summary>
public sealed class InputBinding
{
    /// <summary>XInput button or axis name (e.g. "A", "LeftTrigger", "LeftThumb").</summary>
    public string? XInput { get; set; }

    /// <summary>Keyboard key or mouse button name for simple controls.</summary>
    public string? Keyboard { get; set; }

    /// <summary>Directional keyboard bindings for sticks and D-pads.</summary>
    public DirectionalBinding? KeyboardDirectional { get; set; }
}
