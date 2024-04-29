namespace Cord.Server.Domain.Playlists;

public interface ISongRepository : IRepository<Song> {
    Task<Song?> GetByYoutubeId(string youtubeId);
    Task<long> TotalSongs();
}
