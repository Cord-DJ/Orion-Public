using Cord.Server.Domain.Rooms;
using MongoAsyncEnumerableAdapter;

namespace Cord.Server.Repository.Rooms;

public sealed class MemberRepository : Repository<MemberModel>, IMemberRepository {
    public MemberRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<MemberModel>.IndexKeys.Combine(
            Builders<MemberModel>.IndexKeys.Descending(x => x.RoomId),
            Builders<MemberModel>.IndexKeys.Descending(x => x.UserId)
        );
        Collection.Indexes.CreateOne(new CreateIndexModel<MemberModel>(indexDefinition, new() { Unique = true }));

        indexDefinition = Builders<MemberModel>.IndexKeys.Ascending(x => x.RoomId);
        Collection.Indexes.CreateOne(new CreateIndexModel<MemberModel>(indexDefinition));
        
        indexDefinition = Builders<MemberModel>.IndexKeys.Ascending(x => x.UserId);
        Collection.Indexes.CreateOne(new CreateIndexModel<MemberModel>(indexDefinition));
    }

    public async IAsyncEnumerable<MemberModel> GetAllMembers(ID roomId) {
        var cursor = await Collection.FindAsync(x => x.RoomId == roomId);

        await foreach (var x in cursor.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public async IAsyncEnumerable<ID> GetJoinedRoomIds(ID userId) {
        var rooms = await Collection.FindAsync(x => x.UserId == userId);
        await foreach (var x in rooms.ToAsyncEnumerable()) {
            yield return x.RoomId;
        }
    }

    public Task<long> GetMembersCount(ID roomId) {
        return Collection.Find(x => x.RoomId == roomId).CountDocumentsAsync();
    }

    public Task RemoveByRoom(ID roomId) {
        return Collection.DeleteManyAsync(x => x.RoomId == roomId);
    }


    public static void CreateMapping() {
        // BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IRoleSettings, RoleSettings>());
        // BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<ICurrentSong, CurrentSong>());
        // BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IRole, Role>());

        BsonClassMap.RegisterClassMap<MemberModel>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.RoomId).SetIsRequired(true);
                map.MapMember(x => x.UserId).SetIsRequired(true);

                map.MapMember(x => x.Nick);
                map.MapMember(x => x.Avatar);
                map.MapMember(x => x.Roles);
                map.MapMember(x => x.JoinedAt).SetIsRequired(true);
                map.MapMember(x => x.BoostingSince);
            }
        );
    }
}
