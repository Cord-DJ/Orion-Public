using Cord.Server.Domain.Playlists;

namespace Cord.Server.Domain.Rooms;

public interface ISongsPlayedRepository : IRepository<SongPlayedModel> {
    Task<IReadOnlyCollection<SongPlayedModel>> GetForRoom(ID roomId, int limit = 50);
    Task<long> TotalSongsPlayed();
}
