using Cord.Server.Domain.Users;

namespace Cord.Server.Domain.Playlists;

public sealed class Playlist : IPlaylist {
    readonly IPlaylistRepository playlistRepository;
    readonly SongProvider songProvider;

    PlaylistModel model = default!;

    readonly List<ISong> songs = new();

    // Interface
    public ID Id => model.Id;
    public string Name => model.Name;
    public bool IsProcessing => model.IsProcessing;
    public ID? NextSongId => model.NextSongId;

    public IReadOnlyList<ID> SongIds => model.SongIds.AsReadOnly();
    public IReadOnlyCollection<ISong> Songs => songs.AsReadOnly();
    public bool IsEmpty => model.SongIds.Count == 0;


    public IUser Owner { get; private set; } = default!;

    public Playlist(
        IPlaylistRepository playlistRepository,
        SongProvider songProvider
    ) {
        this.playlistRepository = playlistRepository;
        this.songProvider = songProvider;
    }

    public bool HasSong(ID id) {
        return SongIds.Any(x => x == id);
    }

    public async Task<ISong> AddSong(ISong song) {
        Internal_AddSong(song);
        await Flush();

        return song;
    }

    public void Internal_AddSong(ISong song) {
        if (HasSong(song.Id)) {
            throw new ConflictException(song.Id.ToString());
        }

        model.SongIds.Add(song.Id);
        if (model.NextSongId == null) {
            model = model with { NextSongId = song.Id };
        }
    }

    public async Task DeleteSong(ISong song) {
        if (!HasSong(song.Id)) {
            throw new NotFoundException(nameof(Song), song.Id);
        }

        model.SongIds.Remove(song.Id);
        songs.RemoveAll(x => x.Id == song.Id);

        if (model.NextSongId == song.Id) {
            model = model with { NextSongId = null };
        }

        await Flush();
    }

    public async Task AddSongs(IEnumerable<ISong> songs) {
        foreach (var song in songs) {
            Internal_AddSong(song);
        }

        model = model with { IsProcessing = false };
        await Flush();
    }

    public async Task ReorderSongs(ISong song, int position) {
        if (position < 0 || position > model.SongIds.Count) {
            throw new BadRequestException("position must be greater of equal 0");
        }

        var idx = model.SongIds.IndexOf(song.Id);
        if (idx == -1) {
            throw new NotFoundException(nameof(Song), song.Id);
        }

        model.SongIds.Remove(song.Id);
        model.SongIds.Insert(position, song.Id);

        // Also reorder the preloaded song to avoid another DB query
        var tmp = songs[idx];
        songs.RemoveAt(idx);
        songs.Insert(position, tmp);

        await Flush();
    }

    public async Task<ISong?> GetNextSong() {
        if (IsEmpty) {
            return null;
        }

        var idx = SongIds.Select((v, i) => new { v, i })
            .Where(x => x.v == model.NextSongId)
            .Select(x => x.i)
            .Take(1)
            .FirstOrDefault(-1);

        if (idx == -1) {
            model = model with { NextSongId = SongIds[1 % SongIds.Count] };
            try {
                return await songProvider.GetSong(SongIds[0]); // resolve song
            } catch (NotFoundException) {
                return null;
            }
        }

        var song = SongIds[idx];
        model = model with { NextSongId = SongIds[++idx % SongIds.Count] };
        return await songProvider.GetSong(song);
    }

    public async Task<ISong?> PopNextSong() {
        var song = await GetNextSong();
        await Flush();

        return song;
    }

    public async Task SetNextSong(ISong song) {
        if (!HasSong(song.Id)) {
            throw new NotFoundException(nameof(Song), song.Id);
        }

        model = model with { NextSongId = song.Id };
        await Flush();
    }

    public async Task SetName(string name) {
        model = model with { Name = name };
        await Flush();
    }

    public async Task Flush(bool force = false) {
        await playlistRepository.Update(model);

        await EnsureSongsLoaded(force);
        await DomainEvents.Raise(new PlaylistUpdated(this));
    }

    public async Task EnsureSongsLoaded(bool force = false) {
        if (Songs.Count == 0 || force) {
            var songs = await songProvider.GetSongs(model.SongIds).ToListAsync();

            var newSongs = model.SongIds
                .Select(id => songs.Find(y => y.Id == id))
                .Where(x => x != null)
                .Select(x => x!);

            this.songs.Clear();
            this.songs.AddRange(newSongs);
        }
    }

    public async Task Delete() {
        // TODO: dispose this class?
        await playlistRepository.Remove(Id);
        await DomainEvents.Raise(new PlaylistDeleted(Owner, Id));
    }

    public Task SetActive() => throw new NotImplementedException();

    internal void Load(User owner, PlaylistModel model) {
        Owner = owner;
        this.model = model;
    }
}
