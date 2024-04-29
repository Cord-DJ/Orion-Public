namespace Cord;

public interface IPlaylist {
    ID Id { get; }
    string Name { get; }
    bool IsProcessing { get; }
    ID? NextSongId { get; }
    IReadOnlyCollection<ISong> Songs { get; }

    Task ReorderSongs(ISong song, int position);
    Task SetActive();

    Task<ISong> AddSong(ISong song);
    Task DeleteSong(ISong song);

    Task SetName(string name);
    Task SetNextSong(ISong song);
    Task Delete();
}
