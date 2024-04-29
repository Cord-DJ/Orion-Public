using Cord.Equipment;
using Cord.Server.Domain.Equipment;

namespace Cord.Server.Repository.Equipment;

public sealed class ItemRepository : Repository<Item>, IItemRepository {
    public ItemRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<Item>.IndexKeys.Combine(Builders<Item>.IndexKeys.Descending(x => x.AssetName));

        Collection.Indexes.CreateOne(new CreateIndexModel<Item>(indexDefinition, new() { Unique = true }));
    }

    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(typeof(ItemModification), new ItemModificationSerializer());

        BsonClassMap.RegisterClassMap<Item>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.Type);
                map.MapMember(x => x.Quality);
                map.MapMember(x => x.Name);
                map.MapMember(x => x.AssetName);
                map.MapMember(x => x.Races);
                map.MapMember(x => x.MinimumLevel);
                map.MapMember(x => x.priceCP);
                map.MapMember(x => x.PriceLP);
                map.MapMember(x => x.Modifications);
            }
        );
    }
}
