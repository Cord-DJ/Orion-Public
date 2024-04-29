namespace Cord.Server.Domain.Relationships;

public interface IRelationship : ISnowflakeEntity {
    ID OwnerId { get; }
    ID UserId { get; }
    RelationshipType Type { get; }
}
