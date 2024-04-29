namespace Cord;

sealed class SongPlayedImpl : ISongPlayed, ISnowflakeEntity {
    public ID Id { get; init; }
    public ISong Song { get; }
    public IUser User { get; }

    public int Upvotes { get; init; } = default;
    public int Steals { get; init; } = default;
    public int Downvotes { get; init; } = default;

    public SongPlayedImpl(SongImpl song, UserImpl user) {
        Song = song;
        User = user;
    }

    public override bool Equals(object? obj) => obj is ISongPlayed entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
