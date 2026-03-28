using static Vcon.Input.Native.NativeMethods;
using Xunit;

namespace Vcon.Input.Tests;

public sealed class NativeMethodsTests
{
    [Fact]
    public void ParseKeyName_Space_ReturnsVkSpace()
    {
        var vk = ParseKeyName("Space");

        Assert.Equal(VirtualKeyCode.VK_SPACE, vk);
    }

    [Theory]
    [InlineData("W", 0x57)]
    [InlineData("A", 0x41)]
    [InlineData("S", 0x53)]
    [InlineData("D", 0x44)]
    public void ParseKeyName_SingleLetter_ReturnsCorrectVk(string letter, ushort expectedCode)
    {
        var vk = ParseKeyName(letter);

        Assert.NotNull(vk);
        Assert.Equal(expectedCode, (ushort)vk.Value);
    }

    [Theory]
    [InlineData("Up", 0x26)]
    [InlineData("Down", 0x28)]
    [InlineData("Left", 0x25)]
    [InlineData("Right", 0x27)]
    public void ParseKeyName_ArrowKeys_ReturnsCorrectVk(string name, ushort expectedCode)
    {
        var vk = ParseKeyName(name);

        Assert.NotNull(vk);
        Assert.Equal(expectedCode, (ushort)vk.Value);
    }

    [Theory]
    [InlineData("LeftMouseButton", 0x01)]
    [InlineData("RightMouseButton", 0x02)]
    public void ParseKeyName_MouseButtons(string name, ushort expectedCode)
    {
        var vk = ParseKeyName(name);

        Assert.NotNull(vk);
        Assert.Equal(expectedCode, (ushort)vk.Value);
    }

    [Theory]
    [InlineData("space")]
    [InlineData("SPACE")]
    public void ParseKeyName_CaseInsensitive(string name)
    {
        var vk = ParseKeyName(name);

        Assert.Equal(VirtualKeyCode.VK_SPACE, vk);
    }

    [Fact]
    public void ParseKeyName_Unknown_ReturnsNull()
    {
        Assert.Null(ParseKeyName("NotARealKeyName123"));
    }

    [Fact]
    public void ParseKeyName_EmptyOrNull_ReturnsNull()
    {
        Assert.Null(ParseKeyName(""));
        Assert.Null(ParseKeyName("   "));
        Assert.Null(ParseKeyName(null!));
    }
}
