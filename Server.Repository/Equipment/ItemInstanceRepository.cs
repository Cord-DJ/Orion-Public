using Cord.Server.Domain.Equipment;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Equipment;

public sealed class ItemInstanceRepository : Repository<ItemInstance>, IItemInstanceRepository {
    public ItemInstanceRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<ItemInstance>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<ItemInstance>(indexDefinition));
    }

    public async IAsyncEnumerable<ItemInstance> GetForUser(ID userId) {
        var query = await Collection.FindAsync(x => x.UserId == userId);

        await foreach (var x in query.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<ItemInstance>(
            map => {
                map.MapCreator(x => new(x.Id, x.UserId, x.ItemId, x.AvailableModifications));
                map.MapIdProperty(x => x.Id);
                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.ItemId).SetIsRequired(true);
                map.MapMember(x => x.Modification);

                map.MapMember(x => x.AvailableModifications);
            }
        );
    }
}
