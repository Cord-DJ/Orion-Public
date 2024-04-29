using MediatR;

namespace Cord.Server.Application.Playlists;

public class SetNextSongCommandHandler : ICommandHandler<SetNextSongCommand> {
    public async Task<Unit> Handle(SetNextSongCommand request, CancellationToken cancellationToken) {
        var playlist = await request.User.GetPlaylist(request.PlaylistId);

        await playlist.EnsureSongsLoaded();
        var song = playlist.Songs.FirstOrDefault(x => x.Id == request.SongId);
        if (song == null) {
            throw new NotFoundException(nameof(ISong), request.SongId);
        }

        await playlist.SetNextSong(song);
        return Unit.Value;
    }
}
