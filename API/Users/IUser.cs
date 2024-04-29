using Cord.Equipment;

namespace Cord;

public interface IUser {
    ID Id { get; }
    UserProperties? Properties { get; }

    // These are send to the owner
    string? Email { get; }
    ID? ActivePlaylistId { get; }

    string Name { get; }
    int Discriminator { get; }
    string? Avatar { get; }
    string? Banner { get; }

    ICharacter? Character { get; }

    int? Exp { get; }
    int? MaxExp { get; }
    int? Level { get; }
    IBoost? Boost { get; }
    IUserStats? Stats { get; }

    IReadOnlyList<IPlaylist>? Playlists { get; }

    IPlaylist? ActivePlaylist => Playlists?.Where(x => x.Id == ActivePlaylistId).FirstOrDefault();


    /// These are getters
//   isStaff: boolean;
// //   hasTurbo: boolean;
// //   earlySupporter: boolean;

// //   avatarUrl: string;
// //   bannerUrl: string | null;
// //   /// ==========

    // TODO: use this also for member
    Task Update(UpdateUser update);

    Task UpdatePassword(string currentPassword, string newPassword);

    Task<IPlaylist> CreatePlaylist(string name);
    Task<IPlaylist> ImportPlaylist(string id, ImportType type);
}
