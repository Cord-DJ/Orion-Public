using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Playlists;

public sealed class PlaylistProvider {
    readonly IServiceProvider serviceProvider;
    readonly ISongRepository songRepository;
    readonly IPlaylistRepository playlistRepository;

    public PlaylistProvider(
        IServiceProvider serviceProvider,
        IPlaylistRepository playlistRepository,
        ISongRepository songRepository
    ) {
        this.serviceProvider = serviceProvider;
        this.playlistRepository = playlistRepository;
        this.songRepository = songRepository;
    }

    public async Task<Playlist> GetUserPlaylist(User user, ID id) {
        var model = await playlistRepository.GetUserPlaylist(user.Id, id);
        if (model == null) {
            throw new NotFoundException(nameof(Playlist), id);
        }

        var pl = serviceProvider.GetRequiredService<Playlist>();
        pl.Load(user, model);

        return pl;
    }

    public async IAsyncEnumerable<Playlist> GetUserPlaylists(User user) {
        await foreach (var model in playlistRepository.GetUserPlaylists(user.Id)) {
            var pl = serviceProvider.GetRequiredService<Playlist>();
            pl.Load(user, model);
            await pl.EnsureSongsLoaded();

            yield return pl;
        }
    }

    public async Task<ISong> GetSong(ID id) {
        var song = await songRepository.Get(id);
        if (song == null) {
            throw new NotFoundException(nameof(Song), id);
        }

        return song;
    }

    public async Task<Playlist> CreatePlaylist(User user, string name, bool isProcessing = false) {
        var model = new PlaylistModel(ID.NewId(), user.Id, name, isProcessing, null, new());
        var pl = serviceProvider.GetRequiredService<Playlist>();

        pl.Load(user, model);
        await playlistRepository.Add(model);

        return pl;
    }
}
