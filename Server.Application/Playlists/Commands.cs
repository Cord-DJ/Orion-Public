using Cord.Server.Domain.Users;

namespace Cord.Server.Application.Playlists;

public record CreatePlaylistCommand(User User, string Name) : ICommand<IPlaylist>;

public record UpdatePlaylistCommand(User User, ID Id, string Name) : ICommand<IPlaylist>;

public record DeletePlaylistCommand(User User, ID Id) : ICommand;

public record SetActivePlaylistCommand(User User, ID Id) : ICommand;

public record ImportPlaylistCommand(User User, string PlaylistId, ImportType Type) : ICommand<IPlaylist>;

public record AddSongCommand
    (User User, ID PlaylistId, string SongId, ImportType Type) : ICommand<ISong>; // youtube only supported

public record ReorderSongsCommand(User User, ID PlaylistId, ID SongId, int Position) : ICommand;

public record SetNextSongCommand(User User, ID PlaylistId, ID SongId) : ICommand;

public record DeleteSongCommand(User User, ID PlaylistId, ID SongId) : ICommand;
