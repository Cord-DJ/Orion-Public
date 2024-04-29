using MediatR;

namespace Cord.Server.Application.Playlists;

public class SetActivePlaylistCommandHandler : ICommandHandler<SetActivePlaylistCommand> {
    public async Task<Unit> Handle(SetActivePlaylistCommand request, CancellationToken cancellationToken) {
        await request.User.SetActivePlaylist(request.Id);
        return Unit.Value;
    }
}
