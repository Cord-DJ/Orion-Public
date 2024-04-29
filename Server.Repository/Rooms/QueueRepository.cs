using Cord.Server.Domain.Rooms;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Rooms;

public sealed class QueueRepository : Repository<InQueueModel>, IQueueRepository {
    public QueueRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<InQueueModel>.IndexKeys.Combine(
            Builders<InQueueModel>.IndexKeys.Descending(x => x.RoomId),
            Builders<InQueueModel>.IndexKeys.Descending(x => x.UserId)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<InQueueModel>(indexDefinition, new() { Unique = true }));
        
        indexDefinition = Builders<InQueueModel>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<InQueueModel>(indexDefinition));
    }

    public async IAsyncEnumerable<ID> GetRoomsWithQueue() {
        var cursor = await Collection.FindAsync(_ => true);

        await foreach (var x in cursor.ToAsyncEnumerable().Select(x => x.RoomId).Distinct()) {
            yield return x;
        }
    }

    public async Task<IReadOnlyCollection<ID>> GetQueueForRoom(ID roomId) {
        var queue = await Collection.Find(x => x.RoomId == roomId).SortBy(x => x.Id).ToListAsync();
        return queue.Select(x => x.UserId).ToList();
    }

    public async Task<bool> IsQueueEmpty(ID roomId) {
        var next = await Collection.Find(x => x.RoomId == roomId).CountDocumentsAsync();
        return next == 0;
    }

    public async Task<ID?> PopNext(ID roomId) {
        var next = await Collection.Find(x => x.RoomId == roomId).SortBy(x => x.Id).FirstOrDefaultAsync();
        if (next == null) {
            return null;
        }

        await Remove(next.Id);
        return next.UserId;
    }

    public async Task ReorderUsers(ID roomId, ID[] userIds) {
        await Collection.DeleteManyAsync(x => x.RoomId == roomId);

        var models = new List<InQueueModel>();
        foreach (var id in userIds) {
            models.Add(new(ID.NewId(), id, roomId));
        }

        await Collection.InsertManyAsync(models);
    }

    public async Task<bool> IsInQueue(ID roomId, ID userId) {
        return await Collection.Find(x => x.RoomId == roomId && x.UserId == userId).SingleOrDefaultAsync() != null;
    }

    public Task RemoveByRoom(ID roomId) {
        return Collection.DeleteManyAsync(x => x.RoomId == roomId);
    }

    public Task RemoveUser(ID roomId, ID userId) {
        return Collection.DeleteOneAsync(x => x.RoomId == roomId && x.UserId == userId);
    }

    public async Task<ID?> AddUser(ID roomId, ID userId) {
        // Check and remove from existing queue
        var existing = await Collection.Find(x => x.UserId == userId).SingleOrDefaultAsync();
        if (existing != null) {
            await Remove(existing);
        }

        await Add(new(ID.NewId(), userId, roomId));
        return existing?.RoomId;
    }

    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<InQueueModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.RoomId).SetIsRequired(true);
            }
        );
    }
}
