using Cord.Equipment;

namespace Cord.Server.Domain.Users;

public sealed record MinimalUser : IUser {
    public static readonly MinimalUser System = new(new(1), 0, "System");

    public ID Id { get; }
    public UserProperties? Properties => null;
    public string? Email => null;

    public ID? ActivePlaylistId => null;
    public string Name { get; }
    public int Discriminator { get; }
    public string? Avatar { get; internal init; }
    public string? Banner => null;
    public ICharacter? Character { get; internal set; }

    public int? Exp => null;
    public int? MaxExp => null;
    public int? Level => null;
    public IBoost? Boost => null;
    public IUserStats? Stats => null;

    public IReadOnlyList<IPlaylist>? Playlists => null;

    internal MinimalUser(ID id, int discriminator, string name) {
        Id = id;
        Discriminator = discriminator;
        Name = name;
    }

    public Task<IPlaylist> CreatePlaylist(string name) => throw new NotImplementedException();
    public Task<IPlaylist> ImportPlaylist(string id, ImportType type) => throw new NotImplementedException();
    public Task UpdatePassword(string currentPassword, string newPassword) => throw new NotImplementedException();
    public Task Update(UpdateUser update) => throw new NotImplementedException();
}
