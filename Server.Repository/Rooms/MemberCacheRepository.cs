using Cord.Server.Domain.Rooms;
using Microsoft.Extensions.Caching.Distributed;

namespace Cord.Server.Repository.Rooms;

public sealed class MemberCacheRepository : CacheRepository<MemberModel>, IMemberRepository {
    new IMemberRepository Next => (base.Next as IMemberRepository)!;

    public MemberCacheRepository(IMemberRepository repository, IDistributedCache cache) : base(repository, cache) { }

    public async IAsyncEnumerable<MemberModel> GetAllMembers(ID roomId) {
        var cache = await GetCacheArray($"room#{roomId}");

        if (cache == null) {
            cache = await Next.GetAllMembers(roomId).ToArrayAsync();
            await SetCacheArray($"room#{roomId}", cache.ToArray());
        }

        foreach (var x in cache) {
            yield return x;
        }
    }

    public IAsyncEnumerable<ID> GetJoinedRoomIds(ID userId) => Next.GetJoinedRoomIds(userId);
    public Task<long> GetMembersCount(ID roomId) => Next.GetMembersCount(roomId);
    public Task RemoveByRoom(ID roomId) => Next.RemoveByRoom(roomId);
}
