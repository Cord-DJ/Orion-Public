namespace Cord.Server.Domain.Rooms;

public interface IQueueRepository : IRepository<InQueueModel> {
    IAsyncEnumerable<ID> GetRoomsWithQueue();
    Task<IReadOnlyCollection<ID>> GetQueueForRoom(ID roomId);
    Task<bool> IsQueueEmpty(ID roomId);
    Task ReorderUsers(ID roomId, ID[] userIds);
    Task<ID?> PopNext(ID roomId);
    Task RemoveByRoom(ID roomId);
    Task RemoveUser(ID roomId, ID userId);
    Task<bool> IsInQueue(ID roomId, ID userId);
    Task<ID?> AddUser(ID roomId, ID userId);
}
