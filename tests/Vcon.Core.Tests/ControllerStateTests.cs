using Vcon.Core.Models;
using Xunit;

namespace Vcon.Core.Tests;

public sealed class ControllerStateTests
{
    [Fact]
    public void Clone_ReturnsDeepCopy()
    {
        var original = new ControllerState
        {
            A = true,
            LeftStickX = 0.5f,
            RightTrigger = 0.75f,
            DPadUp = true,
        };

        var clone = original.Clone();

        Assert.Equal(original.A, clone.A);
        Assert.Equal(original.LeftStickX, clone.LeftStickX);
        Assert.Equal(original.RightTrigger, clone.RightTrigger);
        Assert.Equal(original.DPadUp, clone.DPadUp);

        original.A = false;
        original.LeftStickX = 0f;
        original.RightTrigger = 0f;
        original.DPadUp = false;

        Assert.True(clone.A);
        Assert.Equal(0.5f, clone.LeftStickX);
        Assert.Equal(0.75f, clone.RightTrigger);
        Assert.True(clone.DPadUp);
    }

    [Fact]
    public void Reset_ClearsAllValues()
    {
        var state = new ControllerState
        {
            A = true,
            B = true,
            X = true,
            Y = true,
            LeftBumper = true,
            RightBumper = true,
            LeftTrigger = 1f,
            RightTrigger = 1f,
            LeftStickX = 1f,
            LeftStickY = -1f,
            RightStickX = -0.5f,
            RightStickY = 0.25f,
            LeftStickClick = true,
            RightStickClick = true,
            DPadUp = true,
            DPadDown = true,
            DPadLeft = true,
            DPadRight = true,
            Start = true,
            Back = true,
            Guide = true,
        };

        state.Reset();

        Assert.False(state.A);
        Assert.False(state.B);
        Assert.False(state.X);
        Assert.False(state.Y);
        Assert.False(state.LeftBumper);
        Assert.False(state.RightBumper);
        Assert.Equal(0f, state.LeftTrigger);
        Assert.Equal(0f, state.RightTrigger);
        Assert.Equal(0f, state.LeftStickX);
        Assert.Equal(0f, state.LeftStickY);
        Assert.Equal(0f, state.RightStickX);
        Assert.Equal(0f, state.RightStickY);
        Assert.False(state.LeftStickClick);
        Assert.False(state.RightStickClick);
        Assert.False(state.DPadUp);
        Assert.False(state.DPadDown);
        Assert.False(state.DPadLeft);
        Assert.False(state.DPadRight);
        Assert.False(state.Start);
        Assert.False(state.Back);
        Assert.False(state.Guide);
    }

    [Fact]
    public void DefaultValues_AllFalseAndZero()
    {
        var state = new ControllerState();

        Assert.False(state.A);
        Assert.False(state.B);
        Assert.False(state.X);
        Assert.False(state.Y);
        Assert.False(state.LeftBumper);
        Assert.False(state.RightBumper);
        Assert.Equal(0f, state.LeftTrigger);
        Assert.Equal(0f, state.RightTrigger);
        Assert.Equal(0f, state.LeftStickX);
        Assert.Equal(0f, state.LeftStickY);
        Assert.Equal(0f, state.RightStickX);
        Assert.Equal(0f, state.RightStickY);
        Assert.False(state.LeftStickClick);
        Assert.False(state.RightStickClick);
        Assert.False(state.DPadUp);
        Assert.False(state.DPadDown);
        Assert.False(state.DPadLeft);
        Assert.False(state.DPadRight);
        Assert.False(state.Start);
        Assert.False(state.Back);
        Assert.False(state.Guide);
    }
}
