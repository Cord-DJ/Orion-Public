namespace Cord;

public interface ISong {
    ID Id { get; }
    string YoutubeId { get; }
    string Author { get; }
    string Name { get; }
    TimeSpan Duration { get; }
}
