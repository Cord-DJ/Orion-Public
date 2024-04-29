namespace Cord;

public interface IOnlineUser {
    // user id
    ID Id { get; }
    IPosition Position { get; }

    // User is send only if online user is not member of room
    IUser? User { get; }
}
