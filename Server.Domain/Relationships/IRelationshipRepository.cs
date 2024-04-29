namespace Cord.Server.Domain.Relationships;

public interface IRelationshipRepository : IRepository<Relationship> {
    IAsyncEnumerable<Relationship> GetUserRelationships(ID userId);
}
