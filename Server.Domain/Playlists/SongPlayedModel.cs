namespace Cord.Server.Domain.Playlists;

public sealed record SongPlayedModel(
    ID Id,
    ID RoomId,
    ID SongId,
    ID UserId,
    int Upvotes,
    int Steals,
    int Downvotes
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
