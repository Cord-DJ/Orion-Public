using Cord.Server.Domain.Equipment;
using Microsoft.Extensions.Caching.Distributed;

namespace Cord.Server.Repository.Equipment;

public sealed class ItemCacheRepository : CacheRepository<Item, ID>, IItemRepository {
    public ItemCacheRepository(IItemRepository next, IDistributedCache cache) : base(next, cache) { }
}
