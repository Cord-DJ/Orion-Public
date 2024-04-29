using Cord.Server.Domain.Verification;

namespace Cord.Server.Repository.Verification;

public sealed class VerificationRepository : Repository<Domain.Verification.Verification>, IVerificationRepository {
    public VerificationRepository(MongoContext context) : base(null, context) {
        var indexDefinition = Builders<Domain.Verification.Verification>.IndexKeys.Combine(
            Builders<Domain.Verification.Verification>.IndexKeys.Descending(x => x.UserId)
        );

        Collection.Indexes.CreateOne(new CreateIndexModel<Domain.Verification.Verification>(indexDefinition));

        indexDefinition = Builders<Domain.Verification.Verification>.IndexKeys.Combine(
            Builders<Domain.Verification.Verification>.IndexKeys.Descending(x => x.Code)
        );

        Collection.Indexes.CreateOne(
            new CreateIndexModel<Domain.Verification.Verification>(indexDefinition, new() { Unique = true })
        );
    }

    public async Task<Domain.Verification.Verification?> GetByCode(string code) {
        return await Collection.Find(x => x.Code == code).SingleOrDefaultAsync();
    }

    public Task RemoveBy(VerificationType type, ID userId) {
        return Collection.DeleteManyAsync(x => x.VerificationType == type && x.UserId == userId);
    }

    public IAsyncEnumerable<Domain.Verification.Verification> GetOutdated(TimeSpan timeSpan) =>
        throw new NotImplementedException();

    // var id = ID.Parse(Convert.ToString((DateTimeOffset.UtcNow - timeSpan).ToUnixTimeMilliseconds() << 22));
    //
    // var entries = await Collection.FindAsync(x => x.Id < id);
    // await foreach (var x in entries.ToAsyncEnumerable()) {
    //     yield return x;
    // }
    public static void CreateMapping() {
        BsonClassMap.RegisterClassMap<Domain.Verification.Verification>(
            map => {
                map.MapIdProperty(x => x.Id);

                map.MapMember(x => x.Code).SetIsRequired(true);
                map.MapMember(x => x.UserId).SetIsRequired(true);
                map.MapMember(x => x.VerificationType).SetIsRequired(true);
            }
        );
    }
}
