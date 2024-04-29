namespace Cord.Server.Domain.Rooms;

public interface IOnlineRepository : IRepository<OnlineModel> {
    IAsyncEnumerable<(ID, Position)> GetOnlineUsers(ID roomId);
    Task<long> GetOnlineUsersCount(ID roomId);
    long GetOnlineNonGuestUsersCount(ID roomId);
    IAsyncEnumerable<ID> GetByUser(ID userId);
    Task<bool> SetActiveRoom(ID userId, ID roomId, Position position, bool isBot);
    Task<long> PingUser(ID userId, DateTimeOffset now);

    Task RemoveByRoom(ID roomId);
    Task RemoveActiveRoom(ID userId, ID roomId);
    IAsyncEnumerable<ID> GetDisconnectingUsers(TimeSpan duration);

    Task<bool> HasOnlineUser(ID userId, ID roomId);

    Task<long> TotalOnline();
}
