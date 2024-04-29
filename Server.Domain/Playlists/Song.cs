namespace Cord.Server.Domain.Playlists;

public sealed record Song(
    ID Id,
    string YoutubeId,
    string Author,
    string Name,
    TimeSpan Duration
) : ISong, ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
