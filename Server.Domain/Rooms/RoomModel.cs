using Cord.Server.Domain.Playlists;

namespace Cord.Server.Domain.Rooms;

public sealed record RoomModel(
    ID Id,
    ID OwnerId,
    string Name,
    string Link,
    string? Description,
    List<Category> Categories,
    RoomFeature Features,
    string? Icon,
    string? Banner,
    string? Wallpaper,
    CurrentSong? CurrentSong,
    List<IRole> Roles,
    List<UserDebt> Banned,
    List<UserDebt> Muted
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}

public record OnlineModel(ID Id, ID UserId, ID RoomId, Position Position, DateTimeOffset LastPing, bool IsBot) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}

public record InQueueModel(ID Id, ID UserId, ID RoomId) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}

public record GuestModel(string Id, ID UserId) : IEntity<string> {
    public object[] PrimaryKeys => new object[] { Id };
}

public record StealModel(ID RoomId, ID UserId);

public record VoteModel(ID RoomId, ID UserId, Vote Vote);
