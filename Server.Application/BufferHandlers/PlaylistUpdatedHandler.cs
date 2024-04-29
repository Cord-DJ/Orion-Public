using Cord.Server.Domain;
using Cord.Server.Domain.Hub;
using Cord.Server.Domain.Playlists;

namespace Cord.Server.Application.BufferHandlers;

public class PlaylistUpdatedHandler : IHandler<PlaylistUpdated> {
    readonly IGatewaySender gateway;

    public PlaylistUpdatedHandler(IGatewaySender gateway) {
        this.gateway = gateway;
    }

    public async Task Handle(PlaylistUpdated @event, CancellationToken token = default) {
        if (@event.Playlist is Playlist pl) {
            await gateway.Send(pl.Owner, x => x.UpdatePlaylist(pl));
        }
    }
}
