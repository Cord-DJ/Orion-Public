namespace Cord;

sealed class SongImpl : ISong, ISnowflakeEntity {
    public ID Id { get; init; }
    public string YoutubeId { get; init; } = default!;
    public string Author { get; init; } = default!;
    public string Name { get; init; } = default!;
    public TimeSpan Duration { get; init; }

    public override bool Equals(object? obj) => obj is ISong entity && Id == entity.Id;
    public override int GetHashCode() => Id.GetHashCode();
}
