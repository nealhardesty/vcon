using System.Text.Json;
using System.Text.Json.Serialization;
using Vcon.Core.Models;

namespace Vcon.Core.Configuration;

/// <summary>
/// JSON serialization helpers for profiles and application settings.
/// </summary>
public static class ProfileSerializer
{
    /// <summary>Shared serializer options: camelCase, enums as strings, indented.</summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
    };

    /// <summary>Serialize a <see cref="ControllerProfile"/> to a JSON string.</summary>
    public static string SerializeProfile(ControllerProfile profile)
    {
        ArgumentNullException.ThrowIfNull(profile);
        return JsonSerializer.Serialize(profile, Options);
    }

    /// <summary>Deserialize a <see cref="ControllerProfile"/> from a JSON string.</summary>
    public static ControllerProfile? DeserializeProfile(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<ControllerProfile>(json, Options);
    }

    /// <summary>Serialize <see cref="AppSettings"/> to a JSON string.</summary>
    public static string SerializeSettings(AppSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        return JsonSerializer.Serialize(settings, Options);
    }

    /// <summary>Deserialize <see cref="AppSettings"/> from a JSON string.</summary>
    public static AppSettings? DeserializeSettings(string json)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(json);
        return JsonSerializer.Deserialize<AppSettings>(json, Options);
    }
}
