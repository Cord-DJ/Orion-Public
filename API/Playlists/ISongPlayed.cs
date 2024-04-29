namespace Cord;

public interface ISongPlayed {
    ID Id { get; }
    ISong Song { get; }
    IUser User { get; }

    int Upvotes { get; }
    int Steals { get; }
    int Downvotes { get; }
}
