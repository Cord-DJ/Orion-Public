using Cord.Server.Domain.Users;
using MongoAsyncEnumerableAdapter;
using MongoDB.Bson.Serialization.Serializers;
using DateTimeOffsetSerializer = Rikarin.Repository.DateTimeOffsetSerializer;

namespace Cord.Server.Repository.Users;

public sealed class UserRepository : Repository<UserModel>, IUserRepository {
    public UserRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<UserModel>.IndexKeys.Combine(
            Builders<UserModel>.IndexKeys.Descending(x => x.Name),
            Builders<UserModel>.IndexKeys.Descending(x => x.Discriminator)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<UserModel>(indexDefinition, new() { Unique = true }));

        indexDefinition = Builders<UserModel>.IndexKeys.Combine(Builders<UserModel>.IndexKeys.Descending(x => x.Email));
        Collection.Indexes.CreateOne(new CreateIndexModel<UserModel>(indexDefinition, new() { Unique = true }));
    }

    public async IAsyncEnumerable<UserModel> GetEveryone() {
        var rooms = await Collection.FindAsync(x => true);
        await foreach (var x in rooms.ToAsyncEnumerable()) {
            yield return x;
        }
    }

    public async Task<UserModel?> GetByEmail(string email) {
        return await Collection.Find(x => x.Email == email).SingleOrDefaultAsync();
    }

    public async Task<UserModel?> GetByPassword(string password) {
        return await Collection.Find(x => x.Password == password).SingleOrDefaultAsync();
    }

    public async Task<bool> Exists(string email) {
        return await Collection.Find(x => x.Email == email.ToLower().Trim()).SingleOrDefaultAsync() != null;
    }

    public Task<long> GetActiveUsersCount(TimeSpan duration) {
        return Collection.Find(
                x => x.Properties != UserProperties.Guest
                    && x.LastLoggedIn > DateTimeOffset.UtcNow.Subtract(duration)
            )
            .CountDocumentsAsync();
    }

    public Task<long> TotalUsers() {
        return Collection.CountDocumentsAsync(x => x.Properties != UserProperties.Guest);
    }

    public Task<long> TotalVerifiedUsers() {
        return Collection.CountDocumentsAsync(x => x.Verified == Verified.Email);
    }


    public static void CreateMapping() {
        BsonSerializer.RegisterSerializer(new IdSerializer());
        BsonSerializer.RegisterSerializer(new NullableIdSerializer());
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IBoost, Boost>());
        BsonSerializer.RegisterSerializer(new ImpliedImplementationInterfaceSerializer<IUserStats, UserStats>());

        BsonClassMap.RegisterClassMap<UserModel>(
            map => {
                map.MapIdProperty(x => x.Id);
                map.SetIgnoreExtraElements(true); // Due to disconnect timer and avatarInfo

                map.MapMember(x => x.Email).SetIsRequired(true);
                map.MapMember(x => x.Password).SetIsRequired(true);
                map.MapMember(x => x.Name).SetIsRequired(true);
                map.MapMember(x => x.Discriminator);
                map.MapMember(x => x.Avatar);
                map.MapMember(x => x.Banner);

                map.MapMember(x => x.Properties);

                map.MapMember(x => x.ActivePlaylistId);
                map.MapMember(x => x.Preset);

                map.MapMember(x => x.Exp).SetIsRequired(true);
                map.MapMember(x => x.Level).SetIsRequired(true);
                map.MapMember(x => x.LevelPoints).SetIsRequired(true);
                map.MapMember(x => x.CordPoints).SetIsRequired(true);
                map.MapMember(x => x.Boost).SetIsRequired(true);
                map.MapMember(x => x.Stats);

                map.MapMember(x => x.Verified);
                map.MapMember(x => x.LastLoggedIn).SetSerializer(new DateTimeOffsetSerializer());
                // map.MapMember(x => x.DisconnectTimer).SetSerializer(new DateTimeOffsetSerializer(BsonType.String));

                // These was used to remap existing fields from int to string
                // map.MapMember(x => x.Avatar).SetSerializer(new IntToStringSerializer());
                // map.MapMember(x => x.Banner).SetSerializer(new IntToStringSerializer());
            }
        );
    }
}
