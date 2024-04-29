namespace Cord.Server.Domain.Playlists;

public sealed record PlaylistModel(
    ID Id,
    ID UserId,
    string Name,
    bool IsProcessing,
    ID? NextSongId,
    List<ID> SongIds
) : ISnowflakeEntity {
    public object[] PrimaryKeys => new object[] { Id };
}
