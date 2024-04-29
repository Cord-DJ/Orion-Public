using Cord.Equipment;
using Cord.Server.Domain.Equipment;
using MongoAsyncEnumerableAdapter;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Cord.Server.Repository.Equipment;

public sealed class PresetRepository : Repository<Preset>, IPresetRepository {
    public PresetRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<Preset>.IndexKeys.Combine(
            Builders<Preset>.IndexKeys.Descending(x => x.UserId),
            Builders<Preset>.IndexKeys.Descending(x => x.Position)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<Preset>(indexDefinition, new() { Unique = true }));
        
        indexDefinition = Builders<Preset>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<Preset>(indexDefinition));
    }

    public async IAsyncEnumerable<Preset> GetForUser(ID userId) {
        var query = await Collection.FindAsync(x => x.UserId == userId);

        await foreach (var x in query.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public Task<Preset> GetPreset(ID userId, int position) {
        return Collection.Find(x => x.UserId == userId && x.Position == position).SingleAsync();
    }

    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(typeof(SlotType), new EnumSerializer<SlotType>(BsonType.String));
        BsonClassMap.RegisterClassMap<Preset>(
            map => {
                map.MapCreator(x => new(x.Id, x.UserId, x.Character));
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.Character);
                map.MapMember(x => x.UserId);
                map.MapMember(x => x.Position);
            }
        );
    }
}
