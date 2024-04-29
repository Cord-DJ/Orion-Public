namespace Cord;

public interface ICurrentSong {
    ISong Song { get; }
    ID UserId { get; }
    DateTimeOffset EndTime { get; }

    IReadOnlyCollection<ID> Upvotes { get; }
    IReadOnlyCollection<ID> Steals { get; }
    IReadOnlyCollection<ID> Downvotes { get; }
}
