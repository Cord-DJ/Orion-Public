namespace Cord.Server.Domain.Playlists;

public sealed record CurrentSong : ICurrentSong {
    readonly List<ID> upvotes;
    readonly List<ID> steals;
    readonly List<ID> downvotes;

    public ISong Song { get; }
    public ID UserId { get; }
    public DateTimeOffset EndTime { get; }

    public IReadOnlyCollection<ID> Upvotes => upvotes.AsReadOnly();
    public IReadOnlyCollection<ID> Steals => steals.AsReadOnly();
    public IReadOnlyCollection<ID> Downvotes => downvotes.AsReadOnly();

    public CurrentSong(
        Song song,
        ID userId,
        DateTimeOffset endTime,
        List<ID> upvotes,
        List<ID> steals,
        List<ID> downvotes
    ) {
        Song = song;
        UserId = userId;
        EndTime = endTime;

        this.upvotes = upvotes;
        this.steals = steals;
        this.downvotes = downvotes;
    }
}
