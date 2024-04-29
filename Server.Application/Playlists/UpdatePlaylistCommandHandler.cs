namespace Cord.Server.Application.Playlists;

public class UpdatePlaylistCommandHandler : ICommandHandler<UpdatePlaylistCommand, IPlaylist> {
    public async Task<IPlaylist> Handle(UpdatePlaylistCommand request, CancellationToken cancellationToken) {
        var playlist = await request.User.GetPlaylist(request.Id);
        await playlist.SetName(request.Name);

        return playlist;
    }
}
