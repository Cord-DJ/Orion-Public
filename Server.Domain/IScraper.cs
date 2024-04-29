namespace Cord.Server.Domain;

public interface IScraper {
    IAsyncEnumerable<ISong> GetSongsFromPlaylist(string id);
    Task<ISong> GetSong(string id);
    Task<ISong> FindSongByName(string name);
    Task<string> GetPlaylistName(string idOrLink);
    IAsyncEnumerable<(string Id, string Name)> GetUserPlaylists(string idOrLink);
}
