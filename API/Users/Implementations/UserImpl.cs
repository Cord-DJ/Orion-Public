using Cord.Equipment;
using Newtonsoft.Json;
using System.Text;

namespace Cord;

sealed class UserImpl : IUser, ISnowflakeEntity {
    internal readonly List<IPlaylist> playlists = new();

    public ID Id { get; init; }
    public UserProperties? Properties { get; init; }

    public string? Email { get; init; }
    public ID? ActivePlaylistId { get; init; }

    public string Name { get; init; } = default!;
    public int Discriminator { get; init; }
    public string? Avatar { get; init; }
    public string? Banner { get; init; }

    public ICharacter? Character { get; }

    public int? Exp { get; init; }
    public int? MaxExp { get; init; }
    public int? Level { get; init; }
    public IBoost? Boost { get; }
    public IUserStats? Stats { get; }

    public IReadOnlyList<IPlaylist>? Playlists => playlists.AsReadOnly();

    public UserImpl( /*AvatarInfo avatarInfo, */ BoostImpl boost, PlaylistImpl[]? playlists) {
        // AvatarInfo = avatarInfo;
        Boost = boost;

        if (playlists != null) {
            this.playlists.AddRange(playlists);
        }
    }

    public override string ToString() => $"<@{Id}>";

    public async Task Update(UpdateUser update) {
        var json = JsonConvert.SerializeObject(update);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        // TODO: use ID instead of me?
        var response = await CordClient.Http.PatchAsync("/api/users/@me", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task UpdatePassword(string currentPassword, string newPassword) {
        var json = JsonConvert.SerializeObject(new { CurrentPassword = currentPassword, NewPassword = newPassword });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        // TODO: use ID instead of me?
        var response = await CordClient.Http.PatchAsync("/api/users/@me", content);

        response.EnsureSuccessStatusCode();
    }

    public async Task<IPlaylist> CreatePlaylist(string name) {
        var json = JsonConvert.SerializeObject(new { Name = name });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PostAsync("/api/playlists", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<PlaylistImpl>();
    }

    public async Task<IPlaylist> ImportPlaylist(string id, ImportType type) {
        var json = JsonConvert.SerializeObject(new { Id = id, Type = type });
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await CordClient.Http.PostAsync("/api/playlists/import", content);

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<PlaylistImpl>();
    }

    public override bool Equals(object? obj) => obj is IUser entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
