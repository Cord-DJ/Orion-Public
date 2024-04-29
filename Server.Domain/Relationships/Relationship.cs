namespace Cord.Server.Domain.Relationships;

public record Relationship(ID Id, ID OwnerId, ID UserId, RelationshipType Type) : IRelationship {
    public object[] PrimaryKeys => new object[] { Id };
}
