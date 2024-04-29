namespace Cord.Server.Domain.Playlists;

public sealed class SongProvider {
    readonly ISongRepository songRepository;

    public SongProvider(ISongRepository songRepository) {
        this.songRepository = songRepository;
    }

    public async Task<Song> GetSong(ID id) {
        var song = await songRepository.Get(id);
        if (song == null) {
            throw new NotFoundException(nameof(Song), id);
        }

        return song;
    }

    public async IAsyncEnumerable<Song> GetSongs(IEnumerable<ID> ids) {
        await foreach (var model in songRepository.GetMultiple(ids)) {
            yield return model;
        }
    }
}
