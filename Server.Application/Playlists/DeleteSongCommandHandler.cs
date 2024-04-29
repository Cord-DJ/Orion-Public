using MediatR;

namespace Cord.Server.Application.Playlists;

public class DeleteSongCommandHandler : ICommandHandler<DeleteSongCommand> {
    public async Task<Unit> Handle(DeleteSongCommand request, CancellationToken cancellationToken) {
        var playlist = await request.User.GetPlaylist(request.PlaylistId);

        await playlist.EnsureSongsLoaded();
        var song = playlist.Songs.FirstOrDefault(x => x.Id == request.SongId);
        if (song == null) {
            throw new NotFoundException(nameof(ISong), request.SongId);
        }

        await playlist.DeleteSong(song);
        return Unit.Value;
    }
}
