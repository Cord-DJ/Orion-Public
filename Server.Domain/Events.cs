using Cord.Server.Domain.Equipment;
using Cord.Server.Domain.Users;

namespace Cord.Server.Domain;

public record RoomCreated(IRoom Room, IUser Creator) : IDomainEvent;

public record RoomUpdated(IRoom Room) : IDomainEvent;

public record RoomDeleted(IRoom Room) : IDomainEvent;

public record ActiveRoomSet(IUser User, IRoom Room, bool Exists) : IDomainEvent;

public record OnlineUserRemoved(IRoom Room, ID Id) : IDomainEvent;

public record MemberAdded(IRoom Room, IMember Member) : IDomainEvent;

public record MemberUpdated(IRoom Room, IMember Member) : IDomainEvent;

public record MemberRemoved(IRoom Room, IMember Member) : IDomainEvent;

public record MessageCreated(IRoom Room, IMessage Message) : IDomainEvent;

public record MessageDeleted(IRoom Room, ID MessageId) : IDomainEvent;

public record PlaylistCreated(IPlaylist Playlist) : IDomainEvent;

public record PlaylistUpdated(IPlaylist Playlist) : IDomainEvent;

public record PlaylistDeleted(IUser User, ID PlaylistId) : IDomainEvent;

public record ActivePlaylistSet(IUser User) : IDomainEvent;

public record UserUpdated(IUser User) : IDomainEvent;

public record ExperienceGained(IUser User, int Experience) : IDomainEvent;

public record LevelGained(IUser User, int Level) : IDomainEvent;

public record CurrentSongUpdated(IRoom Room, ICurrentSong? OldSong, ICurrentSong? NewSong) : IDomainEvent;

public record Voted(IRoom Room, IUser User, Vote Vote) : IDomainEvent;

public record QueueUpdated(IRoom Room) : IDomainEvent;

public record RoleCreated(IRoom Room, IRole Role) : IDomainEvent;

public record RoleUpdated(IRoom Room, IRole Role) : IDomainEvent;

public record RoleDeleted(IRoom Room, ID RoleId) : IDomainEvent;

// Task Kick(ID roomId, IUser user);
// Task CreateBan(ID roomId, IUser user); // TODO: send user dept?
// Task DeleteBan(ID roomId, IUser user);
// Task CreateMute(ID roomId, IUser user); // TODO: send user dept?
// Task DeleteMute(ID roomId, IUser user);

// Task UpdateQueue(ID roomId, IEnumerable<IUser> users);
// Task UpdateMessage(IMessage message);

public record UserCreated(IUser User) : IDomainEvent;

public record ChangeEmailRequested(IUser User, string? Token) : IDomainEvent;

public record ResetPasswordRequested(IUser User, string Token) : IDomainEvent;

public record PresetChanged(User User, Preset Preset, bool IsPrimary) : IDomainEvent;
