using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Playlists;

namespace Cord.Server.Application.BufferHandlers;

public class PlaylistCreatedHandler : IHandler<PlaylistCreated> {
    readonly IGatewaySender gateway;

    public PlaylistCreatedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(PlaylistCreated @event, CancellationToken token = default) {
        if (@event.Playlist is Playlist pl) {
            await gateway.Send(pl.Owner, x => x.CreatePlaylist(pl));
        }
    }
}
