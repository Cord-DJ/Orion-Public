using Cord.Server.Domain.Playlists;
using MongoDB.Bson.Serialization.Serializers;

namespace Cord.Server.Repository.Playlists;

public sealed class SongRepository : Repository<Song>, ISongRepository {
    public SongRepository(MongoContext context) : base(null, context) { }

    public async Task<Song?> GetByYoutubeId(string youtubeId) {
        // FIXME: this needs to be first cuz there was a bug which allowed to add same songs multiple times
        return await Collection.Find(x => x.YoutubeId == youtubeId).FirstOrDefaultAsync();
    }

    public Task<long> TotalSongs() {
        return Collection.CountDocumentsAsync(_ => true);
    }

    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<ISong, Song>());

        BsonClassMap.RegisterClassMap<Song>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.YoutubeId).SetIsRequired(true);
                map.MapMember(x => x.Author).SetIsRequired(true);
                map.MapMember(x => x.Name).SetIsRequired(true);
                map.MapMember(x => x.Duration).SetIsRequired(true);
            }
        );
    }
}
