namespace Cord;

public interface IRoom {
    ID Id { get; }
    ID OwnerId { get; }
    string Name { get; }
    string Link { get; }
    string? Description { get; }
    IReadOnlyCollection<Category> Categories { get; }
    RoomFeature Features { get; }

    string? Icon { get; }
    string? Banner { get; }
    string? Wallpaper { get; }

    long? MemberCount { get; }
    long? OnlineCount { get; }

    ICurrentSong? CurrentSong { get; }

    IReadOnlyCollection<IOnlineUser>? OnlineUsers { get; }
    IReadOnlyCollection<IMember>? Members { get; }
    IReadOnlyCollection<IUser>? Queue { get; }
    IReadOnlyCollection<IRole> Roles { get; }

    IReadOnlyCollection<IUser>? Banned { get; }
    IReadOnlyCollection<IUser>? Muted { get; }

    IReadOnlyCollection<IMessage>? Messages { get; }

    IReadOnlyCollection<ISongPlayed>? SongHistory { get; }

    Task ReorderQueue(ID[] ids);
    Task AddToQueue(IUser user);
    Task RemoveFromQueue(IUser user);


    Task AddMember(IUser user);
    Task RemoveMember(IUser user);

    Task Vote(Vote vote);
    Task Update(UpdateRoom room);
    Task Delete();

    Task Kick(IUser user);
    Task Ban(IUser user, TimeSpan? duration);
    Task Mute(IUser user, TimeSpan? duration);
    Task Unban(IUser user);
    Task Unmute(IUser user);

    Task<IRole> CreateRole();
    Task DeleteRole(IRole role);
    Task<IRole> UpdateRole(IRole role, UpdateRole values);
    Task ReorderRoles(IRole[] roles);

    Task<IMessage> SendMessage(string message);
}
