using MediatR;

namespace Cord.Server.Application.Playlists;

public class ReorderSongsCommandHandler : ICommandHandler<ReorderSongsCommand> {
    public async Task<Unit> Handle(ReorderSongsCommand request, CancellationToken cancellationToken) {
        var playlist = await request.User.GetPlaylist(request.PlaylistId);

        await playlist.EnsureSongsLoaded();
        await playlist.ReorderSongs(playlist.Songs.Single(x => x.Id == request.SongId), request.Position);

        return Unit.Value;
    }
}
