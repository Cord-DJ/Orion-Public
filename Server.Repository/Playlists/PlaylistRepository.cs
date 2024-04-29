using Cord.Server.Domain.Playlists;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Playlists;

public sealed class PlaylistRepository : Repository<PlaylistModel>, IPlaylistRepository {
    public PlaylistRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<PlaylistModel>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<PlaylistModel>(indexDefinition));
    }

    public async Task<PlaylistModel?> GetUserPlaylist(ID userId, ID id) {
        return await Collection.Find(x => x.Id == id && x.UserId == userId).SingleOrDefaultAsync();
    }

    public async IAsyncEnumerable<PlaylistModel> GetUserPlaylists(ID userId) {
        var cursor = await Collection.FindAsync(x => x.UserId == userId);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public Task<long> TotalPlaylists() {
        return Collection.CountDocumentsAsync(_ => true);
    }

    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<PlaylistModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.Name).SetIsRequired(true);

                map.MapMember(x => x.NextSongId);
                map.MapMember(x => x.SongIds);
                map.MapMember(x => x.IsProcessing);
            }
        );
    }
}
