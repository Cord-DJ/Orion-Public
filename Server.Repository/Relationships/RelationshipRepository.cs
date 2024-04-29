using Cord.Server.Domain.Relationships;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Relationships;

public sealed class RelationshipRepository : Repository<Relationship>, IRelationshipRepository {
    public RelationshipRepository(MongoContext context) : base(null, context) { }

    // public Task<long> TotalSongs() {
    //     return Collection.CountDocumentsAsync(_ => true);
    // }

    public async IAsyncEnumerable<Relationship> GetUserRelationships(ID userId) {
        var cursor = await Collection.FindAsync(x => x.UserId == userId);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public static void CreateMapping() {
        // BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<ISong, Song>());

        BsonClassMap.RegisterClassMap<Relationship>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.OwnerId).SetIsRequired(true);
                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.Type).SetIsRequired(true);
            }
        );
    }
}
