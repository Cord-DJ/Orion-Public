namespace Cord.Server.Domain.Hub;

public interface IGatewayAPI {
    Task Hello(IHello hello);

    // // eg. +42 XP, new level, etc
    Task ValueNotification(string type, string value);

    // send all users in room, queues, messages, roles, online users, current song
    Task Ready(IEnumerable<IRoom> rooms, IUser me);

    Task Kick(ID roomId, IUser user);

    Task CreatePlaylist(IPlaylist playlist);
    Task UpdatePlaylist(IPlaylist playlist);
    Task DeletePlaylist(ID id);

    Task CreateRoom(IRoom room);
    Task UpdateRoom(IRoom room);
    Task DeleteRoom(ID id);

    Task CreateRole(ID roomId, IRole role);
    Task UpdateRole(ID roomId, IRole role);
    Task DeleteRole(ID roomId, ID id);

    Task CreateBan(ID roomId, IUser user); // TODO: send user dept?
    Task DeleteBan(ID roomId, IUser user);

    Task CreateMute(ID roomId, IUser user); // TODO: send user dept?
    Task DeleteMute(ID roomId, IUser user);

    Task UpdateQueue(ID roomId, IEnumerable<IUser> users);

    Task CreateMessage(IMessage message);
    Task UpdateMessage(IMessage message);
    Task DeleteMessage(ID roomId, ID messageId);


    Task CreateMember(ID roomId, IMember member);
    Task UpdateMember(ID roomId, IMember member);
    Task DeleteMember(ID roomId, ID userId);

    // when users change his global avatar, name, etc.
    Task UpdateUser(IUser user);

    Task UpdateCurrentSong(ID roomId, ICurrentSong? song);
    Task UpdateVote(ID roomId, ID userId, Vote vote);

    // Send when user enters the room. Bots can enters multiple rooms at the same time
    Task CreateOnlineUser(ID roomId, IOnlineUser user);
    Task UpdateOnlineUser(ID roomId, IOnlineUser user);
    Task DeleteOnlineUser(ID roomId, ID id);

    Task CreateSongPlayed(ID roomId, ISongPlayed songPlayed);
}
