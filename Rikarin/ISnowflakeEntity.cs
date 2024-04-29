namespace Rikarin;

public interface ISnowflakeEntity {
    ID Id { get; }
    DateTimeOffset CreatedAt => Id.CreatedAt;
}
