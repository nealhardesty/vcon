using Vcon.Core.Models;
using Xunit;

namespace Vcon.Core.Tests;

public sealed class InputBindingTests
{
    [Fact]
    public void InputBinding_DefaultValues_AreNull()
    {
        var binding = new InputBinding();

        Assert.Null(binding.XInput);
        Assert.Null(binding.Keyboard);
        Assert.Null(binding.KeyboardDirectional);
    }

    [Fact]
    public void DirectionalBinding_SetsAllDirections()
    {
        var directional = new DirectionalBinding
        {
            Up = "W",
            Down = "S",
            Left = "A",
            Right = "D",
        };

        Assert.Equal("W", directional.Up);
        Assert.Equal("S", directional.Down);
        Assert.Equal("A", directional.Left);
        Assert.Equal("D", directional.Right);
    }
}
