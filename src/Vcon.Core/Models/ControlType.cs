using System.Text.Json.Serialization;

namespace Vcon.Core.Models;

/// <summary>Type of virtual control rendered on the overlay.</summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ControlType
{
    /// <summary>A binary press/release button (A, B, X, Y, LB, RB, Start, Back, Guide).</summary>
    Button,

    /// <summary>An analog stick with continuous X/Y axes.</summary>
    Stick,

    /// <summary>A trigger with analog or binary activation.</summary>
    Trigger,

    /// <summary>A directional pad with discrete directions.</summary>
    DPad
}
