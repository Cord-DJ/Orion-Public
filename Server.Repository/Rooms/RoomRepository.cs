using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;
using MongoAsyncEnumerableAdapter;
using MongoDB.Bson.Serialization.Serializers;

namespace Cord.Server.Repository.Rooms;

public sealed class RoomRepository : Repository<RoomModel>, IRoomRepository {
    IMongoCollection<VoteModel> VoteCollection { get; }
    IMongoCollection<StealModel> StealCollection { get; }

    public RoomRepository(MongoContext context) : base(null, context) {
        StealCollection = GetCollection<StealModel>();
        VoteCollection = GetCollection<VoteModel>();

        var indexDefinition = Builders<RoomModel>.IndexKeys.Combine(
            Builders<RoomModel>.IndexKeys.Descending(x => x.Link)
        );
        Collection.Indexes.CreateOne(new CreateIndexModel<RoomModel>(indexDefinition, new() { Unique = true }));

        // Votes
        var voteDef = Builders<VoteModel>.IndexKeys.Combine(
            Builders<VoteModel>.IndexKeys.Descending(x => x.RoomId),
            Builders<VoteModel>.IndexKeys.Descending(x => x.UserId)
        );
        VoteCollection.Indexes.CreateOne(new CreateIndexModel<VoteModel>(voteDef, new() { Unique = true }));
        
        voteDef = Builders<VoteModel>.IndexKeys.Ascending(x => x.RoomId);
        VoteCollection.Indexes.CreateOne(new CreateIndexModel<VoteModel>(voteDef));

        // Steals
        var stealDef = Builders<StealModel>.IndexKeys.Combine(
            Builders<StealModel>.IndexKeys.Descending(x => x.RoomId),
            Builders<StealModel>.IndexKeys.Descending(x => x.UserId)
        );
        StealCollection.Indexes.CreateOne(new CreateIndexModel<StealModel>(stealDef, new() { Unique = true }));
        
        stealDef = Builders<StealModel>.IndexKeys.Ascending(x => x.RoomId);
        StealCollection.Indexes.CreateOne(new CreateIndexModel<StealModel>(stealDef));
    }

    public async IAsyncEnumerable<RoomModel> GetAllRooms() {
        var cursor = await Collection.FindAsync(_ => true);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public async IAsyncEnumerable<RoomModel> GetPopularRooms() {
        var cursor = await Collection.FindAsync(x => x.Icon != null && x.Banner != null);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public Task<long> PopularRoomsCount() {
        return Collection.Find(x => x.Icon != null && x.Banner != null).CountDocumentsAsync();
    }

    public async IAsyncEnumerable<RoomModel> GetRoomsWithExpiredSong() {
        var cursor =
            await Collection.FindAsync(x => x.CurrentSong != null && x.CurrentSong.EndTime <= DateTimeOffset.UtcNow);

        await foreach (var x in cursor.ToAsyncEnumerable().Distinct()) {
            yield return x;
        }
    }

    public Task<RoomModel> GetRoomByLink(string? link) {
        return Collection.Find(x => x.Link == link).SingleAsync();
    }

    public async Task<bool> ExistsLink(string? link) {
        if (link == null) {
            return false;
        }

        return await Collection.Find(x => x.Link == link).SingleOrDefaultAsync() != null;
    }

    public async IAsyncEnumerable<RoomModel> GetBasicRoomInfo(IEnumerable<ID> roomsId) {
        var cursor = await Collection.FindAsync(x => roomsId.Contains(x.Id));

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public Task Vote(ID roomId, ID userId, Vote vote) {
        if (vote == Cord.Vote.Steal) {
            return StealCollection.ReplaceOneAsync(
                x => x.UserId.Equals(userId),
                new(roomId, userId),
                new ReplaceOptions { IsUpsert = true }
            );
        }

        return VoteCollection.ReplaceOneAsync(
            x => x.UserId.Equals(userId),
            new(roomId, userId, vote),
            new ReplaceOptions { IsUpsert = true }
        );
    }

    public async Task ResetVotes(ID roomId) {
        await VoteCollection.DeleteManyAsync(x => x.RoomId == roomId);
        await StealCollection.DeleteManyAsync(x => x.RoomId == roomId);
    }

    public async Task<IDictionary<Vote, List<ID>>> GetVotesForRoom(ID roomId) {
        var votes = await VoteCollection.Find(x => x.RoomId == roomId).ToListAsync();
        var steals = await StealCollection.Find(x => x.RoomId == roomId).ToListAsync();

        var ret = new Dictionary<Vote, List<ID>> {
            { Cord.Vote.Upvote, new() }, { Cord.Vote.Steal, new() }, { Cord.Vote.Downvote, new() }
        };

        foreach (var vote in votes) {
            ret[vote.Vote].Add(vote.UserId);
        }

        foreach (var steal in steals) {
            ret[Cord.Vote.Steal].Add(steal.UserId);
        }

        return ret;
    }

    public async Task RemoveByRoom(ID roomId) {
        await VoteCollection.DeleteManyAsync(x => x.RoomId == roomId);
        await StealCollection.DeleteManyAsync(x => x.RoomId == roomId);
    }

    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IRoleSettings, RoleSettings>());
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<ICurrentSong, CurrentSong>());
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IRole, Role>());

        BsonClassMap.RegisterClassMap<Role>(
            map => {
                map.AutoMap();
                map.SetIgnoreExtraElements(true);

                map.MapCreator(x => new(x.Id, x.Position, x.Name, x.Color, RoleSettings.Default, x.Permissions));
            }
        );

        BsonClassMap.RegisterClassMap<CurrentSong>(
            map => {
                map.MapMember(x => x.Song);
                map.MapMember(x => x.UserId);
                map.MapMember(x => x.EndTime);

                map.MapCreator(x => new((Song)x.Song, x.UserId, x.EndTime, new(), new(), new()));
            }
        );

        BsonClassMap.RegisterClassMap<RoomModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.OwnerId).SetIsRequired(true);
                map.MapMember(x => x.Name).SetIsRequired(true);
                map.MapMember(x => x.Link).SetIsRequired(true);
                map.MapMember(x => x.Description).SetIsRequired(true);
                map.MapMember(x => x.Categories);
                map.MapMember(x => x.Features);

                map.MapMember(x => x.CurrentSong);

                map.MapMember(x => x.Icon);
                map.MapMember(x => x.Banner);
                map.MapMember(x => x.Wallpaper);

                map.MapMember(x => x.Roles);
                map.MapMember(x => x.Banned);
                map.MapMember(x => x.Muted);
            }
        );

        BsonClassMap.RegisterClassMap<VoteModel>(
            map => {
                map.MapIdProperty(x => x.UserId);

                map.MapMember(x => x.RoomId).SetIsRequired(true);
                map.MapMember(x => x.Vote).SetIsRequired(true);
            }
        );

        BsonClassMap.RegisterClassMap<StealModel>(
            map => {
                map.MapIdProperty(x => x.UserId);
                map.MapMember(x => x.RoomId).SetIsRequired(true);
            }
        );
    }
}
