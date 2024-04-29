namespace Cord.Server.Domain.Users;

public sealed record OnlineUser(
    ID Id,
    IPosition Position,
    IUser? User
) : IOnlineUser { }
