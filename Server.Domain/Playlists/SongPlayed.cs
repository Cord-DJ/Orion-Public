namespace Cord.Server.Domain.Playlists;

public sealed record SongPlayed(ID Id, ISong Song, IUser User, int Upvotes, int Steals, int Downvotes) : ISongPlayed { }
