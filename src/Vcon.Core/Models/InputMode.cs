using System.Text.Json.Serialization;

namespace Vcon.Core.Models;

/// <summary>Input emulation mode for a profile.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum InputMode
{
    /// <summary>Emulate an Xbox 360 controller via ViGEmBus.</summary>
    XInput,

    /// <summary>Inject keyboard/mouse events via Win32 SendInput.</summary>
    Keyboard
}
