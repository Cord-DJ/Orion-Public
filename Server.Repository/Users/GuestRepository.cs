using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;
using Microsoft.Extensions.Caching.Distributed;

namespace Cord.Server.Repository.Users;

public sealed class GuestRepository : CacheRepository<GuestModel, string>, IGuestRepository {
    public GuestRepository(IDistributedCache cache) : base(null, cache) { }
}
