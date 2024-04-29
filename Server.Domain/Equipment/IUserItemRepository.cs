namespace Cord.Server.Domain.Equipment;

public interface IItemInstanceRepository : IRepository<ItemInstance> {
    IAsyncEnumerable<ItemInstance> GetForUser(ID userId);
}
