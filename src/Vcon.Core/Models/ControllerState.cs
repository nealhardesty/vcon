namespace Vcon.Core.Models;

/// <summary>
/// Full Xbox 360 controller state snapshot with all digital and analog inputs.
/// </summary>
public sealed class ControllerState
{
    // Face buttons
    /// <summary>A button (green).</summary>
    public bool A { get; set; }

    /// <summary>B button (red).</summary>
    public bool B { get; set; }

    /// <summary>X button (blue).</summary>
    public bool X { get; set; }

    /// <summary>Y button (yellow).</summary>
    public bool Y { get; set; }

    // Shoulder buttons
    /// <summary>Left bumper (LB).</summary>
    public bool LeftBumper { get; set; }

    /// <summary>Right bumper (RB).</summary>
    public bool RightBumper { get; set; }

    // Triggers (0.0 – 1.0)
    /// <summary>Left trigger axis (0.0 released, 1.0 fully pressed).</summary>
    public float LeftTrigger { get; set; }

    /// <summary>Right trigger axis (0.0 released, 1.0 fully pressed).</summary>
    public float RightTrigger { get; set; }

    // Analog sticks (−1.0 to +1.0 per axis)
    /// <summary>Left stick horizontal axis (−1.0 left, +1.0 right).</summary>
    public float LeftStickX { get; set; }

    /// <summary>Left stick vertical axis (−1.0 down, +1.0 up).</summary>
    public float LeftStickY { get; set; }

    /// <summary>Right stick horizontal axis (−1.0 left, +1.0 right).</summary>
    public float RightStickX { get; set; }

    /// <summary>Right stick vertical axis (−1.0 down, +1.0 up).</summary>
    public float RightStickY { get; set; }

    /// <summary>Left stick click (L3).</summary>
    public bool LeftStickClick { get; set; }

    /// <summary>Right stick click (R3).</summary>
    public bool RightStickClick { get; set; }

    // D-Pad
    /// <summary>D-Pad up.</summary>
    public bool DPadUp { get; set; }

    /// <summary>D-Pad down.</summary>
    public bool DPadDown { get; set; }

    /// <summary>D-Pad left.</summary>
    public bool DPadLeft { get; set; }

    /// <summary>D-Pad right.</summary>
    public bool DPadRight { get; set; }

    // Meta
    /// <summary>Start button.</summary>
    public bool Start { get; set; }

    /// <summary>Back button.</summary>
    public bool Back { get; set; }

    /// <summary>Guide button (Xbox button).</summary>
    public bool Guide { get; set; }

    /// <summary>Returns a deep copy of this state.</summary>
    public ControllerState Clone() => new()
    {
        A = A, B = B, X = X, Y = Y,
        LeftBumper = LeftBumper, RightBumper = RightBumper,
        LeftTrigger = LeftTrigger, RightTrigger = RightTrigger,
        LeftStickX = LeftStickX, LeftStickY = LeftStickY,
        RightStickX = RightStickX, RightStickY = RightStickY,
        LeftStickClick = LeftStickClick, RightStickClick = RightStickClick,
        DPadUp = DPadUp, DPadDown = DPadDown,
        DPadLeft = DPadLeft, DPadRight = DPadRight,
        Start = Start, Back = Back, Guide = Guide,
    };

    /// <summary>Resets all inputs to their default (released / centered) values.</summary>
    public void Reset()
    {
        A = B = X = Y = false;
        LeftBumper = RightBumper = false;
        LeftTrigger = RightTrigger = 0f;
        LeftStickX = LeftStickY = 0f;
        RightStickX = RightStickY = 0f;
        LeftStickClick = RightStickClick = false;
        DPadUp = DPadDown = DPadLeft = DPadRight = false;
        Start = Back = Guide = false;
    }
}
