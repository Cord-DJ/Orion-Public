using MediatR;

namespace Cord.Server.Application.Playlists;

public class DeletePlaylistCommandHandler : ICommandHandler<DeletePlaylistCommand> {
    public async Task<Unit> Handle(DeletePlaylistCommand request, CancellationToken cancellationToken) {
        await request.User.DeletePlaylist(request.Id);
        return Unit.Value;
    }
}
