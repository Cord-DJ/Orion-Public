namespace Cord.Server.Domain.Playlists;

public interface IPlaylistRepository : IRepository<PlaylistModel> {
    Task<PlaylistModel?> GetUserPlaylist(ID userId, ID id);
    IAsyncEnumerable<PlaylistModel> GetUserPlaylists(ID userId);
    Task<long> TotalPlaylists();
}
