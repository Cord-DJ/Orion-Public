namespace Cord.Server.Application.Playlists;

public class CreatePlaylistCommandHandler : ICommandHandler<CreatePlaylistCommand, IPlaylist> {
    public Task<IPlaylist> Handle(CreatePlaylistCommand request, CancellationToken cancellationToken) =>
        request.User.CreatePlaylist(request.Name);
}
