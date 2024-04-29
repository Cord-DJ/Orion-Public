using Cord.Server.Domain;
using Cord.Server.Domain.Rooms;
using Cord.Server.Domain.Users;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Rooms;

public sealed class OnlineRepository : Repository<OnlineModel>, IOnlineRepository {
    public OnlineRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<OnlineModel>.IndexKeys.Combine(
            Builders<OnlineModel>.IndexKeys.Descending(x => x.RoomId),
            Builders<OnlineModel>.IndexKeys.Descending(x => x.UserId)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<OnlineModel>(indexDefinition, new() { Unique = true }));
        
        indexDefinition = Builders<OnlineModel>.IndexKeys.Ascending(x => x.RoomId);
        Collection.Indexes.CreateOne(new CreateIndexModel<OnlineModel>(indexDefinition));
        
        indexDefinition = Builders<OnlineModel>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<OnlineModel>(indexDefinition));
    }

    public async IAsyncEnumerable<(ID, Position)> GetOnlineUsers(ID roomId) {
        var cursor = await Collection.FindAsync(x => x.RoomId == roomId);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return (x.UserId, x.Position);
        }
    }

    public Task<long> GetOnlineUsersCount(ID roomId) {
        return Collection.Find(x => x.RoomId == roomId).CountDocumentsAsync();
    }

    public long GetOnlineNonGuestUsersCount(ID roomId) {
        var users = GetCollection<UserModel>();

        var query = from o in Collection.AsQueryable()
            join u in users.AsQueryable()
                on o.UserId equals u.Id
            where u.Email != "guest" && o.RoomId == roomId
            select new { o.Id };

        return query.Count();
    }

    public async Task<long> PingUser(ID userId, DateTimeOffset now) {
        var res = await Collection.UpdateManyAsync(
            x => x.UserId == userId,
            Builders<OnlineModel>.Update.Set(x => x.LastPing, now)
        );

        return res.ModifiedCount;
    }

    public async IAsyncEnumerable<ID> GetByUser(ID userId) {
        var cursor = await Collection.FindAsync(x => x.UserId == userId);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x.RoomId;
        }
    }

    public async Task<bool> SetActiveRoom(ID userId, ID roomId, Position position, bool isBot) {
        var result = await Collection.DeleteOneAsync(x => x.UserId == userId && x.RoomId == roomId);
        await Add(new(ID.NewId(), userId, roomId, position, DateTimeOffset.Now, isBot));

        return result.DeletedCount != 0;

        // Collection.ReplaceOneAsync(x => x.UserId == userId && x.RoomId == roomId,
        //     new OnlineModel(Id.NewId(), userId, roomId, position, null),
        //     new ReplaceOptions {
        //         IsUpsert = true
        //     }
        // );
    }

    public Task RemoveByRoom(ID roomId) {
        return Collection.DeleteManyAsync(x => x.RoomId == roomId);
    }

    public Task RemoveActiveRoom(ID userId, ID roomId) {
        return Collection.DeleteOneAsync(x => x.UserId == userId && x.RoomId == roomId);
    }

    public async IAsyncEnumerable<ID> GetDisconnectingUsers(TimeSpan duration) {
        var users = await Collection.FindAsync(x => x.LastPing < DateTimeOffset.UtcNow.Subtract(duration) && !x.IsBot);
        await foreach (var x in users.ToAsyncEnumerable()) {
            yield return x.UserId;
        }
    }

    public async Task<bool> HasOnlineUser(ID userId, ID roomId) {
        return await Collection.Find(x => x.UserId == userId && x.RoomId == roomId).CountDocumentsAsync() != 0;
    }

    public Task<long> TotalOnline() {
        return Collection.CountDocumentsAsync(_ => true);
    }

    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<OnlineModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.RoomId).SetIsRequired(true);
                map.MapMember(x => x.Position).SetIsRequired(true);
                map.MapMember(x => x.LastPing).SetSerializer(new DateTimeOffsetSerializer());
                map.MapMember(x => x.IsBot);
            }
        );
    }
}
