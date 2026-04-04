using System.IO;
using Microsoft.Extensions.Logging;
using Vcon.Core.Abstractions;
using Vcon.Core.Configuration;
using Vcon.Core.Models;

namespace Vcon.Overlay.Services;

/// <summary>
/// File-based <see cref="IProfileManager"/> storing profiles as JSON under %APPDATA%/vcon/profiles/.
/// </summary>
public sealed class ProfileManager : IProfileManager
{
    private readonly string _profilesDir;
    private readonly string _defaultProfilesDir;
    private readonly ILogger<ProfileManager> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private ControllerProfile _activeProfile;

    /// <inheritdoc />
    public ControllerProfile ActiveProfile => _activeProfile;

    /// <inheritdoc />
    public string ProfilesDirectory => _profilesDir;

    /// <inheritdoc />
    public event EventHandler<ControllerProfile>? ActiveProfileChanged;

    public ProfileManager(ILogger<ProfileManager> logger)
    {
        _logger = logger;

        _profilesDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "vcon", "profiles");

        _defaultProfilesDir = Path.Combine(AppContext.BaseDirectory, "profiles");
        _activeProfile = CreateFallbackProfile();

        EnsureProfileDirectory();
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ControllerProfile>> GetAllProfilesAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var profiles = new List<ControllerProfile>();
            var files = Directory.GetFiles(_profilesDir, "*.json");

            foreach (var file in files)
            {
                try
                {
                    var json = await File.ReadAllTextAsync(file, ct);
                    var profile = ProfileSerializer.DeserializeProfile(json);
                    if (profile is not null)
                        profiles.Add(profile);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load profile from {File}", file);
                }
            }

            return profiles;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ControllerProfile?> LoadProfileAsync(string profileId, CancellationToken ct = default)
    {
        var path = GetProfilePath(profileId);
        if (!File.Exists(path))
            return null;

        await _lock.WaitAsync(ct);
        try
        {
            var json = await File.ReadAllTextAsync(path, ct);
            return ProfileSerializer.DeserializeProfile(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load profile '{ProfileId}'", profileId);
            return null;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveProfileAsync(ControllerProfile profile, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(profile);

        await _lock.WaitAsync(ct);
        try
        {
            var path = GetProfilePath(profile.Id);
            var tmpPath = path + ".tmp";
            var json = ProfileSerializer.SerializeProfile(profile);

            await File.WriteAllTextAsync(tmpPath, json, ct);
            File.Move(tmpPath, path, overwrite: true);

            _logger.LogDebug("Saved profile '{ProfileId}'", profile.Id);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<bool> DeleteProfileAsync(string profileId, CancellationToken ct = default)
    {
        var path = GetProfilePath(profileId);
        if (!File.Exists(path))
            return false;

        await _lock.WaitAsync(ct);
        try
        {
            File.Delete(path);
            _logger.LogInformation("Deleted profile '{ProfileId}'", profileId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete profile '{ProfileId}'", profileId);
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<ControllerProfile> CloneProfileAsync(string sourceId, string? newName = null, CancellationToken ct = default)
    {
        var source = await LoadProfileAsync(sourceId, ct)
            ?? throw new InvalidOperationException($"Source profile '{sourceId}' not found.");

        var json = ProfileSerializer.SerializeProfile(source);
        var clone = ProfileSerializer.DeserializeProfile(json)
            ?? throw new InvalidOperationException("Failed to deep-copy profile during clone.");

        clone.Id = GenerateUniqueId(sourceId);
        clone.Name = newName ?? $"{source.Name} (copy)";

        await SaveProfileAsync(clone, ct);
        _logger.LogInformation("Cloned profile '{SourceId}' -> '{CloneId}'", sourceId, clone.Id);
        return clone;
    }

    /// <inheritdoc />
    public async Task SwitchProfileAsync(string profileId, CancellationToken ct = default)
    {
        var profile = await LoadProfileAsync(profileId, ct);

        if (profile is null)
        {
            _logger.LogWarning("Profile '{ProfileId}' not found, using fallback", profileId);
            profile = CreateFallbackProfile();
        }

        _activeProfile = profile;
        ActiveProfileChanged?.Invoke(this, profile);
    }

    private string GenerateUniqueId(string baseId)
    {
        var candidate = $"{baseId}-copy";
        var suffix = 2;

        while (File.Exists(GetProfilePath(candidate)))
        {
            candidate = $"{baseId}-copy-{suffix}";
            suffix++;
        }

        return candidate;
    }

    private void EnsureProfileDirectory()
    {
        Directory.CreateDirectory(_profilesDir);

        if (Directory.GetFiles(_profilesDir, "*.json").Length > 0)
            return;

        if (!Directory.Exists(_defaultProfilesDir))
            return;

        foreach (var file in Directory.GetFiles(_defaultProfilesDir, "*.json"))
        {
            var dest = Path.Combine(_profilesDir, Path.GetFileName(file));
            try
            {
                File.Copy(file, dest, overwrite: false);
                _logger.LogInformation("Copied default profile {File}", Path.GetFileName(file));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to copy default profile {File}", Path.GetFileName(file));
            }
        }
    }

    private string GetProfilePath(string profileId)
        => Path.Combine(_profilesDir, $"{profileId}.json");

    private static ControllerProfile CreateFallbackProfile() => new()
    {
        Id = "xbox-standard",
        Name = "Xbox Standard",
        Mode = InputMode.XInput,
        Opacity = 0.7f,
        Scale = 1.0f,
        Controls = [],
    };
}
