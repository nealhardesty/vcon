using System.Text.Json;
using Vcon.Core.Configuration;
using Vcon.Core.Models;
using Xunit;

namespace Vcon.Core.Tests;

public sealed class ProfileSerializerTests
{
    [Fact]
    public void SerializeProfile_RoundTrips()
    {
        var profile = new ControllerProfile
        {
            Id = "test-profile",
            Name = "Test",
            Mode = InputMode.Keyboard,
            Opacity = 0.85f,
            Scale = 1.1f,
            Controls =
            [
                new ControlDefinition
                {
                    Id = "a-button",
                    Type = ControlType.Button,
                    Label = "A",
                    Position = new PositionInfo
                    {
                        X = 0.1, Y = 0.2,
                        HAnchor = HorizontalAnchor.Right,
                        VAnchor = VerticalAnchor.Bottom,
                    },
                    Size = new SizeInfo { Width = 48, Height = 48, Radius = 0 },
                    Binding = new InputBinding
                    {
                        XInput = "A",
                        Keyboard = "Space",
                    },
                },
            ],
        };

        var json = ProfileSerializer.SerializeProfile(profile);
        var roundTrip = ProfileSerializer.DeserializeProfile(json);

        AssertProfilesEqual(profile, roundTrip);
    }

    [Fact]
    public void DeserializeProfile_InvalidJson_ReturnsNull_OrThrows()
    {
        Assert.Throws<JsonException>(() => ProfileSerializer.DeserializeProfile("{not json"));

        Assert.Null(ProfileSerializer.DeserializeProfile("null"));
    }

    [Fact]
    public void SerializeSettings_RoundTrips()
    {
        var settings = new AppSettings
        {
            ActiveProfileId = "custom-id",
            StartMinimized = true,
            StartWithWindows = false,
            LogLevel = "Debug",
            Hotkeys = new HotkeySettings
            {
                ToggleOverlay = "F8",
                ToggleEditMode = "F9",
                CycleProfile = "F12",
            },
        };

        var json = ProfileSerializer.SerializeSettings(settings);
        var roundTrip = ProfileSerializer.DeserializeSettings(json);

        Assert.NotNull(roundTrip);
        Assert.Equal(settings.ActiveProfileId, roundTrip.ActiveProfileId);
        Assert.Equal(settings.StartMinimized, roundTrip.StartMinimized);
        Assert.Equal(settings.StartWithWindows, roundTrip.StartWithWindows);
        Assert.Equal(settings.LogLevel, roundTrip.LogLevel);
        Assert.Equal(settings.Hotkeys.ToggleOverlay, roundTrip.Hotkeys.ToggleOverlay);
        Assert.Equal(settings.Hotkeys.ToggleEditMode, roundTrip.Hotkeys.ToggleEditMode);
        Assert.Equal(settings.Hotkeys.CycleProfile, roundTrip.Hotkeys.CycleProfile);
    }

    [Fact]
    public void SerializeProfile_IncludesControlDefinitions()
    {
        var profile = new ControllerProfile
        {
            Id = "p",
            Name = "P",
            Controls =
            [
                new ControlDefinition
                {
                    Id = "x-button",
                    Type = ControlType.Button,
                    Binding = new InputBinding { XInput = "X", Keyboard = "X" },
                },
            ],
        };

        var json = ProfileSerializer.SerializeProfile(profile);

        Assert.Contains("\"controls\"", json);
        Assert.Contains("x-button", json);
    }

    [Fact]
    public void DeserializeProfile_CamelCaseProperties()
    {
        const string json = """
            {
              "id": "camel-id",
              "name": "Camel",
              "mode": "keyboard",
              "opacity": 0.9,
              "scale": 1.0,
              "controls": []
            }
            """;

        var profile = ProfileSerializer.DeserializeProfile(json);

        Assert.NotNull(profile);
        Assert.Equal("camel-id", profile.Id);
        Assert.Equal("Camel", profile.Name);
        Assert.Equal(InputMode.Keyboard, profile.Mode);
        Assert.Equal(0.9f, profile.Opacity);
        Assert.Equal(1.0f, profile.Scale);
        Assert.Empty(profile.Controls);
    }

    [Fact]
    public void NullProfile_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ProfileSerializer.SerializeProfile(null!));
    }

    private static void AssertProfilesEqual(ControllerProfile expected, ControllerProfile? actual)
    {
        Assert.NotNull(actual);
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.Name, actual.Name);
        Assert.Equal(expected.Mode, actual.Mode);
        Assert.Equal(expected.Opacity, actual.Opacity);
        Assert.Equal(expected.Scale, actual.Scale);
        Assert.Equal(expected.Controls.Count, actual.Controls.Count);

        for (var i = 0; i < expected.Controls.Count; i++)
        {
            var e = expected.Controls[i];
            var a = actual.Controls[i];
            Assert.Equal(e.Id, a.Id);
            Assert.Equal(e.Type, a.Type);
            Assert.Equal(e.Label, a.Label);
            Assert.Equal(e.DeadZone, a.DeadZone);
            Assert.Equal(e.Position.X, a.Position.X);
            Assert.Equal(e.Position.Y, a.Position.Y);
            Assert.Equal(e.Position.HAnchor, a.Position.HAnchor);
            Assert.Equal(e.Position.VAnchor, a.Position.VAnchor);
            Assert.Equal(e.Size.Width, a.Size.Width);
            Assert.Equal(e.Size.Height, a.Size.Height);
            Assert.Equal(e.Size.Radius, a.Size.Radius);

            if (e.Style is null)
                Assert.Null(a.Style);
            else
            {
                Assert.NotNull(a.Style);
                Assert.Equal(e.Style.Fill, a.Style.Fill);
                Assert.Equal(e.Style.Stroke, a.Style.Stroke);
            }

            Assert.Equal(e.Binding.XInput, a.Binding.XInput);
            Assert.Equal(e.Binding.Keyboard, a.Binding.Keyboard);

            if (e.Binding.KeyboardDirectional is null)
                Assert.Null(a.Binding.KeyboardDirectional);
            else
            {
                Assert.NotNull(a.Binding.KeyboardDirectional);
                Assert.Equal(e.Binding.KeyboardDirectional.Up, a.Binding.KeyboardDirectional.Up);
                Assert.Equal(e.Binding.KeyboardDirectional.Down, a.Binding.KeyboardDirectional.Down);
                Assert.Equal(e.Binding.KeyboardDirectional.Left, a.Binding.KeyboardDirectional.Left);
                Assert.Equal(e.Binding.KeyboardDirectional.Right, a.Binding.KeyboardDirectional.Right);
            }
        }
    }
}
