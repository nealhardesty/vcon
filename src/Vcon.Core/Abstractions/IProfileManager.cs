using Vcon.Core.Models;

namespace Vcon.Core.Abstractions;

/// <summary>
/// Manages CRUD operations and active-profile switching for controller profiles.
/// </summary>
public interface IProfileManager
{
    /// <summary>Get all available profile summaries.</summary>
    Task<IReadOnlyList<ControllerProfile>> GetAllProfilesAsync(CancellationToken ct = default);

    /// <summary>Load a profile by its ID.</summary>
    Task<ControllerProfile?> LoadProfileAsync(string profileId, CancellationToken ct = default);

    /// <summary>Save a profile (create or overwrite).</summary>
    Task SaveProfileAsync(ControllerProfile profile, CancellationToken ct = default);

    /// <summary>Delete a profile by ID.</summary>
    Task<bool> DeleteProfileAsync(string profileId, CancellationToken ct = default);

    /// <summary>Clone an existing profile, assigning a new unique ID and optional display name.</summary>
    Task<ControllerProfile> CloneProfileAsync(string sourceId, string? newName = null, CancellationToken ct = default);

    /// <summary>Get the currently active profile.</summary>
    ControllerProfile ActiveProfile { get; }

    /// <summary>Absolute path to the user profiles directory.</summary>
    string ProfilesDirectory { get; }

    /// <summary>Switch to a different profile by ID.</summary>
    Task SwitchProfileAsync(string profileId, CancellationToken ct = default);

    /// <summary>Raised when the active profile changes.</summary>
    event EventHandler<ControllerProfile>? ActiveProfileChanged;
}
