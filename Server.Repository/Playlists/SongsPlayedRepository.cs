using Cord.Server.Domain.Playlists;
using Cord.Server.Domain.Rooms;

namespace Cord.Server.Repository.Playlists;

public sealed class SongsPlayedRepository : Repository<SongPlayedModel>, ISongsPlayedRepository {
    protected override string CollectionName => "songHistory";

    public SongsPlayedRepository(MongoContext context) : base(null, context) {
        var def = Builders<SongPlayedModel>.IndexKeys.Descending(x => x.RoomId);
        Collection.Indexes.CreateOne(new CreateIndexModel<SongPlayedModel>(def));
    }

    public async Task<IReadOnlyCollection<SongPlayedModel>> GetForRoom(ID roomId, int limit) {
        return await Collection
            .Find(x => x.RoomId == roomId)
            .SortByDescending(x => x.Id)
            .Limit(limit)
            .ToListAsync();
    }

    public Task<long> TotalSongsPlayed() {
        return Collection.CountDocumentsAsync(_ => true);
    }

    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<SongPlayedModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.RoomId).SetIsRequired(true);
                map.MapMember(x => x.SongId).SetIsRequired(true);
                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.Upvotes).SetIsRequired(true);
                map.MapMember(x => x.Steals).SetIsRequired(true);
                map.MapMember(x => x.Downvotes).SetIsRequired(true);
            }
        );
    }
}
