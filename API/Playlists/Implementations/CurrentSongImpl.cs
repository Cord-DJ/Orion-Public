namespace Cord;

sealed class CurrentSongImpl : ICurrentSong {
    internal readonly List<ID> upvotes = new();
    internal readonly List<ID> steals = new();
    internal readonly List<ID> downvotes = new();

    public ISong Song { get; }
    public ID UserId { get; init; }
    public DateTimeOffset EndTime { get; init; }

    public IReadOnlyCollection<ID> Upvotes => upvotes.AsReadOnly();
    public IReadOnlyCollection<ID> Steals => steals.AsReadOnly();
    public IReadOnlyCollection<ID> Downvotes => downvotes.AsReadOnly();

    public CurrentSongImpl(SongImpl song) {
        Song = song;
    }
}
